using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Duckov.Tasks;

[Obsolete]
public class EndingFlow : MonoBehaviour
{
	[SerializeField]
	private List<MonoBehaviour> taskBehaviours = new List<MonoBehaviour>();

	[SerializeField]
	private UnityEvent onBegin;

	[SerializeField]
	private UnityEvent onEnd;

	private void Start()
	{
		Task().Forget();
	}

	private async UniTask Task()
	{
		onBegin.Invoke();
		Debug.Log("Ending begin!");
		foreach (MonoBehaviour taskBehaviour in taskBehaviours)
		{
			await WaitForTaskBehaviour(taskBehaviour);
		}
		Debug.Log("Ending end!");
		onEnd.Invoke();
	}

	private async UniTask WaitForTaskBehaviour(MonoBehaviour mono)
	{
		if (mono is ITaskBehaviour task)
		{
			task.Begin();
			while (task.IsPending())
			{
				await UniTask.Yield();
			}
		}
	}
}
