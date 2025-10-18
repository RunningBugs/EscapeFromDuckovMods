using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI.ProceduralImage;

public class BulletCountHUD : MonoBehaviour
{
	private ItemAgent_Gun gunAgent;

	private CharacterMainControl characterMainControl;

	private ItemAgent_Gun gunAgnet;

	public CanvasGroup canvasGroup;

	public TextMeshProUGUI bulletCountText;

	public TextMeshProUGUI capacityText;

	public ProceduralImage background;

	public Color normalBackgroundColor;

	public Color emptyBackgroundColor;

	private int bulletCount = -1;

	private int totalCount = -1;

	public UnityEvent OnValueChangeEvent;

	private void Awake()
	{
	}

	public void Update()
	{
		if (!characterMainControl)
		{
			characterMainControl = LevelManager.Instance.MainCharacter;
			if ((bool)characterMainControl)
			{
				characterMainControl.OnHoldAgentChanged += OnHoldAgentChanged;
				characterMainControl.CharacterItem.Inventory.onContentChanged += OnInventoryChanged;
				if (characterMainControl.CurrentHoldItemAgent != null)
				{
					OnHoldAgentChanged(characterMainControl.CurrentHoldItemAgent);
				}
				ChangeTotalCount();
				capacityText.text = totalCount.ToString("D2");
			}
		}
		if (gunAgnet == null)
		{
			canvasGroup.alpha = 0f;
			return;
		}
		bool flag = false;
		canvasGroup.alpha = 1f;
		int num = gunAgnet.BulletCount;
		if (bulletCount != num)
		{
			bulletCount = num;
			bulletCountText.text = num.ToString("D2");
			flag = true;
		}
		if (flag)
		{
			OnValueChangeEvent?.Invoke();
			if (bulletCount <= 0 && (totalCount <= 0 || !capacityText.gameObject.activeInHierarchy))
			{
				background.color = emptyBackgroundColor;
			}
			else
			{
				background.color = normalBackgroundColor;
			}
		}
	}

	private void OnInventoryChanged(Inventory inventory, int index)
	{
		ChangeTotalCount();
	}

	private void ChangeTotalCount()
	{
		int num = 0;
		if ((bool)gunAgnet)
		{
			num = gunAgnet.GetBulletCountInInventory();
		}
		if (totalCount != num)
		{
			totalCount = num;
			capacityText.text = totalCount.ToString("D2");
		}
	}

	private void OnDestroy()
	{
		if ((bool)characterMainControl)
		{
			characterMainControl.OnHoldAgentChanged -= OnHoldAgentChanged;
			characterMainControl.CharacterItem.Inventory.onContentChanged -= OnInventoryChanged;
		}
	}

	private void OnHoldAgentChanged(DuckovItemAgent newAgent)
	{
		if (newAgent == null)
		{
			gunAgnet = null;
		}
		gunAgnet = newAgent as ItemAgent_Gun;
		ChangeTotalCount();
	}
}
