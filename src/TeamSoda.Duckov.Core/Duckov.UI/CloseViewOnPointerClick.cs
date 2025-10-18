using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Duckov.UI;

public class CloseViewOnPointerClick : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private const bool FunctionEnabled = false;

	[SerializeField]
	private View view;

	[SerializeField]
	private Graphic graphic;

	private void OnValidate()
	{
		if (view == null)
		{
			view = GetComponent<View>();
		}
		if (graphic == null)
		{
			graphic = GetComponent<Graphic>();
		}
	}

	private void Awake()
	{
		if (view == null)
		{
			view = GetComponent<View>();
		}
		if (graphic == null)
		{
			graphic = GetComponent<Graphic>();
		}
		ManagedUIElement.onOpen += OnViewOpen;
		ManagedUIElement.onClose += OnViewClose;
	}

	private void OnDestroy()
	{
		ManagedUIElement.onOpen -= OnViewOpen;
		ManagedUIElement.onClose -= OnViewClose;
	}

	private void OnViewClose(ManagedUIElement element)
	{
		if (!(element != view) && !(graphic == null))
		{
			graphic.enabled = false;
		}
	}

	private void OnViewOpen(ManagedUIElement element)
	{
		if (!(element != view) && !(graphic == null))
		{
			graphic.enabled = true;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
	}
}
