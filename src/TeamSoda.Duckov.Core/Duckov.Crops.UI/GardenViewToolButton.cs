using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.Crops.UI;

public class GardenViewToolButton : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private GardenView master;

	[SerializeField]
	private GardenView.ToolType tool;

	[SerializeField]
	private GameObject indicator;

	public void OnPointerClick(PointerEventData eventData)
	{
		master.SetTool(tool);
	}

	private void Awake()
	{
		master.onToolChanged += OnToolChanged;
	}

	private void Start()
	{
		Refresh();
	}

	private void Refresh()
	{
		indicator.SetActive(tool == master.Tool);
	}

	private void OnToolChanged()
	{
		Refresh();
	}
}
