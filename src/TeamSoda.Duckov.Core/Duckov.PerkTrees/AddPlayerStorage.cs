using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.PerkTrees;

public class AddPlayerStorage : PerkBehaviour
{
	[SerializeField]
	private int addCapacity;

	private string DescriptionFormat => "PerkBehaviour_AddPlayerStorage".ToPlainText();

	public override string Description => DescriptionFormat.Format(new { addCapacity });

	protected override void OnAwake()
	{
		PlayerStorage.OnRecalculateStorageCapacity += OnRecalculatePlayerStorage;
	}

	protected override void OnOnDestroy()
	{
		PlayerStorage.OnRecalculateStorageCapacity -= OnRecalculatePlayerStorage;
	}

	private void OnRecalculatePlayerStorage(PlayerStorage.StorageCapacityCalculationHolder holder)
	{
		if (base.Master.Unlocked)
		{
			holder.capacity += addCapacity;
		}
	}

	protected override void OnUnlocked()
	{
		base.OnUnlocked();
		PlayerStorage.NotifyCapacityDirty();
	}
}
