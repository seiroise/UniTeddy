using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniTeddy {

	/// <summary>
	/// 頂点のセット、近すぎる頂点などをまとめて扱う。
	/// </summary>
	public class VerticesSet2D {

		public List<Vertex2D> vertices { get; private set; }

		public VerticesSet2D() {
			vertices = new List<Vertex2D>();
		}

		/// <summary>
		/// 距離を確認し付近にすでに存在しない場合に追加する。
		/// </summary>
		/// <param name="p">P.</param>
		public Vertex2D CheckAndAdd(Vector2 p) {
			var nearest = FindNear(p);
			if(nearest != null) {
				return nearest;
			}
			var v = new Vertex2D(p);
			v.index = vertices.Count;
			vertices.Add(v);
			return v;
		}

		/// <summary>
		/// 入力された座標の近くに頂点が存在する場合はそれを返す。
		/// </summary>
		/// <returns>The near.</returns>
		/// <param name="p">P.</param>
		/// <param name="range">Range.</param>
		public Vertex2D FindNear(Vector2 p, float range = 0.01f) {
			if(vertices.Count <= 0) {
				return null;
			}
			float d = range;
			Vertex2D nearest = null;
			for(var i = 0; i < vertices.Count; ++i) {
				var v = vertices[i];
				var td = (v.p - p).magnitude;
				if(td < d) {
					d = td;
					nearest = v;
				}
			}
			return nearest;
		}
	}
}