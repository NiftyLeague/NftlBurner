using CodeStage.AntiCheat.ObscuredTypes;
using Nethereum.Signer;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;


public class Launcher : MonoBehaviour
{
	public static Launcher I;

	public int verificationRetries;
	public int verificationPollTimeout;

	private State state = State.Begin;
	[SerializeField]
	private NiftyUser user = null;
	private string interimUserAddress = "";
	private string interimAuthToken = "";
	private float startDelay;
	private int verificationTries = 0;
	//private int fetchCharacterTries = 0;
	//private HashSet<int> favoriteDegens = new HashSet<int>();
	private float lastVerificationTokenCheck = 0f;

	internal static string apiNetwork;
	internal static string apiVersion;
	internal static string apiUrl;
	internal static string contractAddress;
	internal static string sessionId;
	internal static string shortSessionId;
	internal static string verificationToken;
	internal static bool ranked;
	//private HashSet<int> degens = new HashSet<int>();


#if UNITY_STANDALONE && !UNITY_EDITOR
	private static bool launchedFromLauncher = false;
#endif

	private void Awake()
	{
		I = this;
#if !UNITY_EDITOR
		//Debug.unityLogger.logHandler = XLogger.GetInstance();
#endif

#if UNITY_STANDALONE && !UNITY_EDITOR
		if (HasArg("-ticket") && IsTicketValid(GetArgValue("-ticket")) || launchedFromLauncher)
		{
			launchedFromLauncher = true;
		}
		else
		{
			Fail("Please use Nifty Launcher to launch");
			return;
		}
#endif
		state = State.Begin;
		Application.runInBackground = true;
		startDelay = XRandom.NextFloat(0f, 0.5f);
		sessionId = Guid.NewGuid().ToString();
		shortSessionId = XUtils.GetInt32HashCode(sessionId).ToString("X6");
		//shortSessionId = shortSessionId.Substring(shortSessionId.Length - 6);
	}

	private void Start()
	{
		MenuManager.Instance.SetStatus("Initializing Configuration");
	}

	private bool IsTicketValid(string ticket)
	{
		return Utils.GetMD5Hash($"{XUtils.Timestamp() / 100}-Salt bae!") == ticket;
	}


	private void Update()
	{
		switch (state)
		{
		case State.Begin:
			if (Time.time > startDelay)
			{
				GetConfiguration();
			}
			break;
		case State.Configuration:
			break;
		case State.Authentication:
		case State.Verification:
			if (Time.realtimeSinceStartup - lastVerificationTokenCheck > verificationPollTimeout)
			{
				StartCoroutine(PollVeficationTokenResponse());
			}
			break;
		case State.Initialization:
			break;
		}
	}

	internal static void Logout()
	{
		ClearCachedAddress();
		UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
	}

	[Beebyte.Obfuscator.SkipRename]
	private void StartAuthentication()
	{
		state = State.Authentication;
		MenuManager.Instance.SetStatus("Authenticating");
#if UNITY_WEBGL || UNITY_EDITOR
		JSWrapper.StartAuthentication(gameObject.name, nameof(OnAuthencationResponse));
#else
		VerifyAccount("0x0");
#endif
	}

	private void VerifyAccount(string address)
	{
		lastVerificationTokenCheck = Time.realtimeSinceStartup + 5f;
		state = State.Verification;
		MenuManager.Instance.SetStatus("Verifying Account");
		string cachedVerification = GetCachedVerification();
		if (!string.IsNullOrEmpty(cachedVerification))
		{
			user = NiftyUsers.Login(cachedVerification);
			InitializeProfile();
		}
		else
		{
#if UNITY_WEBGL
			StartCoroutine(VerifyWebGlAuthTokenAndLogin(address, interimAuthToken));
#else
			MenuManager.Instance.SetStatus("Please Verify your Account on your Browser");
			string hexFormat = "X";
			string sess = sessionId.Replace("-", "");
			string guid3 = $"{Guid.NewGuid()}-{Guid.NewGuid()}-{Guid.NewGuid()}".Replace("-", "");
			verificationToken = $"{sess}-{XUtils.TimestampMilliseconds().ToString(hexFormat)}-{XUtils.GetSimpleDeviceId().ToString(hexFormat)}-{guid3}".ToLower();
			Invoke(nameof(OpenVerificationUrl), 2f);
#endif
		}
	}

