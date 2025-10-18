using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.UI;

[RequireComponent(typeof(ScrollRect))]
[ExecuteInEditMode]
public class ScrollViewMaxHeight : UIBehaviour, ILayoutElement
{
	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private RectTransformChangeEventEmitter contentRectChangeEventEmitter;

	[SerializeField]
	private int m_layoutPriority = 1;

	[SerializeField]
	private bool useTargetParentSize;

	[SerializeField]
	private float targetParentHeight = 935f;

	[SerializeField]
	private List<RectTransform> siblings = new List<RectTransform>();

	[SerializeField]
	private float parentLayoutMargin = 16f;

	[SerializeField]
	private float maxHeight = 100f;

	private RectTransform _rectTransform;

	public float preferredHeight
	{
		get
		{
			float y = scrollRect.content.sizeDelta.y;
			float num = maxHeight;
			if (useTargetParentSize)
			{
				float num2 = 0f;
				foreach (RectTransform sibling in siblings)
				{
					num2 += sibling.rect.height;
				}
				num = targetParentHeight - num2 - parentLayoutMargin;
			}
			if (y > num)
			{
				return num;
			}
			return y;
		}
	}

	public virtual float minWidth => -1f;

	public virtual float minHeight => -1f;

	public virtual float preferredWidth => -1f;

	public virtual float flexibleWidth => -1f;

	public virtual float flexibleHeight => -1f;

	public virtual int layoutPriority => m_layoutPriority;

	private RectTransform rectTransform
	{
		get
		{
			if (_rectTransform == null)
			{
				_rectTransform = base.transform as RectTransform;
			}
			return _rectTransform;
		}
	}

	public virtual void CalculateLayoutInputHorizontal()
	{
	}

	public virtual void CalculateLayoutInputVertical()
	{
	}

	private void OnContentRectChange(RectTransform rectTransform)
	{
		SetDirty();
	}

	protected override void OnEnable()
	{
		if (scrollRect == null)
		{
			scrollRect = GetComponent<ScrollRect>();
		}
		if (contentRectChangeEventEmitter == null)
		{
			contentRectChangeEventEmitter = scrollRect.content.GetComponent<RectTransformChangeEventEmitter>();
		}
		if (contentRectChangeEventEmitter == null)
		{
			contentRectChangeEventEmitter = scrollRect.content.gameObject.AddComponent<RectTransformChangeEventEmitter>();
		}
		base.OnEnable();
		contentRectChangeEventEmitter.OnRectTransformChange += OnContentRectChange;
		SetDirty();
	}

	protected override void OnDisable()
	{
		contentRectChangeEventEmitter.OnRectTransformChange -= OnContentRectChange;
		SetDirty();
		base.OnDisable();
	}

	private void Update()
	{
		if (preferredHeight != rectTransform.rect.height)
		{
			SetDirty();
		}
	}

	protected void SetDirty()
	{
		if (IsActive())
		{
			LayoutRebuilder.MarkLayoutForRebuild(base.transform as RectTransform);
		}
	}
}
