using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniTeddy {

	public class ChordalAxis2D {

		public Chord2D origin { get; private set; }
		public List<Chord2D> sleeveChords { get; private set; }
		public List<Chord2D> junctionChords { get; private set; }
		public List<Chord2D> terminalChords { get; private set; }

		public ChordalAxis2D(Face2D terminal, EdgeConnection2D connection) {
			sleeveChords = new List<Chord2D>();
			junctionChords = new List<Chord2D>();
			terminalChords = new List<Chord2D>();

			MakeChordalAxis(terminal, connection);
		}

		/// <summary>
		/// 終端の面と接続情報から軸を作成する
		/// </summary>
		/// <param name="terminal">Terminal.</param>
		/// <param name="connection">Connection.</param>
		void MakeChordalAxis(Face2D terminal, EdgeConnection2D connection) {
			if(terminal.category != Face2D.Category.Terminal) {
				throw new System.ArgumentException("最初の面はterminalでなければなりません。");
			}

			var origin = new Chord2D();
			origin.face = terminal;
			for(int i = 0, j = terminal.edges.Length - 1; i < terminal.edges.Length; j = i++) {
				if(!terminal.edges[i].isExterior) {
					origin.src = terminal.vertices[j];
					origin.dstEdge = terminal.edges[i];
					origin.dst = origin.dstEdge.mid;
					break;
				}
			}

			origin.pair = origin;

			this.terminalChords.Add(origin);
			this.origin = origin;

			Face2D neighbor;
			Stack<Chord2D> exploring = new Stack<Chord2D>();
			List<Chord2D> temporary = new List<Chord2D>();

			exploring.Push(origin);

			while(exploring.Count > 0) {
				var cur = exploring.Pop();
				if(cur == null) {
					continue;
				}

				if(!connection.TryGetNeighbor(cur.dstEdge, cur.face, out neighbor)) {
					throw new System.ArgumentException("接続情報が構築されていません");
				}

				temporary.Clear();
				switch(neighbor.category) {
					case Face2D.Category.Sleeve:
						MakeSleeveChord(cur, neighbor, ref temporary);
						break;
					case Face2D.Category.Junction:
						MakeJunctionChord(cur, neighbor, ref temporary);
						break;
					case Face2D.Category.Terminal:
						MakeTerminalChord(cur, neighbor, ref temporary);
						break;
				}
				for(int i = 0; i < temporary.Count; ++i) {
					exploring.Push(temporary[i]);
				}
			}
		}

		/// <summary>
		/// Sleeve部分のChordを作成する
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="neighbor">Neighbor.</param>
		/// <param name="pushChords">Push chords.</param>
		void MakeSleeveChord(Chord2D from, Face2D neighbor, ref List<Chord2D> pushChords) {
			Edge2D[] nextDstEdge;
			if(!neighbor.TryGetOtheInteriorEdge(from.dstEdge, out nextDstEdge)) {
				throw new System.Exception("辺の内外判定が不正です。");
			}

			var chord = new Chord2D();
			chord.face = neighbor;
			chord.src = from.dst;
			chord.srcEdge = from.dstEdge;
			chord.dstEdge = nextDstEdge[0];
			chord.dst = chord.dstEdge.mid;

			from.connections.Add(chord);

			var pair = new Chord2D();
			pair.face = neighbor;
			pair.src = chord.dst;
			pair.srcEdge = chord.dstEdge;
			pair.dstEdge = chord.srcEdge;
			pair.dst = from.dst;

			pair.connections.Add(from.pair);

			chord.pair = pair;
			pair.pair = chord;

			pushChords.Add(chord);
			sleeveChords.Add(chord);
		}

		/// <summary>
		/// Junction部分のChordを作成する
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="neighbor">Neighbor.</param>
		/// <param name="pushChords">Push chords.</param>
		void MakeJunctionChord(Chord2D from, Face2D neighbor, ref List<Chord2D> pushChords) {
			var chord = new Chord2D();
			chord.face = neighbor;
			chord.src = from.dst;
			chord.srcEdge = from.dstEdge;
			chord.dst = neighbor.g;

			from.connections.Add(chord);

			var pair = new Chord2D();
			pair.face = neighbor;
			pair.src = chord.dst;
			pair.dstEdge = from.dstEdge;
			pair.dst = chord.src;

			pair.connections.Add(from.pair);

			chord.pair = pair;
			pair.pair = chord;

			junctionChords.Add(chord);

			Edge2D[] otherEdges;
			if(!neighbor.TryGetOtheInteriorEdge(from.dstEdge, out otherEdges)) {
				throw new System.Exception("辺の内外判定が不正です。");
			}
			foreach(var e in otherEdges) {
				var c = new Chord2D();
				c.face = neighbor;
				c.src = chord.dst;
				c.dst = e.mid;
				c.dstEdge = e;

				chord.connections.Add(c);

				var p = new Chord2D();
				p.face = neighbor;
				p.src = c.dst;
				c.srcEdge = e;
				p.dst = c.src;

				p.connections.Add(pair);

				c.pair = p;
				p.pair = c;

				junctionChords.Add(c);

				pushChords.Add(c);
			}

			var cp0 = chord.connections[0].pair;
			cp0.connections.Add(chord.connections[1]);

			var cp1 = chord.connections[1].pair;
			cp1.connections.Add(chord.connections[0]);
		}

		/// <summary>
		/// Terminal部分のChordを作成する
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="neighbor">Neighbor.</param>
		/// <param name="pushChords">Push chords.</param>
		void MakeTerminalChord(Chord2D from, Face2D neighbor, ref List<Chord2D> pushChords) {
			var chord = new Chord2D();
			chord.face = neighbor;
			chord.src = neighbor.ExcludeVertex(from.dstEdge);
			chord.dstEdge = from.dstEdge;
			chord.dst = chord.dstEdge.mid;

			chord.pair = chord;

			chord.connections.Add(from.pair);
			from.connections.Add(chord);

			terminalChords.Add(chord);
		}

		/// <summary>
		/// 入力したChordを削除する
		/// </summary>
		/// <param name="chord">Chord.</param>
		public void RemoveChord(Chord2D chord) {
			switch(chord.face.category) {
				case Face2D.Category.Sleeve:
					sleeveChords.Remove(chord);
					sleeveChords.Remove(chord.pair);
					break;
				case Face2D.Category.Junction:
					junctionChords.Remove(chord);
					junctionChords.Remove(chord.pair);
					break;
				case Face2D.Category.Terminal:
					terminalChords.Remove(chord);
					terminalChords.Remove(chord.pair);
					break;
			}
			chord.DisconnectSelf();
		}

		/// <summary>
		/// デバッグ用の簡易描画
		/// </summary>
		public void DebugDraw() {
			foreach(var c in terminalChords) {
				c.DebugDraw(Color.red);
			}
			foreach(var c in sleeveChords) {
				c.DebugDraw(Color.blue);
			}
			foreach(var c in junctionChords) {
				c.DebugDraw(Color.yellow);
			}
		}
	}
}