using UnityEngine;

namespace VLB;

public abstract class VolumetricLightBeamAbstractBase : MonoBehaviour
{
	public delegate void BeamGeometryGeneratedHandler(VolumetricLightBeamAbstractBase beam);

	public enum AttachedLightType
	{
		NoLight,
		OtherLight,
		SpotLight
	}

	public const string ClassName = "VolumetricLightBeamAbstractBase";

	[SerializeField]
	protected int pluginVersion = -1;

	protected Light m_CachedLightSpot;

	public bool hasGeometry => GetBeamGeometry() != null;

	public Bounds bounds
	{
		get
		{
			if (!(GetBeamGeometry() != null))
			{
				return new Bounds(Vector3.zero, Vector3.zero);
			}
			return GetBeamGeometry().meshRenderer.bounds;
		}
	}

	public int _INTERNAL_pluginVersion => pluginVersion;

	public Light lightSpotAttached => m_CachedLightSpot;

	private event BeamGeometryGeneratedHandler BeamGeometryGeneratedEvent;

	public abstract BeamGeometryAbstractBase GetBeamGeometry();

	protected abstract void SetBeamGeometryNull();

	public void RegisterBeamGeometryGeneratedCallback(BeamGeometryGeneratedHandler callback)
	{
		if (hasGeometry)
		{
			callback(this);
		}
		else
		{
			BeamGeometryGeneratedEvent += callback;
		}
	}

	public virtual void GenerateGeometry()
	{
		if (this.BeamGeometryGeneratedEvent != null)
		{
			this.BeamGeometryGeneratedEvent(this);
			this.BeamGeometryGeneratedEvent = null;
		}
	}

	public abstract bool IsScalable();

	public abstract Vector3 GetLossyScale();

	public virtual void CopyPropsFrom(VolumetricLightBeamAbstractBase beamSrc, BeamProps beamProps)
	{
		if (beamProps.HasFlag(BeamProps.Transform))
		{
			base.transform.position = beamSrc.transform.position;
			base.transform.rotation = beamSrc.transform.rotation;
			base.transform.localScale = beamSrc.transform.localScale;
		}
		if (beamProps.HasFlag(BeamProps.SideSoftness))
		{
			UtilsBeamProps.SetThickness(this, UtilsBeamProps.GetThickness(beamSrc));
		}
	}

	public Light GetLightSpotAttachedSlow(out AttachedLightType lightType)
	{
		Light component = GetComponent<Light>();
		if ((bool)component)
		{
			if (component.type == LightType.Spot)
			{
				lightType = AttachedLightType.SpotLight;
				return component;
			}
			lightType = AttachedLightType.OtherLight;
			return null;
		}
		lightType = AttachedLightType.NoLight;
		return null;
	}

	protected void InitLightSpotAttachedCached()
	{
		m_CachedLightSpot = GetLightSpotAttachedSlow(out var _);
	}

	private void OnDestroy()
	{
		DestroyBeam();
	}

	protected void DestroyBeam()
	{
		if (Application.isPlaying)
		{
			BeamGeometryAbstractBase.DestroyBeamGeometryGameObject(GetBeamGeometry());
		}
		SetBeamGeometryNull();
	}
}
