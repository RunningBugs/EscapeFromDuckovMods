using Cinemachine;
using NodeCanvas.Framework;

public class AT_SetVirtualCamera : ActionTask
{
	private static CinemachineVirtualCamera cachedVCam;

	public BBParameter<CinemachineVirtualCamera> target;

	protected override string info => "Set camera :" + ((target.value == null) ? "Empty" : target.value.name);

	protected override void OnExecute()
	{
		base.OnExecute();
		if (cachedVCam != null)
		{
			cachedVCam.gameObject.SetActive(value: false);
		}
		if (target.value != null)
		{
			target.value.gameObject.SetActive(value: true);
			cachedVCam = target.value;
		}
		else
		{
			cachedVCam = null;
		}
		EndAction();
	}
}
