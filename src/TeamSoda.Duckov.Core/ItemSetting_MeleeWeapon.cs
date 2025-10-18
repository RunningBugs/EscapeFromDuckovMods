using ItemStatsSystem;

public class ItemSetting_MeleeWeapon : ItemSettingBase
{
	public bool dealExplosionDamage;

	public override void Start()
	{
		base.Start();
	}

	public override void SetMarkerParam(Item selfItem)
	{
		selfItem.SetBool("IsMeleeWeapon", value: true);
	}
}
