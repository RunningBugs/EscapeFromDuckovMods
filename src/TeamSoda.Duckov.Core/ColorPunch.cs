using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ColorPunch : MonoBehaviour
{
	[SerializeField]
	private Graphic graphic;

	[SerializeField]
	private float duration;

	[SerializeField]
	private Gradient gradient;

	[SerializeField]
	private Color tint = Color.white;

	private Color resetColor;

	private int activeToken;

	private void Awake()
	{
		if (graphic == null)
		{
			graphic = GetComponent<Graphic>();
		}
		resetColor = graphic.color;
	}

	public void Punch()
	{
		DoTask().Forget();
	}

	private int NewToken()
	{
		activeToken = Random.Range(1, int.MaxValue);
		return activeToken;
	}

	private async UniTask DoTask()
	{
		int token = NewToken();
		float time = 0f;
		if (!(duration <= 0f))
		{
			while (time < duration)
			{
				time += Time.unscaledDeltaTime;
				float time2 = time / duration;
				graphic.color = gradient.Evaluate(time2) * tint;
				await UniTask.NextFrame();
				if (token != activeToken)
				{
					return;
				}
			}
		}
		graphic.color = resetColor;
	}
}
