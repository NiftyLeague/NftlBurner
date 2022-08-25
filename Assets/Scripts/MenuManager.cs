using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine.Networking;

public class MenuManager : Singleton<MenuManager>
{
	public AudioManager audioManager;
	[Space]
	public GameObject helpPanel;
	public GameObject leaderboardPanel;
	public GameObject burnPromptPanel;
	public GameObject errorMessagePanel;
	public GameObject connectingPanel;
	public GameObject leaderboardLoadingPanel;
	[Space]
	public GameObject burnButton;
	public GameObject closedBurnButton;
	public GameObject helpButton;
	[Space]
	public ObscuredUInt nftlBalance;
	public ObscuredUInt nftlToBurn;
	[Space]
	public TextMeshProUGUI burnConfirmationText;
	public TextMeshProUGUI nftlBalanceText;
	public TextMeshProUGUI errorMessageText;
	public TMP_InputField nftlToBurnInputField;
	public TextMeshProUGUI connectingMessageText;
	[Space]
	public TweenEaseType textTweenType;
	public float textTweenDuration;
	[Space]
	public Image burnButtonImage;
	public SimpleAnim burnButtonAnim;
	public Sprite burnButtonDefaultSprite;
	public Sprite burnButtonClosedSprite;
	public Sprite[] burnButtonIdleAnimation;
	public Sprite[] burnButtonPressedAnimation;
	public SimpleAnim burningAnim;
	public Sprite[] burningIdleAnimation;
	public Sprite[] burningBurnAnimation;
	public Sprite[] closedVolcanoIdleAnimation;
	[Space]
	public TextMeshProUGUI leaderboardTitleText;
	public List<TextMeshProUGUI> leaderboardNameText;
	public List<TextMeshProUGUI> leaderboardAmountText;
	[Space]
	public ObscuredBool cantConnect;
	[Space]
	public AudioSource jungleAudioSource;
	public float jungleVolumeMax;
	public float jungleVolume;
	[Space]
	public AudioSource volcanoAudioSource;
	[Space]
	public ObscuredBool turnOffVolcano;

	private Coroutine currentErrorMessageCoroutine;
	private ObscuredBool isBurning;
	private ObscuredFloat burnPromptTimer;
	private ObscuredString burnTokenId;

	List<ObscuredString> leaderboardWeeklyNames;
	List<ObscuredUInt> leaderboardWeeklyAmounts;
	List<ObscuredString> leaderboardMonthlyNames;
	List<ObscuredUInt> leaderboardMonthlyAmounts;
	List<ObscuredString> leaderboardAlltimeNames;
	List<ObscuredUInt> leaderboardAlltimeAmounts;
	LeaderboardType leaderboardType;
	private static OrderedDictionary leaderboardRows;

	private void Start()
	{
		connectingPanel.SetActive(true);
		leaderboardPanel.SetActive(false);
		InvokeRepeating(nameof(ChangeJungleVolume), 0f, 5f);
		leaderboardType = LeaderboardType.AllTime;
		burnTokenId = "";

		if (turnOffVolcano)
		{
			burnButtonImage.sprite = burnButtonClosedSprite;
			burnButton.SetActive(false);
			volcanoAudioSource.Stop();
			burningAnim.Play(closedVolcanoIdleAnimation, false);
		}

		closedBurnButton.SetActive(turnOffVolcano);
	}

	private void Update()
	{
		if (burnPromptTimer > 0)
		{
			burnPromptTimer -= Time.deltaTime;
			if (burnPromptTimer <= 0)
			{
				CloseBurnPrompt();
			}
		}
		jungleAudioSource.volume = Mathf.Lerp(jungleAudioSource.volume, jungleVolume, Time.deltaTime);
	}

	private void ChangeJungleVolume()
	{
		jungleVolume = XRandom.NextFloat(jungleVolumeMax);
	}

	public void SetStatus(string statusMessage)
	{
		connectingMessageText.text = statusMessage;
	}

	public void UpdateNFTLBalance(ObscuredUInt amount)
	{
		nftlBalance = amount;
		UpdateNFTLAmountTexts();
	}

	public void Initialize()
	{
		ConnectionSuccessful();
	}

	IEnumerator ConnectWallet()
	{
		connectingPanel.SetActive(true);
		leaderboardPanel.SetActive(false);

		yield return new WaitForSeconds(2);

		if (cantConnect)
		{
			ConnectionFailed();
		}
		else
		{
			ConnectionSuccessful();
		}
	}

	void ConnectionSuccessful()
	{
		connectingPanel.SetActive(false);
		leaderboardPanel.SetActive(true);
		UpdateLeaderboards();
	}

	void ConnectionFailed()
	{
		connectingMessageText.text = "Cannot Connect. Please Check Internet Connection and Try Again.";
		connectingMessageText.color = Color.red;
	}

	public void LeaderboardTwitterButton()
	{
		Debug.Log("Visiting Twitter...");
		CloseErrorMessage();
		audioManager.PlaySound(AudioManager.SoundID.PressButton);
	}

