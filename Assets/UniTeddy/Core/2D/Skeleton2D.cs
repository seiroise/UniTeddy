using System.Collections;
using System.Collections.Generic;
using UniTeddy;
using UniTriangulation2D;
using UnityEngine;

namespace UniTeddy {

	/// <summary>
	/// ChordalAxisの結果を元によりTeddyを扱うのに最適化した平面分割を行ったもの
	/// </summary>
	public class Skeleton2D {

		public ChordalAxis2D axis { get; private set; }
		public List<Fan2D> fans { get; private set; }

		public Skeleton2D(ChordalAxis2D axis, List<Vector2> contour) {
			this.axis = axis;
			fans = PruneBranch(axis, contour);
			ElevateInterior();
		}

		/// <summary>
		/// 軸の接続情報から余分な軸を削除する
		/// </summary>
		/// <returns>The fans.</returns>
		List<Fan2D> PruneBranch(ChordalAxis2D axis, List<Vector2> contour) {
			Stack<Chord2D> exploring = new Stack<Chord2D>();
			List<Vertex2D> vertices = new List<Vertex2D>();
			List<Fan2D> fans = new List<Fan2D>();

			List<Chord2D> route = new List<Chord2D>();
			Chord2D start = null;

			foreach(var c in axis.terminalChords) {
				exploring.Push(c);
			}

			while(exploring.Count > 0) {
				var c = exploring.Pop();
				if(c.face.category == Face2D.Category.Terminal) {
					vertices.Clear();
					route.Clear();
					route.Add(c);
					start = c;
				}

				if(c.face.category == Face2D.Category.Junction) {
					vertices.Add(c.pair.dstEdge.a);
					vertices.Add(c.pair.dstEdge.b);
					var fan = new Fan2D(c.dst, vertices, contour, c.face);
					fans.Add(fan);

					// chordの接続を切る
					foreach(var r in route) {
						axis.RemoveChord(r);
					}

				} else {
					var cc = c.dstEdge.circumscribedCircle;
					vertices.Add(c.face.ExcludeVertex(c.dstEdge));

					bool unoverlapped = false;
					foreach(var v in vertices) {
						if(!cc.Overlap(v.p)) {
							unoverlapped = true;
							break;
						}
					}
					if(unoverlapped) {
						vertices.Add(c.dstEdge.a);
						vertices.Add(c.dstEdge.b);
						var fan = new Fan2D(c.dstEdge.mid, vertices, contour, c.face);
						fans.Add(fan);

						// chordの接続を切る
						foreach(var r in route) {
							axis.RemoveChord(r);
						}

					} else {
						route.Add(c.connections[0]);
						exploring.Push(c.connections[0]);
					}
				}

			}

			return fans;
		}

		/// <summary>
		/// 内部頂点の高さを計算する
		/// </summary>
		void ElevateInterior() {
			var connection = new VertexConnection2D();
			foreach(var c in axis.sleeveChords) {
				connection.AddSleeveChord(c);
			}
			foreach(var c in axis.junctionChords) {
				connection.AddJunctionChord(c);
			}

			foreach(var pair in connection.connection) {
				var baseV = pair.Key;
				float sum = 0f;
				int count = 0;
				foreach(var v in pair.Value) {
					if(v.isExterior) {
						sum += (v.p - baseV.p).magnitude;
						count++;
					}
				}
				baseV.elevation = sum / count;
			}
		}

