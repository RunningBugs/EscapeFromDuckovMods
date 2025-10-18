using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class MainMenuCamera : MonoBehaviour
{
	public Vector2 yawRange;

	public Vector2 pitchRange;

	public Transform pitchRoot;

	[FormerlySerializedAs("posRange")]
	public Vector2 posRangeX;

	public Vector2 posRangeY;

	private void Update()
	{
		Vector3 mousePosition = Input.mousePosition;
		float num = Screen.width;
		float num2 = Screen.height;
		float t = mousePosition.x / num;
		float t2 = mousePosition.y / num2;
		base.transform.localRotation = quaternion.Euler(0f, Mathf.Lerp(yawRange.x, yawRange.y, t) * (MathF.PI / 180f), 0f);
		if ((bool)pitchRoot)
		{
			pitchRoot.localRotation = quaternion.Euler(Mathf.Lerp(pitchRange.x, pitchRange.y, t2) * (MathF.PI / 180f), 0f, 0f);
		}
		base.transform.localPosition = new Vector3(Mathf.Lerp(posRangeX.x, posRangeX.y, t), Mathf.Lerp(posRangeY.x, posRangeY.y, t2), 0f);
	}
}
