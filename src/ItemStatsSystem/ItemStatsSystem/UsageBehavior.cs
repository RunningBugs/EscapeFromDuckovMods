using UnityEngine;

namespace ItemStatsSystem;

public abstract class UsageBehavior : MonoBehaviour
{
	public struct DisplaySettingsData
	{
		public bool display;

		public string description;

		public string Description => description;
	}

	public virtual DisplaySettingsData DisplaySettings => default(DisplaySettingsData);

	public abstract bool CanBeUsed(Item item, object user);

	protected abstract void OnUse(Item item, object user);

	public void Use(Item item, object user)
	{
		OnUse(item, user);
	}
}
