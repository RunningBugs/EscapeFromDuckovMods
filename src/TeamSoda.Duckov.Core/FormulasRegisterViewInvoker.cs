using System.Collections.Generic;
using Duckov.UI;
using Duckov.Utilities;
using UnityEngine;

public class FormulasRegisterViewInvoker : InteractableBase
{
	[SerializeField]
	private List<Tag> additionalTags;

	protected override void Awake()
	{
		base.Awake();
		finishWhenTimeOut = true;
	}

	protected override void OnInteractFinished()
	{
		FormulasRegisterView.Show(additionalTags);
	}
}
