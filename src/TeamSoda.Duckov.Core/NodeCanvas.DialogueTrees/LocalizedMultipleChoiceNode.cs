using System;
using System.Collections.Generic;
using Dialogues;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace NodeCanvas.DialogueTrees;

[ParadoxNotion.Design.Icon("List", false, "")]
[Name("Multiple Choice Localized", 0)]
[Category("Branch")]
[Description("Prompt a Dialogue Multiple Choice. A choice will be available if the choice condition(s) are true or there is no choice conditions. The Actor selected is used for the condition checks and will also Say the selection if the option is checked.")]
[Color("b3ff7f")]
public class LocalizedMultipleChoiceNode : DTNode
{
	[Serializable]
	public class Choice
	{
		public bool isUnfolded = true;

		public LocalizedStatement statement;

		public ConditionTask condition;

		public Choice()
		{
		}

		public Choice(LocalizedStatement statement)
		{
			this.statement = statement;
		}
	}

	[SliderField(0f, 10f)]
	public float availableTime;

	public bool saySelection;

	[SerializeField]
	[AutoSortWithChildrenConnections]
	private List<Choice> availableChoices = new List<Choice>();

	public override int maxOutConnections => availableChoices.Count;

	public override bool requireActorSelection => true;

	protected override Status OnExecute(Component agent, IBlackboard bb)
	{
		if (base.outConnections.Count == 0)
		{
			return Error("There are no connections to the Multiple Choice Node!");
		}
		Dictionary<IStatement, int> dictionary = new Dictionary<IStatement, int>();
		for (int i = 0; i < availableChoices.Count; i++)
		{
			ConditionTask condition = availableChoices[i].condition;
			if (condition == null || condition.CheckOnce(base.finalActor.transform, bb))
			{
				LocalizedStatement statement = availableChoices[i].statement;
				dictionary[statement] = i;
			}
		}
		if (dictionary.Count == 0)
		{
			base.DLGTree.Stop(success: false);
			return Status.Failure;
		}
		DialogueTree.RequestMultipleChoices(new MultipleChoiceRequestInfo(base.finalActor, dictionary, availableTime, OnOptionSelected)
		{
			showLastStatement = true
		});
		return Status.Running;
	}

	private void OnOptionSelected(int index)
	{
		base.status = Status.Success;
		Action action = delegate
		{
			base.DLGTree.Continue(index);
		};
		if (saySelection)
		{
			LocalizedStatement statement = availableChoices[index].statement;
			DialogueTree.RequestSubtitles(new SubtitlesRequestInfo(base.finalActor, statement, action));
		}
		else
		{
			action();
		}
	}
}
