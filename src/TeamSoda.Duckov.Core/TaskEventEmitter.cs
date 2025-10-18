using UnityEngine;

public class TaskEventEmitter : MonoBehaviour
{
	[SerializeField]
	private string eventKey;

	[SerializeField]
	private bool emitOnAwake;

	public void SetKey(string key)
	{
		eventKey = key;
	}

	private void Awake()
	{
		if (emitOnAwake)
		{
			EmitEvent();
		}
	}

	public void EmitEvent()
	{
		Debug.Log("TaskEvent:" + eventKey);
		TaskEvent.EmitTaskEvent(eventKey);
	}
}
