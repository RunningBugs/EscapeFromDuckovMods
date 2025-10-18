using System;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.Tips;

[Serializable]
internal struct TipEntry
{
	[SerializeField]
	private string tipID;

	public string TipID => tipID;

	[LocalizationKey("Default")]
	public string DescriptionKey
	{
		get
		{
			return "Tips_" + tipID;
		}
		set
		{
		}
	}

	public string Description => DescriptionKey.ToPlainText();
}
