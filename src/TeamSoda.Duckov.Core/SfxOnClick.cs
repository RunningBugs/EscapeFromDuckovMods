using Duckov;
using UnityEngine;
using UnityEngine.EventSystems;

public class SfxOnClick : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private string sfx;

	public void OnPointerClick(PointerEventData eventData)
	{
		AudioManager.Post(sfx);
	}
}
