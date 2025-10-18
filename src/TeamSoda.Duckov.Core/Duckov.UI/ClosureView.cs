using Cysharp.Threading.Tasks;
using Duckov.Scenes;
using Duckov.UI.Animations;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public class ClosureView : View
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private FadeGroup contentFadeGroup;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	[LocalizationKey("Default")]
	private string evacuatedTitleTextKey = "UI_Closure_Escaped";

	[SerializeField]
	private Color evacuatedTitleTextColor = Color.white;

	[SerializeField]
	[LocalizationKey("Default")]
	private string failedTitleTextKey = "UI_Closure_Dead";

	[SerializeField]
	private Color failedTitleTextColor = Color.red;

	[SerializeField]
	private GameObject damageInfoContainer;

	[SerializeField]
	private TextMeshProUGUI damageSourceText;

	[SerializeField]
	private Image expBar_OldFill;

	[SerializeField]
	private Image expBar_CurrentFill;

	[SerializeField]
	private AnimationCurve expBarAnimationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private float expBarAnimationTime = 3f;

	[SerializeField]
	private TextMeshProUGUI expDisplay;

	[SerializeField]
	private string expFormat = "{0}/<sub>{1}</sub>";

	[SerializeField]
	private TextMeshProUGUI levelDisplay;

	[SerializeField]
	private string levelFormat = "Lv.{0}";

	[SerializeField]
	private PunchReceiver levelDisplayPunchReceiver;

	[SerializeField]
	private PunchReceiver barPunchReceiver;

	[SerializeField]
	private Button continueButton;

	[SerializeField]
	private PunchReceiver continueButtonPunchReceiver;

	private string sfx_Pop = "UI/pop";

	private string sfx_ExpUp = "UI/exp_up";

	private string sfx_LvUp = "UI/level_up";

	private bool continueButtonClicked;

	private bool canContinue;

	private float lastTimeExpUpSfxPlayed = float.MinValue;

	private const float minIntervalForExpUpSfx = 0.05f;

	private int cachedLevel = -1;

	private (long from, long to) cachedLevelRange;

	private long cachedLevelLength;

	private int displayingLevel = -1;

	public static ClosureView Instance => View.GetViewInstance<ClosureView>();

	private string EvacuatedTitleText => evacuatedTitleTextKey.ToPlainText();

	private string FailedTitleText => failedTitleTextKey.ToPlainText();

	protected override void Awake()
	{
		base.Awake();
		continueButton.onClick.AddListener(OnContinueButtonClicked);
	}

	private void OnContinueButtonClicked()
	{
		if (canContinue)
		{
			continueButtonClicked = true;
			contentFadeGroup.Hide();
		}
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		fadeGroup.Show();
		contentFadeGroup.Show();
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
	}

	public static async UniTask ShowAndReturnTask(float duration = 0.5f)
	{
		if (!(Instance == null))
		{
			Instance.canContinue = false;
			await BlackScreen.ShowAndReturnTask(null, 1f, duration);
			if ((bool)MultiSceneCore.Instance)
			{
				MultiSceneCore.Instance.PlayStinger();
			}
			Instance.Open();
			Instance.SetupTitle(dead: false);
			Instance.SetupBeginning();
			Instance.damageInfoContainer.SetActive(value: false);
			await BlackScreen.HideAndReturnTask(null, 0f, duration);
			Instance.canContinue = true;
			await Instance.ClosureTask();
		}
	}

	public static async UniTask ShowAndReturnTask(DamageInfo dmgInfo, float duration = 0.5f)
	{
		if (Instance == null)
		{
			return;
		}
		Instance.canContinue = false;
		await BlackScreen.ShowAndReturnTask(null, 1f, duration);
		if (!(Instance == null))
		{
			Instance.Open();
			Instance.SetupTitle(dead: true);
			Instance.SetupBeginning();
			Instance.SetupDamageInfo(dmgInfo);
			await BlackScreen.HideAndReturnTask(null, 0f, duration);
			if (!(Instance == null))
			{
				Instance.canContinue = true;
				await Instance.ClosureTask();
			}
		}
	}

	private void SetupDamageInfo(DamageInfo dmgInfo)
	{
		damageSourceText.text = dmgInfo.GenerateDescription();
		damageInfoContainer.gameObject.SetActive(value: true);
	}

	private async UniTask ClosureTask()
	{
		continueButtonClicked = false;
		long cachedExp = EXPManager.CachedExp;
		long eXP = EXPManager.EXP;
		await AnimateExpBar(cachedExp, eXP);
		continueButton.gameObject.SetActive(value: true);
		continueButtonPunchReceiver.Punch();
		AudioManager.Post(sfx_Pop);
		while (!continueButtonClicked)
		{
			await UniTask.NextFrame();
		}
		AudioManager.Post("UI/confirm");
	}

	private void SetupBeginning()
	{
		long cachedExp = EXPManager.CachedExp;
		long eXP = EXPManager.EXP;
		Refresh(0f, cachedExp, eXP);
		continueButton.gameObject.SetActive(value: false);
	}

	private void SetupTitle(bool dead)
	{
		if (dead)
		{
			titleText.color = failedTitleTextColor;
			titleText.text = FailedTitleText;
		}
		else
		{
			titleText.color = evacuatedTitleTextColor;
			titleText.text = EvacuatedTitleText;
		}
	}

	private async UniTask AnimateExpBar(long fromExp, long toExp)
	{
		if (fromExp != toExp)
		{
			float time = 0f;
			long displayingExp = fromExp;
			while (time < expBarAnimationTime && fromExp != toExp)
			{
				float time2 = time / expBarAnimationTime;
				long num = Refresh(expBarAnimationCurve.Evaluate(time2), fromExp, toExp);
				if (num != displayingExp)
				{
					SpitExpUpSfx((float)(num - fromExp) / (float)(toExp - fromExp));
				}
				displayingExp = num;
				time += Time.unscaledDeltaTime;
				await UniTask.NextFrame();
			}
			SpitExpUpSfx(1f);
		}
		SetExpDisplay(toExp, fromExp);
		SetLevelDisplay(cachedLevel);
	}

	private void SpitExpUpSfx(float expDelta)
	{
		float unscaledTime = Time.unscaledTime;
		if (!(unscaledTime - lastTimeExpUpSfxPlayed < 0.05f))
		{
			lastTimeExpUpSfxPlayed = unscaledTime;
			AudioManager.SetRTPC("ExpDelta", expDelta);
			AudioManager.Post(sfx_ExpUp);
		}
	}

	private long Refresh(float t, long fromExp, long toExp)
	{
		long num = LongLerp(fromExp, toExp, t);
		SetExpDisplay(num, fromExp);
		SetLevelDisplay(cachedLevel);
		return num;
	}

	private long LongLerp(long from, long to, float t)
	{
		return (long)((float)(to - from) * t) + from;
	}

	private void CacheLevelInfo(int level)
	{
		if (level != cachedLevel)
		{
			cachedLevel = level;
			cachedLevelRange = EXPManager.Instance.GetLevelExpRange(level);
			cachedLevelLength = cachedLevelRange.to - cachedLevelRange.from;
		}
	}

	private void SetExpDisplay(long currentExp, long oldExp)
	{
		int level = EXPManager.Instance.LevelFromExp(currentExp);
		CacheLevelInfo(level);
		float fillAmount = 0f;
		if (oldExp >= cachedLevelRange.from && oldExp <= cachedLevelRange.to)
		{
			fillAmount = (float)(oldExp - cachedLevelRange.from) / (float)cachedLevelLength;
		}
		float fillAmount2 = (float)(currentExp - cachedLevelRange.from) / (float)cachedLevelLength;
		expBar_OldFill.fillAmount = fillAmount;
		expBar_CurrentFill.fillAmount = fillAmount2;
		expDisplay.text = string.Format(expFormat, currentExp, cachedLevelRange.to);
	}

	private void SetLevelDisplay(int level)
	{
		if (displayingLevel > 0 && level != displayingLevel)
		{
			LevelUpPunch();
		}
		displayingLevel = level;
		levelDisplay.text = string.Format(levelFormat, level);
	}

	private void LevelUpPunch()
	{
		levelDisplayPunchReceiver?.Punch();
		barPunchReceiver?.Punch();
		AudioManager.Post(sfx_LvUp);
	}

	internal override void TryQuit()
	{
	}
}
