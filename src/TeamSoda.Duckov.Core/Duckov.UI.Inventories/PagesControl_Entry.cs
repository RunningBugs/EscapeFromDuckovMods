using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI.Inventories;

public class PagesControl_Entry : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private GameObject selectedIndicator;

	[SerializeField]
	private Button button;

	private PagesControl master;

	private int index;

	private bool selected;

	private void Awake()
	{
		button.onClick.AddListener(OnButtonClicked);
	}

	private void OnButtonClicked()
	{
		master.NotifySelect(index);
	}

	internal void Setup(PagesControl master, int i, bool selected)
	{
		this.master = master;
		index = i;
		this.selected = selected;
		text.text = $"{index}";
		selectedIndicator.SetActive(this.selected);
	}
}
