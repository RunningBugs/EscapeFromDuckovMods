using System;

namespace ItemStatsSystem;

public class MenuPathAttribute : Attribute
{
	public string path;

	public MenuPathAttribute(string path)
	{
		this.path = path;
	}
}
