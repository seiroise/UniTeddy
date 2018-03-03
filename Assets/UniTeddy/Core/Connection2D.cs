using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniTeddy {

	/// <summary>
	/// 辺とそれを挟む面のペアを表現する
	/// </summary>
	public class Connection2D {

		Dictionary<Edge2D, List<Face2D>> _connection;

		int daburi = 0;

		public Connection2D() {
			_connection = new Dictionary<Edge2D, List<Face2D>>();
		}

		/// <summary>
		/// 接続関係に入力された面を追加する。
		/// </summary>
		/// <returns>The add.</returns>
		/// <param name="face">Face.</param>
		public void Add(Face2D face) {
			Add(face, face.e0);
			Add(face, face.e1);
			Add(face, face.e2);
		}

		/// <summary>
		/// 接続関係を追加する。
		/// </summary>
		/// <returns>The add.</returns>
		/// <param name="face">Face.</param>
		/// <param name="edge">Edge.</param>
		void Add(Face2D face, Edge2D edge) {
			List<Face2D> faces;
			if(!_connection.ContainsKey(edge)) {
				_connection.Add(edge, new List<Face2D>() { face });
			} else {
				daburi++;
				_connection.TryGetValue(edge, out faces);
				faces.Add(face);
			}
		}

		/// <summary>
		/// 入力した辺を界に隣接している面を取得する。
		/// </summary>
		/// <returns><c>true</c>, if get neighbor was tryed, <c>false</c> otherwise.</returns>
		/// <param name="edge">Edge.</param>
		/// <param name="face">Face.</param>
		/// <param name="neighbor">Neighbor.</param>
		public bool TryGetNeighbor(Edge2D edge, Face2D face, out Face2D neighbor) {
			List<Face2D> faces;
			if(_connection.TryGetValue(edge, out faces)) {
				neighbor = faces.Find(f => f != face);
				return neighbor != null;
			} else {
				neighbor = null;
				return false;
			}
		}

		public override string ToString() {
			return string.Format("[Connection2D] {0}", daburi);
		}
	}
}