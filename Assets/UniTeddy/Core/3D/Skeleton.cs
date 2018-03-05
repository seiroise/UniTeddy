using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniTeddy {

	/// <summary>
	/// 立体の骨格
	/// </summary>
	public class Skeleton {

		public List<Surface> surfaces { get; private set; }
		public List<ElevatedEdge> edges { get; private set; }

		List<Vector3> vertices;
		List<int> indices;

		public Skeleton(List<Surface> surfaces, List<ElevatedEdge> edges) {
			this.surfaces = surfaces;
			this.edges = edges;
			this.vertices = new List<Vector3>();
			this.indices = new List<int>();
		}

		public Mesh ToMesh() {
			ElevateEdge();
			TriangulateSurface();

			var mesh = new Mesh();
			mesh.name = "Teddy mesh";

			mesh.SetVertices(vertices);
			mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);

			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			mesh.RecalculateTangents();

			return mesh;
		}

		void ElevateEdge() {
			this.vertices.Clear();
			var index = 0;
			foreach(var edge in edges) {
				index = edge.Elevate(index);
				this.vertices.AddRange(edge.elevatedVertices.Select(v => { return v.p; }));
			}
		}

		void TriangulateSurface() {
			this.indices.Clear();
			foreach(var surf in surfaces) {
				surf.Triangulate(this.indices);
			}
		}
	}
}