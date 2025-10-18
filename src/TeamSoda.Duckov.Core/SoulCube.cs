using UnityEngine;

public class SoulCube : MonoBehaviour
{
	private enum States
	{
		spawn,
		goToTarget
	}

	private States currentState;

	private SoulCollector target;

	private Vector3 direction;

	private float stateTimer;

	public Vector2 speedRange;

	private float spawnSpeed;

	public float spawnTime;

	public float toTargetSpeed;

	public AnimationCurve spawnSpeedCurve;

	private Vector3 velocity;

	public Transform roatePart;

	public Vector2 rotateSpeedRange = new Vector2(300f, 1000f);

	private float rotateSpeed;

	private Vector3 rotateAxis;

	public void Init(SoulCollector collectorTarget)
	{
		target = collectorTarget;
		direction = Random.insideUnitSphere + Vector3.up;
		direction.Normalize();
		spawnSpeed = Random.Range(speedRange.x, speedRange.y);
		roatePart.transform.localRotation = Quaternion.Euler(Random.insideUnitSphere * 360f);
		rotateAxis = Random.insideUnitSphere;
		rotateSpeed = Random.Range(rotateSpeedRange.x, rotateSpeedRange.y);
	}

	private void Update()
	{
		roatePart.Rotate(rotateSpeed * rotateAxis * Time.deltaTime);
		if (target == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		stateTimer += Time.deltaTime;
		switch (currentState)
		{
		case States.spawn:
			velocity = spawnSpeed * direction * spawnSpeedCurve.Evaluate(Mathf.Clamp01(stateTimer / spawnTime));
			base.transform.position += velocity * Time.deltaTime;
			if (stateTimer > spawnTime)
			{
				currentState = States.goToTarget;
			}
			break;
		case States.goToTarget:
			base.transform.position = Vector3.MoveTowards(base.transform.position, target.transform.position, toTargetSpeed * Time.deltaTime);
			if (Vector3.Distance(base.transform.position, target.transform.position) < 0.3f)
			{
				AddCube();
			}
			break;
		}
	}

	private void AddCube()
	{
		if ((bool)target)
		{
			target.AddCube();
		}
		Object.Destroy(base.gameObject);
	}
}
