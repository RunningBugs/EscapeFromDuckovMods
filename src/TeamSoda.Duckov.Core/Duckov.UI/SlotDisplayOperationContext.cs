namespace Duckov.UI;

public struct SlotDisplayOperationContext
{
	public enum Operation
	{
		None,
		Equip,
		Unequip,
		Deny
	}

	public SlotDisplay slotDisplay;

	public Operation operation;

	public bool succeed;

	public SlotDisplayOperationContext(SlotDisplay slotDisplay, Operation operation, bool succeed)
	{
		this.slotDisplay = slotDisplay;
		this.operation = operation;
		this.succeed = succeed;
	}
}
