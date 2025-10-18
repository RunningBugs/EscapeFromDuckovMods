using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.MiniMaps;

public interface IPointOfInterest
{
	int OverrideScene => -1;

	Sprite Icon => null;

	Color Color => Color.white;

	string DisplayName => null;

	Color ShadowColor => Color.white;

	float ShadowDistance => 0f;

	bool IsArea => false;

	float AreaRadius => 1f;

	bool HideIcon => false;

	float ScaleFactor => 1f;

	void NotifyClicked(PointerEventData eventData)
	{
	}
}
