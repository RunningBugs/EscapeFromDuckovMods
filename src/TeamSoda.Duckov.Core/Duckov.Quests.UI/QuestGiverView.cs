using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Duckov.UI;
using Duckov.UI.Animations;
using Duckov.Utilities;
using SodaCraft.Localizations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.Quests.UI;

public class QuestGiverView : View, ISingleSelectionMenu<QuestEntry>
{
	[SerializeField]
	private FadeGroup fadeGroup;

	[SerializeField]
	private RectTransform questEntriesParent;

	[SerializeField]
	private QuestCompletePanel questCompletePanel;

	[SerializeField]
	private QuestGiverTabs tabs;

	[SerializeField]
	private QuestEntry entryPrefab;

	[SerializeField]
	private GameObject entryPlaceHolder;

	[SerializeField]
	private QuestViewDetails details;

	[SerializeField]
	private Button btn_Interact;

	[SerializeField]
	private TextMeshProUGUI btnText;

	[SerializeField]
	private Image btnImage;

	[SerializeField]
	private string btnText_AcceptQuest = "接受任务";

	[SerializeField]
	private string btnText_CompleteQuest = "完成任务";

	[SerializeField]
	private Color interactableBtnImageColor = Color.green;

	[SerializeField]
	private Color uninteractableBtnImageColor = Color.gray;

	[SerializeField]
	private GameObject uninspectedAvaliableRedDot;

	[SerializeField]
	private GameObject activeRedDot;

	private string sfx_AcceptQuest = "UI/mission_accept";

	private string sfx_CompleteQuest = "UI/mission_large";

	private PrefabPool<QuestEntry> _entryPool;

	private List<QuestEntry> activeEntries = new List<QuestEntry>();

	private QuestGiver target;

	private QuestEntry selectedQuestEntry;

	private UniTask completeUITask;

	private bool btnAcceptQuest;

	private bool btnCompleteQuest;

	public static QuestGiverView Instance => View.GetViewInstance<QuestGiverView>();

	public string BtnText_CompleteQuest => btnText_CompleteQuest.ToPlainText();

	public string BtnText_AcceptQuest => btnText_AcceptQuest.ToPlainText();

	private PrefabPool<QuestEntry> EntryPool
	{
		get
		{
			if (_entryPool == null)
			{
				_entryPool = new PrefabPool<QuestEntry>(entryPrefab, questEntriesParent, delegate(QuestEntry e)
				{
					activeEntries.Add(e);
				}, delegate(QuestEntry e)
				{
					activeEntries.Remove(e);
				});
			}
			return _entryPool;
		}
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		fadeGroup.Show();
		RefreshList();
		RefreshDetails();
		QuestManager.onQuestListsChanged += OnQuestListChanged;
		Quest.onQuestStatusChanged += OnQuestStatusChanged;
	}

	protected override void OnClose()
	{
		base.OnClose();
		fadeGroup.Hide();
		QuestManager.onQuestListsChanged -= OnQuestListChanged;
		Quest.onQuestStatusChanged -= OnQuestStatusChanged;
	}

	private void OnDisable()
	{
		if (details != null)
		{
			details.Setup(null);
		}
	}

