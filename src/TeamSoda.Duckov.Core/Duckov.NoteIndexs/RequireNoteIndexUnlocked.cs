using Duckov.Quests;

namespace Duckov.NoteIndexs;

public class RequireNoteIndexUnlocked : Condition
{
	public string key;

	public override bool Evaluate()
	{
		return NoteIndex.GetNoteUnlocked(key);
	}
}
