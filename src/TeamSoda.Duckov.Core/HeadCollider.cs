using UnityEngine;

public class HeadCollider : MonoBehaviour
{
	private CharacterMainControl character;

	[SerializeField]
	private SphereCollider sphereCollider;

	public void Init(CharacterMainControl _character)
	{
		character = _character;
		character.OnTeamChanged += OnSetTeam;
	}

	private void OnDestroy()
	{
		if ((bool)character)
		{
			character.OnTeamChanged -= OnSetTeam;
		}
	}

	private void OnSetTeam(Teams team)
	{
		bool flag = Team.IsEnemy(Teams.player, team);
		sphereCollider.enabled = flag;
	}

	private void OnDrawGizmos()
	{
		Color yellow = Color.yellow;
		yellow.a = 0.3f;
		Gizmos.color = yellow;
		Gizmos.DrawSphere(base.transform.position, sphereCollider.radius * base.transform.lossyScale.x);
	}
}
