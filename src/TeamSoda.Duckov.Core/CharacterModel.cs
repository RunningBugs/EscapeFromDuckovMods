using System;
using System.Collections.Generic;
using ItemStatsSystem.Items;
using UnityEngine;

public class CharacterModel : MonoBehaviour
{
	public CharacterMainControl characterMainControl;

	public bool invisable;

	[SerializeField]
	private Transform lefthandSocket;

	[SerializeField]
	private Transform rightHandSocket;

	private Quaternion defaultRightHandLocalRotation;

	[SerializeField]
	private HurtVisual hurtVisual;

	[SerializeField]
	private Transform armorSocket;

	[SerializeField]
	private Transform helmatSocket;

	[SerializeField]
	private Transform faceSocket;

	[SerializeField]
	private Transform backpackSocket;

	[SerializeField]
	private Transform meleeWeaponSocket;

	[SerializeField]
	private Transform popTextSocket;

	[SerializeField]
	private List<CharacterSubVisuals> subVisuals;

	[SerializeField]
	private List<Renderer> renderers;

	[SerializeField]
	private CustomFaceInstance customFace;

	public bool autoSyncRightHandRotation = true;

	public float damageReceiverRadius = 0.45f;

	private int showHairHash = "ShowHair".GetHashCode();

	private int showMouthHash = "ShowMouth".GetHashCode();

	private bool helmatShowMouth = true;

	private bool helmatShowHair = true;

	private bool faceMaskShowHair = true;

	private bool faceMaskShowMouth = true;

	private bool destroied;

	public Transform LefthandSocket => lefthandSocket;

	public Transform RightHandSocket => rightHandSocket;

	public Transform ArmorSocket => armorSocket;

	public Transform HelmatSocket => helmatSocket;

	public Transform FaceMaskSocket
	{
		get
		{
			if ((bool)faceSocket)
			{
				return faceSocket;
			}
			return helmatSocket;
		}
	}

	public Transform BackpackSocket => backpackSocket;

	public Transform MeleeWeaponSocket => meleeWeaponSocket;

	public Transform PopTextSocket => popTextSocket;

	public CustomFaceInstance CustomFace => customFace;

	public bool Hidden => characterMainControl.Hidden;

	public event Action<CharacterModel> OnDestroyEvent;

	public event Action OnCharacterSetEvent;

	public event Action OnAttackOrShootEvent;

	private void Awake()
	{
		defaultRightHandLocalRotation = rightHandSocket.localRotation;
	}

	private void Start()
	{
		CharacterSubVisuals component = GetComponent<CharacterSubVisuals>();
		if (component != null)
		{
			if (subVisuals.Contains(component))
			{
				RemoveVisual(component);
			}
			AddSubVisuals(component);
		}
	}

	private void LateUpdate()
	{
		if (autoSyncRightHandRotation)
		{
			SyncRightHandRotation();
		}
	}

	public void OnMainCharacterSetted(CharacterMainControl _characterMainControl)
	{
		characterMainControl = _characterMainControl;
		if (!characterMainControl)
		{
			return;
		}
		if ((bool)characterMainControl.attackAction)
		{
			characterMainControl.attackAction.OnAttack += OnAttack;
		}
		characterMainControl.OnShootEvent += OnShoot;
		characterMainControl.EquipmentController.OnHelmatSlotContentChanged += OnHelmatSlotContentChange;
		characterMainControl.EquipmentController.OnFaceMaskSlotContentChanged += OnFaceMaskSlotContentChange;
		if (_characterMainControl.mainDamageReceiver != null)
		{
			CapsuleCollider component = _characterMainControl.mainDamageReceiver.GetComponent<CapsuleCollider>();
			if (component != null)
			{
				component.radius = damageReceiverRadius;
				if (damageReceiverRadius <= 0f)
				{
					component.enabled = false;
				}
			}
		}
		this.OnCharacterSetEvent?.Invoke();
		hurtVisual.SetHealth(_characterMainControl.Health);
	}

	private void CharacterMainControl_OnShootEvent(DuckovItemAgent obj)
	{
		throw new NotImplementedException();
	}

