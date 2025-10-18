using System;
using UnityEngine;

namespace Duckov.UI;

public static class TradingUIUtilities
{
	private static UnityEngine.Object activeMerchant;

	public static IMerchant ActiveMerchant
	{
		get
		{
			return activeMerchant as IMerchant;
		}
		set
		{
			activeMerchant = value as UnityEngine.Object;
			TradingUIUtilities.OnActiveMerchantChanged?.Invoke(value);
		}
	}

	public static event Action<IMerchant> OnActiveMerchantChanged;
}
