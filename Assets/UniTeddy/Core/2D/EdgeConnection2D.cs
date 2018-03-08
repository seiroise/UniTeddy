using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniTeddy {

	/// <summary>
	/// 辺による面の接続関係
	/// </summary>
	public class EdgeConnection2D {

		Dictionary<Edge2D, List<Face2D>> _connection;

		public EdgeConnection2D() {
			_connection = new Dictionary<Edge2D, List<Face2D>>();
		}

		/// <summary>
		/// 接続関係に入力した面を追加する。
		/// </summary>
		/// <param name="face">Face.</param>
		public void AddFace(Face2D face) {
			for(var i = 0; i < 3; ++i) {
				Add(face, face.edges[i]);
			}
		}

		/// <summary>
		/// 辺と対応する面の接続関係を追加する
		/// </summary>
		/// <returns>The add.</returns>
		/// <param name="face">Face.</param>
		/// <param name="edge">Edge.</param>
		void Add(Face2D face, Edge2D edge) {
			if(!_connection.ContainsKey(edge)) {
				_connection.Add(edge, new List<Face2D>() { face });
			} else {
				_connection[edge].Add(face);
			}
		}

		/// <summary>
		/// 入力した面から入力した辺を介する隣接する面を取得する。
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

		/// <summary>
		/// 簡易描画
		/// </summary>
		/// <param name="interior">Interior.</param>
		/// <param name="exterior">Exterior.</param>
		public void DebugDraw() {
			foreach(var item in _connection) {
				if(!item.Key.isExterior) {
					if(item.Value.Count == 2) {
						// 正しい外部辺
						Debug.DrawLine(item.Key.a.p, item.Key.b.p, Color.cyan);
					} else {
						// 不正な外部辺
						Debug.DrawLine(item.Key.a.p, item.Key.b.p, Color.red);
					}
				} else {
					if(item.Value.Count == 1) {
						// 正しい内部辺
						Debug.DrawLine(item.Key.a.p, item.Key.b.p, Color.blue);
					} else {
						// 不正な内部辺
						Debug.DrawLine(item.Key.a.p, item.Key.b.p, Color.green);
					}
				}
			}
		}
	}
}