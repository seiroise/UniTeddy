﻿using System.Collections;
using System.Collections.Generic;
using UniTriangulation2D;
using UnityEngine;

namespace UniTeddy {

	public class Vertex2D {

		public Vector2 p { get; private set; }

		public bool isExterior { get; set; }
		public float elevation { get; set; }
		public int index { get; set; }

		public Vertex2D(Vector2 p) {
			this.p = p;
		}

		public override bool Equals(object obj) {
			return (obj as Vertex2D).GetHashCode() == GetHashCode();
		}

		public override int GetHashCode() {
			return p.GetHashCode();
		}

		public void DebugDraw(Color color) {
			DebugExtention.DrawCircle2D(p, elevation * 0.25f + 0.1f, color);
		}
	}
}