using System;
using System.IO;
using UnityEngine;

namespace Duckov.Modding;

[Serializable]
public struct ModInfo
{
	public string path;

	public string name;

	public string displayName;

	public string description;

	public Texture2D preview;

	public bool dllFound;

	public bool isSteamItem;

	public ulong publishedFileId;

	public string dllPath => Path.Combine(path, name + ".dll");
}
