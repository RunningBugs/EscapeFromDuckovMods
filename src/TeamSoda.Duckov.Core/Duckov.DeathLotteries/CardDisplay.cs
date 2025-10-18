using System;
using Duckov.UI.Animations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.DeathLotteries;

public class CardDisplay : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerMoveHandler
{
	private RectTransform rectTransform;

	[SerializeField]
	private RectTransform cardTransform;

	[SerializeField]
	private FadeGroup frontFadeGroup;

	[SerializeField]
	private FadeGroup backFadeGroup;

	[SerializeField]
	private float idleAmp = 10f;

	[SerializeField]
	private float idleFrequency = 0.5f;

	[SerializeField]
	private float rotateSpeed = 300f;

	[SerializeField]
	private float flipSpeed = 300f;

	private bool facingFront;

	private bool hovering;

	private Vector2 pointerPosition;

	private Rect cachedRect;

	private float cachedRadius;

	private void Awake()
	{
		rectTransform = base.transform as RectTransform;
		RefreshFadeGroups();
	}

	private void CacheRadius()
	{
		cachedRect = rectTransform.rect;
		Rect rect = cachedRect;
		cachedRadius = Mathf.Sqrt(rect.width * rect.width + rect.height * rect.height) / 2f;
	}

	private void Update()
	{
		if (rectTransform.rect != cachedRect)
		{
			CacheRadius();
		}
		HandleAnimation();
	}

	private void HandleAnimation()
	{
		Quaternion rotation = cardTransform.rotation;
		if ((facingFront && !frontFadeGroup.IsShown) || (!facingFront && !backFadeGroup.IsShown))
		{
			rotation = Quaternion.RotateTowards(rotation, Quaternion.Euler(0f, 90f, 0f), flipSpeed * Time.deltaTime);
			if (Mathf.Approximately(Quaternion.Angle(rotation, Quaternion.Euler(0f, 90f, 0f)), 0f))
			{
				rotation = Quaternion.Euler(0f, -90f, 0f);
				RefreshFadeGroups();
			}
		}
		else
		{
			rotation = Quaternion.RotateTowards(rotation, GetIdealRotation(), rotateSpeed * Time.deltaTime);
		}
		cardTransform.rotation = rotation;
	}

	private void OnEnable()
	{
		CacheRadius();
	}

	private Quaternion GetIdealRotation()
	{
		if (rectTransform.rect != cachedRect)
		{
			CacheRadius();
		}
		if (hovering && !Mathf.Approximately(cachedRadius, 0f))
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, pointerPosition, null, out var localPoint);
			Vector2 center = rectTransform.rect.center;
			Vector2 vector = localPoint - center;
			float num = Mathf.Max(10f, cachedRadius);
			Vector2 vector2 = Vector2.ClampMagnitude(vector / num, 1f);
			return Quaternion.Euler((0f - vector2.y) * idleAmp, (0f - vector2.x) * idleAmp, 0f);
		}
		return Quaternion.Euler(Mathf.Sin(Time.time * idleFrequency * MathF.PI * 2f) * idleAmp, Mathf.Cos(Time.time * idleFrequency * MathF.PI * 2f) * idleAmp, 0f);
	}

	private void SkipAnimation()
	{
		RefreshFadeGroups();
		cardTransform.rotation = GetIdealRotation();
	}

	public void SetFacing(bool facingFront, bool skipAnimation = false)
	{
		this.facingFront = facingFront;
		if (skipAnimation)
		{
			SkipAnimation();
		}
	}

	public void Flip()
	{
		SetFacing(!facingFront);
	}

	private void RefreshFadeGroups()
	{
		if (facingFront)
		{
			frontFadeGroup.Show();
			backFadeGroup.Hide();
		}
		else
		{
			frontFadeGroup.Hide();
			backFadeGroup.Show();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		hovering = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		hovering = false;
	}

	public void OnPointerMove(PointerEventData eventData)
	{
		pointerPosition = eventData.position;
	}
}
