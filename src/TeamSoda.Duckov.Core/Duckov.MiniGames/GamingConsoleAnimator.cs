using UnityEngine;

namespace Duckov.MiniGames;

public class GamingConsoleAnimator : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private GamingConsole console;

	private Vector2 joyStick_Current;

	private Vector2 joyStick_Target;

	[SerializeField]
	private MiniGame Game
	{
		get
		{
			if (console == null)
			{
				return null;
			}
			return console.Game;
		}
	}

	private void Update()
	{
		Tick();
	}

	private void Tick()
	{
		if (Game == null)
		{
			Clear();
		}
		else if (!CameraMode.Active)
		{
			joyStick_Target = Game.GetAxis();
			joyStick_Current = Vector2.Lerp(joyStick_Current, joyStick_Target, 0.25f);
			Vector2 vector = joyStick_Current;
			animator.SetFloat("AxisX", vector.x);
			animator.SetFloat("AxisY", vector.y);
			animator.SetBool("ButtonA", Game.GetButton(MiniGame.Button.A));
			animator.SetBool("ButtonB", Game.GetButton(MiniGame.Button.B));
		}
	}

	private void Clear()
	{
		animator.SetBool("ButtonA", value: false);
		animator.SetBool("ButtonB", value: false);
		animator.SetFloat("AxisX", 0f);
		animator.SetFloat("AxisY", 0f);
	}
}
