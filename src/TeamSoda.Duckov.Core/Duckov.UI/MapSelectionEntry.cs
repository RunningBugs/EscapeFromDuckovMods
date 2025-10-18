using Duckov.Economy;
using Duckov.Quests;
using Duckov.Scenes;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.UI;

public class MapSelectionEntry : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private MapSelectionView master;

	[SerializeField]
	private TextMeshProUGUI displayNameText;

	[SerializeField]
	private CostDisplay costDisplay;

	[SerializeField]
	private GameObject lockedIndicator;

	[SerializeField]
	private Condition[] conditions;

	[SerializeField]
	private Cost cost;

	[SerializeField]
	[SceneID]
	private string sceneID;

	[SerializeField]
	private int beaconIndex;

	[SerializeField]
	private Sprite fullScreenImage;

	public Cost Cost => cost;

	public bool ConditionsSatisfied
	{
		get
		{
			if (conditions == null)
			{
				return true;
			}
			return conditions.Satisfied();
		}
	}

	public string SceneID => sceneID;

	public int BeaconIndex => beaconIndex;

	public Sprite FullScreenImage => fullScreenImage;

	public void Setup(MapSelectionView master)
	{
		this.master = master;
		Refresh();
	}

	private void OnEnable()
	{
		Refresh();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (ConditionsSatisfied)
		{
			master.NotifyEntryClicked(this, eventData);
		}
	}

	private void Refresh()
	{
		SceneInfoEntry sceneInfo = SceneInfoCollection.GetSceneInfo(sceneID);
		displayNameText.text = sceneInfo.DisplayName;
		lockedIndicator.gameObject.SetActive(!ConditionsSatisfied);
		costDisplay.Setup(cost);
		costDisplay.gameObject.SetActive(!cost.IsFree);
	}
}
