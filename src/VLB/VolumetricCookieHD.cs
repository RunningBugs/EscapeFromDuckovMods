using System;
using UnityEngine;

namespace VLB;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(VolumetricLightBeamHD))]
[HelpURL("http://saladgamer.com/vlb-doc/comp-cookie-hd/")]
[AddComponentMenu("VLB/HD/Volumetric Cookie HD")]
public class VolumetricCookieHD : MonoBehaviour
{
	public const string ClassName = "VolumetricCookieHD";

	[SerializeField]
	private float m_Contribution = 1f;

	[SerializeField]
	private Texture m_CookieTexture;

	[SerializeField]
	private CookieChannel m_Channel = CookieChannel.Alpha;

	[SerializeField]
	private bool m_Negative;

	[SerializeField]
	private Vector2 m_Translation = Consts.Cookie.TranslationDefault;

	[SerializeField]
	private float m_Rotation;

	[SerializeField]
	private Vector2 m_Scale = Consts.Cookie.ScaleDefault;

	private VolumetricLightBeamHD m_Master;

	public float contribution
	{
		get
		{
			return m_Contribution;
		}
		set
		{
			if (m_Contribution != value)
			{
				m_Contribution = value;
				SetDirty();
			}
		}
	}

	public Texture cookieTexture
	{
		get
		{
			return m_CookieTexture;
		}
		set
		{
			if (m_CookieTexture != value)
			{
				m_CookieTexture = value;
				SetDirty();
			}
		}
	}

	public CookieChannel channel
	{
		get
		{
			return m_Channel;
		}
		set
		{
			if (m_Channel != value)
			{
				m_Channel = value;
				SetDirty();
			}
		}
	}

	public bool negative
	{
		get
		{
			return m_Negative;
		}
		set
		{
			if (m_Negative != value)
			{
				m_Negative = value;
				SetDirty();
			}
		}
	}

	public Vector2 translation
	{
		get
		{
			return m_Translation;
		}
		set
		{
			if (m_Translation != value)
			{
				m_Translation = value;
				SetDirty();
			}
		}
	}

	public float rotation
	{
		get
		{
			return m_Rotation;
		}
		set
		{
			if (m_Rotation != value)
			{
				m_Rotation = value;
				SetDirty();
			}
		}
	}

	public Vector2 scale
	{
		get
		{
			return m_Scale;
		}
		set
		{
			if (m_Scale != value)
			{
				m_Scale = value;
				SetDirty();
			}
		}
	}

	private void SetDirty()
	{
		if ((bool)m_Master)
		{
			m_Master.SetPropertyDirty(DirtyProps.CookieProps);
		}
	}

	public static void ApplyMaterialProperties(VolumetricCookieHD instance, BeamGeometryHD geom)
	{
		if ((bool)instance && instance.enabled && instance.cookieTexture != null)
		{
			geom.SetMaterialProp(ShaderProperties.HD.CookieTexture, instance.cookieTexture);
			geom.SetMaterialProp(ShaderProperties.HD.CookieProperties, new Vector4(instance.negative ? instance.contribution : (0f - instance.contribution), (float)instance.channel, Mathf.Cos(instance.rotation * (MathF.PI / 180f)), Mathf.Sin(instance.rotation * (MathF.PI / 180f))));
			geom.SetMaterialProp(ShaderProperties.HD.CookiePosAndScale, new Vector4(instance.translation.x, instance.translation.y, instance.scale.x, instance.scale.y));
		}
		else
		{
			geom.SetMaterialProp(ShaderProperties.HD.CookieTexture, BeamGeometryHD.InvalidTexture.Null);
			geom.SetMaterialProp(ShaderProperties.HD.CookieProperties, Vector4.zero);
		}
	}

	private void Awake()
	{
		m_Master = GetComponent<VolumetricLightBeamHD>();
	}

	private void OnEnable()
	{
		SetDirty();
	}

	private void OnDisable()
	{
		SetDirty();
	}

	private void OnDidApplyAnimationProperties()
	{
		SetDirty();
	}

	private void Start()
	{
		if (Application.isPlaying)
		{
			SetDirty();
		}
	}

	private void OnDestroy()
	{
		if (Application.isPlaying)
		{
			SetDirty();
		}
	}
}
