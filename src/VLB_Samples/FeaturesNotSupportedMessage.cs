using UnityEngine;
using VLB;

namespace VLB_Samples;

public class FeaturesNotSupportedMessage : MonoBehaviour
{
	private void Start()
	{
		if (!Noise3D.isSupported)
		{
			Debug.LogWarning(Noise3D.isNotSupportedString);
		}
	}
}