	[Beebyte.Obfuscator.SkipRename]
	private void OpenVerificationUrl()
	{
		Application.OpenURL($"https://app.niftyleague.com/verification?token={verificationToken}&nonce={GetNonce()}");
	}

	private IEnumerator VerifyWebGlAuthTokenAndLogin(string address, string authToken)
	{
		string verification = $"{address.ToLower()},{interimAuthToken}";
		SetCachedVerification(verification);

		yield return new WaitForSecondsRealtime(0.1f);
		yield return VerifyAuthToken();
		yield return new WaitForSecondsRealtime(0.1f);
		verification = GetCachedVerification();

		try
		{
			user = NiftyUsers.Login(verification);
		}
		catch { }

		if (user == null)
		{
			VerificationFailed();
			yield break;
		}

		InitializeProfile();
	}

	private IEnumerator VerifyAuthToken()
	{
		string verificationToken = GetCachedVerification();
		if (string.IsNullOrEmpty(verificationToken) || !verificationToken.Contains(','))
		{
			yield break;
		}
		string[] tokens = verificationToken.Split(',');

		UnityWebRequest www = null;
		Dictionary<string, string> headers = new Dictionary<string, string>
		{
			{ "authorizationToken", tokens[1] }
		};

		bool isValid = false;
		int retries = 3;
		while (retries-- > 0)
		{
			yield return new WaitForSecondsRealtime(2 - retries);
			yield return Utils.GetRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/verification/address", (w) => www = w, headers);
			if (www.result != UnityWebRequest.Result.Success)
			{
				Debug.LogWarning(www.error);
				if (www.responseCode == 401 || www.responseCode == 403)
				{
					break;
				}
			}
			else if (www.downloadHandler != null && !string.IsNullOrEmpty(www.downloadHandler.text))
			{
				if (www.downloadHandler.text.Replace("\"", "").ToLower() == tokens[0].ToLower())
				{
					isValid = true;
					break;
				}
				print(www.downloadHandler.text);
			}
		}
		if (!isValid)
		{
			Debug.Log("Clearing cached address");
			ClearCachedAddress();
		}
	}

	private IEnumerator PollVeficationTokenResponse()
	{
		lastVerificationTokenCheck = Time.realtimeSinceStartup;
		if (string.IsNullOrEmpty(verificationToken))
		{
			yield break;
		}

		UnityWebRequest request = UnityWebRequest.Get($"https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/verification?token={verificationToken}");
		yield return request.SendWebRequest();
		try
		{
			JObject response = JObject.Parse(request.downloadHandler.text);
			JObject body = JObject.Parse(response["body"].Value<string>());
			string address = body["address"].Value<string>().ToLower();
			string nonce = body["nonce"].Value<string>();
			string verification = body["verification"].Value<string>();
			int timestamp = body["timestamp"].Value<int>();
			print($"Verification results {address} with nonce {nonce} at {timestamp}");
			interimUserAddress = address;
			if (nonce != GetNonce())
			{
				Debug.LogError($"Verification nonce ({nonce}) does not match current nonce ({GetNonce()})");
				VerificationFailed();
				yield break;
			}
			string auth = body["auth"].Value<string>();
			OnSignatureResponse($"true,{verification},{auth}");
		}
		catch { }
	}

	private string GetNonce()
	{
		return $"0x{sessionId.GetHashCode():X}".ToLower();
	}

