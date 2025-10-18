using ItemStatsSystem;
using UnityEngine;

namespace Duckov.UI;

public class ItemShortcutPanel : MonoBehaviour
{
	[SerializeField]
	private ItemShortcutButton[] buttons;

	private bool initialized;

	public Inventory Target { get; private set; }

	public CharacterMainControl Character { get; internal set; }

	private void Awake()
	{
		LevelManager.OnLevelInitialized += OnLevelInitialized;
		if (LevelManager.LevelInited)
		{
			Initialize();
		}
	}

	private void OnDestroy()
	{
		LevelManager.OnLevelInitialized -= OnLevelInitialized;
	}

	private void OnLevelInitialized()
	{
		Initialize();
	}

	private void Initialize()
	{
		Character = LevelManager.Instance?.MainCharacter;
		if (Character == null)
		{
			return;
		}
		Target = LevelManager.Instance?.MainCharacter?.CharacterItem?.Inventory;
		if (Target == null)
		{
			return;
		}
		for (int i = 0; i < buttons.Length; i++)
		{
			ItemShortcutButton itemShortcutButton = buttons[i];
			if (!(itemShortcutButton == null))
			{
				itemShortcutButton.Initialize(this, i);
			}
		}
		initialized = true;
	}
}
