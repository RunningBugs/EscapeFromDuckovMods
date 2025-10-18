using UnityEngine;

public class DarkRoomFade : MonoBehaviour
{
	public float maxRange = 100f;

	public float speed = 20f;

	public Renderer[] renderers;

	private Vector3 startPos;

	private float range;

	private bool started;

	public void StartFade()
	{
		started = true;
		base.enabled = true;
		startPos = CharacterMainControl.Main.transform.position;
	}

	private void Awake()
	{
		range = 0f;
		UpdateMaterial();
		if (!started)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (!started)
		{
			base.enabled = false;
		}
		range += speed * Time.deltaTime;
		UpdateMaterial();
		if (range > maxRange)
		{
			base.enabled = false;
		}
	}

	private void UpdateMaterial()
	{
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		materialPropertyBlock.SetFloat("_Range", range);
		materialPropertyBlock.SetVector("_CenterPos", startPos);
		Renderer[] array = renderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetPropertyBlock(materialPropertyBlock);
		}
	}

	private void Collect()
	{
		renderers = GetComponentsInChildren<Renderer>();
		Renderer[] array = renderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].sharedMaterial.SetFloat("_Range", 0f);
		}
	}

	public void SetRenderers(bool enable)
	{
		Renderer[] array = renderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].enabled = enable;
		}
	}

	public static void SetRenderersEnable(bool enable)
	{
		DarkRoomFade[] array = Object.FindObjectsByType<DarkRoomFade>(FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetRenderers(enable);
		}
	}
}
