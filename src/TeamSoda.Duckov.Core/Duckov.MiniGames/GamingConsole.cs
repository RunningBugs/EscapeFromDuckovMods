using System;
using System.Collections.Generic;
using Cinemachine;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using ItemStatsSystem;
using ItemStatsSystem.Data;
using ItemStatsSystem.Items;
using Saves;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.MiniGames;

public class GamingConsole : InteractableBase
{
	[Serializable]
	private class SaveData
	{
		public ItemTreeData monitorData;

		public ItemTreeData consoleData;
	}

	[SerializeField]
	private List<MiniGame> possibleGames;

	[SerializeField]
	private RenderTexture rt;

	[SerializeField]
	private MiniGameInputHandler inputHandler;

	[SerializeField]
	private CinemachineVirtualCamera virtualCamera;

	[SerializeField]
	private float transitionTime = 1f;

	[SerializeField]
	private Transform vcamEndPosition;

	[SerializeField]
	private Transform vcamLookTarget;

	[SerializeField]
	private AnimationCurve posCurve;

	[SerializeField]
	private AnimationCurve rotCurve;

	[SerializeField]
	private AnimationCurve fovCurve;

	[SerializeField]
	private float activeFov = 45f;

	[SerializeField]
	private Transform teleportToPositionWhenBegin;

	[SerializeField]
	private Item mainItem;

	[SerializeField]
	[LocalizationKey("Default")]
	private string incompleteNotificationText = "GamingConsole_Incomplete";

	[SerializeField]
	[LocalizationKey("Default")]
	private string noGameNotificationText = "GamingConsole_NoGame";

	private MiniGame game;

	private string SaveKey = "GamingConsoleData";

	private bool loading;

	private bool loaded;

	private bool isBeingDestroyed;

	private int animateToken;

	public MiniGame SelectedGame
	{
		get
		{
			if (CatridgeGameID == null)
			{
				return null;
			}
			return possibleGames.Find((MiniGame e) => e != null && e.ID == CatridgeGameID);
		}
	}

	public MiniGame Game => game;

	public Slot MonitorSlot => mainItem.Slots["Monitor"];

	public Slot ConsoleSlot => mainItem.Slots["Console"];

	public bool controllerConnected
	{
		get
		{
			if (mainItem == null)
			{
				return false;
			}
			if (ConsoleSlot == null)
			{
				return false;
			}
			Item content = ConsoleSlot.Content;
			if (content == null)
			{
				return false;
			}
			Slot slot = content.Slots["FcController"];
			if (slot == null)
			{
				return false;
			}
			return slot.Content != null;
		}
	}

	public Item Monitor
	{
		get
		{
			if (MonitorSlot == null)
			{
				return null;
			}
			return MonitorSlot.Content;
		}
	}

	public Item Console
	{
		get
		{
			if (ConsoleSlot == null)
			{
				return null;
			}
			return ConsoleSlot.Content;
		}
	}

	public Item Cartridge
	{
		get
		{
			if (Console == null)
			{
				return null;
			}
			if (!Console.Slots)
			{
				Debug.LogError(Console.DisplayName + " has no catridge slot");
				return null;
			}
			Slot slot = Console.Slots["Cartridge"];
			if (slot == null)
			{
				Debug.LogError(Console.DisplayName + " has no catridge slot");
				return null;
			}
			return slot.Content;
		}
	}

	public string CatridgeGameID
	{
		get
		{
			if (Cartridge == null)
			{
				return null;
			}
			return Cartridge.Constants.GetString("GameID");
		}
	}

	public event Action<GamingConsole> onContentChanged;

	public event Action<GamingConsole> OnAfterAnimateIn;

	public event Action<GamingConsole> OnBeforeAnimateOut;

	public static event Action<bool> OnGamingConsoleInteractChanged;

