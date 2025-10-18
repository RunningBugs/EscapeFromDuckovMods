using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class SodaPointLight : MonoBehaviour
{
	[SerializeField]
	private Renderer lightRenderer;

	[SerializeField]
	[Range(0f, 1f)]
	private float hardness = 0.5f;

	[SerializeField]
	[Range(1f, 5f)]
	private float fallOff = 1f;

	[FormerlySerializedAs("dayColor")]
	[SerializeField]
	[ColorUsage(false, true)]
	private Color lightColor = Color.white;

	public bool enviromentTint;

	private int lightColorID = Shader.PropertyToID("_LightColor");

	private int hardnessID = Shader.PropertyToID("_Hardness");

	private int fallOffID = Shader.PropertyToID("_FallOff");

	private int enviromentTintID = Shader.PropertyToID("_EnviromentTintOn");

	private MaterialPropertyBlock block;

	public float FallOff
	{
		get
		{
			return fallOff;
		}
		set
		{
			fallOff = value;
			SyncToLight();
		}
	}

	public float Hardness
	{
		get
		{
			return hardness;
		}
		set
		{
			hardness = value;
			SyncToLight();
		}
	}

	public Color LightColor
	{
		get
		{
			return lightColor;
		}
		set
		{
			lightColor = value;
			SyncToLight();
		}
	}

	private void Awake()
	{
		SyncToLight();
	}

	private void SyncToLight()
	{
		if (block == null)
		{
			block = new MaterialPropertyBlock();
		}
		block.SetFloat(hardnessID, hardness);
		block.SetFloat(fallOffID, fallOff);
		block.SetColor(lightColorID, LightColor);
		block.SetFloat(enviromentTintID, enviromentTint ? 1f : 0f);
		if ((bool)lightRenderer)
		{
			lightRenderer.SetPropertyBlock(block);
		}
	}

	private void OnValidate()
	{
		SyncToLight();
	}

	private void OnDrawGizmosSelected()
	{
		_ = base.transform.lossyScale;
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Color color = lightColor;
		color.a = 1f;
		Gizmos.color = color;
		Gizmos.DrawWireSphere(Vector3.zero, 1f);
	}
}
