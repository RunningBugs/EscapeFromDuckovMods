using System;
using System.Text;
using ItemStatsSystem;
using ItemStatsSystem.Stats;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.Events;

namespace Duckov.Endowment;

public class EndowmentEntry : MonoBehaviour
{
	[Serializable]
	public struct ModifierDescription
	{
		public string statKey;

		public ModifierType type;

		public float value;

		[LocalizationKey("Default")]
		private string DisplayNameKey
		{
			get
			{
				return "Stat_" + statKey;
			}
			set
			{
			}
		}

		public string DescriptionText
		{
			get
			{
				string text = DisplayNameKey.ToPlainText();
				string text2 = "";
				switch (type)
				{
				case ModifierType.Add:
					text2 = ((!(value >= 0f)) ? $"{value}" : $"+{value}");
					break;
				case ModifierType.PercentageAdd:
					text2 = ((!(value >= 0f)) ? $"-{(0f - value) * 100f:00.#}%" : $"+{value * 100f:00.#}%");
					break;
				case ModifierType.PercentageMultiply:
					text2 = $"x{(1f + value) * 100f:00.#}%";
					break;
				}
				return text + " " + text2;
			}
		}
	}

	[SerializeField]
	private EndowmentIndex index;

	[SerializeField]
	private Sprite icon;

	[SerializeField]
	[LocalizationKey("Default")]
	private string requirementTextKey;

	[SerializeField]
	private bool unlockedByDefault;

	[SerializeField]
	private ModifierDescription[] modifiers;

	public UnityEvent<EndowmentEntry> onActivate;

	public UnityEvent<EndowmentEntry> onDeactivate;

	public EndowmentIndex Index => index;

	[LocalizationKey("Default")]
	private string displayNameKey
	{
		get
		{
			return $"Endowmment_{index}";
		}
		set
		{
		}
	}

	[LocalizationKey("Default")]
	private string descriptionKey
	{
		get
		{
			return $"Endowmment_{index}_Desc";
		}
		set
		{
		}
	}

	public string RequirementText => requirementTextKey.ToPlainText();

	public Sprite Icon => icon;

	public string DisplayName => displayNameKey.ToPlainText();

	public string Description => descriptionKey.ToPlainText();

	public string DescriptionAndEffects
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			string description = Description;
			stringBuilder.AppendLine(description);
			ModifierDescription[] array = Modifiers;
			foreach (ModifierDescription modifierDescription in array)
			{
				stringBuilder.AppendLine("- " + modifierDescription.DescriptionText);
			}
			return stringBuilder.ToString();
		}
	}

	public ModifierDescription[] Modifiers => modifiers;

	private Item CharacterItem
	{
		get
		{
			if (CharacterMainControl.Main == null)
			{
				return null;
			}
			return CharacterMainControl.Main.CharacterItem;
		}
	}

	public bool UnlockedByDefault => unlockedByDefault;

	public void Activate()
	{
		ApplyModifiers();
		onActivate?.Invoke(this);
	}

	public void Deactivate()
	{
		DeleteModifiers();
		onDeactivate?.Invoke(this);
	}

	private void ApplyModifiers()
	{
		if (!(CharacterItem == null))
		{
			DeleteModifiers();
			ModifierDescription[] array = modifiers;
			for (int i = 0; i < array.Length; i++)
			{
				ModifierDescription modifierDescription = array[i];
				CharacterItem.AddModifier(modifierDescription.statKey, new Modifier(modifierDescription.type, modifierDescription.value, this));
			}
		}
	}

	private void DeleteModifiers()
	{
		if (!(CharacterItem == null))
		{
			CharacterItem.RemoveAllModifiersFrom(this);
		}
	}
}
