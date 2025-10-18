using System;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public struct CustomFaceSettingData
{
	[HideInInspector]
	public bool savedSetting;

	public CustomFaceHeadSetting headSetting;

	public int hairID;

	public CustomFacePartInfo hairInfo;

	public int eyeID;

	public CustomFacePartInfo eyeInfo;

	public int eyebrowID;

	public CustomFacePartInfo eyebrowInfo;

	public int mouthID;

	public CustomFacePartInfo mouthInfo;

	public int tailID;

	public CustomFacePartInfo tailInfo;

	public int footID;

	public CustomFacePartInfo footInfo;

	public int wingID;

	public CustomFacePartInfo wingInfo;

	public string DataToJson()
	{
		return Regex.Replace(JsonUtility.ToJson(this, prettyPrint: false), "\\d+\\.\\d+", (Match match) => float.Parse(match.Value).ToString("0.###"));
	}

	public static bool JsonToData(string jsonData, out CustomFaceSettingData data)
	{
		try
		{
			data = JsonUtility.FromJson<CustomFaceSettingData>(jsonData);
		}
		catch (Exception)
		{
			Debug.LogError("捏脸参数违法");
			data = default(CustomFaceSettingData);
			return false;
		}
		return true;
	}
}
