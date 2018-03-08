using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UniTeddy {

	public class SkeletalVolume {

		public List<Surface> surfaces { get; private set; }
		public List<ElevatedEdge> edges { get; private set; }

		List<Vector3> vertices { get; set; }
		List<int> indices { get; set; }

		public SkeletalVolume(List<Surface> surfaces, List<ElevatedEdge> edges) {
			this.surfaces = surfaces;
			this.edges = edges;

			vertices = new List<Vector3>();
			indices = new List<int>();
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

		public void ElevateEdge() {
			vertices.Clear();
			var index = 0;
			foreach(var edge in edges) {
				index = edge.Elevate(index);
				vertices.AddRange(edge.elevatedVertices.Select(v => { return v.p; }));
			}
		}

		public void TriangulateSurface() {
			indices.Clear();
			foreach(var surf in surfaces) {
				surf.Triangulate(indices);
			}
		}
	}
}