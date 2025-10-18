using System;
using NodeCanvas.DialogueTrees;
using SodaCraft.Localizations;
using UnityEngine;

namespace Dialogues;

[Serializable]
public class LocalizedStatement : IStatement
{
	[SerializeField]
	private string _textKey = string.Empty;

	[SerializeField]
	private AudioClip _audio;

	[SerializeField]
	private string _meta = string.Empty;

	public string text => textKey.ToPlainText();

	public string textKey
	{
		get
		{
			return _textKey;
		}
		set
		{
			_textKey = value;
		}
	}

	public AudioClip audio
	{
		get
		{
			return _audio;
		}
		set
		{
			_audio = value;
		}
	}

	public string meta
	{
		get
		{
			return _meta;
		}
		set
		{
			_meta = value;
		}
	}

	public LocalizedStatement()
	{
	}

	public LocalizedStatement(string textKey)
	{
		_textKey = textKey;
	}

	public LocalizedStatement(string textKey, AudioClip audio)
	{
		_textKey = textKey;
		this.audio = audio;
	}

	public LocalizedStatement(string textKey, AudioClip audio, string meta)
	{
		_textKey = textKey;
		this.audio = audio;
		this.meta = meta;
	}
}
