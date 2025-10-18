using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.UI;

public class DebugKontextMenuInvoker : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private KontextMenu kontextMenu;

	public void OnPointerClick(PointerEventData eventData)
	{
		Show(eventData.position);
	}

	public void Show(Vector2 point)
	{
		kontextMenu.InstanceShow(this, point, new KontextMenuDataEntry
		{
			text = "你好",
			action = delegate
			{
				Debug.Log("好");
			}
		}, new KontextMenuDataEntry
		{
			text = "你好2",
			action = delegate
			{
				Debug.Log("好好");
			}
		}, new KontextMenuDataEntry
		{
			text = "你好3",
			action = delegate
			{
				Debug.Log("好好好");
			}
		}, new KontextMenuDataEntry
		{
			text = "你好4",
			action = delegate
			{
				Debug.Log("好好好好");
			}
		}, new KontextMenuDataEntry
		{
			text = "你好5",
			action = delegate
			{
				Debug.Log("好好好好好");
			}
		});
	}
}
