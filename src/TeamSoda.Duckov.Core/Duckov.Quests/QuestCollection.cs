using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Duckov.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Duckov.Quests;

[CreateAssetMenu(menuName = "Quest Collection")]
public class QuestCollection : ScriptableObject, IList<Quest>, ICollection<Quest>, IEnumerable<Quest>, IEnumerable, ISelfValidator
{
	[SerializeField]
	private List<Quest> list;

	public static QuestCollection Instance => GameplayDataSettings.QuestCollection;

	public Quest this[int index]
	{
		get
		{
			return list[index];
		}
		set
		{
			list[index] = value;
		}
	}

	public int Count => list.Count;

	public bool IsReadOnly => true;

	public void Add(Quest item)
	{
		list.Add(item);
	}

	public void Clear()
	{
		list.Clear();
	}

	public bool Contains(Quest item)
	{
		return list.Contains(item);
	}

	public void CopyTo(Quest[] array, int arrayIndex)
	{
		list.CopyTo(array, arrayIndex);
	}

	public IEnumerator<Quest> GetEnumerator()
	{
		return list.GetEnumerator();
	}

	public int IndexOf(Quest item)
	{
		return list.IndexOf(item);
	}

	public void Insert(int index, Quest item)
	{
		list.Insert(index, item);
	}

	public bool Remove(Quest item)
	{
		return list.Remove(item);
	}

	public void RemoveAt(int index)
	{
		list.RemoveAt(index);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Collect()
	{
	}

	public void Validate(SelfValidationResult result)
	{
		_ = from e in list
			group e by (e == null) ? (-1) : e.ID;
		if ((from e in list
			group e by (e == null) ? (-1) : e.ID).Any((IGrouping<int, Quest> g) => g.Count() > 1))
		{
			result.AddError("存在冲突的QuestID。").WithFix("自动重新分配ID", AutoFixID);
		}
	}

	private void AutoFixID()
	{
		int num = list.Max((Quest e) => e.ID) + 1;
		foreach (IGrouping<int, Quest> item in from e in list
			group e by e.ID into g
			where g.Count() > 1
			select g)
		{
			int num2 = 0;
			foreach (Quest item2 in item)
			{
				if (!(item2 == null) && num2++ != 0)
				{
					item2.ID = num++;
				}
			}
		}
	}

	public Quest Get(int id)
	{
		return list.FirstOrDefault((Quest q) => q != null && q.ID == id);
	}
}
