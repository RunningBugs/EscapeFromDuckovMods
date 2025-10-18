using System;
using Eflatun.SceneReference;
using SodaCraft.Localizations;
using UnityEngine;

[Serializable]
public class SceneInfoEntry
{
	[SerializeField]
	private string id;

	[SerializeField]
	private SceneReference sceneReference;

	[LocalizationKey("Default")]
	[SerializeField]
	private string displayName;

	[LocalizationKey("Default")]
	[SerializeField]
	private string description;

	public int BuildIndex
	{
		get
		{
			if (sceneReference.UnsafeReason != SceneReferenceUnsafeReason.None)
			{
				return -1;
			}
			return sceneReference.BuildIndex;
		}
	}

	public string ID => id;

	public SceneReference SceneReference => sceneReference;

	public string Description => description.ToPlainText();

	public string DisplayName
	{
		get
		{
			if (string.IsNullOrEmpty(displayName))
			{
				return id;
			}
			return displayName.ToPlainText();
		}
	}

	public string DisplayNameRaw
	{
		get
		{
			if (string.IsNullOrEmpty(displayName))
			{
				return id;
			}
			return displayName;
		}
	}

	public bool IsLoaded
	{
		get
		{
			if (sceneReference == null)
			{
				return false;
			}
			if (sceneReference.UnsafeReason == SceneReferenceUnsafeReason.None)
			{
				return sceneReference.LoadedScene.isLoaded;
			}
			return false;
		}
	}

	public SceneInfoEntry()
	{
	}

	public SceneInfoEntry(string id, SceneReference sceneReference)
	{
		this.id = id;
		this.sceneReference = sceneReference;
	}
}
