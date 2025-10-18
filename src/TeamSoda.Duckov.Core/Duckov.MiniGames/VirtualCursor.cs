using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.MiniGames;

public class VirtualCursor : MiniGameBehaviour
{
	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private RectTransform moveArea;

	[SerializeField]
	private Canvas canvas;

	private RectTransform canvasRectTransform;

	[SerializeField]
	private GraphicRaycaster raycaster;

	[SerializeField]
	private float sensitivity = 0.5f;

	private static GameObject raycastGO;

	private static VirtualCursorTarget target;

	[NonSerialized]
	private List<Graphic> m_RaycastResults = new List<Graphic>();

	private Vector3 eventPositionWatch;

	[NonSerialized]
	private static List<Graphic> s_canvasGraphics = new List<Graphic>();

	[NonSerialized]
	private static readonly List<Graphic> s_SortedGraphics = new List<Graphic>();

	private void Awake()
	{
		if (rectTransform == null)
		{
			rectTransform = base.transform as RectTransform;
		}
		if (moveArea == null)
		{
			moveArea = rectTransform.parent as RectTransform;
		}
		if (canvas == null)
		{
			canvas = GetComponentInParent<Canvas>();
		}
		canvasRectTransform = canvas.transform as RectTransform;
		if (raycaster == null)
		{
			raycaster = GetComponentInParent<GraphicRaycaster>();
		}
	}

	private void Update()
	{
		if (base.Game == null || ((bool)base.Game.Console && !base.Game.Console.Interacting))
		{
			return;
		}
		Vector2 mouseDelta = UIInputManager.MouseDelta;
		Vector3 localPosition = rectTransform.localPosition + (Vector3)mouseDelta * sensitivity;
		Rect rect = moveArea.rect;
		localPosition.x = Mathf.Clamp(localPosition.x, rect.min.x, rect.max.x);
		localPosition.y = Mathf.Clamp(localPosition.y, rect.min.y, rect.max.y);
		rectTransform.localPosition = localPosition;
		List<RaycastResult> list = new List<RaycastResult>();
		Raycast(list);
		RaycastResult raycastResult = FindFirstRaycast(list);
		if (raycastResult.gameObject != raycastGO)
		{
			VirtualCursorTarget virtualCursorTarget = target;
			VirtualCursorTarget virtualCursorTarget2 = ((!(raycastResult.gameObject != null)) ? null : raycastResult.gameObject.GetComponent<VirtualCursorTarget>());
			if (virtualCursorTarget2 != virtualCursorTarget)
			{
				target = virtualCursorTarget2;
				OnChange(virtualCursorTarget2, virtualCursorTarget);
			}
		}
		if (UIInputManager.WasClickedThisFrame && target != null)
		{
			target.OnClick();
		}
	}

	private void OnChange(VirtualCursorTarget newTarget, VirtualCursorTarget oldTarget)
	{
		if (newTarget != null)
		{
			newTarget.OnCursorEnter();
		}
		if (oldTarget != null)
		{
			oldTarget.OnCursorExit();
		}
	}

	private void Raycast(List<RaycastResult> resultAppendList)
	{
		if (canvas == null)
		{
			return;
		}
		IList<Graphic> raycastableGraphicsForCanvas = GraphicRegistry.GetRaycastableGraphicsForCanvas(canvas);
		s_canvasGraphics.Clear();
		if (raycastableGraphicsForCanvas != null && raycastableGraphicsForCanvas.Count > 0)
		{
			for (int i = 0; i < raycastableGraphicsForCanvas.Count; i++)
			{
				s_canvasGraphics.Add(raycastableGraphicsForCanvas[i]);
			}
			Camera eventCamera = raycaster.eventCamera;
			Vector3 vector = eventCamera.WorldToScreenPoint(base.transform.position);
			vector.z = 0f;
			eventPositionWatch = vector;
			m_RaycastResults.Clear();
			Raycast(canvas, eventCamera, vector, raycastableGraphicsForCanvas, m_RaycastResults);
			int count = m_RaycastResults.Count;
			for (int j = 0; j < count; j++)
			{
				GameObject gameObject = m_RaycastResults[j].gameObject;
				float distance = 0f;
				Vector3 forward = gameObject.transform.forward;
				RaycastResult item = new RaycastResult
				{
					gameObject = gameObject,
					module = raycaster,
					distance = distance,
					screenPosition = vector,
					displayIndex = 0,
					index = resultAppendList.Count,
					depth = m_RaycastResults[j].depth,
					sortingLayer = canvas.sortingLayerID,
					sortingOrder = canvas.sortingOrder,
					worldPosition = vector,
					worldNormal = -forward
				};
				resultAppendList.Add(item);
			}
		}
	}

	private static void Raycast(Canvas canvas, Camera eventCamera, Vector2 pointerPosition, IList<Graphic> foundGraphics, List<Graphic> results)
	{
		int count = foundGraphics.Count;
		for (int i = 0; i < count; i++)
		{
			Graphic graphic = foundGraphics[i];
			if (graphic.raycastTarget && !graphic.canvasRenderer.cull && graphic.depth != -1 && RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, eventCamera, graphic.raycastPadding) && (!(eventCamera != null) || !(eventCamera.WorldToScreenPoint(graphic.rectTransform.position).z > eventCamera.farClipPlane)) && graphic.Raycast(pointerPosition, eventCamera))
			{
				s_SortedGraphics.Add(graphic);
			}
		}
		s_SortedGraphics.Sort((Graphic g1, Graphic g2) => g2.depth.CompareTo(g1.depth));
		count = s_SortedGraphics.Count;
		for (int num = 0; num < count; num++)
		{
			results.Add(s_SortedGraphics[num]);
		}
		s_SortedGraphics.Clear();
	}

	private static RaycastResult FindFirstRaycast(List<RaycastResult> candidates)
	{
		int count = candidates.Count;
		for (int i = 0; i < count; i++)
		{
			if (!(candidates[i].gameObject == null))
			{
				return candidates[i];
			}
		}
		return default(RaycastResult);
	}

	internal static bool IsHovering(VirtualCursorTarget virtualCursorTarget)
	{
		return virtualCursorTarget == target;
	}
}
