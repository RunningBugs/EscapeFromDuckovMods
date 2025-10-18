using System.Collections.Generic;
using UnityEngine;

public interface IMiniMapDataProvider
{
	Sprite CombinedSprite { get; }

	List<IMiniMapEntry> Maps { get; }

	float PixelSize { get; }

	Vector3 CombinedCenter { get; }
}
