using System;
using ItemStatsSystem;
using UnityEngine;

public class SpawnPaperBoxAction : EffectAction
{
	public enum Sockets
	{
		root,
		helmat,
		armor
	}

	public Sockets socket = Sockets.helmat;

	public PaperBox paperBoxPrefab;

	private PaperBox instance;

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
			if ((bool)transform && (bool)paperBoxPrefab)
			{
				instance = UnityEngine.Object.Instantiate(paperBoxPrefab, transform);
				instance.character = MainControl;
			}
		}
	}

	private void OnDestroy()
	{
		if ((bool)instance)
		{
			UnityEngine.Object.Destroy(instance.gameObject);
		}
	}
}