	private void OnHelmatSlotContentChange(Slot slot)
	{
		if (slot != null)
		{
			helmatShowHair = slot.Content == null || slot.Content.Constants.GetBool(showHairHash);
			helmatShowMouth = slot.Content == null || slot.Content.Constants.GetBool(showMouthHash, defaultResult: true);
			if ((bool)customFace && (bool)customFace.hairSocket)
			{
				customFace.hairSocket.gameObject.SetActive(helmatShowHair && faceMaskShowHair);
			}
			if ((bool)customFace && (bool)customFace.mouthPart.socket)
			{
				customFace.mouthPart.socket.gameObject.SetActive(helmatShowMouth && faceMaskShowMouth);
			}
		}
	}

	private void OnFaceMaskSlotContentChange(Slot slot)
	{
		if (slot != null)
		{
			faceMaskShowHair = slot.Content == null || slot.Content.Constants.GetBool(showHairHash, defaultResult: true);
			faceMaskShowMouth = slot.Content == null || slot.Content.Constants.GetBool(showMouthHash, defaultResult: true);
			if ((bool)customFace && (bool)customFace.hairSocket)
			{
				customFace.hairSocket.gameObject.SetActive(helmatShowHair && faceMaskShowHair);
			}
			if ((bool)customFace && (bool)customFace.mouthPart.socket)
			{
				customFace.mouthPart.socket.gameObject.SetActive(helmatShowMouth && faceMaskShowMouth);
			}
		}
	}

	private void OnDestroy()
	{
		if (destroied)
		{
			return;
		}
		destroied = true;
		this.OnDestroyEvent?.Invoke(this);
		if ((bool)characterMainControl)
		{
			if ((bool)characterMainControl.attackAction)
			{
				characterMainControl.attackAction.OnAttack -= OnAttack;
			}
			characterMainControl.OnShootEvent -= OnShoot;
			characterMainControl.EquipmentController.OnHelmatSlotContentChanged -= OnHelmatSlotContentChange;
			characterMainControl.EquipmentController.OnFaceMaskSlotContentChanged -= OnFaceMaskSlotContentChange;
		}
	}

	private void SyncRightHandRotation()
	{
		if ((bool)characterMainControl)
		{
			bool flag = true;
			if (characterMainControl.Running)
			{
				flag = false;
			}
			Quaternion to = ((!flag) ? (rightHandSocket.parent.transform.rotation * defaultRightHandLocalRotation) : Quaternion.LookRotation(characterMainControl.CurrentAimDirection, Vector3.up));
			float maxDegreesDelta = 999f;
			if (0 == 0)
			{
				maxDegreesDelta = 360f * Time.deltaTime;
			}
			rightHandSocket.rotation = Quaternion.RotateTowards(rightHandSocket.rotation, to, maxDegreesDelta);
		}
	}

	public void AddSubVisuals(CharacterSubVisuals visuals)
	{
		visuals.mainModel = this;
		if (!subVisuals.Contains(visuals))
		{
			subVisuals.Add(visuals);
			renderers.AddRange(visuals.renderers);
			hurtVisual.SetRenderers(renderers);
			visuals.SetRenderersHidden(Hidden);
		}
	}

	public void RemoveVisual(CharacterSubVisuals _subVisuals)
	{
		subVisuals.Remove(_subVisuals);
		foreach (Renderer renderer in _subVisuals.renderers)
		{
			renderers.Remove(renderer);
		}
		hurtVisual.SetRenderers(renderers);
	}

	public void SyncHiddenToMainCharacter()
	{
		bool renderersHidden = Hidden;
		if (!Team.IsEnemy(Teams.player, characterMainControl.Team))
		{
			renderersHidden = false;
		}
		if (subVisuals.Count <= 0)
		{
			return;
		}
		foreach (CharacterSubVisuals subVisual in subVisuals)
		{
			if (!(subVisual == null))
			{
				subVisual.SetRenderersHidden(renderersHidden);
			}
		}
	}

	public void SetFaceFromPreset(CustomFacePreset preset)
	{
		if (!(preset == null) && (bool)customFace)
		{
			customFace.LoadFromData(preset.settings);
		}
	}

	public void SetFaceFromData(CustomFaceSettingData data)
	{
		if ((bool)customFace)
		{
			customFace.LoadFromData(data);
		}
	}

	private void OnAttack()
	{
		this.OnAttackOrShootEvent?.Invoke();
	}

	public void ForcePlayAttackAnimation()
	{
		OnAttack();
	}

	private void OnShoot(DuckovItemAgent agent)
	{
		this.OnAttackOrShootEvent?.Invoke();
	}
}
