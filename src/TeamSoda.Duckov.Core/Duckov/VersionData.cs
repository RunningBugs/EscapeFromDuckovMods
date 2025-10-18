using System;

namespace Duckov;

[Serializable]
public struct VersionData
{
	public int mainVersion;

	public int subVersion;

	public int buildVersion;

	public string suffix;

	public override string ToString()
	{
		return $"{mainVersion}.{subVersion}.{buildVersion}{suffix}";
	}
}
