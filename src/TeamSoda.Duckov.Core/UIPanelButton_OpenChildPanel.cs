using UnityEngine;
using UnityEngine.EventSystems;

public class UIPanelButton_OpenChildPanel : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private UIPanel master;

	[SerializeField]
	private UIPanel target;

	private void Awake()
	{
		if (master == null)
		{
			master = GetComponentInParent<UIPanel>();
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		master?.OpenChild(target);
		eventData.Use();
	}
}
