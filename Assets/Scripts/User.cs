using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class NiftyUsers
{
	public static NiftyUser me { get; private set; }

	public static NiftyUser Login(string verification)
	{
		me = new NiftyUser(verification);
		return me;
	}

	public static string GetMyAuthorization()
	{
		string token = me.authorization;
#if USE_TEST_TOKEN
		string[] toks = {
			"gAAAAABhtRWrCxVg0pI0el5V-_wDPVHzZ2G-z2ecuKH7HL9Delxi0E1DTs-2ZYdMnvJJCmSRKZ9h1Ww1KwcNFRHELJccZU19rBS-BvTXI9XTg5E7FD6qKwWWAb_9YyGtYmWJKR0fZf99d7UciWtgNaCKgUJJoV8-Ctn8n6QaYnBATb5uusGCGMuGBuXEEbMbsFb5Q4EEykhjXkEmmGkoPUQ8aqO6WlSzoFpzH7OAynJbakZMyu1gbqbFAhzKRP06vssAPUQeMmD2ap2jWyj6fpdaLafgGEwivQ==",
			"gAAAAABhswWkWxRya2GpYsnclia737_G9UK2T3pC1X_ZwP-aMnlqfVyyvsq0DH7_8dvIkeeyG95vZZ43Wc4n6VgKZvPACniBLs3lmy_5XCB_s7G2k4slFynjTp4F7gQaquM1IlRl-SclTzGNE8VAHxnKwGLO77e_bwKb0M9xMuuligjYsdpyjo8ScjvfpqEndZNDiUxYT5GNnS_FUv6Wceadwj1zMVA9BDa5sfit8EM6hOZuK8Xqzzuks_jNz12VmqgdOzOsFj4gATnGSMJG8k4efwOXyFFn9g==",
		};
		token = toks[Application.dataPath.Contains("_clone") ? 1: 0];
#endif
		return token;
	}
}

[Serializable]
public class NiftyUser
{
	public string id { get; }
	public string address { get; }
	public ObscuredUInt balance { get; private set; }
	public ObscuredUInt arcadeTokenBalance { get; private set; }

	public Timeout timeout = null;
	public List<int> degens;
	internal string authorization;


	public NiftyUser(string verification)
	{
		address = verification;
		if (verification.Contains(','))
		{
			string[] tokens = verification.Split(',');
			address = tokens[0];
			authorization = tokens[1];
		}
		address = address.ToLower();
		id = address.Replace("0x", "");
		timeout = null;
	}


	public void SetDegens(List<int> degens)
	{
		this.degens = degens;
	}

	public void SetBalances(uint balance, uint arcadeTokenBalance)
	{
		this.balance = balance;
		this.arcadeTokenBalance = arcadeTokenBalance;
	}
}

public class Timeout
{
	public ObscuredInt createdAt;
	public ObscuredInt endAt;
	public ObscuredInt abandonCount;
	public ObscuredInt duration;
	public ObscuredString penalty;
	public ObscuredString description;

	public Timeout(int createdAt, int endAt, int abandonCount, int duration, string penalty, string description)
	{
		this.createdAt = createdAt;
		this.endAt = endAt;
		this.abandonCount = abandonCount;
		this.duration = duration;
		this.penalty = penalty;
		this.description = description;
	}

	public bool IsExpired()
	{
		return XUtils.Timestamp() >= endAt;
	}

	public int GetTimeRemaining()
	{
		return Mathf.Max(0, endAt - (int)XUtils.Timestamp());
	}
}
