using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniTeddy {

	[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
	public class DemoObject : MonoBehaviour {

		public Vector3 axis = new Vector3(10f, 5f, 2f);

		MeshRenderer _renderer;
		MeshFilter _filter;

		void Awake() {
			_renderer = GetComponent<MeshRenderer>();
			_filter = GetComponent<MeshFilter>();
		}

		void Update() {
			transform.Rotate(axis * Time.deltaTime);
		}

		public void OnBuild(Teddy teddy) {
			_filter.sharedMesh = teddy.volume.ToMesh();
		}
	}
}