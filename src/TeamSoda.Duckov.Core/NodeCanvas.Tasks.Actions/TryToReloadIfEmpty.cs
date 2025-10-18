using Duckov;
using NodeCanvas.Framework;
using SodaCraft.Localizations;

namespace NodeCanvas.Tasks.Actions;

public class TryToReloadIfEmpty : ActionTask<AICharacterController>
{
	public string poptextWhileReloading = "PopText_Reloading";

	public bool postSound;

	private bool isFirstTime = true;

	public string SoundKey => "normal";

	private string Key => poptextWhileReloading;

	private string DisplayText => poptextWhileReloading.ToPlainText();

	protected override string OnInit()
	{
		return null;
	}

	protected override void OnExecute()
	{
		ItemAgent_Gun gun = base.agent.CharacterMainControl.GetGun();
		if (gun == null)
		{
			EndAction(success: true);
			return;
		}
		if (gun.BulletCount <= 0)
		{
			base.agent.CharacterMainControl.TryToReload();
			if (!isFirstTime)
			{
				if (!base.agent.CharacterMainControl.Health.Hidden && poptextWhileReloading != string.Empty && base.agent.canTalk)
				{
					base.agent.CharacterMainControl.PopText(poptextWhileReloading.ToPlainText());
				}
				if (postSound && SoundKey != string.Empty && (bool)base.agent && (bool)base.agent.CharacterMainControl)
				{
					AudioManager.PostQuak(SoundKey, base.agent.CharacterMainControl.AudioVoiceType, base.agent.CharacterMainControl.gameObject);
				}
			}
		}
		isFirstTime = false;
		EndAction(success: true);
	}

	protected override void OnUpdate()
	{
	}

	protected override void OnStop()
	{
	}

	protected override void OnPause()
	{
	}
}
