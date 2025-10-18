using System.Collections.Generic;
using UnityEngine;

public class CustomFacePart : MonoBehaviour
{
	public int id;

	public bool mirror;

	public Transform centerObject;

	public Transform leftObject;

	public Transform rightObject;

	public List<Renderer> customColorRenderers;

	public List<Renderer> renderers;

	public string customColorKey = "_Tint";

	private CustomFaceInstance parent;

	private MaterialPropertyBlock block;

	public CustomFaceInstance Parent => parent;

	private void CollectRenders()
	{
		renderers.Clear();
		MeshRenderer[] componentsInChildren = base.transform.GetComponentsInChildren<MeshRenderer>();
		SkinnedMeshRenderer[] componentsInChildren2 = base.transform.GetComponentsInChildren<SkinnedMeshRenderer>();
		renderers.AddRange(componentsInChildren);
		renderers.AddRange(componentsInChildren2);
	}

	public void SetInfo(CustomFacePartInfo info, CustomFaceInstance _parent)
	{
		parent = _parent;
		if (mirror)
		{
			base.transform.localScale = new Vector3(-1f, 1f, 1f);
		}
		else
		{
			base.transform.localScale = Vector3.one;
		}
		Quaternion quaternion = Quaternion.Euler(0f, 0f - info.leftRightAngle, 0f);
		Quaternion quaternion2 = Quaternion.Euler(0f, 0f - info.leftRightAngle - info.distanceAngle, 0f);
		Quaternion quaternion3 = Quaternion.Euler(0f, 0f - info.leftRightAngle + info.distanceAngle, 0f);
		if ((bool)centerObject)
		{
			Vector3 vector = quaternion * Vector3.forward;
			centerObject.localPosition = vector * info.radius + Vector3.up * (info.height + info.heightOffset);
			centerObject.localRotation = Quaternion.LookRotation(vector);
			centerObject.localRotation = Quaternion.Euler(0f, 0f, info.twist) * centerObject.localRotation;
			centerObject.localScale = Vector3.one * info.scale;
		}
		if ((bool)leftObject)
		{
			Vector3 vector2 = quaternion2 * Vector3.forward;
			leftObject.localPosition = vector2 * info.radius + Vector3.up * (info.height + info.heightOffset);
			leftObject.localRotation = Quaternion.LookRotation(vector2, Vector3.up);
			leftObject.localRotation = Quaternion.AngleAxis(info.twist, vector2) * leftObject.localRotation;
			leftObject.localScale = Vector3.one * info.scale;
		}
		if ((bool)rightObject)
		{
			Vector3 vector3 = quaternion3 * Vector3.forward;
			rightObject.localPosition = vector3 * info.radius + Vector3.up * (info.height + info.heightOffset);
			rightObject.localRotation = Quaternion.LookRotation(vector3, Vector3.up);
			rightObject.localRotation = Quaternion.AngleAxis(0f - info.twist, vector3) * rightObject.localRotation;
			rightObject.localScale = Vector3.one * info.scale;
		}
		if (block == null)
		{
			block = new MaterialPropertyBlock();
		}
		info.color.a = 1f;
		block.SetColor(customColorKey, info.color);
		foreach (Renderer customColorRenderer in customColorRenderers)
		{
			if ((bool)customColorRenderer)
			{
				customColorRenderer.SetPropertyBlock(block);
			}
		}
	}
}
