using System.Collections.Generic;
using Duckov.Utilities;
using UnityEngine;

public class CustomFaceSaveLoad : MonoBehaviour
{
	public CustomFaceInstance instance;

	public CustomFaceLoadSaveButton buttonPfb;

	public Transform slotButtonParent;

	private List<CustomFaceLoadSaveButton> slotButtons;

	private int currentSlot;

	private CustomFaceData faceData => GameplayDataSettings.CustomFaceData;

	private void Awake()
	{
		slotButtons = new List<CustomFaceLoadSaveButton>();
		for (int i = 0; i < 5; i++)
		{
			CustomFaceLoadSaveButton item = CreateAButton(i, i.ToString(), slotButtonParent);
			slotButtons.Add(item);
		}
		buttonPfb.gameObject.SetActive(value: false);
		SetSlotAndLoad(0);
	}

	private CustomFaceLoadSaveButton CreateAButton(int index, string name, Transform parent)
	{
		CustomFaceLoadSaveButton customFaceLoadSaveButton = Object.Instantiate(buttonPfb);
		customFaceLoadSaveButton.Init(this, index, name);
		customFaceLoadSaveButton.transform.SetParent(parent, worldPositionStays: false);
		return customFaceLoadSaveButton;
	}

	public void SetSlotAndLoad(int slot)
	{
		currentSlot = slot;
		UpdateSelection();
		LoadData(slot);
	}

	private void UpdateSelection()
	{
		foreach (CustomFaceLoadSaveButton slotButton in slotButtons)
		{
			slotButton.SetSelection(slotButton.index == currentSlot);
		}
	}

	private void LoadData(int slot)
	{
		if (!ES3.KeyExists($"CustomFaceData_{slot}"))
		{
			LoadDefault();
			return;
		}
		CustomFaceSettingData saveData = (CustomFaceSettingData)ES3.Load($"CustomFaceData_{slot}");
		instance.LoadFromData(saveData);
	}

	public void LoadDefault()
	{
		CustomFaceSettingData settings = faceData.DefaultPreset.settings;
		instance.LoadFromData(settings);
	}

	public void SaveDataToCurrentSlot()
	{
		SaveData(currentSlot);
	}

	private void SaveData(int slot)
	{
		CustomFaceSettingData value = instance.ConvertToSaveData();
		ES3.Save($"CustomFaceData_{slot}", value);
	}
}
