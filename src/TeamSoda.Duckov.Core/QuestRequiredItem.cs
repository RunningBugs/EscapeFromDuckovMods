using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestRequiredItem : MonoBehaviour
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI text;

	public void Set(int itemTypeID, int count = 1)
	{
		if (itemTypeID <= 0 || count <= 0)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		ItemMetaData metaData = ItemAssetsCollection.GetMetaData(itemTypeID);
		if (metaData.id == 0)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		icon.sprite = metaData.icon;
		text.text = $"{metaData.DisplayName} x{count}";
		base.gameObject.SetActive(value: true);
	}
}
