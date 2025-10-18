using Duckov.NoteIndexs;
using Duckov.UI.Animations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public class NoteIndexView_Inspector : MonoBehaviour
{
	[SerializeField]
	private FadeGroup placeHolder;

	[SerializeField]
	private FadeGroup content;

	[SerializeField]
	private TextMeshProUGUI textTitle;

	[SerializeField]
	private TextMeshProUGUI textContent;

	[SerializeField]
	private Image image;

	private Note note;

	private void Awake()
	{
		placeHolder.Show();
		content.SkipHide();
	}

	internal void Setup(Note value)
	{
		if (value == null)
		{
			placeHolder.Show();
			content.Hide();
			return;
		}
		note = value;
		SetupContent(note);
		placeHolder.Hide();
		content.Show();
		NoteIndex.SetNoteRead(value.key);
	}

	private void SetupContent(Note value)
	{
		textTitle.text = value.Title;
		textContent.text = value.Content;
		image.sprite = value.image;
		image.gameObject.SetActive(value.image == null);
	}
}
