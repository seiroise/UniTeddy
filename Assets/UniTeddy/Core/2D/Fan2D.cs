using System.Collections;
using System.Collections.Generic;
using UniTriangulation2D;
using UnityEngine;

namespace UniTeddy {

	public class Fan2D {

		public Vector2 baseVertex { get; private set; }
		public List<Vector2> fanVertices { get; private set; }
		public float baseElevation { get; private set; }

		public Face2D baseFace { get; set; }
		public Chord2D baseChord { get; set; }

		public Fan2D(Vector2 baseVertex, List<Vector2> fanVertices, List<Vector2> contour, Face2D baseFace, Chord2D baseChord) {
			this.baseVertex = baseVertex;
			this.fanVertices = AlignFanVertices(fanVertices, contour);
			this.baseFace = baseFace;
			this.baseChord = baseChord;

			this.baseElevation = ComputeBaseElevation();
		}

		List<Vector2> AlignFanVertices(List<Vector2> src, List<Vector2> contour) {
			var dst = new List<Vector2>(src.Count);
			for(var i = 0; i < contour.Count; ++i) {
				for(var j = 0; j < src.Count; ++j) {
					if(contour[i] == src[j]) {
						dst.Add(src[j]);
						break;
					}
				}
			}
			return dst;
		}

		float ComputeBaseElevation() {
			float sum = 0f;
			for(var i = 0; i < fanVertices.Count; ++i) {
				sum += (fanVertices[i] - baseVertex).sqrMagnitude;
			}
			return Mathf.Sqrt(sum) / fanVertices.Count;
		}

		public void DebugDraw(Color color) {
			for(int i = 0; i < fanVertices.Count - 1; ++i) {
				DebugExtention.DrawTriangle(baseVertex, fanVertices[i], fanVertices[i + 1], color);
			}
		}
	}
}