	private string GetCachedVerification()
	{
		string cached = PlayerPrefs.GetString("cached", null);
		if (string.IsNullOrEmpty(cached))
		{
			return null;
		}

		string[] tokens;
		try
		{
			string value = XUtils.Decrypt(Convert.FromBase64String(cached), true);
			tokens = value.Split(',');
		}
		catch
		{
			ClearCachedAddress();
			return null;
		}

		if (tokens.Length != 4)
		{
			ClearCachedAddress();
			return null;
		}
#if DEVELOPMENT_BUILD
		int timeout = XUtils.MIN * 15;
#else
		int timeout = XUtils.DAY * 30;
#endif
		int timestamp;
		int.TryParse(tokens[0], out timestamp);
		if (timestamp <= 0 || XUtils.Timestamp() - timestamp > timeout)
		{
			ClearCachedAddress();
			return null;
		}

		long id;
		long.TryParse(tokens[1], out id);
		if (id != XUtils.GetSimpleDeviceId())
		{
			ClearCachedAddress();
			return null;
		}

		if (!tokens[2].StartsWith("0x") || tokens[2].Length < 20)
		{
			ClearCachedAddress();
			return null;
		}
		return $"{tokens[2]},{tokens[3]}";
	}

	public static void ClearCachedAddress()
	{
		PlayerPrefs.DeleteKey("cached");
		PlayerPrefs.Save();
	}

	public static bool HasValidCachedAddress()
	{
		return I.GetCachedVerification() != null;
	}

	private void GetConfiguration()
	{
		state = State.Configuration;
		JSWrapper.GetConfiguration(gameObject.name, nameof(OnGetConfigurationResponse));
	}

	private void SetCachedVerification(string verification)
	{
		string value = $"{XUtils.Timestamp()},{XUtils.GetSimpleDeviceId()},{verification}";
		string cacheValue = Convert.ToBase64String(XUtils.Encrypt(value, true));
		PlayerPrefs.SetString("cached", cacheValue);
		PlayerPrefs.Save();
	}

	[Beebyte.Obfuscator.SkipRename]
	private void OnSignatureResponse(string result)
	{
		user = null;
		try
		{
			string[] tokens = result.Split(',');
			bool success = bool.Parse(tokens[0]);
			if (!success)
			{
				string reason = tokens.Length > 1 ? tokens[1] : "Unknown";
				throw new Exception($"Verification unsuccessful: {reason}");
			}

			EthereumMessageSigner signer = new EthereumMessageSigner();
			var signedMessage = tokens[1];
			var address = signer.EncodeUTF8AndEcRecover(GetVerificationMessage(interimUserAddress), signedMessage);
			print($"Interim: {interimUserAddress}, Verified: {address.ToLower()}");
#if !UNITY_EDITOR
			if (interimUserAddress.ToLower() == address.ToLower())
#endif
			{
#if UNITY_WEBGL
				string verification = interimUserAddress.ToLower();
#else
				string verification = $"{address.ToLower()},{tokens[2]}";
#endif
				user = NiftyUsers.Login(verification);
				SetCachedVerification(verification);
			}
		}
		catch (Exception e)
		{
			Debug.LogError("Verification failed");
			Debug.LogError(e);
		}

		if (user == null)
		{
			VerificationFailed();
			return;
		}

		InitializeProfile();
	}

	[Beebyte.Obfuscator.SkipRename]
	private void InitializeProfile()
	{
		state = State.Initialization;
		MenuManager.Instance.SetStatus("Initializing Profile");

		StartCoroutine(_InitializeProfile(user.address));

		DiscordController.UpdatePresence("");
	}

	private void ProfileDegensReady()
	{
		//user.SetDegens(degens.ToList());
		//PlayerSpriteManager.InitializeAvailableDegens(degens.ToList());
		MenuManager.Instance.Initialize();
		//MenuManager.Instance.SetTokenBalance(user.arcadeTokenBalance);
	}

