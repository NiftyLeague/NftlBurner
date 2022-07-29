using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioManager : MonoBehaviour
{
	public List<AudioSource> soundEffectAudioSources;
	public List<Sound> soundList;
	private Dictionary<SoundID, Sound> soundDictionary;
	private bool hasInitializedSoundDictionary;
	private int lastAudioSourceUsed;

	private void Start()
	{
		Initialize();
	}

	public void Initialize()
	{
		if (hasInitializedSoundDictionary)
		{
			return;
		}

		soundDictionary = new Dictionary<SoundID, Sound>();

		foreach (Sound sound in soundList)
		{
			soundDictionary.Add(sound.soundID, sound);
		}

		hasInitializedSoundDictionary = true;
	}

	public void PlaySound(SoundID soundID, float volume = 0.15f)
	{
		List<AudioSource> audioSourcesInSound = soundDictionary[soundID].audioSources;
		AudioSource soundEffectSource = audioSourcesInSound[UnityEngine.Random.Range(0, audioSourcesInSound.Count)];
		AudioSource currentEffectSource = soundEffectAudioSources[lastAudioSourceUsed++];
		currentEffectSource.clip = soundEffectSource.clip;
		currentEffectSource.pitch = soundEffectSource.pitch;
		currentEffectSource.volume = volume;
		currentEffectSource.Play();

		if (lastAudioSourceUsed >= 50)
		{
			lastAudioSourceUsed = 0;
		}
	}

	[Serializable]
	public class Sound
	{
		public List<AudioSource> audioSources;
		public SoundID soundID;
	}

	public enum SoundID
	{
		None,
		HoverOverButton,
		PressButton,
		ErrorMessage,
		Eruption,
		Sparkle,
		Squeeze,
		Whoosh
	}
}
