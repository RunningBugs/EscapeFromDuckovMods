using ItemStatsSystem;
using LeTai.TrueShadow;
using TMPro;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;

public class WeightBarHUD : MonoBehaviour
{
	private CharacterMainControl characterMainControl;

	private float percent;

	private float weight;

	private float maxWeight;

	public ProceduralImage fillImage;

	public TrueShadow glow;

	public Color lightColor;

	public Color normalColor;

	public Color heavyColor;

	public Color overWeightColor;

	public TextMeshProUGUI weightText;

	public string weightTextFormat = "{0:0.#}/{1:0.#}kg";

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
		float totalWeight = characterMainControl.CharacterItem.TotalWeight;
		float a = characterMainControl.MaxWeight;
		if (!Mathf.Approximately(totalWeight, weight) || !Mathf.Approximately(a, maxWeight))
		{
			weight = totalWeight;
			maxWeight = a;
			percent = weight / maxWeight;
			weightText.text = string.Format(weightTextFormat, weight, maxWeight);
			fillImage.fillAmount = percent;
			SetColor();
		}
	}

	private void SetColor()
	{
		Color color = ((percent < 0.25f) ? lightColor : ((percent < 0.75f) ? normalColor : ((!(percent < 1f)) ? overWeightColor : heavyColor)));
		Color.RGBToHSV(color, out var H, out var S, out var V);
		Color color2 = color;
		if (S > 0.4f)
		{
			S = 0.4f;
			V = 1f;
			color2 = Color.HSVToRGB(H, S, V);
		}
		glow.Color = color;
		fillImage.color = color2;
		weightText.color = color;
	}
}
