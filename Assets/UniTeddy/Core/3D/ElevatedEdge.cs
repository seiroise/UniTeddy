using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniTeddy {

	/// <summary>
	/// 高さ情報を持った辺
	/// </summary>
	public class ElevatedEdge {

		public Vertex2D foot { get; private set; }
		public Vertex2D top { get; private set; }

		public List<IndexedVertex> elevatedVertices { get; private set; }

		public ElevatedEdge(Vertex2D foot, Vertex2D top) {
			this.foot = foot;
			this.top = top;
		}

		public override int GetHashCode() {
			return foot.GetHashCode() * 123 ^ top.GetHashCode() * 456;
		}

		public int Elevate(int startIndex, int divnum = 8, float scale = 1f) {
			elevatedVertices = new List<IndexedVertex>(divnum + 1);

			Vector3 start = foot.p;
			Vector3 end = top.p;
			end.z = -top.elevation;
			Vector3 c = foot.p;
			c.z = end.z;

			for(var i = 0; i < divnum + 1; ++i) {
				elevatedVertices.Add(new IndexedVertex(GetPoint(start, c, end, (float)i / divnum), startIndex++));
			}
			return startIndex;
		}

		public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t) {
			t = Mathf.Clamp01(t);
			float oneMinusT = 1f - t;
			return oneMinusT * oneMinusT * p0 +
				2f * oneMinusT * t * p1 +
				t * t * p2;
		}
	}
}