using System.Collections;
using System.Collections.Generic;
using UniTriangulation2D;
using UnityEngine;

namespace UniTeddy {

	public class Face2D {

		public enum Category {
			Terminal = 0, Sleeve = 1, Junction = 2
		}

		public Category category { get; set; }

		public Vertex2D[] vertices { get; private set; }

		public Vertex2D a { get { return vertices[0]; } }
		public Vertex2D b { get { return vertices[1]; } }
		public Vertex2D c { get { return vertices[2]; } }

		public Vertex2D g { get; private set; }

		public Edge2D[] edges { get; private set; }

		public Face2D(Vertex2D a, Vertex2D b, Vertex2D c, Vertex2D g) {
			this.vertices = new Vertex2D[3];
			this.vertices[0] = a;
			this.vertices[1] = b;
			this.vertices[2] = c;

			this.g = g;

			this.edges = new Edge2D[3];
		}

		/// <summary>
		/// 指定した内部辺以外の他の内部辺を取得する
		/// </summary>
		/// <returns><c>true</c>, if get othe interior edge was tryed, <c>false</c> otherwise.</returns>
		/// <param name="interiorEdge">Interior edge.</param>
		/// <param name="others">Others.</param>
		public bool TryGetOtheInteriorEdge(Edge2D interiorEdge, out Edge2D[] others) {
			if(category != Category.Terminal) {
				others = new Edge2D[(int)category];
				var count = 0;
				for(var i = 0; i < edges.Length; ++i) {
					if(!edges[i].isExterior && !edges[i].Equals(interiorEdge)) {
						others[count++] = edges[i];
					}
				}
				return count == others.Length;
			} else {
				others = null;
				return false;
			}
		}

		/// <summary>
		/// 入力した辺に含まれない頂点を返す
		/// </summary>
		/// <returns>The vertex.</returns>
		/// <param name="e">E.</param>
		public Vertex2D ExcludeVertex(Edge2D e) {
			if(!e.HasVertex(a)) {
				return a;
			} else if(!e.HasVertex(b)) {
				return b;
			} else {
				return c;
			}
		}

		/// <summary>
		/// デバッグ用の簡易描画
		/// </summary>
		public void DebugDraw() {
			switch(category) {
				case Category.Junction:
					DebugExtention.DrawTriangle(a.p, b.p, c.p, Color.yellow);
					break;
				case Category.Sleeve:
					DebugExtention.DrawTriangle(a.p, b.p, c.p, Color.gray);
					break;
				case Category.Terminal:
					DebugExtention.DrawTriangle(a.p, b.p, c.p, Color.red);
					break;
			}
		}
	}
}