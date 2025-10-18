using System;
using System.Collections.Generic;
using System.Linq;
using Duckov.Buildings;
using Duckov.Quests.UI;
using Duckov.Utilities;
using UnityEngine;

namespace Duckov.Quests;

public class QuestGiver : InteractableBase
{
	[SerializeField]
	private QuestGiverID questGiverID;

	private GameObject inspectionIndicator;

	private IEnumerable<Quest> _possibleQuests;

	private List<Quest> avaliableQuests = new List<Quest>();

	private IEnumerable<Quest> PossibleQuests
	{
		get
		{
			if (_possibleQuests == null && QuestManager.Instance != null)
			{
				_possibleQuests = QuestManager.Instance.GetAllQuestsByQuestGiverID(questGiverID);
			}
			return _possibleQuests;
		}
	}

	public QuestGiverID ID => questGiverID;

	protected override void Awake()
	{
		base.Awake();
		QuestManager.onQuestListsChanged += OnQuestListsChanged;
		BuildingManager.OnBuildingBuilt += OnBuildingBuilt;
		QuestManager.OnTaskFinishedEvent = (Action<Quest, Task>)Delegate.Combine(QuestManager.OnTaskFinishedEvent, new Action<Quest, Task>(OnTaskFinished));
		inspectionIndicator = UnityEngine.Object.Instantiate(GameplayDataSettings.Prefabs.QuestMarker);
		inspectionIndicator.transform.SetParent(base.transform);
		inspectionIndicator.transform.position = base.transform.TransformPoint(interactMarkerOffset + Vector3.up * 0.5f);
	}

	protected override void Start()
	{
		base.Start();
		RefreshInspectionIndicator();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		QuestManager.onQuestListsChanged -= OnQuestListsChanged;
		BuildingManager.OnBuildingBuilt -= OnBuildingBuilt;
		QuestManager.OnTaskFinishedEvent = (Action<Quest, Task>)Delegate.Remove(QuestManager.OnTaskFinishedEvent, new Action<Quest, Task>(OnTaskFinished));
	}

	private void OnTaskFinished(Quest quest, Task task)
	{
		RefreshInspectionIndicator();
	}

	private void OnBuildingBuilt(int buildingID)
	{
		RefreshInspectionIndicator();
	}

	private bool AnyQuestNeedsInspection()
	{
		return QuestManager.GetActiveQuestsFromGiver(questGiverID).Any((Quest e) => e != null && e.NeedInspection);
	}

	private bool AnyQuestAvaliable()
	{
		foreach (Quest possibleQuest in PossibleQuests)
		{
			if (QuestManager.IsQuestAvaliable(possibleQuest.ID))
			{
				return true;
			}
		}
		return false;
	}

	private void OnQuestListsChanged(QuestManager manager)
	{
		RefreshInspectionIndicator();
	}

	private void RefreshInspectionIndicator()
	{
		if ((bool)inspectionIndicator)
		{
			bool num = AnyQuestNeedsInspection();
			bool flag = AnyQuestAvaliable();
			bool active = num || flag;
			inspectionIndicator.gameObject.SetActive(active);
		}
	}

	public void ActivateQuest(Quest quest)
	{
		QuestManager.Instance.ActivateQuest(quest.ID, questGiverID);
	}

	internal List<Quest> GetAvaliableQuests()
	{
		List<Quest> list = new List<Quest>();
		foreach (Quest possibleQuest in PossibleQuests)
		{
			if (QuestManager.IsQuestAvaliable(possibleQuest.ID))
			{
				list.Add(possibleQuest);
			}
		}
		return list;
	}

	protected override void OnInteractStart(CharacterMainControl interactCharacter)
	{
		base.OnInteractStart(interactCharacter);
		QuestGiverView instance = QuestGiverView.Instance;
		if (instance == null)
		{
			StopInteract();
			return;
		}
		instance.Setup(this);
		instance.Open();
	}

	protected override void OnInteractStop()
	{
		base.OnInteractStop();
		if ((bool)QuestGiverView.Instance && QuestGiverView.Instance.open)
		{
			QuestGiverView.Instance?.Close();
		}
	}

	protected override void OnUpdate(CharacterMainControl _interactCharacter, float deltaTime)
	{
		base.OnUpdate(_interactCharacter, deltaTime);
		if (!QuestGiverView.Instance || !QuestGiverView.Instance.open)
		{
			StopInteract();
		}
	}
}
