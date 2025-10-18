using Duckov.MasterKeys.UI;

namespace Duckov.MasterKeys;

public class MasterKeyRegisterViewInvoker : InteractableBase
{
	protected override void Awake()
	{
		base.Awake();
		finishWhenTimeOut = true;
	}

	protected override void OnInteractFinished()
	{
		MasterKeysRegisterView.Show();
	}
}
