using Duckov.UI.Animations;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public class ItemDetailsPanel : ManagedUIElement
{
	private static ItemDetailsPanel instance;

	private Item target;

	[SerializeField]
	private ItemDetailsDisplay display;

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private Button closeButton;

	private ManagedUIElement source;

	protected override void Awake()
	{
		base.Awake();
		if (instance == null)
		{
			instance = this;
		}
		closeButton.onClick.AddListener(OnCloseButtonClicked);
	}

	private void OnCloseButtonClicked()
	{
		Close();
	}

	public static void Show(Item target, ManagedUIElement source = null)
	{
		if (!(instance == null))
		{
			instance.Open(target, source);
		}
	}

	public void Open(Item target, ManagedUIElement source)
	{
		this.target = target;
		this.source = source;
		Open(source);
	}

	protected override void OnOpen()
	{
		if (!(target == null))
		{
			base.gameObject.SetActive(value: true);
			Setup(target);
			fadeGroup.Show();
		}
	}

	protected override void OnClose()
	{
		UnregisterEvents();
		target = null;
		fadeGroup.Hide();
	}

	private void OnDisable()
	{
		UnregisterEvents();
	}

	internal void Setup(Item target)
	{
		display.Setup(target);
	}

	private void UnregisterEvents()
	{
		display.UnregisterEvents();
	}
}
