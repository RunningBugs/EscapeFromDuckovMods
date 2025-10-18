using Duckov.Economy;
using Duckov.UI;
using Duckov.UI.Animations;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.DeathLotteries;

public class DeathLotteryCard : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private CardDisplay cardDisplay;

	[SerializeField]
	private ItemDisplay itemDisplay;

	[SerializeField]
	private CostDisplay costDisplay;

	[SerializeField]
	private GameObject freeIndicator;

	[SerializeField]
	private FadeGroup costFade;

	[SerializeField]
	private GameObject selectedIndicator;

	private DeathLotteryVIew master;

	private int index;

	private Item targetItem;

	public int Index => index;

	private bool Selected
	{
		get
		{
			if (master == null)
			{
				return false;
			}
			if (master.Target == null)
			{
				return false;
			}
			if (master.Target.CurrentStatus.selectedItems == null)
			{
				return false;
			}
			return master.Target.CurrentStatus.selectedItems.Contains(Index);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!(master == null) && !(master.Target == null))
		{
			DeathLottery.OptionalCosts cost = master.Target.GetCost();
			master.NotifyEntryClicked(this, cost.costA);
		}
	}

	public void Setup(DeathLotteryVIew master, int index)
	{
		if (!(master == null) && !(master.Target == null))
		{
			this.master = master;
			targetItem = master.Target.ItemInstances[index];
			this.index = index;
			itemDisplay.Setup(targetItem);
			cardDisplay.SetFacing(master.Target.CurrentStatus.selectedItems.Contains(index), skipAnimation: true);
			Refresh();
		}
	}

	public void NotifyFacing(bool uncovered)
	{
		cardDisplay.SetFacing(uncovered);
		Refresh();
	}

	private void Refresh()
	{
		selectedIndicator.SetActive(Selected);
	}

	private void Awake()
	{
		costFade.Hide();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (master.Target.CurrentStatus.SelectedCount < master.Target.MaxChances)
		{
			Cost costA = master.Target.GetCost().costA;
			costDisplay.Setup(costA);
			freeIndicator.SetActive(costA.IsFree);
			costFade.Show();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		costFade.Hide();
	}
}
