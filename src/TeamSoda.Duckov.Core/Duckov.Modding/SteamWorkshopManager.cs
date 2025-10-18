using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Sirenix.Utilities;
using Steamworks;
using UnityEngine;

namespace Duckov.Modding;

public class SteamWorkshopManager : MonoBehaviour
{
	private CallResult<SteamUGCQueryCompleted_t> CRSteamUGCQueryCompleted;

	private CallResult<CreateItemResult_t> CRCreateItemResult;

	private UGCQueryHandle_t activeQueryHandle;

	private static List<SteamUGCDetails_t> ugcDetailsCache = new List<SteamUGCDetails_t>();

	private bool createItemResultFired;

	private CreateItemResult_t createItemResult;

	public static SteamWorkshopManager Instance { get; private set; }

	public static ulong punBytesProcess { get; private set; }

	public static ulong punBytesTotal { get; private set; }

	public static float UploadingProgress => (float)((double)punBytesProcess / (double)punBytesTotal);

	public bool UploadSucceed { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		ModManager.Rescan();
		SendQueryDetailsRequest();
		ModManager.OnScan += OnScanMods;
	}

	private void OnDisable()
	{
		ModManager.OnScan -= OnScanMods;
	}

	private void OnScanMods(List<ModInfo> list)
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		foreach (SteamUGCDetails_t item in ugcDetailsCache)
		{
			PublishedFileId_t nPublishedFileId = item.m_nPublishedFileId;
			EItemState itemState = (EItemState)SteamUGC.GetItemState(nPublishedFileId);
			if ((itemState | EItemState.k_EItemStateInstalled) == itemState && SteamUGC.GetItemInstallInfo(nPublishedFileId, out var _, out var pchFolder, 1024u, out var _))
			{
				if (!ModManager.TryProcessModFolder(pchFolder, out var info, isSteamItem: true, nPublishedFileId.m_PublishedFileId))
				{
					Debug.LogError("Mod processing failed! \nPath:" + pchFolder);
				}
				else
				{
					list.Add(info);
				}
			}
		}
	}

	public void SendQueryDetailsRequest()
	{
		if (!SteamManager.Initialized)
		{
			return;
		}
		if (CRSteamUGCQueryCompleted == null)
		{
			CRSteamUGCQueryCompleted = CallResult<SteamUGCQueryCompleted_t>.Create(OnSteamUGCQueryCompleted);
		}
		HashSet<PublishedFileId_t> hashSet = new HashSet<PublishedFileId_t>();
		uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
		PublishedFileId_t[] array = new PublishedFileId_t[numSubscribedItems];
		SteamUGC.GetSubscribedItems(array, numSubscribedItems);
		hashSet.AddRange(array);
		foreach (ModInfo modInfo in ModManager.modInfos)
		{
			if (modInfo.publishedFileId != 0L)
			{
				hashSet.Add((PublishedFileId_t)modInfo.publishedFileId);
			}
		}
		SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(SteamUGC.CreateQueryUGCDetailsRequest(hashSet.ToArray(), (uint)hashSet.Count));
		CRSteamUGCQueryCompleted.Set(hAPICall);
		new StringBuilder();
	}

	private void OnSteamUGCQueryCompleted(SteamUGCQueryCompleted_t completed, bool bIOFailure)
	{
		if (bIOFailure)
		{
			Debug.LogError("Steam UGC Query failed", base.gameObject);
			return;
		}
		UGCQueryHandle_t handle = completed.m_handle;
		uint unNumResultsReturned = completed.m_unNumResultsReturned;
		for (uint num = 0u; num < unNumResultsReturned; num++)
		{
			SteamUGC.GetQueryUGCResult(handle, num, out var pDetails);
			ugcDetailsCache.Add(pDetails);
		}
		SteamUGC.ReleaseQueryUGCRequest(handle);
		ModManager.Instance.ScanAndActivateMods();
	}

	public async UniTask<PublishedFileId_t> RequestNewWorkshopItemID()
	{
		if (!SteamManager.Initialized)
		{
			return default(PublishedFileId_t);
		}
		if (CRCreateItemResult == null)
		{
			CRCreateItemResult = CallResult<CreateItemResult_t>.Create(OnCreateItemResult);
		}
		Debug.Log("Requesting new PublishedFileId");
		createItemResultFired = false;
		SteamAPICall_t hAPICall = SteamUGC.CreateItem((AppId_t)3167020u, EWorkshopFileType.k_EWorkshopFileTypeFirst);
		CRCreateItemResult.Set(hAPICall, delegate(CreateItemResult_t result, bool failure)
		{
			Debug.Log("Creat Item Result Fired B");
			createItemResultFired = true;
			createItemResult = result;
		});
		while (!createItemResultFired)
		{
			await UniTask.Yield();
		}
		if (createItemResult.m_eResult != EResult.k_EResultOK)
		{
			Debug.LogError($"Failed to create workshop item.\nResult: {createItemResult.m_eResult}");
			return default(PublishedFileId_t);
		}
		return createItemResult.m_nPublishedFileId;
	}

	private void OnCreateItemResult(CreateItemResult_t result, bool bIOFailure)
	{
		Debug.Log("Creat Item Result Fired A");
		createItemResultFired = true;
		createItemResult = result;
	}

	public async UniTask<bool> UploadWorkshopItem(string path, string changeNote = "Unknown")
	{
		if (!SteamManager.Initialized)
		{
			return false;
		}
		Debug.Log("Begin uploading mod: \n" + path);
		if (!ModManager.TryProcessModFolder(path, out var modInfo, isSteamItem: false, 0uL))
		{
			Debug.LogError("Failed to process mod folder:\n" + path);
			return false;
		}
		if (!modInfo.dllFound)
		{
			Debug.LogError("Mod's dll file not found.\n" + modInfo.dllPath);
			return false;
		}
		if (modInfo.publishedFileId == 0L)
		{
			Debug.Log("Requesting PublishedFileId for mod " + modInfo.name + " \n" + path);
			PublishedFileId_t publishedFileId_t = await RequestNewWorkshopItemID();
			if (publishedFileId_t.m_PublishedFileId == 0L)
			{
				Debug.Log("Failed to request PublishedFileId.");
				return false;
			}
			modInfo.publishedFileId = publishedFileId_t.m_PublishedFileId;
			try
			{
				Debug.Log($"MOD [{modInfo.name}] ({modInfo.publishedFileId}) Writing mod's info.ini  \n{path}");
				ModManager.WriteModInfoINI(modInfo);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				Debug.LogError($"Successfully created steam workshop item (PublishedFileId:{publishedFileId_t}). But failed to write to info.ini.");
				return false;
			}
		}
		UGCUpdateHandle_t handle = SteamUGC.StartItemUpdate((AppId_t)3167020u, (PublishedFileId_t)modInfo.publishedFileId);
		SteamUGC.SetItemTitle(handle, modInfo.displayName);
		SteamUGC.SetItemDescription(handle, modInfo.description);
		SteamUGC.SetItemVisibility(handle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityPublic);
		SteamUGC.SetItemTags(handle, new List<string> { "Mod" });
		string text = Path.Combine(modInfo.path, "preview.png");
		text = text.Replace('\\', '/');
		SteamUGC.SetItemPreview(handle, text);
		string path2 = modInfo.path;
		path2 = path2.Replace('\\', '/');
		bool flag = SteamUGC.SetItemContent(handle, path2);
		Debug.Log($"SetItemContent \n{path2}\n{flag}");
		bool submitCallDone = false;
		SubmitItemUpdateResult_t submitResult = default(SubmitItemUpdateResult_t);
		Debug.Log($"MOD [{modInfo.name}] ({modInfo.publishedFileId}) Begin uploading to steam.  \n{path}");
		using (CallResult<SubmitItemUpdateResult_t> callHandler = CallResult<SubmitItemUpdateResult_t>.Create(delegate(SubmitItemUpdateResult_t result, bool failure)
		{
			submitCallDone = true;
			submitResult = result;
		}))
		{
			SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(handle, changeNote);
			callHandler.Set(hAPICall);
			while (!submitCallDone)
			{
				SteamUGC.GetItemUpdateProgress(handle, out var punBytesProcessed, out var num);
				punBytesProcess = punBytesProcessed;
				punBytesTotal = num;
				await UniTask.Yield();
			}
		}
		Debug.Log($"MOD [{modInfo.name}] ({modInfo.publishedFileId}) Upload result returned.  \n{path}");
		if (submitResult.m_eResult != EResult.k_EResultOK)
		{
			Debug.LogError($"Failed to upload mod.\nResult:{submitResult.m_eResult} \npath: {modInfo.path}");
			return false;
		}
		if (submitResult.m_bUserNeedsToAcceptWorkshopLegalAgreement)
		{
			SteamFriends.ActivateGameOverlayToWebPage($"steam://url/CommunityFilePage/{modInfo.publishedFileId}");
		}
		UploadSucceed = true;
		return true;
	}

	public static bool IsOwner(ModInfo info)
	{
		if (!SteamManager.Initialized)
		{
			return false;
		}
		if (info.publishedFileId == 0L)
		{
			return false;
		}
		foreach (SteamUGCDetails_t item in ugcDetailsCache)
		{
			if (item.m_nPublishedFileId.m_PublishedFileId == info.publishedFileId)
			{
				return item.m_ulSteamIDOwner == SteamUser.GetSteamID().m_SteamID;
			}
		}
		return false;
	}
}
