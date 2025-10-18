using System;
using NodeCanvas.Framework;
using ParadoxNotion;

namespace Duckov.Quests.Relations;

public class QuestRelationNodeBase : Node
{
	public override int maxInConnections => 64;

	public override int maxOutConnections => 64;

	public override Type outConnectionType => typeof(QuestRelationConnection);

	public override bool allowAsPrime => true;

	public override bool canSelfConnect => false;

	public override Alignment2x2 commentsAlignment => Alignment2x2.Default;

	public override Alignment2x2 iconAlignment => Alignment2x2.Default;
}
