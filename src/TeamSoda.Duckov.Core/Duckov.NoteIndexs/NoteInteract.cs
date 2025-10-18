using Duckov.UI;

namespace Duckov.NoteIndexs;

public class NoteInteract : InteractableBase
{
	public string noteKey;

	[LocalizationKey("Default")]
	public string noteTitle;

	[LocalizationKey("Default")]
	public string noteContent;

	protected override void Start()
	{
		base.Start();
		if (NoteIndex.GetNoteUnlocked(noteKey))
		{
			base.gameObject.SetActive(value: false);
		}
		finishWhenTimeOut = true;
	}

	protected override void OnInteractFinished()
	{
		NoteIndex.SetNoteUnlocked(noteKey);
		NoteIndexView.ShowNote(noteKey);
		base.gameObject.SetActive(value: false);
	}

	private void OnValidate()
	{
		noteTitle = "Note_" + noteKey + "_Title";
		noteContent = "Note_" + noteKey + "_Content";
	}

	public void ReName()
	{
		base.gameObject.name = "Note_" + noteKey;
	}
}
