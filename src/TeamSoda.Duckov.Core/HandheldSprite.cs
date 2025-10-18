using UnityEngine;

public class HandheldSprite : MonoBehaviour
{
	public DuckovItemAgent agent;

	public SpriteRenderer spriteRenderer;

	private void Start()
	{
		if ((bool)agent.Item)
		{
			spriteRenderer.sprite = agent.Item.Icon;
		}
	}
}
