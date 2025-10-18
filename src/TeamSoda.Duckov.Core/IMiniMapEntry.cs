using UnityEngine;

public interface IMiniMapEntry
{
	Sprite Sprite { get; }

	float PixelSize { get; }

	Vector2 Offset { get; }

	string SceneID { get; }

	bool Hide { get; }

	bool NoSignal { get; }
}