	private void OnQuestStatusChanged(Quest quest)
	{
		if (quest == selectedQuestEntry?.Target)
		{
			RefreshDetails();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		tabs.onSelectionChanged += OnTabChanged;
		btn_Interact.onClick.AddListener(OnInteractButtonClicked);
	}

	private void OnInteractButtonClicked()
	{
		if (btnAcceptQuest)
		{
			Quest quest = details.Target;
			if (quest != null && QuestManager.IsQuestAvaliable(quest.ID))
			{
				QuestManager.Instance.ActivateQuest(quest.ID, target.ID);
				AudioManager.Post(sfx_AcceptQuest);
			}
		}
		else if (btnCompleteQuest)
		{
			Quest quest2 = details.Target;
			if (!(quest2 == null) && quest2.TryComplete())
			{
				ShowCompleteUI(quest2);
				AudioManager.Post(sfx_CompleteQuest);
			}
		}
	}

	private void ShowCompleteUI(Quest quest)
	{
		completeUITask = questCompletePanel.Show(quest);
	}

	private void OnTabChanged(QuestGiverTabs tabs)
	{
		RefreshList();
		RefreshDetails();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		QuestManager.onQuestListsChanged -= OnQuestListChanged;
	}

	private void OnQuestListChanged(QuestManager manager)
	{
		RefreshList();
		SetSelection(null);
		RefreshDetails();
	}

	public void Setup(QuestGiver target)
	{
		this.target = target;
		RefreshList();
	}

	private void RefreshList()
	{
		Quest keepQuest = selectedQuestEntry?.Target;
		selectedQuestEntry = null;
		EntryPool.ReleaseAll();
		List<Quest> questsToShow = GetQuestsToShow();
		bool flag = questsToShow.Count > 0;
		entryPlaceHolder.SetActive(!flag);
		RefreshRedDots();
		if (!flag)
		{
			return;
		}
		foreach (Quest item in questsToShow)
		{
			QuestEntry questEntry = EntryPool.Get(questEntriesParent);
			questEntry.transform.SetAsLastSibling();
			questEntry.SetMenu(this);
			questEntry.Setup(item);
		}
		QuestEntry questEntry2 = activeEntries.Find((QuestEntry e) => e.Target == keepQuest);
		if (questEntry2 != null)
		{
			SetSelection(questEntry2);
		}
		else
		{
			SetSelection(null);
		}
	}

	private void RefreshRedDots()
	{
		uninspectedAvaliableRedDot.SetActive(AnyUninspectedAvaliableQuest());
		activeRedDot.SetActive(AnyUninspectedActiveQuest());
	}

	private bool AnyUninspectedActiveQuest()
	{
		if (target == null)
		{
			return false;
		}
		return QuestManager.AnyActiveQuestNeedsInspection(target.ID);
	}

	private bool AnyUninspectedAvaliableQuest()
	{
		if (target == null)
		{
			return false;
		}
		return target.GetAvaliableQuests().Any((Quest e) => e != null && e.NeedInspection);
	}

	private List<Quest> GetQuestsToShow()
	{
		List<Quest> list = new List<Quest>();
		if (target == null)
		{
			return list;
		}
		switch (tabs.GetStatus())
		{
		case QuestStatus.None:
			return list;
		case QuestStatus.Avaliable:
			list.AddRange(target.GetAvaliableQuests());
			break;
		case QuestStatus.Active:
			list.AddRange(QuestManager.GetActiveQuestsFromGiver(target.ID));
			break;
		case QuestStatus.Finished:
			list.AddRange(QuestManager.GetHistoryQuestsFromGiver(target.ID));
			break;
		}
		return list;
	}

	private void RefreshDetails()
	{
		Quest quest = selectedQuestEntry?.Target;
		details.Setup(quest);
		RefreshInteractButton();
		bool interactable = (bool)quest && (QuestManager.IsQuestActive(quest) || quest.Complete);
		details.Interactable = interactable;
		details.Refresh();
	}

	private void RefreshInteractButton()
	{
		btnAcceptQuest = false;
		btnCompleteQuest = false;
		Quest quest = selectedQuestEntry?.Target;
		if (quest == null)
		{
			btn_Interact.gameObject.SetActive(value: false);
			return;
		}
		QuestStatus status = tabs.GetStatus();
		bool active = false;
		switch (status)
		{
		case QuestStatus.Avaliable:
			active = true;
			btn_Interact.interactable = true;
			btnImage.color = interactableBtnImageColor;
			btnText.text = BtnText_AcceptQuest;
			btnAcceptQuest = true;
			break;
		case QuestStatus.Active:
		{
			active = true;
			bool flag = quest.AreTasksFinished();
			btn_Interact.interactable = flag;
			btnImage.color = (flag ? interactableBtnImageColor : uninteractableBtnImageColor);
			btnText.text = BtnText_CompleteQuest;
			btnCompleteQuest = true;
			break;
		}
		}
		btn_Interact.gameObject.SetActive(active);
	}

	public QuestEntry GetSelection()
	{
		return selectedQuestEntry;
	}

	public bool SetSelection(QuestEntry selection)
	{
		selectedQuestEntry = selection;
		if (selection != null)
		{
			QuestManager.SetEverInspected(selection.Target.ID);
		}
		RefreshDetails();
		RefreshEntries();
		RefreshRedDots();
		return true;
	}

	private void RefreshEntries()
	{
		foreach (QuestEntry activeEntry in activeEntries)
		{
			activeEntry.NotifyRefresh();
		}
	}

	internal override void TryQuit()
	{
		if (questCompletePanel.isActiveAndEnabled)
		{
			questCompletePanel.Skip();
		}
		else
		{
			Close();
		}
	}
}
