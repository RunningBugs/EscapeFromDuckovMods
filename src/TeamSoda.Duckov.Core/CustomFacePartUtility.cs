using System;
using UnityEngine;

[Serializable]
public class CustomFacePartUtility
{
	public CustomFaceInstance parent;

	public CustomFacePartTypes partType;

	public CustomFacePartInfo partInfo;

	public Transform socket;

	private CustomFacePart facePartInstance;

	public CustomFacePart PartInstance => facePartInstance;

	public int GetCurrentPartID()
	{
		if (facePartInstance != null)
		{
			return facePartInstance.id;
		}
		return -1;
	}

	public string GetCurrentPartName()
	{
		if (facePartInstance != null)
		{
			return facePartInstance.id.ToString();
		}
		return "Empty";
	}

	public void ChangePart(CustomFacePart newInstance)
	{
		if ((bool)facePartInstance)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(facePartInstance.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(facePartInstance.gameObject);
			}
		}
		facePartInstance = newInstance;
		newInstance.transform.SetParent(socket);
		newInstance.transform.localPosition = Vector3.zero;
		newInstance.transform.localRotation = Quaternion.identity;
	}

	public void RefreshThisPart()
	{
		if ((bool)facePartInstance)
		{
			facePartInstance.SetInfo(partInfo, parent);
		}
	}
}
