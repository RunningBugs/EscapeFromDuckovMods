using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.Modding.UI;

public class ModEntry : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI textTitle;

	[SerializeField]
	private TextMeshProUGUI textName;

	[SerializeField]
	private TextMeshProUGUI textDescription;

	[SerializeField]
	private RawImage preview;

	[SerializeField]
	private GameObject activeIndicator;

	[SerializeField]
	private GameObject nameCollisionIndicator;

	[SerializeField]
	private Button toggleButton;

	[SerializeField]
	private GameObject steamItemIndicator;

	[SerializeField]
	private GameObject steamItemOwnerIndicator;

	[SerializeField]
	private GameObject notSteamItemIndicator;

	[SerializeField]
	private Button uploadButton;

	[SerializeField]
	private GameObject failedIndicator;

	private ModManagerUI master;

	private ModInfo info;

	private void Awake()
	{
		toggleButton.onClick.AddListener(OnToggleButtonClicked);
		uploadButton.onClick.AddListener(OnUploadButtonClicked);
		ModManager.OnModLoadingFailed = (Action<string, string>)Delegate.Combine(ModManager.OnModLoadingFailed, new Action<string, string>(OnModLoadingFailed));
		failedIndicator.SetActive(value: false);
	}

	private void OnDestroy()
	{
		ModManager.OnModLoadingFailed = (Action<string, string>)Delegate.Remove(ModManager.OnModLoadingFailed, new Action<string, string>(OnModLoadingFailed));
	}

	private void OnModLoadingFailed(string dllPath, string message)
	{
		if (!(dllPath != info.dllPath))
		{
			Debug.LogError(message);
			failedIndicator.SetActive(value: true);
		}
	}

	private void OnUploadButtonClicked()
	{
		if (!(master == null))
		{
			master.BeginUpload(info).Forget();
		}
	}

	private void OnToggleButtonClicked()
	{
		if (ModManager.Instance == null)
		{
			Debug.LogError("ModManager.Instance Not Found");
			return;
		}
		ModBehaviour instance;
		bool num = ModManager.IsModActive(info, out instance);
		bool flag = num && instance.info.path.Trim() == info.path.Trim();
		if (num && flag)
		{
			ModManager.Instance.DeactivateMod(info);
		}
		else
		{
			ModManager.Instance.ActivateMod(info);
		}
	}

	private void OnEnable()
	{
		ModManager.OnModStatusChanged += OnModStatusChanged;
	}

	private void OnDisable()
	{
		ModManager.OnModStatusChanged -= OnModStatusChanged;
	}

	private void OnModStatusChanged()
	{
		RefreshStatus();
	}

	private void RefreshStatus()
	{
		ModBehaviour instance;
		bool num = ModManager.IsModActive(info, out instance);
		bool flag = num && instance.info.path.Trim() == info.path.Trim();
		bool active = num && !flag;
		activeIndicator.SetActive(flag);
		nameCollisionIndicator.SetActive(active);
	}

	private void RefreshInfo()
	{
		textTitle.text = info.displayName;
		textName.text = info.name;
		textDescription.text = info.description;
		preview.texture = info.preview;
		steamItemIndicator.SetActive(info.isSteamItem);
		notSteamItemIndicator.SetActive(!info.isSteamItem);
		bool flag = SteamWorkshopManager.IsOwner(info);
		steamItemOwnerIndicator.SetActive(flag);
		bool active = flag || !info.isSteamItem;
		uploadButton.gameObject.SetActive(active);
	}

	public void Setup(ModManagerUI master, ModInfo modInfo)
	{
		this.master = master;
		info = modInfo;
		RefreshInfo();
		RefreshStatus();
	}
}
