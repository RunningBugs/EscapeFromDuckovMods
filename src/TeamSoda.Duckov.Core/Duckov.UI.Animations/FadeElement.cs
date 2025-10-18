using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Duckov.UI.Animations;

public abstract class FadeElement : MonoBehaviour
{
	protected UniTask activeTask;

	private int activeTaskToken;

	[SerializeField]
	private bool manageGameObjectActive;

	[SerializeField]
	private float delay;

	[SerializeField]
	private string sfx_Show;

	[SerializeField]
	private string sfx_Hide;

	private bool isShown;

	public UniTask ActiveTask => activeTask;

	protected int ActiveTaskToken => activeTaskToken;

	protected bool ManageGameObjectActive => manageGameObjectActive;

	public bool IsFading { get; private set; }

	private void CacheNewTaskToken()
	{
		activeTaskToken = Random.Range(1, int.MaxValue);
	}

	public async UniTask Show(float delay = 0f)
	{
		CacheNewTaskToken();
		activeTask = WrapShowTask(ActiveTaskToken, delay);
		await activeTask;
		isShown = true;
	}

	public async UniTask Hide()
	{
		CacheNewTaskToken();
		activeTask = WrapHideTask(ActiveTaskToken, delay);
		isShown = false;
		await activeTask;
	}

	private async UniTask WrapShowTask(int token, float delay = 0f)
	{
		await UniTask.WaitForSeconds(this.delay + delay, ignoreTimeScale: true);
		if (!(this == null))
		{
			if (ActiveTaskToken == token && manageGameObjectActive)
			{
				base.gameObject.SetActive(value: true);
			}
			IsFading = true;
			if (!string.IsNullOrWhiteSpace(sfx_Show))
			{
				AudioManager.Post(sfx_Show);
			}
			await UniTask.NextFrame();
			await ShowTask(token);
			if (ActiveTaskToken == token)
			{
				IsFading = false;
			}
		}
	}

	private async UniTask WrapHideTask(int token, float delay = 0f)
	{
		if (!string.IsNullOrWhiteSpace(sfx_Hide) && isShown)
		{
			AudioManager.Post(sfx_Hide);
		}
		IsFading = true;
		await UniTask.WaitForSeconds(this.delay + delay, ignoreTimeScale: true);
		await HideTask(token);
		if (!(this == null))
		{
			if (ActiveTaskToken == token && manageGameObjectActive)
			{
				base.gameObject?.SetActive(value: false);
			}
			if (ActiveTaskToken == token)
			{
				IsFading = false;
			}
		}
	}

	protected abstract UniTask ShowTask(int token);

	protected abstract UniTask HideTask(int token);

	protected abstract void OnSkipHide();

	protected abstract void OnSkipShow();

	public void SkipHide()
	{
		activeTaskToken = 0;
		OnSkipHide();
		if (ManageGameObjectActive)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	internal void SkipShow()
	{
		activeTaskToken = 0;
		OnSkipShow();
		if (ManageGameObjectActive)
		{
			base.gameObject.SetActive(value: true);
		}
	}
}
