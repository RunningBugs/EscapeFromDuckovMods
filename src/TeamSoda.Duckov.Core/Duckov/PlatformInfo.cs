using System;
using UnityEngine;

namespace Duckov;

public static class PlatformInfo
{
	private static Func<string> _getIDFunc;

	private static Func<string> _getDisplayNameFunc;

	public static Platform Platform
	{
		get
		{
			if (Application.isEditor)
			{
				return Platform.UnityEditor;
			}
			return GameMetaData.Instance.Platform;
		}
		set
		{
			GameMetaData.Instance.Platform = value;
		}
	}

	public static Func<string> GetIDFunc
	{
		get
		{
			return _getIDFunc;
		}
		set
		{
			_getIDFunc = value;
		}
	}

	public static Func<string> GetDisplayNameFunc
	{
		get
		{
			return _getDisplayNameFunc;
		}
		set
		{
			_getDisplayNameFunc = value;
		}
	}

	public static string GetID()
	{
		string text = null;
		if (GetIDFunc != null)
		{
			text = GetIDFunc();
		}
		if (text == null)
		{
			text = Environment.MachineName;
		}
		return text;
	}

	public static string GetDisplayName()
	{
		if (GetDisplayNameFunc != null)
		{
			return GetDisplayNameFunc();
		}
		return "UNKOWN";
	}
}
