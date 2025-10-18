using System.Collections.Generic;
using UnityEngine;

namespace Duckov.MiniGames.Examples.HelloWorld;

public class Move : MiniGameBehaviour
{
	[SerializeField]
	private Rigidbody rigidbody;

	[SerializeField]
	private float speed = 10f;

	[SerializeField]
	private float jumpSpeed = 5f;

	private List<Collider> touchingColliders = new List<Collider>();

	private void Awake()
	{
		if (rigidbody == null)
		{
			rigidbody = GetComponent<Rigidbody>();
		}
	}

	protected override void OnUpdate(float deltaTime)
	{
		bool flag = CanJump();
		Vector2 vector = base.Game.GetAxis() * speed;
		float y = rigidbody.velocity.y;
		if (base.Game.GetButtonDown(MiniGame.Button.A) && flag)
		{
			y = jumpSpeed;
		}
		rigidbody.velocity = new Vector3(vector.x, y, vector.y);
	}

	private bool CanJump()
	{
		if (touchingColliders.Count > 0)
		{
			return true;
		}
		return false;
	}

	private void OnCollisionEnter(Collision collision)
	{
		touchingColliders.Add(collision.collider);
	}

	private void OnCollisionExit(Collision collision)
	{
		touchingColliders.Remove(collision.collider);
	}
}
