using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DevCam : MonoBehaviour
{
	public Camera devCamera;

	public Transform postTarget;

	private bool active;

	public Transform root;

	public static bool devCamOn;

	private float timer = 1.5f;

	private int pressCounter;

	private void Awake()
	{
		root.gameObject.SetActive(value: false);
		Shader.SetGlobalFloat("DevCamOn", 0f);
		devCamOn = false;
	}

	private void Toggle()
	{
		active = true;
		devCamOn = active;
		Shader.SetGlobalFloat("DevCamOn", active ? 1f : 0f);
		root.gameObject.SetActive(active);
		for (int i = 0; i < Display.displays.Length; i++)
		{
			if (i == 1 && active)
			{
				Display.displays[i].Activate();
			}
		}
		UniversalRenderPipelineAsset universalRenderPipelineAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
		if (universalRenderPipelineAsset != null)
		{
			universalRenderPipelineAsset.shadowDistance = 500f;
		}
	}

	private void OnDestroy()
	{
		devCamOn = false;
	}

	private void Update()
	{
		if (!LevelManager.LevelInited || Gamepad.all.Count <= 0)
		{
			return;
		}
		timer -= Time.deltaTime;
		if (timer <= 0f)
		{
			timer = 0f;
			pressCounter = 0;
		}
		if (Gamepad.current.leftStickButton.isPressed && Gamepad.current.rightStickButton.wasPressedThisFrame)
		{
			pressCounter++;
			timer = 1.5f;
			Debug.Log("Toggle Dev Cam");
			if (pressCounter >= 2)
			{
				pressCounter = 0;
				Toggle();
			}
		}
		if (CharacterMainControl.Main != null)
		{
			postTarget.position = CharacterMainControl.Main.transform.position;
		}
	}
}
