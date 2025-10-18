using Saves;
using SodaCraft.Localizations;
using UnityEngine;

namespace Duckov.Quests.Tasks;

public class QuestTask_CheckSaveData : Task
{
	[SerializeField]
	private string saveDataKey;

	[SerializeField]
	[LocalizationKey("Quests")]
	private string description;

	public string SaveDataKey => saveDataKey;

	private bool SaveDataTrue
	{
		get
		{
			return SavesSystem.Load<bool>(saveDataKey);
		}
		set
		{
			SavesSystem.Save(saveDataKey, value);
		}
	}

	public override string Description => description.ToPlainText();

	protected override void OnInit()
	{
		base.OnInit();
	}

	private void OnDisable()
	{
	}

	protected override bool CheckFinished()
	{
		return SaveDataTrue;
	}

	public override object GenerateSaveData()
	{
		return SaveDataTrue;
	}

	public override void SetupSaveData(object data)
	{
	}
}
