using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.UI;

[ExecuteInEditMode]
public class RectTransformChangeEventEmitter : UIBehaviour
{
	public event Action<RectTransform> OnRectTransformChange;

	private void SetDirty()
	{
		this.OnRectTransformChange?.Invoke(base.transform as RectTransform);
	}

	protected override void OnRectTransformDimensionsChange()
	{
		SetDirty();
	}

	protected override void OnEnable()
	{
		SetDirty();
	}
}
