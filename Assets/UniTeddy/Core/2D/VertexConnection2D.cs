using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniTeddy {

	/// <summary>
	/// 頂点による辺の接続関係
	/// </summary>
	public class VertexConnection2D {

		public Dictionary<Vertex2D, List<Vertex2D>> connection { get; private set; }

		public VertexConnection2D() {
			connection = new Dictionary<Vertex2D, List<Vertex2D>>();
		}

		/// <summary>
		/// SleeveなChordを追加する
		/// </summary>
		/// <param name="c">C.</param>
		public void AddSleeveChord(Chord2D c) {
			CheckAndAdd(c.src, c.dst);
			CheckAndAdd(c.dst, c.src);

			CheckAndAdd(c.dst, c.dstEdge.a);
			CheckAndAdd(c.dst, c.dstEdge.b);

			var exclude = c.face.ExcludeVertex(c.dstEdge);
			CheckAndAdd(c.dst, exclude);
			CheckAndAdd(c.src, exclude);

			var include = c.dstEdge.IncludeVertex(c.srcEdge);
			CheckAndAdd(c.src, include);
		}

		/// <summary>
		/// JunctionなChordを追加する
		/// </summary>
		/// <param name="c">C.</param>
		public void AddJunctionChord(Chord2D c) {
			CheckAndAdd(c.src, c.dst);
			CheckAndAdd(c.dst, c.src);

			Vertex2D a, b, src, dst;
			if(c.srcEdge != null) {
				a = c.srcEdge.a;
				b = c.srcEdge.b;
				src = c.src;
				dst = c.dst;
			} else {
				a = c.dstEdge.a;
				b = c.dstEdge.b;
				// 反転
				src = c.dst;
				dst = c.src;
			}

			CheckAndAdd(src, a);
			CheckAndAdd(src, b);
			CheckAndAdd(dst, a);
			CheckAndAdd(dst, b);
		}

		/// <summary>
		/// 重複がないかを確認して追加する
		/// </summary>
		/// <param name="v">V.</param>
		/// <param name="e">E.</param>
		void CheckAndAdd(Vertex2D v, Vertex2D e) {
			if(!connection.ContainsKey(v)) {
				connection.Add(v, new List<Vertex2D> { e });
			} else {
				if(!connection[v].Contains(e)) {
					connection[v].Add(e);
				}
			}
		}
	}
}