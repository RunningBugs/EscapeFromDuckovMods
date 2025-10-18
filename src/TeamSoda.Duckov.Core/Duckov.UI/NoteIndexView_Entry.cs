using System;
using Duckov.NoteIndexs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Duckov.UI;

public class NoteIndexView_Entry : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private GameObject highlightIndicator;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI indexText;

	[SerializeField]
	private GameObject notReadIndicator;

	private Note note;

	private Action<NoteIndexView_Entry> onClicked;

	private Func<string> getDisplayingNote;

	public string key => note.key;

	private void OnEnable()
	{
		NoteIndex.onNoteStatusChanged = (Action<string>)Delegate.Combine(NoteIndex.onNoteStatusChanged, new Action<string>(OnNoteStatusChanged));
	}

	private void OnDisable()
	{
		NoteIndex.onNoteStatusChanged = (Action<string>)Delegate.Remove(NoteIndex.onNoteStatusChanged, new Action<string>(OnNoteStatusChanged));
	}

	private void OnNoteStatusChanged(string key)
	{
		if (!(key != note.key))
		{
			RefreshNotReadIndicator();
		}
	}

	private void RefreshNotReadIndicator()
	{
		notReadIndicator.SetActive(NoteIndex.GetNoteUnlocked(key) && !NoteIndex.GetNoteRead(key));
	}

	internal void NotifySelectedDisplayingNoteChanged(string displayingNote)
	{
		RefreshHighlight();
	}

	private void RefreshHighlight()
	{
		bool active = false;
		if (getDisplayingNote != null)
		{
			active = getDisplayingNote?.Invoke() == key;
		}
		highlightIndicator.SetActive(active);
	}

	internal void Setup(Note note, Action<NoteIndexView_Entry> onClicked, Func<string> getDisplayingNote, int index)
	{
		bool noteUnlocked = NoteIndex.GetNoteUnlocked(note.key);
		this.note = note;
		titleText.text = (noteUnlocked ? note.Title : "???");
		this.onClicked = onClicked;
		this.getDisplayingNote = getDisplayingNote;
		if (index > 0)
		{
			indexText.text = index.ToString("000");
		}
		RefreshNotReadIndicator();
		RefreshHighlight();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		onClicked?.Invoke(this);
	}
}
