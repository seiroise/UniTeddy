using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UniTriangulation2D;

namespace UniTeddy {

	/// <summary>
	/// 五十嵐先生のteddy
	/// </summary>
	public class Teddy {

		public List<Face2D> faces { get; private set; }
		public Chord2D originChord { get; private set; }

		public Teddy(List<Vector2> contour) {
			if(contour.Count < 4) {
				throw new System.ArgumentException("輪郭の頂点数は4以上必要です。");
			}

			// faceの作成
			var delaunay = new Delaunay2D(contour);
			faces = CategorizeFaces(contour, delaunay);

			// chordの作成
			var connection = MakeFaceConnection(faces);
			var terminal = faces.Find(t => t.category == Face2D.Category.Terminal);
			originChord = MakeChordAxis(terminal, connection);
		}

		/*
		 * Methods
		 */

		/// <summary>
		/// 面にカテゴライズを行い、それのリストを返す。
		/// </summary>
		/// <returns>The faces.</returns>
		/// <param name="contour">Contour.</param>
		/// <param name="delaunay">Delaunay.</param>
		List<Face2D> CategorizeFaces(List<Vector2> contour, Delaunay2D delaunay) {
			List<Face2D> faces = new List<Face2D>();
			delaunay.ForeachTriangles(t => {
				faces.Add(MakeFace(t, contour));
			});
			return faces;
		}

		/// <summary>
		/// 面同士の接続情報を作成する
		/// </summary>
		/// <returns>The face connection.</returns>
		/// <param name="faces">Faces.</param>
		Connection2D MakeFaceConnection(List<Face2D> faces) {
			var connection = new Connection2D();
			for(var i = 0; i < faces.Count; ++i) {
				connection.Add(faces[i]);
			}
			return connection;
		}

		/*
		 * Making chord axis methods
		 */

		/// <summary>
		/// Terminalな面と接続情報から軸を作成する
		/// </summary>
		/// <returns>The chord graph.</returns>
		/// <param name="terminal">Terminal.</param>
		/// <param name="connection">Connection.</param>
		Chord2D MakeChordAxis(Face2D terminal, Connection2D connection) {
			if(terminal.category != Face2D.Category.Terminal) {
				throw new System.ArgumentException("最初の面はterminalでなければなりません。");
			}

			var origin = new Chord2D();

			Edge2D commonEdge = null;

			for(int i = 0, j = terminal.edges.Length - 1; i < terminal.edges.Length; j = i++) {
				if(terminal.edges[i].isInternal) {
					origin.src = terminal.triangle.points[j];
					commonEdge = terminal.edges[i];
					break;
				}
			}
			if(commonEdge == null) {
				throw new System.ArgumentException("隣接面がありません");
			}

			origin.dst = commonEdge.midpoint;
			origin.dstEdge = commonEdge;
			origin.face = terminal;

			// ここから再帰をループに展開して探索
			Face2D neighbor;
			Stack<Chord2D> exploring = new Stack<Chord2D>();
			exploring.Push(origin);

			while(exploring.Count > 0) {
				var cur = exploring.Pop();

				if(!connection.TryGetNeighbor(cur.dstEdge, cur.face, out neighbor)) {
					throw new System.ArgumentException("接続情報が構築されていません");
				}

				Chord2D next;

				switch(neighbor.category) {

				case Face2D.Category.Sleeve:
					next = new Chord2D();
					next.face = neighbor;
					next.srcEdge = cur.dstEdge;
					next.src = cur.dstEdge.midpoint;

					Face2D nextNeighbor;
					for(var i = 0; i < neighbor.edges.Length; ++i) {
						if(connection.TryGetNeighbor(neighbor.edges[i], neighbor, out nextNeighbor)) {
							if(nextNeighbor != cur.face) {
								next.dstEdge = neighbor.edges[i];
								break;
							}
						}
					}

					next.dst = next.dstEdge.midpoint;

					cur.connections.Add(next);

					exploring.Push(next);

					break;

				case Face2D.Category.Junction:

					next = new Chord2D();
					next.face = neighbor;
					next.srcEdge = cur.dstEdge;
					next.src = cur.dstEdge.midpoint;
					next.dst = neighbor.triangle.g;

					cur.connections.Add(next);

					for(var i = 0; i < neighbor.edges.Length; ++i) {
						var e = neighbor.edges[i];
						if(connection.TryGetNeighbor(e, neighbor, out nextNeighbor)) {
							if(nextNeighbor != cur.face) {
								var nextChord = new Chord2D();
								nextChord.face = neighbor;
								nextChord.src = neighbor.triangle.g;
								nextChord.dstEdge = e;
								nextChord.dst = nextChord.dstEdge.midpoint;

								next.connections.Add(nextChord);

								exploring.Push(nextChord);
							}
						}
					}

					break;

				case Face2D.Category.Terminal:

					next = new Chord2D();
					next.face = neighbor;
					next.dstEdge = cur.dstEdge;
					next.dst = cur.dstEdge.midpoint;
					next.src = neighbor.ExcludePoint(cur.dstEdge);

					cur.connections.Add(next);

					// terminalなので探索スタックには追加しない。
					break;
				}
			}

			return origin;
		}

		/*
		 * Static methods
		 */

		/// <summary>
		/// 入力した三角形の輪郭上にもつ辺の数から面情報を作成して返す。
		/// </summary>
		/// <returns>The contour on number.</returns>
		/// <param name="t">T.</param>
		/// <param name="contour">Contour.</param>
		public static Face2D MakeFace(Triangle2D t, List<Vector2> contour) {
			var face = new Face2D(t);

			bool isExternal = false;
			int externalCount = 0;
			for(int i = 0, j = t.points.Length - 1; i < t.points.Length; j = i++) {
				isExternal = IsContourEdge(contour, t.points[j], t.points[i]);
				externalCount += isExternal ? 1 : 0;
				var edge = new Edge2D(
					t.points[j],
					t.points[i],
					!isExternal
				);
				face.edges[j] = edge;
			}

			switch(externalCount) {
			case 0:
				face.category = Face2D.Category.Junction;
				break;
			case 1:
				face.category = Face2D.Category.Sleeve;
				break;
			case 2:
				face.category = Face2D.Category.Terminal;
				break;
			}

			return face;
		}

		/// <summary>
		/// 入力した三角形が入力した座標から作られる辺を持っているか
		/// </summary>
		/// <returns><c>true</c>, if edge was hased, <c>false</c> otherwise.</returns>
		/// <param name="t">T.</param>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		public static bool HasEdge(Triangle2D t, Vector2 a, Vector2 b) {
			return t.HasPoint(a) && t.HasPoint(b);
		}

		/// <summary>
		/// 入力したに2点からなる辺が輪郭であるかを判定する。
		/// </summary>
		/// <returns><c>true</c>, if contour edge was ised, <c>false</c> otherwise.</returns>
		/// <param name="contour">Contour.</param>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		public static bool IsContourEdge(List<Vector2> contour, Vector2 a, Vector2 b) {
			for(int i = 0, j = contour.Count - 1; i < contour.Count; j = i++) {
				if((contour[j] == a && contour[i] == b) || (contour[i] == a && contour[j] == b)) {
					return true;
				}
			}
			return false;
		}
	}
}