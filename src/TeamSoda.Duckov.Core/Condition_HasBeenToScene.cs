using Duckov.Quests;
using Duckov.Scenes;

public class Condition_HasBeenToScene : Condition
{
	[SceneID]
	public string sceneID;

	public override bool Evaluate()
	{
		return MultiSceneCore.GetVisited(sceneID);
	}
}
