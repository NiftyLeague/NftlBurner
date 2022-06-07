using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpriteLoader : MonoBehaviour
{
	private static SpriteLoader I;


	private void Awake()
	{
		if (I == null)
		{
			I = this;
		}
	}

	public static void LoadSpritesheet(string url, int columns, int rows, int cellWith, int cellHeight, Action<List<Sprite>> onComplete, Action<int, int> onProgress = null)
	{
		I.StartCoroutine(I._LoadSpritesheet(url, columns, rows, cellWith, cellHeight, onComplete, onProgress));
	}

	private IEnumerator _LoadSpritesheet(string url, int columns, int rows, int cellWith, int cellHeight, Action<List<Sprite>> onComplete, Action<int, int> onProgress)
	{
		UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(www.error);
		}
		else
		{
			Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
			texture.filterMode = FilterMode.Point;
			yield return GetSprites(texture, columns, rows, cellWith, cellHeight, onComplete, onProgress);
			yield break;
		}
		onComplete(null);
	}

	private IEnumerator GetSprites(Texture2D texture, int columns, int rows, int cellWith, int cellHeight, Action<List<Sprite>> onComplete, Action<int, int> onProgress)
	{
		if (texture.width != columns * cellWith || texture.width != rows * cellHeight)
		{
			Debug.Log($"The dimension of the texture ({texture.width}x{texture.height}) does not match the column and rows and cell dimension provided");
			yield break;
		}

		int frame = 0;
		int height = rows * cellHeight;
		List<Sprite> sprites = new List<Sprite>();
		for (int j = 1; j <= rows; j++)
		{
			for (int i = 0; i < columns; i++)
			{
				Sprite s = Sprite.Create(texture, new Rect(i * cellWith, height - (j * cellHeight), cellWith, cellHeight), new Vector2(0.5f, 0.5f), 16);
				s.name = frame.ToString();
				sprites.Add(s);
				frame++;
				yield return new WaitForEndOfFrame();
				if (onProgress != null)
				{
					onProgress(frame, 250);
				}
			}
		}
		onComplete(sprites);
	}
}
