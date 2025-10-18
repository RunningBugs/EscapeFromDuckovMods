using SodaCraft.Localizations;
using UnityEngine;
using UnityEngine.Events;

namespace Duckov.MiniGames.GoldMiner;

public class GoldMiner_ShopItem : MonoBehaviour
{
	[SerializeField]
	private Sprite icon;

	[LocalizationKey("Default")]
	[SerializeField]
	private string displayNameKey;

	[SerializeField]
	private int basePrice;

	public UnityEvent<GoldMiner> onBought;

	public Sprite Icon => icon;

	public string DisplayNameKey => displayNameKey;

	public string DisplayName => displayNameKey.ToPlainText();

	public int BasePrice => basePrice;

	public void OnBought(GoldMiner target)
	{
		onBought?.Invoke(target);
	}
}
