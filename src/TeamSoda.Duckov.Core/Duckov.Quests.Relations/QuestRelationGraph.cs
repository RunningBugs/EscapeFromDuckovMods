using System;
using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion;
using UnityEngine;

namespace Duckov.Quests.Relations;

[CreateAssetMenu(menuName = "Quests/Relations")]
public class QuestRelationGraph : Graph
{
	public static int selectedQuestID = -1;

	public override Type baseNodeType => typeof(QuestRelationNodeBase);

	public override bool requiresAgent => false;

	public override bool requiresPrimeNode => false;

	public override bool isTree => false;

	public override PlanarDirection flowDirection => PlanarDirection.Vertical;

	public override bool allowBlackboardOverrides => true;

	public override bool canAcceptVariableDrops => true;

	public QuestRelationNode GetNode(int questID)
	{
		return base.allNodes.Find((Node node) => node is QuestRelationNode questRelationNode && questRelationNode.questID == questID) as QuestRelationNode;
	}

	public List<int> GetRequiredIDs(int targetID)
	{
		List<int> list = new List<int>();
		QuestRelationNode node = GetNode(targetID);
		if (node == null)
		{
			return list;
		}
		foreach (Connection inConnection in node.inConnections)
		{
			if (inConnection.sourceNode is QuestRelationNode { questID: var questID })
			{
				list.Add(questID);
			}
			else if (inConnection.sourceNode is QuestRelationProxyNode { questID: var questID2 })
			{
				list.Add(questID2);
			}
		}
		return list;
	}

	protected override void OnGraphValidate()
	{
		CheckDuplicates();
	}

	internal void CheckDuplicates()
	{
	}
}
