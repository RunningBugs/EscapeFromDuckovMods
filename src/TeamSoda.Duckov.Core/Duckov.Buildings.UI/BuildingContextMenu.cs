using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Duckov.Buildings.UI;

public class BuildingContextMenu : MonoBehaviour
{
	private RectTransform rectTransform;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private BuildingContextMenuEntry recycleButton;

	public Building Target { get; private set; }

	private void Awake()
	{
		rectTransform = base.transform as RectTransform;
		recycleButton.onPointerClick += OnRecycleButtonClicked;
	}

	private void OnRecycleButtonClicked(BuildingContextMenuEntry entry)
	{
		if (!(Target == null))
		{
			BuildingManager.ReturnBuilding(Target.GUID).Forget();
		}
	}

	public void Setup(Building target)
	{
		Target = target;
		if (target == null)
		{
			Hide();
			return;
		}
		nameText.text = target.DisplayName;
		Show();
	}

	private void LateUpdate()
	{
		if (Target == null)
		{
			Hide();
			return;
		}
		Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(GameCamera.Instance.renderCamera, Target.transform.position);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(base.transform.parent as RectTransform, screenPoint, null, out var localPoint);
		rectTransform.localPosition = localPoint;
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
