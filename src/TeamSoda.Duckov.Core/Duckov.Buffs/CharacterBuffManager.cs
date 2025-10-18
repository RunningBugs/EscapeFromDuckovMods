using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Duckov.Buffs;

public class CharacterBuffManager : MonoBehaviour
{
	[SerializeField]
	private CharacterMainControl master;

	private List<Buff> buffs = new List<Buff>();

	private ReadOnlyCollection<Buff> _readOnlyBuffsCollection;

	private List<Buff> outOfTimeBuffsBuffer = new List<Buff>();

	public CharacterMainControl Master => master;

	public ReadOnlyCollection<Buff> Buffs
	{
		get
		{
			if (_readOnlyBuffsCollection == null)
			{
				_readOnlyBuffsCollection = new ReadOnlyCollection<Buff>(buffs);
			}
			return _readOnlyBuffsCollection;
		}
	}

	public event Action<CharacterBuffManager, Buff> onAddBuff;

	public event Action<CharacterBuffManager, Buff> onRemoveBuff;

	private void Awake()
	{
		if (master == null)
		{
			master = GetComponent<CharacterMainControl>();
		}
	}

	public void AddBuff(Buff buffPrefab, CharacterMainControl fromWho, int overrideWeaponID = 0)
	{
		if (buffPrefab == null)
		{
			return;
		}
		Buff buff = buffs.Find((Buff e) => e.ID == buffPrefab.ID);
		if ((bool)buff)
		{
			buff.NotifyIncomingBuffWithSameID(buffPrefab);
			return;
		}
		Buff buff2 = UnityEngine.Object.Instantiate(buffPrefab);
		buff2.Setup(this);
		buff2.fromWho = fromWho;
		if (overrideWeaponID > 0)
		{
			buff2.fromWeaponID = overrideWeaponID;
		}
		buffs.Add(buff2);
		this.onAddBuff?.Invoke(this, buff2);
	}

	public void RemoveBuff(int buffID, bool removeOneLayer)
	{
		Buff buff = buffs.Find((Buff e) => e.ID == buffID);
		if (buff != null)
		{
			RemoveBuff(buff, removeOneLayer);
		}
	}

	public void RemoveBuffsByTag(Buff.BuffExclusiveTags buffTag, bool removeOneLayer)
	{
		if (buffTag == Buff.BuffExclusiveTags.NotExclusive)
		{
			return;
		}
		foreach (Buff item in buffs.FindAll((Buff e) => e.ExclusiveTag == buffTag))
		{
			if (item != null)
			{
				RemoveBuff(item, removeOneLayer);
			}
		}
	}

	public bool HasBuff(int buffID)
	{
		return buffs.Find((Buff e) => e.ID == buffID) != null;
	}

	public Buff GetBuffByTag(Buff.BuffExclusiveTags tag)
	{
		if (tag == Buff.BuffExclusiveTags.NotExclusive)
		{
			return null;
		}
		return buffs.Find((Buff e) => e.ExclusiveTag == tag);
	}

	public void RemoveBuff(Buff toRemove, bool oneLayer)
	{
		if (oneLayer && toRemove.CurrentLayers > 1)
		{
			toRemove.CurrentLayers--;
			if (toRemove.CurrentLayers >= 1)
			{
				return;
			}
		}
		if (buffs.Remove(toRemove))
		{
			this.onRemoveBuff?.Invoke(this, toRemove);
			UnityEngine.Object.Destroy(toRemove.gameObject);
		}
	}

	private void Update()
	{
		bool flag = false;
		foreach (Buff buff in buffs)
		{
			if (buff == null)
			{
				flag = true;
			}
			else if (buff.IsOutOfTime)
			{
				buff.NotifyOutOfTime();
				outOfTimeBuffsBuffer.Add(buff);
			}
			else
			{
				buff.NotifyUpdate();
			}
		}
		if (outOfTimeBuffsBuffer.Count > 0)
		{
			foreach (Buff item in outOfTimeBuffsBuffer)
			{
				if (item != null)
				{
					RemoveBuff(item, oneLayer: false);
				}
			}
			outOfTimeBuffsBuffer.Clear();
		}
		if (flag)
		{
			buffs.RemoveAll((Buff e) => e == null);
		}
	}
}
