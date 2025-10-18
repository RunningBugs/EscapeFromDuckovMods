using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;

public class HealthHUD : MonoBehaviour
{
	private CharacterMainControl characterMainControl;

	private float percent = -1f;

	private float maxHealth;

	private float currenthealth;

	public ProceduralImage fillImage;

	public ProceduralImage backgroundImage;

	public Color backgroundColor;

	public Color emptyBackgroundColor;

	public TextMeshProUGUI text;

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
		float num = characterMainControl.Health.MaxHealth;
		float currentHealth = characterMainControl.Health.CurrentHealth;
		float a = currentHealth / num;
		if (!Mathf.Approximately(a, percent))
		{
			percent = a;
			fillImage.fillAmount = percent;
			if (percent <= 0f)
			{
				backgroundImage.color = emptyBackgroundColor;
			}
			else
			{
				backgroundImage.color = backgroundColor;
			}
		}
		if (num != maxHealth || currentHealth != currenthealth)
		{
			maxHealth = num;
			currenthealth = currentHealth;
			text.text = currenthealth.ToString("0.#") + " / " + maxHealth.ToString("0.#");
		}
	}
}
