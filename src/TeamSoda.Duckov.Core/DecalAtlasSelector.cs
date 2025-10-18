using UnityEngine;
using UnityEngine.Rendering.Universal;

[DisallowMultipleComponent]
public class DecalAtlasSelector : MonoBehaviour
{
	[Header("Atlas 设置")]
	[Min(1f)]
	public int rows = 1;

	[Min(1f)]
	public int columns = 1;

	[Min(0f)]
	public int index;

	private DecalProjector projector;

	private void OnValidate()
	{
		if (projector == null)
		{
			projector = GetComponent<DecalProjector>();
		}
		if (!(projector == null) && rows > 0 && columns > 0)
		{
			int num = rows * columns;
			int num2 = Mathf.Clamp(index, 0, num - 1);
			Vector2 uvScale = new Vector2(1f / (float)columns, 1f / (float)rows);
			Vector2 uvBias = new Vector2((float)(num2 % columns) * uvScale.x, 1f - uvScale.y - (float)(num2 / columns) * uvScale.y);
			projector.uvScale = uvScale;
			projector.uvBias = uvBias;
		}
	}
}
