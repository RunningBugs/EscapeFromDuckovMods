using Duckov.BlackMarkets.UI;
using Duckov.Crops;
using Duckov.Crops.UI;
using Duckov.Endowment.UI;
using Duckov.MasterKeys.UI;
using Duckov.MiniGames;
using Duckov.MiniMaps.UI;
using Duckov.Quests.UI;
using Duckov.UI;
using UnityEngine;

public class ViewsProxy : MonoBehaviour
{
	public void ShowInventoryView()
	{
		if (LevelManager.Instance.IsBaseLevel && (bool)PlayerStorage.Instance)
		{
			PlayerStorage.Instance.InteractableLootBox.InteractWithMainCharacter();
		}
		else
		{
			InventoryView.Show();
		}
	}

	public void ShowQuestView()
	{
		QuestView.Show();
	}

	public void ShowMapView()
	{
		MiniMapView.Show();
	}

	public void ShowKeyView()
	{
		MasterKeysView.Show();
	}

	public void ShowPlayerStats()
	{
		PlayerStatsView.Instance.Open();
	}

	public void ShowEndowmentView()
	{
		EndowmentSelectionPanel.Show();
	}

	public void ShowMapSelectionView()
	{
		MapSelectionView.Instance.Open();
	}

	public void ShowRepairView()
	{
		ItemRepairView.Instance.Open();
	}

	public void ShowFormulasIndexView()
	{
		FormulasIndexView.Show();
	}

	public void ShowBitcoinView()
	{
		BitcoinMinerView.Show();
	}

	public void ShowStorageDock()
	{
		StorageDock.Show();
	}

	public void ShowBlackMarket_Demands()
	{
		BlackMarketView.Show(BlackMarketView.Mode.Demand);
	}

	public void ShowBlackMarket_Supplies()
	{
		BlackMarketView.Show(BlackMarketView.Mode.Supply);
	}

	public void ShowSleepView()
	{
		SleepView.Show();
	}

	public void ShowATMView()
	{
		ATMView.Show();
	}

	public void ShowDecomposeView()
	{
		ItemDecomposeView.Show();
	}

	public void ShowGardenView(Garden garnden)
	{
		GardenView.Show(garnden);
	}

	public void ShowGamingConsoleView(GamingConsole console)
	{
		GamingConsoleView.Show(console);
	}
}
