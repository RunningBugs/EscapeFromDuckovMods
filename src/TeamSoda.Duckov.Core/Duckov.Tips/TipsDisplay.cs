using System.Linq;
using TMPro;
using UnityEngine;

namespace Duckov.Tips;

public class TipsDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private TipEntry[] entries;

	[SerializeField]
	private CanvasGroup canvasGroup;

	public void DisplayRandom()
	{
		if (entries.Length != 0)
		{
			TipEntry tipEntry = entries[Random.Range(0, entries.Length)];
			text.text = tipEntry.Description;
		}
	}

	public void Display(string tipID)
	{
		TipEntry tipEntry = entries.FirstOrDefault((TipEntry e) => e.TipID == tipID);
		if (!(tipEntry.TipID != tipID))
		{
			text.text = tipEntry.Description;
		}
	}

	private void OnEnable()
	{
		canvasGroup.alpha = (SceneLoader.HideTips ? 0f : 1f);
		DisplayRandom();
	}
}
