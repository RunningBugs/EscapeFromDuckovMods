using System;
using NodeCanvas.Framework;
using ParadoxNotion;

namespace Duckov.PerkTrees;

public class PerkRelationNodeBase : Node
{
	public override int maxInConnections => 16;

	public override int maxOutConnections => 16;

	public override Type outConnectionType => typeof(PerkRelationConnection);

	public override bool allowAsPrime => true;

	public override bool canSelfConnect => false;

	public override Alignment2x2 commentsAlignment => Alignment2x2.Default;

	public override Alignment2x2 iconAlignment => Alignment2x2.Default;
}
