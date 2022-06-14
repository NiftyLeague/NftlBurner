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
	[Space]
	public GameObject burnButton;
	public GameObject helpButton;
	[Space]
	public ObscuredUInt nftlBalance;
	public ObscuredUInt nftlToBurn;
	[Space]
	public TextMeshProUGUI nftlBalanceText;
	public TextMeshProUGUI errorMessageText;
	public TMP_InputField nftlToBurnInputField;
	public TextMeshProUGUI connectingMessageText;
	[Space]
	public TweenEaseType textTweenType;
	public float textTweenDuration;
	[Space]
	public SimpleAnim burnButtonAnim;
	public Sprite[] burnButtonIdleAnimation;
	public Sprite[] burnButtonPressedAnimation;
	public SimpleAnim burningAnim;
	public Sprite[] burningIdleAnimation;
	public Sprite[] burningBurnAnimation;
	[Space]
	public TextMeshProUGUI leaderboardTitleText;
	public List<TextMeshProUGUI> leaderboardNameText;
	public List<TextMeshProUGUI> leaderboardAmountText;
	[Space]
	public ObscuredBool cantConnect;

	private Coroutine currentErrorMessageCoroutine;
	private ObscuredBool isBurning;
	private ObscuredFloat burnPromptTimer;

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

	void UpdateLeaderboards()
	{
		DisplayLeaderboard();
	}

	public void ChangeCurrentLeaderboard()
	{
		CloseErrorMessage();
		leaderboardType++;
		if (leaderboardType > LeaderboardType.AllTime)
		{
			leaderboardType = LeaderboardType.Weekly;
		}
		DisplayLeaderboard();
		audioManager.PlaySound(AudioManager.SoundID.PressButton);
	}

	void DisplayLeaderboard()
	{
		StartCoroutine(UpdateLeaderboardDisplay());

		//List<ObscuredString> leaderboardNames = new List<ObscuredString>();
		//List<ObscuredUInt> leaderboardAmounts = new List<ObscuredUInt>();

		//switch (leaderboardToShow)
		//{
		//	case 0:
		//		leaderboardTitleText.text = "WEEKLY LEADERBOARD";
		//		leaderboardNames = leaderboardWeeklyNames;
		//		leaderboardAmounts = leaderboardWeeklyAmounts;
		//		break;
		//	case 1:
		//		leaderboardTitleText.text = "MONTHLY LEADERBOARD";
		//		leaderboardNames = leaderboardMonthlyNames;
		//		leaderboardAmounts = leaderboardMonthlyAmounts;
		//		break;
		//	case 2:
		//		leaderboardTitleText.text = "ALLTIME LEADERBOARD";
		//		leaderboardNames = leaderboardAlltimeNames;
		//		leaderboardAmounts = leaderboardAlltimeAmounts;
		//		break;
		//}

		//for (int i = 0; i < 10; i++)
		//{
		//	string nextName = leaderboardNames[i];
		//	leaderboardNameText[i].text = nextName.ToUpper();
		//	leaderboardAmountText[i].text = leaderboardAmounts[i].ToString("n0");
		//}
	}

	private IEnumerator UpdateLeaderboardDisplay()
	{
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
	}

	IEnumerator FetchLeaderboardData(LeaderboardType type)
	{
		leaderboardRows = new OrderedDictionary(10);
		UnityWebRequest www = null;
		yield return Utils.GetRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/scores?count=10&game=wen_game&score_type=score", (w) => www = w);
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
		if (isBurning)
		{
			ErrorMessage("You're already burning some tokens!");
			return;
		}

		if (nftlToBurn <= 0)
		{
			ErrorMessage("Must input more than 0 NFTL to burn!");
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
		audioManager.PlaySound(AudioManager.SoundID.hoverOverButton);
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

		audioManager.PlaySound(AudioManager.SoundID.hoverOverButton);
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

		burningAnim.Play(burningBurnAnimation, false);

		yield return new WaitForSeconds(14.7f);

		burningAnim.Play(burningIdleAnimation, false);

		yield return SubmitTokenBurn();

		yield return Launcher.I.RefreshNFTLBalance();

		UpdateLeaderboards();

		isBurning = false;

		ChangeBurnButtonState(false);
	}

	void ErrorMessage(string message)
	{
		CloseErrorMessage();

		errorMessageText.text = message;

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
		byte[] data = Encoding.ASCII.GetBytes(@"{
			'id': 'token-burn',
			'currency': 'nftl',
			'price': nftlToBurn
		}".Replace('\'', '"'));
		yield return Utils.PostRequest("www.google.com", data, (w) => www = w, headers);

		if (www.result != UnityWebRequest.Result.Success)
		{
			print("Failed to fetch inventory");
			yield break;
		}
		else
		{
			print(www.downloadHandler.text);
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
	Weekly = 0,
	Monthly = 1,
	AllTime = 2
}

public class LeaderboardRow
{
	public string userId;
	public string username;
	public int score;
	public string avatar;
}
