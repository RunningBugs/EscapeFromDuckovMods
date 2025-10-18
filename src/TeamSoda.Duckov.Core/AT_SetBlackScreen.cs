using Cysharp.Threading.Tasks;
using Duckov.UI;
using NodeCanvas.Framework;

public class AT_SetBlackScreen : ActionTask
{
	public bool show;

	private UniTask task;

	protected override void OnExecute()
	{
		if (show)
		{
			task = BlackScreen.ShowAndReturnTask();
		}
		else
		{
			task = BlackScreen.HideAndReturnTask();
		}
	}

	protected override void OnUpdate()
	{
		if (task.Status != UniTaskStatus.Pending)
		{
			EndAction();
		}
	}
}
