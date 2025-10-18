using System.Collections;
using UnityEngine;

namespace VLB;

[ExecuteInEditMode]
[HelpURL("http://saladgamer.com/vlb-doc/comp-skewinghandle-sd/")]
public class SkewingHandleSD : MonoBehaviour
{
	public const string ClassName = "SkewingHandleSD";

	public VolumetricLightBeamSD volumetricLightBeam;

	public bool shouldUpdateEachFrame;

	public bool IsAttachedToSelf()
	{
		if (volumetricLightBeam != null)
		{
			return volumetricLightBeam.gameObject == base.gameObject;
		}
		return false;
	}

	public bool CanSetSkewingVector()
	{
		if (volumetricLightBeam != null)
		{
			return volumetricLightBeam.canHaveMeshSkewing;
		}
		return false;
	}

	public bool CanUpdateEachFrame()
	{
		if (CanSetSkewingVector())
		{
			return volumetricLightBeam.trackChangesDuringPlaytime;
		}
		return false;
	}

	private bool ShouldUpdateEachFrame()
	{
		if (shouldUpdateEachFrame)
		{
			return CanUpdateEachFrame();
		}
		return false;
	}

	private void OnEnable()
	{
		if (CanSetSkewingVector())
		{
			SetSkewingVector();
		}
	}

	private void Start()
	{
		if (Application.isPlaying && ShouldUpdateEachFrame())
		{
			StartCoroutine(CoUpdate());
		}
	}

	private IEnumerator CoUpdate()
	{
		while (ShouldUpdateEachFrame())
		{
			SetSkewingVector();
			yield return null;
		}
	}

	private void SetSkewingVector()
	{
		Vector3 skewingLocalForwardDirection = volumetricLightBeam.transform.InverseTransformPoint(base.transform.position);
		volumetricLightBeam.skewingLocalForwardDirection = skewingLocalForwardDirection;
	}
}
