using Cysharp.Threading.Tasks;
using Duckov.UI.DialogueBubbles;
using SodaCraft.Localizations;
using UnityEngine;

public class BunkerDoorVisual : MonoBehaviour
{
	[LocalizationKey("Dialogues")]
	public string welcomeText;

	[LocalizationKey("Dialogues")]
	public string leaveText;

	public Transform textBubblePoint;

	public bool inRange = true;

	public Animator animator;

	private void Awake()
	{
		animator.SetBool("InRange", inRange);
	}

	public void OnEnter()
	{
		if (!inRange)
		{
			inRange = true;
			animator.SetBool("InRange", inRange);
			PopText(welcomeText.ToPlainText(), 0.5f, inRange).Forget();
		}
	}

	public void OnExit()
	{
		if (inRange)
		{
			inRange = false;
			animator.SetBool("InRange", inRange);
			PopText(leaveText.ToPlainText(), 0f, inRange).Forget();
		}
	}

	private async UniTask PopText(string text, float delay, bool _inRange)
	{
		await UniTask.WaitForSeconds(delay);
		if (inRange == _inRange)
		{
			DialogueBubblesManager.Show(text, textBubblePoint).Forget();
		}
	}
}
