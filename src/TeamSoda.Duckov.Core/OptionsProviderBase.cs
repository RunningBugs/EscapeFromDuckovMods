using UnityEngine;

public abstract class OptionsProviderBase : MonoBehaviour
{
	public abstract string Key { get; }

	public abstract string[] GetOptions();

	public abstract string GetCurrentOption();

	public abstract void Set(int index);
}
