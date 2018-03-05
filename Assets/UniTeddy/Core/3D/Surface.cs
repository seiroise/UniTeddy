using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniTeddy {

	/// <summary>
	/// 骨格上の立体的な面
	/// </summary>
	public class Surface {

		public enum Divergent {
			Exterior, Interior
		}

		public Divergent divergent { get; private set; }
		public List<ElevatedEdge> edges { get; private set; }

		public Surface(Divergent divergent, ElevatedEdge e0, ElevatedEdge e1) {
			this.divergent = divergent;
			edges = new List<ElevatedEdge>();

			if(divergent == Divergent.Exterior) {
				
			}

			edges.Add(e0);
			edges.Add(e1);
		}

		public void Triangulate(List<int> indices) {
			if(divergent == Divergent.Exterior) {
				var e0v = edges[0].elevatedVertices;
				var e1v = edges[1].elevatedVertices;

				int size = e0v.Count - 1;

				for(var i = 0; i < size - 1; ++i) {
					indices.Add(e0v[i].index);
					indices.Add(e0v[i + 1].index);
					indices.Add(e1v[i].index);

					indices.Add(e0v[i + 1].index);
					indices.Add(e1v[i + 1].index);
					indices.Add(e1v[i].index);
				}

				indices.Add(e0v[size - 1].index);
				indices.Add(e0v[size].index);
				indices.Add(e1v[size - 1].index);
			} else {
				var e0v = edges[0].elevatedVertices;
				var e1v = edges[1].elevatedVertices;

				int size = e0v.Count - 1;

				for(var i = size; i > 0; --i) {
					indices.Add(e0v[i].index);
					indices.Add(e0v[i - 1].index);
					indices.Add(e1v[i].index);

					indices.Add(e0v[i - 1].index);
					indices.Add(e1v[i - 1].index);
					indices.Add(e1v[i].index);
				}

				indices.Add(e0v[1].index);
				indices.Add(e0v[0].index);
				indices.Add(e1v[1].index);
			}
		}
	}
}