using ItemStatsSystem;

public class ItemSetting_NightVision : ItemSettingBase
{
	private bool nightVisionOn = true;

	public override void OnInit()
	{
		if ((bool)_item)
		{
			_item.onPluggedIntoSlot += OnplugedIntoSlot;
		}
	}

	private void OnplugedIntoSlot(Item item)
	{
		nightVisionOn = true;
		SyncModifiers();
	}

	private void OnDestroy()
	{
		if ((bool)_item)
		{
			_item.onPluggedIntoSlot -= OnplugedIntoSlot;
		}
	}

	public void ToggleNightVison()
	{
		nightVisionOn = !nightVisionOn;
		SyncModifiers();
	}

	private void SyncModifiers()
	{
		if ((bool)_item)
		{
			_item.Modifiers.ModifierEnable = nightVisionOn;
		}
	}

	public override void SetMarkerParam(Item selfItem)
	{
		selfItem.SetBool("IsNightVision", value: true);
	}
}
