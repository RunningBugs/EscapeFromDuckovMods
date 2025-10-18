using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using TMPro;
using UnityEngine;

namespace Duckov.UI;

public class NotificationText : MonoBehaviour
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private float duration = 1.2f;

	[SerializeField]
	private float durationIfPending = 0.65f;

	private static Queue<string> pendingTexts = new Queue<string>();

	private bool showing;

	private int PendingCount => pendingTexts.Count;

	public static void Push(string text)
	{
		if (pendingTexts.Count <= 0 || !(pendingTexts.Peek() == text))
		{
			pendingTexts.Enqueue(text);
		}
	}

	private static string Pop()
	{
		return pendingTexts.Dequeue();
	}

	private void Update()
	{
		if (!showing && PendingCount > 0)
		{
			ShowNext().Forget();
		}
	}

	private async UniTask ShowNext()
	{
		if (PendingCount != 0)
		{
			showing = true;
			string text = Pop();
			this.text.text = text;
			fadeGroup.Show();
			await UniTask.WaitForSeconds((PendingCount > 0) ? durationIfPending : duration, ignoreTimeScale: true);
			await fadeGroup.HideAndReturnTask();
			showing = false;
		}
	}
}
