using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Analytics : MonoBehaviour
{
	private static Analytics I;

	private static string url = "https://c1iiz7wkq9.execute-api.us-east-1.amazonaws.com/prod/api/event";

	private void Awake()
	{
		if (I != null && I != this && I.gameObject != null)
		{
			DestroyImmediate(gameObject);
			return;
		}
		I = this;
		if (I)
		{
			DontDestroyOnLoad(gameObject);
		}
		SceneManager.sceneLoaded -= OnSceneLoaded;
		SceneManager.sceneLoaded += OnSceneLoaded;

		Dictionary<string, string> data = new Dictionary<string, string>();
		data.Add("OS", SystemInfo.operatingSystemFamily.ToString());
		data.Add("OSFull", SystemInfo.operatingSystem);
		data.Add("Version", Application.version);
		data.Add("Screen", $"{Screen.width}x{Screen.height}");
		I.StartCoroutine(Utils.PostJsonRequest(url, GetCustomEvent("Launch", data)));
	}

	public static void SendPlayerEvent(string name, Dictionary<string, string> details = null)
	{
		if (I == null || NiftyUsers.me == null)
		{
			return;
		}
		if (details == null)
		{
			details = new Dictionary<string, string>();
		}
		details.Add("Address", NiftyUsers.me.address);
		I.StartCoroutine(Utils.PostJsonRequest(url, GetCustomEvent(name, details)));
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		StartCoroutine(Utils.PostJsonRequest(url, GetLevelEvent($"Scene{scene.name}")));
	}

	private static string GetLevelEvent(string page)
	{
		return $"{{'n':'pageview','u':'https://nifty-wen.com/{page}','d':'nifty-wen.com','r':null,'w':{Screen.width}}}".Replace('\'', '"');
	}

	private static string GetCustomEvent(string name, Dictionary<string, string> details)
	{
		string detailStr = "{";
		foreach (var kv in details)
		{
			detailStr += $"'{kv.Key}':'{kv.Value.Replace("'", "").Replace(",", "").Replace("\"", "")}',".Replace("'", "\\\"");
		}
		detailStr = detailStr.Remove(detailStr.Length - 1) + "}";
		string e = $"{{'n':'{name}','u':'https://nifty-wen.com','d':'nifty-wen.com','r':null,'w':{Screen.width},'p':'{detailStr}'}}".Replace('\'', '"');
		return e;
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}
}
