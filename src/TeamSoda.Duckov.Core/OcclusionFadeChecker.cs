using UnityEngine;

public class OcclusionFadeChecker : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		OcclusionFadeTrigger component = other.GetComponent<OcclusionFadeTrigger>();
		if ((bool)component)
		{
			component.Enter();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		OcclusionFadeTrigger component = other.GetComponent<OcclusionFadeTrigger>();
		if ((bool)component)
		{
			component.Leave();
		}
	}
}
