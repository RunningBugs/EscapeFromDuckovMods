using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.Quests.Tasks;

public class QuestTask_TaskEvent : Task
{
	[SerializeField]
	private string eventKey;

	[SerializeField]
	[LocalizationKey("Quests")]
	private string description;

	private bool finished;

	[SerializeField]
	private MapElementForTask mapElement;

	public string EventKey => eventKey;

	public override string Description => description.ToPlainText();

	private void OnTaskEvent(string _key)
	{
		if (_key == eventKey)
		{
			finished = true;
			SetMapElementVisable(visable: false);
			ReportStatusChanged();
		}
	}

	protected override void OnInit()
	{
		base.OnInit();
		TaskEvent.OnTaskEvent += OnTaskEvent;
		SetMapElementVisable(!IsFinished());
	}

	private void OnDisable()
	{
		TaskEvent.OnTaskEvent -= OnTaskEvent;
	}

	protected override bool CheckFinished()
	{
		return finished;
	}

	public override object GenerateSaveData()
	{
		return finished;
	}

	public override void SetupSaveData(object data)
	{
		if (data is bool flag)
		{
			finished = flag;
		}
	}

	private void SetMapElementVisable(bool visable)
	{
		if ((bool)mapElement && mapElement.enabled)
		{
			if (visable)
			{
				mapElement.name = base.Master.DisplayName;
			}
			mapElement.SetVisibility(visable);
		}
	}
}
