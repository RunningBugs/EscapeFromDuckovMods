using UnityEngine;

namespace VLB_Samples;

public class FreeCameraController : MonoBehaviour
{
	public float cameraSensitivity = 90f;

	public float speedNormal = 10f;

	public float speedFactorSlow = 0.25f;

	public float speedFactorFast = 3f;

	public float speedClimb = 4f;

	private float rotationH;

	private float rotationV;

	private bool m_UseMouseView = true;

	private bool useMouseView
	{
		get
		{
			return m_UseMouseView;
		}
		set
		{
			m_UseMouseView = value;
			Cursor.lockState = (value ? CursorLockMode.Locked : CursorLockMode.None);
			Cursor.visible = !value;
		}
	}

	private void Start()
	{
		useMouseView = true;
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		rotationH = eulerAngles.y;
		rotationV = eulerAngles.x;
		if (rotationV > 180f)
		{
			rotationV -= 360f;
		}
	}

	private void Update()
	{
		if (useMouseView)
		{
			rotationH += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
			rotationV -= Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
		}
		rotationV = Mathf.Clamp(rotationV, -90f, 90f);
		base.transform.rotation = Quaternion.AngleAxis(rotationH, Vector3.up);
		base.transform.rotation *= Quaternion.AngleAxis(rotationV, Vector3.right);
		float num = speedNormal;
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			num *= speedFactorFast;
		}
		else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
		{
			num *= speedFactorSlow;
		}
		base.transform.position += num * Input.GetAxis("Vertical") * Time.deltaTime * base.transform.forward;
		base.transform.position += num * Input.GetAxis("Horizontal") * Time.deltaTime * base.transform.right;
		if (Input.GetKey(KeyCode.Q))
		{
			base.transform.position += speedClimb * Time.deltaTime * Vector3.up;
		}
		if (Input.GetKey(KeyCode.E))
		{
			base.transform.position += speedClimb * Time.deltaTime * Vector3.down;
		}
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
		{
			useMouseView = !useMouseView;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			useMouseView = false;
		}
	}
}
