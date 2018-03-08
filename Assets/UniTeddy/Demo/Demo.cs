using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleStorage;
using UnityEngine.Events;

namespace UniTeddy {

	[RequireComponent(typeof(LineRenderer), typeof(MeshRenderer), typeof(MeshFilter))]
	public class Demo : MonoBehaviour {

		[System.Serializable]
		public class TeddyEvent : UnityEvent<Teddy> { }

		[Range(0.01f, 1f)]
		public float interval = 0.25f;
		public TextAsset json;

		public bool drawVertices = true;
		public bool drawFaces = true;
		public bool drawConnection = true;
		public bool drawChordalAxis = true;
		public bool drawSkeleton = true;

		public TeddyEvent onBuild;

		List<Vector2> _points;
		bool _isDragging;

		Teddy _teddy;

		LineRenderer _lineRenderer;

		void Awake() {
			_lineRenderer = GetComponent<LineRenderer>();

			if(json) {
				_points = Storage.LoadList<Vector2>(json);
				Build(_points);
			} else {
				_points = new List<Vector2>();
			}
		}

		void Update() {
			if(Input.GetMouseButtonDown(0)) {
				ClearPositions();
				_isDragging = true;
				AddPosition(GetMousePosition());

			} else if(Input.GetMouseButtonUp(0)) {
				_isDragging = false;
				Build(_points);

			} else if(_isDragging) {
				Vector2 pos = GetMousePosition();
				if((pos - _points[_points.Count - 1]).magnitude > interval) {
					AddPosition(pos);
				}
			}
			if(_teddy != null) {
				if(drawVertices) {
					foreach(var v in _teddy.vset.vertices) {
						v.DebugDraw(Color.magenta);
					}
				}
				if(drawFaces) {
					foreach(var f in _teddy.faces) {
						f.DebugDraw();
					}
				}
				if(drawConnection) {
					_teddy.connection.DebugDraw();
				}
				if(drawChordalAxis) {
					_teddy.axis.DebugDraw();
				}
				if(drawSkeleton) {
					_teddy.skeleton.DebugDraw();
				}
			}
		}

		void OnDestroy() {
			Storage.SaveList<Vector2>(_points, "contour.json");
		}

		void Build(List<Vector2> contour) {
			var sw = System.Diagnostics.Stopwatch.StartNew();
			var teddy = new Teddy(contour);
			sw.Stop();
			Debug.Log(string.Format("elapsed: {0}(ms)", sw.ElapsedMilliseconds));

			onBuild.Invoke(teddy);
		}

		Vector2 GetMousePosition() {
			Vector3 mpos = Input.mousePosition;
			mpos.z = -Camera.main.transform.position.z;
			return Camera.main.ScreenToWorldPoint(mpos);
		}

		void AddPosition(Vector2 point) {
			_points.Add(point);
			_lineRenderer.positionCount = _points.Count;
			_lineRenderer.SetPosition(_points.Count - 1, point);
		}

		void ClearPositions() {
			_points.Clear();
			_lineRenderer.positionCount = 0;
		}
	}
}