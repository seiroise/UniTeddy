using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UniTriangulation2D;

namespace UniTeddy {

	public class Face2D {

		public enum Category {
			Terminal = 0, Sleeve = 1, Junction = 2
		}

		public Category category { get; set; }
		public Triangle2D triangle { get; private set; }

		public Edge2D[] edges { get; private set; }

		public Edge2D e0 { get { return edges[0]; } }
		public Edge2D e1 { get { return edges[1]; } }
		public Edge2D e2 { get { return edges[2]; } }

		public bool isPruned { get; set; }

		public Face2D(Triangle2D t) {
			this.triangle = t;
			this.edges = new Edge2D[3];
		}

		/// <summary>
		/// 入力した辺に含まれない座標を返す。
		/// </summary>
		/// <returns>The point.</returns>
		/// <param name="e">E.</param>
		public Vector2 ExcludePoint(Edge2D e) {
			if(!e.HasPoint(triangle.p0)) {
				return triangle.p0;
			} else if(!e.HasPoint(triangle.p1)) {
				return triangle.p1;
			}
			return triangle.p2;
		}

		/// <summary>
		/// 入力した内部辺以外の内部辺を見つけ、返す。
		/// </summary>
		/// <returns>The other internal edge.</returns>
		/// <param name="internalEdge">Internal edge.</param>
		public bool TryGetOtherInternalEdge(Edge2D internalEdge, out Edge2D[] others) {
			if(category != Category.Terminal) {
				others = new Edge2D[(int)category];
				var count = 0;
				for(var i = 0; i < edges.Length; ++i) {
					if(edges[i].isInternal && edges[i] != internalEdge) {
						others[count] = edges[i];
						count++;
					}
				}
				return count == others.Length;
			}
			others = null;
			return false;
		}

		/// <summary>
		/// 面積を計算して返す。
		/// ヘロンの公式より
		/// https://ja.wikipedia.org/wiki/%E3%83%98%E3%83%AD%E3%83%B3%E3%81%AE%E5%85%AC%E5%BC%8F
		/// </summary>
		/// <returns>The size.</returns>
		public float ComputeSize() {
			var s = (e0.length + e1.length + e2.length) * 0.5f;
			return Mathf.Sqrt(s * (s - e0.length) * (s - e1.length) * (s - e2.length));
		}

		/// <summary>
		/// デバッグ用の簡易描画
		/// </summary>
		public void DebugDraw() {
			switch(category) {
			case Category.Terminal:
				triangle.DebugDraw(Color.red);
				break;
			case Category.Sleeve:
					triangle.DebugDraw(Color.gray);
				break;
			case Category.Junction:
				triangle.DebugDraw(Color.yellow);
				break;
			}
		}

		public override string ToString() {
			return string.Format("[Face2D: category={0}, triangle={1}, edges={2}, e0={3}, e1={4}, e2={5}, isPruned={6}]", category, triangle, edges, e0, e1, e2, isPruned);
		}
	}
}