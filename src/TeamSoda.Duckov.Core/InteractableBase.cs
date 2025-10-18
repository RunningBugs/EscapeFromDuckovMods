using System;
using System.Collections.Generic;
using Duckov;
using Duckov.MasterKeys;
using Duckov.Scenes;
using Duckov.Utilities;
using ItemStatsSystem;
using ItemStatsSystem.Items;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.Events;

public class InteractableBase : MonoBehaviour, IProgress
{
	public enum WhenToUseRequireItemTypes
	{
		None,
		OnFinshed,
		OnTimeOut,
		OnStartInteract
	}

	public bool interactableGroup;

	[SerializeField]
	private List<InteractableBase> otherInterablesInGroup;

	public bool zoomIn = true;

	private List<InteractableBase> _interactbleList = new List<InteractableBase>();

	[SerializeField]
	private float interactTime;

	public bool finishWhenTimeOut = true;

	private float interactTimer;

	public Vector3 interactMarkerOffset;

	public bool overrideInteractName;

	[LocalizationKey("Default")]
	private string defaultInteractNameKey = "UI_Interact";

	[LocalizationKey("Interact")]
	public string _overrideInteractNameKey;

	public Collider interactCollider;

	public bool requireItem;

	public bool requireOnce = true;

	[ItemTypeID]
	public int requireItemId;

	public float unlockTime;

	public bool overrideItemUsedKey;

	public string overrideItemUsedSaveKey;

	public WhenToUseRequireItemTypes whenToUseRequireItem;

	public UnityEvent OnRequiredItemUsedEvent;

	private int requireItemDataKeyCached;

	private bool requireItemUsed;

	private ItemMetaData? _cachedMeta;

	public UnityEvent<CharacterMainControl, InteractableBase> OnInteractStartEvent;

	public UnityEvent<CharacterMainControl, InteractableBase> OnInteractTimeoutEvent;

	public UnityEvent<CharacterMainControl, InteractableBase> OnInteractFinishedEvent;

	public bool disableOnFinish;

	public float coolTime;

	private float lastStopTime = -1f;

	protected CharacterMainControl interactCharacter;

	private bool timeOut;

	[SerializeField]
	private bool interactMarkerVisible = true;

	private InteractMarker markerObject;

	public float InteractTime
	{
		get
		{
			if (requireItem && !requireItemUsed)
			{
				return interactTime + unlockTime;
			}
			return interactTime;
		}
	}

	public string InteractName
	{
		get
		{
			if (overrideInteractName)
			{
				return _overrideInteractNameKey.ToPlainText();
			}
			return defaultInteractNameKey.ToPlainText();
		}
		set
		{
			overrideInteractName = true;
			_overrideInteractNameKey = value;
		}
	}

	private bool ShowBaseInteractName
	{
		get
		{
			if (overrideInteractName)
			{
				return ShowBaseInteractNameInspector;
			}
			return false;
		}
	}

	protected virtual bool ShowBaseInteractNameInspector => true;

	private ItemMetaData CachedMeta
	{
		get
		{
			if (!_cachedMeta.HasValue)
			{
				_cachedMeta = ItemAssetsCollection.GetMetaData(requireItemId);
			}
			return _cachedMeta.Value;
		}
	}

	protected virtual bool ShowUnityEvents => true;

	public bool Interacting => interactCharacter != null;

	public bool MarkerActive
	{
		get
		{
			return interactMarkerVisible;
		}
		set
		{
			if (base.enabled)
			{
				interactMarkerVisible = value;
				if (value)
				{
					ActiveMarker();
				}
				else if ((bool)markerObject)
				{
					markerObject.gameObject.SetActive(value: false);
				}
			}
		}
	}

	public static event Action<InteractableBase> OnInteractStartStaticEvent;

	public List<InteractableBase> GetInteractableList()
	{
		_interactbleList.Clear();
		_interactbleList.Add(this);
		if (!interactableGroup || otherInterablesInGroup.Count <= 0)
		{
			return _interactbleList;
		}
		foreach (InteractableBase item in otherInterablesInGroup)
		{
			if (!(item == null) && item.gameObject.activeInHierarchy)
			{
				_interactbleList.Add(item);
			}
		}
		return _interactbleList;
	}

	protected virtual void Awake()
	{
		requireItemDataKeyCached = GetKey();
		if (interactCollider == null)
		{
			interactCollider = GetComponent<Collider>();
			if (interactCollider == null)
			{
				interactCollider = base.gameObject.AddComponent<BoxCollider>();
				interactCollider.enabled = false;
			}
		}
		if (interactCollider != null)
		{
			interactCollider.gameObject.layer = LayerMask.NameToLayer("Interactable");
		}
		foreach (InteractableBase item in otherInterablesInGroup)
		{
			if ((bool)item)
			{
				item.MarkerActive = false;
				item.transform.position = base.transform.position;
				item.transform.rotation = base.transform.rotation;
				item.interactMarkerOffset = interactMarkerOffset;
			}
		}
		_interactbleList = new List<InteractableBase>();
	}

