using System.Collections;
using System.Collections.Generic;
using UniTriangulation2D;
using UnityEngine;

namespace UniTeddy {

	public class Edge2D {

		public Vector2 a { get; private set; }
		public Vector2 b { get; private set; }

		public Vector2 midpoint { get; private set; }
		public float length { get; private set; }
		public bool isInternal { get; private set; }

		Circle2D _circumscribedCircle;
		public Circle2D circumscribedCircle {
			get {
				if(_circumscribedCircle == null) _circumscribedCircle = new Circle2D(midpoint, (a - midpoint).magnitude);
				return _circumscribedCircle;
			}
		}

		public Edge2D(Vector2 a, Vector2 b, bool isInternal) {
			this.a = a;
			this.b = b;
			this.midpoint = (a + b) * 0.5f;
			this.length = (a - b).magnitude * 0.5f;

			this.isInternal = isInternal;
		}

		public static bool operator ==(Edge2D a, Edge2D b) {
			return a.GetHashCode() == b.GetHashCode();
		}

		public static bool operator !=(Edge2D a, Edge2D b) {
			return a.GetHashCode() != b.GetHashCode();
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

		public Vector2 ExcludePoint(Vector2 p) {
			return a == p ? b : a;
		}
	}
}