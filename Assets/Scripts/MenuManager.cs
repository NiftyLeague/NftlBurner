using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
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
	int leaderboardToShow;

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
		leaderboardAlltimeNames = new List<ObscuredString>();
		leaderboardAlltimeAmounts = new List<ObscuredUInt>();

		//TEST LIST DELETE LATER
		leaderboardAlltimeNames.Add("matt higgins");
		leaderboardAlltimeNames.Add("gary vee");
		leaderboardAlltimeNames.Add("big jon");
		leaderboardAlltimeNames.Add("seiya");
		leaderboardAlltimeNames.Add("0x94894385345");
		leaderboardAlltimeNames.Add("max.eth");
		leaderboardAlltimeNames.Add("nifty chap");
		leaderboardAlltimeNames.Add("tessa");
		leaderboardAlltimeNames.Add("coolboi");
		leaderboardAlltimeNames.Add("bagz");

		leaderboardAlltimeAmounts.Add(2000009);
		leaderboardAlltimeAmounts.Add(2000000);
		leaderboardAlltimeAmounts.Add(500040);
		leaderboardAlltimeAmounts.Add(80000);
		leaderboardAlltimeAmounts.Add(75345);
		leaderboardAlltimeAmounts.Add(70000);
		leaderboardAlltimeAmounts.Add(69400);
		leaderboardAlltimeAmounts.Add(62000);
		leaderboardAlltimeAmounts.Add(500);
		leaderboardAlltimeAmounts.Add(8);

		leaderboardMonthlyNames = new List<ObscuredString>();
		leaderboardMonthlyAmounts = new List<ObscuredUInt>();

		//TEST LIST DELETE LATER
		leaderboardMonthlyNames.Add("timmy tims");
		leaderboardMonthlyNames.Add("reginald");
		leaderboardMonthlyNames.Add("snow");
		leaderboardMonthlyNames.Add("bootyman");
		leaderboardMonthlyNames.Add("robot94993");
		leaderboardMonthlyNames.Add("other tim");
		leaderboardMonthlyNames.Add("i love nifty stuff");
		leaderboardMonthlyNames.Add("life is good");
		leaderboardMonthlyNames.Add("bigbrainz");
		leaderboardMonthlyNames.Add("diamondhands");

		leaderboardMonthlyAmounts.Add(200009);
		leaderboardMonthlyAmounts.Add(200000);
		leaderboardMonthlyAmounts.Add(50400);
		leaderboardMonthlyAmounts.Add(8000);
		leaderboardMonthlyAmounts.Add(745);
		leaderboardMonthlyAmounts.Add(700);
		leaderboardMonthlyAmounts.Add(694);
		leaderboardMonthlyAmounts.Add(620);
		leaderboardMonthlyAmounts.Add(50);
		leaderboardMonthlyAmounts.Add(5);

		leaderboardWeeklyNames = new List<ObscuredString>();
		leaderboardWeeklyAmounts = new List<ObscuredUInt>();

		//TEST LIST DELETE LATER
		leaderboardWeeklyNames.Add("george clooney");
		leaderboardWeeklyNames.Add("brad pitt");
		leaderboardWeeklyNames.Add("matt damon");
		leaderboardWeeklyNames.Add("don cheadle");
		leaderboardWeeklyNames.Add("elliot gould");
		leaderboardWeeklyNames.Add("casey affleck");
		leaderboardWeeklyNames.Add("eddie jemison");
		leaderboardWeeklyNames.Add("shaobo qin");
		leaderboardWeeklyNames.Add("carl reiner");
		leaderboardWeeklyNames.Add("bernie mac");

		leaderboardWeeklyAmounts.Add(20090);
		leaderboardWeeklyAmounts.Add(20000);
		leaderboardWeeklyAmounts.Add(5400);
		leaderboardWeeklyAmounts.Add(800);
		leaderboardWeeklyAmounts.Add(73);
		leaderboardWeeklyAmounts.Add(70);
		leaderboardWeeklyAmounts.Add(69);
		leaderboardWeeklyAmounts.Add(62);
		leaderboardWeeklyAmounts.Add(5);
		leaderboardWeeklyAmounts.Add(2);

		DisplayLeaderboard();
	}

	public void ChangeCurrentLeaderboard()
	{
		CloseErrorMessage();
		leaderboardToShow++;
		if (leaderboardToShow >= 3)
		{
			leaderboardToShow = 0;
		}
		DisplayLeaderboard();
		audioManager.PlaySound(AudioManager.SoundID.PressButton);
	}

	void DisplayLeaderboard()
	{
		List<ObscuredString> leaderboardNames = new List<ObscuredString>();
		List<ObscuredUInt> leaderboardAmounts = new List<ObscuredUInt>();

		switch (leaderboardToShow)
		{
			case 0:
				leaderboardTitleText.text = "WEEKLY LEADERBOARD";
				leaderboardNames = leaderboardWeeklyNames;
				leaderboardAmounts = leaderboardWeeklyAmounts;
				break;
			case 1:
				leaderboardTitleText.text = "MONTHLY LEADERBOARD";
				leaderboardNames = leaderboardMonthlyNames;
				leaderboardAmounts = leaderboardMonthlyAmounts;
				break;
			case 2:
				leaderboardTitleText.text = "ALLTIME LEADERBOARD";
				leaderboardNames = leaderboardAlltimeNames;
				leaderboardAmounts = leaderboardAlltimeAmounts;
				break;
		}

		for (int i = 0; i < 10; i++)
		{
			string nextName = leaderboardNames[i];
			leaderboardNameText[i].text = nextName.ToUpper();
			leaderboardAmountText[i].text = leaderboardAmounts[i].ToString("n0");
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
