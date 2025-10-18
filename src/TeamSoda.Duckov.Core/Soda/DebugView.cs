using System;
using Cysharp.Threading.Tasks;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Soda;

public class DebugView : MonoBehaviour
{
	private DebugView instance;

	private Vector2 screenRes;

	private ResModes resMode;

	private TextureModes texMode;

	public TextMeshProUGUI resText;

	public TextMeshProUGUI texText;

	public TextMeshProUGUI fpsText1;

	public TextMeshProUGUI fpsText2;

	public TextMeshProUGUI inputDeviceText;

	public TextMeshProUGUI bloomText;

	public TextMeshProUGUI edgeLightText;

	public TextMeshProUGUI aoText;

	public TextMeshProUGUI dofText;

	public TextMeshProUGUI invincibleText;

	public TextMeshProUGUI reporterText;

	public UniversalRendererData rendererData;

	private float[] deltaTimes;

	private int frameIndex;

	public int frameSampleCount = 30;

	public GameObject openButton;

	public GameObject panel;

	public VolumeProfile volumeProfile;

	private bool bloomActive;

	private bool edgeLightActive;

	private bool aoActive;

	private int inputDevice;

	private bool dofActive;

	private bool invincible;

	private bool reporterActive;

	private Light light;

	[ItemTypeID]
	public int createItemID;

	public DebugView Instance => instance;

	public bool EdgeLightActive => edgeLightActive;

	public static event Action<DebugView> OnDebugViewConfigChanged;

	private void Awake()
	{
	}

	private void OnDestroy()
	{
		LevelManager.OnLevelInitialized -= OnlevelInited;
		SceneManager.activeSceneChanged -= OnSceneLoaded;
	}

	private void InitFromData()
	{
		if (PlayerPrefs.HasKey("ResMode"))
		{
			resMode = (ResModes)PlayerPrefs.GetInt("ResMode");
		}
		else
		{
			resMode = ResModes.R720p;
		}
		if (PlayerPrefs.HasKey("TexMode"))
		{
			texMode = (TextureModes)PlayerPrefs.GetInt("TexMode");
		}
		else
		{
			texMode = TextureModes.High;
		}
		if (PlayerPrefs.HasKey("InputDevice"))
		{
			inputDevice = PlayerPrefs.GetInt("InputDevice");
		}
		else
		{
			inputDevice = 1;
		}
		if (PlayerPrefs.HasKey("BloomActive"))
		{
			bloomActive = PlayerPrefs.GetInt("BloomActive") != 0;
		}
		else
		{
			bloomActive = true;
		}
		if (PlayerPrefs.HasKey("EdgeLightActive"))
		{
			edgeLightActive = PlayerPrefs.GetInt("EdgeLightActive") != 0;
		}
		else
		{
			edgeLightActive = true;
		}
		if (PlayerPrefs.HasKey("AOActive"))
		{
			aoActive = PlayerPrefs.GetInt("AOActive") != 0;
		}
		else
		{
			aoActive = false;
		}
		if (PlayerPrefs.HasKey("DofActive"))
		{
			dofActive = PlayerPrefs.GetInt("DofActive") != 0;
		}
		else
		{
			dofActive = false;
		}
		if (PlayerPrefs.HasKey("ReporterActive"))
		{
			reporterActive = PlayerPrefs.GetInt("ReporterActive") != 0;
		}
		else
		{
			reporterActive = false;
		}
	}

	private void Update()
	{
		deltaTimes[frameIndex] = Time.deltaTime;
		frameIndex++;
		if (frameIndex >= frameSampleCount)
		{
			frameIndex = 0;
			float num = 0f;
			for (int i = 0; i < frameSampleCount; i++)
			{
				num += deltaTimes[i];
			}
			int num2 = Mathf.RoundToInt((float)frameSampleCount / Mathf.Max(0.0001f, num));
			fpsText1.text = num2.ToString();
			fpsText2.text = num2.ToString();
		}
	}

	public void SetInputDevice(int type)
	{
		type = 1;
		if (type == 0)
		{
			InputManager.SetInputDevice(InputManager.InputDevices.touch);
			inputDeviceText.text = "触摸";
			PlayerPrefs.SetInt("InputDevice", 0);
		}
		else
		{
			InputManager.SetInputDevice(InputManager.InputDevices.mouseKeyboard);
			inputDeviceText.text = "键鼠";
			PlayerPrefs.SetInt("InputDevice", 1);
		}
	}

	public void SetRes(int resModeIndex)
	{
		SetRes((ResModes)resModeIndex);
	}

