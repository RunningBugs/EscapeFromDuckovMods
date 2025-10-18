using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerController : MonoBehaviour
{
	[SerializeField]
	private CanvasScaler canvasScaler;

	private Vector2Int cachedResolution;

	private void OnValidate()
	{
		if (canvasScaler == null)
		{
			canvasScaler = GetComponent<CanvasScaler>();
		}
	}

	private void Awake()
	{
		OnValidate();
	}

	private void OnEnable()
	{
		Refresh();
	}

	private void Refresh()
	{
		if (!(canvasScaler == null))
		{
			Vector2Int currentResolution = GetCurrentResolution();
			float num = (float)currentResolution.x / (float)currentResolution.y;
			Vector2 referenceResolution = canvasScaler.referenceResolution;
			float num2 = referenceResolution.x / referenceResolution.y;
			if (num > num2)
			{
				canvasScaler.matchWidthOrHeight = 1f;
			}
			else
			{
				canvasScaler.matchWidthOrHeight = 0f;
			}
			cachedResolution = currentResolution;
		}
	}

	private void FixedUpdate()
	{
		if (!ResolutionMatch())
		{
			Refresh();
		}
	}

	private bool ResolutionMatch()
	{
		Vector2Int currentResolution = GetCurrentResolution();
		if (cachedResolution.x == currentResolution.x)
		{
			return cachedResolution.y == currentResolution.y;
		}
		return false;
	}

	private Vector2Int GetCurrentResolution()
	{
		return new Vector2Int(Display.main.renderingWidth, Display.main.renderingHeight);
	}
}
