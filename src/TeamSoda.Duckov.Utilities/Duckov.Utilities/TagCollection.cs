using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Duckov.Utilities;

[Serializable]
public class TagCollection : ICollection<Tag>, IEnumerable<Tag>, IEnumerable
{
	public List<Tag> list = new List<Tag>();

	public int Count => list.Count;

	public bool IsReadOnly => false;

	public bool Check(ICollection<Tag> requireTags, ICollection<Tag> excludeTags)
	{
		Print(list);
		Print(requireTags);
		Print(excludeTags);
		foreach (Tag requireTag in requireTags)
		{
			if (!(requireTag == null) && !Contains(requireTag))
			{
				return false;
			}
		}
		foreach (Tag excludeTag in excludeTags)
		{
			if (!(excludeTag == null) && Contains(excludeTag))
			{
				return false;
			}
		}
		return true;
		static string Print(ICollection<Tag> tags)
		{
			string text = "";
			foreach (Tag tag in tags)
			{
				text += $"{tag.name}({tag.GetInstanceID()})";
			}
			return text;
		}
	}

	public void Add(Tag item)
	{
		if (!(item == null) && !Contains(item))
		{
			list.Add(item);
		}
	}

	public void Clear()
	{
		list.Clear();
	}

	public bool Contains(Tag item)
	{
		if (item == null)
		{
			return false;
		}
		return list.Any((Tag e) => e != null && e.Hash == item.Hash);
	}

	public bool Contains(string tagName)
	{
		return list.Any((Tag e) => e != null && e.name == tagName);
	}

	public void CopyTo(Tag[] array, int arrayIndex)
	{
		list.CopyTo(array, arrayIndex);
	}

	public IEnumerator<Tag> GetEnumerator()
	{
		return list.GetEnumerator();
	}

	public bool Remove(Tag item)
	{
		return list.RemoveAll((Tag e) => e.Hash == item.Hash) > 0;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return list.GetEnumerator();
	}

	public Tag Get(int index)
	{
		if (index < 0 || index >= list.Count)
		{
			return null;
		}
		return list[index];
	}
}
