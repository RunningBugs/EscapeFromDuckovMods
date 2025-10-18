using System;
using Duckov.PerkTrees;
using Duckov.UI.Animations;
using Duckov.Utilities;
using LeTai.TrueShadow;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public class PerkDetails : MonoBehaviour
{
	[SerializeField]
	private FadeGroup content;

	[SerializeField]
	private FadeGroup placeHolder;

	[SerializeField]
	private TextMeshProUGUI text_Name;

	[SerializeField]
	private TextMeshProUGUI text_Description;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private TrueShadow iconShadow;

	[SerializeField]
	private GameObject unlockedIndicator;

	[SerializeField]
	private GameObject activationInfoParent;

	[SerializeField]
	private TextMeshProUGUI text_RequireLevel;

	[SerializeField]
	private CostDisplay costDisplay;

	[SerializeField]
	private Color normalTextColor = Color.white;

	[SerializeField]
	private Color unsatisfiedTextColor = Color.red;

	[SerializeField]
	private Button activateButton;

	[SerializeField]
	private Button beginButton;

	[SerializeField]
	private GameObject buttonUnsatisfiedPlaceHolder;

	[SerializeField]
	private GameObject buttonUnavaliablePlaceHolder;

	[SerializeField]
	private GameObject inProgressPlaceHolder;

	[SerializeField]
	private Image progressFillImage;

	[SerializeField]
	private TextMeshProUGUI countDownText;

	private Perk showingPerk;

	private bool editable;

	[SerializeField]
	private string RequireLevelFormatKey => "UI_Perk_RequireLevel";

	[SerializeField]
	private string RequireLevelFormat => RequireLevelFormatKey.ToPlainText();

	private void Awake()
	{
		beginButton.onClick.AddListener(OnBeginButtonClicked);
		activateButton.onClick.AddListener(OnActivateButtonClicked);
	}

	private void OnActivateButtonClicked()
	{
		showingPerk.ConfirmUnlock();
	}

	private void OnBeginButtonClicked()
	{
		showingPerk.SubmitItemsAndBeginUnlocking();
	}

	private void OnEnable()
	{
		Refresh();
	}

	public void Setup(Perk perk, bool editable = false)
	{
		UnregisterEvents();
		showingPerk = perk;
		this.editable = editable;
		Refresh();
		RegisterEvents();
	}

	private void RegisterEvents()
	{
		if ((bool)showingPerk)
		{
			showingPerk.onUnlockStateChanged += OnTargetStateChanged;
		}
	}

	private void OnTargetStateChanged(Perk perk, bool arg2)
	{
		Refresh();
	}

	private void UnregisterEvents()
	{
		if ((bool)showingPerk)
		{
			showingPerk.onUnlockStateChanged -= OnTargetStateChanged;
		}
	}

	private void Refresh()
	{
		if (showingPerk == null)
		{
			content.Hide();
			placeHolder.Show();
			return;
		}
		text_Name.text = showingPerk.DisplayName;
		text_Description.text = showingPerk.Description;
		icon.sprite = showingPerk.Icon;
		(float, Color, bool) shadowOffsetAndColorOfQuality = GameplayDataSettings.UIStyle.GetShadowOffsetAndColorOfQuality(showingPerk.DisplayQuality);
		iconShadow.IgnoreCasterColor = true;
		iconShadow.Color = shadowOffsetAndColorOfQuality.Item2;
		iconShadow.Inset = shadowOffsetAndColorOfQuality.Item3;
		iconShadow.OffsetDistance = shadowOffsetAndColorOfQuality.Item1;
		bool flag = !showingPerk.Unlocked && editable;
		bool flag2 = showingPerk.AreAllParentsUnlocked();
		bool flag3 = false;
		if (flag2)
		{
			flag3 = showingPerk.Requirement.AreSatisfied();
		}
		activateButton.gameObject.SetActive(value: false);
		beginButton.gameObject.SetActive(value: false);
		buttonUnavaliablePlaceHolder.SetActive(value: false);
		buttonUnsatisfiedPlaceHolder.SetActive(value: false);
		inProgressPlaceHolder.SetActive(value: false);
		unlockedIndicator.SetActive(showingPerk.Unlocked);
		if (!showingPerk.Unlocked)
		{
			if (showingPerk.Unlocking)
			{
				if (showingPerk.GetRemainingTime() <= TimeSpan.Zero)
				{
					activateButton.gameObject.SetActive(value: true);
				}
				else
				{
					inProgressPlaceHolder.SetActive(value: true);
				}
			}
			else if (flag2)
			{
				if (flag3)
				{
					beginButton.gameObject.SetActive(value: true);
				}
				else
				{
					buttonUnsatisfiedPlaceHolder.SetActive(value: true);
				}
			}
			else
			{
				buttonUnavaliablePlaceHolder.SetActive(value: true);
			}
		}
		if (flag)
		{
			SetupActivationInfo();
		}
		activationInfoParent.SetActive(flag);
		content.Show();
		placeHolder.Hide();
	}

	private void SetupActivationInfo()
	{
		if ((bool)showingPerk)
		{
			int level = showingPerk.Requirement.level;
			if (level > 0)
			{
				bool flag = EXPManager.Level >= level;
				string text = "#" + (flag ? normalTextColor.ToHexString() : unsatisfiedTextColor.ToHexString());
				text_RequireLevel.gameObject.SetActive(value: true);
				int level2 = showingPerk.Requirement.level;
				string color = text;
				text_RequireLevel.text = RequireLevelFormat.Format(new
				{
					level = level2,
					color = color
				});
			}
			else
			{
				text_RequireLevel.gameObject.SetActive(value: false);
			}
			costDisplay.Setup(showingPerk.Requirement.cost);
		}
	}

	private void Update()
	{
		if ((bool)showingPerk && showingPerk.Unlocking && inProgressPlaceHolder.activeSelf)
		{
			UpdateCountDown();
		}
	}

	private void UpdateCountDown()
	{
		TimeSpan remainingTime = showingPerk.GetRemainingTime();
		if (remainingTime <= TimeSpan.Zero)
		{
			Refresh();
			return;
		}
		progressFillImage.fillAmount = showingPerk.GetProgress01();
		countDownText.text = $"{remainingTime.Days} {remainingTime.Hours:00}:{remainingTime.Minutes:00}:{remainingTime.Seconds:00}.{remainingTime.Milliseconds:000}";
	}
}
