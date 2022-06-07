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
	public uint nftlBalance;
	public uint nftlToBurn;
	[Space]
	public TextMeshProUGUI nftlBalanceText;
	public TextMeshProUGUI nftlToBurnText;

	private bool isBurning;

    private void Start()
    {
		UpdateNFTLAmountTexts();
    }

    public void ConnectWalletButton()
	{
		Debug.Log("Connecting and Logging in Wallet...");

		nftlBalance = 1000000;
		UpdateNFTLAmountTexts();
	}

	public void BurnButton()
	{
		if (isBurning)
		{
			return;
		}

		burnPromptPanel.SetActive(true);
	}

	public void BurnPromptYesButton()
	{
		burnPromptPanel.SetActive(false);
		isBurning = true;
		Debug.Log("BUURRRN BABY BURN");
	}

	public void BurnPromptNoButton()
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

	public void SetToBurnAmount(string amount)
	{
		uint newAmount = uint.Parse(amount);
		if (newAmount > nftlBalance)
		{
			newAmount = nftlBalance;
		}

		nftlToBurn = newAmount;

		UpdateNFTLAmountTexts();
	}

	void UpdateNFTLAmountTexts()
	{
		nftlBalanceText.text = nftlBalance.ToString("n0");
		nftlToBurnText.text = nftlToBurn.ToString("n0");
	}
}
