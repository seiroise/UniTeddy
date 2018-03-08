using System.Collections;
using System.Collections.Generic;
using UniTriangulation2D;
using UnityEngine;

namespace UniTeddy {

	public class Chord2D {

		public Vertex2D src { get; set; }
		public Vertex2D dst { get; set; }

		public Edge2D srcEdge { get; set; }
		public Edge2D dstEdge { get; set; }

		public Face2D face { get; set; }
		public Chord2D pair { get; set; }
		public List<Chord2D> connections { get; set; }

		public Chord2D() {
			connections = new List<Chord2D>();
		}

		/// <summary>
		/// 他のChordとの接続関係を切る
		/// </summary>
		public void DisconnectSelf() {
			foreach(var c in connections) {
				c.pair.connections.Remove(this);
			}
			foreach(var c in pair.connections) {
				c.pair.connections.Remove(this);
			}
			connections.Clear();
			pair.connections.Clear();
		}

		/// <summary>
		/// デバッグ用の簡易描画
		/// </summary>
		public void DebugDraw(Color color) {
			if(pair == null) {
				DebugExtention.DrawArrow(src.p, dst.p, color);
			} else {
				Vector2 o = Quaternion.AngleAxis(90f, Vector3.forward) * (dst.p - src.p).normalized * 0.02f;
				DebugExtention.DrawArrow(src.p + o, dst.p + o, color, 0.1f);
				DebugExtention.DrawArrow(pair.src.p - o, pair.dst.p - o, color, 0.1f);
			}
		}
	}
}