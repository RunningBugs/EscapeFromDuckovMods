using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.Options;
using Duckov.UI;
using Duckov.UI.DialogueBubbles;
using Duckov.Utilities;
using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
	public enum InputDevices
	{
		mouseKeyboard,
		touch
	}

	private static InputDevices inputDevice = InputDevices.mouseKeyboard;

	public CharacterMainControl characterMainControl;

	public AimTargetFinder aimTargetFinder;

	public float runThreshold = 0.85f;

	private Vector3 worldMoveInput;

	public static Action OnInteractButtonDown;

	private Transform aimTargetCol;

	private LayerMask obsticleLayers;

	private RaycastHit[] obsticleHits;

	private RaycastHit hittedCharacterDmgReceiverInfo;

	private RaycastHit hittedObsticleInfo;

	private RaycastHit hittedHead;

	private LayerMask aimCheckLayers;

	private CharacterMainControl foundCharacter;

	public static readonly int PrimaryWeaponSlotHash = "PrimaryWeapon".GetHashCode();

	public static readonly int SecondaryWeaponSlotHash = "SecondaryWeapon".GetHashCode();

	public static readonly int MeleeWeaponSlotHash = "MeleeWeapon".GetHashCode();

	private Camera mainCam;

	private float checkGunDurabilityCoolTimer;

	private float checkGunDurabilityCoolTime = 2f;

	private Transform aimTarget;

	private Vector2 joystickAxisInput;

	private Vector2 moveAxisInput;

	private Vector2 aimScreenPoint;

	private Vector3 inputAimPoint;

	public static bool useRunInputBuffer = false;

	private HashSet<GameObject> blockInputSources = new HashSet<GameObject>();

	private int inputActiveCoolCounter;

	private bool adsInput;

	private bool runInputBuffer;

	private bool runInput;

	private bool runInptutThisFrame;

	private bool newRecoil;

	private ItemAgent_Gun recoilGun;

	private float recoilV;

	private float recoilH;

	private float recoilRecover;

	private bool triggerInput;

	private Vector2 recoilNeedToRecover;

	private Vector2 inputMousePosition;

	private Vector2 _aimMousePosCache;

	private bool aimMousePosFirstSynced;

	private bool cursorVisable = true;

	private bool aimingEnemyHead;

	private bool currentFocus = true;

	private float fovCache = -1f;

	private float _oppositeDelta;

	private float recoilTimer;

	private float recoilTime = 0.04f;

	private float recoilRecoverTime = 0.1f;

	private Vector2 recoilThisShot;

	public static InputDevices InputDevice => inputDevice;

	public Vector3 WorldMoveInput => worldMoveInput;

	public Transform AimTarget => aimTargetCol;

	public Vector2 MoveAxisInput => moveAxisInput;

	public Vector2 AimScreenPoint => aimScreenPoint;

	public Vector3 InputAimPoint => inputAimPoint;

	private static InputManager instance
	{
		get
		{
			if (LevelManager.Instance == null)
			{
				return null;
			}
			return LevelManager.Instance.InputManager;
		}
	}

	public static bool InputActived
	{
		get
		{
			if (!instance)
			{
				return false;
			}
			if (GameManager.Paused)
			{
				return false;
			}
			if (CameraMode.Active)
			{
				return false;
			}
			if (!LevelManager.LevelInited)
			{
				return false;
			}
			if (!CharacterMainControl.Main || CharacterMainControl.Main.Health.IsDead)
			{
				return false;
			}
			return instance.inputActiveCoolCounter <= 0;
		}
	}

	public Vector2 MousePos => inputMousePosition;

	public bool TriggerInput => triggerInput;

	private Vector2 AimMousePosition
	{
		get
		{
			if (!aimMousePosFirstSynced)
			{
				aimMousePosFirstSynced = true;
				if (Mouse.current != null)
				{
					_aimMousePosCache = Mouse.current.position.ReadValue();
				}
			}
			return _aimMousePosCache;
		}
		set
		{
			if (!aimMousePosFirstSynced)
			{
				aimMousePosFirstSynced = true;
				if (Mouse.current != null)
				{
					_aimMousePosCache = Mouse.current.position.ReadValue();
				}
			}
			_aimMousePosCache = value;
		}
	}

	public bool AimingEnemyHead => aimingEnemyHead;

	public static event Action OnInputDeviceChanged;

	public static event Action<int> OnSwitchBulletTypeInput;

	public static event Action<int> OnSwitchWeaponInput;

	private void OnDestroy()
	{
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
	}

	private void Start()
	{
		obsticleHits = new RaycastHit[3];
		obsticleLayers = (int)GameplayDataSettings.Layers.wallLayerMask | (int)GameplayDataSettings.Layers.groundLayerMask;
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		currentFocus = hasFocus;
		if (!currentFocus)
		{
			Cursor.lockState = CursorLockMode.None;
		}
	}

	private void Awake()
	{
		if (blockInputSources == null)
		{
			blockInputSources = new HashSet<GameObject>();
		}
	}

	public static void DisableInput(GameObject source)
	{
		if (!(source == null))
		{
			instance.inputActiveCoolCounter = 2;
			instance.blockInputSources.Add(source);
		}
	}

	public static void ActiveInput(GameObject source)
	{
		if (!(source == null))
		{
			instance.blockInputSources.Remove(source);
		}
	}

	public static void SetInputDevice(InputDevices _inputDevice)
	{
		InputManager.OnInputDeviceChanged?.Invoke();
	}

	private void UpdateCursor()
	{
		if (LevelManager.Instance == null || characterMainControl == null || !characterMainControl.gameObject.activeInHierarchy)
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			return;
		}
		bool flag = !characterMainControl || characterMainControl.Health.IsDead;
		bool flag2 = true;
		if (InputActived && !flag)
		{
			flag2 = false;
		}
		if (CameraMode.Active)
		{
			flag2 = false;
		}
		if (View.ActiveView != null)
		{
			flag2 = true;
		}
		if (!Application.isFocused)
		{
			flag2 = true;
		}
		if (cursorVisable != flag2)
		{
			cursorVisable = !cursorVisable;
		}
		if (cursorVisable)
		{
			recoilNeedToRecover = Vector2.zero;
			if (Mouse.current != null)
			{
				AimMousePosition = Mouse.current.position.ReadValue();
			}
		}
		if (Application.isFocused)
		{
			Cursor.visible = cursorVisable;
		}
		else
		{
			Cursor.visible = true;
		}
		bool flag3 = false;
		if (CameraMode.Active)
		{
			flag3 = true;
		}
		if (currentFocus)
		{
			Cursor.lockState = (flag3 ? CursorLockMode.Locked : CursorLockMode.Confined);
		}
		else
		{
			Cursor.lockState = CursorLockMode.None;
		}
	}

	private void Update()
	{
		if (!characterMainControl)
		{
			return;
		}
		if (!mainCam)
		{
			mainCam = LevelManager.Instance.GameCamera.renderCamera;
			return;
		}
		UpdateInputActived();
		UpdateCursor();
		if (runInput)
		{
			if (runInptutThisFrame)
			{
				runInputBuffer = !runInputBuffer;
			}
		}
		else if (moveAxisInput.magnitude < 0.1f)
		{
			runInputBuffer = false;
		}
		else if (adsInput)
		{
			runInputBuffer = false;
		}
		characterMainControl.SetRunInput(useRunInputBuffer ? runInputBuffer : runInput);
		SetMoveInput(moveAxisInput);
		if (InputDevice == InputDevices.touch)
		{
			UpdateJoystickAim();
			UpdateAimWhileUsingTouch();
		}
		if (checkGunDurabilityCoolTimer <= checkGunDurabilityCoolTime)
		{
			checkGunDurabilityCoolTimer += Time.deltaTime;
		}
		runInptutThisFrame = false;
	}

	private void UpdateInputActived()
	{
		blockInputSources.RemoveWhere((GameObject x) => x == null || !x.activeInHierarchy);
		if (blockInputSources.Count > 0)
		{
			instance.inputActiveCoolCounter = 2;
		}
		else if (instance.inputActiveCoolCounter > 0)
		{
			instance.inputActiveCoolCounter--;
		}
	}

	private void UpdateAimWhileUsingTouch()
	{
	}

	public void SetTrigger(bool trigger, bool triggerThisFrame, bool releaseThisFrame)
	{
		triggerInput = false;
		if (!characterMainControl)
		{
			return;
		}
		if (!InputActived)
		{
			characterMainControl.Trigger(trigger: false, triggerThisFrame: false, releaseThisFrame: false);
			return;
		}
		triggerInput = trigger;
		characterMainControl.Trigger(trigger, triggerThisFrame, releaseThisFrame);
		if (trigger)
		{
			CheckGunDurability();
		}
		if (triggerThisFrame)
		{
			runInputBuffer = false;
			characterMainControl.Attack();
		}
	}

	private void CheckAttack()
	{
		if (InputDevice == InputDevices.touch && (!characterMainControl.CurrentAction || !characterMainControl.CurrentAction.Running))
		{
			ItemAgent_MeleeWeapon meleeWeapon = characterMainControl.GetMeleeWeapon();
			if (!(meleeWeapon == null) && meleeWeapon.AttackableTargetInRange())
			{
				characterMainControl.Attack();
			}
		}
	}

	private void CheckGunDurability()
	{
		if (!(checkGunDurabilityCoolTimer <= checkGunDurabilityCoolTime))
		{
			ItemAgent_Gun gun = characterMainControl.GetGun();
			if (gun != null && gun.Item.Durability <= 0f)
			{
				DialogueBubblesManager.Show("Pop_GunBroken".ToPlainText(), characterMainControl.transform, 2.5f).Forget();
			}
		}
	}

	private Vector3 TrnasAxisInputToWorld(Vector2 axisInput)
	{
		Vector3 zero = Vector3.zero;
		if (!mainCam)
		{
			return zero;
		}
		if (!characterMainControl)
		{
			return zero;
		}
		if (MoveDirectionOptions.MoveViaCharacterDirection)
		{
			Vector3 vector = inputAimPoint - characterMainControl.transform.position;
			vector.y = 0f;
			if (vector.magnitude < 1f)
			{
				return characterMainControl.transform.forward;
			}
			vector.Normalize();
			Vector3 vector2 = Quaternion.Euler(0f, 90f, 0f) * vector;
			return axisInput.x * vector2 + axisInput.y * vector;
		}
		Vector3 right = mainCam.transform.right;
		right.y = 0f;
		right.Normalize();
		Vector3 forward = mainCam.transform.forward;
		forward.y = 0f;
		forward.Normalize();
		return axisInput.x * right + axisInput.y * forward;
	}

	public void SetSwitchBulletTypeInput(int dir)
	{
		if ((bool)characterMainControl && InputActived)
		{
			InputManager.OnSwitchBulletTypeInput?.Invoke(dir);
		}
	}

	public void SetSwitchWeaponInput(int dir)
	{
		if ((bool)characterMainControl && InputActived)
		{
			InputManager.OnSwitchWeaponInput?.Invoke(dir);
			characterMainControl.SwitchWeapon(dir);
		}
	}

	public void SetSwitchInteractInput(int dir)
	{
		if ((bool)characterMainControl && InputActived)
		{
			characterMainControl.SwitchInteractSelection((dir <= 0) ? 1 : (-1));
		}
	}

	public void SetMoveInput(Vector2 axisInput)
	{
		moveAxisInput = axisInput;
		if (!characterMainControl)
		{
			return;
		}
		if (!InputActived)
		{
			characterMainControl.SetMoveInput(Vector3.zero);
			return;
		}
		worldMoveInput = TrnasAxisInputToWorld(axisInput);
		Vector3 normalized = worldMoveInput;
		if (normalized.magnitude > 0.02f)
		{
			normalized = normalized.normalized;
		}
		characterMainControl.SetMoveInput(normalized);
	}

	public void SetRunInput(bool run)
	{
		if ((bool)characterMainControl)
		{
			if (!InputActived)
			{
				runInput = false;
				runInptutThisFrame = false;
				characterMainControl.SetRunInput(_runInput: false);
			}
			else
			{
				runInptutThisFrame = !runInput && run;
				runInput = run;
			}
		}
	}

	public void SetAdsInput(bool ads)
	{
		if ((bool)characterMainControl)
		{
			if (!InputActived)
			{
				characterMainControl.SetAdsInput(_adsInput: false);
				adsInput = false;
			}
			else
			{
				adsInput = ads;
				characterMainControl.SetAdsInput(ads);
			}
		}
	}

	public void ToggleView()
	{
		if ((bool)characterMainControl && InputActived)
		{
			CameraArm.ToggleView();
		}
	}

	public void ToggleNightVision()
	{
		if ((bool)characterMainControl && InputActived)
		{
			characterMainControl.ToggleNightVision();
		}
	}

	public void SetAimInputUsingJoystick(Vector2 _joystickAxisInput)
	{
		if (InputDevice != InputDevices.mouseKeyboard && (bool)characterMainControl)
		{
			if (!InputActived)
			{
				joystickAxisInput = Vector3.zero;
			}
			else
			{
				joystickAxisInput = _joystickAxisInput;
			}
		}
	}

	private void UpdateJoystickAim()
	{
	}

	public void SetAimType(AimTypes aimType)
	{
		if ((bool)characterMainControl && InputActived)
		{
			SkillBase currentRunningSkill = characterMainControl.GetCurrentRunningSkill();
			if (aimType != characterMainControl.AimType && currentRunningSkill != null)
			{
				Debug.Log("skill is running:" + currentRunningSkill.name);
			}
			else
			{
				characterMainControl.SetAimType(aimType);
			}
		}
	}

	public void SetMousePosition(Vector2 mousePosition)
	{
		inputMousePosition = mousePosition;
	}

	public void SetAimInputUsingMouse(Vector2 mouseDelta)
	{
		aimingEnemyHead = false;
		AimMousePosition += mouseDelta * OptionsManager.MouseSensitivity / 10f;
		if (!characterMainControl || !InputActived)
		{
			return;
		}
		ItemAgent_Gun gun = characterMainControl.GetGun();
		if ((bool)gun)
		{
			AimMousePosition = ProcessMousePosViaRecoil(AimMousePosition, mouseDelta, gun);
		}
		Vector2 deltaValue = default(Vector2);
		if (Application.isFocused && InputActived && !Application.isEditor)
		{
			Vector2 mousePosition = AimMousePosition;
			ClampMousePosInWindow(ref mousePosition, ref deltaValue);
			AimMousePosition = mousePosition;
		}
		aimScreenPoint = AimMousePosition;
		characterMainControl.GetCurrentRunningSkill();
		Ray ray = LevelManager.Instance.GameCamera.renderCamera.ScreenPointToRay(aimScreenPoint);
		Plane plane = new Plane(Vector3.up, Vector3.up * (characterMainControl.transform.position.y + 0.5f));
		float enter = 0f;
		plane.Raycast(ray, out enter);
		Vector3 vector = ray.origin + ray.direction * enter;
		Debug.DrawLine(vector, vector + Vector3.up * 3f, Color.yellow);
		Vector3 aimPoint = vector;
		if ((bool)gun && characterMainControl.CanControlAim())
		{
			if (Physics.Raycast(ray, out hittedHead, 100f, 1 << LayerMask.NameToLayer("HeadCollider")))
			{
				aimingEnemyHead = true;
			}
			Vector3 position = characterMainControl.transform.position;
			if ((bool)gun)
			{
				position = gun.muzzle.transform.position;
			}
			Vector3 vector2 = vector - position;
			vector2.y = 0f;
			vector2.Normalize();
			Vector3 axis = Vector3.Cross(vector2, ray.direction);
			aimCheckLayers = GameplayDataSettings.Layers.damageReceiverLayerMask;
			for (int i = 0; (float)i < 45f; i++)
			{
				int num = i;
				if (i > 23)
				{
					num = -(i - 23);
				}
				float num2 = 1.5f;
				Vector3 vector3 = Quaternion.AngleAxis(-2f * (float)num, axis) * vector2;
				Ray ray2 = new Ray(position + num2 * vector3, vector3);
				if (Physics.SphereCast(ray2, 0.02f, out hittedCharacterDmgReceiverInfo, gun.BulletDistance, aimCheckLayers, QueryTriggerInteraction.Ignore) && hittedCharacterDmgReceiverInfo.distance > 0.1f && !Physics.SphereCast(ray2, 0.1f, out hittedObsticleInfo, hittedCharacterDmgReceiverInfo.distance, obsticleLayers, QueryTriggerInteraction.Ignore))
				{
					aimPoint = hittedCharacterDmgReceiverInfo.point;
					break;
				}
			}
		}
		if (aimingEnemyHead)
		{
			Vector3 direction = ray.direction;
			Vector3 rhs = hittedHead.collider.transform.position - hittedHead.point;
			float num3 = Vector3.Dot(direction, rhs);
			aimPoint = hittedHead.point + direction * num3 * 0.5f;
		}
		inputAimPoint = vector;
		characterMainControl.SetAimPoint(aimPoint);
		if (Application.isFocused && currentFocus && InputActived)
		{
			Mouse.current.WarpCursorPosition(AimMousePosition);
		}
	}

	private Vector2 ProcessMousePosViaCameraChange(Vector2 inputMousePos)
	{
		Camera renderCamera = LevelManager.Instance.GameCamera.renderCamera;
		if (fovCache < 0f)
		{
			fovCache = renderCamera.fieldOfView;
			return inputMousePos;
		}
		float fieldOfView = renderCamera.fieldOfView;
		Vector2 vector = new Vector2(inputMousePos.x / (float)Screen.width * 2f - 1f, inputMousePos.y / (float)Screen.height * 2f - 1f);
		float num = Mathf.Tan(fovCache * (MathF.PI / 180f) / 2f) / Mathf.Tan(fieldOfView * (MathF.PI / 180f) / 2f);
		Vector2 vector2 = vector * num;
		Vector2 result = new Vector2((vector2.x + 1f) * 0.5f * (float)Screen.width, (vector2.y + 1f) * 0.5f * (float)Screen.height);
		fovCache = fieldOfView;
		return result;
	}

	private void ClampMousePosInWindow(ref Vector2 mousePosition, ref Vector2 deltaValue)
	{
		Vector2 zero = Vector2.zero;
		zero.x = Mathf.Clamp(mousePosition.x, 0f, Screen.width);
		zero.y = Mathf.Clamp(mousePosition.y, 0f, Screen.height);
		deltaValue = zero - mousePosition;
		mousePosition = zero;
	}

	public void Interact()
	{
		if ((bool)characterMainControl && InputActived)
		{
			characterMainControl.Interact();
			OnInteractButtonDown?.Invoke();
		}
	}

	public void PutAway()
	{
		if ((bool)characterMainControl && InputActived)
		{
			characterMainControl.ChangeHoldItem(null);
		}
	}

	public void SwitchItemAgent(int index)
	{
		if ((bool)characterMainControl && InputActived)
		{
			switch (index)
			{
			case 1:
				characterMainControl.SwitchHoldAgentInSlot(PrimaryWeaponSlotHash);
				break;
			case 2:
				characterMainControl.SwitchHoldAgentInSlot(SecondaryWeaponSlotHash);
				break;
			case 3:
				characterMainControl.SwitchHoldAgentInSlot(MeleeWeaponSlotHash);
				break;
			}
		}
	}

	public void StopAction()
	{
		if (InputActived && (bool)characterMainControl.CurrentAction && characterMainControl.CurrentAction.IsStopable())
		{
			characterMainControl.CurrentAction.StopAction();
		}
	}

	private bool CheckInAimAngleAndNoObsticle()
	{
		if (!characterMainControl)
		{
			return false;
		}
		if (aimTarget == null || characterMainControl.CurrentUsingAimSocket == null)
		{
			return false;
		}
		Vector3 position = characterMainControl.CurrentUsingAimSocket.position;
		position.y = 0f;
		Vector3 position2 = aimTarget.position;
		position2.y = 0f;
		Vector3 vector = position2 - position;
		float magnitude = vector.magnitude;
		vector.Normalize();
		float num = Mathf.Atan(0.25f / magnitude) * 57.29578f;
		if (!(Vector3.Angle(characterMainControl.CurrentAimDirection, vector) < num))
		{
			return false;
		}
		Vector3 vector2 = position + Vector3.up * characterMainControl.CurrentUsingAimSocket.position.y;
		Vector3 vector3 = vector;
		Debug.DrawLine(vector2, vector2 + vector3 * magnitude);
		return Physics.SphereCastNonAlloc(vector2, 0.1f, vector3, obsticleHits, magnitude, obsticleLayers, QueryTriggerInteraction.Ignore) <= 0;
	}

	public void ReleaseItemSkill()
	{
		if (InputActived)
		{
			characterMainControl.ReleaseSkill(SkillTypes.itemSkill);
		}
	}

	public void ReleaseCharacterSkill()
	{
		if (InputActived)
		{
			characterMainControl.ReleaseSkill(SkillTypes.characterSkill);
		}
	}

	public bool CancleSkill()
	{
		if (!characterMainControl)
		{
			return false;
		}
		return characterMainControl.CancleSkill();
	}

	public void Dash()
	{
		if ((bool)characterMainControl && InputActived)
		{
			characterMainControl.TryCatchFishInput();
			characterMainControl.Dash();
		}
	}

	public void StartCharacterSkillAim()
	{
		if ((bool)characterMainControl && InputActived && !(characterMainControl.skillAction.characterSkillKeeper.Skill == null) && characterMainControl.StartSkillAim(SkillTypes.characterSkill) && (bool)characterMainControl.skillAction.CurrentRunningSkill && characterMainControl.skillAction.CurrentRunningSkill.SkillContext.releaseOnStartAim)
		{
			characterMainControl.ReleaseSkill(SkillTypes.characterSkill);
		}
	}

	public void StartItemSkillAim()
	{
		if ((bool)characterMainControl && InputActived && (bool)characterMainControl.agentHolder.Skill && characterMainControl.StartSkillAim(SkillTypes.itemSkill) && (bool)characterMainControl.skillAction.CurrentRunningSkill && characterMainControl.skillAction.CurrentRunningSkill.SkillContext.releaseOnStartAim)
		{
			characterMainControl.ReleaseSkill(SkillTypes.itemSkill);
		}
	}

	public void AddRecoil(ItemAgent_Gun gun)
	{
		if ((bool)gun)
		{
			recoilGun = gun;
			float recoilMultiplier = LevelManager.Rule.RecoilMultiplier;
			recoilV = UnityEngine.Random.Range(gun.RecoilVMin, gun.RecoilVMax) * gun.RecoilScaleV * (1f / gun.CharacterRecoilControl) * recoilMultiplier;
			recoilH = UnityEngine.Random.Range(gun.RecoilHMin, gun.RecoilHMax) * gun.RecoilScaleH * (1f / gun.CharacterRecoilControl) * recoilMultiplier;
			recoilRecover = gun.RecoilRecover;
			recoilTime = Mathf.Min(gun.RecoilTime, 1f / gun.ShootSpeed);
			recoilRecoverTime = gun.RecoilRecoverTime;
			recoilTimer = 0f;
			newRecoil = true;
		}
	}

	private Vector2 ProcessMousePosViaRecoil(Vector2 mousePos, Vector2 mouseDelta, ItemAgent_Gun gun)
	{
		if (!gun || recoilGun != gun)
		{
			newRecoil = false;
			recoilNeedToRecover = Vector2.zero;
			return mousePos;
		}
		Vector3 position = characterMainControl.transform.position;
		if (newRecoil)
		{
			Vector2 vector = LevelManager.Instance.GameCamera.renderCamera.WorldToScreenPoint(position);
			Vector2 normalized = (mousePos - vector).normalized;
			recoilThisShot = normalized * recoilV + recoilH * -Vector2.Perpendicular(normalized);
		}
		Vector3.Distance(InputAimPoint, position);
		float num = Time.deltaTime;
		if (recoilTimer + num >= recoilTime)
		{
			num = recoilTime - recoilTimer;
		}
		if (num > 0f)
		{
			Vector2 vector2 = recoilThisShot * num / recoilTime * Screen.height / 1440f;
			mousePos += vector2;
			recoilNeedToRecover += vector2;
			Vector2 deltaValue = Vector2.zero;
			ClampMousePosInWindow(ref mousePos, ref deltaValue);
			recoilNeedToRecover += deltaValue;
		}
		if (num <= 0f && recoilTimer > recoilRecoverTime && recoilNeedToRecover.magnitude > 0f)
		{
			float num2 = Time.deltaTime;
			if (recoilTimer - num2 < recoilRecoverTime)
			{
				num2 = recoilTimer - recoilRecoverTime;
			}
			Vector2 vector3 = Vector2.MoveTowards(recoilNeedToRecover, Vector2.zero, num2 * recoilRecover * (float)Screen.height / 1440f);
			mousePos += vector3 - recoilNeedToRecover;
			recoilNeedToRecover = vector3;
		}
		float num3 = Vector2.Dot(-recoilNeedToRecover.normalized, mouseDelta);
		if (num3 > 0f)
		{
			_oppositeDelta = 0f;
			recoilNeedToRecover = Vector2.MoveTowards(recoilNeedToRecover, Vector2.zero, num3);
		}
		else
		{
			_oppositeDelta += mouseDelta.magnitude;
			if (_oppositeDelta > 15f * (float)Screen.height / 1440f)
			{
				_oppositeDelta = 0f;
				recoilNeedToRecover = Vector3.zero;
			}
		}
		recoilTimer += Time.deltaTime;
		newRecoil = false;
		return mousePos;
	}
}
