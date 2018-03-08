using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SimpleStorage {

	/// <summary>
	/// 主にJson形式でデータのやりとりを行う簡易のストレージ
	/// </summary>
	public class Storage {

		[Serializable]
		public class SerializableList<T> {
			
			[SerializeField]
			List<T> _target;

			public List<T> ToList() { return _target; }

			public SerializableList(List<T> target) {
				_target = target;
			}
		}

		/// <summary>
		/// リストをjson形式で保存する。
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="filename">Filename.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void SaveList<T>(List<T> target, string filename) {
			var text = JsonUtility.ToJson(new SerializableList<T>(target));
			Save(text, filename);
		}

		/// <summary>
		/// json形式のリストを読み込む。
		/// </summary>
		/// <returns>The list.</returns>
		/// <param name="filename">Filename.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static bool TryLoadList<T>(string filename, out List<T> target) {
			string text;
			if(TryLoad(filename, out text)) {
				target = JsonUtility.FromJson<SerializableList<T>>(text).ToList();
				return true;
			} else {
				target = null;
				return false;
			}
		}

		/// <summary>
		/// 入力した文字列をfilenameのファイルに書き込む
		/// </summary>
		/// <returns>The save.</returns>
		/// <param name="text">Text.</param>
		/// <param name="filename">Filename.</param>
		public static void Save(string text, string filename) {
			string path = Path.Combine(Application.streamingAssetsPath, filename);
			FileInfo fi = new FileInfo(path);
			using(StreamWriter w = fi.CreateText()) {
				w.WriteLine(text);
				w.Flush();
			}
		}

		/// <summary>
		/// filenameのファイルからテキストを読み込み、返す。
		/// </summary>
		/// <returns>The load.</returns>
		/// <param name="filename">Filename.</param>
		public static bool TryLoad(string filename, out string text) {
			string path = Path.Combine(Application.streamingAssetsPath, filename);
			if(File.Exists(path)) {
				text = File.ReadAllText(path);
				return true;
			} else {
				text = "";
				return false;
			}
		}

		/// <summary>
		/// TextAssetからリストを読み込む
		/// </summary>
		/// <returns>The list.</returns>
		/// <param name="asset">Asset.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static List<T> LoadList<T>(TextAsset asset) {
			return JsonUtility.FromJson<SerializableList<T>>(asset.text).ToList();
		}
	}
}