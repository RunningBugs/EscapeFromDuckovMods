using Cysharp.Threading.Tasks;
using Duckov.Economy;
using Duckov.UI.Animations;
using Eflatun.SceneReference;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.UI;

public class MapSelectionView : View
{
	[SerializeField]
	private FadeGroup mainFadeGroup;

	[SerializeField]
	private FadeGroup confirmIndicatorFadeGroup;

	[SerializeField]
	private TextMeshProUGUI destinationDisplayNameText;

	[SerializeField]
	private CostDisplay confirmCostDisplay;

	private string sfx_EntryClicked = "UI/confirm";

	private string sfx_ShowDestination = "UI/destination_show";

	private string sfx_ConfirmDestination = "UI/destination_confirm";

	[SerializeField]
	private ColorPunch confirmColorPunch;

	[SerializeField]
	private Button btnConfirm;

	[SerializeField]
	private Button btnCancel;

	[SerializeField]
	private SceneReference overrideLoadingScreen;

	private bool loading;

	private bool confirmButtonClicked;

	private bool cancelButtonClicked;

	public static MapSelectionView Instance => View.GetViewInstance<MapSelectionView>();

	protected override void Awake()
	{
		base.Awake();
		btnConfirm.onClick.AddListener(delegate
		{
			confirmButtonClicked = true;
		});
		btnCancel.onClick.AddListener(delegate
		{
			cancelButtonClicked = true;
		});
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		confirmIndicatorFadeGroup.SkipHide();
		mainFadeGroup.Show();
	}

	protected override void OnClose()
	{
		base.OnClose();
		mainFadeGroup.Hide();
	}

	internal void NotifyEntryClicked(MapSelectionEntry mapSelectionEntry, PointerEventData eventData)
	{
		if (mapSelectionEntry.Cost.Enough)
		{
			AudioManager.Post(sfx_EntryClicked);
			string sceneID = mapSelectionEntry.SceneID;
			LevelManager.loadLevelBeaconIndex = mapSelectionEntry.BeaconIndex;
			loading = true;
			LoadTask(sceneID, mapSelectionEntry.Cost).Forget();
		}
	}

	private async UniTask LoadTask(string sceneID, Cost cost)
	{
		btnCancel.gameObject.SetActive(value: true);
		btnConfirm.gameObject.SetActive(value: false);
		SceneInfoEntry sceneInfo = SceneInfoCollection.GetSceneInfo(sceneID);
		SetupSceneInfo(sceneInfo);
		confirmCostDisplay.Setup(cost);
		confirmCostDisplay.gameObject.SetActive(!cost.IsFree);
		AudioManager.Post(sfx_ShowDestination);
		await confirmIndicatorFadeGroup.ShowAndReturnTask();
		btnConfirm.gameObject.SetActive(value: true);
		bool num = await WaitForConfirm();
		btnCancel.gameObject.SetActive(value: false);
		btnConfirm.gameObject.SetActive(value: false);
		if (num && cost.Enough)
		{
			cost.Pay();
			confirmColorPunch.Punch();
			AudioManager.Post(sfx_ConfirmDestination);
			await UniTask.WaitForSeconds(0.5f, ignoreTimeScale: true);
			SceneLoader.Instance.LoadScene(sceneID, overrideLoadingScreen, clickToConinue: true).Forget();
		}
		else
		{
			confirmIndicatorFadeGroup.Hide();
		}
		loading = false;
	}

	private async UniTask<bool> WaitForConfirm()
	{
		confirmButtonClicked = false;
		cancelButtonClicked = false;
		while (true)
		{
			if (cancelButtonClicked)
			{
				return false;
			}
			if (confirmButtonClicked)
			{
				break;
			}
			await UniTask.Yield();
		}
		return true;
	}

	private void SetupSceneInfo(SceneInfoEntry info)
	{
		if (info != null)
		{
			string displayName = info.DisplayName;
			destinationDisplayNameText.text = displayName;
			destinationDisplayNameText.color = Color.white;
		}
	}

	internal override void TryQuit()
	{
		if (!loading)
		{
			Close();
		}
	}
}
