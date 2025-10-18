using System;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public abstract class GoldMinerArtifactBehaviour : MiniGameBehaviour
{
	[SerializeField]
	protected GoldMinerArtifact master;

	protected GoldMinerRunData Run
	{
		get
		{
			if (master == null)
			{
				return null;
			}
			if (master.Master == null)
			{
				return null;
			}
			return master.Master.run;
		}
	}

	protected GoldMiner GoldMiner
	{
		get
		{
			if (master == null)
			{
				return null;
			}
			return master.Master;
		}
	}

	private void Awake()
	{
		if (!master)
		{
			master = GetComponent<GoldMinerArtifact>();
		}
		GoldMinerArtifact goldMinerArtifact = master;
		goldMinerArtifact.OnAttached = (Action<GoldMinerArtifact>)Delegate.Combine(goldMinerArtifact.OnAttached, new Action<GoldMinerArtifact>(OnAttached));
		GoldMinerArtifact goldMinerArtifact2 = master;
		goldMinerArtifact2.OnDetached = (Action<GoldMinerArtifact>)Delegate.Combine(goldMinerArtifact2.OnDetached, new Action<GoldMinerArtifact>(OnDetached));
	}

	protected virtual void OnAttached(GoldMinerArtifact artifact)
	{
	}

	protected virtual void OnDetached(GoldMinerArtifact artifact)
	{
	}
}
