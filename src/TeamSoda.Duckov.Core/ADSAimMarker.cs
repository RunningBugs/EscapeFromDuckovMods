using System.Collections.Generic;
using Duckov.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

public class ADSAimMarker : MonoBehaviour
{
	[HideInInspector]
	public ADSAimMarker selfPrefab;

	public bool hideNormalCrosshair = true;

	public AimMarker parentAimMarker;

	public RectTransform aimMarkerUI;

	public RectTransform followUI;

	public CanvasGroup canvasGroup;

	public float followSpeed;

	public float followMaxDistance = 30f;

	private float adsValue = -1f;

	private float canvasAlpha;

	public Vector2 adsAlphaRemap = new Vector2(0f, 1f);

	public List<ProceduralImage> proceduralImages;

	private List<CanvasGroup> proceduralImageCanvasGroups;

	public List<PunchReceiver> shootPunchReceivers;

	public List<SingleCrosshair> crosshairs;

	public Graphic sniperRoundRenderer;

	public Graphic followSniperRoundRenderer;

	private float scatter;

	private float minScatter;

	private RectTransform selfRect;

	private int sniperCenterShaderHash = Shader.PropertyToID("_RoundCenter");

	public float CanvasAlpha => canvasAlpha;

	public void CollectCrosshairs()
	{
		crosshairs.Clear();
		SingleCrosshair[] componentsInChildren = GetComponentsInChildren<SingleCrosshair>();
		foreach (SingleCrosshair item in componentsInChildren)
		{
			crosshairs.Add(item);
		}
	}

	private void Awake()
	{
		proceduralImageCanvasGroups = new List<CanvasGroup>();
		for (int i = 0; i < proceduralImages.Count; i++)
		{
			proceduralImageCanvasGroups.Add(proceduralImages[i].GetComponent<CanvasGroup>());
		}
		selfRect = GetComponent<RectTransform>();
	}

	private void LateUpdate()
	{
		if ((bool)selfRect)
		{
			selfRect.localScale = Vector3.one;
		}
		followUI.position = Vector3.Lerp(followUI.position, aimMarkerUI.position, Time.deltaTime * followSpeed);
		if (Vector3.Distance(followUI.position, aimMarkerUI.position) > followMaxDistance)
		{
			followUI.position = Vector3.MoveTowards(aimMarkerUI.position, followUI.position, followMaxDistance);
		}
		foreach (SingleCrosshair crosshair in crosshairs)
		{
			if ((bool)crosshair)
			{
				crosshair.UpdateScatter(scatter);
			}
		}
		SetSniperRenderer();
	}

	public void SetAimMarkerPos(Vector3 pos)
	{
		aimMarkerUI.position = pos;
	}

	public void OnShoot()
	{
		foreach (PunchReceiver shootPunchReceiver in shootPunchReceivers)
		{
			if ((bool)shootPunchReceiver)
			{
				shootPunchReceiver.Punch();
			}
		}
	}

	public void SetScatter(float _currentScatter, float _minScatter)
	{
		scatter = _currentScatter;
		minScatter = _minScatter;
	}

	public void SetAdsValue(float _adsValue)
	{
		adsValue = _adsValue;
		canvasAlpha = _adsValue;
		if (adsAlphaRemap.y > adsAlphaRemap.x)
		{
			canvasAlpha = Mathf.Clamp01((_adsValue - adsAlphaRemap.x) / (adsAlphaRemap.y - adsAlphaRemap.x));
		}
		this.canvasGroup.alpha = canvasAlpha;
		for (int i = 0; i < proceduralImages.Count; i++)
		{
			ProceduralImage proceduralImage = proceduralImages[i];
			if ((bool)proceduralImage)
			{
				float num = Mathf.Clamp(scatter - minScatter, 0f, 10f) * 2f;
				proceduralImage.FalloffDistance = Mathf.Lerp(25f, 1f, canvasAlpha) + num;
				CanvasGroup canvasGroup = proceduralImageCanvasGroups[i];
				if ((bool)canvasGroup)
				{
					canvasGroup.alpha = Mathf.Clamp(1f - (num - 2f) / 15f, 0.3f, 1f);
				}
			}
		}
	}

	private void SetSniperRenderer()
	{
		if ((bool)sniperRoundRenderer)
		{
			Vector2 vector = RectTransformUtility.WorldToScreenPoint(null, aimMarkerUI.position) / new Vector2(Screen.width, Screen.height);
			sniperRoundRenderer.material.SetVector(sniperCenterShaderHash, vector);
		}
		if ((bool)followSniperRoundRenderer)
		{
			Vector2 vector2 = RectTransformUtility.WorldToScreenPoint(null, followUI.position) / new Vector2(Screen.width, Screen.height);
			followSniperRoundRenderer.material.SetVector(sniperCenterShaderHash, vector2);
		}
	}
}
