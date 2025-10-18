using Duckov.Scenes;
using SodaCraft.Localizations;
using SodaCraft.StringUtilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Duckov.Quests.Tasks;

public class QuestTask_ReachLocation : Task
{
	[SerializeField]
	private MultiSceneLocation location;

	[SerializeField]
	private float radius = 1f;

	[SerializeField]
	private bool finished;

	[SerializeField]
	private Transform target;

	[SerializeField]
	private MapElementForTask mapElement;

	public string descriptionFormatkey => "Task_ReachLocation";

	public string DescriptionFormat => descriptionFormatkey.ToPlainText();

	public string TargetLocationDisplayName => location.GetDisplayName();

	public override string Description => DescriptionFormat.Format(new { TargetLocationDisplayName });

	private void OnEnable()
	{
		SceneLoader.onFinishedLoadingScene += OnFinishedLoadingScene;
		MultiSceneCore.OnSubSceneLoaded += OnSubSceneLoaded;
	}

	private void Start()
	{
		CacheLocation();
	}

	private void OnDisable()
	{
		SceneLoader.onFinishedLoadingScene -= OnFinishedLoadingScene;
		MultiSceneCore.OnSubSceneLoaded -= OnSubSceneLoaded;
	}

	protected override void OnInit()
	{
		base.OnInit();
		if (!IsFinished())
		{
			SetMapElementVisable(visable: true);
		}
	}

	private void OnFinishedLoadingScene(SceneLoadingContext context)
	{
		CacheLocation();
	}

	private void OnSubSceneLoaded(MultiSceneCore core, Scene scene)
	{
		LevelManager.LevelInitializingComment = "Reach location task caching";
		CacheLocation();
	}

	private void CacheLocation()
	{
		target = location.GetLocationTransform();
	}

	private void Update()
	{
		if (finished || target == null)
		{
			return;
		}
		CharacterMainControl main = CharacterMainControl.Main;
		if (!(main == null))
		{
			if ((main.transform.position - target.position).magnitude <= radius)
			{
				finished = true;
				SetMapElementVisable(visable: false);
			}
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

	private void SetMapElementVisable(bool visable)
	{
		if ((bool)mapElement)
		{
			if (visable)
			{
				mapElement.locations.Clear();
				mapElement.locations.Add(location);
				mapElement.range = radius;
				mapElement.name = base.Master.DisplayName;
			}
			mapElement.SetVisibility(visable);
		}
	}
}
