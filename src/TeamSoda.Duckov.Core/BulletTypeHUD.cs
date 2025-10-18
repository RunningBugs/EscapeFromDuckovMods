using System.Collections.Generic;
using Duckov.UI;
using Duckov.Utilities;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI.ProceduralImage;

public class BulletTypeHUD : MonoBehaviour
{
	private CharacterMainControl characterMainControl;

	private ItemAgent_Gun gunAgent;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private TextMeshProUGUI bulletTypeText;

	[SerializeField]
	private ProceduralImage background;

	[SerializeField]
	private Color normalColor;

	[SerializeField]
	private Color emptyColor;

	private int bulletTpyeID = -2;

	[SerializeField]
	private GameObject typeList;

	public UnityEvent OnTypeChangeEvent;

	public GameObject indicator;

	private int selectIndex;

	private int totalSelctionCount;

	[SerializeField]
	private BulletTypeSelectButton originSelectButton;

	private List<BulletTypeSelectButton> selectionsHUD;

	private PrefabPool<BulletTypeSelectButton> _selectionsCache;

	private bool listOpen;

	private PrefabPool<BulletTypeSelectButton> Selections
	{
		get
		{
			if (_selectionsCache == null)
			{
				_selectionsCache = new PrefabPool<BulletTypeSelectButton>(originSelectButton);
			}
			return _selectionsCache;
		}
	}

	private bool CanOpenList
	{
		get
		{
			if (!characterMainControl)
			{
				return false;
			}
			if ((bool)characterMainControl.CurrentAction && characterMainControl.CurrentAction.Running)
			{
				return false;
			}
			if (!InputManager.InputActived)
			{
				return false;
			}
			return true;
		}
	}

	private void Awake()
	{
		selectionsHUD = new List<BulletTypeSelectButton>();
		originSelectButton.gameObject.SetActive(value: false);
		WeaponButton.OnWeaponButtonSelected += OnWeaponButtonSelected;
		typeList.SetActive(value: false);
		InputManager.OnSwitchBulletTypeInput += OnSwitchInput;
	}

	private void OnDestroy()
	{
		WeaponButton.OnWeaponButtonSelected -= OnWeaponButtonSelected;
		if ((bool)characterMainControl)
		{
			characterMainControl.OnHoldAgentChanged -= OnHoldAgentChanged;
		}
		InputManager.OnSwitchBulletTypeInput -= OnSwitchInput;
	}

	private void OnWeaponButtonSelected(WeaponButton button)
	{
		RectTransform obj = canvasGroup.transform as RectTransform;
		RectTransform rectTransform = button.transform as RectTransform;
		obj.position = (Vector2)rectTransform.position + (rectTransform.rect.center + (Vector2)((rectTransform.rect.height / 2f + 8f) * rectTransform.up)) * rectTransform.lossyScale;
	}

	public void Update()
	{
		if (!characterMainControl)
		{
			characterMainControl = LevelManager.Instance.MainCharacter;
			if ((bool)characterMainControl)
			{
				characterMainControl.OnHoldAgentChanged += OnHoldAgentChanged;
				if (characterMainControl.CurrentHoldItemAgent != null)
				{
					OnHoldAgentChanged(characterMainControl.CurrentHoldItemAgent);
				}
			}
		}
		if (gunAgent == null)
		{
			canvasGroup.alpha = 0f;
			canvasGroup.interactable = false;
			return;
		}
		canvasGroup.alpha = 1f;
		canvasGroup.interactable = true;
		if (bulletTypeText != null && gunAgent.GunItemSetting != null)
		{
			int targetBulletID = gunAgent.GunItemSetting.TargetBulletID;
			if (bulletTpyeID != targetBulletID)
			{
				bulletTpyeID = targetBulletID;
				if (bulletTpyeID >= 0)
				{
					bulletTypeText.text = gunAgent.GunItemSetting.CurrentBulletName;
					bulletTypeText.color = Color.black;
					background.color = normalColor;
				}
				else
				{
					bulletTypeText.text = "UI_Bullet_NotAssigned".ToPlainText();
					bulletTypeText.color = Color.white;
					background.color = emptyColor;
				}
				OnTypeChangeEvent?.Invoke();
			}
		}
		if (listOpen && !CanOpenList)
		{
			CloseList();
		}
		if (!CharacterInputControl.GetChangeBulletTypeWasPressed())
		{
			return;
		}
		if (!listOpen)
		{
			OpenList();
			return;
		}
		if (selectIndex < selectionsHUD.Count && selectionsHUD[selectIndex] != null)
		{
			SetBulletType(selectionsHUD[selectIndex].BulletTypeID);
		}
		CloseList();
	}

