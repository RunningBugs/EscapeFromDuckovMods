using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class CopyTextOnClick : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private string content => Path.Combine(Application.persistentDataPath, "Saves");

	public void OnPointerClick(PointerEventData eventData)
	{
		GUIUtility.systemCopyBuffer = content;
	}
}
