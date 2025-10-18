using System;
using ItemStatsSystem;
using Unity.Mathematics;
using UnityEngine;

public class FxAction : EffectAction
{
	public enum Sockets
	{
		root,
		helmat,
		armor
	}

	public Sockets socket = Sockets.helmat;

	public GameObject fxPfb;

	private CharacterMainControl _mainControl;

	private CharacterMainControl MainControl
	{
		get
		{
			if (_mainControl == null)
			{
				_mainControl = base.Master?.Item?.GetCharacterMainControl();
			}
			return _mainControl;
		}
	}

	protected override void OnTriggered(bool positive)
	{
		if ((bool)MainControl && (bool)MainControl.characterModel)
		{
			Transform transform = MainControl.transform;
			switch (socket)
			{
			case Sockets.helmat:
				transform = MainControl.characterModel.HelmatSocket;
				break;
			case Sockets.armor:
				transform = MainControl.characterModel.ArmorSocket;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			case Sockets.root:
				break;
			}
			if ((bool)transform && (bool)fxPfb)
			{
				UnityEngine.Object.Instantiate(fxPfb, transform.position, quaternion.identity);
			}
		}
	}
}
