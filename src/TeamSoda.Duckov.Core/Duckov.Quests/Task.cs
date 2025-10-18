using System;
using Saves;
using UnityEngine;

namespace Duckov.Quests;

[Serializable]
public abstract class Task : MonoBehaviour, ISaveDataProvider
{
	[SerializeField]
	private Quest master;

	[SerializeField]
	private int id;

	[SerializeField]
	private bool forceFinish;

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

	public virtual string Description => "未定义Task描述。";

	public virtual string[] ExtraDescriptsions => new string[0];

	public virtual Sprite Icon => null;

	public virtual bool Interactable => false;

	public virtual bool PossibleValidInteraction => false;

	public virtual bool NeedInspection => false;

	public virtual string InteractText => "交互";

	public event Action<Task> onStatusChanged;

	public virtual void Interact()
	{
		Debug.LogWarning($"{GetType()}可能未定义交互行为");
	}

	public bool IsFinished()
	{
		if (forceFinish)
		{
			return true;
		}
		return CheckFinished();
	}

	protected abstract bool CheckFinished();

	public abstract object GenerateSaveData();

	public abstract void SetupSaveData(object data);

	protected void ReportStatusChanged()
	{
		this.onStatusChanged?.Invoke(this);
		if (IsFinished())
		{
			Master?.NotifyTaskFinished(this);
		}
	}

	internal void Init()
	{
		if (IsFinished())
		{
			base.enabled = false;
		}
		else
		{
			OnInit();
		}
	}

	protected virtual void OnInit()
	{
	}

	internal void ForceFinish()
	{
		forceFinish = true;
		this.onStatusChanged?.Invoke(this);
	}
}
