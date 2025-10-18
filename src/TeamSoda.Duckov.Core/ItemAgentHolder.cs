using System;
using ItemStatsSystem;
using UnityEngine;

public class ItemAgentHolder : MonoBehaviour
{
	public CharacterMainControl characterController;

	private DuckovItemAgent currentHoldItemAgent;

	private Transform _currentUsingSocketCache;

	private static int handheldHash = "Handheld".GetHashCode();

	private ItemAgent_Gun _gunRef;

	private ItemAgent_MeleeWeapon _meleeRef;

	private ItemSetting_Skill _skillRef;

	private bool holdStady;

	private float holdStadyTime = 0.15f;

	private float holdStadyTimer;

	public DuckovItemAgent CurrentHoldItemAgent => currentHoldItemAgent;

	public Transform CurrentUsingSocket
	{
		get
		{
			if (!currentHoldItemAgent)
			{
				_currentUsingSocketCache = null;
			}
			return _currentUsingSocketCache;
		}
	}

	public ItemAgent_Gun CurrentHoldGun
	{
		get
		{
			if ((bool)_gunRef && (bool)currentHoldItemAgent && _gunRef.gameObject == currentHoldItemAgent.gameObject)
			{
				return _gunRef;
			}
			_gunRef = null;
			return null;
		}
	}

	public ItemAgent_MeleeWeapon CurrentHoldMeleeWeapon
	{
		get
		{
			if ((bool)_meleeRef && (bool)currentHoldItemAgent && _meleeRef.gameObject == currentHoldItemAgent.gameObject)
			{
				return _meleeRef;
			}
			_meleeRef = null;
			return null;
		}
	}

	public ItemSetting_Skill Skill => _skillRef;

	public event Action<DuckovItemAgent> OnHoldAgentChanged;

	public DuckovItemAgent ChangeHoldItem(Item item)
	{
		DestroyCurrentItemAgent();
		if (item == null)
		{
			this.OnHoldAgentChanged?.Invoke(null);
			return null;
		}
		ItemAgent itemAgent = item.CreateHandheldAgent();
		if (itemAgent == null)
		{
			this.OnHoldAgentChanged?.Invoke(null);
			return null;
		}
		currentHoldItemAgent = itemAgent as DuckovItemAgent;
		if (currentHoldItemAgent == null)
		{
			UnityEngine.Object.Destroy(itemAgent.gameObject);
			this.OnHoldAgentChanged?.Invoke(null);
			return null;
		}
		currentHoldItemAgent.SetHolder(characterController);
		Transform transform;
		switch (currentHoldItemAgent.handheldSocket)
		{
		case HandheldSocketTypes.normalHandheld:
			transform = characterController.characterModel.RightHandSocket;
			break;
		case HandheldSocketTypes.meleeWeapon:
			transform = characterController.characterModel.MeleeWeaponSocket;
			break;
		case HandheldSocketTypes.leftHandSocket:
			transform = characterController.characterModel.LefthandSocket;
			if (transform == null)
			{
				transform = characterController.characterModel.RightHandSocket;
			}
			break;
		default:
			transform = characterController.characterModel.RightHandSocket;
			break;
		}
		currentHoldItemAgent.transform.SetParent(transform, worldPositionStays: false);
		_currentUsingSocketCache = transform;
		currentHoldItemAgent.transform.localPosition = Vector3.zero;
		currentHoldItemAgent.transform.localRotation = Quaternion.identity;
		currentHoldItemAgent.Item.onItemTreeChanged += OnAgentItemTreeChanged;
		_gunRef = currentHoldItemAgent as ItemAgent_Gun;
		_meleeRef = currentHoldItemAgent as ItemAgent_MeleeWeapon;
		if (!IsSkillItem(item))
		{
			_skillRef = null;
		}
		else
		{
			_skillRef = item.GetComponent<ItemSetting_Skill>();
		}
		if ((bool)_skillRef)
		{
			characterController.SetSkill(SkillTypes.itemSkill, _skillRef.Skill, itemAgent.gameObject);
		}
		else
		{
			characterController.SetSkill(SkillTypes.itemSkill, null, null);
		}
		holdStadyTimer = 0f;
		holdStady = false;
		itemAgent.gameObject.SetActive(value: false);
		this.OnHoldAgentChanged?.Invoke(currentHoldItemAgent);
		return currentHoldItemAgent;
	}

	public void SetTrigger(bool trigger, bool triggerThisFrame, bool releaseThisFrame)
	{
		if ((bool)currentHoldItemAgent && characterController.CanUseHand() && CurrentHoldGun != null)
		{
			CurrentHoldGun.SetTrigger(trigger, triggerThisFrame, releaseThisFrame);
		}
	}

	private void OnDestroy()
	{
		if ((bool)currentHoldItemAgent)
		{
			currentHoldItemAgent.Item.onItemTreeChanged -= OnAgentItemTreeChanged;
		}
	}

	private void DestroyCurrentItemAgent()
	{
		_skillRef = null;
		if (!(currentHoldItemAgent == null))
		{
			if (currentHoldItemAgent.Item != null)
			{
				currentHoldItemAgent.Item.onItemTreeChanged -= OnAgentItemTreeChanged;
				currentHoldItemAgent.Item.AgentUtilities.ReleaseActiveAgent();
			}
			currentHoldItemAgent = null;
		}
	}

	private void OnAgentItemTreeChanged(Item item)
	{
		if (item == null || currentHoldItemAgent == null || currentHoldItemAgent.Item != item || item.GetCharacterItem() != characterController.CharacterItem)
		{
			DestroyCurrentItemAgent();
		}
	}

	private bool IsSkillItem(Item item)
	{
		if (item == null)
		{
			return false;
		}
		return item.GetBool("IsSkill");
	}

	private void Update()
	{
		if (currentHoldItemAgent != null && !holdStady)
		{
			holdStadyTimer += Time.deltaTime;
			if (holdStadyTimer > holdStadyTime)
			{
				holdStady = true;
				currentHoldItemAgent.gameObject.SetActive(value: true);
			}
		}
	}
}
