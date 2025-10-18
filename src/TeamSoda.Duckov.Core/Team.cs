public class Team
{
	public static bool IsEnemy(Teams selfTeam, Teams targetTeam)
	{
		switch (selfTeam)
		{
		case Teams.middle:
			return false;
		case Teams.all:
			return true;
		default:
			if (targetTeam == Teams.middle)
			{
				return false;
			}
			return selfTeam != targetTeam;
		}
	}
}