	[Beebyte.Obfuscator.SkipRename]
	private void OnAuthencationResponse(string result)
	{
		user = null;
		interimUserAddress = null;

		try
		{
			bool success = false;
			string address;
			List<string> characterTraits = new List<string>();

			string[] tokens = result.Split(',');
			success = bool.Parse(tokens[0]);
			if (!success)
			{
				Debug.LogWarning(result);
				throw new Exception($"Authentication unsuccessful");
			}
			address = tokens[1];
			if (address == "0x0")
			{
				Fail("Authentication Failed\n\nPlease connect your wallet and try again");
				return;
			}
			else if (address.Length < 20 || !Utils.IsHexString(address))
			{
				throw new Exception("Account address is not a valid hex string");
			}
			interimUserAddress = address.ToLower();
			interimAuthToken = tokens[3];

			//favoriteDegens = new HashSet<int>();
			if (tokens.Length > 4)
			{
				for (int i = 4; i < tokens.Length; i++)
				{
					if (!string.IsNullOrEmpty(tokens[i]))
					{
						int degenId;
						int.TryParse(tokens[i], out degenId);
						if (degenId > 0)
						{
							//favoriteDegens.Add(degenId);
						}
					}
				}
			}
		}
		catch (Exception e)
		{
			Debug.LogError("Authentication failed");
			Debug.LogError(e);
			AuthenticationFailed();
			return;
		}
		if (interimUserAddress == null)
		{
			AuthenticationFailed();
			return;
		}
		VerifyAccount(interimUserAddress);
	}

	private void AuthenticationFailed()
	{
		Fail("Authentication Failed!\nRetrying...");
		Invoke(nameof(StartAuthentication), 2f);
	}

	private void VerificationFailed()
	{
		verificationTries++;
		if (verificationTries >= verificationRetries)
		{
			Fail("Failed to Verify Account");
		}
		else
		{
			Fail("Failed to Verify Account!\nRetrying...");
			Invoke(nameof(StartAuthentication), 2f);
		}
	}


	private void Fail(string message)
	{
		MenuManager.Instance.SetStatus(message);
		state = State.Failure;
	}

	private string GetShortAddress(string address)
	{
		string id = address.Replace("0x", "").ToLower();
		return $"0x{id.Substring(0, 4)}...{id.Substring(id.Length - 4)}";
	}

	private string GetVerificationMessage(string address)
	{
		return $"Please sign this message to verify that {GetShortAddress(address)} belongs to you. {GetNonce()}";
	}

	[Beebyte.Obfuscator.SkipRename]
	private void OnGetConfigurationResponse(string result)
	{
		try
		{
			string[] tokens = result.ToLower().Split(',');
			apiNetwork = tokens[0].Trim();
			apiVersion = apiVersion = tokens.Length > 1 && !string.IsNullOrEmpty(tokens[1].Trim()) ? tokens[1].Trim() : "0";
			print($"Using Network ({apiNetwork}) version ({apiVersion})");
			apiUrl = $"https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/info?network={apiNetwork}&version={apiVersion}&characters=false";

		}
		catch (Exception e)
		{
			Debug.LogError("Initialization failed");
			Debug.LogError(e);
		}
		if (string.IsNullOrEmpty(apiNetwork) || string.IsNullOrEmpty(apiVersion))
		{
			Fail("Failed to initialize the configuration");
			return;
		}
		StartCoroutine(DownloadConfigInfo());
	}

