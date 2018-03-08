using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniTeddy {

	/// <summary>
	/// 番号付された頂点
	/// </summary>
	public class IndexedVertex {

		public Vector3 p { get; private set; }
		public int index { get; private set; }

		public IndexedVertex(Vector3 p, int index) {
			this.p = p;
			this.index = index;
		}
	}
}