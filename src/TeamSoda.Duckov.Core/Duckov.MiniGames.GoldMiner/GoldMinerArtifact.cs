using System;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.MiniGames.GoldMiner;

public class GoldMinerArtifact : MiniGameBehaviour
{
	[SerializeField]
	private string id;

	[SerializeField]
	private Sprite icon;

	[SerializeField]
	private bool allowMultiple;

	[SerializeField]
	private int basePrice;

	[SerializeField]
	private int quality;

	private GoldMiner master;

	public Action<GoldMinerArtifact> OnAttached;

	public Action<GoldMinerArtifact> OnDetached;

	[LocalizationKey("Default")]
	private string displayNameKey
	{
		get
		{
			return "GoldMiner_" + id;
		}
		set
		{
		}
	}

	[LocalizationKey("Default")]
	private string descriptionKey
	{
		get
		{
			return "GoldMiner_" + id + "_Desc";
		}
		set
		{
		}
	}

	public bool AllowMultiple => allowMultiple;

	public string DisplayName => displayNameKey.ToPlainText();

	public string Description => descriptionKey.ToPlainText();

	public int Quality => quality;

	public int BasePrice => basePrice;

	public string ID => id;

	public Sprite Icon => icon;

	public GoldMiner Master => master;

	public void Attach(GoldMiner master)
	{
		this.master = master;
		base.transform.SetParent(master.transform);
		OnAttached?.Invoke(this);
	}

	public void Detatch(GoldMiner master)
	{
		OnDetached?.Invoke(this);
		if (master != this.master)
		{
			Debug.LogError("Artifact is being notified detach by a different GoldMiner instance.", master.gameObject);
		}
		this.master = null;
	}

	private void OnDestroy()
	{
		Detatch(master);
	}
}