	private async UniTask Load()
	{
		if (loading)
		{
			Debug.LogError("Component is loading in progress, aborting.");
			return;
		}
		while (!LevelManager.LevelInited)
		{
			await UniTask.Yield();
		}
		SaveData data = SavesSystem.Load<SaveData>(SaveKey);
		if (data == null)
		{
			loaded = true;
			return;
		}
		if (data.monitorData != null)
		{
			Item item = await ItemTreeData.InstantiateAsync(data.monitorData);
			if (item != null)
			{
				if (!MonitorSlot.Plug(item, out var unpluggedItem))
				{
					ItemUtilities.SendToPlayer(item);
				}
				if (unpluggedItem != null)
				{
					unpluggedItem.DestroyTree();
				}
			}
		}
		if (data.consoleData != null)
		{
			Item item2 = await ItemTreeData.InstantiateAsync(data.consoleData);
			if (item2 != null)
			{
				if (!ConsoleSlot.Plug(item2, out var unpluggedItem2))
				{
					ItemUtilities.SendToPlayer(item2);
				}
				if (unpluggedItem2 != null)
				{
					unpluggedItem2.DestroyTree();
				}
			}
		}
		loading = false;
		loaded = true;
		this.onContentChanged?.Invoke(this);
	}

	private void Save()
	{
		if (!loading && loaded)
		{
			SaveData saveData = new SaveData();
			if (Console != null)
			{
				saveData.consoleData = ItemTreeData.FromItem(Console);
			}
			if (Monitor != null)
			{
				saveData.monitorData = ItemTreeData.FromItem(Monitor);
			}
			SavesSystem.Save(SaveKey, saveData);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		UIInputManager.OnCancel += OnUICancel;
		SavesSystem.OnCollectSaveData += Save;
		inputHandler.enabled = false;
		mainItem.onItemTreeChanged += OnContentChanged;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		GamingConsole.OnGamingConsoleInteractChanged?.Invoke(obj: false);
		UIInputManager.OnCancel -= OnUICancel;
		SavesSystem.OnCollectSaveData -= Save;
		isBeingDestroyed = true;
	}

	private void OnDisable()
	{
		GamingConsole.OnGamingConsoleInteractChanged?.Invoke(obj: false);
	}

	protected override void Start()
	{
		base.Start();
		Load().Forget();
	}

	private void OnContentChanged(Item item)
	{
		this.onContentChanged?.Invoke(this);
		RefreshGame();
	}

	private void OnUICancel(UIInputEventData data)
	{
		if (!data.Used && base.Interacting)
		{
			StopInteract();
			data.Use();
		}
	}

	protected override void OnInteractStart(CharacterMainControl interactCharacter)
	{
		base.OnInteractStart(interactCharacter);
		GamingConsole.OnGamingConsoleInteractChanged?.Invoke(this);
		if (Console == null || Monitor == null || Cartridge == null)
		{
			NotificationText.Push(incompleteNotificationText.ToPlainText());
			StopInteract();
			return;
		}
		if (SelectedGame == null)
		{
			NotificationText.Push(noGameNotificationText.ToPlainText());
			StopInteract();
			return;
		}
		RefreshGame();
		inputHandler.enabled = controllerConnected;
		AnimateCameraIn().Forget();
		HUDManager.RegisterHideToken(this);
		CharacterMainControl.Main.SetPosition(teleportToPositionWhenBegin.position);
		GamingConsoleHUD.Show();
	}

	private async UniTask AnimateCameraIn()
	{
		int token = (animateToken = UnityEngine.Random.Range(0, int.MaxValue));
		Vector3 toPos = vcamEndPosition.position;
		Quaternion rotation = vcamEndPosition.rotation;
		float toFov = activeFov;
		if (!(GameCamera.Instance == null) && !(GameCamera.Instance.mainVCam == null))
		{
			CinemachineVirtualCamera mainVCam = GameCamera.Instance.mainVCam;
			Vector3 fromPos = mainVCam.transform.position;
			Quaternion fromRot = mainVCam.transform.rotation;
			float fromFov = mainVCam.m_Lens.FieldOfView;
			virtualCamera.transform.position = fromPos;
			virtualCamera.transform.rotation = fromRot;
			virtualCamera.Priority = 10;
			float time = 0f;
			while (time < transitionTime)
			{
				time += Time.deltaTime;
				float time2 = time / transitionTime;
				float t = posCurve.Evaluate(time2);
				float t2 = rotCurve.Evaluate(time2);
				float t3 = fovCurve.Evaluate(time2);
				Vector3 vector = Vector3.Lerp(fromPos, toPos, t);
				rotation = Quaternion.LookRotation(vcamLookTarget.position - vector, Vector3.up);
				Quaternion rotation2 = Quaternion.Lerp(fromRot, rotation, t2);
				float fieldOfView = Mathf.Lerp(fromFov, toFov, t3);
				virtualCamera.transform.SetPositionAndRotation(vector, rotation2);
				virtualCamera.m_Lens.FieldOfView = fieldOfView;
				await UniTask.Yield();
				if (animateToken != token)
				{
					return;
				}
			}
			rotation = Quaternion.LookRotation(vcamLookTarget.position - toPos, Vector3.up);
		}
		virtualCamera.transform.SetPositionAndRotation(toPos, rotation);
		virtualCamera.m_Lens.FieldOfView = toFov;
		this.OnAfterAnimateIn?.Invoke(this);
	}

	private async UniTask AnimateCameraOut()
	{
		this.OnBeforeAnimateOut?.Invoke(this);
		int token = (animateToken = UnityEngine.Random.Range(0, int.MaxValue));
		GameCamera instance = GameCamera.Instance;
		if (!(instance == null))
		{
			CinemachineVirtualCamera mainVCam = instance.mainVCam;
			if (!(mainVCam == null))
			{
				Vector3 fromPos = virtualCamera.transform.position;
				float fromFov = activeFov;
				float time = 0f;
				while (time < transitionTime)
				{
					if (mainVCam == null)
					{
						return;
					}
					time += Time.deltaTime;
					float time2 = 1f - time / transitionTime;
					float t = 1f - posCurve.Evaluate(time2);
					float t2 = 1f - rotCurve.Evaluate(time2);
					float t3 = 1f - fovCurve.Evaluate(time2);
					Vector3 position = mainVCam.transform.position;
					Quaternion rotation = mainVCam.transform.rotation;
					float fieldOfView = mainVCam.m_Lens.FieldOfView;
					Vector3 vector = Vector3.Lerp(fromPos, position, t);
					Quaternion rotation2 = Quaternion.Lerp(Quaternion.LookRotation(vcamLookTarget.position - vector, Vector3.up), rotation, t2);
					float fieldOfView2 = Mathf.Lerp(fromFov, fieldOfView, t3);
					virtualCamera.transform.SetPositionAndRotation(vector, rotation2);
					virtualCamera.m_Lens.FieldOfView = fieldOfView2;
					await UniTask.Yield();
					if (animateToken != token)
					{
						return;
					}
				}
			}
		}
		virtualCamera.Priority = -1;
	}

	protected override void OnInteractStop()
	{
		base.OnInteractStop();
		GamingConsole.OnGamingConsoleInteractChanged?.Invoke(obj: false);
		inputHandler.enabled = false;
		AnimateCameraOut().Forget();
		HUDManager.UnregisterHideToken(this);
		GamingConsoleHUD.Hide();
	}

	private void RefreshGame()
	{
		if (game == null)
		{
			CreateGame(SelectedGame);
		}
		else if (SelectedGame == null || SelectedGame.ID != game.ID)
		{
			CreateGame(SelectedGame);
		}
	}

	private void CreateGame(MiniGame prefab)
	{
		if (!isBeingDestroyed)
		{
			if (game != null)
			{
				UnityEngine.Object.Destroy(game.gameObject);
			}
			if (!(prefab == null))
			{
				game = UnityEngine.Object.Instantiate(prefab);
				game.transform.SetParent(base.transform, worldPositionStays: true);
				game.SetRenderTexture(rt);
				game.SetConsole(this);
				inputHandler.SetGame(game);
			}
		}
	}
}
