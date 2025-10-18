using System;
using UnityEngine;

[Serializable]
public struct CharacterRandomPresetInfo
{
	public CharacterRandomPreset randomPreset;

	[Range(0f, 1f)]
	public float weight;
}
