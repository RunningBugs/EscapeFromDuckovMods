using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Duckov.Utilities;

[CreateAssetMenu(menuName = "String Lists/List")]
public class StringList : ScriptableObject, IEnumerable<string>, IEnumerable
{
	[SerializeField]
	private List<string> strings = new List<string>();

	private ReadOnlyCollection<string> strings_ReadOnly;

	public ReadOnlyCollection<string> Strings
	{
		get
		{
			if (strings_ReadOnly == null)
			{
				strings_ReadOnly = new ReadOnlyCollection<string>(strings);
			}
			return strings_ReadOnly;
		}
	}

	public IEnumerator<string> GetEnumerator()
	{
		return strings.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
