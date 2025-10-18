using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Duckov.Tasks;

public class ParallelTask : MonoBehaviour, ITaskBehaviour
{
	[SerializeField]
	private bool beginOnStart;

	[SerializeField]
	private List<MonoBehaviour> tasks;

	[SerializeField]
	private UnityEvent onBegin;

	[SerializeField]
	private UnityEvent onComplete;

	private bool running;

	private bool complete;

	private void Start()
	{
		if (beginOnStart)
		{
			Begin();
		}
	}

	private async UniTask MainTask()
	{
		foreach (MonoBehaviour task in tasks)
		{
			if (!(task == null) && task is ITaskBehaviour taskBehaviour)
			{
				taskBehaviour.Begin();
			}
		}
		bool anyTaskPending = false;
		while (anyTaskPending)
		{
			if (this == null)
			{
				return;
			}
			anyTaskPending = false;
			foreach (MonoBehaviour task2 in tasks)
			{
				if (!(task2 == null) && task2 is ITaskBehaviour taskBehaviour2 && taskBehaviour2.IsPending())
				{
					anyTaskPending = true;
					break;
				}
			}
			if (!anyTaskPending)
			{
				break;
			}
			await UniTask.Yield();
		}
		running = false;
		complete = true;
		onComplete?.Invoke();
	}

	public void Begin()
	{
		if (!running)
		{
			running = true;
			complete = false;
			onBegin?.Invoke();
			MainTask().Forget();
		}
	}

	public bool IsComplete()
	{
		return complete;
	}

	public bool IsPending()
	{
		return running;
	}
}
