using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class OcclusionFadeObject : MonoBehaviour
{
	public OcclusionFadeTypes fadeType;

	public string topName = "Fade";

	public OcclusionFadeTrigger[] triggers;

	public Renderer[] renderers;

	public List<Material> originMaterials;

	private List<Material> tempMaterials;

	private Transform topTransform;

	private int enterCounter;

	private bool hiding;

	private bool triggerEnabled = true;

	private void Collect()
	{
		CollectTriggers();
		CollectRenderers();
	}

	private void CollectTriggers()
	{
		triggers = new OcclusionFadeTrigger[0];
		triggers = GetComponentsInChildren<OcclusionFadeTrigger>();
		if (triggers.Length == 0)
		{
			return;
		}
		OcclusionFadeTrigger[] array = triggers;
		foreach (OcclusionFadeTrigger obj in array)
		{
			obj.parent = this;
			Collider[] componentsInChildren = obj.GetComponentsInChildren<Collider>(includeInactive: true);
			if (componentsInChildren.Length != 0)
			{
				Collider[] array2 = componentsInChildren;
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j].isTrigger = true;
				}
			}
		}
	}

	private void CollectRenderers()
	{
		topTransform = FindFirst(base.transform, topName);
		if (!(topTransform == null))
		{
			renderers = topTransform.GetComponentsInChildren<Renderer>(includeInactive: true);
			originMaterials.Clear();
			Renderer[] array = renderers;
			foreach (Renderer renderer in array)
			{
				originMaterials.AddRange(renderer.sharedMaterials);
			}
		}
	}

	public void OnEnter()
	{
		enterCounter++;
		Refresh();
	}

	public void OnLeave()
	{
		enterCounter--;
		Refresh();
	}

	private void Refresh()
	{
		SyncEnable();
		if (!triggerEnabled)
		{
			hiding = false;
			Sync();
		}
		else if (enterCounter > 0 && !hiding)
		{
			hiding = true;
			Sync();
		}
		else if (enterCounter <= 0 && hiding)
		{
			hiding = false;
			Sync();
		}
	}

	private void OnEnable()
	{
		SyncEnable();
	}

	private void OnDisable()
	{
		SyncEnable();
	}

	private void SyncEnable()
	{
		if (triggerEnabled != base.enabled)
		{
			OcclusionFadeTrigger[] array = triggers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].gameObject.SetActive(base.enabled);
			}
			triggerEnabled = base.enabled;
		}
	}

	private void Sync()
	{
		SyncEnable();
		switch (fadeType)
		{
		case OcclusionFadeTypes.Fade:
		{
			if (tempMaterials == null)
			{
				tempMaterials = new List<Material>();
			}
			Renderer[] array;
			if (hiding)
			{
				int num = 0;
				array = renderers;
				foreach (Renderer renderer3 in array)
				{
					if (!(renderer3 == null))
					{
						tempMaterials.Clear();
						for (int j = 0; j < renderer3.materials.Length; j++)
						{
							Material mat = originMaterials[num];
							Material maskedMaterial = OcclusionFadeManager.Instance.GetMaskedMaterial(mat);
							tempMaterials.Add(maskedMaterial);
							num++;
						}
						renderer3.SetSharedMaterials(tempMaterials);
					}
				}
				break;
			}
			int num2 = 0;
			array = renderers;
			foreach (Renderer renderer4 in array)
			{
				if (!(renderer4 == null))
				{
					tempMaterials.Clear();
					for (int k = 0; k < renderer4.materials.Length; k++)
					{
						tempMaterials.Add(originMaterials[num2]);
						num2++;
					}
					renderer4.SetSharedMaterials(tempMaterials);
				}
			}
			break;
		}
		case OcclusionFadeTypes.ShadowOnly:
		{
			Renderer[] array;
			if (hiding)
			{
				array = renderers;
				foreach (Renderer renderer in array)
				{
					if (!(renderer == null))
					{
						renderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
					}
				}
				break;
			}
			array = renderers;
			foreach (Renderer renderer2 in array)
			{
				if (!(renderer2 == null))
				{
					renderer2.shadowCastingMode = ShadowCastingMode.On;
				}
			}
			break;
		}
		}
	}

	private void Hide()
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i]?.gameObject.SetActive(value: false);
		}
	}

	private void Show()
	{
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i]?.gameObject.SetActive(value: true);
		}
	}

	private Transform FindFirst(Transform root, string checkName)
	{
		for (int i = 0; i < root.childCount; i++)
		{
			Transform child = root.GetChild(i);
			if (child.name == checkName)
			{
				return child;
			}
			if (child.childCount > 0)
			{
				Transform transform = FindFirst(child, checkName);
				if (transform != null)
				{
					return transform;
				}
			}
		}
		return null;
	}
}
