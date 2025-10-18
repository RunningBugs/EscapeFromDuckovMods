using System;
using Duckov;
using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.Tasks.Actions;

public class PostSound : ActionTask<AICharacterController>
{
	public enum VoiceSounds
	{
		normal,
		surprise,
		death
	}

	public VoiceSounds voiceSound;

	protected override string info => $"Post Sound: {voiceSound.ToString()} ";

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		if ((bool)base.agent && (bool)base.agent.CharacterMainControl)
		{
			if (!base.agent.canTalk)
			{
				EndAction(success: true);
				return;
			}
			GameObject gameObject = base.agent.CharacterMainControl.gameObject;
			switch (voiceSound)
			{
			case VoiceSounds.normal:
				AudioManager.PostQuak("normal", base.agent.CharacterMainControl.AudioVoiceType, gameObject);
				break;
			case VoiceSounds.surprise:
				AudioManager.PostQuak("surprise", base.agent.CharacterMainControl.AudioVoiceType, gameObject);
				break;
			case VoiceSounds.death:
				AudioManager.PostQuak("death", base.agent.CharacterMainControl.AudioVoiceType, gameObject);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
		EndAction(success: true);
	}

	protected override void OnStop()
	{
	}

	protected override void OnPause()
	{
	}
}
