using UnityEngine;

namespace Duckov.Utilities;

[CreateAssetMenu(menuName = "String Lists/Collection")]
public class StringLists : ScriptableObject
{
	private static StringLists cachedDefault;

	[SerializeField]
	private StringList statKeys;

	[SerializeField]
	private StringList slotNames;

	[SerializeField]
	private StringList itemAgentKeys;

	private static StringLists Default
	{
		get
		{
			if (cachedDefault == null)
			{
				cachedDefault = Resources.Load<StringLists>("DefaultStringLists");
			}
			return cachedDefault;
		}
	}

	public static StringList StatKeys => Default?.statKeys;

	public static StringList SlotNames => Default?.slotNames;

	public static StringList ItemAgentKeys => Default?.itemAgentKeys;
}
