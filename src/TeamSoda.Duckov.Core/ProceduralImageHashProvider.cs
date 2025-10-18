using LeTai.TrueShadow;
using LeTai.TrueShadow.PluginInterfaces;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;

[ExecuteInEditMode]
public class ProceduralImageHashProvider : MonoBehaviour, ITrueShadowCustomHashProvider
{
	[SerializeField]
	private ProceduralImage proceduralImage;

	[SerializeField]
	private TrueShadow trueShadow;

	private void Awake()
	{
		if (trueShadow == null)
		{
			trueShadow = GetComponent<TrueShadow>();
		}
		if (proceduralImage == null)
		{
			proceduralImage = GetComponent<ProceduralImage>();
		}
	}

	private void Refresh()
	{
		trueShadow.CustomHash = Hash();
	}

	private void OnValidate()
	{
		if (trueShadow == null)
		{
			trueShadow = GetComponent<TrueShadow>();
		}
		if (proceduralImage == null)
		{
			proceduralImage = GetComponent<ProceduralImage>();
		}
		Refresh();
	}

	private void OnRectTransformDimensionsChange()
	{
		Refresh();
	}

	private int Hash()
	{
		return proceduralImage.InfoCache.GetHashCode() + proceduralImage.color.GetHashCode();
	}
}
