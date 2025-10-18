using ItemStatsSystem;

public interface IMerchant
{
	int ConvertPrice(Item item, bool selling = false);
}
