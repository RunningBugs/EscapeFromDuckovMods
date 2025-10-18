using System;
using System.Text;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.Utilities;

[Serializable]
public class CustomData
{
	private const bool ReturnDefaultIfTryingToGetOtherType = true;

	private const bool LogWarningWhenTryingToGetOtherType = true;

	private const bool AllowChangeTypeWithSet = false;

	[SerializeField]
	private string key;

	[SerializeField]
	private CustomDataType dataType;

	[SerializeField]
	private byte[] data = new byte[0];

	[SerializeField]
	private bool display;

	private byte[] Data
	{
		get
		{
			return data;
		}
		set
		{
			data = value;
			this.OnSetData?.Invoke(this);
		}
	}

	[SerializeField]
	private string displayNameKey => "Var_" + key;

	public bool Display
	{
		get
		{
			return display;
		}
		set
		{
			display = value;
		}
	}

	public string DisplayName => displayNameKey.ToPlainText();

	public string Key => key;

	public CustomDataType DataType => dataType;

	public event Action<CustomData> OnSetData;

	public byte[] GetRawCopied()
	{
		byte[] array = new byte[Data.Length];
		Data.CopyTo(array, 0);
		return array;
	}

	public void SetRaw(byte[] value)
	{
		Data = new byte[value.Length];
		value.CopyTo(Data, 0);
	}

	public float GetFloat()
	{
		if (dataType != CustomDataType.Float)
		{
			Debug.Log($"Trying to get Float, but custom data {key} is {dataType}");
			return 0f;
		}
		try
		{
			return BitConverter.ToSingle(Data, 0);
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
			return 0f;
		}
	}

	public void SetFloat(float value)
	{
		if (dataType != CustomDataType.Float)
		{
			Debug.LogWarning("Setting value in a different type! Allowed by CustomData.AllowChangeTypeWithSet");
		}
		else
		{
			Data = BitConverter.GetBytes(value);
		}
	}

	public int GetInt()
	{
		if (dataType != CustomDataType.Int)
		{
			Debug.Log($"Trying to get Int, but custom data {key} is {dataType}");
			return 0;
		}
		try
		{
			return BitConverter.ToInt32(Data, 0);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error: " + ex.Message);
			return 0;
		}
	}

	public void SetInt(int value)
	{
		if (dataType != CustomDataType.Int)
		{
			Debug.LogWarning("Setting value in a different type! Allowed by CustomData.AllowChangeTypeWithSet");
		}
		else
		{
			Data = BitConverter.GetBytes(value);
		}
	}

	public bool GetBool()
	{
		if (dataType != CustomDataType.Bool)
		{
			Debug.Log($"Trying to get Bool, but custom data {key} is {dataType}");
			return false;
		}
		try
		{
			return BitConverter.ToBoolean(Data, 0);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error: " + ex.Message);
			return false;
		}
	}

	public void SetBool(bool value)
	{
		if (dataType != CustomDataType.Bool)
		{
			Debug.LogWarning("Setting value in a different type! Allowed by CustomData.AllowChangeTypeWithSet");
		}
		else
		{
			Data = BitConverter.GetBytes(value);
		}
	}

	public string GetString()
	{
		if (dataType != CustomDataType.String)
		{
			Debug.Log($"Trying to get String, but custom data {key} is {dataType}");
			return string.Empty;
		}
		try
		{
			return Encoding.UTF8.GetString(Data, 0, Data.Length);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error: " + ex.Message);
			return "*INVALID_VALUE*";
		}
	}

	public void SetString(string value)
	{
		if (dataType != CustomDataType.String)
		{
			Debug.LogWarning("Setting value in a different type! Allowed by CustomData.AllowChangeTypeWithSet");
			return;
		}
		try
		{
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			Data = new byte[bytes.Length];
			bytes.CopyTo(Data, 0);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error: " + ex.Message);
		}
	}

	public string GetValueDisplayString(string format = "")
	{
		switch (dataType)
		{
		case CustomDataType.Raw:
			return "*BINARY DATA*";
		case CustomDataType.Float:
			return GetFloat().ToString(format);
		case CustomDataType.Int:
			return GetInt().ToString(format);
		case CustomDataType.Bool:
			if (GetBool())
			{
				return "+";
			}
			return "-";
		case CustomDataType.String:
			return GetString().ToPlainText();
		default:
			return "*INVALID*";
		}
	}

	public CustomData(string key, CustomDataType dataType, byte[] data)
	{
		this.key = key;
		this.dataType = dataType;
		Data = new byte[data.Length];
		data.CopyTo(Data, 0);
	}

	public CustomData(string key, float floatValue)
	{
		this.key = key;
		dataType = CustomDataType.Float;
		SetFloat(floatValue);
	}

	public CustomData(string key, int intValue)
	{
		this.key = key;
		dataType = CustomDataType.Int;
		SetInt(intValue);
	}

	public CustomData(string key, bool boolValue)
	{
		this.key = key;
		dataType = CustomDataType.Bool;
		SetBool(boolValue);
	}

	public CustomData(string key, string stringValue)
	{
		this.key = key;
		dataType = CustomDataType.String;
		SetString(stringValue);
	}

	public CustomData()
	{
	}

	public CustomData(CustomData copyFrom)
	{
		key = copyFrom.key;
		dataType = copyFrom.dataType;
		Data = copyFrom.GetRawCopied();
	}
}
