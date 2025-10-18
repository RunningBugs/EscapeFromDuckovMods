using Duckov.UI.Animations;
using UnityEngine;

namespace Duckov.UI;

public class PlayerStatsView : View
{
	[SerializeField]
	private FadeGroup fadeGroup;

	public static PlayerStatsView Instance => View.GetViewInstance<PlayerStatsView>();

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		fadeGroup.Show();
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
	}

	private void OnEnable()
	{
		RegisterEvents();
	}

	private void OnDisable()
	{
		UnregisterEvents();
	}

	private void RegisterEvents()
	{
	}

	private void UnregisterEvents()
	{
	}
}
