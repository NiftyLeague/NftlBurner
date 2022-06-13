using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIVersion : MonoBehaviour
{
	public TextMeshProUGUI version;
	public TextMeshProUGUI session;


	void Start()
	{
		version.text = $"v{Application.version}";
		session.text = Launcher.shortSessionId;
	}
}
