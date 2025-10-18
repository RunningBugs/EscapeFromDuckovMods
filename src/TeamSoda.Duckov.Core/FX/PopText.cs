using System.Collections.Generic;
using UnityEngine;

namespace FX;

public class PopText : MonoBehaviour
{
	public static PopText instance;

	public PopTextEntity popTextPrefab;

	public List<PopTextEntity> inactiveEntries;

	public List<PopTextEntity> activeEntries;

	public float spawnVelocity = 5f;

	public float gravityValue = -9.8f;

	public float lifeTime = 1f;

	public AnimationCurve sizeOverLife;

	public float randomAngle = 10f;

	public Sprite debugSprite;

	private List<PopTextEntity> recycleList = new List<PopTextEntity>();

	private void Awake()
	{
		instance = this;
	}

	private PopTextEntity GetOrCreateEntry()
	{
		PopTextEntity popTextEntity;
		if (inactiveEntries.Count > 0)
		{
			popTextEntity = inactiveEntries[0];
			inactiveEntries.RemoveAt(0);
		}
		popTextEntity = Object.Instantiate(popTextPrefab, base.transform);
		activeEntries.Add(popTextEntity);
		popTextEntity.gameObject.SetActive(value: true);
		return popTextEntity;
	}

	public void InstancePop(string text, Vector3 worldPosition, Color color, float size, Sprite sprite = null)
	{
		PopTextEntity orCreateEntry = GetOrCreateEntry();
		orCreateEntry.Color = color;
		orCreateEntry.size = size;
		orCreateEntry.transform.localScale = Vector3.one * size;
		Transform obj = orCreateEntry.transform;
		obj.position = worldPosition;
		obj.rotation = LookAtMainCamera(worldPosition);
		float x = Random.Range(0f - randomAngle, randomAngle);
		float z = Random.Range(0f - randomAngle, randomAngle);
		Vector3 vector = Quaternion.Euler(x, 0f, z) * Vector3.up;
		orCreateEntry.SetupContent(text, sprite);
		orCreateEntry.velocity = vector * spawnVelocity;
		orCreateEntry.spawnTime = Time.time;
	}

	private static Quaternion LookAtMainCamera(Vector3 position)
	{
		if ((bool)Camera.main)
		{
			Transform transform = Camera.main.transform;
			return Quaternion.LookRotation(-(transform.position - position), transform.up);
		}
		return Quaternion.identity;
	}

	public void Recycle(PopTextEntity entry)
	{
		entry.gameObject.SetActive(value: false);
		activeEntries.Remove(entry);
		inactiveEntries.Add(entry);
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		Vector3 vector = Vector3.up * gravityValue;
		bool flag = false;
		foreach (PopTextEntity activeEntry in activeEntries)
		{
			if (activeEntry == null)
			{
				flag = true;
				continue;
			}
			Transform obj = activeEntry.transform;
			obj.position += activeEntry.velocity * deltaTime;
			obj.rotation = LookAtMainCamera(obj.position);
			activeEntry.velocity += vector * deltaTime;
			activeEntry.transform.localScale = sizeOverLife.Evaluate(activeEntry.timeSinceSpawn / lifeTime) * activeEntry.size * Vector3.one;
			float t = Mathf.Clamp01(activeEntry.timeSinceSpawn / lifeTime * 2f - 1f);
			Color color = Color.Lerp(activeEntry.Color, activeEntry.EndColor, t);
			activeEntry.SetColor(color);
			if (activeEntry.timeSinceSpawn > lifeTime)
			{
				recycleList.Add(activeEntry);
			}
		}
		if (recycleList.Count > 0)
		{
			foreach (PopTextEntity recycle in recycleList)
			{
				Recycle(recycle);
			}
			recycleList.Clear();
		}
		if (flag)
		{
			activeEntries.RemoveAll((PopTextEntity e) => e == null);
		}
	}

	private void PopTest()
	{
		Vector3 worldPosition = base.transform.position;
		CharacterMainControl main = CharacterMainControl.Main;
		if (main != null)
		{
			worldPosition = main.transform.position + Vector3.up * 2f;
		}
		InstancePop("Test", worldPosition, Color.white, 1f, debugSprite);
	}

	public static void Pop(string text, Vector3 worldPosition, Color color, float size, Sprite sprite = null)
	{
		if (!DevCam.devCamOn && (bool)instance)
		{
			instance.InstancePop(text, worldPosition, color, size, sprite);
		}
	}
}
