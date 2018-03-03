using System.Collections;
using System.Collections.Generic;
using UniTriangulation2D;
using UnityEngine;

namespace UniTeddy {

	public class Chord2D {

		public Vector2 src { get; set; }
		public Vector2 dst { get; set; }

		public Edge2D srcEdge { get; set; }
		public Edge2D dstEdge { get; set; }

		public Face2D face { get; set; }

		public List<Chord2D> connections { get; set; }

		public Chord2D() {
			connections = new List<Chord2D>();
		}

		public void DebugDraw() {
			Debug.DrawLine(src, dst, Color.cyan);
			for(var i = 0; i < connections.Count; ++i) {
				connections[i].DebugDraw();
			}
		}
	}
}