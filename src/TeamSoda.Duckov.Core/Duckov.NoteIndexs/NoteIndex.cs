using System;
using System.Collections.Generic;
using System.Linq;
using Saves;
using Sirenix.Utilities;
using UnityEngine;

namespace Duckov.NoteIndexs;

public class NoteIndex : MonoBehaviour
{
	[Serializable]
	private struct SaveData
	{
		public List<string> unlockedNotes;

		public List<string> readNotes;

		public SaveData(NoteIndex from)
		{
			unlockedNotes = from.unlockedNotes.ToList();
			readNotes = from.unlockedNotes.ToList();
		}

		public void Setup(NoteIndex to)
		{
			to.unlockedNotes.Clear();
			if (unlockedNotes != null)
			{
				to.unlockedNotes.AddRange(unlockedNotes);
			}
			to.readNotes.Clear();
			if (readNotes != null)
			{
				to.readNotes.AddRange(readNotes);
			}
		}
	}

	[SerializeField]
	private List<Note> notes = new List<Note>();

	private Dictionary<string, Note> _dic;

	private HashSet<string> unlockedNotes = new HashSet<string>();

	private HashSet<string> readNotes = new HashSet<string>();

	public static Action<string> onNoteStatusChanged;

	private const string SaveKey = "NoteIndexData";

	public static NoteIndex Instance => GameManager.NoteIndex;

	public List<Note> Notes => notes;

	private Dictionary<string, Note> MDic
	{
		get
		{
			if (_dic == null)
			{
				RebuildDic();
			}
			return _dic;
		}
	}

	public HashSet<string> UnlockedNotes => unlockedNotes;

	public HashSet<string> ReadNotes => unlockedNotes;

	private void RebuildDic()
	{
		if (_dic == null)
		{
			_dic = new Dictionary<string, Note>();
		}
		_dic.Clear();
		foreach (Note note in notes)
		{
			_dic[note.key] = note;
		}
	}

	public static IEnumerable<string> GetAllNotes(bool unlockedOnly = true)
	{
		if (Instance == null)
		{
			yield break;
		}
		foreach (Note note in Instance.notes)
		{
			string key = note.key;
			if (!unlockedOnly || GetNoteUnlocked(key))
			{
				yield return note.key;
			}
		}
	}

	private void Awake()
	{
		SavesSystem.OnCollectSaveData += Save;
		SavesSystem.OnSetFile += Load;
		Load();
	}

	private void OnDestroy()
	{
		SavesSystem.OnCollectSaveData -= Save;
		SavesSystem.OnSetFile -= Load;
	}

	private void Save()
	{
		SaveData value = new SaveData(this);
		SavesSystem.Save("NoteIndexData", value);
	}

	private void Load()
	{
		SavesSystem.Load<SaveData>("NoteIndexData").Setup(this);
	}

	public void MSetEntryDynamic(Note note)
	{
		MDic[note.key] = note;
	}

	public Note MGetNote(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			Debug.LogError("Trying to get note with an empty key.");
			return null;
		}
		if (!MDic.TryGetValue(key, out var value))
		{
			Debug.LogError("Cannot find note: " + key);
			return null;
		}
		return value;
	}

	public static Note GetNote(string key)
	{
		if (Instance == null)
		{
			return null;
		}
		return Instance.MGetNote(key);
	}

	public static bool SetNoteDynamic(Note note)
	{
		if (Instance == null)
		{
			return false;
		}
		Instance.MSetEntryDynamic(note);
		return true;
	}

	public static bool GetNoteUnlocked(string noteKey)
	{
		if (Instance == null)
		{
			return false;
		}
		return Instance.unlockedNotes.Contains(noteKey);
	}

	public static bool GetNoteRead(string noteKey)
	{
		if (Instance == null)
		{
			return false;
		}
		return Instance.readNotes.Contains(noteKey);
	}

	public static void SetNoteUnlocked(string noteKey)
	{
		if (!(Instance == null))
		{
			Instance.unlockedNotes.Add(noteKey);
			onNoteStatusChanged?.Invoke(noteKey);
		}
	}

	public static void SetNoteRead(string noteKey)
	{
		if (!(Instance == null))
		{
			Instance.readNotes.Add(noteKey);
			onNoteStatusChanged?.Invoke(noteKey);
		}
	}

	internal static int GetTotalNoteCount()
	{
		if (Instance == null)
		{
			return 0;
		}
		return Instance.Notes.Count();
	}

	internal static int GetUnlockedNoteCount()
	{
		if (Instance == null)
		{
			return 0;
		}
		return Instance.UnlockedNotes.Count;
	}
}
