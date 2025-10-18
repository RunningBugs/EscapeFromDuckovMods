using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public class ItemDecomposeView : View
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private InventoryDisplay characterInventoryDisplay;

	[SerializeField]
	private InventoryDisplay storageDisplay;

	[SerializeField]
	private FadeGroup detailsFadeGroup;

	[SerializeField]
	private ItemDetailsDisplay detailsDisplay;

	[SerializeField]
	private DecomposeSlider countSlider;

	[SerializeField]
	private TextMeshProUGUI targetNameDisplay;

	[SerializeField]
	private CostDisplay resultDisplay;

	[SerializeField]
	private GameObject cannotDecomposeIndicator;

	[SerializeField]
	private GameObject noItemSelectedIndicator;

	[SerializeField]
	private Button decomposeButton;

	[SerializeField]
	private GameObject busyIndicator;

	private bool decomposing;

	public static ItemDecomposeView Instance => View.GetViewInstance<ItemDecomposeView>();

	private Item SelectedItem => ItemUIUtilities.SelectedItem;

	protected override void Awake()
	{
		base.Awake();
		decomposeButton.onClick.AddListener(OnDecomposeButtonClick);
		countSlider.OnValueChangedEvent += OnSliderValueChanged;
		SetupEmpty();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		countSlider.OnValueChangedEvent -= OnSliderValueChanged;
	}

	private void OnDecomposeButtonClick()
	{
		if (!decomposing && !(SelectedItem == null))
		{
			int value = countSlider.Value;
			DecomposeTask(SelectedItem, value).Forget();
		}
	}

	private void OnFastPick(UIInputEventData data)
	{
		OnDecomposeButtonClick();
		data.Use();
	}

	private async UniTask DecomposeTask(Item item, int count)
	{
		decomposing = true;
		busyIndicator.SetActive(value: true);
		if (item != null)
		{
			AudioManager.PlayPutItemSFX(item);
		}
		await DecomposeDatabase.Decompose(item, count);
		busyIndicator.SetActive(value: false);
		decomposing = false;
		Refresh();
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		fadeGroup.Show();
		ItemUIUtilities.Select(null);
		detailsFadeGroup.SkipHide();
		if (CharacterMainControl.Main != null)
		{
			characterInventoryDisplay.gameObject.SetActive(value: true);
			characterInventoryDisplay.Setup(CharacterMainControl.Main.CharacterItem.Inventory, null, (Item e) => e == null || DecomposeDatabase.CanDecompose(e.TypeID));
		}
		else
		{
			characterInventoryDisplay.gameObject.SetActive(value: false);
		}
		if (PlayerStorage.Inventory != null)
		{
			storageDisplay.gameObject.SetActive(value: true);
			storageDisplay.Setup(PlayerStorage.Inventory, null, (Item e) => e == null || DecomposeDatabase.CanDecompose(e.TypeID));
		}
		else
		{
			storageDisplay.gameObject.SetActive(value: false);
		}
		Refresh();
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
	}

	private void OnEnable()
	{
		ItemUIUtilities.OnSelectionChanged += OnSelectionChanged;
		UIInputManager.OnFastPick += OnFastPick;
	}

	private void OnDisable()
	{
		ItemUIUtilities.OnSelectionChanged -= OnSelectionChanged;
		UIInputManager.OnFastPick -= OnFastPick;
	}

	private void OnSelectionChanged()
	{
		Refresh();
	}

	private void OnSliderValueChanged(float value)
	{
		RefreshResult(SelectedItem, Mathf.RoundToInt(value));
	}

	private void Refresh()
	{
		if (SelectedItem == null)
		{
			SetupEmpty();
		}
		else
		{
			Setup(SelectedItem);
		}
	}

	private void SetupEmpty()
	{
		detailsFadeGroup.Hide();
		targetNameDisplay.text = "-";
		resultDisplay.Clear();
		cannotDecomposeIndicator.SetActive(value: false);
		decomposeButton.gameObject.SetActive(value: false);
		noItemSelectedIndicator.SetActive(value: true);
		busyIndicator.SetActive(value: false);
		countSlider.SetMinMax(1, 1);
		countSlider.Value = 1;
	}

	private void Setup(Item selectedItem)
	{
		if (!(selectedItem == null))
		{
			noItemSelectedIndicator.SetActive(value: false);
			detailsDisplay.Setup(selectedItem);
			detailsFadeGroup.Show();
			targetNameDisplay.text = selectedItem.DisplayName;
			bool valid = DecomposeDatabase.GetDecomposeFormula(selectedItem.TypeID).valid;
			decomposeButton.gameObject.SetActive(valid);
			cannotDecomposeIndicator.gameObject.SetActive(!valid);
			SetupSlider(selectedItem);
			RefreshResult(selectedItem, Mathf.RoundToInt(countSlider.Value));
			busyIndicator.SetActive(decomposing);
		}
	}

	private void SetupSlider(Item selectedItem)
	{
		if (selectedItem.Stackable)
		{
			countSlider.SetMinMax(1, selectedItem.StackCount);
			countSlider.Value = selectedItem.StackCount;
		}
		else
		{
			countSlider.SetMinMax(1, 1);
			countSlider.Value = 1;
		}
	}

	private void RefreshResult(Item selectedItem, int count)
	{
		if (selectedItem == null)
		{
			countSlider.SetMinMax(1, 1);
			countSlider.Value = 1;
			return;
		}
		DecomposeFormula decomposeFormula = DecomposeDatabase.GetDecomposeFormula(selectedItem.TypeID);
		if (decomposeFormula.valid)
		{
			_ = selectedItem.Stackable;
			resultDisplay.Setup(decomposeFormula.result, count);
		}
		else
		{
			resultDisplay.Clear();
		}
	}

	internal static void Show()
	{
		ItemDecomposeView instance = Instance;
		if (!(instance == null))
		{
			instance.Open();
		}
	}
}
