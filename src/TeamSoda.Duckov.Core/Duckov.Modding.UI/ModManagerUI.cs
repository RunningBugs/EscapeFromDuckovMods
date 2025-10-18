using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using Duckov.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Duckov.Modding.UI;

public class ModManagerUI : MonoBehaviour
{
	[SerializeField]
	private FadeGroup mainFadeGroup;

	[SerializeField]
	private FadeGroup contentFadeGroup;

	[SerializeField]
	private FadeGroup agreementFadeGroup;

	[SerializeField]
	private FadeGroup uploaderFadeGroup;

	[SerializeField]
	private ModUploadPanel uploadPanel;

	[SerializeField]
	private Button rejectBtn;

	[SerializeField]
	private Button agreementBtn;

	[SerializeField]
	private ModEntry entryTemplate;

	[SerializeField]
	private Button quitBtn;

	public UnityEvent onQuit;

	private PrefabPool<ModEntry> _pool;

	private bool uploading;

	private ModManager Master => ModManager.Instance;

	private PrefabPool<ModEntry> Pool
	{
		get
		{
			if (_pool == null)
			{
				_pool = new PrefabPool<ModEntry>(entryTemplate);
			}
			return _pool;
		}
	}

	private void Awake()
	{
		agreementBtn.onClick.AddListener(OnAgreementBtnClicked);
		quitBtn.onClick.AddListener(Quit);
		rejectBtn.onClick.AddListener(OnRejectBtnClicked);
	}

	private void OnRejectBtnClicked()
	{
		ModManager.AllowActivatingMod = false;
		Quit();
	}

	private void OnAgreementBtnClicked()
	{
		ModManager.AllowActivatingMod = true;
		agreementFadeGroup.Hide();
		contentFadeGroup.Show();
	}

	private void Show()
	{
		mainFadeGroup.Show();
	}

	private void OnEnable()
	{
		ModManager.Rescan();
		Pool.ReleaseAll();
		foreach (ModInfo modInfo in ModManager.modInfos)
		{
			Pool.Get().Setup(this, modInfo);
		}
		uploaderFadeGroup.SkipHide();
		if (!ModManager.AllowActivatingMod)
		{
			contentFadeGroup.SkipHide();
			agreementFadeGroup.Show();
		}
		else
		{
			agreementFadeGroup.SkipHide();
			contentFadeGroup.Show();
		}
	}

	private void Hide()
	{
		mainFadeGroup.Hide();
	}

	private void Quit()
	{
		onQuit?.Invoke();
		Hide();
	}

	internal async UniTask BeginUpload(ModInfo info)
	{
		if (!uploading)
		{
			uploading = true;
			contentFadeGroup.Hide();
			await uploadPanel.Execute(info);
			contentFadeGroup.Show();
			uploading = false;
		}
	}
}
