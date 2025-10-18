using UnityEngine;

namespace Duckov.UI.Animations;

public class Revolver : MonoBehaviour
{
	public Vector3 pivot;

	public Vector3 axis = Vector3.forward;

	public float rPM;

	private void Update()
	{
		Quaternion quaternion = Quaternion.AngleAxis(Time.deltaTime * rPM / 60f * 360f, axis);
		Vector3 vector = base.transform.localPosition - pivot;
		Vector3 vector2 = quaternion * vector;
		Vector3 localPosition = pivot + vector2;
		base.transform.localPosition = localPosition;
	}

	private void OnDrawGizmosSelected()
	{
		if (base.transform.parent != null)
		{
			Gizmos.matrix = base.transform.parent.localToWorldMatrix;
		}
		Gizmos.DrawLine(pivot, base.transform.localPosition);
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(pivot, 1f);
	}
}
