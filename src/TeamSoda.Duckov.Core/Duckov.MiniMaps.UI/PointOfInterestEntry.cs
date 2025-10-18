using LeTai.TrueShadow;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace Duckov.MiniMaps.UI;

public class PointOfInterestEntry : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private RectTransform rectTransform;

	private MiniMapDisplay master;

	private MonoBehaviour target;

	private IPointOfInterest pointOfInterest;

	private MiniMapDisplayEntry minimapEntry;

	[SerializeField]
	private Transform iconContainer;

	[SerializeField]
	private Sprite defaultIcon;

	[SerializeField]
	private Color defaultColor = Color.white;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private TrueShadow shadow;

	[SerializeField]
	private TextMeshProUGUI displayName;

	[SerializeField]
	private ProceduralImage areaDisplay;

	[SerializeField]
	private Image areaFill;

	[SerializeField]
	private float areaLineThickness = 1f;

	[SerializeField]
	private string caption;

	private Vector3 cachedWorldPosition = Vector3.zero;

	public MonoBehaviour Target => target;

	private float ParentLocalScale => base.transform.parent.localScale.x;

	internal void Setup(MiniMapDisplay master, MonoBehaviour target, MiniMapDisplayEntry minimapEntry)
	{
		rectTransform = base.transform as RectTransform;
		this.master = master;
		this.target = target;
		this.minimapEntry = minimapEntry;
		this.pointOfInterest = null;
		icon.sprite = defaultIcon;
		icon.color = defaultColor;
		areaDisplay.color = defaultColor;
		Color color = defaultColor;
		color.a *= 0.1f;
		areaFill.color = color;
		caption = target.name;
		icon.gameObject.SetActive(value: true);
		if (target is IPointOfInterest pointOfInterest)
		{
			this.pointOfInterest = pointOfInterest;
			icon.gameObject.SetActive(!this.pointOfInterest.HideIcon);
			icon.sprite = ((pointOfInterest.Icon != null) ? pointOfInterest.Icon : defaultIcon);
			icon.color = pointOfInterest.Color;
			if ((bool)shadow)
			{
				shadow.Color = pointOfInterest.ShadowColor;
				shadow.OffsetDistance = pointOfInterest.ShadowDistance;
			}
			string value = this.pointOfInterest.DisplayName;
			caption = pointOfInterest.DisplayName;
			if (string.IsNullOrEmpty(value))
			{
				displayName.gameObject.SetActive(value: false);
			}
			else
			{
				displayName.gameObject.SetActive(value: true);
				displayName.text = this.pointOfInterest.DisplayName;
			}
			if (pointOfInterest.IsArea)
			{
				areaDisplay.gameObject.SetActive(value: true);
				rectTransform.sizeDelta = this.pointOfInterest.AreaRadius * Vector2.one * 2f;
				areaDisplay.color = pointOfInterest.Color;
				color = pointOfInterest.Color;
				color.a *= 0.1f;
				areaFill.color = color;
				areaDisplay.BorderWidth = areaLineThickness / ParentLocalScale;
			}
			else
			{
				icon.enabled = true;
				areaDisplay.gameObject.SetActive(value: false);
			}
			RefreshPosition();
			base.gameObject.SetActive(value: true);
		}
	}

	private void RefreshPosition()
	{
		cachedWorldPosition = target.transform.position;
		Vector3 centerOfObjectScene = MiniMapCenter.GetCenterOfObjectScene(target);
		Vector3 vector = target.transform.position - centerOfObjectScene;
		Vector3 point = new Vector2(vector.x, vector.z);
		Vector3 position = minimapEntry.transform.localToWorldMatrix.MultiplyPoint(point);
		base.transform.position = position;
		UpdateScale();
		UpdateRotation();
	}

	private void Update()
	{
		UpdateScale();
		UpdatePosition();
		UpdateRotation();
	}

	private void UpdateScale()
	{
		float num = ((pointOfInterest != null) ? pointOfInterest.ScaleFactor : 1f);
		iconContainer.localScale = Vector3.one * num / ParentLocalScale;
		if (pointOfInterest != null && pointOfInterest.IsArea)
		{
			areaDisplay.BorderWidth = areaLineThickness / ParentLocalScale;
			areaDisplay.FalloffDistance = 1f / ParentLocalScale;
		}
	}

	private void UpdatePosition()
	{
		if (cachedWorldPosition != target.transform.position)
		{
			RefreshPosition();
		}
	}

	private void UpdateRotation()
	{
		base.transform.rotation = Quaternion.identity;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		pointOfInterest.NotifyClicked(eventData);
		if (CheatMode.Active && UIInputManager.Ctrl && UIInputManager.Alt && UIInputManager.Shift && MiniMapCenter.GetSceneID(target) != null)
		{
			CharacterMainControl.Main.SetPosition(target.transform.position);
		}
	}
}
