using System.Collections;
using System.Collections.Generic;
using UniTriangulation2D;
using UnityEngine;

namespace UniTeddy {

	public class Fan2D {

		public Vertex2D baseVertex { get; private set; }
		public List<Vertex2D> fanVertices { get; private set; }

		public Face2D baseFace { get; set; }

		public Fan2D(Vertex2D baseVertex, List<Vertex2D> fanVertices, List<Vector2> contour, Face2D baseFace) {
			this.baseVertex = baseVertex;
			this.fanVertices = AlignFanVertices(fanVertices, contour);
			this.baseFace = baseFace;
		}

		/// <summary>
		/// 輪郭に沿うように頂点を整列させる
		/// </summary>
		/// <returns>The fan vertices.</returns>
		/// <param name="src">Source.</param>
		/// <param name="contour">Contour.</param>
		List<Vertex2D> AlignFanVertices(List<Vertex2D> src, List<Vector2> contour) {
			// 輪郭の始点と終点ををまたぐ場合は特別な措置が必要
			var dst = new List<Vertex2D>(src.Count);

			bool insert = false;
			int insertCount = 0;
			int prevAdd = 0;
			int count = contour.Count;
			for(int i = 0; i < count; ++i) {
				for(var j = 0; j < src.Count; ++j) {
					if(contour[i] == src[j].p) {
						if(i - prevAdd > 1) {
							insert = true;
						}
						if(!insert) {
							prevAdd = i;
							dst.Add(src[j]);
						} else {
							dst.Insert(insertCount++, src[j]);
						}
						break;
					}
				}
			}

			// 始点と終点が重なっているケースが存在する。
			if(dst[0] == dst[dst.Count - 1]) {
				dst.RemoveAt(dst.Count - 1);
			}

			return dst;
		}

		/// <summary>
		/// デバッグ用の簡易描画
		/// </summary>
		/// <param name="color">Color.</param>
		public void DebugDraw(Color color) {
			for(int i = 0; i < fanVertices.Count - 1; ++i) {
				DebugExtention.DrawTriangle(baseVertex.p, fanVertices[i].p, fanVertices[i + 1].p, color);
			}
		}
	}
}