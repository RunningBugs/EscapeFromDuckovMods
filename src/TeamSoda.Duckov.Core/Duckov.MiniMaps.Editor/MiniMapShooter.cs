using UnityEngine;

namespace Duckov.MiniMaps.Editor;

public class MiniMapShooter : MonoBehaviour
{
	private void Awake()
	{
		Object.Destroy(base.gameObject);
	}
}
