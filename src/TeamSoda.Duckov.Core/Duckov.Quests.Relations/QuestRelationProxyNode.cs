using Duckov.Utilities;

namespace Duckov.Quests.Relations;

public class QuestRelationProxyNode : QuestRelationNodeBase
{
	private static QuestCollection _questCollection;

	public int questID;

	public override int maxInConnections => 0;

	private static QuestCollection QuestCollection
	{
		get
		{
			if (_questCollection == null)
			{
				_questCollection = GameplayDataSettings.QuestCollection;
			}
			return _questCollection;
		}
	}

	private void SelectQuest()
	{
	}
}