	protected virtual void Start()
	{
		if (requireItem && (bool)MultiSceneCore.Instance && MultiSceneCore.Instance.inLevelData.TryGetValue(requireItemDataKeyCached, out var value) && value is bool && (bool)value)
		{
			requireItem = false;
			requireItemUsed = true;
			OnRequiredItemUsedEvent?.Invoke();
		}
		MarkerActive = interactMarkerVisible;
	}

	private void ActiveMarker()
	{
		if ((bool)markerObject)
		{
			if (!markerObject.gameObject.activeInHierarchy)
			{
				markerObject.gameObject.SetActive(value: true);
			}
		}
		else
		{
			markerObject = UnityEngine.Object.Instantiate(GameplayDataSettings.Prefabs.InteractMarker, base.transform);
			markerObject.transform.localPosition = interactMarkerOffset;
			CheckInteractable();
		}
	}

	public void SetMarkerUsed()
	{
		if ((bool)markerObject)
		{
			markerObject.MarkAsUsed();
		}
	}

	public bool StartInteract(CharacterMainControl _interactCharacter)
	{
		if (!_interactCharacter)
		{
			return false;
		}
		if (requireItem && !TryGetRequiredItem(_interactCharacter).hasItem)
		{
			return false;
		}
		if (interactCharacter == _interactCharacter)
		{
			return false;
		}
		if (!CheckInteractable())
		{
			return false;
		}
		if (requireItem && whenToUseRequireItem == WhenToUseRequireItemTypes.OnStartInteract && !UseRequiredItem(_interactCharacter))
		{
			StopInteract();
			return false;
		}
		interactCharacter = _interactCharacter;
		interactTimer = 0f;
		timeOut = false;
		OnInteractStartEvent?.Invoke(_interactCharacter, this);
		InteractableBase.OnInteractStartStaticEvent?.Invoke(this);
		try
		{
			OnInteractStart(_interactCharacter);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			if ((bool)CharacterMainControl.Main)
			{
				CharacterMainControl.Main.PopText("OnInteractStart开始失败，Log Error");
			}
			return false;
		}
		return true;
	}

	public InteractableBase GetInteractableInGroup(int index)
	{
		if (index == 0)
		{
			return this;
		}
		List<InteractableBase> interactableList = GetInteractableList();
		if (index >= interactableList.Count)
		{
			return null;
		}
		return interactableList[index];
	}

	public void InternalStopInteract()
	{
		interactCharacter = null;
		lastStopTime = Time.time;
		OnInteractStop();
	}

	public void StopInteract()
	{
		CharacterMainControl characterMainControl = interactCharacter;
		if ((bool)characterMainControl && characterMainControl.interactAction.Running && characterMainControl.interactAction.InteractingTarget == this)
		{
			interactCharacter.interactAction.StopAction();
		}
		else
		{
			InternalStopInteract();
		}
	}

	public void UpdateInteract(CharacterMainControl _interactCharacter, float deltaTime)
	{
		interactTimer += deltaTime;
		OnUpdate(_interactCharacter, deltaTime);
		if (timeOut || !(interactTimer >= InteractTime))
		{
			return;
		}
		if (requireItem && whenToUseRequireItem == WhenToUseRequireItemTypes.OnTimeOut && !UseRequiredItem(_interactCharacter))
		{
			StopInteract();
			return;
		}
		if (requireItem && whenToUseRequireItem == WhenToUseRequireItemTypes.None && !requireItemUsed)
		{
			requireItemUsed = true;
			OnRequiredItemUsedEvent?.Invoke();
			if ((bool)MultiSceneCore.Instance)
			{
				MultiSceneCore.Instance.inLevelData[requireItemDataKeyCached] = true;
				Debug.Log("设置使用过物品为true");
			}
		}
		timeOut = true;
		OnTimeOut();
		OnInteractTimeoutEvent?.Invoke(_interactCharacter, this);
		if (finishWhenTimeOut)
		{
			FinishInteract(_interactCharacter);
		}
	}

