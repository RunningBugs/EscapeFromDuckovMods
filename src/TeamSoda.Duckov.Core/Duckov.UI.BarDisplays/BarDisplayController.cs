using UnityEngine;

namespace Duckov.UI.BarDisplays;

public class BarDisplayController : MonoBehaviour
{
	[SerializeField]
	private BarDisplay bar;

	protected virtual float Current => 0f;

	protected virtual float Max => 0f;

	protected void Refresh()
	{
		float current = Current;
		float max = Max;
		bar.SetValue(current, max);
	}
}
