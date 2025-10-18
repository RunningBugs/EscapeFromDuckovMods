using ItemStatsSystem;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;

public class StaminaHUD : MonoBehaviour
{
	private CharacterMainControl characterMainControl;

	private float percent;

	public CanvasGroup canvasGroup;

	private float targetAlpha;

	public ProceduralImage fillImage;

	public Gradient glowColor;

	private Item item => characterMainControl.CharacterItem;

	private void Update()
	{
		if (!characterMainControl)
		{
			characterMainControl = LevelManager.Instance.MainCharacter;
			if (!characterMainControl)
			{
				return;
			}
		}
		float a = characterMainControl.CurrentStamina / characterMainControl.MaxStamina;
		if (!Mathf.Approximately(a, percent))
		{
			percent = a;
			fillImage.fillAmount = percent;
			SetColor();
			if (Mathf.Approximately(a, 1f))
			{
				targetAlpha = 0f;
			}
			else
			{
				targetAlpha = 1f;
			}
		}
		UpdateAlpha(Time.unscaledDeltaTime);
	}

	private void SetColor()
	{
		Color.RGBToHSV(glowColor.Evaluate(percent), out var H, out var S, out var V);
		S = 0.4f;
		V = 1f;
		Color color = Color.HSVToRGB(H, S, V);
		fillImage.color = color;
	}

	private void UpdateAlpha(float deltaTime)
	{
		if (targetAlpha != canvasGroup.alpha)
		{
			canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, 5f * deltaTime);
		}
	}
}
