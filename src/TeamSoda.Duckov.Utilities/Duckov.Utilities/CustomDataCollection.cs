using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Duckov.Utilities;

[Serializable]
public class CustomDataCollection : ICollection<CustomData>, IEnumerable<CustomData>, IEnumerable
{
	[SerializeField]
	private List<CustomData> entries = new List<CustomData>();

	private bool dirty = true;

	private Dictionary<int, CustomData> _dictionary;

	private Dictionary<int, CustomData> Dictionary
	{
		get
		{
			if (_dictionary == null || dirty)
			{
				RebuildDictionary();
			}
			return _dictionary;
		}
	}

	public int Count => entries.Count;

	public bool IsReadOnly => false;

	private void SetDirty()
	{
		dirty = true;
	}

	private void RebuildDictionary()
	{
		if (_dictionary == null)
		{
			_dictionary = new Dictionary<int, CustomData>();
		}
		_dictionary.Clear();
		foreach (CustomData entry in entries)
		{
			if (entry != null && !string.IsNullOrWhiteSpace(entry.Key))
			{
				_dictionary[entry.Key.GetHashCode()] = entry;
			}
		}
	}

	public CustomData GetEntry(int hash)
	{
		if (Dictionary.TryGetValue(hash, out var value))
		{
			return value;
		}
		return null;
	}

	public CustomData GetEntry(string key)
	{
		int hashCode = key.GetHashCode();
		return GetEntry(hashCode);
	}

	public void SetRaw(string key, CustomDataType type, byte[] bytes, bool createNewIfNotExist = true)
	{
		CustomData entry = GetEntry(key);
		if (entry == null)
		{
			if (createNewIfNotExist)
			{
				entry = new CustomData(key, type, bytes);
				Add(entry);
			}
			else
			{
				Debug.LogError("Data with key " + key + " doesn't exist.");
			}
		}
		else
		{
			entry.SetRaw(bytes);
		}
	}

	public void SetRaw(int hash, CustomDataType type, byte[] bytes)
	{
		CustomData entry = GetEntry(hash);
		if (entry == null)
		{
			Debug.LogError($"Data with hash {hash} doesn't exist.");
		}
		else
		{
			entry.SetRaw(bytes);
		}
	}

	public byte[] GetRawCopied(string key, byte[] defaultResult = null)
	{
		CustomData entry = GetEntry(key);
		if (entry == null)
		{
			return defaultResult;
		}
		return entry.GetRawCopied();
	}

	public byte[] GetRawCopied(int hash, byte[] defaultResult = null)
	{
		CustomData entry = GetEntry(hash);
		if (entry == null)
		{
			return defaultResult;
		}
		return entry.GetRawCopied();
	}

	public float GetFloat(string key, float defaultResult = 0f)
	{
		return GetEntry(key)?.GetFloat() ?? defaultResult;
	}

	public int GetInt(string key, int defaultResult = 0)
	{
		return GetEntry(key)?.GetInt() ?? defaultResult;
	}

	public bool GetBool(string key, bool defaultResult = false)
	{
		return GetEntry(key)?.GetBool() ?? defaultResult;
	}

	public string GetString(string key, string defaultResult = null)
	{
		CustomData entry = GetEntry(key);
		if (entry == null)
		{
			return defaultResult;
		}
		return entry.GetString();
	}

	public float GetFloat(int hash, float defaultResult = 0f)
	{
		return GetEntry(hash)?.GetFloat() ?? defaultResult;
	}

	public int GetInt(int hash, int defaultResult = 0)
	{
		return GetEntry(hash)?.GetInt() ?? defaultResult;
	}

	public bool GetBool(int hash, bool defaultResult = false)
	{
		return GetEntry(hash)?.GetBool() ?? defaultResult;
	}

	public string GetString(int hash, string defaultResult = null)
	{
		CustomData entry = GetEntry(hash);
		if (entry == null)
		{
			return defaultResult;
		}
		return entry.GetString();
	}

