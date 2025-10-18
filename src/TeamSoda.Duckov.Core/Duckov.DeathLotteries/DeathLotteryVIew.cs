using Cysharp.Threading.Tasks;
using Duckov.Economy;
using Duckov.UI;
using Duckov.UI.Animations;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using TMPro;
using UnityEngine;

namespace Duckov.DeathLotteries;

public class DeathLotteryVIew : View
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[LocalizationKey("Default")]
	[SerializeField]
	private string remainingTextFormatKey = "DeathLottery_Remaining";

	[LocalizationKey("Default")]
	[SerializeField]
	private string noRemainingChances = "DeathLottery_NoRemainingChances";

	[SerializeField]
	private TextMeshProUGUI remainingCountText;

	[SerializeField]
	private DeathLotteryCard[] cards;

	[SerializeField]
	private FadeGroup selectionBusyIndicator;

	private DeathLottery target;

	private UniTask selectTask;

	private string RemainingTextFormat => remainingTextFormatKey.ToPlainText();

	public DeathLottery Target => target;

	public int RemainingChances
	{
		get
		{
			if (Target == null)
			{
				return 0;
			}
			return Target.RemainingChances;
		}
	}

	private bool ProcessingSelection => selectTask.Status == UniTaskStatus.Pending;

	protected override void OnOpen()
	{
		base.OnOpen();
		fadeGroup.Show();
		selectionBusyIndicator.SkipHide();
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
	}

	protected override void Awake()
	{
		base.Awake();
		DeathLottery.OnRequestUI += Show;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		DeathLottery.OnRequestUI -= Show;
	}

	private void Show(DeathLottery target)
	{
		this.target = target;
		Setup();
		Open();
	}

	private void RefreshTexts()
	{
		remainingCountText.text = ((RemainingChances > 0) ? RemainingTextFormat.Format(new
		{
			amount = RemainingChances
		}) : noRemainingChances.ToPlainText());
	}

	private void Setup()
	{
		if (target == null || target.Loading)
		{
			return;
		}
		DeathLottery.Status currentStatus = target.CurrentStatus;
		if (currentStatus.valid)
		{
			for (int i = 0; i < currentStatus.candidates.Count; i++)
			{
				cards[i].Setup(this, i);
			}
			RefreshTexts();
			HandleRemaining();
		}
	}

	internal void NotifyEntryClicked(DeathLotteryCard deathLotteryCard, Cost cost)
	{
		if (!(deathLotteryCard == null) && !ProcessingSelection && RemainingChances > 0)
		{
			int index = deathLotteryCard.Index;
			if (!target.CurrentStatus.selectedItems.Contains(index))
			{
				selectTask = SelectTask(index, cost);
			}
		}
	}

	private async UniTask SelectTask(int index, Cost cost)
	{
		selectionBusyIndicator.Show();
		bool uncovered = await target.Select(index, cost);
		cards[index].NotifyFacing(uncovered);
		RefreshTexts();
		selectionBusyIndicator.Hide();
		HandleRemaining();
	}

	private void HandleRemaining()
	{
		if (RemainingChances <= 0)
		{
			DeathLotteryCard[] array = cards;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].NotifyFacing(uncovered: true);
			}
		}
	}
}
