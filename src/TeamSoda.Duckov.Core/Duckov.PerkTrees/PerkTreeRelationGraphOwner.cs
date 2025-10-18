using System.Collections.Generic;
using NodeCanvas.Framework;

namespace Duckov.PerkTrees;

public class PerkTreeRelationGraphOwner : GraphOwner<PerkRelationGraph>
{
	private PerkRelationGraph _relationGraph;

	public PerkRelationGraph RelationGraph
	{
		get
		{
			if (_relationGraph == null)
			{
				_relationGraph = graph as PerkRelationGraph;
			}
			return _relationGraph;
		}
	}

	public List<Perk> GetRequiredNodes(Perk node)
	{
		PerkRelationNode relatedNode = RelationGraph.GetRelatedNode(node);
		if (relatedNode == null)
		{
			return null;
		}
		List<PerkRelationNode> incomingNodes = RelationGraph.GetIncomingNodes(relatedNode);
		if (incomingNodes == null)
		{
			return null;
		}
		if (incomingNodes.Count < 1)
		{
			return null;
		}
		List<Perk> list = new List<Perk>();
		foreach (PerkRelationNode item in incomingNodes)
		{
			Perk relatedNode2 = item.relatedNode;
			if (!(relatedNode2 == null))
			{
				list.Add(relatedNode2);
			}
		}
		return list;
	}

	internal PerkRelationNode GetRelatedNode(Perk perk)
	{
		if (RelationGraph == null)
		{
			return null;
		}
		return RelationGraph.GetRelatedNode(perk);
	}
}
