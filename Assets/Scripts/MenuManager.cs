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
	public GameObject burnPromptPanel;
	[Space]
	public GameObject burnButton;
	public GameObject helpButton;
	public GameObject connectButton;
	[Space]
	public uint nftlBalance;
	public uint nftlToBurn;
	[Space]
	public TextMeshProUGUI nftlBalanceText;
	public TextMeshProUGUI errorMessageText;
	public TMP_InputField nftlToBurnInputField;
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

	private Coroutine currentErrorMessageCoroutine;
	private bool isBurning;
	private float burnPromptTimer;

    private void Start()
    {
		UpdateNFTLAmountTexts();
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

    public void ConnectWalletButton()
	{
		Debug.Log("Connecting and Logging in Wallet...");
		audioManager.PlaySound(AudioManager.SoundID.PressButton);
		nftlBalance = 1000000;
		UpdateNFTLAmountTexts();
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
		connectButton.SetActive(!pressed);
		nftlToBurnInputField.interactable = !pressed;
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
		if (currentErrorMessageCoroutine != null)
		{
			StopCoroutine(currentErrorMessageCoroutine);
		}

		errorMessageText.text = message;

		currentErrorMessageCoroutine = StartCoroutine(ErrorMessagePlay());

		audioManager.PlaySound(AudioManager.SoundID.ErrorMessage);
	}

	IEnumerator ErrorMessagePlay()
	{
		errorMessageText.color = new Color32(255, 0, 0, 255);

		yield return new WaitForSeconds(5);

		Tween<float> scaleTween = new Tween<float>(255f, 0f, textTweenDuration, textTweenType);
		while (!scaleTween.IsEnded())
		{
			yield return new WaitForEndOfFrame();
			errorMessageText.color = new Color32(255, 0, 0, (byte)scaleTween.Update(Time.deltaTime));
		}
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
