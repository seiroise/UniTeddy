using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniTeddy {

	/// <summary>
	/// メッシュ作成のために番号づけされた座標
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