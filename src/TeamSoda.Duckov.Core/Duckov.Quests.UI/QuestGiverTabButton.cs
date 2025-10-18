using UnityEngine;
using UnityEngine.UI;

namespace Duckov.Quests.UI;

public class QuestGiverTabButton : MonoBehaviour
{
	[SerializeField]
	private Button button;

	[SerializeField]
	private GameObject selectedIndicator;

	[SerializeField]
	private QuestStatus status;

	private QuestGiverTabs master;

	public QuestStatus Status => status;

	private bool Selected
	{
		get
		{
			if (master == null)
			{
				return false;
			}
			return master.GetSelection() == this;
		}
	}

	internal void Setup(QuestGiverTabs questGiverTabs)
	{
		master = questGiverTabs;
		Refresh();
	}

	private void Awake()
	{
		button.onClick.AddListener(OnClick);
	}

	private void OnClick()
	{
		if (!(master == null))
		{
			master.SetSelection(this);
		}
	}

	internal void Refresh()
	{
		selectedIndicator.SetActive(Selected);
	}
}
