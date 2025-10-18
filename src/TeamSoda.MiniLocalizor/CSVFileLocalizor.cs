using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MiniExcelLibs;
using MiniLocalizor;
using UnityEngine;

public class CSVFileLocalizor : ILocalizationProvider
{
	private string path;

	private Dictionary<string, DataEntry> dic = new Dictionary<string, DataEntry>();

	private SystemLanguage _language;

	private const bool convertFromEscapes = true;

	public string Path => path;

	public SystemLanguage Language => _language;

	public CSVFileLocalizor(string path)
	{
		this.path = path;
		if (!Enum.TryParse<SystemLanguage>(System.IO.Path.GetFileNameWithoutExtension(path), out _language))
		{
			_language = SystemLanguage.Unknown;
		}
		BuildDictionary();
	}

	public CSVFileLocalizor(SystemLanguage language)
	{
		if (!Enum.TryParse<SystemLanguage>(System.IO.Path.GetFileNameWithoutExtension(path = System.IO.Path.Combine(Application.streamingAssetsPath, "Localization/" + language.ToString() + ".csv")), out _language))
		{
			_language = SystemLanguage.Unknown;
		}
		BuildDictionary();
	}

	public void BuildDictionary()
	{
		dic.Clear();
		if (!File.Exists(path))
		{
			Debug.LogWarning("本地化文件不存在 " + path + ", 将创建本地文件");
			File.Create(path);
		}
		try
		{
			using FileStream stream = File.OpenRead(path);
			foreach (DataEntry item in stream.Query<DataEntry>(null, ExcelType.CSV))
			{
				if (item != null && !string.IsNullOrEmpty(item.key))
				{
					dic[item.key] = item;
				}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
			Debug.LogError("读取文件时发生错误，请尝试关闭外部编辑软件（如Excel）再试。");
		}
	}

	public string Get(string key)
	{
		DataEntry entry = GetEntry(key);
		if (entry == null)
		{
			return null;
		}
		string value = entry.value;
		return ConvertFromEscapes(value);
	}

	private static string ConvertFromEscapes(string origin)
	{
		if (string.IsNullOrEmpty(origin))
		{
			return origin;
		}
		return Regex.Unescape(origin);
	}

	private static string ConvertToEscapes(string origin)
	{
		if (string.IsNullOrEmpty(origin))
		{
			return origin;
		}
		return Regex.Escape(origin);
	}

	public DataEntry GetEntry(string key)
	{
		if (!dic.TryGetValue(key, out var value))
		{
			return null;
		}
		return value;
	}

	public bool HasKey(string key)
	{
		return dic.ContainsKey(key);
	}
}
