using UnityEngine;

namespace Duckov.Crops;

public interface IGardenSizeAdder
{
	Vector2Int GetValue(string gardenID);
}
