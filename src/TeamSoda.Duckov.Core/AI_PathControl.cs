using Pathfinding;
using UnityEngine;

public class AI_PathControl : MonoBehaviour
{
	public Seeker seeker;

	public CharacterMainControl controller;

	public Path path;

	public float nextWaypointDistance = 3f;

	private int currentWaypoint;

	private bool reachedEndOfPath;

	public float stopDistance = 0.2f;

	private bool moving;

	private bool waitingForPathResult;

	public bool ReachedEndOfPath => reachedEndOfPath;

	public bool Moving => moving;

	public bool WaitingForPathResult => waitingForPathResult;

	public void Start()
	{
	}

	public void MoveToPos(Vector3 pos)
	{
		reachedEndOfPath = false;
		path = null;
		seeker.StartPath(base.transform.position, pos, OnPathComplete);
		waitingForPathResult = true;
	}

	public void OnPathComplete(Path p)
	{
		if (!p.error)
		{
			path = p;
			currentWaypoint = 0;
			moving = true;
		}
		waitingForPathResult = false;
	}

	public void Update()
	{
		moving = path != null;
		if (path == null)
		{
			return;
		}
		reachedEndOfPath = false;
		float num;
		while (true)
		{
			num = Vector3.Distance(base.transform.position, path.vectorPath[currentWaypoint]);
			if (!(num < nextWaypointDistance))
			{
				break;
			}
			if (currentWaypoint + 1 < path.vectorPath.Count)
			{
				currentWaypoint++;
				continue;
			}
			reachedEndOfPath = true;
			break;
		}
		Vector3 normalized = (path.vectorPath[currentWaypoint] - base.transform.position).normalized;
		if (reachedEndOfPath)
		{
			float num2 = Mathf.Sqrt(num / nextWaypointDistance);
			controller.SetMoveInput(normalized * num2);
			if (num < stopDistance)
			{
				path = null;
				controller.SetMoveInput(Vector2.zero);
			}
		}
		else
		{
			controller.SetMoveInput(normalized);
		}
	}

	public void StopMove()
	{
		path = null;
		controller.SetMoveInput(Vector3.zero);
		waitingForPathResult = false;
	}
}
