using UnityEngine;

namespace ECM2.Examples;

public class CharacterPause : MonoBehaviour
{
	private Character _character;

	private void Awake()
	{
		_character = GetComponent<Character>();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			_character.Pause(!_character.isPaused);
		}
	}
}
