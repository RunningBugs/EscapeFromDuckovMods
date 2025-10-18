using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftViewFilterBtnEntry : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private GameObject selectedIndicator;

	private CraftView.FilterInfo info;

	private CraftView master;

	private int index;

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!(master == null))
		{
			master.SetFilter(index);
		}
	}

	public void Setup(CraftView master, CraftView.FilterInfo filterInfo, int index, bool selected)
	{
		this.master = master;
		info = filterInfo;
		this.index = index;
		icon.sprite = filterInfo.icon;
		displayNameText.text = filterInfo.displayNameKey.ToPlainText();
		selectedIndicator.SetActive(selected);
	}
}
