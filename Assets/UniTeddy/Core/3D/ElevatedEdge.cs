using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniTeddy {

	/// <summary>
	/// 高さ情報を持った辺
	/// </summary>
	public class ElevatedEdge {

		public Vector2 foot { get; private set; }
		public Vector2 top { get; private set; }

		public float topElevation { get; private set; }

		public List<IndexedVertex> elevatedVertices { get; private set; }

		public ElevatedEdge(Vector2 foot, Vector2 top, float topElevation) {
			this.foot = foot;
			this.top = top;
			this.topElevation = topElevation;
		}

		public override int GetHashCode() {
			return foot.GetHashCode() * 123 ^ top.GetHashCode() * 456;
		}

		public override bool Equals(object obj) {
			var e = obj as ElevatedEdge;
			return GetHashCode() == e.GetHashCode();
		}

		public int Elevate(int startIndex, int divnum = 8, float scale = 1f) {
			elevatedVertices = new List<IndexedVertex>(divnum + 1);

			Vector3 start = foot;
			Vector3 end = top;
			end.z = -topElevation;
			Vector3 c = foot;
			c.z = -topElevation;

			for(var i = 0; i < divnum + 1; ++i) {
				elevatedVertices.Add(new IndexedVertex(GetPoint(start, c, end, (float)i / divnum), startIndex++));
			}

			/*
			Vector3 foot = this.foot;
			Vector3 step = this.top - this.foot;
			step.z = -topElevation;
			step /= divnum;

			for(var i = 0; i < divnum + 1; ++i) {
				Vector3 pos = foot + step * i;
				elevatedVertices.Add(new IndexedVertex(pos, startIndex++));
			}
			*/
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