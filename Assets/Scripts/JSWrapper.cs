using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Collections;

#if UNITY_EDITOR || UNITY_STANDALONE
using System.Reflection;
#endif // #if UNITY_EDITOR

public class JSWrapper : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR

	[DllImport("__Internal")]
	public static extern void DispatchEvent(string eventName, string detail);
	[DllImport("__Internal")]
	public static extern void SubmitTraits(string traits, string name, string callback);
	[DllImport("__Internal")]
	public static extern void StartAuthentication(string name, string callback);
	[DllImport("__Internal")]
	public static extern void SignMessage(string from, string message, string password, string name, string callback);
	[DllImport("__Internal")]
	public static extern void GetConfiguration(string name, string callback);
	[DllImport("__Internal")]
	public static extern void SyncFs();

#else // #if UNITY_WEBGL && !UNITY_EDITOR

	private static MonoBehaviour _launcher = null;
	private static MonoBehaviour Launcher
	{
		get
		{
			if (_launcher != null)
			{
				return _launcher;
			}
			return FindObjectOfType<Launcher>();
		}
	}

	public static void DispatchEvent(string eventName, string detail)
	{
		PrintCallDetails(MethodBase.GetCurrentMethod(), eventName, detail);
	}


	public static void SubmitTraits(string traits, string name, string callback)
	{
		PrintCallDetails(MethodBase.GetCurrentMethod(), traits, name, callback);

		Launcher.StartCoroutine(OnSubmitTraitMapMock());
		IEnumerator OnSubmitTraitMapMock()
		{
			yield return new WaitForSeconds(5f);
			Launcher.SendMessage(callback, "true");
		}
	}

	public static void StartAuthentication(string name, string callback)
	{
		PrintCallDetails(MethodBase.GetCurrentMethod(), name, callback);
		Launcher.StartCoroutine(OnAuthenticatedMock());

		IEnumerator OnAuthenticatedMock()
		{
			yield return new WaitForEndOfFrame();
			string auth = "test_auth";
			string[] results = {
				//$"true,0x0b9c624f45493f044ca468971110955345281679,Vitalik,{auth}",
				$"true,0xc9e2ea211a16d5d5d9de68804f85b13c52d8c548,Orange,{auth}",
				$"true,0x5f5732de939f04c032292355062254a6c03bfd64,Bolo,{auth}",
				$"true,0x32fcc745555671b84ccf89cd573d3da69f95a971,Orange Fish,{auth}",
				$"true,0xb970e591772f2ceb482bcd03a8d2f1924a4044ce,Snarfy,{auth}",
				$"true,0xa41dcee235f7f8ab2c7d8a3e36fdc63704c142ae,DojaCat,{auth}",
				$"true,0x9a8a631aef07d9d52936f9ae1b38c6245013d3d5,Mattyink,{auth}",
				$"true,0x05636488c25eab8ab58bf43e9a18e85a8935800e,richyrich35,{auth}",
			};
			string result = "true,0x594f49b52400DB1D87c7dB3F784Be20D50972ae0,Vitalik,gAAAAABh6n82tO12HkpywxsQpmZJbZolOtHokZXQoXFEmF6r7C1zk8uFVbpNkV2ZtwXRvu24raZozWcDusqo3VHQH-YdyxH4Qr_3Q2oK_PcwLcBUd_trc8cH0Oq-Pib57m0f3fatx3VAlCzAIOZ0UH4wzkLLY7ge5H0PLaeEtY-hUE_bfY3LRsv1jdJPgmyNqJZnDX7DnusjaNwZXDJfRIuTTm3nf9P-GJuMrLYhWSqTpV5KZcPkLO4FI1TBL9d4oO4g1ALMSXJuEkY2uUBbEsp7jRvjY7jPcsagOjG2zzf2FzV21L2t8I6Zsk3tP-jBUNEa0wm8ZdACiI0xq5julMdrsyJXSdF-8Q==,1,7712,4226,151";
			Launcher.SendMessage(callback, result);
		}
	}

	public static void GetConfiguration(string name, string callback)
	{
		PrintCallDetails(MethodBase.GetCurrentMethod(), name, callback);
		Launcher.StartCoroutine(OnGetConfigurationMock());

		IEnumerator OnGetConfigurationMock()
		{
			yield return new WaitForEndOfFrame();
			string result = $"mainnet,";
			Launcher.SendMessage(callback, result);
		}
	}

	public static void SignMessage(string from, string message, string password, string name, string callback)
	{
		PrintCallDetails(MethodBase.GetCurrentMethod(), from, message, password, name, callback);
		Launcher.StartCoroutine(OnVerifyMock());

		IEnumerator OnVerifyMock()
		{
			yield return new WaitForEndOfFrame();
			string result = "true,0x4b533d695cca9f1e65c11a225cebb5087980d1fa14e0a77b42a5e5c5d87e418c177bbef85104c30b6f402616577f613efec1a850c3234976b0b727cd097687161b";
			Launcher.SendMessage(callback, result);
		}
	}

	private static void PrintCallDetails(MethodBase methodBase, params object[] args)
	{
#if UNITY_EDITOR
		string s = $"<b>JSWrapper.{methodBase.Name}</b> was called with the following args";
		var paramsInfo = methodBase.GetParameters();
		for (int i = 0; i < args.Length; i++)
		{
			s += $"\n{paramsInfo[i].Name}: <i>\"{args[i]}\"</i>";
		}
		Debug.Log(s);
#endif
	}
#endif // #else
}
