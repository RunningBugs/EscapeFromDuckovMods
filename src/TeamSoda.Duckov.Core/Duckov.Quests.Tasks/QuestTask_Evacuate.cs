using Duckov.Scenes;
using Eflatun.SceneReference;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Duckov.Quests.Tasks;

public class QuestTask_Evacuate : Task
{
	[SerializeField]
	[SceneID]
	private string requireSceneID;

	[SerializeField]
	private bool finished;

	private SceneInfoEntry RequireSceneInfo => SceneInfoCollection.GetSceneInfo(requireSceneID);

	private SceneReference RequireScene => RequireSceneInfo?.SceneReference;

	private string descriptionFormatKey => "Task_Evacuate";

	private string DescriptionFormat => descriptionFormatKey.ToPlainText();

	private string TargetDisplayName
	{
		get
		{
			if (RequireScene != null && RequireScene.UnsafeReason == SceneReferenceUnsafeReason.None)
			{
				return RequireSceneInfo.DisplayName;
			}
			if (base.Master.RequireScene != null && base.Master.RequireScene.UnsafeReason == SceneReferenceUnsafeReason.None)
			{
				return base.Master.RequireSceneInfo.DisplayName;
			}
			return "Scene_Any".ToPlainText();
		}
	}

	public override string Description => DescriptionFormat.Format(new { TargetDisplayName });

	private void OnEnable()
	{
		LevelManager.OnEvacuated += OnEvacuated;
	}

	private void OnDisable()
	{
		LevelManager.OnEvacuated -= OnEvacuated;
	}

	private void OnEvacuated(EvacuationInfo info)
	{
		if (finished)
		{
			return;
		}
		if (RequireScene == null || RequireScene.UnsafeReason == SceneReferenceUnsafeReason.Empty)
		{
			if (base.Master.SceneRequirementSatisfied)
			{
				finished = true;
				ReportStatusChanged();
			}
		}
		else if (RequireScene.UnsafeReason == SceneReferenceUnsafeReason.None && RequireScene.LoadedScene.isLoaded)
		{
			finished = true;
			ReportStatusChanged();
		}
	}

	public override object GenerateSaveData()
	{
		return finished;
	}

	protected override bool CheckFinished()
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
}
