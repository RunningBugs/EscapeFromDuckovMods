using System;

namespace Duckov.Quests.UI;

[Flags]
public enum QuestStatus
{
	None = 0,
	Avaliable = 2,
	Active = 4,
	Finished = 8
}
