using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CustomFacePartCollection
{
	[SerializeField]
	private List<CustomFacePartMeta> parts;

	public int totalCount => parts.Count;

	public void Clear()
	{
		parts.Clear();
	}

	public CustomFacePart GetNextOrPrevPrefab(int currentID, int direction)
	{
		int num = parts.FindIndex((CustomFacePartMeta part) => part.id == currentID);
		num = ((num != -1) ? (num + direction) : 0);
		if (num < 0)
		{
			num = parts.Count - 1;
		}
		else if (num >= parts.Count)
		{
			num = 0;
		}
		return parts[num].part;
	}

	public CustomFacePart GetPartPrefab(int id)
	{
		int num = parts.FindIndex((CustomFacePartMeta part) => part.id == id);
		if (num < 0)
		{
			return parts[0].part;
		}
		return parts[num].part;
	}

	public void Sort()
	{
		parts.Sort((CustomFacePartMeta a, CustomFacePartMeta b) => a.id.CompareTo(b.id));
	}

	public void AddNewCollection(CustomFacePart newPart)
	{
		CustomFacePartMeta item = new CustomFacePartMeta
		{
			part = newPart,
			id = newPart.id
		};
		parts.Add(item);
	}
}
