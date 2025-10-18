using System;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.NoteIndexs;

[Serializable]
public class Note
{
	[SerializeField]
	public string key;

	[SerializeField]
	public Sprite image;

	[LocalizationKey("Default")]
	public string titleKey
	{
		get
		{
			return "Note_" + key + "_Title";
		}
		set
		{
		}
	}

	[LocalizationKey("Default")]
	public string contentKey
	{
		get
		{
			return "Note_" + key + "_Content";
		}
		set
		{
		}
	}

	public string Title => titleKey.ToPlainText();

	private Sprite previewSprite => image;

	public string Content => contentKey.ToPlainText();
}
