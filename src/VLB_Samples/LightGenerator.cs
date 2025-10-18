using UnityEngine;
using VLB;

namespace VLB_Samples;

public class LightGenerator : MonoBehaviour
{
	[Range(1f, 100f)]
	[SerializeField]
	private int CountX = 10;

	[Range(1f, 100f)]
	[SerializeField]
	private int CountY = 10;

	[SerializeField]
	private float OffsetUnits = 1f;

	[SerializeField]
	private float PositionY = 1f;

	[SerializeField]
	private bool NoiseEnabled;

	[SerializeField]
	private bool AddLight = true;

	public void Generate()
	{
		for (int i = 0; i < CountX; i++)
		{
			for (int j = 0; j < CountY; j++)
			{
				GameObject gameObject = null;
				gameObject = ((!AddLight) ? new GameObject("Light_" + i + "_" + j, typeof(VolumetricLightBeamSD), typeof(Rotater)) : new GameObject("Light_" + i + "_" + j, typeof(Light), typeof(VolumetricLightBeamSD), typeof(Rotater)));
				gameObject.transform.SetPositionAndRotation(new Vector3((float)i * OffsetUnits, PositionY, (float)j * OffsetUnits), Quaternion.Euler((float)Random.Range(-45, 45) + 90f, Random.Range(0, 360), 0f));
				VolumetricLightBeamSD component = gameObject.GetComponent<VolumetricLightBeamSD>();
				if (AddLight)
				{
					Light component2 = gameObject.GetComponent<Light>();
					component2.type = LightType.Spot;
					component2.color = new Color(Random.value, Random.value, Random.value, 1f);
					component2.range = Random.Range(3f, 8f);
					component2.intensity = Random.Range(0.2f, 5f);
					component2.spotAngle = Random.Range(10f, 90f);
					if (Config.Instance.geometryOverrideLayer)
					{
						component2.cullingMask = ~(1 << Config.Instance.geometryLayerID);
					}
				}
				else
				{
					component.color = new Color(Random.value, Random.value, Random.value, 1f);
					component.fallOffEnd = Random.Range(3f, 8f);
					component.spotAngle = Random.Range(10f, 90f);
				}
				component.coneRadiusStart = Random.Range(0f, 0.1f);
				component.geomCustomSides = Random.Range(12, 36);
				component.fresnelPow = Random.Range(1f, 7.5f);
				component.noiseMode = (NoiseEnabled ? NoiseMode.WorldSpace : NoiseMode.Disabled);
				gameObject.GetComponent<Rotater>().EulerSpeed = new Vector3(0f, Random.Range(-500, 500), 0f);
			}
		}
	}
}
