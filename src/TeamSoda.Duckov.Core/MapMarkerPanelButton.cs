using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MapMarkerPanelButton : MonoBehaviour
{
	[SerializeField]
	private GameObject selectionIndicator;

	[SerializeField]
	private Image image;

	[SerializeField]
	private Button button;

	public Image Image => image;

	public void Setup(UnityAction action, bool selected)
	{
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(action);
		selectionIndicator.gameObject.SetActive(selected);
	}
}
