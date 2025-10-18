using UnityEngine;

namespace VLB;

[ExecuteInEditMode]
[RequireComponent(typeof(LODGroup))]
[HelpURL("http://saladgamer.com/vlb-doc/comp-lodbeamgroup/")]
public class LODBeamGroup : MonoBehaviour
{
	[SerializeField]
	private VolumetricLightBeamAbstractBase[] m_LODBeams;

	[SerializeField]
	private bool m_ResetAllLODsLocalTransform;

	[SerializeField]
	private BeamProps m_LOD0PropsToCopy = (BeamProps)(-1);

	[SerializeField]
	private bool m_CopyLOD0PropsEachFrame;

	[SerializeField]
	private bool m_CullVolumetricDustParticles = true;

	private LODGroup m_LODGroup;

	private void Awake()
	{
		m_LODGroup = GetComponent<LODGroup>();
		SetupLodGroupData();
	}

	private void Start()
	{
		UnifyBeamsProperties();
	}

	public LOD[] GetLODsFromLODGroup()
	{
		return m_LODGroup.GetLODs();
	}

	private void SetLODRenderer(int lodIdx, Renderer renderer)
	{
		SetLODRenderers(lodIdx, (!renderer) ? null : new Renderer[1] { renderer });
	}

	private void SetLODRenderers(int lodIdx, Renderer[] renderers)
	{
		LOD[] lODs = m_LODGroup.GetLODs();
		lODs[lodIdx].renderers = renderers;
		m_LODGroup.SetLODs(lODs);
	}

	private void SetLOD(int lodIdx)
	{
		LOD[] lODs = m_LODGroup.GetLODs();
		if (!lODs.IsValidIndex(lodIdx))
		{
			return;
		}
		BeamGeometryAbstractBase beamGeometry = m_LODBeams[lodIdx].GetBeamGeometry();
		if (!beamGeometry)
		{
			return;
		}
		MeshRenderer meshRenderer = beamGeometry.meshRenderer;
		if (!meshRenderer)
		{
			return;
		}
		if (m_CullVolumetricDustParticles)
		{
			VolumetricDustParticles component = m_LODBeams[lodIdx].GetComponent<VolumetricDustParticles>();
			if ((bool)component)
			{
				ParticleSystemRenderer particleSystemRenderer = component.FindRenderer();
				if ((bool)particleSystemRenderer)
				{
					SetLODRenderers(lodIdx, new Renderer[2] { meshRenderer, particleSystemRenderer });
					return;
				}
			}
		}
		if (lODs[lodIdx].renderers == null || lODs[lodIdx].renderers.Length != 1 || lODs[lodIdx].renderers[0] != meshRenderer)
		{
			SetLODRenderer(lodIdx, meshRenderer);
		}
	}

	private void OnBeamGeometryGenerated(VolumetricLightBeamAbstractBase beam)
	{
		if (GetLODsFromLODGroup() == null || m_LODBeams == null)
		{
			return;
		}
		for (int i = 0; i < m_LODBeams.Length; i++)
		{
			if (m_LODBeams[i] == beam)
			{
				SetLOD(i);
				break;
			}
		}
	}

	private void SetupLodGroupData()
	{
		if (m_LODGroup == null)
		{
			return;
		}
		LOD[] lODsFromLODGroup = GetLODsFromLODGroup();
		if (lODsFromLODGroup == null)
		{
			return;
		}
		if (m_LODBeams == null || m_LODBeams.Length < lODsFromLODGroup.Length)
		{
			Utils.ResizeArray(ref m_LODBeams, lODsFromLODGroup.Length);
		}
		for (int i = 0; i < m_LODBeams.Length; i++)
		{
			if (m_LODBeams[i] == null)
			{
				if (i < lODsFromLODGroup.Length)
				{
					SetLODRenderer(i, null);
				}
			}
			else
			{
				m_LODBeams[i].RegisterBeamGeometryGeneratedCallback(OnBeamGeometryGenerated);
			}
		}
	}

	private void UnifyBeamsProperties()
	{
		if (m_LODBeams == null)
		{
			return;
		}
		if (m_ResetAllLODsLocalTransform)
		{
			VolumetricLightBeamAbstractBase[] lODBeams = m_LODBeams;
			foreach (VolumetricLightBeamAbstractBase volumetricLightBeamAbstractBase in lODBeams)
			{
				if ((bool)volumetricLightBeamAbstractBase)
				{
					volumetricLightBeamAbstractBase.transform.localPosition = Vector3.zero;
					volumetricLightBeamAbstractBase.transform.localRotation = Quaternion.identity;
					volumetricLightBeamAbstractBase.transform.localScale = Vector3.one;
				}
			}
		}
		if (m_LOD0PropsToCopy == (BeamProps)0 || m_LODBeams.Length <= 1)
		{
			return;
		}
		VolumetricLightBeamAbstractBase volumetricLightBeamAbstractBase2 = m_LODBeams[0];
		if (volumetricLightBeamAbstractBase2 == null)
		{
			return;
		}
		for (int j = 1; j < m_LODBeams.Length; j++)
		{
			VolumetricLightBeamAbstractBase volumetricLightBeamAbstractBase3 = m_LODBeams[j];
			if ((bool)volumetricLightBeamAbstractBase3)
			{
				volumetricLightBeamAbstractBase3.CopyPropsFrom(volumetricLightBeamAbstractBase2, m_LOD0PropsToCopy);
				UtilsBeamProps.SetColorFromLight(volumetricLightBeamAbstractBase3, fromLight: false);
				UtilsBeamProps.SetFallOffEndFromLight(volumetricLightBeamAbstractBase3, fromLight: false);
				UtilsBeamProps.SetIntensityFromLight(volumetricLightBeamAbstractBase3, fromLight: false);
				UtilsBeamProps.SetSpotAngleFromLight(volumetricLightBeamAbstractBase3, fromLight: false);
			}
		}
	}

	private void Update()
	{
		if (m_CopyLOD0PropsEachFrame)
		{
			UnifyBeamsProperties();
		}
	}
}
