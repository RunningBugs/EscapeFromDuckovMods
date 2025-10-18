using Cysharp.Threading.Tasks;
using Duckov.UI.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EvacuationCountdownUI : MonoBehaviour
{
	private static EvacuationCountdownUI _instance;

	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private Image progressFill;

	[SerializeField]
	private TextMeshProUGUI countdownDigit;

	[SerializeField]
	private string digitFormat = "{0:00}:{1:00}<sub>.{2:000}</sub>";

	private CountDownArea target;

	public static EvacuationCountdownUI Instance => _instance;

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
		}
		if (_instance != this)
		{
			Debug.LogWarning("Multiple Evacuation Countdown UI detected");
		}
	}

	private string ToDigitString(float number)
	{
		int num = (int)number;
		int num2 = Mathf.Min(999, Mathf.RoundToInt((number - (float)num) * 1000f));
		int num3 = num / 60;
		num -= num3 * 60;
		return string.Format(digitFormat, num3, num, num2);
	}

	private void Update()
	{
		if (target == null && fadeGroup.IsShown)
		{
			Hide().Forget();
		}
		Refresh();
	}

	private void Refresh()
	{
		if (!(target == null))
		{
			progressFill.fillAmount = target.Progress;
			countdownDigit.text = ToDigitString(target.RemainingTime);
		}
	}

	private async UniTask Hide()
	{
		target = null;
		await fadeGroup.HideAndReturnTask();
	}

	private async UniTask Show(CountDownArea target)
	{
		this.target = target;
		if (!(this.target == null))
		{
			await fadeGroup.ShowAndReturnTask();
		}
	}

	public static void Request(CountDownArea target)
	{
		if (!(Instance == null))
		{
			Instance.Show(target).Forget();
		}
	}

	public static void Release(CountDownArea target)
	{
		if (!(Instance == null) && Instance.target == target)
		{
			Instance.Hide().Forget();
		}
	}
}
