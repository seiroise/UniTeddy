using System.Collections;
using System.Collections.Generic;
using UniTriangulation2D;
using UnityEngine;

namespace UniTeddy {

	public class Edge2D {

		public Vertex2D a { get; private set; }
		public Vertex2D b { get; private set; }
		public Vertex2D mid { get; private set; }

		public bool isExterior { get; private set; }

		Circle2D _circumscribedCirle;
		public Circle2D circumscribedCircle {
			get {
				if(_circumscribedCirle == null) _circumscribedCirle = new Circle2D(mid.p, (a.p - mid.p).magnitude);
				return _circumscribedCirle;
			}
		}

		public Edge2D(Vertex2D a, Vertex2D b, Vertex2D mid, bool isExterior) {
			this.a = a;
			this.b = b;
			this.mid = mid;
			this.isExterior = isExterior;
		}

		public override bool Equals(object obj) {
			return (obj as Edge2D).GetHashCode() == GetHashCode();
		}

		public override int GetHashCode() {
			return a.GetHashCode() ^ b.GetHashCode();
		}

		public bool HasVertex(Vertex2D v) {
			return v.GetHashCode() == a.GetHashCode() || v.GetHashCode() == b.GetHashCode();
		}

		public Vertex2D IncludeVertex(Edge2D e) {
			if(e.HasVertex(a)) {
				return a;
			} else {
				return b;
			}
		}

		public Vertex2D GetOtherVertex(Vertex2D v) {
			return v == a ? b : a;
		}
	}
}