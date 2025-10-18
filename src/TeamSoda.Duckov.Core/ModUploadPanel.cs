using Cysharp.Threading.Tasks;
using Duckov.Modding;
using Duckov.UI.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModUploadPanel : MonoBehaviour
{
	[SerializeField]
	private FadeGroup fgMain;

	[SerializeField]
	private FadeGroup fgLoading;

	[SerializeField]
	private FadeGroup fgContent;

	[SerializeField]
	private TextMeshProUGUI txtTitle;

	[SerializeField]
	private TextMeshProUGUI txtDescription;

	[SerializeField]
	private RawImage preview;

	[SerializeField]
	private TextMeshProUGUI txtModName;

	[SerializeField]
	private TextMeshProUGUI txtPath;

	[SerializeField]
	private TextMeshProUGUI txtPublishedFileID;

	[SerializeField]
	private GameObject indicatorNew;

	[SerializeField]
	private GameObject indicatorUpdate;

	[SerializeField]
	private GameObject indicatorOwnershipWarning;

	[SerializeField]
	private GameObject indicatorInvalidContent;

	[SerializeField]
	private Button btnUpload;

	[SerializeField]
	private Button btnCancel;

	[SerializeField]
	private FadeGroup fgButtonMain;

	[SerializeField]
	private FadeGroup fgProgressBar;

	[SerializeField]
	private TextMeshProUGUI progressText;

	[SerializeField]
	private Image progressBarFill;

	[SerializeField]
	private FadeGroup fgSucceed;

	[SerializeField]
	private FadeGroup fgFailed;

	[SerializeField]
	private float closeAfterSeconds = 2f;

	[SerializeField]
	private Texture2D defaultPreviewTexture;

	private bool cancelClicked;

	private bool uploadClicked;

	private bool waitingForUpload;

	private void Awake()
	{
		btnCancel.onClick.AddListener(OnCancelBtnClick);
		btnUpload.onClick.AddListener(OnUploadBtnClick);
	}

	private void OnUploadBtnClick()
	{
		uploadClicked = true;
	}

	private void OnCancelBtnClick()
	{
		cancelClicked = true;
	}

	public async UniTask Execute(ModInfo info)
	{
		string path = info.path;
		SteamWorkshopManager workshopManager = SteamWorkshopManager.Instance;
		if (workshopManager == null || !SteamManager.Initialized)
		{
			Debug.LogError("Cannot execute uplaod panel. SteamWorkshopManager and SteamManager are required.");
		}
		Clean();
		fgMain.Show();
		fgLoading.Show();
		bool flag = ModManager.TryProcessModFolder(path, out info, isSteamItem: false, 0uL);
		txtPath.text = path.Replace('\\', '/');
		btnUpload.gameObject.SetActive(flag);
		if (flag)
		{
			txtTitle.text = info.displayName;
			txtDescription.text = info.description;
			txtPublishedFileID.text = ((info.publishedFileId != 0) ? info.publishedFileId.ToString() : "-");
			txtModName.text = info.name;
			preview.texture = info.preview;
		}
		else
		{
			txtTitle.text = "???";
			txtDescription.text = "???";
			txtPublishedFileID.text = "???";
			txtModName.text = "???";
			preview.texture = defaultPreviewTexture;
		}
		bool flag2 = flag && info.publishedFileId == 0;
		bool flag3 = SteamWorkshopManager.IsOwner(info);
		indicatorNew.SetActive(flag2);
		indicatorUpdate.SetActive(!flag2);
		indicatorOwnershipWarning.SetActive(!flag3);
		indicatorInvalidContent.SetActive(!flag);
		await fgLoading.HideAndReturnTask();
		fgContent.Show();
		fgButtonMain.Show();
		cancelClicked = false;
		uploadClicked = false;
		while (!cancelClicked && !uploadClicked)
		{
			await UniTask.Yield();
		}
		if (cancelClicked)
		{
			fgMain.Hide();
			return;
		}
		fgButtonMain.Hide();
		fgProgressBar.Show();
		waitingForUpload = true;
		bool num = await workshopManager.UploadWorkshopItem(path, "");
		waitingForUpload = false;
		fgProgressBar.Hide();
		if (num)
		{
			if (ModManager.TryProcessModFolder(path, out var info2, isSteamItem: false, 0uL))
			{
				txtPublishedFileID.text = $"{info2.publishedFileId}";
			}
			fgSucceed.Show();
		}
		else
		{
			fgFailed.Show();
		}
		await UniTask.WaitForSeconds(closeAfterSeconds);
		fgMain.Hide();
	}

	private void Update()
	{
		if (waitingForUpload)
		{
			progressBarFill.fillAmount = SteamWorkshopManager.UploadingProgress;
			ulong punBytesProcess = SteamWorkshopManager.punBytesProcess;
			ulong punBytesTotal = SteamWorkshopManager.punBytesTotal;
			progressText.text = FormatBytes(punBytesProcess) + " / " + FormatBytes(punBytesTotal);
		}
	}

	private static string FormatBytes(ulong bytes)
	{
		if (bytes < 1024)
		{
			return $"{bytes}bytes";
		}
		if (bytes < 1048576)
		{
			return $"{(float)bytes / 1024f:0.0}KB";
		}
		if (bytes < 1073741824)
		{
			return $"{(float)bytes / 1048576f:0.0}MB";
		}
		return $"{(float)bytes / 1.0737418E+09f:0.0}GB";
	}

	private void Clean()
	{
		fgLoading.SkipHide();
		fgContent.SkipHide();
		indicatorNew.SetActive(value: false);
		indicatorUpdate.SetActive(value: false);
		indicatorOwnershipWarning.SetActive(value: false);
		indicatorInvalidContent.SetActive(value: false);
		txtPublishedFileID.text = "-";
		txtPath.text = "-";
		fgButtonMain.SkipHide();
		fgProgressBar.SkipHide();
		fgSucceed.SkipHide();
		fgFailed.SkipHide();
		waitingForUpload = false;
	}
}
