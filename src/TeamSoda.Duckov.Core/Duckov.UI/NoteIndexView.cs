using System;
using Duckov.NoteIndexs;
using Duckov.UI.Animations;
using Duckov.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public class NoteIndexView : View
{
	[SerializeField]
	private FadeGroup mainFadeGroup;

	[SerializeField]
	private GameObject noEntryIndicator;

	[SerializeField]
	private NoteIndexView_Entry entryTemplate;

	[SerializeField]
	private NoteIndexView_Inspector inspector;

	[SerializeField]
	private TextMeshProUGUI noteCountText;

	[SerializeField]
	private ScrollRect indexScrollView;

	private PrefabPool<NoteIndexView_Entry> _pool;

	private string displayingNote;

	private bool needFocus;

	private PrefabPool<NoteIndexView_Entry> Pool
	{
		get
		{
			if (_pool == null)
			{
				_pool = new PrefabPool<NoteIndexView_Entry>(entryTemplate);
			}
			return _pool;
		}
	}

	private void OnEnable()
	{
		NoteIndex.onNoteStatusChanged = (Action<string>)Delegate.Combine(NoteIndex.onNoteStatusChanged, new Action<string>(OnNoteStatusChanged));
	}

	private void OnDisable()
	{
		NoteIndex.onNoteStatusChanged = (Action<string>)Delegate.Remove(NoteIndex.onNoteStatusChanged, new Action<string>(OnNoteStatusChanged));
	}

	private void Update()
	{
		if (needFocus)
		{
			needFocus = false;
			MoveScrollViewToActiveEntry();
		}
	}

	private void OnNoteStatusChanged(string noteKey)
	{
		RefreshEntries();
	}

	public void DoOpen()
	{
		Open();
	}

	protected override void OnOpen()
	{
		base.OnOpen();
		mainFadeGroup.Show();
		RefreshEntries();
		SetDisplayTargetNote(displayingNote);
	}

	protected override void OnClose()
	{
		base.OnClose();
		mainFadeGroup.Hide();
	}

	protected override void OnCancel()
	{
		Close();
	}

	private void RefreshNoteCount()
	{
		int totalNoteCount = NoteIndex.GetTotalNoteCount();
		int unlockedNoteCount = NoteIndex.GetUnlockedNoteCount();
		noteCountText.text = $"{unlockedNoteCount} / {totalNoteCount}";
	}

	private void RefreshEntries()
	{
		RefreshNoteCount();
		Pool.ReleaseAll();
		if (NoteIndex.Instance == null)
		{
			return;
		}
		int num = 0;
		foreach (string allNote in NoteIndex.GetAllNotes(unlockedOnly: false))
		{
			Note note = NoteIndex.GetNote(allNote);
			if (note != null)
			{
				NoteIndexView_Entry noteIndexView_Entry = Pool.Get();
				num++;
				noteIndexView_Entry.Setup(note, OnEntryClicked, GetDisplayingNote, num);
			}
		}
		noEntryIndicator.SetActive(num <= 0);
	}

	private string GetDisplayingNote()
	{
		return displayingNote;
	}

	public void SetDisplayTargetNote(string noteKey)
	{
		Note note = null;
		if (!string.IsNullOrWhiteSpace(noteKey))
		{
			note = NoteIndex.GetNote(noteKey);
		}
		if (note == null)
		{
			displayingNote = null;
		}
		else
		{
			displayingNote = note.key;
		}
		foreach (NoteIndexView_Entry activeEntry in Pool.ActiveEntries)
		{
			activeEntry.NotifySelectedDisplayingNoteChanged(displayingNote);
		}
		inspector.Setup(note);
	}

	private void OnEntryClicked(NoteIndexView_Entry entry)
	{
		string key = entry.key;
		if (!NoteIndex.GetNoteUnlocked(key))
		{
			SetDisplayTargetNote("");
		}
		else
		{
			SetDisplayTargetNote(key);
		}
	}

	public static void ShowNote(string noteKey, bool unlock = true)
	{
		NoteIndexView viewInstance = View.GetViewInstance<NoteIndexView>();
		if (!(viewInstance == null))
		{
			if (unlock)
			{
				NoteIndex.SetNoteUnlocked(noteKey);
			}
			if (!(View.ActiveView is NoteIndexView))
			{
				viewInstance.Open();
			}
			viewInstance.SetDisplayTargetNote(noteKey);
			viewInstance.needFocus = true;
		}
	}

	private void MoveScrollViewToActiveEntry()
	{
		NoteIndexView_Entry displayingEntry = GetDisplayingEntry();
		if (!(displayingEntry == null))
		{
			RectTransform rectTransform = displayingEntry.transform as RectTransform;
			if (!(rectTransform == null))
			{
				float num = 0f - rectTransform.anchoredPosition.y;
				float height = indexScrollView.content.rect.height;
				float verticalNormalizedPosition = 1f - num / height;
				indexScrollView.verticalNormalizedPosition = verticalNormalizedPosition;
			}
		}
	}

	private NoteIndexView_Entry GetDisplayingEntry()
	{
		foreach (NoteIndexView_Entry activeEntry in Pool.ActiveEntries)
		{
			if (activeEntry.key == displayingNote)
			{
				return activeEntry;
			}
		}
		return null;
	}
}
