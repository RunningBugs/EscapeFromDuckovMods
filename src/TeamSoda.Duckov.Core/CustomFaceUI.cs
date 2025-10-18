using System;
using System.Collections.Generic;
using Duckov.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomFaceUI : MonoBehaviour
{
	private static CustomFaceUI activeView;

	public List<CustomFaceTabs> tabs;

	public List<GameObject> panels;

	private CustomFaceInstance faceInstance;

	[SerializeField]
	private float rotateSpeed = 15f;

	public CustomFaceUIColorPicker skinColorPicker;

	public CustomFaceSlider headSizeSlider;

	public CustomFaceSlider headHeightSlider;

	public CustomFaceSlider headRoundnessSlider;

	public CustomFaceUISwitch hairSwitch;

	public CustomFaceUIColorPicker hairColorPicker;

	public CustomFaceUISwitch eyeSwitch;

	public CustomFaceUIColorPicker eyeColorPicker;

	public CustomFaceSlider eyeDistanceSlider;

	public CustomFaceSlider eyeHeightSlider;

	public CustomFaceSlider eyeSizeSlider;

	public CustomFaceSlider eyeTwistSlider;

	public CustomFaceUISwitch eyebrowSwitch;

	public CustomFaceUIColorPicker eyebrowColorPicker;

	public CustomFaceSlider eyebrowDistanceSlider;

	public CustomFaceSlider eyebrowHeightSlider;

	public CustomFaceSlider eyebrowSizeSlider;

	public CustomFaceSlider eyebrowTwistSlider;

	public CustomFaceUISwitch mouthSwitch;

	public CustomFaceUIColorPicker mouthColorPicker;

	public CustomFaceSlider mouthSizeSlider;

	public CustomFaceSlider mouthHeightSlider;

	public CustomFaceSlider mouthLeftRightSlider;

	public CustomFaceSlider mouthTwistSlider;

	public CustomFaceUISwitch wingSwitch;

	public CustomFaceUIColorPicker wingColorPicker;

	public CustomFaceSlider wingSizeSlider;

	public CustomFaceUISwitch tailSwitch;

	public CustomFaceUIColorPicker tailColorPicker;

	public CustomFaceSlider tailSizeSlider;

	public CustomFaceUISwitch footSwitch;

	public CustomFaceSlider footSizeSlider;

	private bool dirty;

	public bool canControl;

	public List<CustomFacePreset> presets;

	public static CustomFaceUI ActiveView
	{
		get
		{
			if ((bool)activeView && activeView.gameObject.activeInHierarchy)
			{
				return activeView;
			}
			activeView = null;
			return null;
		}
	}

	public static event Action OnCustomUIViewChanged;

	private event Action onLoadValues;

	public void SetFace(CustomFaceInstance face)
	{
		if (faceInstance != null)
		{
			faceInstance.OnLoadFaceData -= OnLoadFaceData;
		}
		faceInstance = face;
		faceInstance.OnLoadFaceData += OnLoadFaceData;
		Init();
	}

	private void OnLoadFaceData()
	{
		LoadValues();
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		if (faceInstance != null)
		{
			faceInstance.OnLoadFaceData -= OnLoadFaceData;
		}
		if (activeView == this)
		{
			activeView = null;
		}
		CustomFaceUI.OnCustomUIViewChanged?.Invoke();
	}

	private void OnEnable()
	{
		SelectTab(tabs[0]);
		activeView = this;
		CustomFaceUI.OnCustomUIViewChanged?.Invoke();
	}

	private void OnDisable()
	{
		if (activeView == this)
		{
			activeView = null;
		}
		CustomFaceUI.OnCustomUIViewChanged?.Invoke();
	}

	private void Init()
	{
		skinColorPicker.Init(this, "UI_CustomFace_SkinColor");
		headSizeSlider.Init(0.6f, 1.4f, this, "UI_CustomFace_HeadSize");
		headHeightSlider.Init(0f, 0.6f, this, "UI_CustomFace_HeadHeight");
		headRoundnessSlider.Init(0.35f, 1f, this, "UI_CustomFace_HeadRoundness");
		hairSwitch.Init(this, CustomFacePartTypes.hair, "UI_CustomFace_HairType");
		hairColorPicker.Init(this, "UI_CustomFace_HairColor");
		eyeSwitch.Init(this, CustomFacePartTypes.eye, "UI_CustomFace_EyeType");
		eyeColorPicker.Init(this, "UI_CustomFace_EyeColor");
		eyeDistanceSlider.Init(0f, 90f, this, "UI_CustomFace_EyeSpace");
		eyeHeightSlider.Init(-0.3f, 0.3f, this, "UI_CustomFace_EyeHeight");
		eyeSizeSlider.Init(0.3f, 4f, this, "UI_CustomFace_EyeSize");
		eyeTwistSlider.Init(-90f, 90f, this, "UI_CustomFace_EyeRotate");
		eyebrowSwitch.Init(this, CustomFacePartTypes.eyebrow, "UI_CustomFace_EyebrowType");
		eyebrowColorPicker.Init(this, "UI_CustomFace_EyebrowColor");
		eyebrowDistanceSlider.Init(0f, 90f, this, "UI_CustomFace_EyebrowSpace");
		eyebrowHeightSlider.Init(-0.3f, 0.3f, this, "UI_CustomFace_EyebrowHeight");
		eyebrowSizeSlider.Init(0.3f, 4f, this, "UI_CustomFace_EyebrowSize");
		eyebrowTwistSlider.Init(-90f, 90f, this, "UI_CustomFace_EyebrowRotate");
		mouthSwitch.Init(this, CustomFacePartTypes.mouth, "UI_CustomFace_MouthType");
		mouthColorPicker.Init(this, "UI_CustomFace_MouthColor");
		mouthSizeSlider.Init(0.3f, 4f, this, "UI_CustomFace_MouthSize");
		mouthHeightSlider.Init(-0.3f, 0.3f, this, "UI_CustomFace_MouthHeight");
		mouthLeftRightSlider.Init(-50f, 50f, this, "UI_CustomFace_MouthOffset");
		mouthTwistSlider.Init(-90f, 90f, this, "UI_CustomFace_MouthRotate");
		wingSwitch.Init(this, CustomFacePartTypes.wing, "UI_CustomFace_WingType");
		wingColorPicker.Init(this, "UI_CustomFace_WingColor");
		wingSizeSlider.Init(0.5f, 2f, this, "UI_CustomFace_WingSize");
		tailSwitch.Init(this, CustomFacePartTypes.tail, "UI_CustomFace_TailType");
		tailColorPicker.Init(this, "UI_CustomFace_TailColor");
		tailSizeSlider.Init(0.3f, 2f, this, "UI_CustomFace_TailSize");
		footSwitch.Init(this, CustomFacePartTypes.foot, "UI_CustomFace_FootType");
		footSizeSlider.Init(0.5f, 1.5f, this, "UI_CustomFace_FootSize");
		LoadValues();
		foreach (CustomFaceTabs tab in tabs)
		{
			tab.master = this;
		}
	}

	private void LoadValues()
	{
		skinColorPicker.SetColor(faceInstance.headSetting.mainColor);
		headSizeSlider.SetValue(1f + faceInstance.headSetting.headScaleOffset);
		headHeightSlider.SetValue(faceInstance.headSetting.foreheadHeight);
		headRoundnessSlider.SetValue(faceInstance.headSetting.foreheadRound);
		hairSwitch.SetName(faceInstance.hairPart.GetCurrentPartName());
		hairColorPicker.SetColor(faceInstance.hairPart.partInfo.color);
		eyeSwitch.SetName(faceInstance.eyePart.GetCurrentPartName());
		eyeColorPicker.SetColor(faceInstance.eyePart.partInfo.color);
		eyeDistanceSlider.SetValue(faceInstance.eyePart.partInfo.distanceAngle);
		eyeHeightSlider.SetValue(faceInstance.eyePart.partInfo.height);
		eyeSizeSlider.SetValue(faceInstance.eyePart.partInfo.scale);
		eyeTwistSlider.SetValue(faceInstance.eyePart.partInfo.twist);
		eyebrowSwitch.SetName(faceInstance.eyebrowPart.GetCurrentPartName());
		eyebrowColorPicker.SetColor(faceInstance.eyebrowPart.partInfo.color);
		eyebrowDistanceSlider.SetValue(faceInstance.eyebrowPart.partInfo.distanceAngle);
		eyebrowHeightSlider.SetValue(faceInstance.eyebrowPart.partInfo.height);
		eyebrowSizeSlider.SetValue(faceInstance.eyebrowPart.partInfo.scale);
		eyebrowTwistSlider.SetValue(faceInstance.eyebrowPart.partInfo.twist);
		mouthSwitch.SetName(faceInstance.mouthPart.GetCurrentPartName());
		mouthColorPicker.SetColor(faceInstance.mouthPart.partInfo.color);
		mouthSizeSlider.SetValue(faceInstance.mouthPart.partInfo.scale);
		mouthHeightSlider.SetValue(faceInstance.mouthPart.partInfo.height);
		mouthLeftRightSlider.SetValue(faceInstance.mouthPart.partInfo.leftRightAngle);
		mouthTwistSlider.SetValue(faceInstance.mouthPart.partInfo.twist);
		wingSwitch.SetName(faceInstance.wingLPart.GetCurrentPartName());
		wingColorPicker.SetColor(faceInstance.wingLPart.partInfo.color);
		wingSizeSlider.SetValue(faceInstance.wingLPart.partInfo.scale);
		tailSwitch.SetName(faceInstance.tailPart.GetCurrentPartName());
		tailColorPicker.SetColor(faceInstance.tailPart.partInfo.color);
		tailSizeSlider.SetValue(faceInstance.tailPart.partInfo.scale);
		footSwitch.SetName(faceInstance.footLPart.GetCurrentPartName());
		footSizeSlider.SetValue(faceInstance.footLPart.partInfo.scale);
	}

	public void SelectTab(CustomFaceTabs tab)
	{
		foreach (GameObject panel in panels)
		{
			panel.SetActive(tab.panels.Contains(panel));
		}
		foreach (CustomFaceTabs tab2 in tabs)
		{
			tab2.SetSelectVisual(tab == tab2);
		}
	}

	public void SetDirty()
	{
		dirty = true;
	}

	private void LateUpdate()
	{
		if (dirty && (bool)faceInstance)
		{
			RefreshInfos();
			dirty = false;
		}
	}

	public void RefreshInfos()
	{
		if (!(faceInstance == null))
		{
			faceInstance.headSetting.mainColor = skinColorPicker.CurrentColor;
			faceInstance.headSetting.headScaleOffset = headSizeSlider.Value - 1f;
			faceInstance.headSetting.foreheadHeight = headHeightSlider.Value;
			faceInstance.headSetting.foreheadRound = headRoundnessSlider.Value;
			faceInstance.hairPart.partInfo.color = hairColorPicker.CurrentColor;
			faceInstance.eyePart.partInfo.color = eyeColorPicker.CurrentColor;
			faceInstance.eyePart.partInfo.distanceAngle = eyeDistanceSlider.Value;
			faceInstance.eyePart.partInfo.height = eyeHeightSlider.Value;
			faceInstance.eyePart.partInfo.scale = eyeSizeSlider.Value;
			faceInstance.eyePart.partInfo.twist = eyeTwistSlider.Value;
			faceInstance.eyebrowPart.partInfo.color = eyebrowColorPicker.CurrentColor;
			faceInstance.eyebrowPart.partInfo.distanceAngle = eyebrowDistanceSlider.Value;
			faceInstance.eyebrowPart.partInfo.height = eyebrowHeightSlider.Value;
			faceInstance.eyebrowPart.partInfo.scale = eyebrowSizeSlider.Value;
			faceInstance.eyebrowPart.partInfo.twist = eyebrowTwistSlider.Value;
			faceInstance.mouthPart.partInfo.color = mouthColorPicker.CurrentColor;
			faceInstance.mouthPart.partInfo.scale = mouthSizeSlider.Value;
			faceInstance.mouthPart.partInfo.height = mouthHeightSlider.Value;
			faceInstance.mouthPart.partInfo.leftRightAngle = mouthLeftRightSlider.Value;
			faceInstance.mouthPart.partInfo.twist = mouthTwistSlider.Value;
			faceInstance.wingLPart.partInfo.color = wingColorPicker.CurrentColor;
			faceInstance.wingRPart.partInfo.color = wingColorPicker.CurrentColor;
			faceInstance.wingLPart.partInfo.scale = wingSizeSlider.Value;
			faceInstance.wingRPart.partInfo.scale = wingSizeSlider.Value;
			faceInstance.tailPart.partInfo.color = tailColorPicker.CurrentColor;
			faceInstance.tailPart.partInfo.scale = tailSizeSlider.Value;
			faceInstance.footLPart.partInfo.scale = footSizeSlider.Value;
			faceInstance.footRPart.partInfo.scale = footSizeSlider.Value;
			faceInstance.RefreshAll();
		}
	}

	public string SwitchPart(CustomFacePartTypes type, int direction)
	{
		if (faceInstance == null)
		{
			return "";
		}
		CustomFacePart customFacePart = faceInstance.SwitchPart(type, faceInstance, direction);
		if (customFacePart == null)
		{
			return "";
		}
		return customFacePart.id.ToString();
	}

	public void SaveToMainCharacter()
	{
		if ((bool)faceInstance && canControl)
		{
			CustomFaceSettingData setting = faceInstance.ConvertToSaveData();
			LevelManager.Instance.CustomFaceManager.SaveSettingToMainCharacter(setting);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!(faceInstance == null) && eventData.button == PointerEventData.InputButton.Left)
		{
			float angle = (0f - eventData.delta.x) * rotateSpeed * Time.deltaTime;
			faceInstance.transform.Rotate(Vector3.up, angle);
		}
	}

	public void RandomPreset()
	{
		faceInstance.LoadFromData(presets.GetRandom().settings);
	}
}
