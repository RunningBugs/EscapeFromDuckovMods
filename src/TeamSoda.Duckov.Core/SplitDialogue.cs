using Cysharp.Threading.Tasks;
using Duckov.UI;
using Duckov.UI.Animations;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SplitDialogue : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private enum Status
	{
		Idle,
		Normal,
		Busy,
		Complete,
		Canceled
	}

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private Button confirmButton;

	[SerializeField]
	private TextMeshProUGUI countText;

	[SerializeField]
	private GameObject normalIndicator;

	[SerializeField]
	private GameObject busyIndicator;

	[SerializeField]
	private GameObject completeIndicator;

	[SerializeField]
	private Slider slider;

	private Item target;

	private Inventory destination;

	private int destinationIndex;

	private Inventory cachedInInventory;

	private Status status;

	public static SplitDialogue Instance
	{
		get
		{
			if (GameplayUIManager.Instance == null)
			{
				return null;
			}
			return GameplayUIManager.Instance.SplitDialogue;
		}
	}

	private void OnEnable()
	{
		View.OnActiveViewChanged += OnActiveViewChanged;
	}

	private void OnDisable()
	{
		View.OnActiveViewChanged -= OnActiveViewChanged;
	}

	private void OnActiveViewChanged()
	{
		Hide();
	}

	private void Awake()
	{
		confirmButton.onClick.AddListener(OnConfirmButtonClicked);
		slider.onValueChanged.AddListener(OnSliderValueChanged);
	}

	private void OnSliderValueChanged(float value)
	{
		RefreshCountText();
	}

	private void RefreshCountText()
	{
		countText.text = slider.value.ToString("0");
	}

	private void OnConfirmButtonClicked()
	{
		if (status == Status.Normal)
		{
			Confirm().Forget();
		}
	}

	private void Setup(Item target, Inventory destination = null, int destinationIndex = -1)
	{
		this.target = target;
		this.destination = destination;
		this.destinationIndex = destinationIndex;
		slider.minValue = 1f;
		slider.maxValue = target.StackCount;
		slider.value = (float)(target.StackCount - 1) / 2f;
		RefreshCountText();
		SwitchStatus(Status.Normal);
		cachedInInventory = target.InInventory;
	}

	public void Cancel()
	{
		if (status == Status.Normal)
		{
			SwitchStatus(Status.Canceled);
			Hide();
		}
	}

	private async UniTask Confirm()
	{
		if (status == Status.Normal)
		{
			if (cachedInInventory == target.InInventory)
			{
				SwitchStatus(Status.Busy);
				await DoSplit(Mathf.RoundToInt(slider.value));
			}
			SwitchStatus(Status.Complete);
			Hide();
		}
	}

	private void Hide()
	{
		fadeGroup.Hide();
	}

	private async UniTask DoSplit(int value)
	{
		if (value != 0)
		{
			if (value == target.StackCount)
			{
				Send(target);
				return;
			}
			Send(await target.Split(value));
			ItemUIUtilities.Select(null);
		}
		void Send(Item item)
		{
			item.Detach();
			if (destination != null && destination.Capacity > destinationIndex && destination.GetItemAt(destinationIndex) == null)
			{
				destination.AddAt(item, destinationIndex);
			}
			else
			{
				ItemUtilities.SendToPlayerCharacterInventory(item, dontMerge: true);
			}
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.pointerCurrentRaycast.gameObject == base.gameObject)
		{
			Cancel();
		}
	}

	private void SwitchStatus(Status status)
	{
		this.status = status;
		normalIndicator.SetActive(status == Status.Normal);
		busyIndicator.SetActive(status == Status.Busy);
		completeIndicator.SetActive(status == Status.Complete);
		switch (status)
		{
		}
	}

	public static void SetupAndShow(Item item)
	{
		if (!(Instance == null))
		{
			Instance.Setup(item);
			Instance.fadeGroup.Show();
		}
	}

	public static void SetupAndShow(Item item, Inventory destinationInventory, int destinationIndex)
	{
		if (!(Instance == null))
		{
			Instance.Setup(item, destinationInventory, destinationIndex);
			Instance.fadeGroup.Show();
		}
	}
}
