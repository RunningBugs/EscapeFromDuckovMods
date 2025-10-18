using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmDialogue : MonoBehaviour
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private Button btnConfirm;

	[SerializeField]
	private Button btnCancel;

	private bool canceled;

	private bool confirmed;

	private bool executing;

	private void Awake()
	{
		btnConfirm.onClick.AddListener(OnConfirmed);
		btnCancel.onClick.AddListener(OnCanceled);
	}

	private void OnCanceled()
	{
		canceled = true;
	}

	private void OnConfirmed()
	{
		confirmed = true;
	}

	public async UniTask<bool> Execute()
	{
		if (executing)
		{
			return false;
		}
		executing = true;
		bool result = await DoExecute();
		executing = false;
		return result;
	}

	private async UniTask<bool> DoExecute()
	{
		Debug.Log("Executing confirm dialogue");
		await fadeGroup.ShowAndReturnTask();
		canceled = false;
		confirmed = false;
		while (!canceled && !confirmed)
		{
			await UniTask.Yield();
		}
		bool result = false;
		if (confirmed)
		{
			result = true;
		}
		fadeGroup.Hide();
		return result;
	}

	internal void SkipHide()
	{
		fadeGroup.SkipHide();
	}
}
