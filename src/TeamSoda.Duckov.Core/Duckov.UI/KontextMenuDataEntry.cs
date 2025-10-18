using System;
using UnityEngine;

namespace Duckov.UI;

public class KontextMenuDataEntry
{
	public Sprite icon;

	public string text;

	public Action action;

	public void Invoke()
	{
		action?.Invoke();
	}
}
