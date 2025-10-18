using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomFaceUISwitch : MonoBehaviour
{
	public TextLocalizor titleText;

	public TextMeshProUGUI nameText;

	public Button leftButton;

	public Button rightButton;

	public CustomFaceUI master;

	public CustomFacePartTypes type;

	private void Awake()
	{
		leftButton.onClick.AddListener(OnLeftClick);
		rightButton.onClick.AddListener(OnRightClick);
	}

	public void Init(CustomFaceUI _master, CustomFacePartTypes partType, string title)
	{
		master = _master;
		type = partType;
		titleText.Key = title;
	}

	private void OnLeftClick()
	{
		Switch(-1);
	}

	private void OnRightClick()
	{
		Switch(1);
	}

	public void SetName(string name)
	{
		nameText.text = name;
	}

	private void Switch(int direction)
	{
		string text = master.SwitchPart(type, direction);
		nameText.text = text;
	}
}
