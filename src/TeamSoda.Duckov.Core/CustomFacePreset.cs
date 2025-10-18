using System.Text.RegularExpressions;
using UnityEngine;

public class CustomFacePreset : ScriptableObject
{
	public CustomFaceSettingData settings;

	private void CopyJsonToClipBoard()
	{
		GUIUtility.systemCopyBuffer = DataToJson();
	}

	private void PastyFromJsonData()
	{
		if (CustomFaceSettingData.JsonToData(GUIUtility.systemCopyBuffer, out var data))
		{
			settings = data;
		}
	}

	private string DataToJson()
	{
		return Regex.Replace(JsonUtility.ToJson(settings, prettyPrint: false), "\\d+\\.\\d+", (Match match) => float.Parse(match.Value).ToString("0.###"));
	}
}
