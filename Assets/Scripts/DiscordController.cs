using System;
using System.Collections;
using UnityEngine;

public class DiscordController : MonoBehaviour
{
	private static DiscordController I = null;
	private static Discord.Discord discord;

	void Awake()
	{
		I = this;
		try
		{
			discord = new Discord.Discord(982801162648563723, (UInt64)Discord.CreateFlags.NoRequireDiscord);
		}
		catch { }
	}

	void Update()
	{
		if (discord != null)
		{
			discord.RunCallbacks();
		}
	}

	public static void UpdatePresence(string state)
	{
		if (discord == null || I == null)
		{
			return;
		}
		I.StartCoroutine(_UpdatePresence(state));
	}

	private static IEnumerator _UpdatePresence(string state)
	{
		if (discord == null)
		{
			yield break;
		}
		var am = discord.GetActivityManager();
		var activity = new Discord.Activity
		{
			//State = state,
			//Type = Discord.ActivityType.Playing
		};
		am.UpdateActivity(activity, (res) => { });
		yield return new WaitForSecondsRealtime(0.5f);
		am.ClearActivity((res) => { });
	}

	private void OnApplicationQuit()
	{
		if (discord == null)
		{
			return;
		}
		discord.GetActivityManager().ClearActivity((res) => { });
	}

	private void OnDisable()
	{
		if (discord == null)
		{
			return;
		}
		discord.GetActivityManager().ClearActivity((res) => { });
	}
}
