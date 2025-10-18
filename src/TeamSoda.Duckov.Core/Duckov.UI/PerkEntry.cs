using System;
using Duckov.PerkTrees;
using Duckov.Utilities;
using LeTai.TrueShadow;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.UI;

public class PerkEntry : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPoolable
{
	[Serializable]
	public struct Look
	{
		public Color iconColor;

		public Material material;

		public Color frameColor;

		public Color frameGlowColor;

		public Color backgroundColor;
	}

	[SerializeField]
	private Image icon;

	[SerializeField]
	private TrueShadow iconShadow;

	[SerializeField]
	private GameObject selectionIndicator;

	[SerializeField]
	private Image frame;

	[SerializeField]
	private TrueShadow frameGlow;

	[SerializeField]
	private Image background;

	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private PunchReceiver punchReceiver;

	[SerializeField]
	private GameObject inProgressIndicator;

	[SerializeField]
	private GameObject timeUpIndicator;

	[SerializeField]
	private GameObject avaliableForResearchIndicator;

	[SerializeField]
	private Look activeLook;

	[SerializeField]
	private Look avaliableLook;

	[SerializeField]
	private Look unavaliableLook;

	private RectTransform _rectTransform;

	private PerkTreeView master;

	private Perk target;

	public RectTransform RectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = GetComponent<RectTransform>();
			}
			return _rectTransform;
		}
	}

	public Perk Target => target;

	private void SwitchToActiveLook()
	{
		ApplyLook(activeLook);
	}

	private void SwitchToAvaliableLook()
	{
		ApplyLook(avaliableLook);
	}

	private void SwitchToUnavaliableLook()
	{
		ApplyLook(unavaliableLook);
	}

	public void Setup(PerkTreeView master, Perk target)
	{
		UnregisterEvents();
		this.master = master;
		this.target = target;
		icon.sprite = target.Icon;
		(float, Color, bool) shadowOffsetAndColorOfQuality = GameplayDataSettings.UIStyle.GetShadowOffsetAndColorOfQuality(target.DisplayQuality);
		iconShadow.IgnoreCasterColor = true;
		iconShadow.Color = shadowOffsetAndColorOfQuality.Item2;
		iconShadow.OffsetDistance = shadowOffsetAndColorOfQuality.Item1;
		iconShadow.Inset = shadowOffsetAndColorOfQuality.Item3;
		displayNameText.text = target.DisplayName;
		Refresh();
		RegisterEvents();
	}

	private void Refresh()
	{
		if (!(target == null))
		{
			bool unlocked = target.Unlocked;
			bool flag = target.AreAllParentsUnlocked();
			if (unlocked)
			{
				SwitchToActiveLook();
			}
			else if (flag)
			{
				SwitchToAvaliableLook();
			}
			else
			{
				SwitchToUnavaliableLook();
			}
			bool unlocking = target.Unlocking;
			bool flag2 = target.GetRemainingTime() <= TimeSpan.Zero;
			avaliableForResearchIndicator.SetActive(!unlocked && !unlocking && target.AreAllParentsUnlocked() && target.Requirement.AreSatisfied());
			inProgressIndicator.SetActive(!unlocked && unlocking && !flag2);
			timeUpIndicator.SetActive(!unlocked && unlocking && flag2);
			if (!(master == null))
			{
				selectionIndicator.SetActive(master.GetSelection() == this);
			}
		}
	}

	private void OnMasterSelectionChanged(PerkEntry entry)
	{
		Refresh();
	}

	private void RegisterEvents()
	{
		if ((bool)master)
		{
			master.onSelectionChanged += OnMasterSelectionChanged;
		}
		if ((bool)target)
		{
			target.onUnlockStateChanged += OnTargetStateChanged;
		}
	}

	private void OnTargetStateChanged(Perk perk, bool state)
	{
		punchReceiver?.Punch();
		Refresh();
	}

	private void UnregisterEvents()
	{
		if ((bool)master)
		{
			master.onSelectionChanged -= OnMasterSelectionChanged;
		}
		if ((bool)target)
		{
			target.onUnlockStateChanged -= OnTargetStateChanged;
		}
	}

	private void OnDisable()
	{
		UnregisterEvents();
	}

	public void NotifyPooled()
	{
	}

	public void NotifyReleased()
	{
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!(master == null))
		{
			punchReceiver?.Punch();
			master.SetSelection(this);
		}
	}

	internal Vector2 GetLayoutPosition()
	{
		if (target == null)
		{
			return Vector2.zero;
		}
		return target.GetLayoutPosition();
	}

	private void ApplyLook(Look look)
	{
		icon.material = look.material;
		icon.color = look.iconColor;
		frame.color = look.frameColor;
		frameGlow.enabled = look.frameGlowColor.a > 0f;
		frameGlow.Color = look.frameGlowColor;
		background.color = look.backgroundColor;
	}

	private void FixedUpdate()
	{
		if (inProgressIndicator.activeSelf && target.GetRemainingTime() <= TimeSpan.Zero)
		{
			Refresh();
		}
	}
}
