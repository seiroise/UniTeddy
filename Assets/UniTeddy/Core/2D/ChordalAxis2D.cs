using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniTeddy {

	/// <summary>
	/// Teddyのspineを作るための軸を表す。
	/// </summary>
	public class ChordalAxis2D {

		public Chord2D origin { get; private set; }
		public List<Chord2D> sleeveChords { get; private set; }
		public List<Chord2D> junctionChords { get; private set; }
		public List<Chord2D> terminalChords { get; private set; }
		public List<Fan2D> fans { get; private set; }

		public ChordalAxis2D(Face2D terminal, Connection2D connection) {
			sleeveChords = new List<Chord2D>();
			junctionChords = new List<Chord2D>();
			terminalChords = new List<Chord2D>();

			MakeChordalAxis(terminal, connection);
		}

		/// <summary>
		/// 終端の面と接続情報から軸を作成する。
		/// </summary>
		/// <param name="terminal">Terminal.</param>
		/// <param name="connection">Connection.</param>
		void MakeChordalAxis(Face2D terminal, Connection2D connection) {
			if(terminal.category != Face2D.Category.Terminal) {
				throw new System.ArgumentException("最初の面はterminalでなければなりません。");
			}

			var origin = new Chord2D();
			origin.face = terminal;
			for(int i = 0, j = terminal.edges.Length - 1; i < terminal.edges.Length; j = i++) {
				if(terminal.edges[i].isInternal) {
					origin.src = terminal.triangle.points[j];
					origin.dstEdge = terminal.edges[i];
					break;
				}
			}

			this.terminalChords.Add(origin);

			Face2D neighbor;
			Stack<Chord2D> exploring = new Stack<Chord2D>();
			List<Chord2D> temporary = new List<Chord2D>();
			exploring.Push(origin);

			while(exploring.Count > 0) {
				var cur = exploring.Pop();
				if(cur == null) {
					continue;
				}

				if(!connection.TryGetNeighbor(cur.dstEdge, cur.face, out neighbor)) {
					throw new System.ArgumentException("接続情報が構築されていません");
				}
				temporary.Clear();
				if(neighbor.category == Face2D.Category.Sleeve) {
					MakeSleeveChord(cur, neighbor, ref temporary);
				} else if(neighbor.category == Face2D.Category.Junction) {
					MakeJunctionChord(cur, neighbor, ref temporary);
				} else {
					MakeTerminalChord(cur, neighbor, ref temporary);
				}

				for(int i = 0; i < temporary.Count; ++i) {
					exploring.Push(temporary[i]);
				}
			}

			this.origin = origin;
		}

		/// <summary>
		/// Sleeve部分のchordを作成
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="neighbor">Neighbor.</param>
		/// <param name="pushChords">Push chords.</param>
		void MakeSleeveChord(Chord2D from, Face2D neighbor, ref List<Chord2D> pushChords) {
			Edge2D[] nextDstEdges;
			if(!neighbor.TryGetOtherInternalEdge(from.dstEdge, out nextDstEdges)) {
				throw new System.Exception("辺の内外判定が不正です。");
			}

			var chord = new Chord2D();
			chord.face = neighbor;
			chord.srcEdge = from.dstEdge;
			chord.dstEdge = nextDstEdges[0];
			from.connections.Add(chord);

			var pair = new Chord2D();
			pair.face = neighbor;
			pair.srcEdge = chord.dstEdge;
			pair.dstEdge = chord.srcEdge;
			pair.connections.Add(from.pair);

			chord.pair = pair;
			pair.pair = chord;

			pushChords.Add(chord);

			sleeveChords.Add(chord);
		}

		/// <summary>
		/// Junction部分のchordを作成
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="neighbor">Neighbor.</param>
		/// <param name="pushChords">Push chords.</param>
		void MakeJunctionChord(Chord2D from, Face2D neighbor, ref List<Chord2D> pushChords) {
			var chord = new Chord2D();
			chord.face = neighbor;
			chord.srcEdge = from.dstEdge;
			chord.dst = neighbor.triangle.g;
			from.connections.Add(chord);

			var pair = new Chord2D();
			pair.face = neighbor;
			pair.src = chord.dst;
			pair.dstEdge = chord.srcEdge;
			pair.connections.Add(from.pair);

			chord.pair = pair;
			pair.pair = chord;

			junctionChords.Add(chord);

			Edge2D[] nextDstEdges;
			if(!neighbor.TryGetOtherInternalEdge(from.dstEdge, out nextDstEdges)) {
				throw new System.Exception("辺の内外判定が不正です。");
			}

			// 他の二つ
			for(int i = 0; i < nextDstEdges.Length; ++i) {
				var c = new Chord2D();
				c.face = neighbor;
				c.src = chord.dst;
				c.dstEdge = nextDstEdges[i];
				chord.connections.Add(c);

				junctionChords.Add(c);

				var cpair = new Chord2D();
				cpair.face = neighbor;
				cpair.dst = c.src;
				cpair.srcEdge = nextDstEdges[i];
				cpair.connections.Add(pair);

				c.pair = cpair;
				cpair.pair = c;

				pushChords.Add(c);
			}

			// 接続
			var cp0 = chord.connections[0].pair;
			cp0.connections.Add(chord.connections[1]);

			var cp1 = chord.connections[1].pair;
			cp1.connections.Add(chord.connections[0]);
		}

		/// <summary>
		/// Terminal部分のchordを作成
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="neighbor">Neighbor.</param>
		/// <param name="pushChords">Push chords.</param>
		void MakeTerminalChord(Chord2D from, Face2D neighbor, ref List<Chord2D> pushChords) {
			var chord = new Chord2D();
			chord.face = neighbor;
			chord.src = neighbor.ExcludePoint(from.dstEdge);
			chord.dstEdge = from.dstEdge;

			from.connections.Add(chord);
			chord.connections.Add(from.pair);

			this.terminalChords.Add(chord);
		}

		/*
		 * Related prune
		 */

		/// <summary>
		/// 余分な枝の剪定
		/// </summary>
		/// <returns>The branch.</returns>
		public void PruneBranch(List<Vector2> contour) {
			List<Vector2> checking = new List<Vector2>();
			Stack<Chord2D> exploring = new Stack<Chord2D>();

			List<Fan2D> fans = new List<Fan2D>();

			for(var i = 0; i < terminalChords.Count; ++i) {
				exploring.Push(terminalChords[i]);
			}

			while(exploring.Count > 0) {
				var c = exploring.Pop();
				if(c.face.category == Face2D.Category.Terminal) {
					checking.Clear();
				}

				c.face.isPruned = true;
				if(c.face.category == Face2D.Category.Junction) {
					checking.Add(c.srcEdge.a);
					checking.Add(c.srcEdge.b);
					var fan = new Fan2D(c.face.triangle.g, checking, contour, c.face, c);
					fans.Add(fan);
				} else {
					var cc = c.dstEdge.circumscribedCircle;
					checking.Add(c.face.ExcludePoint(c.dstEdge));

					bool unoverlaped = false;
					for(var i = 0; i < checking.Count; ++i) {
						if(!cc.Overlap(checking[i])) {
							unoverlaped = true;
							break;
						}
					}
					if(unoverlaped) {
						checking.Add(c.dstEdge.a);
						checking.Add(c.dstEdge.b);
						var fan = new Fan2D(c.dstEdge.midpoint, checking, contour, c.face, c);
						fans.Add(fan);
					} else {
						exploring.Push(c.connections[0]);
					}
				}
			}

			this.fans = fans;
		}

		/// <summary>
		/// 基点のある面が同じfanを検索しそれらを繋げる。
		/// </summary>
		public void RefineFan(List<Vector2> contour) {
			Dictionary<Face2D, List<Fan2D>> dic = new Dictionary<Face2D, List<Fan2D>>();
			for(var i = 0; i < fans.Count; ++i) {
				if(!dic.ContainsKey(fans[i].baseFace)) {
					dic.Add(fans[i].baseFace, new List<Fan2D>());
				}
				dic[fans[i].baseFace].Add(fans[i]);
			}

			// 結合
			foreach(var face in dic.Keys) {
				var f = dic[face];
				if(f.Count == 2) {
					Debug.Log("結合");
					List<Vector2> vertices = new List<Vector2>();
					for(var i = 0; i < f.Count; ++i) {
						vertices.AddRange(f[i].fanVertices);
						fans.Remove(f[i]);
					}

					var connections = f[0].baseChord.connections;
					Debug.Log("count: " + connections.Count);
					Chord2D baseChord = null;
					foreach(var c in connections) {
						if(c.pair != f[1].baseChord) {
							baseChord = c;
						}
					}

					var fan = new Fan2D(baseChord.dst, vertices, contour, face, baseChord);
					fans.Add(fan);
				}
			}
		}

		/// <summary>
		/// 確認のための簡易描画
		/// </summary>
		/// <param name="color">Color.</param>
		public void DebugDraw(Color color) {
			Stack<Chord2D> chords = new Stack<Chord2D>();
			chords.Push(origin.connections[0]);
			while(chords.Count > 0) {
				var c = chords.Pop();
				if(!c.face.isPruned) {
					c.DebugDraw(color);
				}
				if(c.face.category == Face2D.Category.Terminal) continue;
				for(int i = 0; i < c.connections.Count; ++i) {
					chords.Push(c.connections[i]);
				}
			}
			for(var i = 0; i < fans.Count; ++i) {
				fans[i].DebugDraw(Color.green);
			}
		}

		/// <summary>
		/// Teddyで膨らませる時の稜線になる点を取得する。
		/// </summary>
		/// <returns>The spine.</returns>
		public List<Vector2> GetSpine() {
			List<Vector2> spinePoints = new List<Vector2>();

			Stack<Chord2D> chords = new Stack<Chord2D>();
			chords.Push(origin.connections[0]);
			while(chords.Count > 0) {
				var c = chords.Pop();
				if(!c.face.isPruned) {
					spinePoints.Add(c.dst);
				}
				if(c.face.category == Face2D.Category.Terminal) continue;
				for(int i = 0; i < c.connections.Count; ++i) {
					chords.Push(c.connections[i]);
				}
			}

			for(var i = 0; i < fans.Count; ++i) {
				spinePoints.Add(fans[i].baseVertex);
			}

			return spinePoints;
		}

		/*
		 * Related skeleton
		 */

		/// <summary>
		/// 軸情報から立体的な骨組みを作成す、それを返す。
		/// </summary>
		/// <returns>The skeleton.</returns>
		public Skeleton BuildSkeleton() {
			var edgeDic = new Dictionary<ElevatedEdge, ElevatedEdge>();
			List<Surface> surfaces = new List<Surface>();

			// fan
			for(var i = 0; i < fans.Count; ++i) {
				var f = fans[i];
				for(var j = 0; j < f.fanVertices.Count - 1; ++j) {
					surfaces.Add(MakeSurface_Exterior(edgeDic, f.fanVertices[j], f.fanVertices[j + 1], f.baseVertex, f.baseElevation));
				}
			}

			// sleeve
			for(var i = 0; i < sleeveChords.Count; ++i) {
				var chord = sleeveChords[i];
				if(chord.face.isPruned) continue;

				// 分割してとんがってる方の頂点を仮にcとしてそれ以外をaとbとする
				Vector2 c, a, b;
				c = a = b = new Vector2(0f, 0f);
				var t = chord.face.triangle;
				for(var j = 0; j < t.points.Length; ++j) {
					var p = t.points[j];
					if(chord.dstEdge.HasPoint(p) && chord.srcEdge.HasPoint(p)) {
						c = p;
						a = t.points[(j + 1) % 3];
						b = t.points[(j + 2) % 3];
					}
				}

				// 外積判定

				float srcElevation, dstElevation;
				srcElevation = chord.srcEdge.length * 0.5f;
				dstElevation = chord.dstEdge.length * 0.5f;

				// とんがってる側(三角形側)
				surfaces.Add(MakeSurface_Interior(edgeDic, chord.src, srcElevation, chord.dst, dstElevation, c));
				// とんがってない側(四角形側) 多分向きあるよね。
				surfaces.Add(MakeSurface_Exterior(edgeDic, a, b, chord.src, srcElevation));
				surfaces.Add(MakeSurface_Interior(edgeDic, chord.src, srcElevation, chord.dst, dstElevation, chord.dstEdge.ExcludePoint(c)));
			}

			// junction
			for(var i = 0; i < junctionChords.Count; ++i) {
				var chord = junctionChords[i];

			}

			var edges = new List<ElevatedEdge>(edgeDic.Values);
			return new Skeleton(surfaces, edges);
		}

		/// <summary>
		/// 外側に向かって広がる面を追加する
		/// </summary>
		/// <param name="dic">Dic.</param>
		/// <param name="a">外側</param>
		/// <param name="b">外側</param>
		/// <param name="m">内側</param>
		/// <param name="mElevation">mの高さ</param>
		static Surface MakeSurface_Exterior(Dictionary<ElevatedEdge, ElevatedEdge> dic, Vector2 a, Vector2 b, Vector2 m, float mElevation) {
			float dir = UtilsMath.Cross(a - m, m - b);
			if(dir > 0) {
				var temp = a;
				a = b;
				b = a;
			}

			ElevatedEdge ma = new ElevatedEdge(a, m, mElevation);
			ElevatedEdge mb = new ElevatedEdge(b, m, mElevation);

			CheckAndAdd(dic, ma);
			CheckAndAdd(dic, mb);

			Surface surf = new Surface(Surface.Divergent.Exterior, dic[ma], dic[mb]);

			return surf;
		}

		/// <summary>
		/// 内側に向かって広がる面を追加する
		/// </summary>
		/// <param name="dic">Dic.</param>
		/// <param name="a">内側</param>
		/// <param name="aElevation">aの高さ</param>
		/// <param name="b">内側</param>
		/// <param name="bElevation">bの高さ</param>
		/// <param name="m">外側</param>
		static Surface MakeSurface_Interior(Dictionary<ElevatedEdge, ElevatedEdge> dic, Vector2 a, float aElevation, Vector2 b, float bElevation, Vector2 m) {
			float dir = UtilsMath.Cross(a - m, m - b);
			if(dir > 0) {
				var temp = a;
				a = b;
				b = temp;
			}

			ElevatedEdge am = new ElevatedEdge(m, a, aElevation);
			ElevatedEdge bm = new ElevatedEdge(m, b, bElevation);

			CheckAndAdd(dic, am);
			CheckAndAdd(dic, bm);

			Surface surf = new Surface(Surface.Divergent.Interior, dic[am], dic[bm]);

			return surf;
		}

		static void CheckAndAdd(Dictionary<ElevatedEdge, ElevatedEdge> dic, ElevatedEdge edge) {
			if(!dic.ContainsKey(edge)) {
				dic.Add(edge, edge);
			}
		}
	}
}