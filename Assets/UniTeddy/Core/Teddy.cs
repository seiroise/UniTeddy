using System.Collections;
using System.Collections.Generic;
using UniTriangulation2D;
using UnityEngine;

namespace UniTeddy {

	public class Teddy {

		public VerticesSet2D vset { get; private set; }
		public List<Face2D> faces { get; private set; }
		public EdgeConnection2D connection { get; private set; }
		public ChordalAxis2D axis { get; private set; }
		public Skeleton2D skeleton { get; private set; }
		public SkeletalVolume volume { get; private set; }

		public Teddy(List<Vector2> contour) {
			if(contour.Count < 4) {
				throw new System.ArgumentException("輪郭の頂点数は4つ以上必要です。");
			}

			// 面情報を作成する。
			var delaunay = Delaunay2D.Contour(contour);
			vset = MakeVerticesSet(contour);
			faces = MakeFaces(delaunay, contour, vset);

			// 辺による接続関係を作成する
			connection = MakeEdgeConnection(faces);

			// 面同士の接続関係を作成する
			var terminal = faces.Find(t => t.category == Face2D.Category.Terminal);
			axis = new ChordalAxis2D(terminal, connection);

			// 平面の骨格情報
			skeleton = new Skeleton2D(axis, contour);

			// 立体に変換
			volume = skeleton.ToSkeletalVolume();
		}

		/// <summary>
		/// 頂点セットを作成する
		/// </summary>
		/// <param name="points">Points.</param>
		VerticesSet2D MakeVerticesSet(List<Vector2> points) {
			var vSet = new VerticesSet2D();
			for(var i = 0; i < points.Count; ++i) {
				var v = vSet.CheckAndAdd(points[i]);
				v.isExterior = true;
			}
			return vSet;
		}

		/// <summary>
		/// 面情報を作成する。
		/// </summary>
		/// <returns>The faces.</returns>
		/// <param name="delaunay">Delaunay.</param>
		/// <param name="vSet">V set.</param>
		List<Face2D> MakeFaces(Delaunay2D delaunay, List<Vector2> contour, VerticesSet2D vSet) {
			var faces = new List<Face2D>();
			delaunay.ForeachTriangles(t => {
				faces.Add(MakeFace(t, contour, vSet));
			});

			return faces;
		}

		/// <summary>
		/// 面情報を作成する。
		/// </summary>
		/// <returns>The face.</returns>
		/// <param name="t">T.</param>
		/// <param name="contour">Contour.</param>
		/// <param name="vSet">V set.</param>
		Face2D MakeFace(Triangle2D t, List<Vector2> contour, VerticesSet2D vSet) {
			var a = vSet.FindNear(t.p0);
			var b = vSet.FindNear(t.p1);
			var c = vSet.FindNear(t.p2);

			var g = vSet.CheckAndAdd((a.p + b.p + c.p) / 3f);
			g.isExterior = false;

			var face = new Face2D(a, b, c, g);

			int externalCount = 0;
			bool isExterior = false;
			for(int i = 0, j = 2; i < 3; j = i++) {
				isExterior = IsContourEdge(contour, face.vertices[i].p, face.vertices[j].p);
				externalCount += isExterior ? 1 : 0;
				var mid = vSet.CheckAndAdd((face.vertices[j].p + face.vertices[i].p) * 0.5f);
				mid.isExterior = false;
				var edge = new Edge2D(face.vertices[j], face.vertices[i], mid, isExterior);
				face.edges[j] = edge;
			}

			switch(externalCount) {
				case 0:
					face.category = Face2D.Category.Junction;
					break;
				case 1:
					face.category = Face2D.Category.Sleeve;
					break;
				case 2:
					face.category = Face2D.Category.Terminal;
					break;
				default:
					throw new System.Exception("輪郭情報とマッチしません。");
			}

			return face;
		}

		/// <summary>
		/// 入力した二つの頂点が輪郭状の
		/// </summary>
		/// <returns><c>true</c>, if contour edge was ised, <c>false</c> otherwise.</returns>
		/// <param name="contour">Contour.</param>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		bool IsContourEdge(List<Vector2> contour, Vector2 a, Vector2 b) {
			for(int i = 0, j = contour.Count - 1; i < contour.Count; j = i++) {
				if((contour[j] == a && contour[i] == b) || (contour[i] == a && contour[j] == b)) {
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 辺による面の接続関係を作成する。
		/// </summary>
		/// <returns>The edge connection.</returns>
		/// <param name="faces">Faces.</param>
		EdgeConnection2D MakeEdgeConnection(List<Face2D> faces) {
			var edgeConnection = new EdgeConnection2D();
			for(var i = 0; i < faces.Count; ++i) {
				edgeConnection.AddFace(faces[i]);
			}
			return edgeConnection;
		}
	

	}
}