	private IEnumerator DownloadConfigInfo()
	{
		UnityWebRequest request = UnityWebRequest.Get("https://nifty-league.s3.amazonaws.com/assets/config-wg");
		yield return request.SendWebRequest();
		Dictionary<string, string[]> config = null;
		try
		{
			config = ParseConfig(request.downloadHandler.text);
		}
		catch (Exception)
		{
			Debug.LogWarning("Failed to parse configuration");
		}

		if (config == null || !config.ContainsKey("MinAllowedVersion") || string.Compare(Application.version, config["MinAllowedVersion"][1]) < 0)
		{
			if (config != null && config.ContainsKey("MinAllowedVersion") && config["MinAllowedVersion"].Length > 2)
			{
				Fail(config["MinAllowedVersion"][2]);
			}
			else
			{
				Fail("Failed to Initialize");
			}
			yield break;
		}

		UnityWebRequest www = UnityWebRequest.Get(apiUrl);
		yield return www.SendWebRequest();

		if (www.result != UnityWebRequest.Result.Success)
		{
			Debug.Log(www.error);
			Fail("Failed to download the configuration");
		}
		else
		{
			try
			{
				JObject response = JObject.Parse(www.downloadHandler.text);
				if (response["data"]["id"].Type == JTokenType.String)
				{
					contractAddress = response["data"]["id"].Value<string>();
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning(www.downloadHandler.text);
				Debug.LogError(e);
			}
		}

		if (!string.IsNullOrEmpty(contractAddress) && contractAddress.Length > 20)
		{
			yield return VerifyAuthToken();
			StartAuthentication();
		}
		else
		{
			Fail("Failed to download the configuration");
		}
	}

	private Dictionary<string, string[]> ParseConfig(string configText)
	{
#if DEBUG_BUILD
		/*string s = @"
MinAllowedVersion,0.1.1,Please Upgrade To the Latest Version
";
		print(Convert.ToBase64String(XUtils.Encrypt(s, true)));*/
#endif
		Dictionary<string, string[]> config = new Dictionary<string, string[]>();
		string configValue = XUtils.Decrypt(Convert.FromBase64String(configText), true);
		foreach (string line in configValue.Split('\n'))
		{
			var tokens = line.Split(',');
			if (tokens.Length > 2)
			{
				config.Add(tokens[0], tokens);
			}
		}
		return config;
	}

	private IEnumerator _InitializeProfile(string address)
	{
		UnityWebRequest www = null;
		Dictionary<string, string> headers = new Dictionary<string, string>
		{
			{ "authorizationToken", NiftyUsers.GetMyAuthorization() },
		};
		yield return Utils.PostJsonRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/accounts/session", @$"{{""session"":""{sessionId}""}}", (w) => www = w, headers);
		if (www.result != UnityWebRequest.Result.Success)
		{
			Fail("Failed to obtain session");
			yield break;
		}

		www = null;
		bool isValidSession = false;
		bool isBanned = false;
		uint balance = 0;
		yield return Utils.GetRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/accounts/account", (w) => www = w, headers);
		if (www.result != UnityWebRequest.Result.Success)
		{
			Fail("Failed to fetch account");
			yield break;
		}
		else
		{
			try
			{
				JObject account = JObject.Parse(www.downloadHandler.text);
				isBanned = account["is_banned"] != null && account["is_banned"].Value<bool>();
				isValidSession = account["session_key"].Value<string>() == sessionId;

			}
			catch
			{
				isValidSession = false;
			}
		}

		if (isBanned)
		{
			Fail("You account has been flagged for abuse!\n\nIf you believe this flag was placed incorrectly please contact us");
			yield break;
		}

		if (!isValidSession)
		{
			Fail("Invalid Session\nPlease Try Again");
			yield break;
		}

		user.SetBalance(balance);

		yield return RefreshNFTLBalance();
		//yield return GetDegens();

		ProfileDegensReady();
	}

	public IEnumerator RefreshNFTLBalance()
	{
		UnityWebRequest www = null;
		Dictionary<string, string> headers = new Dictionary<string, string>
		{
			{ "authorizationToken", NiftyUsers.GetMyAuthorization() },
		};
		yield return Utils.GetRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/accounts/account", (w) => www = w, headers);

		uint nftlBalance = 0;
		if (www.result != UnityWebRequest.Result.Success)
		{
			Fail("Failed to fetch inventory");
			yield break;
		}
		else
		{
			try
			{
				JObject inventory = JObject.Parse(www.downloadHandler.text);
				float nftlBalanceFromInventory = inventory["balance"] != null ? (float)inventory["balance"] : 0;
				nftlBalance = (uint)Mathf.FloorToInt(nftlBalanceFromInventory);
				user.SetNFTLBalance(nftlBalance);
			}
			catch
			{
				Debug.Log("Failed to update NFTL Token balance");
			}
		}
	}

	public IEnumerator StartNewMatch()
	{
		UnityWebRequest www = null;
		Dictionary<string, string> headers = new Dictionary<string, string>
		{
			{ "authorizationToken", NiftyUsers.GetMyAuthorization() },
		};
		yield return Utils.GetRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/matches/wen-game/start", (w) => www = w, headers);

		uint arcadeBalance = 0;
		if (www.result != UnityWebRequest.Result.Success)
		{
			Fail("Failed to Insert Token");
			yield break;
		}
		else
		{
			try
			{
				print(www.downloadHandler.text);
				JObject inventory = JObject.Parse(www.downloadHandler.text);
				arcadeBalance = inventory["balance"] != null ? (uint)inventory["balance"] : 0;
				user.SetNFTLBalance(arcadeBalance);
			}
			catch
			{
				Debug.Log("Failed to update Arcade Token balance");
			}
		}
	}


	//private IEnumerator GetDegens()
	//{
	//	string result = null;
	//	yield return WebRequestHelper.GetRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/profiles/profile/avatars",
	//		"", true, false, resp => result = resp);
	//	degens = new HashSet<int>();
	//	try
	//	{
	//		JObject avatars = JObject.Parse(result);
	//		foreach (JObject avatar in avatars["avatars"])
	//		{
	//			try
	//			{
	//				int tokenId = avatar["id"].Value<int>();
	//				degens.Add(tokenId);
	//			}
	//			catch (Exception ex)
	//			{
	//				Debug.LogWarning("Failed to add a degen to profile");
	//				Debug.LogError(ex);
	//			}
	//		}
	//	}
	//	catch (Exception e)
	//	{
	//		print(result);
	//		print(e);
	//	}
	//}

	private void ProfileInitializationFailed()
	{
		//if (fetchCharacterTries <= 3)
		//{
		//	Fail("Failed to initialize your profile\nRetrying...");
		//	Invoke(nameof(InitializeProfile), 3f + fetchCharacterTries);
		//}
		//else
		//{
		//	Fail("Failed to initialize your profile");
		//}

		Fail("Failed to initialize your profile");
	}


	public static IEnumerator ValidateSession(Action<bool> OnResponse)
	{
		UnityWebRequest www = null;
		bool isValidSession = false;
		Dictionary<string, string> headers = new Dictionary<string, string>
		{
			{ "authorizationToken", NiftyUsers.GetMyAuthorization() },
		};
		yield return Utils.GetRequest("https://odgwhiwhzb.execute-api.us-east-1.amazonaws.com/prod/accounts/session", (w) => www = w, headers);
		if (www.result == UnityWebRequest.Result.Success)
		{
			try
			{
				JObject session = JObject.Parse(www.downloadHandler.text);
				isValidSession = session["session_key"].Value<string>() == sessionId;
			}
			catch
			{
				isValidSession = false;
			}
		}
		OnResponse(isValidSession);
	}

	private static bool HasArg(string arg)
	{
		string[] args = System.Environment.GetCommandLineArgs();
		return args.FirstOrDefault(a => a == arg) != null;
	}

	private static string GetArgValue(string arg, string defult = null)
	{
		string[] args = System.Environment.GetCommandLineArgs();
		int argValueIndex = Array.FindIndex(args, a => a == arg) + 1;
		return (argValueIndex > 0 && args.Length > argValueIndex) ? args[argValueIndex] : defult;
	}

	private enum State
	{
		Begin,
		Authentication,
		Verification,
		Configuration,
		Initialization,
		Ready,
		Failure,
	}
}