	public void SetRes(ResModes mode)
	{
		resMode = mode;
		screenRes.x = Display.main.systemWidth;
		screenRes.y = Display.main.systemHeight;
		PlayerPrefs.SetInt("ResMode", (int)mode);
		int num = 1;
		int num2 = 1;
		switch (resMode)
		{
		case ResModes.Source:
			num = Mathf.RoundToInt(screenRes.x);
			num2 = Mathf.RoundToInt(screenRes.y);
			break;
		case ResModes.HalfRes:
			num = Mathf.RoundToInt(screenRes.x / 2f);
			num2 = Mathf.RoundToInt(screenRes.y / 2f);
			break;
		case ResModes.R720p:
			num = Mathf.RoundToInt(screenRes.x / screenRes.y * 720f);
			num2 = 720;
			break;
		case ResModes.R480p:
			num = Mathf.RoundToInt(screenRes.x / screenRes.y * 480f);
			num2 = 480;
			break;
		}
		resText.text = $"{num}x{num2}";
		Screen.SetResolution(num, num2, FullScreenMode.FullScreenWindow);
		DebugView.OnDebugViewConfigChanged?.Invoke(this);
	}

	public void SetTexture(int texModeIndex)
	{
		SetTexture((TextureModes)texModeIndex);
	}

	public void SetTexture(TextureModes mode)
	{
		texMode = mode;
		QualitySettings.globalTextureMipmapLimit = (int)texMode;
		switch (texMode)
		{
		case TextureModes.High:
			texText.text = "高";
			break;
		case TextureModes.Middle:
			texText.text = "中";
			break;
		case TextureModes.Low:
			texText.text = "低";
			break;
		case TextureModes.VeryLow:
			texText.text = "极低";
			break;
		}
		PlayerPrefs.SetInt("TexMode", (int)texMode);
		DebugView.OnDebugViewConfigChanged?.Invoke(this);
	}

	private void OnlevelInited()
	{
		SetInvincible(invincible);
	}

	private void OnSceneLoaded(Scene s1, Scene s2)
	{
		SetShadow().Forget();
	}

	private async UniTaskVoid SetShadow()
	{
		await UniTask.WaitForEndOfFrame(this);
		await UniTask.WaitForEndOfFrame(this);
		await UniTask.WaitForSeconds(0.2f, ignoreTimeScale: true);
		light = RenderSettings.sun;
		if ((bool)light)
		{
			light.shadows = (edgeLightActive ? LightShadows.Soft : LightShadows.None);
		}
	}

	public void ToggleBloom()
	{
		bloomActive = !bloomActive;
		SetBloom(bloomActive);
	}

	private void SetBloom(bool active)
	{
		Bloom component;
		bool num = volumeProfile.TryGet<Bloom>(out component);
		bloomText.text = (active ? "开" : "关");
		if (num)
		{
			component.active = active;
		}
		bloomActive = active;
		PlayerPrefs.SetInt("BloomActive", bloomActive ? 1 : 0);
		DebugView.OnDebugViewConfigChanged?.Invoke(this);
	}

	public void ToggleEdgeLight()
	{
		edgeLightActive = !edgeLightActive;
		SetEdgeLight(edgeLightActive);
	}

	private void SetEdgeLight(bool active)
	{
		edgeLightText.text = (active ? "开" : "关");
		edgeLightActive = active;
		PlayerPrefs.SetInt("EdgeLightActive", edgeLightActive ? 1 : 0);
		UniversalRenderPipelineAsset universalRenderPipelineAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
		if (universalRenderPipelineAsset != null)
		{
			universalRenderPipelineAsset.supportsCameraDepthTexture = (active ? true : false);
		}
		SetShadow();
		DebugView.OnDebugViewConfigChanged?.Invoke(this);
	}

	public void ToggleAO()
	{
		aoActive = !aoActive;
		SetAO(aoActive);
	}

	public void ToggleDof()
	{
		dofActive = !dofActive;
		SetDof(dofActive);
	}

	public void ToggleInvincible()
	{
		invincible = !invincible;
		SetInvincible(invincible);
	}

	private void SetReporter(bool active)
	{
	}

	public void ToggleReporter()
	{
		SetReporter(!reporterActive);
	}

	private void SetAO(bool active)
	{
		ScriptableRendererFeature scriptableRendererFeature = rendererData.rendererFeatures.Find((ScriptableRendererFeature a) => a.name == "ScreenSpaceAmbientOcclusion");
		if (scriptableRendererFeature != null)
		{
			scriptableRendererFeature.SetActive(active);
			aoText.text = (active ? "开" : "关");
			PlayerPrefs.SetInt("AOActive", active ? 1 : 0);
		}
		DebugView.OnDebugViewConfigChanged?.Invoke(this);
	}

	private void SetDof(bool active)
	{
	}

	private void SetInvincible(bool active)
	{
		invincibleText.text = (active ? "开" : "关");
		invincible = active;
		DebugView.OnDebugViewConfigChanged?.Invoke(this);
	}

	public void CreateItem()
	{
		CreateItemTask().Forget();
	}

	private async UniTaskVoid CreateItemTask()
	{
		if (CharacterMainControl.Main != null)
		{
			Item item = await ItemAssetsCollection.InstantiateAsync(createItemID);
			if (!(item == null))
			{
				item.Drop(CharacterMainControl.Main, createRigidbody: true);
			}
		}
	}
}
