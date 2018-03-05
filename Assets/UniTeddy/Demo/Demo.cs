﻿using System.Collections;
using System.Collections.Generic;
using UniTriangulation2D;
using UnityEngine;

namespace UniTeddy {

	[RequireComponent(typeof(LineRenderer), typeof(MeshFilter), typeof(MeshRenderer))]
	public class Demo : MonoBehaviour {

		Teddy _teddy;

		List<Vector2> _points;
		bool _isDragging;

		LineRenderer _lineRenderer;
		MeshFilter _meshFilter;

		void Awake() {
			_points = new List<Vector2>();
			_lineRenderer = GetComponent<LineRenderer>();
			_meshFilter = GetComponent<MeshFilter>();

			/*
			// Edge2Dの等価性テスト
			Vector2 p0 = new Vector2(0f, 1f);
			Vector2 p1 = new Vector2(10f, 5f);

			Edge2D e0 = new Edge2D(p0, p1);
			Edge2D e1 = new Edge2D(p0, p1);
			Edge2D e2 = new Edge2D(p1, p0);
			*/

			/*
			// Vector2 の透過性テスト
			Vector2 p0 = new Vector2(0f, 1f);
			Vector2 p1 = new Vector2(1f, 0f);
			Debug.Log(p0 == p1);
			Debug.Log(p0.GetHashCode() == p1.GetHashCode());
			*/
		}

		void Update() {

			if(Input.GetMouseButtonDown(0)) {
				ClearPositions();
				_isDragging = true;
				AddPosition(GetMousePosition());
			} else if(Input.GetMouseButtonUp(0)) {
				_isDragging = false;
				/*
				for(var i = 0; i < _points.Count; ++i) {
					Debug.Log(_points[i]);
				}
				*/
				var sw = System.Diagnostics.Stopwatch.StartNew();
				_teddy = new Teddy(_points);
				sw.Stop();
				Debug.Log(sw.ElapsedMilliseconds);

				_meshFilter.sharedMesh = _teddy.skeleton.ToMesh();

			} else if(Input.GetMouseButton(0)) {
				Vector2 pos = GetMousePosition();
				if((pos - _points[_points.Count - 1]).magnitude > 0.5f) {
					AddPosition(pos);
				}
			}

			if(_teddy != null) {
				var faces = _teddy.faces;
				for(var i = 0; i < faces.Count; ++i) {
					faces[i].DebugDraw();
				}
				_teddy.axis.DebugDraw(Color.cyan);
			}
		}

		Vector2 GetMousePosition() {
			Vector3 mpos = Input.mousePosition;
			mpos.z = -Camera.main.transform.position.z;
			return Camera.main.ScreenToWorldPoint(mpos);
		}

		void AddPosition(Vector2 position) {
			_points.Add(position);
			_lineRenderer.positionCount = _points.Count;
			_lineRenderer.SetPosition(_points.Count - 1, position);
		}

		void ClearPositions() {
			_points.Clear();
			_lineRenderer.positionCount = 0;
		}
	}
}