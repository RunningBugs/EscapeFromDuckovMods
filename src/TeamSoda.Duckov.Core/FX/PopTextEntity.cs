using TMPro;
using UnityEngine;

namespace FX;

public class PopTextEntity : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	private RectTransform _spriteRendererRectTransform_cache;

	[SerializeField]
	private TextMeshPro _tmp;

	public Vector3 velocity;

	public float size;

	private Color color;

	private Color endColor;

	public float spawnTime;

	private RectTransform spriteRendererRectTransform
	{
		get
		{
			if (_spriteRendererRectTransform_cache == null)
			{
				_spriteRendererRectTransform_cache = spriteRenderer.GetComponent<RectTransform>();
			}
			return _spriteRendererRectTransform_cache;
		}
	}

	private TextMeshPro tmp => _tmp;

	public TextMeshPro Tmp => tmp;

	public Color EndColor => endColor;

	public Color Color
	{
		get
		{
			return color;
		}
		set
		{
			color = value;
			endColor = color;
			endColor.a = 0f;
		}
	}

	public float timeSinceSpawn => Time.time - spawnTime;

	private string text
	{
		get
		{
			return tmp.text;
		}
		set
		{
			tmp.text = value;
		}
	}

	public void SetupContent(string text, Sprite sprite = null)
	{
		this.text = text;
		if (sprite == null)
		{
			spriteRenderer.gameObject.SetActive(value: false);
			return;
		}
		spriteRenderer.gameObject.SetActive(value: true);
		spriteRenderer.sprite = sprite;
		spriteRenderer.transform.localScale = Vector3.one * (0.5f / (sprite.rect.width / sprite.pixelsPerUnit));
	}

	internal void SetColor(Color newColor)
	{
		Tmp.color = newColor;
		spriteRenderer.color = newColor;
	}
}
