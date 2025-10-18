using Duckov.UI;
using UnityEngine;

namespace Duckov.NoteIndexs;

public class NoteIndexProxy : MonoBehaviour
{
	public void UnlockNote(string key)
	{
		NoteIndex.SetNoteUnlocked(key);
	}

	public void UnlockAndShowNote(string key)
	{
		NoteIndexView.ShowNote(key);
	}
}
