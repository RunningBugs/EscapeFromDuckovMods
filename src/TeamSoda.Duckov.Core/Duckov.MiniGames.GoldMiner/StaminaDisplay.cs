using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.MiniGames.GoldMiner;

public class StaminaDisplay : MonoBehaviour
{
	[SerializeField]
	private GoldMiner master;

	[SerializeField]
	private Image fill;

	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private Gradient normalColor;

	[SerializeField]
	private Color extraColor = Color.red;

	private void FixedUpdate()
	{
		Refresh();
	}

	private void Refresh()
	{
		if (master == null)
		{
			return;
		}
		GoldMinerRunData run = master.run;
		if (run == null)
		{
			return;
		}
		float stamina = run.stamina;
		float value = run.maxStamina.Value;
		float value2 = run.extraStamina.Value;
		if (stamina > 0f)
		{
			float num = stamina / value;
			fill.fillAmount = num;
			fill.color = normalColor.Evaluate(num);
			text.text = $"{stamina:0.0}";
			return;
		}
		float num2 = value2 + stamina;
		if (num2 < 0f)
		{
			num2 = 0f;
		}
		float fillAmount = num2 / value2;
		fill.fillAmount = fillAmount;
		fill.color = extraColor;
		text.text = $"{num2:0.00}";
	}
}
