using Duckov.UI;
using UnityEngine;

public class ViewTabDisplayEntry : MonoBehaviour
{
	[SerializeField]
	private string viewTypeName;

	[SerializeField]
	private GameObject indicator;

	[SerializeField]
	private PunchReceiver punch;

	private void Awake()
	{
		ManagedUIElement.onOpen += OnViewOpen;
		ManagedUIElement.onClose += OnViewClose;
		HideIndicator();
	}

	private void OnDestroy()
	{
		ManagedUIElement.onOpen -= OnViewOpen;
		ManagedUIElement.onClose -= OnViewClose;
	}

	private void Start()
	{
		if (View.ActiveView != null && View.ActiveView.GetType().Name == viewTypeName)
		{
			ShowIndicator();
		}
	}

	private void OnViewClose(ManagedUIElement element)
	{
		if (element.GetType().Name == viewTypeName)
		{
			HideIndicator();
		}
	}

	private void OnViewOpen(ManagedUIElement element)
	{
		if (element.GetType().Name == viewTypeName)
		{
			ShowIndicator();
		}
	}

	private void ShowIndicator()
	{
		indicator.SetActive(value: true);
		punch.Punch();
	}

	private void HideIndicator()
	{
		indicator.SetActive(value: false);
	}
}
