using Cysharp.Threading.Tasks;
using Duckov.UI.DialogueBubbles;
using SodaCraft.Localizations;
using UnityEngine;

public class DialogueBubbleProxy : MonoBehaviour
{
	[LocalizationKey("Dialogues")]
	public string textKey;

	public float yOffset;

	public float duration = 2f;

	public void Pop()
	{
		DialogueBubblesManager.Show(textKey.ToPlainText(), base.transform, yOffset, needInteraction: false, skippable: false, -1f, duration).Forget();
	}

	public void Pop(string text, float speed = -1f)
	{
		DialogueBubblesManager.Show(text, base.transform, yOffset, needInteraction: false, skippable: false, speed).Forget();
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawCube(base.transform.position + Vector3.up * yOffset, Vector3.one * 0.2f);
	}
}
