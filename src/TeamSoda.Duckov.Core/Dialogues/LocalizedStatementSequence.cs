using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using SodaCraft.StringUtilities;
using UnityEngine;

namespace Dialogues;

public class LocalizedStatementSequence : DTNode
{
	public BBParameter<string> keyPrefix;

	public BBParameter<int> beginIndex;

	public BBParameter<int> endIndex;

	public BBParameter<string> format = new BBParameter<string>("{keyPrefix}_{index}");

	private int index;

	public override bool requireActorSelection => true;

	protected override Status OnExecute(Component agent, IBlackboard bb)
	{
		Begin();
		return Status.Running;
	}

	private void Begin()
	{
		index = beginIndex.value - 1;
		Next();
	}

	private void Next()
	{
		index++;
		if (index > endIndex.value)
		{
			base.status = Status.Success;
			base.DLGTree.Continue();
		}
		else
		{
			LocalizedStatement statement = new LocalizedStatement(format.value.Format(new
			{
				keyPrefix = keyPrefix.value,
				index = index
			}));
			DialogueTree.RequestSubtitles(new SubtitlesRequestInfo(base.finalActor, statement, OnStatementFinish));
		}
	}

	private void OnStatementFinish()
	{
		Next();
	}
}
