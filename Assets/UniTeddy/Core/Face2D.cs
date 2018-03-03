using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UniTriangulation2D;

namespace UniTeddy {

	public class Face2D {

		public enum Category {
			Terminal, Sleeve, Junction
		}

		public Category category { get; set; }
		public Triangle2D triangle { get; private set; }

		public Edge2D[] edges { get; private set; }

		public Edge2D e0 { get { return edges[0]; } }
		public Edge2D e1 { get { return edges[1]; } }
		public Edge2D e2 { get { return edges[2]; } }

		public Face2D(Triangle2D t) {
			this.triangle = t;
			this.edges = new Edge2D[3];
		}

		public Vector2 ExcludePoint(Edge2D e) {
			if(!e.HasPoint(triangle.p0)) {
				return triangle.p0;
			} else if(!e.HasPoint(triangle.p1)) {
				return triangle.p1;
			}
			return triangle.p2;
		}

		public void DebugDraw() {
			switch(category) {
			case Category.Terminal:
				triangle.DebugDraw(Color.red);
				break;
			case Category.Sleeve:
				triangle.DebugDraw(Color.blue);
				break;
			case Category.Junction:
				triangle.DebugDraw(Color.yellow);
				break;
			}
		}
	}
}