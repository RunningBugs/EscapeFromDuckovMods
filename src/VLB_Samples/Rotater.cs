using UnityEngine;
using UnityEngine.Serialization;

namespace VLB_Samples;

public class Rotater : MonoBehaviour
{
	[FormerlySerializedAs("m_EulerSpeed")]
	public Vector3 EulerSpeed = Vector3.zero;

	private void Update()
	{
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		eulerAngles += EulerSpeed * Time.deltaTime;
		base.transform.rotation = Quaternion.Euler(eulerAngles);
	}
}
