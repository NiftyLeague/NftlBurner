using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class MenuManager : MonoBehaviour
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
	public uint nftlBalance;
	public uint nftlToBurn;
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
	public List<TextMeshProUGUI> leaderboardNameText;
	public List<TextMeshProUGUI> leaderboardAmountText;
	[Space]
	public bool cantConnect;

	private Coroutine currentErrorMessageCoroutine;
	private bool isBurning;
	private float burnPromptTimer;

	List<string> leaderboardNames;
	List<uint> leaderboardAmounts;

	private void Start()
    {
		StartCoroutine(ConnectWallet());
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

    IEnumerator ConnectWallet()
	{
		connectingMessageText.text = "Connecting To Wallet...";
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
		nftlBalance = 1000000;
		UpdateNFTLAmountTexts();
		UpdateLeaderboard();
	}

	void ConnectionFailed()
	{
		connectingMessageText.text = "Cannot Connect. Please Check Internet Connection and Try Again.";
		connectingMessageText.color = Color.red;
	}

	public void LeaderboardTwitterButton()
	{
		Debug.Log("Visiting Twitter...");
		audioManager.PlaySound(AudioManager.SoundID.PressButton);
	}

	public void LeaderboardDownloadButton()
	{
		Debug.Log("Downloading...");
		audioManager.PlaySound(AudioManager.SoundID.PressButton);
	}

	void UpdateLeaderboard()
	{
		leaderboardNames = new List<string>();
		leaderboardAmounts = new List<uint>();

		//TEST LIST DELETE LATER
		leaderboardNames.Add("matt higgins");
		leaderboardNames.Add("gary vee");
		leaderboardNames.Add("big jon");
		leaderboardNames.Add("seiya");
		leaderboardNames.Add("0x94894385345");
		leaderboardNames.Add("max.eth");
		leaderboardNames.Add("nifty chap");
		leaderboardNames.Add("tessa");
		leaderboardNames.Add("coolboi");
		leaderboardNames.Add("bagz");

		leaderboardAmounts.Add(2000009);
		leaderboardAmounts.Add(2000000);
		leaderboardAmounts.Add(500040);
		leaderboardAmounts.Add(80000);
		leaderboardAmounts.Add(75345);
		leaderboardAmounts.Add(70000);
		leaderboardAmounts.Add(69400);
		leaderboardAmounts.Add(62000);
		leaderboardAmounts.Add(500);
		leaderboardAmounts.Add(2);

		DisplayLeaderboard();
	}

	void DisplayLeaderboard()
	{
		for (int i = 0; i < 10; i++)
		{
			leaderboardNameText[i].text = leaderboardNames[i].ToUpper();
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

		isBurning = false;

		ChangeBurnButtonState(false);

		BurnNFTLTokens();
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
		//errorMessageText.color = new Color32(255, 0, 0, 255);
		errorMessagePanel.SetActive(true);

		yield return new WaitForSeconds(5);

		errorMessagePanel.SetActive(false);

		//Tween<float> scaleTween = new Tween<float>(255f, 0f, textTweenDuration, textTweenType);
		//while (!scaleTween.IsEnded())
		//{
		//	yield return new WaitForEndOfFrame();
		//	errorMessageText.color = new Color32(255, 0, 0, (byte)scaleTween.Update(Time.deltaTime));
		//}
	}

	void BurnNFTLTokens()
	{
		nftlBalance -= nftlToBurn;
		nftlToBurn = 0;
		UpdateNFTLAmountTexts();
	}

	void UpdateNFTLAmountTexts()
	{
		nftlBalanceText.text = nftlBalance.ToString("n0");
		nftlToBurnInputField.text = nftlToBurn.ToString("n0");
	}
}
