using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FOW.Demos;

public class TeamsDemo : MonoBehaviour
{
	public Text teamText;

	public Color team1Color = Color.blue;

	public List<FogOfWarRevealer> team1Members = new List<FogOfWarRevealer>();

	public Color team2Color = Color.green;

	public List<FogOfWarRevealer> team2Members = new List<FogOfWarRevealer>();

	public Color team3Color = Color.red;

	public List<FogOfWarRevealer> team3Members = new List<FogOfWarRevealer>();

	private int team;

	private void Awake()
	{
		team = 2;
		changeTeams();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			changeTeams();
		}
	}

	private void changeTeams()
	{
		team++;
		team %= 3;
		teamText.text = $"VIEWING AS TEAM {team + 1}";
		foreach (FogOfWarRevealer team1Member in team1Members)
		{
			team1Member.enabled = false;
			team1Member.GetComponent<FogOfWarHider>().enabled = true;
		}
		foreach (FogOfWarRevealer team2Member in team2Members)
		{
			team2Member.enabled = false;
			team2Member.GetComponent<FogOfWarHider>().enabled = true;
		}
		foreach (FogOfWarRevealer team3Member in team3Members)
		{
			team3Member.enabled = false;
			team3Member.GetComponent<FogOfWarHider>().enabled = true;
		}
		switch (team)
		{
		case 0:
			teamText.color = team1Color;
			{
				foreach (FogOfWarRevealer team1Member2 in team1Members)
				{
					team1Member2.enabled = true;
					team1Member2.GetComponent<FogOfWarHider>().enabled = false;
				}
				break;
			}
		case 1:
			teamText.color = team2Color;
			{
				foreach (FogOfWarRevealer team2Member2 in team2Members)
				{
					team2Member2.enabled = true;
					team2Member2.GetComponent<FogOfWarHider>().enabled = false;
				}
				break;
			}
		case 2:
			teamText.color = team3Color;
			{
				foreach (FogOfWarRevealer team3Member2 in team3Members)
				{
					team3Member2.enabled = true;
					team3Member2.GetComponent<FogOfWarHider>().enabled = false;
				}
				break;
			}
		}
	}
}