	private void OnHoldAgentChanged(DuckovItemAgent newAgent)
	{
		if (newAgent == null)
		{
			gunAgent = null;
		}
		gunAgent = newAgent as ItemAgent_Gun;
		CloseList();
	}

	private void OnSwitchInput(int dir)
	{
		if (listOpen)
		{
			selectIndex -= dir;
			if (totalSelctionCount == 0)
			{
				selectIndex = 0;
			}
			else if (selectIndex >= totalSelctionCount)
			{
				selectIndex = 0;
			}
			else if (selectIndex < 0)
			{
				selectIndex = totalSelctionCount - 1;
			}
			for (int i = 0; i < selectionsHUD.Count; i++)
			{
				selectionsHUD[i].SetSelection(i == selectIndex);
			}
		}
	}

	private void OpenList()
	{
		Debug.Log("OpenList");
		if (CanOpenList && !listOpen)
		{
			typeList.SetActive(value: true);
			listOpen = true;
			indicator.SetActive(value: false);
			RefreshContent();
		}
	}

	public void CloseList()
	{
		if (listOpen)
		{
			typeList.SetActive(value: false);
			listOpen = false;
			indicator.SetActive(value: true);
		}
	}

	private void RefreshContent()
	{
		selectionsHUD.Clear();
		Selections.ReleaseAll();
		Dictionary<int, BulletTypeInfo> dictionary = new Dictionary<int, BulletTypeInfo>();
		ItemSetting_Gun gunItemSetting = gunAgent.GunItemSetting;
		if (gunItemSetting != null)
		{
			dictionary = gunItemSetting.GetBulletTypesInInventory(characterMainControl.CharacterItem.Inventory);
		}
		if (bulletTpyeID > 0 && !dictionary.ContainsKey(bulletTpyeID))
		{
			BulletTypeInfo bulletTypeInfo = new BulletTypeInfo();
			bulletTypeInfo.bulletTypeID = bulletTpyeID;
			bulletTypeInfo.count = 0;
			dictionary.Add(bulletTpyeID, bulletTypeInfo);
		}
		if (dictionary.Count <= 0)
		{
			BulletTypeInfo bulletTypeInfo2 = new BulletTypeInfo();
			bulletTypeInfo2.bulletTypeID = -1;
			bulletTypeInfo2.count = 0;
			dictionary.Add(-1, bulletTypeInfo2);
		}
		totalSelctionCount = dictionary.Count;
		int num = 0;
		selectIndex = 0;
		foreach (KeyValuePair<int, BulletTypeInfo> item in dictionary)
		{
			BulletTypeSelectButton bulletTypeSelectButton = Selections.Get(typeList.transform);
			bulletTypeSelectButton.gameObject.SetActive(value: true);
			bulletTypeSelectButton.transform.SetAsLastSibling();
			bulletTypeSelectButton.Init(item.Value.bulletTypeID, item.Value.count);
			if (bulletTpyeID == item.Value.bulletTypeID)
			{
				bulletTypeSelectButton.SetSelection(selected: true);
				selectIndex = num;
			}
			selectionsHUD.Add(bulletTypeSelectButton);
			Debug.Log($"BUlletType {selectIndex}:{item.Value.bulletTypeID}");
			num++;
		}
	}

	public void SetBulletType(int typeID)
	{
		CloseList();
		if ((bool)gunAgent && (bool)gunAgent.GunItemSetting)
		{
			bool num = gunAgent.GunItemSetting.TargetBulletID != typeID;
			gunAgent.GunItemSetting.SetTargetBulletType(typeID);
			if (num)
			{
				characterMainControl.TryToReload();
			}
		}
	}
}
