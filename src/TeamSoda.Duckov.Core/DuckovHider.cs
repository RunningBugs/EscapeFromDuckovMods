using Cysharp.Threading.Tasks;
using FOW;

public class DuckovHider : HiderBehavior
{
	public CharacterMainControl character;

	private float hideDelay = 0.2f;

	private bool targetHide;

	private bool mainCharacterDied;

	protected override void Awake()
	{
		base.Awake();
		LevelManager.OnMainCharacterDead += OnMainCharacterDie;
	}

	private void OnDestroy()
	{
		LevelManager.OnMainCharacterDead -= OnMainCharacterDie;
	}

	protected override void OnHide()
	{
		if ((bool)LevelManager.Instance && LevelManager.Instance.IsRaidMap && !mainCharacterDied)
		{
			targetHide = true;
			HideDelay();
		}
	}

	protected override void OnReveal()
	{
		targetHide = false;
		character.Show();
	}

	private async UniTask HideDelay()
	{
		await UniTask.WaitForSeconds(hideDelay);
		if (targetHide && character != null)
		{
			character.Hide();
		}
	}

	private void OnMainCharacterDie(DamageInfo damageInfo)
	{
		mainCharacterDied = true;
		OnReveal();
	}
}
