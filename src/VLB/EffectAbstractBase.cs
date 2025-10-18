using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace VLB;

[AddComponentMenu("")]
public class EffectAbstractBase : MonoBehaviour
{
	[Flags]
	public enum ComponentsToChange
	{
		UnityLight = 1,
		VolumetricLightBeam = 2,
		VolumetricDustParticles = 4
	}

	public const string ClassName = "EffectAbstractBase";

	public ComponentsToChange componentsToChange = (ComponentsToChange)2147483647;

	[FormerlySerializedAs("restoreBaseIntensity")]
	public bool restoreIntensityOnDisable = true;

	protected VolumetricLightBeamAbstractBase m_Beam;

	protected Light m_Light;

	protected VolumetricDustParticles m_Particles;

	protected float m_BaseIntensityBeamInside;

	protected float m_BaseIntensityBeamOutside;

	protected float m_BaseIntensityLight;

	[Obsolete("Use 'restoreIntensityOnDisable' instead")]
	public bool restoreBaseIntensity
	{
		get
		{
			return restoreIntensityOnDisable;
		}
		set
		{
			restoreIntensityOnDisable = value;
		}
	}

	public virtual void InitFrom(EffectAbstractBase Source)
	{
		if ((bool)Source)
		{
			componentsToChange = Source.componentsToChange;
			restoreIntensityOnDisable = Source.restoreIntensityOnDisable;
		}
	}

	private void GetIntensity(VolumetricLightBeamSD beam)
	{
		if ((bool)beam)
		{
			m_BaseIntensityBeamInside = beam.intensityInside;
			m_BaseIntensityBeamOutside = beam.intensityOutside;
		}
	}

	private void GetIntensity(VolumetricLightBeamHD beam)
	{
		if ((bool)beam)
		{
			m_BaseIntensityBeamOutside = beam.intensity;
		}
	}

	private void SetIntensity(VolumetricLightBeamSD beam, float additive)
	{
		if ((bool)beam)
		{
			beam.intensityInside = Mathf.Max(0f, m_BaseIntensityBeamInside + additive);
			beam.intensityOutside = Mathf.Max(0f, m_BaseIntensityBeamOutside + additive);
		}
	}

	private void SetIntensity(VolumetricLightBeamHD beam, float additive)
	{
		if ((bool)beam)
		{
			beam.intensity = Mathf.Max(0f, m_BaseIntensityBeamOutside + additive);
		}
	}

	protected void SetAdditiveIntensity(float additive)
	{
		if (componentsToChange.HasFlag(ComponentsToChange.VolumetricLightBeam) && (bool)m_Beam)
		{
			SetIntensity(m_Beam as VolumetricLightBeamSD, additive);
			SetIntensity(m_Beam as VolumetricLightBeamHD, additive);
		}
		if (componentsToChange.HasFlag(ComponentsToChange.UnityLight) && (bool)m_Light)
		{
			m_Light.intensity = Mathf.Max(0f, m_BaseIntensityLight + additive);
		}
		if (componentsToChange.HasFlag(ComponentsToChange.VolumetricDustParticles) && (bool)m_Particles)
		{
			m_Particles.alphaAdditionalRuntime = 1f + additive;
		}
	}

	private void Awake()
	{
		m_Beam = GetComponent<VolumetricLightBeamAbstractBase>();
		m_Light = GetComponent<Light>();
		m_Particles = GetComponent<VolumetricDustParticles>();
		GetIntensity(m_Beam as VolumetricLightBeamSD);
		GetIntensity(m_Beam as VolumetricLightBeamHD);
		m_BaseIntensityLight = (m_Light ? m_Light.intensity : 0f);
	}

	protected virtual void OnEnable()
	{
		StopAllCoroutines();
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		if (restoreIntensityOnDisable)
		{
			SetAdditiveIntensity(0f);
		}
	}
}
