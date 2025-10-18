using UnityEngine;

public class PaperBox : MonoBehaviour
{
	[HideInInspector]
	public CharacterMainControl character;

	public Transform setActiveWhileStandStill;

	private void Update()
	{
		if ((bool)character && (bool)setActiveWhileStandStill)
		{
			bool flag = character.Velocity.magnitude < 0.2f;
			if (setActiveWhileStandStill.gameObject.activeSelf != flag)
			{
				setActiveWhileStandStill.gameObject.SetActive(flag);
			}
		}
	}
}
