using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Splines;
using UnityEngine.UI;

namespace UI_Spline_Renderer.Example;

public class DraggableSplinePointExample : MonoBehaviour, IDragHandler, IEventSystemHandler, IBeginDragHandler, IEndDragHandler
{
	public UISplineRenderer uiSplineRenderer;

	public Image myImage;

	public int splineIndex;

	public int knotIndex;

	public Color connectedColor;

	public bool isConnected;

	private BezierKnot _originalKnot;

	public void OnDrag(PointerEventData eventData)
	{
		Vector3 vector = base.transform.parent.InverseTransformPoint(eventData.position);
		BezierKnot value = new BezierKnot(vector);
		uiSplineRenderer.splineContainer[splineIndex].SetKnot(knotIndex, value);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		uiSplineRenderer.raycastTarget = false;
		myImage.raycastTarget = false;
		uiSplineRenderer.color = Color.white;
		if (!isConnected)
		{
			_originalKnot = uiSplineRenderer.splineContainer[splineIndex][knotIndex];
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		foreach (GameObject item in eventData.hovered)
		{
			if ((bool)item.GetComponent<DragPortExample>())
			{
				Connect(item.transform);
				uiSplineRenderer.raycastTarget = true;
				myImage.raycastTarget = true;
				return;
			}
		}
		Disconnect();
	}

	private void Connect(Transform t)
	{
		Vector3 vector = base.transform.parent.InverseTransformPoint(t.position);
		BezierKnot value = new BezierKnot(vector);
		uiSplineRenderer.splineContainer[splineIndex].SetKnot(knotIndex, value);
		uiSplineRenderer.color = connectedColor;
		isConnected = true;
	}

	private void Disconnect()
	{
		uiSplineRenderer.color = Color.white;
		uiSplineRenderer.splineContainer[splineIndex].SetKnot(knotIndex, _originalKnot);
		isConnected = false;
		uiSplineRenderer.raycastTarget = true;
		myImage.raycastTarget = true;
	}
}
