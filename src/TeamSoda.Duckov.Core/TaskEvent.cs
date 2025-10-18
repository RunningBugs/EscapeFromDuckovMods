using System;

public class TaskEvent
{
	public static event Action<string> OnTaskEvent;

	public static void EmitTaskEvent(string taskEventKey)
	{
		TaskEvent.OnTaskEvent?.Invoke(taskEventKey);
	}
}
