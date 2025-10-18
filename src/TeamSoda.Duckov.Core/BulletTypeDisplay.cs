using ItemStatsSystem;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;

public class BulletTypeDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI bulletDisplayName;

	[LocalizationKey("Default")]
	private string NotAssignedTextKey => "UI_Bullet_NotAssigned";

	internal void Setup(int targetBulletID)
	{
		if (targetBulletID < 0)
		{
			bulletDisplayName.text = NotAssignedTextKey.ToPlainText();
			return;
		}
		ItemMetaData metaData = ItemAssetsCollection.GetMetaData(targetBulletID);
		bulletDisplayName.text = metaData.DisplayName;
	}
}
