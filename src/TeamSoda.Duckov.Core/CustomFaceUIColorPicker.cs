using System;
using System.Collections.Generic;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomFaceUIColorPicker : MonoBehaviour
{
	public CustomFaceUI master;

	public CustomFaceUIColorPickerButton singleButton;

	public Transform buttonParent;

	public TextLocalizor titleText;

	public GimpPalette gimpPalette;

	public Image colorDisplay;

	public Button pickerToggleBtn;

	private List<CustomFaceUIColorPickerButton> buttons;

	private bool created;

	private Color currentColor;

	public IEnumerable<Color> colors
	{
		get
		{
			GimpPalette.Entry[] entries = gimpPalette.entries;
			for (int i = 0; i < entries.Length; i++)
			{
				GimpPalette.Entry entry = entries[i];
				yield return entry.color;
			}
		}
	}

	public Color CurrentColor => currentColor;

	public event Action<Color> OnSetColor;

	private void Awake()
	{
		pickerToggleBtn.onClick.AddListener(OnPickToggleBtnClicked);
	}

	private void OnPickToggleBtnClicked()
	{
		buttonParent.gameObject.SetActive(!buttonParent.gameObject.activeSelf);
	}

	public void Init(CustomFaceUI _master, string titleKey)
	{
		master = _master;
		titleText.Key = titleKey;
		if (buttons == null)
		{
			buttons = new List<CustomFaceUIColorPickerButton>();
		}
		if (!created)
		{
			foreach (Color color2 in colors)
			{
				Color color = new Color(color2.r, color2.g, color2.b, 1f);
				CustomFaceUIColorPickerButton customFaceUIColorPickerButton = UnityEngine.Object.Instantiate(singleButton, buttonParent);
				customFaceUIColorPickerButton.Init(this, color);
				customFaceUIColorPickerButton.transform.SetParent(buttonParent);
				buttons.Add(customFaceUIColorPickerButton);
			}
			singleButton.gameObject.SetActive(value: false);
			created = true;
		}
		UpdateSelection();
		buttonParent.gameObject.SetActive(value: false);
	}

	private void UpdateSelection()
	{
		if (buttons == null || buttons.Count < 1)
		{
			return;
		}
		foreach (CustomFaceUIColorPickerButton button in buttons)
		{
			button.SetSelection(button.Color.CompareRGB(currentColor));
		}
	}

	public void SetColor(Color _color)
	{
		currentColor = _color;
		master.SetDirty();
		UpdateSelection();
		currentColor.a = 1f;
		colorDisplay.color = currentColor;
		buttonParent.gameObject.SetActive(value: false);
	}
}
