using Duckov.PerkTrees;
using Duckov.Utilities;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public class RequireItemEntry : MonoBehaviour, IPoolable
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private string textFormat = "{0} x{1}";

	public void NotifyPooled()
	{
	}

	public void NotifyReleased()
	{
	}

	public void Setup(PerkRequirement.RequireItemEntry target)
	{
		int id = target.id;
		ItemMetaData metaData = ItemAssetsCollection.GetMetaData(id);
		icon.sprite = metaData.icon;
		string displayName = metaData.DisplayName;
		int itemCount = ItemUtilities.GetItemCount(id);
		text.text = string.Format(textFormat, displayName, target.amount, itemCount);
	}
}
