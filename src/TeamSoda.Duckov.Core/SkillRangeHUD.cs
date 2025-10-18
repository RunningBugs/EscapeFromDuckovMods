using UnityEngine;

public class SkillRangeHUD : MonoBehaviour
{
	public Transform rangeTarget;

	public Renderer rangeRenderer;

	private Material rangeMat;

	public void SetRange(float range)
	{
		rangeTarget.localScale = Vector3.one * range;
	}

	public void SetProgress(float progress)
	{
		if (rangeMat == null)
		{
			rangeMat = rangeRenderer.material;
		}
		if (!(rangeMat == null))
		{
			rangeMat.SetFloat("_Progress", progress);
		}
	}
}
