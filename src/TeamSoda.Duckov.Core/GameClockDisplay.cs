using TMPro;
using UnityEngine;

public class GameClockDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI text;

	private void Awake()
	{
		Refresh();
	}

	private void OnEnable()
	{
		GameClock.OnGameClockStep += Refresh;
	}

	private void OnDisable()
	{
		GameClock.OnGameClockStep -= Refresh;
	}

	private void Refresh()
	{
		string text = ((!(GameClock.Instance == null)) ? $"{GameClock.Hour:00}:{GameClock.Minut:00}" : "--:--");
		this.text.text = text;
	}
}
