using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniTeddy {

	/// <summary>
	/// 頂点情報
	/// </summary>
	public class Vertex2D {

		public Vector2 p { get; private set;}

		public float elevation { get; set; }

		public Vertex2D(Vector2 p) {
			this.p = p;
		}
	}
}