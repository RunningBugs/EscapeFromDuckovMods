using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using UnityEngine;

namespace Dialogues;

public class LocalizedStatementNode : DTNode
{
	public BBParameter<string> key;

	public BBParameter<bool> useSequence;

	public BBParameter<int> sequenceIndex;

	public override bool requireActorSelection => true;

	private string Key
	{
		get
		{
			if (useSequence.value)
			{
				return $"{key.value}_{sequenceIndex.value}";
			}
			return key.value;
		}
	}

	private LocalizedStatement CreateStatement()
	{
		return new LocalizedStatement(Key);
	}

	protected override Status OnExecute(Component agent, IBlackboard bb)
	{
		LocalizedStatement statement = CreateStatement();
		DialogueTree.RequestSubtitles(new SubtitlesRequestInfo(base.finalActor, statement, OnStatementFinish));
		return Status.Running;
	}

	private void OnStatementFinish()
	{
		base.status = Status.Success;
		base.DLGTree.Continue();
	}
}
