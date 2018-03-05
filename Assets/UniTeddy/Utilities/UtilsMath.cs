using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniTeddy {

	public static class UtilsMath {

		public static float Cross(Vector2 a, Vector2 b) {
			return a.x * b.y - a.y * b.x;
		}
	}
}