		/// <summary>
		/// ボリュームのある骨格に変換する
		/// </summary>
		public SkeletalVolume ToSkeletalVolume() {
			var surfaces = new List<Surface>();
			var dic = new Dictionary<ElevatedEdge, ElevatedEdge>();

			// fan
			foreach(var f in fans) {
				for(var i = 0; i < f.fanVertices.Count - 1; ++i) {
					surfaces.Add(MakeSurface_Exterior(dic, f.fanVertices[i], f.fanVertices[i + 1], f.baseVertex));
				}
				// 反転
				var mirror = f.baseVertex.mirror;
				for(var i = 0; i < f.fanVertices.Count - 1; ++i) {
					surfaces.Add(MakeSurface_Exterior(dic, f.fanVertices[i], f.fanVertices[i + 1], mirror, true));
				}
			}

			// sleeve
			foreach(var chord in axis.sleeveChords) {
				var vertices = chord.face.vertices;
				Vertex2D c, a, b;
				c = a = b = null;
				for(var i = 0; i < vertices.Length; ++i) {
					var v = vertices[i];
					if(chord.dstEdge.HasVertex(v) && chord.srcEdge.HasVertex(v)) {
						c = v;
						a = vertices[(i + 1) % 3];
						b = vertices[(i + 2) % 3];
					}
				}

				surfaces.Add(MakeSurface_Interior(dic, chord.dst, chord.src, c));
				surfaces.Add(MakeSurface_Interior(dic, chord.dst.mirror, chord.src.mirror, c, true));

				surfaces.Add(MakeSurface_Exterior(dic, a, b, chord.src));
				surfaces.Add(MakeSurface_Exterior(dic, a, b, chord.src.mirror, true));

				var other = chord.dstEdge.GetOtherVertex(c);
				surfaces.Add(MakeSurface_Interior(dic, chord.src, chord.dst, other));
				surfaces.Add(MakeSurface_Interior(dic, chord.src.mirror, chord.dst.mirror, other, true));
			}

			// junction
			foreach(var chord in axis.junctionChords) {
				Edge2D edge;
				Vertex2D v;

				if(chord.dstEdge != null) {
					edge = chord.dstEdge;
					v = chord.src;
				} else {
					edge = chord.srcEdge;
					v = chord.dst;
				}

				surfaces.Add(MakeSurface_Interior(dic, v, edge.mid, edge.a));
				surfaces.Add(MakeSurface_Interior(dic, v.mirror, edge.mid.mirror, edge.a, true));

				surfaces.Add(MakeSurface_Interior(dic, v, edge.mid, edge.b));
				surfaces.Add(MakeSurface_Interior(dic, v.mirror, edge.mid.mirror, edge.b, true));
			}

			return new SkeletalVolume(surfaces, new List<ElevatedEdge>(dic.Keys));
		}

		/// <summary>
		/// 外側に広がる面を作成する
		/// </summary>
		/// <returns>The surface exterior.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="m">M.</param>
		Surface MakeSurface_Exterior(Dictionary<ElevatedEdge, ElevatedEdge> dic, Vertex2D a, Vertex2D b, Vertex2D m, bool isReverse = false) {
			var d = UtilsMath.Cross(a.p - m.p, m.p - b.p);
			if(!isReverse ? d >= 0 : d < 0) {
				var temp = a;
				a = b;
				b = temp;
			}

			ElevatedEdge ma = new ElevatedEdge(a, m);
			ElevatedEdge mb = new ElevatedEdge(b, m);

			CheckAndAdd(dic, ma);
			CheckAndAdd(dic, mb);

			Surface surf = new Surface(Surface.Divergent.Exterior, dic[ma], dic[mb]);
			return surf;
		}

		/// <summary>
		/// 内側に広がる面を作成する
		/// </summary>
		/// <returns>The surface interior.</returns>
		/// <param name="dic">Dic.</param>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="m">M.</param>
		Surface MakeSurface_Interior(Dictionary<ElevatedEdge, ElevatedEdge> dic, Vertex2D a, Vertex2D b, Vertex2D m, bool isReverse = false) {
			var d = UtilsMath.Cross(a.p - m.p, m.p - b.p);
			if(!isReverse ? d >= 0 : d < 0) {
				var temp = a;
				a = b;
				b = temp;
			}

			ElevatedEdge ma = new ElevatedEdge(m, a);
			ElevatedEdge mb = new ElevatedEdge(m, b);

			CheckAndAdd(dic, ma);
			CheckAndAdd(dic, mb);

			Surface surf = new Surface(Surface.Divergent.Interior, dic[ma], dic[mb]);
			return surf;
		}

		/// <summary>
		/// 辞書に含まれていない場合に追加
		/// </summary>
		/// <param name="dic">Set.</param>
		/// <param name="edge">Edge.</param>
		void CheckAndAdd(Dictionary<ElevatedEdge, ElevatedEdge> dic, ElevatedEdge edge) {
			if(!dic.ContainsKey(edge)) {
				dic.Add(edge, edge);
			}
		}

		/// <summary>
		/// デバッグ用の簡易描画
		/// </summary>
		public void DebugDraw() {
			foreach(var f in fans) {
				f.DebugDraw(Color.green);
			}
		}
	}
}