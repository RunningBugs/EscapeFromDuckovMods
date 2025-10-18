using System;
using System.Collections.Generic;
using Duckov.Utilities;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class NightVisionVisual : MonoBehaviour
{
	[Serializable]
	public struct NightVisionType
	{
		public string intro;

		public VolumeProfile profile;

		[FormerlySerializedAs("thermalCharacter")]
		public bool thermalOn;

		public bool thermalBackground;
	}

	private int nightVisionType;

	public Volume thermalVolume;

	public NightVisionType[] nightVisionTypes;

	private CharacterMainControl character;

	public ScriptableRendererData rendererData;

	public List<string> renderFeatureNames;

	private ScriptableRendererFeature thermalCharacterRednerFeature;

	private ScriptableRendererFeature thermalBackgroundRednerFeature;

	public Transform nightVisionLight;

	public string thermalCharacterRednerFeatureKey = "ThermalCharacter";

	public string thermalBackgroundRednerFeatureKey = "ThermalBackground";

	private bool levelInited;

	public void Awake()
	{
		CollectRendererData();
		Refresh();
	}

	private void OnDestroy()
	{
		nightVisionType = 0;
		Refresh();
	}

	private void CollectRendererData()
	{
		if (rendererData == null)
		{
			return;
		}
		for (int i = 0; i < rendererData.rendererFeatures.Count; i++)
		{
			if (rendererData.rendererFeatures[i].name == thermalCharacterRednerFeatureKey)
			{
				thermalCharacterRednerFeature = rendererData.rendererFeatures[i];
			}
			else if (rendererData.rendererFeatures[i].name == thermalBackgroundRednerFeatureKey)
			{
				thermalBackgroundRednerFeature = rendererData.rendererFeatures[i];
			}
		}
	}

	private void Update()
	{
		bool flag = false;
		int num = CheckNightVisionType();
		if (num >= nightVisionTypes.Length)
		{
			num = 1;
		}
		if (nightVisionType != num)
		{
			nightVisionType = num;
			flag = true;
		}
		if (LevelManager.LevelInited != levelInited)
		{
			levelInited = LevelManager.LevelInited;
			flag = true;
		}
		if (flag)
		{
			Refresh();
		}
		if ((bool)character && nightVisionLight.gameObject.activeInHierarchy)
		{
			nightVisionLight.transform.position = character.transform.position + Vector3.up * 2f;
		}
	}

	private int CheckNightVisionType()
	{
		if (!character)
		{
			if (LevelManager.LevelInited)
			{
				character = CharacterMainControl.Main;
			}
			return 0;
		}
		return Mathf.RoundToInt(character.NightVisionType);
	}

	public void Refresh()
	{
		bool flag = this.nightVisionType > 0;
		thermalVolume.gameObject.SetActive(flag);
		nightVisionLight.gameObject.SetActive(flag);
		NightVisionType nightVisionType = nightVisionTypes[this.nightVisionType];
		bool flag2 = nightVisionType.thermalOn && flag;
		bool active = nightVisionType.thermalBackground && flag;
		thermalVolume.profile = nightVisionType.profile;
		thermalCharacterRednerFeature.SetActive(flag2);
		thermalBackgroundRednerFeature.SetActive(active);
		Shader.SetGlobalFloat("ThermalOn", flag2 ? 1f : 0f);
		if (LevelManager.LevelInited)
		{
			if (flag2)
			{
				LevelManager.Instance.FogOfWarManager.mainVis.ObstacleMask = GameplayDataSettings.Layers.fowBlockLayersWithThermal;
			}
			else
			{
				LevelManager.Instance.FogOfWarManager.mainVis.ObstacleMask = GameplayDataSettings.Layers.fowBlockLayers;
			}
		}
	}
}
