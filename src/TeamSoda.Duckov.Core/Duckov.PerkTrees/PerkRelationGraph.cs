using System;
using System.Collections.Generic;
using NodeCanvas.Framework;
using ParadoxNotion;

namespace Duckov.PerkTrees;

public class PerkRelationGraph : Graph
{
	public override Type baseNodeType => typeof(PerkRelationNodeBase);

	public override bool requiresAgent => true;

	public override bool requiresPrimeNode => false;

	public override bool isTree => false;

	public override PlanarDirection flowDirection => PlanarDirection.Vertical;

	public override bool allowBlackboardOverrides => true;

	public override bool canAcceptVariableDrops => true;

	public PerkRelationNode GetRelatedNode(Perk perk)
	{
		return base.allNodes.Find(delegate(Node node)
		{
			if (node == null)
			{
				return false;
			}
			return node is PerkRelationNode perkRelationNode && perkRelationNode.relatedNode == perk;
		}) as PerkRelationNode;
	}

	public List<PerkRelationNode> GetIncomingNodes(PerkRelationNode skillTreeNode)
	{
		List<PerkRelationNode> list = new List<PerkRelationNode>();
		foreach (Connection inConnection in skillTreeNode.inConnections)
		{
			if (inConnection != null && inConnection.sourceNode is PerkRelationNode item)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<PerkRelationNode> GetOutgoingNodes(PerkRelationNode skillTreeNode)
	{
		List<PerkRelationNode> list = new List<PerkRelationNode>();
		foreach (Connection outConnection in skillTreeNode.outConnections)
		{
			if (outConnection != null && outConnection.targetNode is PerkRelationNode item)
			{
				list.Add(item);
			}
		}
		return list;
	}
}
