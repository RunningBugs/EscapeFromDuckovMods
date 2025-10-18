using System.Collections.Generic;
using Duckov.Utilities;
using NodeCanvas.Framework;

namespace Duckov.Quests.Relations;

public class QuestRelationNode : QuestRelationNodeBase
{
	public int questID;

	private static QuestCollection _questCollection;

	internal bool isDuplicate;

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

	public List<int> GetParents()
	{
		List<int> list = new List<int>();
		foreach (Connection inConnection in base.inConnections)
		{
			if (inConnection.sourceNode is QuestRelationNode questRelationNode)
			{
				list.Add(questRelationNode.questID);
			}
			else if (inConnection.sourceNode is QuestRelationProxyNode questRelationProxyNode)
			{
				list.Add(questRelationProxyNode.questID);
			}
		}
		return list;
	}

	public List<int> GetChildren()
	{
		List<int> list = new List<int>();
		foreach (Connection outConnection in base.outConnections)
		{
			if (outConnection.sourceNode is QuestRelationNode questRelationNode)
			{
				list.Add(questRelationNode.questID);
			}
		}
		return list;
	}
}
