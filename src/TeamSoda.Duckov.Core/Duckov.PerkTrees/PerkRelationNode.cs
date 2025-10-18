using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using UnityEngine;

namespace Duckov.PerkTrees;

public class PerkRelationNode : PerkRelationNodeBase
{
	public Perk relatedNode;

	public Vector2 cachedPosition;

	private bool dirty = true;

	internal bool isDuplicate;

	internal bool isInvalid;

	internal void SetDirty()
	{
		dirty = true;
	}

	public override void OnDestroy()
	{
		if (relatedNode == null)
		{
			return;
		}
		IEnumerable<Node> enumerable = base.graph.allNodes.Where((Node e) => (e as PerkRelationNode).relatedNode == relatedNode);
		if (enumerable.Count() > 2)
		{
			return;
		}
		foreach (Node item in enumerable)
		{
			if (item is PerkRelationNode perkRelationNode)
			{
				perkRelationNode.isDuplicate = false;
				perkRelationNode.SetDirty();
			}
		}
	}

	internal void NotifyIncomingStateChanged()
	{
		relatedNode.NotifyParentStateChanged();
	}
}
