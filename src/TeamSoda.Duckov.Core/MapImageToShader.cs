using UnityEngine;
using UnityEngine.UI;

public class MapImageToShader : MonoBehaviour
{
	public RectTransform rect;

	private Material material;

	private int zeroPointID = Shader.PropertyToID("_ZeroPoint");

	private int upVectorID = Shader.PropertyToID("_UpVector");

	private int rightVectorID = Shader.PropertyToID("_RightVector");

	private int scaleID = Shader.PropertyToID("_Scale");

	private void Start()
	{
	}

	private void Update()
	{
		if (!material)
		{
			material = GetComponent<Image>().material;
		}
		if ((bool)material)
		{
			Rect rect = this.rect.rect;
			Vector3 vector = rect.min;
			Vector3 vector2 = rect.max;
			Vector3 vector3 = new Vector3(vector.x, vector.y);
			Vector3 vector4 = new Vector3(vector.x, vector2.y);
			Vector3 vector5 = new Vector3(vector2.x, vector.y);
			Vector3 vector6 = base.transform.TransformPoint(vector3);
			Vector3 vector7 = base.transform.TransformVector(vector4 - vector3);
			Vector3 vector8 = base.transform.TransformVector(vector5 - vector3);
			material.SetVector(zeroPointID, vector6);
			material.SetVector(upVectorID, vector7);
			material.SetVector(rightVectorID, vector8);
			material.SetFloat(scaleID, this.rect.lossyScale.x);
		}
	}
}
