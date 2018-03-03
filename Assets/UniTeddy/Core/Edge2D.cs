using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniTeddy {

	public class Edge2D {

		public Vector2 a { get; private set; }
		public Vector2 b { get; private set; }

		public Vector2 midpoint { get; private set; }

		public bool isInternal { get; private set; }

		public Edge2D(Vector2 a, Vector2 b, bool isInternal) {
			this.a = a;
			this.b = b;
			this.midpoint = (a + b) * 0.5f;

			this.isInternal = isInternal;
		}

		public override bool Equals(object obj) {
			Edge2D s = obj as Edge2D;
			return s.GetHashCode() == GetHashCode();
		}

		public override int GetHashCode() {
			return a.GetHashCode() ^ b.GetHashCode();
		}

		public bool HasPoint(Vector2 p) {
			return (a.GetHashCode() == p.GetHashCode()) || (b.GetHashCode() == p.GetHashCode());
		}
	}
}