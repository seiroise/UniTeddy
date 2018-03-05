using System.Collections;
using System.Collections.Generic;
using UniTriangulation2D;
using UnityEngine;

namespace UniTeddy {

	public class Chord2D {

		public Vector2 src { get; set; }
		public Vector2 dst { get; set; }

		Edge2D _srcEdge, _dstEdge;
		public Edge2D srcEdge { get { return _srcEdge; } set { _srcEdge = value; src = value.midpoint; } }
		public Edge2D dstEdge { get { return _dstEdge; } set { _dstEdge = value; dst = value.midpoint; } }

		public Face2D face { get; set; }

		public Chord2D pair { get; set; }

		public List<Chord2D> connections { get; set; }

		public Chord2D() {
			connections = new List<Chord2D>();
		}

		public void DebugDraw(Color color) {
			DebugExtention.DrawArrow(src, dst, color);
			if(pair != null) {
				DebugExtention.DrawArrow(pair.src, pair.dst, color * 0.75f);
			}
		}
	}
}