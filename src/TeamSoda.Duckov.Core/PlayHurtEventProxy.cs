using Duckov;
using UnityEngine;

public class PlayHurtEventProxy : MonoBehaviour
{
	[SerializeField]
	private string critSfx;

	[SerializeField]
	private string nonCritSfx;

	public void Play(bool crit)
	{
		if (crit)
		{
			AudioManager.Post(critSfx, base.gameObject);
		}
		else
		{
			AudioManager.Post(nonCritSfx, base.gameObject);
		}
	}
}
