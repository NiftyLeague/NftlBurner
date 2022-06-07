using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class WebRequestHelper
{
	public static IEnumerator PostJsonRequest(string url, string body, bool isAuthenticated = false, bool isEncrypted = false, Action<string> onComplete = null)
	{
		UnityWebRequest www = null;
		Dictionary<string, string> headers = new Dictionary<string, string>();
		if (isAuthenticated)
		{
			headers.Add("authorizationToken", NiftyUsers.GetMyAuthorization());
		}

		if (isEncrypted)
		{
			headers.Add("NL-Encryption", "1.1");
			body = XUtils.ToXorBase64(body, NiftyUsers.GetMyAuthorization());
			yield return Utils.PostRequest(url, Encoding.ASCII.GetBytes(body), (w) => www = w, headers);
		}
		else
		{
			yield return Utils.PostJsonRequest(url, body, (w) => www = w, headers);
		}

		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(www.error);
			if (onComplete != null)
			{
				onComplete(null);
			}
			yield break;
		}

		if (onComplete != null)
		{
			onComplete(www.downloadHandler.text);
		}
	}


	public static IEnumerator PostRequest(string url, string body, bool isAuthenticated = false, bool isEncrypted = false, Action<string> onComplete = null)
	{
		UnityWebRequest www = null;
		Dictionary<string, string> headers = new Dictionary<string, string>();
		if (isAuthenticated)
		{
			headers.Add("authorizationToken", NiftyUsers.GetMyAuthorization());
		}

		if (isEncrypted)
		{
			headers.Add("NL-Encryption", "1.1");
			body = XUtils.ToXorBase64(body, NiftyUsers.GetMyAuthorization());
		}

		yield return Utils.PostRequest(url, Encoding.ASCII.GetBytes(body), (w) => www = w, headers);
		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(www.error);
			if (onComplete != null)
			{
				onComplete(null);
			}
			yield break;
		}

		if (onComplete != null)
		{
			onComplete(www.downloadHandler.text);
		}
	}


	public static IEnumerator GetRequest(string url, string queryParams, bool isAuthenticated = false, bool isEncrypted = false, Action<string> onComplete = null)
	{
		UnityWebRequest www = null;
		Dictionary<string, string> headers = new Dictionary<string, string>();
		if (isAuthenticated)
		{
			headers.Add("authorizationToken", NiftyUsers.GetMyAuthorization());
		}

		if (isEncrypted)
		{
			headers.Add("NL-Encryption", "1.1");
			queryParams = "params=" + XUtils.ToXorBase64(queryParams, NiftyUsers.GetMyAuthorization());
		}

		yield return Utils.GetRequest($"{url}?{queryParams}", (w) => www = w, headers);
		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(www.error);
			if (onComplete != null)
			{
				onComplete(null);
			}
			yield break;
		}

		if (onComplete != null)
		{
			onComplete(www.downloadHandler.text);
		}
	}

	public static IEnumerator DownloadFile(string url, Action<byte[]> onComplete = null)
	{
		string cachePath = Path.Combine(Application.persistentDataPath, "cache", Utils.GetMD5Hash(url).ToString());
		bool cacheExists = File.Exists(cachePath);
		bool cacheExpired = cacheExists && DateTime.UtcNow.Subtract(File.GetLastWriteTimeUtc(cachePath)).TotalDays > 30;
		if (cacheExists && !cacheExpired)
		{
			Debug.Log($"Using cache for {url}");
			Debug.Log(cachePath);
			if (onComplete != null)
			{
				onComplete(File.ReadAllBytes(cachePath));
			}
			yield break;
		}
		if (cacheExpired)
		{
			File.Delete(cachePath);
		}

		UnityWebRequest www = UnityWebRequest.Get(url);
		yield return www.SendWebRequest();
		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(www.error + " " + url);
			if (onComplete != null)
			{
				onComplete(null);
			}
			yield break;
		}
		Debug.Log(www.downloadHandler.data);
		if (www.downloadedBytes < 1)
		{
			Debug.Log($"Empty data array {url}");
			yield break;
		}

		File.WriteAllBytes(cachePath, www.downloadHandler.data);

		if (onComplete != null)
		{
			onComplete(www.downloadHandler.data);
		}
	}

	public static byte[] GetDownloadedFile(string url)
	{
		string cachePath = Path.Combine(Application.persistentDataPath, "cache", Utils.GetMD5Hash(url).ToString());
		bool cacheExists = File.Exists(cachePath);
		if (cacheExists)
		{
			return File.ReadAllBytes(cachePath);
		}
		return null;
	}
}
