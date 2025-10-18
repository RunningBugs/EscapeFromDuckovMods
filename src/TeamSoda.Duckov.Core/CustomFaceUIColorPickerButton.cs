using UnityEngine;
using UnityEngine.UI;

public class CustomFaceUIColorPickerButton : MonoBehaviour
{
	private CustomFaceUIColorPicker master;

	private Color color;

	public Button button;

	public Image selectedFrameImage;

	public Color Color => color;

	public void Init(CustomFaceUIColorPicker _master, Color _color)
	{
		master = _master;
		color = _color;
		ColorBlock colors = button.colors;
		colors.normalColor = color;
		colors.highlightedColor = color;
		colors.selectedColor = color;
		button.colors = colors;
	}

	private void Awake()
	{
		button.onClick.AddListener(OnClick);
	}

	private void OnClick()
	{
		master.SetColor(color);
	}

	public void SetSelection(bool selected)
	{
		selectedFrameImage.gameObject.SetActive(selected);
	}
}
