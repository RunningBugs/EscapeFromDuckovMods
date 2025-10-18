using Duckov.UI;
using Duckov.UI.Animations;
using UnityEngine;

public class ATMView : View
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private ATMPanel atmPanel;

	public static ATMView Instance => View.GetViewInstance<ATMView>();

	protected override void Awake()
	{
		base.Awake();
	}

	public static void Show()
	{
		ATMView instance = Instance;
		if (!(instance == null))
		{
			instance.Open();
		}
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		fadeGroup.Show();
		atmPanel.ShowSelectPanel(skipHideOthers: true);
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
	}
}
