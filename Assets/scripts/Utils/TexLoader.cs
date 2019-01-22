using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TexLoader : MonoBehaviour {
	public static Dictionary<string, Texture2D> loadedTexturesDict = new Dictionary<string, Texture2D>();

	public static string TexBasePath
	{
		get { 
			return Path.Combine(Application.streamingAssetsPath, "storedAssets");
		}
	}



    public static Texture LoadFromFile(string filePath) {
		byte[] texBytes = null;
		string fullPath = Path.Combine(TexBasePath, filePath);
		if (File.Exists(fullPath)) {
			texBytes = File.ReadAllBytes(fullPath);
		}

		Texture2D newTex = new Texture2D(2, 2);
		if (newTex.LoadImage(texBytes)) {
			loadedTexturesDict.Add(filePath, newTex);
			return newTex;
		}
		else {
			return null;
		}

	}

	public static Texture GetOrLoadTex(string filePath) {
		if (loadedTexturesDict.ContainsKey(filePath)) {
			return loadedTexturesDict[filePath];
		}else {
			return LoadFromFile(filePath);
		}
	}

	/// <summary>
	/// destroys all textures stored in the dict
	/// </summary>
	public static void PurgeTexDict() {
		foreach(KeyValuePair<string, Texture2D> kvp in loadedTexturesDict) {
			Destroy(kvp.Value);
		}

		loadedTexturesDict.Clear();
	}
}