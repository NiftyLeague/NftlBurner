using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIVersion : MonoBehaviour
{
	public Text version;
	public Text session;


	void Start()
	{
		version.text = $"v{Application.version}";
		session.text = Launcher.shortSessionId;
	}
}
