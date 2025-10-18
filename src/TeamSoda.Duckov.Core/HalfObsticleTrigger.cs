using UnityEngine;

public class HalfObsticleTrigger : MonoBehaviour
{
	public HalfObsticle parent;

	private void OnTriggerEnter(Collider other)
	{
		parent.OnTriggerEnter(other);
	}

	private void OnTriggerExit(Collider other)
	{
		parent.OnTriggerExit(other);
	}
}
