using Duckov.Utilities;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.Quests.UI;

public class RewardEntry : MonoBehaviour, IPoolable
{
	[SerializeField]
	private Image rewardIcon;

	[SerializeField]
	private TextMeshProUGUI rewardText;

	[SerializeField]
	private Button claimButton;

	[SerializeField]
	private GameObject claimableIndicator;

	[SerializeField]
	private Image statusIcon;

	[SerializeField]
	private TextMeshProUGUI buttonText;

	[SerializeField]
	private GameObject claimingIcon;

	[SerializeField]
	private Sprite claimIcon;

	[LocalizationKey("Default")]
	[SerializeField]
	private string claimTextKey = "UI_Quest_RewardClaim";

	[SerializeField]
	private Sprite claimedIcon;

	[LocalizationKey("Default")]
	[SerializeField]
	private string claimedTextKey = "UI_Quest_RewardClaimed";

	[SerializeField]
	private bool interactable;

	private Reward target;

	public bool Interactable
	{
		get
		{
			return interactable;
		}
		internal set
		{
			interactable = value;
		}
	}

	private void Awake()
	{
		claimButton.onClick.AddListener(OnClaimButtonClicked);
	}

	private void OnClaimButtonClicked()
	{
		target?.Claim();
	}

	public void NotifyPooled()
	{
	}

	public void NotifyReleased()
	{
		UnregisterEvents();
	}

	internal void Setup(Reward target)
	{
		UnregisterEvents();
		this.target = target;
		if (!(target == null))
		{
			Refresh();
			RegisterEvents();
		}
	}

	private void RegisterEvents()
	{
		if (!(target == null))
		{
			target.onStatusChanged += OnTargetStatusChanged;
		}
	}

	private void UnregisterEvents()
	{
		if (!(target == null))
		{
			target.onStatusChanged -= OnTargetStatusChanged;
		}
	}

	private void OnTargetStatusChanged()
	{
		Refresh();
	}

	private void Refresh()
	{
		if (target == null)
		{
			return;
		}
		rewardText.text = target.Description;
		Sprite icon = target.Icon;
		rewardIcon.gameObject.SetActive(icon);
		rewardIcon.sprite = icon;
		bool claimed = target.Claimed;
		bool claimable = target.Claimable;
		bool flag = Interactable && claimable;
		bool active = !Interactable && claimable && !claimed;
		claimButton.gameObject.SetActive(flag);
		if (claimableIndicator != null)
		{
			claimableIndicator.SetActive(active);
		}
		if (flag)
		{
			if ((bool)buttonText)
			{
				buttonText.text = (claimed ? claimedTextKey.ToPlainText() : claimTextKey.ToPlainText());
			}
			statusIcon.sprite = (claimed ? claimedIcon : claimIcon);
			claimButton.interactable = !claimed;
			statusIcon.gameObject.SetActive(!target.Claiming);
			claimingIcon.gameObject.SetActive(target.Claiming);
		}
	}
}
