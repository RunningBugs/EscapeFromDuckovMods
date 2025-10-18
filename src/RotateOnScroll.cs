using UnityEngine;

public class RotateOnScroll : MonoBehaviour
{
	public float rotationSpeed = 2000f;

	private void Update()
	{
		float axis = Input.GetAxis("Mouse ScrollWheel");
		if (axis != 0f)
		{
			float angle = axis * rotationSpeed * Time.deltaTime * 10f;
			base.transform.Rotate(Vector3.up, angle);
		}
	}
}
