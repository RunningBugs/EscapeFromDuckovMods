using UnityEngine;
using UnityEngine.UI;

namespace Duckov.Sounds;

public class SoundDisplay : MonoBehaviour
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private float removeRecordAfterTime = 1f;

	[SerializeField]
	private float triggerVelocity = 10f;

	[SerializeField]
	private float gravity = 1f;

	[SerializeField]
	private float untriggerVelocity = 100f;

	private float value;

	private float velocity;

	private AISound sound;

	public float Value => value;

	public AISound CurrentSount => sound;

	internal void Trigger(AISound sound)
	{
		this.sound = sound;
		base.gameObject.SetActive(value: true);
		velocity = triggerVelocity;
		value += velocity * Time.deltaTime;
	}

	private void Update()
	{
		velocity -= gravity * Time.deltaTime;
		value += velocity * Time.deltaTime;
		if (value > 1f || value < 0f)
		{
			velocity = 0f;
		}
		value = Mathf.Clamp01(value);
		image.color = new Color(1f, 1f, 1f, value);
	}
}
