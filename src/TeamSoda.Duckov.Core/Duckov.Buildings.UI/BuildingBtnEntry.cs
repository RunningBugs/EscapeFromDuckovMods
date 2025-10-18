using System;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.Buildings.UI;

public class BuildingBtnEntry : MonoBehaviour
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private CostDisplay costDisplay;

	[SerializeField]
	private LongPressButton recycleButton;

	[SerializeField]
	private TextMeshProUGUI amountText;

	[SerializeField]
	[LocalizationKey("Default")]
	private string tokenFormatKey;

	[SerializeField]
	private TextMeshProUGUI tokenText;

	[SerializeField]
	private GameObject reachedAmountLimitationIndicator;

	[SerializeField]
	private Image backGround;

	[SerializeField]
	private Color normalColor;

	[SerializeField]
	private Color avaliableColor;

	private BuildingInfo info;

	private string TokenFormat => tokenFormatKey.ToPlainText();

	public BuildingInfo Info => info;

	public bool CostEnough
	{
		get
		{
			if (info.TokenAmount > 0)
			{
				return true;
			}
			if (info.cost.Enough)
			{
				return true;
			}
			return false;
		}
	}

	public event Action<BuildingBtnEntry> onButtonClicked;

	public event Action<BuildingBtnEntry> onRecycleRequested;

	private void Awake()
	{
		button.onClick.AddListener(OnButtonClicked);
		recycleButton.onPressFullfilled.AddListener(OnRecycleButtonTriggered);
	}

	private void OnRecycleButtonTriggered()
	{
		this.onRecycleRequested?.Invoke(this);
	}

	private void OnEnable()
	{
		BuildingManager.OnBuildingListChanged += Refresh;
	}

	private void OnDisable()
	{
		BuildingManager.OnBuildingListChanged -= Refresh;
	}

	private void OnButtonClicked()
	{
		this.onButtonClicked?.Invoke(this);
	}

	internal void Setup(BuildingInfo buildingInfo)
	{
		info = buildingInfo;
		Refresh();
	}

	private void Refresh()
	{
		int tokenAmount = info.TokenAmount;
		nameText.text = info.DisplayName;
		descriptionText.text = info.Description;
		tokenText.text = TokenFormat.Format(new { tokenAmount });
		icon.sprite = info.iconReference;
		costDisplay.Setup(info.cost);
		costDisplay.gameObject.SetActive(tokenAmount <= 0);
		bool reachedAmountLimit = info.ReachedAmountLimit;
		amountText.text = ((info.maxAmount > 0) ? $"{info.CurrentAmount}/{info.maxAmount}" : $"{info.CurrentAmount}/∞");
		reachedAmountLimitationIndicator.SetActive(reachedAmountLimit);
		bool flag = !info.ReachedAmountLimit && CostEnough;
		backGround.color = (flag ? avaliableColor : normalColor);
		recycleButton.gameObject.SetActive(info.CurrentAmount > 0);
	}
}
