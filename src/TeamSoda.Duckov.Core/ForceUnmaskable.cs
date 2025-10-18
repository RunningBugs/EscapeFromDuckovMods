using UnityEngine;
using UnityEngine.UI;

public class ForceUnmaskable : MonoBehaviour
{
	private void OnEnable()
	{
		MaskableGraphic[] components = GetComponents<MaskableGraphic>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].maskable = false;
		}
	}
}
