using ItemStatsSystem;
using LeTai.TrueShadow;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;

public class BulletTypeSelectButton : MonoBehaviour
{
	private int bulletTypeID;

	private int bulletCount;

	public BulletTypeHUD bulletTypeHUD;

	public TextMeshProUGUI nameText;

	public TextMeshProUGUI countText;

	public TrueShadow selectShadow;

	public GameObject indicator;

	public int BulletTypeID => bulletTypeID;

	public void SetSelection(bool selected)
	{
		selectShadow.enabled = selected;
		indicator.SetActive(selected);
	}

	public void Init(int id, int count)
	{
		bulletTypeID = id;
		bulletCount = count;
		SetSelection(selected: false);
		RefreshContent();
	}

	public void RefreshContent()
	{
		nameText.text = GetBulletName(bulletTypeID);
		countText.text = bulletCount.ToString();
	}

	public string GetBulletName(int id)
	{
		if (id > 0)
		{
			return ItemAssetsCollection.GetMetaData(id).DisplayName;
		}
		return "UI_Bullet_NotAssigned".ToPlainText();
	}
}
