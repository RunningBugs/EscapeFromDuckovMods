using System;
using UnityEngine;

namespace VLB;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(VolumetricLightBeamAbstractBase))]
[HelpURL("http://saladgamer.com/vlb-doc/comp-dustparticles/")]
[AddComponentMenu("VLB/Common/Volumetric Dust Particles")]
public class VolumetricDustParticles : MonoBehaviour
{
	public const string ClassName = "VolumetricDustParticles";

	[Range(0f, 1f)]
	public float alpha = 0.5f;

	[Range(0.0001f, 0.1f)]
	public float size = 0.01f;

	public ParticlesDirection direction;

	public Vector3 velocity = Consts.DustParticles.VelocityDefault;

	[Obsolete("Use 'velocity' instead")]
	public float speed = 0.03f;

	public float density = 5f;

	[MinMaxRange(0f, 1f)]
	public MinMaxRangeFloat spawnDistanceRange = Consts.DustParticles.SpawnDistanceRangeDefault;

	[Obsolete("Use 'spawnDistanceRange' instead")]
	public float spawnMinDistance;

	[Obsolete("Use 'spawnDistanceRange' instead")]
	public float spawnMaxDistance = 0.7f;

	public bool cullingEnabled;

	public float cullingMaxDistance = 10f;

	[SerializeField]
	private float m_AlphaAdditionalRuntime = 1f;

	private ParticleSystem m_Particles;

	private ParticleSystemRenderer m_Renderer;

	private Material m_Material;

	private Gradient m_GradientCached = new Gradient();

	private bool m_RuntimePropertiesDirty = true;

	private VolumetricLightBeamAbstractBase m_Master;

	public bool isCulled { get; private set; }

	public float alphaAdditionalRuntime
	{
		get
		{
			return m_AlphaAdditionalRuntime;
		}
		set
		{
			if (m_AlphaAdditionalRuntime != value)
			{
				m_AlphaAdditionalRuntime = value;
				m_RuntimePropertiesDirty = true;
			}
		}
	}

	public bool particlesAreInstantiated => m_Particles;

	public int particlesCurrentCount
	{
		get
		{
			if (!m_Particles)
			{
				return 0;
			}
			return m_Particles.particleCount;
		}
	}

	public int particlesMaxCount
	{
		get
		{
			if (!m_Particles)
			{
				return 0;
			}
			return m_Particles.main.maxParticles;
		}
	}

	public ParticleSystemRenderer FindRenderer()
	{
		if ((bool)m_Renderer)
		{
			return m_Renderer;
		}
		return m_Particles.GetComponent<ParticleSystemRenderer>();
	}

	private void Start()
	{
		isCulled = false;
		m_Master = GetComponent<VolumetricLightBeamAbstractBase>();
		HandleBackwardCompatibility(m_Master._INTERNAL_pluginVersion, 20200);
		InstantiateParticleSystem();
		SetActiveAndPlay();
	}

	private void InstantiateParticleSystem()
	{
		base.gameObject.ForeachComponentsInDirectChildrenOnly(delegate(ParticleSystem ps)
		{
			UnityEngine.Object.DestroyImmediate(ps.gameObject);
		}, includeInactive: true);
		m_Particles = Config.Instance.NewVolumetricDustParticles();
		if ((bool)m_Particles)
		{
			m_Particles.transform.SetParent(base.transform, worldPositionStays: false);
			m_Renderer = m_Particles.GetComponent<ParticleSystemRenderer>();
			m_Material = new Material(m_Renderer.sharedMaterial);
			m_Renderer.material = m_Material;
		}
	}

	private void OnEnable()
	{
		SetActiveAndPlay();
	}

	private void SetActive(bool active)
	{
		if ((bool)m_Particles)
		{
			m_Particles.gameObject.SetActive(active);
		}
	}

	private void SetActiveAndPlay()
	{
		SetActive(active: true);
		Play();
	}

	private void Play()
	{
		if ((bool)m_Particles)
		{
			SetParticleProperties();
			m_Particles.Simulate(0f);
			m_Particles.Play(withChildren: true);
		}
	}

	private void OnDisable()
	{
		SetActive(active: false);
	}

	private void OnDestroy()
	{
		if ((bool)m_Particles)
		{
			UnityEngine.Object.DestroyImmediate(m_Particles.gameObject);
			m_Particles = null;
		}
		if ((bool)m_Material)
		{
			UnityEngine.Object.DestroyImmediate(m_Material);
			m_Material = null;
		}
	}

	private void Update()
	{
		UpdateCulling();
		if (UtilsBeamProps.CanChangeDuringPlaytime(m_Master))
		{
			SetParticleProperties();
		}
		if (m_RuntimePropertiesDirty && m_Material != null)
		{
			m_Material.SetColor(ShaderProperties.ParticlesTintColor, new Color(1f, 1f, 1f, alphaAdditionalRuntime));
			m_RuntimePropertiesDirty = false;
		}
	}

