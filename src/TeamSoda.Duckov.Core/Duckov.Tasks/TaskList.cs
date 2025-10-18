using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Duckov.Tasks;

public class TaskList : MonoBehaviour, ITaskBehaviour
{
	[SerializeField]
	private bool beginOnStart;

	[SerializeField]
	private List<MonoBehaviour> tasks;

	[SerializeField]
	private UnityEvent onBegin;

	[SerializeField]
	private UnityEvent onComplete;

	[SerializeField]
	private bool listenToSkipSignal;

	private bool running;

	private bool complete;

	private int currentTaskIndex;

	private ITaskBehaviour currentTask;

	private bool skip;

	private void Start()
	{
		if (beginOnStart)
		{
			Begin();
		}
	}

	private async UniTask MainTask()
	{
		for (int i = 0; i < tasks.Count; i++)
		{
			currentTaskIndex = i;
			MonoBehaviour monoBehaviour = tasks[currentTaskIndex];
			if (monoBehaviour == null || !(monoBehaviour is ITaskBehaviour taskBehaviour))
			{
				continue;
			}
			currentTask = taskBehaviour;
			currentTask.Begin();
			while (currentTask != null && currentTask.IsPending() && !currentTask.IsComplete())
			{
				if (this == null)
				{
					return;
				}
				if (skip)
				{
					currentTask.Skip();
				}
				await UniTask.Yield();
			}
		}
		complete = true;
		running = false;
		onComplete?.Invoke();
	}

	public void Begin()
	{
		if (!running)
		{
			skip = false;
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

	public void Skip()
	{
		skip = true;
	}
}
