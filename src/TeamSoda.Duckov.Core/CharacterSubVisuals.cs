using System.Collections.Generic;
using UnityEngine;

public class CharacterSubVisuals : MonoBehaviour
{
	private CharacterMainControl character;

	public List<Renderer> renderers;

	public List<ParticleSystem> particles;

	public List<Light> lights;

	public List<SodaPointLight> sodaPointLights;

	private int hiddenLayer;

	private int showLayer;

	private int sodaLightShowLayer;

	private bool hidden;

	private bool layerInited;

	public bool logWhenSetVisual;

	public CharacterModel mainModel;

	public bool debug;

	private void InitLayers()
	{
		if (!layerInited)
		{
			layerInited = true;
			hiddenLayer = LayerMask.NameToLayer("SpecialCamera");
			showLayer = LayerMask.NameToLayer("Character");
			sodaLightShowLayer = LayerMask.NameToLayer("SodaLight");
		}
	}

	private void SetRenderers()
	{
		renderers.Clear();
		particles.Clear();
		lights.Clear();
		sodaPointLights.Clear();
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>(includeInactive: true);
		foreach (Renderer renderer in componentsInChildren)
		{
			ParticleSystem component = renderer.GetComponent<ParticleSystem>();
			if ((bool)component)
			{
				particles.Add(component);
				continue;
			}
			SodaPointLight component2 = renderer.GetComponent<SodaPointLight>();
			if ((bool)component2)
			{
				sodaPointLights.Add(component2);
			}
			else
			{
				renderers.Add(renderer);
			}
		}
		Light[] componentsInChildren2 = GetComponentsInChildren<Light>(includeInactive: true);
		foreach (Light item in componentsInChildren2)
		{
			lights.Add(item);
		}
	}

	public void AddRenderer(Renderer renderer)
	{
		if (!(renderer == null) && !renderers.Contains(renderer))
		{
			InitLayers();
			int layer = (hidden ? hiddenLayer : showLayer);
			renderer.gameObject.layer = layer;
			renderers.Add(renderer);
			if ((bool)character)
			{
				character.RemoveVisual(this);
				character.AddSubVisuals(this);
			}
		}
	}

	public void SetRenderersHidden(bool _hidden)
	{
		hidden = _hidden;
		InitLayers();
		int layer = (_hidden ? hiddenLayer : showLayer);
		int num = renderers.Count;
		for (int i = 0; i < num; i++)
		{
			if (renderers[i] == null)
			{
				renderers.RemoveAt(i);
				i--;
				num--;
			}
			else
			{
				renderers[i].gameObject.layer = layer;
			}
		}
		int num2 = particles.Count;
		for (int j = 0; j < num2; j++)
		{
			if (particles[j] == null)
			{
				particles.RemoveAt(j);
				j--;
				num2--;
			}
			else
			{
				particles[j].gameObject.layer = layer;
			}
		}
		int num3 = lights.Count;
		for (int k = 0; k < num3; k++)
		{
			Light light = lights[k];
			if (light == null)
			{
				lights.RemoveAt(k);
				k--;
				num3--;
				continue;
			}
			light.gameObject.layer = layer;
			if (hidden)
			{
				light.cullingMask = 0;
			}
			else
			{
				light.cullingMask = -1;
			}
		}
		int layer2 = (_hidden ? hiddenLayer : sodaLightShowLayer);
		int num4 = sodaPointLights.Count;
		for (int l = 0; l < sodaPointLights.Count; l++)
		{
			if (sodaPointLights[l] == null)
			{
				sodaPointLights.RemoveAt(l);
				l--;
				num4--;
			}
			else
			{
				sodaPointLights[l].gameObject.layer = layer2;
			}
		}
	}

	private void OnTransformParentChanged()
	{
		CharacterMainControl componentInParent = GetComponentInParent<CharacterMainControl>(includeInactive: true);
		SetCharacter(componentInParent);
	}

	public void SetCharacter(CharacterMainControl newCharacter)
	{
		if (newCharacter != null)
		{
			newCharacter.AddSubVisuals(this);
			character = newCharacter;
		}
	}

	private void OnDestroy()
	{
		if (character != null)
		{
			character.RemoveVisual(this);
		}
	}
}