	public void LeaderboardDownloadButton()
	{
		Debug.Log("Downloading...");
		CloseErrorMessage();
		audioManager.PlaySound(AudioManager.SoundID.PressButton);
	}

	public void ChangeCurrentLeaderboard()
	{
		CloseErrorMessage();
		leaderboardType++;
		if (leaderboardType > LeaderboardType.Monthly)
		{
			leaderboardType = LeaderboardType.AllTime;
		}
		UpdateLeaderboards();
		audioManager.PlaySound(AudioManager.SoundID.PressButton);
	}

	void UpdateLeaderboards()
	{
		StartCoroutine(UpdateLeaderboardDisplay());
	}

	private IEnumerator UpdateLeaderboardDisplay()
	{
		leaderboardLoadingPanel.SetActive(true);

		for (int i = 0; i < 10; i++)
		{
			leaderboardNameText[i].text = "";
			leaderboardAmountText[i].text = "";
		}

		switch (leaderboardType)
		{
		//case LeaderboardType.Weekly:
		//	leaderboardTitleText.text = "WEEKLY LEADERBOARD";
		//	break;
		case LeaderboardType.Monthly:
			leaderboardTitleText.text = "MONTHLY LEADERBOARD";
			break;
		case LeaderboardType.AllTime:
			leaderboardTitleText.text = "ALL TIME LEADERBOARD";
			break;
		}

		// display loading
		yield return FetchLeaderboardData(leaderboardType);
		int count = 0;
		foreach (var row in leaderboardRows.Values.Cast<LeaderboardRow>())
		{
			leaderboardNameText[count].text = $"{row.username}\n";
			leaderboardAmountText[count].text = row.score.ToString("n0") + "\n";
			count++;
		}

		for (int i = count; i < 10; i++)
		{
			leaderboardNameText[i].text = "---\n";
			leaderboardAmountText[i].text = "---\n";
		}

		leaderboardLoadingPanel.SetActive(false);
	}

	IEnumerator FetchLeaderboardData(LeaderboardType type)
	{
		leaderboardRows = new OrderedDictionary(10);
		UnityWebRequest www = null;
		string lbType = "all_time";
		switch (type)
		{

		case LeaderboardType.Monthly:
			lbType = "monthly";
			break;
		case LeaderboardType.AllTime:
			lbType = "all_time";
			break;
		}
		yield return Utils.GetRequest($"https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/scores?count=10&game=nftl_burner&score_type=score&time_window={lbType}", (w) => www = w);
		if (www.result != UnityWebRequest.Result.Success)
		{
			print("Failed to fetch leaderboard data");
			yield break;
		}
		else
		{
			try
			{
				JObject response = JObject.Parse(www.downloadHandler.text);
				foreach (JObject row in response["data"])
				{
					var lbRow = new LeaderboardRow();
					lbRow.userId = (string)row["user_id"];
					lbRow.score = (int)(float)row["score"];
					leaderboardRows.Add(lbRow.userId, lbRow);
				}
			}
			catch
			{
				Debug.Log("Failed to update Arcade Token balance");
			}
		}

		string ids = string.Join(",", leaderboardRows.Keys.Cast<string>());
		www = null;
		yield return Utils.GetRequest($"https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/profiles/public/profiles?ids={ids}", (w) => www = w);
		if (www.result != UnityWebRequest.Result.Success)
		{
			print("Failed to fetch profile names");
			yield break;
		}
		else
		{
			try
			{
				JObject profiles = JObject.Parse(www.downloadHandler.text);
				foreach (var profile in profiles)
				{
					LeaderboardRow lbRow = leaderboardRows[profile.Key] as LeaderboardRow;
					lbRow.username = (string)profile.Value["name_cased"];
					if (profile.Value["avatar"] != null && profile.Value["avatar"]["id"] != null)
					{
						lbRow.avatar = (string)profile.Value["avatar"]["id"];
					}
				}
			}
			catch
			{
				Debug.Log("Failed to update Arcade Token balance");
			}
		}
	}

	public void BurnButton()
	{
		if (turnOffVolcano)
		{
			ErrorMessage("The volcano is not currently active.");
			return;
		}

		Analytics.SendPlayerEvent("BurnClicked", new Dictionary<string, string>() { { "Amount", nftlToBurn.ToString() } });
		if (isBurning)
		{
			ErrorMessage("You're already burning some tokens!");
			return;
		}

		if (nftlToBurn < 100)
		{
			ErrorMessage("MINIMUM BURN AMOUNT IS 100 NFTL!");
			return;
		}

		CloseErrorMessage();

		if (burnPromptPanel.activeInHierarchy)
		{
			CloseBurnPrompt();
			StartCoroutine(BurnTokens());
			audioManager.PlaySound(AudioManager.SoundID.PressButton);
			return;
		}

		burnPromptPanel.SetActive(true);
		burnConfirmationText.text = $"ARE YOU SURE YOU WANT TO BURN {nftlToBurn:n0} NFTL?\n\nPRESS BURN AGAIN TO CONFIRM".ToUpper();
		burnPromptTimer = 10;
		audioManager.PlaySound(AudioManager.SoundID.PressButton);
	}

