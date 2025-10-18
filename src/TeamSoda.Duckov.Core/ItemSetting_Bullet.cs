using ItemStatsSystem;

public class ItemSetting_Bullet : ItemSettingBase
{
	public override void SetMarkerParam(Item selfItem)
	{
		selfItem.SetBool("IsBullet", value: true);
	}
}
