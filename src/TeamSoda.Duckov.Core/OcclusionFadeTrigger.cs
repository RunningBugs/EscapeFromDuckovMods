using UnityEngine;

public class OcclusionFadeTrigger : MonoBehaviour
{
	public OcclusionFadeObject parent;

	private void Awake()
	{
		base.gameObject.layer = LayerMask.NameToLayer("VisualOcclusion");
	}

	public void Enter()
	{
		parent.OnEnter();
	}

	public void Leave()
	{
		parent.OnLeave();
	}
}
