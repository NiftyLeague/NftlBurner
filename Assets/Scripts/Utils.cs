using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;


public static class Utils
{
	public static GameObject CreateGameObject(GameObject parent, string name)
	{
		return CreateGameObject(parent, name, Vector3.zero, Quaternion.identity);
	}

	public static GameObject CreateGameObject(GameObject parent, string name, Vector3 position, Quaternion rotation)
	{
		var go = new GameObject();
		if (parent)
		{
			go.transform.SetParent(parent.transform);
		}
		go.transform.localPosition = position;
		go.transform.localRotation = rotation;
		go.name = name;
		return go;
	}

	public static string UnderscoreToCamelCase(string name)
	{
		string[] array = name.Split('_', '-', ' ');
		for (int i = 0; i < array.Length; i++)
		{
			string s = array[i];
			string first = string.Empty;
			string rest = string.Empty;
			if (s.Length > 0)
			{
				first = Char.ToUpperInvariant(s[0]).ToString();
			}
			if (s.Length > 1)
			{
				rest = s.Substring(1).ToLowerInvariant();
			}
			array[i] = first + rest;
		}
		return string.Join("", array);
	}

	public static string ToFirstLetterUppercase(string value)
	{
		char[] array = value.ToCharArray();
		if (array.Length >= 1)
		{
			if (char.IsLower(array[0]))
			{
				array[0] = char.ToUpper(array[0]);
			}
		}
		for (int i = 1; i < array.Length; i++)
		{
			if (array[i - 1] == ' ')
			{
				if (char.IsLower(array[i]))
				{
					array[i] = char.ToUpper(array[i]);
				}
			}
		}
		return new string(array);
	}

	public static string ToSpacedCamelCase(string camelCase)
	{
		return Regex.Replace(camelCase, "([A-Z])", " $1").Trim();
	}

	public static string GetMD5Hash(string input)
	{
		StringBuilder hash = new StringBuilder();
		MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
		byte[] bytes = md5provider.ComputeHash(Encoding.ASCII.GetBytes(input));
		for (int i = 0; i < bytes.Length; i++)
		{
			hash.Append(bytes[i].ToString("x2"));
		}
		return hash.ToString().ToLower();
	}

	public static string GetSHA1Hash(string input)
	{
		StringBuilder hash = new StringBuilder();
		SHA1CryptoServiceProvider provider = new SHA1CryptoServiceProvider();
		byte[] bytes = provider.ComputeHash(Encoding.ASCII.GetBytes(input));
		for (int i = 0; i < bytes.Length; i++)
		{
			hash.Append(bytes[i].ToString("x2"));
		}
		return hash.ToString().ToLower();
	}

	public static bool IsHexString(string value)
	{
		return Regex.IsMatch(value, @"\A\b(0[xX])?[0-9a-fA-F]+\b\Z");
	}

	public static IEnumerator PostJsonRequest(string url, string json, Action<UnityWebRequest> callback = null, Dictionary<string, string> headers = null)
	{
		UnityWebRequest www = new UnityWebRequest(url, "POST");
		www.timeout = 6;
		www.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
		www.downloadHandler = new DownloadHandlerBuffer();
		www.SetRequestHeader("Content-Type", "application/json");
		if (headers != null)
		{
			foreach (var kv in headers)
			{
				www.SetRequestHeader(kv.Key, kv.Value);
			}
		}
		yield return www.SendWebRequest();
		www.uploadHandler.Dispose();
		if (callback != null)
		{
			callback.Invoke(www);
		}
	}

	public static IEnumerator PostRequest(string url, byte[] body, Action<UnityWebRequest> callback = null, Dictionary<string, string> headers = null)
	{
		UnityWebRequest www = new UnityWebRequest(url, "POST");
		www.timeout = 6;
		www.uploadHandler = new UploadHandlerRaw(body);
		www.downloadHandler = new DownloadHandlerBuffer();
		www.SetRequestHeader("Content-Type", "application/octet-stream");
		if (headers != null)
		{
			foreach (var kv in headers)
			{
				www.SetRequestHeader(kv.Key, kv.Value);
			}
		}
		yield return www.SendWebRequest();
		www.uploadHandler.Dispose();
		if (callback != null)
		{
			callback.Invoke(www);
		}
	}

	public static IEnumerator OptionsRequest(string url, Action<UnityWebRequest> callback = null, Dictionary<string, string> headers = null)
	{
		UnityWebRequest www = new UnityWebRequest(url, "OPTIONS");
		www.timeout = 6;
		www.downloadHandler = new DownloadHandlerBuffer();
		if (headers != null)
		{
			foreach (var kv in headers)
			{
				www.SetRequestHeader(kv.Key, kv.Value);
			}
		}
		yield return www.SendWebRequest();
		if (callback != null)
		{
			callback.Invoke(www);
		}
	}

	public static IEnumerator GetRequest(string url, Action<UnityWebRequest> callback = null, Dictionary<string, string> headers = null)
	{
		UnityWebRequest www = new UnityWebRequest(url, "GET");
		www.timeout = 6;
		www.downloadHandler = new DownloadHandlerBuffer();
		if (headers != null)
		{
			foreach (var kv in headers)
			{
				www.SetRequestHeader(kv.Key, kv.Value);
			}
		}
		yield return www.SendWebRequest();
		if (callback != null)
		{
			callback.Invoke(www);
		}
	}
}

