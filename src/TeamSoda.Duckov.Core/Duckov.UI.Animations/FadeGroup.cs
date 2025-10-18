using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Duckov.UI.Animations;

public class FadeGroup : MonoBehaviour
{
	[SerializeField]
	private List<FadeElement> fadeElements = new List<FadeElement>();

	[SerializeField]
	private bool skipHideOnStart = true;

	[SerializeField]
	private bool showOnEnable;

	[SerializeField]
	private bool skipHideBeforeShow = true;

	public bool manageGameObjectActive;

	private bool isHidingInProgress;

	private bool isShowingInProgress;

	private bool isShown;

	public bool debug;

	private int activeTaskToken;

	public bool IsHidingInProgress => isHidingInProgress;

	public bool IsShowingInProgress => isShowingInProgress;

	public bool IsShown => isShown;

	public bool IsHidden => !isShown;

	public bool IsFading => fadeElements.Any((FadeElement e) => e != null && e.IsFading);

	public event Action<FadeGroup> OnFadeComplete;

	public event Action<FadeGroup> OnShowComplete;

	public event Action<FadeGroup> OnHideComplete;

	private void Start()
	{
		if (skipHideOnStart)
		{
			SkipHide();
		}
	}

	private void OnEnable()
	{
		if (showOnEnable)
		{
			Show();
		}
	}

	[ContextMenu("Show")]
	public void Show()
	{
		if (debug)
		{
			Debug.Log("Fadegroup SHOW " + base.name);
		}
		skipHideOnStart = false;
		if (manageGameObjectActive)
		{
			base.gameObject.SetActive(value: true);
		}
		ShowTask().Forget();
	}

	[ContextMenu("Hide")]
	public void Hide()
	{
		if (debug)
		{
			Debug.Log("Fadegroup HIDE " + base.name, base.gameObject);
		}
		HideTask().Forget();
	}

	public void Toggle()
	{
		if (IsShown)
		{
			Hide();
		}
		else if (IsHidden)
		{
			Show();
		}
	}

	public UniTask ShowAndReturnTask()
	{
		if (skipHideBeforeShow)
		{
			SkipHide();
		}
		if (manageGameObjectActive)
		{
			base.gameObject.SetActive(value: true);
		}
		return ShowTask();
	}

	public UniTask HideAndReturnTask()
	{
		return HideTask();
	}

	private int CacheNewTaskToken()
	{
		activeTaskToken = UnityEngine.Random.Range(0, int.MaxValue);
		return activeTaskToken;
	}

	public async UniTask ShowTask()
	{
		isHidingInProgress = false;
		isShowingInProgress = true;
		isShown = true;
		int token = CacheNewTaskToken();
		List<UniTask> list = new List<UniTask>();
		foreach (FadeElement fadeElement in fadeElements)
		{
			if (fadeElement == null)
			{
				Debug.LogWarning("Element in fade group " + base.name + " is null");
			}
			else
			{
				list.Add(fadeElement.Show());
			}
		}
		await UniTask.WhenAll(list);
		if (token == activeTaskToken)
		{
			ShowComplete();
		}
	}

	public async UniTask HideTask()
	{
		isShowingInProgress = false;
		isHidingInProgress = true;
		isShown = false;
		int token = CacheNewTaskToken();
		List<UniTask> list = new List<UniTask>();
		foreach (FadeElement fadeElement in fadeElements)
		{
			if (!(fadeElement == null))
			{
				list.Add(fadeElement.Hide());
			}
		}
		await UniTask.WhenAll(list);
		if (token == activeTaskToken)
		{
			HideComplete();
		}
	}

	private void ShowComplete()
	{
		isShowingInProgress = false;
		this.OnFadeComplete?.Invoke(this);
		this.OnShowComplete?.Invoke(this);
	}

	private void HideComplete()
	{
		isHidingInProgress = false;
		this.OnFadeComplete?.Invoke(this);
		this.OnHideComplete?.Invoke(this);
		if (!(this == null) && manageGameObjectActive)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void SkipHide()
	{
		foreach (FadeElement fadeElement in fadeElements)
		{
			if (fadeElement == null)
			{
				Debug.LogWarning("Element in fade group " + base.name + " is null");
			}
			else
			{
				fadeElement.SkipHide();
			}
		}
		if (manageGameObjectActive)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	internal void SkipShow()
	{
		foreach (FadeElement fadeElement in fadeElements)
		{
			if (fadeElement == null)
			{
				Debug.LogWarning("Element in fade group " + base.name + " is null");
			}
			else
			{
				fadeElement.SkipShow();
			}
		}
		if (manageGameObjectActive)
		{
			base.gameObject.SetActive(value: true);
		}
	}
}
