using Saves;
using UnityEngine;

public class SaveDataBoolProxy : MonoBehaviour
{
	public string key;

	public bool value;

	public void Save()
	{
		SavesSystem.Save(key, value);
		Debug.Log($"SetSaveData:{key} to {value}");
	}
}
