using UnityEngine;
using UnityEngine.UI;

namespace Duckov.Quests.UI;

public class QuestViewFlagButton : MonoBehaviour
{
	[SerializeField]
	private QuestView master;

	[SerializeField]
	private Button button;

	[SerializeField]
	private QuestView.ShowContent content;

	[SerializeField]
	private GameObject selectionIndicator;

	private void Awake()
	{
		button.onClick.AddListener(OnButtonClicked);
		master.onShowingContentChanged += OnMasterShowingContentChanged;
		Refresh();
	}

	private void OnButtonClicked()
	{
		master.SetShowingContent(content);
	}

	private void OnMasterShowingContentChanged(QuestView view, QuestView.ShowContent content)
	{
		Refresh();
	}

	private void Refresh()
	{
		bool active = master.ShowingContentType == content;
		selectionIndicator.SetActive(active);
	}
}
