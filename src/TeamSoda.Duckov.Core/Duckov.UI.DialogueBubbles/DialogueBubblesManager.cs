using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.UI.DialogueBubbles;

public class DialogueBubblesManager : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private DialogueBubble prefab;

	[SerializeField]
	private Graphic raycastReceiver;

	private List<DialogueBubble> bubbles = new List<DialogueBubble>();

	public static DialogueBubblesManager Instance { get; private set; }

	public static event Action<PointerEventData> onPointerClick;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		prefab.gameObject.SetActive(value: false);
		raycastReceiver.enabled = false;
	}

	public static async UniTask Show(string text, Transform target, float yOffset = -1f, bool needInteraction = false, bool skippable = false, float speed = -1f, float duration = 2f)
	{
		if (Instance == null)
		{
			return;
		}
		DialogueBubble dialogueBubble = Instance.bubbles.FirstOrDefault((DialogueBubble e) => e != null && e.Target == target);
		if (dialogueBubble == null)
		{
			dialogueBubble = Instance.bubbles.FirstOrDefault((DialogueBubble e) => e != null && !e.gameObject.activeSelf);
		}
		if (dialogueBubble == null)
		{
			if (Instance.prefab == null)
			{
				return;
			}
			dialogueBubble = UnityEngine.Object.Instantiate(Instance.prefab, Instance.transform);
			Instance.bubbles.Add(dialogueBubble);
		}
		Instance.raycastReceiver.enabled = needInteraction;
		await dialogueBubble.Show(text, target, yOffset, needInteraction, skippable, speed, duration);
		if ((bool)Instance && (bool)Instance.raycastReceiver)
		{
			Instance.raycastReceiver.enabled = false;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		DialogueBubblesManager.onPointerClick?.Invoke(eventData);
	}
}
