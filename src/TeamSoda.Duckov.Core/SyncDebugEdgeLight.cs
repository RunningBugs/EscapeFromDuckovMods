using Soda;
using UnityEngine;

public class SyncDebugEdgeLight : MonoBehaviour
{
	private void Awake()
	{
		DebugView.OnDebugViewConfigChanged += OnDebugConfigChanged;
	}

	private void OnDestroy()
	{
		DebugView.OnDebugViewConfigChanged -= OnDebugConfigChanged;
	}

	private void OnDebugConfigChanged(DebugView debugView)
	{
		if (!(debugView == null))
		{
			base.gameObject.SetActive(debugView.EdgeLightActive);
		}
	}
}