	public void SetFloat(string key, float value, bool createNewIfNotExist = true)
	{
		CustomData entry = GetEntry(key);
		if (entry == null)
		{
			if (createNewIfNotExist)
			{
				entry = new CustomData(key, value);
				Add(entry);
			}
			else
			{
				Debug.LogError("Data with key " + key + " doesn't exist.");
			}
		}
		else
		{
			entry.SetFloat(value);
		}
	}

	public void SetInt(string key, int value, bool createNewIfNotExist = true)
	{
		CustomData entry = GetEntry(key);
		if (entry == null)
		{
			if (createNewIfNotExist)
			{
				entry = new CustomData(key, value);
				Add(entry);
			}
			else
			{
				Debug.LogError("Data with key " + key + " doesn't exist.");
			}
		}
		else
		{
			entry.SetInt(value);
		}
	}

	public void SetBool(string key, bool value, bool createNewIfNotExist = true)
	{
		CustomData entry = GetEntry(key);
		if (entry == null)
		{
			if (createNewIfNotExist)
			{
				entry = new CustomData(key, value);
				Add(entry);
			}
			else
			{
				Debug.LogError("Data with key " + key + " doesn't exist.");
			}
		}
		else
		{
			entry.SetBool(value);
		}
	}

	public void SetString(string key, string value, bool createNewIfNotExist = true)
	{
		CustomData entry = GetEntry(key);
		if (entry == null)
		{
			if (createNewIfNotExist)
			{
				entry = new CustomData(key, value);
				Add(entry);
			}
			else
			{
				Debug.LogError("Data with key " + key + " doesn't exist.");
			}
		}
		else
		{
			entry.SetString(value);
		}
	}

	public void SetFloat(int hash, float value)
	{
		CustomData entry = GetEntry(hash);
		if (entry == null)
		{
			Debug.LogError($"Data with hash {hash} not found");
		}
		else
		{
			entry.SetFloat(value);
		}
	}

	public void SetInt(int hash, int value)
	{
		CustomData entry = GetEntry(hash);
		if (entry == null)
		{
			Debug.LogError($"Data with hash {hash} not found");
		}
		else
		{
			entry.SetInt(value);
		}
	}

	public void SetBool(int hash, bool value)
	{
		CustomData entry = GetEntry(hash);
		if (entry == null)
		{
			Debug.LogError($"Data with hash {hash} not found");
		}
		else
		{
			entry.SetBool(value);
		}
	}

	public void SetString(int hash, string value)
	{
		CustomData entry = GetEntry(hash);
		if (entry == null)
		{
			Debug.LogError($"Data with hash {hash} not found");
		}
		else
		{
			entry.SetString(value);
		}
	}

	public void Set(string key, float value, bool createNewIfNotExist = true)
	{
		SetFloat(key, value, createNewIfNotExist);
	}

	public void Set(string key, int value, bool createNewIfNotExist = true)
	{
		SetInt(key, value, createNewIfNotExist);
	}

	public void Set(string key, bool value, bool createNewIfNotExist = true)
	{
		SetBool(key, value, createNewIfNotExist);
	}

	public void Set(string key, string value, bool createNewIfNotExist = true)
	{
		SetString(key, value, createNewIfNotExist);
	}

	public void Set(int hash, float value)
	{
		SetFloat(hash, value);
	}

	public void Set(int hash, int value)
	{
		SetInt(hash, value);
	}

	public void Set(int hash, bool value)
	{
		SetBool(hash, value);
	}

	public void Set(int hash, string value)
	{
		SetString(hash, value);
	}

	public void Add(CustomData item)
	{
		entries.Add(item);
		SetDirty();
	}

	public void Clear()
	{
		entries.Clear();
		SetDirty();
	}

	public bool Contains(CustomData item)
	{
		return entries.Contains(item);
	}

	public void CopyTo(CustomData[] array, int arrayIndex)
	{
		entries.CopyTo(array, arrayIndex);
	}

	public bool Remove(CustomData item)
	{
		bool result = entries.Remove(item);
		SetDirty();
		return result;
	}

	public IEnumerator<CustomData> GetEnumerator()
	{
		return entries.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return entries.GetEnumerator();
	}
}