	public void FinishInteract(CharacterMainControl _interactCharacter)
	{
		if (requireItem && whenToUseRequireItem == WhenToUseRequireItemTypes.OnFinshed && !UseRequiredItem(_interactCharacter))
		{
			StopInteract();
			return;
		}
		try
		{
			OnInteractFinished();
			OnInteractFinishedEvent?.Invoke(_interactCharacter, this);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		StopInteract();
		if (disableOnFinish)
		{
			base.enabled = false;
			if ((bool)markerObject)
			{
				markerObject.gameObject.SetActive(value: false);
			}
			if ((bool)interactCollider)
			{
				interactCollider.enabled = false;
			}
		}
	}

	protected virtual void OnUpdate(CharacterMainControl _interactCharacter, float deltaTime)
	{
	}

	protected virtual void OnTimeOut()
	{
	}

	private bool UseRequiredItem(CharacterMainControl interactCharacter)
	{
		Debug.Log("尝试使用");
		(bool, Item) tuple = TryGetRequiredItem(interactCharacter);
		Item item = tuple.Item2;
		if (!tuple.Item1 || tuple.Item2 == null)
		{
			return false;
		}
		if (item.UseDurability)
		{
			Debug.Log("尝试消耗耐久");
			item.Durability -= 1f;
			if (item.Durability <= 0f)
			{
				item.Detach();
				item.DestroyTree();
			}
		}
		else if (!item.Stackable)
		{
			Debug.Log("尝试直接消耗掉");
			item.Detach();
			item.DestroyTree();
		}
		else
		{
			Debug.Log("尝试消耗堆叠");
			item.StackCount--;
		}
		if (requireOnce)
		{
			requireItem = false;
			requireItemUsed = true;
			OnRequiredItemUsedEvent?.Invoke();
			if ((bool)MultiSceneCore.Instance)
			{
				MultiSceneCore.Instance.inLevelData[requireItemDataKeyCached] = true;
				Debug.Log("设置使用过物品为true");
			}
		}
		return true;
	}

	public bool CheckInteractable()
	{
		if (interactCharacter != null)
		{
			if (!(interactCharacter.interactAction.InteractingTarget != this))
			{
				return false;
			}
			StopInteract();
		}
		if (Time.time - lastStopTime < coolTime && coolTime > 0f && lastStopTime > 0f)
		{
			return false;
		}
		return IsInteractable();
	}

	protected virtual bool IsInteractable()
	{
		return true;
	}

	protected virtual void OnInteractStart(CharacterMainControl interactCharacter)
	{
	}

	protected virtual void OnInteractStop()
	{
	}

	protected virtual void OnInteractFinished()
	{
	}

	public string GetInteractName()
	{
		if (overrideInteractName)
		{
			return InteractName;
		}
		return "UI_Interact".ToPlainText();
	}

	public string GetRequiredItemName()
	{
		if (!requireItem)
		{
			return null;
		}
		return CachedMeta.DisplayName;
	}

	public Sprite GetRequireditemIcon()
	{
		if (!requireItem)
		{
			return null;
		}
		return CachedMeta.icon;
	}

	protected virtual void OnDestroy()
	{
		if (Interacting)
		{
			StopInteract();
		}
	}

	public virtual Progress GetProgress()
	{
		Progress result = default(Progress);
		if (Interacting && InteractTime > 0f)
		{
			result.inProgress = true;
			result.total = InteractTime;
			result.current = interactTimer;
		}
		else
		{
			result.inProgress = false;
		}
		return result;
	}

	public (bool hasItem, Item ItemInstance) TryGetRequiredItem(CharacterMainControl fromCharacter)
	{
		if (!requireItem)
		{
			return (hasItem: false, ItemInstance: null);
		}
		if (!fromCharacter)
		{
			return (hasItem: false, ItemInstance: null);
		}
		if (MasterKeysManager.IsActive(requireItemId))
		{
			return (hasItem: true, ItemInstance: null);
		}
		foreach (Slot slot in fromCharacter.CharacterItem.Slots)
		{
			if ((bool)slot.Content && slot.Content.TypeID == requireItemId)
			{
				return (hasItem: true, ItemInstance: slot.Content);
			}
		}
		foreach (Item item in fromCharacter.CharacterItem.Inventory)
		{
			if (item.TypeID == requireItemId)
			{
				return (hasItem: true, ItemInstance: item);
			}
			if (!(item.Slots != null) || item.Slots.Count <= 0)
			{
				continue;
			}
			foreach (Slot slot2 in item.Slots)
			{
				if (slot2.Content != null && slot2.Content.TypeID == requireItemId)
				{
					return (hasItem: true, ItemInstance: slot2.Content);
				}
			}
		}
		foreach (Item item2 in LevelManager.Instance.PetProxy.Inventory)
		{
			if (item2.TypeID == requireItemId)
			{
				return (hasItem: true, ItemInstance: item2);
			}
			if (!item2.Slots || item2.Slots.Count <= 0)
			{
				continue;
			}
			foreach (Slot slot3 in item2.Slots)
			{
				if (slot3.Content != null && slot3.Content.TypeID == requireItemId)
				{
					return (hasItem: true, ItemInstance: slot3.Content);
				}
			}
		}
		return (hasItem: false, ItemInstance: null);
	}

	private int GetKey()
	{
		if (overrideItemUsedKey)
		{
			return overrideItemUsedSaveKey.GetHashCode();
		}
		Vector3 vector = base.transform.position * 10f;
		int x = Mathf.RoundToInt(vector.x);
		int y = Mathf.RoundToInt(vector.y);
		int z = Mathf.RoundToInt(vector.z);
		Vector3Int vector3Int = new Vector3Int(x, y, z);
		return $"Intact_{vector3Int}".GetHashCode();
	}

	public void InteractWithMainCharacter()
	{
		CharacterMainControl.Main?.Interact(this);
	}

	private void OnDrawGizmos()
	{
		if (interactMarkerVisible)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(base.transform.TransformPoint(interactMarkerOffset), 0.1f);
		}
	}
}
