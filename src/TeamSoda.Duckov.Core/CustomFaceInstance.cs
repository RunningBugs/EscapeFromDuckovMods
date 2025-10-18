using System;
using Duckov.Utilities;
using UnityEngine;

public class CustomFaceInstance : MonoBehaviour
{
	public Renderer[] mainRenderers;

	public CustomFaceHeadSetting headSetting;

	public Transform headJoint;

	public Transform foreheadJoint;

	public float foreheadDefaultHeight = 0.19f;

	public Transform helmatSocket;

	public Transform faceMaskSocket;

	public float helmatSocketYoffset = -0.2f;

	public Transform hairSocket;

	[SerializeField]
	public CustomFacePartUtility hairPart;

	[SerializeField]
	public CustomFacePartUtility eyePart;

	[SerializeField]
	public CustomFacePartUtility eyebrowPart;

	[SerializeField]
	public CustomFacePartUtility mouthPart;

	[SerializeField]
	public CustomFacePartUtility tailPart;

	[SerializeField]
	public CustomFacePartUtility wingLPart;

	[SerializeField]
	public CustomFacePartUtility wingRPart;

	[SerializeField]
	public CustomFacePartUtility footRPart;

	[SerializeField]
	public CustomFacePartUtility footLPart;

	public Transform leftHandSocket;

	public Transform righthandSocket;

	private MaterialPropertyBlock mainBlock;

	[SerializeField]
	private CharacterSubVisuals subvisuals;

	[SerializeField]
	private CustomFaceSettingData convertedData;

	private CustomFaceData data => GameplayDataSettings.CustomFaceData;

	public event Action OnLoadFaceData;

	private void TestConvert()
	{
		convertedData = ConvertToSaveData();
	}

	public void AddRendererToSubVisual(Renderer renderer)
	{
		if ((bool)subvisuals)
		{
			subvisuals.AddRenderer(renderer);
		}
	}

	private (CustomFacePartUtility, CustomFacePartUtility, CustomFacePartCollection) GetDataByPartType(CustomFacePartTypes type)
	{
		CustomFacePartUtility item = null;
		CustomFacePartUtility item2 = null;
		CustomFacePartCollection item3 = null;
		switch (type)
		{
		case CustomFacePartTypes.hair:
			item = hairPart;
			item3 = data.Hairs;
			break;
		case CustomFacePartTypes.eye:
			item = eyePart;
			item3 = data.Eyes;
			break;
		case CustomFacePartTypes.eyebrow:
			item = eyebrowPart;
			item3 = data.Eyebrows;
			break;
		case CustomFacePartTypes.mouth:
			item = mouthPart;
			item3 = data.Mouths;
			break;
		case CustomFacePartTypes.tail:
			item = tailPart;
			item3 = data.Tails;
			break;
		case CustomFacePartTypes.foot:
			item = footLPart;
			item2 = footRPart;
			item3 = data.Foots;
			break;
		case CustomFacePartTypes.wing:
			item = wingLPart;
			item2 = wingRPart;
			item3 = data.Wings;
			break;
		}
		return (item, item2, item3);
	}

	public CustomFacePart SwitchPart(CustomFacePartTypes type, CustomFaceInstance parent, int direction)
	{
		var (customFacePartUtility, customFacePartUtility2, customFacePartCollection) = GetDataByPartType(type);
		if (customFacePartUtility == null || customFacePartCollection == null || customFacePartCollection.totalCount <= 0)
		{
			return null;
		}
		CustomFacePart nextOrPrevPrefab = customFacePartCollection.GetNextOrPrevPrefab(customFacePartUtility.GetCurrentPartID(), direction);
		if (!nextOrPrevPrefab)
		{
			return null;
		}
		CustomFacePart result = SwitchOnePartInternal(customFacePartUtility, nextOrPrevPrefab, parent, mirror: false);
		if (customFacePartUtility2 != null)
		{
			SwitchOnePartInternal(customFacePartUtility2, nextOrPrevPrefab, parent, mirror: true);
		}
		return result;
	}

	private CustomFacePart ChangePart(CustomFacePartTypes type, CustomFaceInstance parent, int id)
	{
		var (customFacePartUtility, customFacePartUtility2, customFacePartCollection) = GetDataByPartType(type);
		if (customFacePartUtility == null || customFacePartCollection == null || customFacePartCollection.totalCount <= 0)
		{
			return null;
		}
		CustomFacePart partPrefab = customFacePartCollection.GetPartPrefab(id);
		if (!partPrefab)
		{
			return null;
		}
		CustomFacePart result = SwitchOnePartInternal(customFacePartUtility, partPrefab, parent, mirror: false);
		if (customFacePartUtility2 != null)
		{
			SwitchOnePartInternal(customFacePartUtility2, partPrefab, parent, mirror: true);
		}
		return result;
	}

	private CustomFacePart SwitchOnePartInternal(CustomFacePartUtility part, CustomFacePart pfb, CustomFaceInstance parent, bool mirror)
	{
		CustomFacePart customFacePart = InstantiatePartFromPrefab(pfb);
		customFacePart.mirror = mirror;
		part.ChangePart(customFacePart);
		part.RefreshThisPart();
		if ((bool)parent)
		{
			foreach (Renderer renderer in customFacePart.renderers)
			{
				parent.AddRendererToSubVisual(renderer);
			}
		}
		return customFacePart;
	}

