using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Duckov.Buildings.UI;

public class GridDisplay : MonoBehaviour
{
	[HideInInspector]
	[SerializeField]
	private BuildingArea targetArea;

	[SerializeField]
	private float animationDuration;

	[SerializeField]
	private AnimationCurve showCurve;

	[SerializeField]
	private AnimationCurve hideCurve;

	private static int gridShowHideTaskToken;

	public static GridDisplay Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		Close();
	}

	public void Setup(BuildingArea buildingArea)
	{
		Vector2Int lowerLeftCorner = buildingArea.LowerLeftCorner;
		Vector4 value = new Vector4(lowerLeftCorner.x, lowerLeftCorner.y, buildingArea.Size.x * 2 - 1, buildingArea.Size.y * 2 - 1);
		Shader.SetGlobalVector("BuildingGrid_AreaPosAndSize", value);
		ShowGrid();
		HidePreview();
		ShowGrid();
	}

	public static void Close()
	{
		HidePreview();
		HideGrid();
	}

	public static async UniTask SetGridShowHide(bool show, AnimationCurve curve, float duration)
	{
		int token;
		do
		{
			token = Random.Range(0, int.MaxValue);
		}
		while (token == gridShowHideTaskToken);
		gridShowHideTaskToken = token;
		float time = 0f;
		if (duration <= 0f)
		{
			Shader.SetGlobalFloat("BuildingGrid_Building", show ? 1 : 0);
			return;
		}
		while (time < duration)
		{
			time += Time.unscaledDeltaTime;
			float time2 = time / duration;
			float value = Mathf.Lerp((!show) ? 1 : 0, show ? 1 : 0, curve.Evaluate(time2));
			Shader.SetGlobalFloat("BuildingGrid_Building", value);
			await UniTask.Yield();
			if (token != gridShowHideTaskToken)
			{
				return;
			}
		}
		Shader.SetGlobalFloat("BuildingGrid_Building", show ? 1 : 0);
	}

	public static void HideGrid()
	{
		if ((bool)Instance)
		{
			SetGridShowHide(show: false, Instance.hideCurve, Instance.animationDuration).Forget();
		}
		else
		{
			Shader.SetGlobalFloat("BuildingGrid_Building", 0f);
		}
	}

	public static void ShowGrid()
	{
		if ((bool)Instance)
		{
			SetGridShowHide(show: true, Instance.showCurve, Instance.animationDuration).Forget();
		}
		else
		{
			Shader.SetGlobalFloat("BuildingGrid_Building", 1f);
		}
	}

	public static void HidePreview()
	{
		Shader.SetGlobalVector("BuildingGrid_BuildingPosAndSize", Vector4.zero);
	}

	internal void SetBuildingPreviewCoord(Vector2Int coord, Vector2Int dimensions, BuildingRotation rotation, bool validPlacement)
	{
		if ((int)rotation % 2 > 0)
		{
			dimensions = new Vector2Int(dimensions.y, dimensions.x);
		}
		Vector4 value = new Vector4(coord.x, coord.y, dimensions.x, dimensions.y);
		Shader.SetGlobalVector("BuildingGrid_BuildingPosAndSize", value);
		Shader.SetGlobalFloat("BuildingGrid_CanBuild", validPlacement ? 1 : 0);
	}
}
