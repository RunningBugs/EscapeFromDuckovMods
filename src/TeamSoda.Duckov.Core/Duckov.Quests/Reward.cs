using System;
using Saves;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Duckov.Quests;

public abstract class Reward : MonoBehaviour, ISelfValidator, ISaveDataProvider
{
	[SerializeField]
	private int id;

	[SerializeField]
	private Quest master;

	public int ID
	{
		get
		{
			return id;
		}
		internal set
		{
			id = value;
		}
	}

	public bool Claimable => master.Complete;

	public virtual Sprite Icon => null;

	public virtual string Description => "未定义奖励描述";

	public abstract bool Claimed { get; }

	public virtual bool Claiming { get; }

	public virtual bool AutoClaim => false;

	public Quest Master
	{
		get
		{
			return master;
		}
		internal set
		{
			master = value;
		}
	}

	public static event Action<Reward> OnRewardClaimed;

	internal event Action onStatusChanged;

	public void Claim()
	{
		if (Claimable && !Claimed)
		{
			OnClaim();
			Master.NotifyRewardClaimed(this);
			Reward.OnRewardClaimed?.Invoke(this);
		}
	}

	public abstract void OnClaim();

	public virtual void Validate(SelfValidationResult result)
	{
		if (master == null)
		{
			result.AddWarning("Reward需要master(Quest)。").WithFix("设为父物体中的Quest。", delegate
			{
				master = GetComponent<Quest>();
				if (master == null)
				{
					master = GetComponentInParent<Quest>();
				}
			});
		}
		if (!(master != null))
		{
			return;
		}
		if (base.transform != master.transform && !base.transform.IsChildOf(master.transform))
		{
			result.AddError("Task需要存在于master子物体中。").WithFix("设为master子物体", delegate
			{
				base.transform.SetParent(master.transform);
			});
		}
		if (!master.rewards.Contains(this))
		{
			result.AddError("Master的Task列表中不包含本物体。").WithFix("将本物体添加至master的Task列表中", delegate
			{
				master.rewards.Add(this);
			});
		}
	}

	public abstract object GenerateSaveData();

	public abstract void SetupSaveData(object data);

	private void Awake()
	{
		Master.onStatusChanged += OnMasterStatusChanged;
	}

	private void OnDestroy()
	{
		Master.onStatusChanged -= OnMasterStatusChanged;
	}

	public void OnMasterStatusChanged(Quest quest)
	{
		this.onStatusChanged?.Invoke();
	}

	protected void ReportStatusChanged()
	{
		this.onStatusChanged?.Invoke();
	}

	public virtual void NotifyReload(Quest questInstance)
	{
	}
}