	private CustomFacePart InstantiatePartFromPrefab(CustomFacePart pfb)
	{
		if (!pfb)
		{
			return null;
		}
		CustomFacePart result = null;
		if (Application.isPlaying)
		{
			result = UnityEngine.Object.Instantiate(pfb);
		}
		return result;
	}

	public void RefreshAll()
	{
		UpdateHead();
		hairPart.RefreshThisPart();
		eyePart.RefreshThisPart();
		eyebrowPart.partInfo.heightOffset = eyePart.partInfo.height;
		if ((bool)faceMaskSocket)
		{
			faceMaskSocket.localPosition = (eyePart.partInfo.height + eyePart.partInfo.heightOffset) * Vector3.up;
		}
		eyebrowPart.RefreshThisPart();
		mouthPart.RefreshThisPart();
		tailPart.RefreshThisPart();
		footLPart.RefreshThisPart();
		footRPart.RefreshThisPart();
		wingLPart.RefreshThisPart();
		wingRPart.RefreshThisPart();
		SetMainColor();
	}

	private void OnValidate()
	{
		RefreshAll();
	}

	private void LateUpdate()
	{
		UpdateHead();
		UpdateHands();
	}

	private void UpdateHands()
	{
		CustomFacePart partInstance = wingLPart.PartInstance;
		if ((bool)partInstance && (bool)partInstance.centerObject)
		{
			CustomFacePart partInstance2 = wingRPart.PartInstance;
			partInstance.centerObject.transform.position = leftHandSocket.transform.position;
			partInstance.centerObject.transform.rotation = leftHandSocket.transform.rotation;
			partInstance2.centerObject.transform.position = righthandSocket.transform.position;
			partInstance2.centerObject.transform.rotation = righthandSocket.transform.rotation;
		}
	}

	private void UpdateHead()
	{
		if ((bool)foreheadJoint)
		{
			foreheadJoint.localPosition = Vector3.up * (foreheadDefaultHeight + headSetting.foreheadHeight);
			foreheadJoint.localScale = Vector3.one + Vector3.up * (headSetting.foreheadRound - 1f);
			headJoint.localScale = Vector3.one * (headSetting.headScaleOffset + 1f);
			if ((bool)hairSocket)
			{
				hairSocket.localPosition = foreheadJoint.localPosition;
			}
			if ((bool)helmatSocket)
			{
				helmatSocket.localPosition = foreheadJoint.localPosition + Vector3.up * helmatSocketYoffset;
			}
		}
	}

	private void SetMainColor()
	{
		if (mainBlock == null)
		{
			mainBlock = new MaterialPropertyBlock();
		}
		mainBlock.SetColor("_Tint", headSetting.mainColor);
		Renderer[] array = mainRenderers;
		foreach (Renderer renderer in array)
		{
			if ((bool)renderer)
			{
				renderer.SetPropertyBlock(mainBlock);
			}
		}
	}

	public CustomFaceSettingData ConvertToSaveData()
	{
		return new CustomFaceSettingData
		{
			headSetting = headSetting,
			hairID = hairPart.GetCurrentPartID(),
			hairInfo = hairPart.partInfo,
			eyeID = eyePart.GetCurrentPartID(),
			eyeInfo = eyePart.partInfo,
			eyebrowID = eyebrowPart.GetCurrentPartID(),
			eyebrowInfo = eyebrowPart.partInfo,
			mouthID = mouthPart.GetCurrentPartID(),
			mouthInfo = mouthPart.partInfo,
			tailID = tailPart.GetCurrentPartID(),
			tailInfo = tailPart.partInfo,
			footID = footLPart.GetCurrentPartID(),
			footInfo = footLPart.partInfo,
			wingID = wingLPart.GetCurrentPartID(),
			wingInfo = wingLPart.partInfo
		};
	}

	public void LoadFromData(CustomFaceSettingData saveData)
	{
		headSetting = saveData.headSetting;
		hairPart.partInfo = saveData.hairInfo;
		eyePart.partInfo = saveData.eyeInfo;
		eyebrowPart.partInfo = saveData.eyebrowInfo;
		mouthPart.partInfo = saveData.mouthInfo;
		tailPart.partInfo = saveData.tailInfo;
		footLPart.partInfo = saveData.footInfo;
		footRPart.partInfo = saveData.footInfo;
		wingLPart.partInfo = saveData.wingInfo;
		wingRPart.partInfo = saveData.wingInfo;
		ChangePart(CustomFacePartTypes.hair, this, saveData.hairID);
		ChangePart(CustomFacePartTypes.eye, this, saveData.eyeID);
		ChangePart(CustomFacePartTypes.eyebrow, this, saveData.eyebrowID);
		ChangePart(CustomFacePartTypes.mouth, this, saveData.mouthID);
		ChangePart(CustomFacePartTypes.tail, this, saveData.tailID);
		ChangePart(CustomFacePartTypes.foot, this, saveData.footID);
		ChangePart(CustomFacePartTypes.wing, this, saveData.wingID);
		RefreshAll();
		this.OnLoadFaceData?.Invoke();
	}
}