	private void SetParticleProperties()
	{
		if (!m_Particles || !m_Particles.gameObject.activeSelf)
		{
			return;
		}
		m_Particles.transform.localRotation = UtilsBeamProps.GetInternalLocalRotation(m_Master);
		m_Particles.transform.localScale = (m_Master.IsScalable() ? Vector3.one : Vector3.one.Divide(m_Master.GetLossyScale()));
		float num = UtilsBeamProps.GetFallOffEnd(m_Master) * (spawnDistanceRange.maxValue - spawnDistanceRange.minValue);
		float num2 = num * density;
		int maxParticles = (int)(num2 * 4f);
		ParticleSystem.MainModule main = m_Particles.main;
		ParticleSystem.MinMaxCurve startLifetime = main.startLifetime;
		startLifetime.mode = ParticleSystemCurveMode.TwoConstants;
		startLifetime.constantMin = 4f;
		startLifetime.constantMax = 6f;
		main.startLifetime = startLifetime;
		ParticleSystem.MinMaxCurve startSize = main.startSize;
		startSize.mode = ParticleSystemCurveMode.TwoConstants;
		startSize.constantMin = size * 0.9f;
		startSize.constantMax = size * 1.1f;
		main.startSize = startSize;
		ParticleSystem.MinMaxGradient startColor = main.startColor;
		if (UtilsBeamProps.GetColorMode(m_Master) == ColorMode.Flat)
		{
			startColor.mode = ParticleSystemGradientMode.Color;
			Color colorFlat = UtilsBeamProps.GetColorFlat(m_Master);
			colorFlat.a *= alpha;
			startColor.color = colorFlat;
		}
		else
		{
			startColor.mode = ParticleSystemGradientMode.Gradient;
			Gradient colorGradient = UtilsBeamProps.GetColorGradient(m_Master);
			GradientColorKey[] colorKeys = colorGradient.colorKeys;
			GradientAlphaKey[] alphaKeys = colorGradient.alphaKeys;
			for (int i = 0; i < alphaKeys.Length; i++)
			{
				alphaKeys[i].alpha *= alpha;
			}
			m_GradientCached.SetKeys(colorKeys, alphaKeys);
			startColor.gradient = m_GradientCached;
		}
		main.startColor = startColor;
		ParticleSystem.MinMaxCurve startSpeed = main.startSpeed;
		startSpeed.constant = ((direction == ParticlesDirection.Random) ? Mathf.Abs(velocity.z) : 0f);
		main.startSpeed = startSpeed;
		ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = m_Particles.velocityOverLifetime;
		velocityOverLifetime.enabled = direction != ParticlesDirection.Random;
		velocityOverLifetime.space = ((direction != ParticlesDirection.LocalSpace) ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local);
		velocityOverLifetime.xMultiplier = velocity.x;
		velocityOverLifetime.yMultiplier = velocity.y;
		velocityOverLifetime.zMultiplier = velocity.z;
		main.maxParticles = maxParticles;
		float thickness = UtilsBeamProps.GetThickness(m_Master);
		float fallOffEnd = UtilsBeamProps.GetFallOffEnd(m_Master);
		ParticleSystem.ShapeModule shape = m_Particles.shape;
		shape.shapeType = ParticleSystemShapeType.ConeVolume;
		float num3 = UtilsBeamProps.GetConeAngle(m_Master) * Mathf.Lerp(0.7f, 1f, thickness);
		shape.angle = num3 * 0.5f;
		float a = UtilsBeamProps.GetConeRadiusStart(m_Master) * Mathf.Lerp(0.3f, 1f, thickness);
		float b = Utils.ComputeConeRadiusEnd(fallOffEnd, num3);
		shape.radius = Mathf.Lerp(a, b, spawnDistanceRange.minValue);
		shape.length = num;
		float z = fallOffEnd * spawnDistanceRange.minValue;
		shape.position = new Vector3(0f, 0f, z);
		shape.arc = 360f;
		shape.randomDirectionAmount = ((direction == ParticlesDirection.Random) ? 1f : 0f);
		ParticleSystem.EmissionModule emission = m_Particles.emission;
		ParticleSystem.MinMaxCurve rateOverTime = emission.rateOverTime;
		rateOverTime.constant = num2;
		emission.rateOverTime = rateOverTime;
		if ((bool)m_Renderer)
		{
			m_Renderer.sortingLayerID = UtilsBeamProps.GetSortingLayerID(m_Master);
			m_Renderer.sortingOrder = UtilsBeamProps.GetSortingOrder(m_Master);
		}
	}

	private void HandleBackwardCompatibility(int serializedVersion, int newVersion)
	{
		if (serializedVersion == -1 || serializedVersion == newVersion)
		{
			return;
		}
		if (serializedVersion < 1880)
		{
			if (direction == ParticlesDirection.Random)
			{
				direction = ParticlesDirection.LocalSpace;
			}
			else
			{
				direction = ParticlesDirection.Random;
			}
			velocity = new Vector3(0f, 0f, speed);
		}
		if (serializedVersion < 1940)
		{
			spawnDistanceRange = new MinMaxRangeFloat(spawnMinDistance, spawnMaxDistance);
		}
		Utils.MarkCurrentSceneDirty();
	}

	private void UpdateCulling()
	{
		if (!m_Particles)
		{
			return;
		}
		bool flag = true;
		bool fadeOutEnabled = UtilsBeamProps.GetFadeOutEnabled(m_Master);
		if ((cullingEnabled || fadeOutEnabled) && m_Master.hasGeometry)
		{
			if ((bool)Config.Instance.fadeOutCameraTransform)
			{
				float num = cullingMaxDistance;
				if (fadeOutEnabled)
				{
					num = Mathf.Min(num, UtilsBeamProps.GetFadeOutEnd(m_Master));
				}
				float num2 = num * num;
				flag = m_Master.bounds.SqrDistance(Config.Instance.fadeOutCameraTransform.position) <= num2;
			}
			else
			{
				Debug.LogErrorFormat(base.gameObject, "Fail to retrieve the camera with tag '{0}' (specified in VLB Config's 'fadeOutCameraTag') for the {1} Culling feature.", Config.Instance.fadeOutCameraTag, "VolumetricDustParticles");
			}
		}
		if (m_Particles.gameObject.activeSelf != flag)
		{
			SetActive(flag);
			isCulled = !flag;
		}
		if (flag && !m_Particles.isPlaying)
		{
			m_Particles.Play();
		}
	}
}