	public void CloseBurnPrompt()
	{
		burnPromptPanel.SetActive(false);
	}

	public void HoverOverHelp()
	{
		helpPanel.SetActive(true);
		CloseErrorMessage();
	}

	public void ExitHelp()
	{
		helpPanel.SetActive(false);
	}

	public void HoverOverButton()
	{
		audioManager.PlaySound(AudioManager.SoundID.HoverOverButton);
	}

	public void SetToBurnAmount(string amount)
	{
		uint newAmount = 0;

		amount = amount.Replace(",", "");

		if (amount != "")
		{
			newAmount = uint.Parse(amount);
		}

		if (newAmount > nftlBalance)
		{
			newAmount = nftlBalance;
			ErrorMessage("You can only burn up to the amount you own!");
		}

		nftlToBurn = newAmount;

		UpdateNFTLAmountTexts();

		audioManager.PlaySound(AudioManager.SoundID.HoverOverButton);
	}

	void ChangeBurnButtonState(bool pressed)
	{
		if (pressed)
		{
			burnButtonAnim.Play(burnButtonPressedAnimation, false);
		}
		else
		{
			burnButtonAnim.Play(burnButtonIdleAnimation, false);
		}

		burnButton.SetActive(!pressed);
		helpButton.SetActive(!pressed);
		nftlToBurnInputField.interactable = !pressed;
		leaderboardPanel.SetActive(!pressed);
	}


	IEnumerator BurnTokens()
	{
		isBurning = true;
		ChangeBurnButtonState(true);

		burnTokenId = "";
		yield return SubmitTokenBurn();
		if (string.IsNullOrEmpty(burnTokenId))
		{
			isBurning = false;
			ChangeBurnButtonState(false);
			yield break;
		}
		Analytics.SendPlayerEvent("BurnStarted", new Dictionary<string, string>() { { "Amount", nftlToBurn.ToString() } });
		yield return Launcher.I.RefreshNFTLBalance();

		burningAnim.Play(burningBurnAnimation, true);

		yield return new WaitUntil(() => burningAnim.GetFrame() >= 7);
		audioManager.PlaySound(AudioManager.SoundID.Whoosh, 0.5f);

		yield return new WaitUntil(() => burningAnim.GetFrame() >= 14);
		audioManager.PlaySound(AudioManager.SoundID.Squeeze, 0.5f);

		yield return new WaitUntil(() => burningAnim.GetFrame() >= 29);
		audioManager.PlaySound(AudioManager.SoundID.Sparkle, 0.7f);

		yield return new WaitUntil(() => burningAnim.GetFrame() >= 46);
		audioManager.PlaySound(AudioManager.SoundID.Eruption, 1f);

		yield return new WaitUntil(() => burningAnim.GetProgress() >= 1f);
		burningAnim.Play(burningIdleAnimation, false);

		UpdateLeaderboards();
		isBurning = false;
		ChangeBurnButtonState(false);
	}

	void ErrorMessage(string message)
	{
		CloseErrorMessage();
		burnPromptPanel.SetActive(false);
		errorMessageText.text = message.ToUpper();
		currentErrorMessageCoroutine = StartCoroutine(ErrorMessagePlay());
		audioManager.PlaySound(AudioManager.SoundID.ErrorMessage);
	}

	void CloseErrorMessage()
	{
		errorMessagePanel.SetActive(false);
		if (currentErrorMessageCoroutine != null)
		{
			StopCoroutine(currentErrorMessageCoroutine);
		}
	}

	IEnumerator ErrorMessagePlay()
	{
		errorMessagePanel.SetActive(true);
		yield return new WaitForSeconds(5);
		errorMessagePanel.SetActive(false);
	}

	public IEnumerator SubmitTokenBurn()
	{
		UnityWebRequest www = null;
		Dictionary<string, string> headers = new Dictionary<string, string>
		{
			{ "authorizationToken", NiftyUsers.GetMyAuthorization() },
		};
		string data = $"{{ 'amount': {nftlToBurn} }}".Replace('\'', '"');
		yield return Utils.PostRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/matches/nftl-burner/burn", Encoding.ASCII.GetBytes(data), (w) => www = w, headers);

		if (www.result != UnityWebRequest.Result.Success)
		{
			if (www.responseCode == 400)
			{
				ErrorMessage(www.downloadHandler.text);
			}
			else
			{
				print(www.error);
				ErrorMessage("Failed to Burn!");
			}
			yield break;
		}
		else
		{
			print(www.downloadHandler.text);
			if (www.downloadHandler.text.Contains("amount") && www.downloadHandler.text.Contains(nftlToBurn.ToString()))
			{
				burnTokenId = www.downloadHandler.text;
			}
		}
	}

	void UpdateNFTLAmountTexts()
	{
		nftlBalanceText.text = nftlBalance.ToString("n0");
		nftlToBurnInputField.text = nftlToBurn.ToString("n0");
	}
}

public enum LeaderboardType
{
	AllTime = 0,
	Monthly = 1
}

public class LeaderboardRow
{
	public string userId;
	public string username;
	public int score;
	public string avatar;
}
