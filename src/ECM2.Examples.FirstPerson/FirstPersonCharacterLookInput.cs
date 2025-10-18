using UnityEngine;

namespace ECM2.Examples.FirstPerson;

public class FirstPersonCharacterLookInput : MonoBehaviour
{
	[Space(15f)]
	public bool invertLook = true;

	[Tooltip("Mouse look sensitivity")]
	public Vector2 mouseSensitivity = new Vector2(1f, 1f);

	[Space(15f)]
	[Tooltip("How far in degrees can you move the camera down.")]
	public float minPitch = -80f;

	[Tooltip("How far in degrees can you move the camera up.")]
	public float maxPitch = 80f;

	private FirstPersonCharacter _character;

	private void Awake()
	{
		_character = GetComponent<FirstPersonCharacter>();
	}

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void Update()
	{
		Vector2 vector = new Vector2
		{
			x = Input.GetAxisRaw("Mouse X"),
			y = Input.GetAxisRaw("Mouse Y")
		};
		vector *= mouseSensitivity;
		_character.AddControlYawInput(vector.x);
		_character.AddControlPitchInput(invertLook ? (0f - vector.y) : vector.y);
	}
}
