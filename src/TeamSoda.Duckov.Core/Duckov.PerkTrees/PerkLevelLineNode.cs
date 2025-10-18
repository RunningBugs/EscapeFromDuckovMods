using UnityEngine;

namespace Duckov.PerkTrees;

public class PerkLevelLineNode : PerkRelationNodeBase
{
	public Vector2 cachedPosition;

	public string DisplayName => name;

	public override int maxInConnections => 0;

	public override int maxOutConnections => 0;
}
