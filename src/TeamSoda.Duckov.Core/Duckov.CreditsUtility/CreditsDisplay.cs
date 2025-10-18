using System.Collections.Generic;
using UnityEngine;

namespace Duckov.CreditsUtility;

public class CreditsDisplay : MonoBehaviour
{
	private class GenerationStatus
	{
		public List<Token> records = new List<Token>();

		public Stack<Transform> transforms = new Stack<Transform>();

		public bool s;

		public bool l;

		public bool b;

		public Color color = Color.white;

		public VerticalEntry activeItem;

		public void Flush()
		{
			s = false;
			l = false;
			b = false;
			color = Color.white;
		}
	}

	[SerializeField]
	private bool setupOnAwake;

	[SerializeField]
	private TextAsset content;

	[SerializeField]
	private Transform rootContentTransform;

	[SerializeField]
	private float internalItemSpacing = 8f;

	[SerializeField]
	private float mainSpacing = 16f;

	[SerializeField]
	private float itemWidth = 350f;

	[Header("Prefabs")]
	[SerializeField]
	private HorizontalEntry horizontalPrefab;

	[SerializeField]
	private VerticalEntry verticalPrefab;

	[SerializeField]
	private EmptyEntry emptyPrefab;

	[SerializeField]
	private TextEntry textPrefab;

	[SerializeField]
	private ImageEntry imagePrefab;

	private GenerationStatus status;

	private Transform CurrentTransform => GetCurrentTransform();

	private void ParseAndDisplay()
	{
		Reset();
		CreditsLexer creditsLexer = new CreditsLexer(content.text);
		BeginVerticalLayout();
		foreach (Token item in creditsLexer)
		{
			if (status.records.Count > 0)
			{
				_ = status.records[status.records.Count - 1];
			}
			status.records.Add(item);
			switch (item.type)
			{
			case TokenType.Invalid:
				Debug.LogError("Invalid Token: " + item.text);
				continue;
			case TokenType.End:
				break;
			case TokenType.String:
				DoText(item.text);
				continue;
			case TokenType.Instructor:
				DoInstructor(item.text);
				continue;
			case TokenType.EmptyLine:
				EndItem();
				continue;
			default:
				continue;
			}
			break;
		}
		EndLayout();
	}

	private void EndItem()
	{
		if ((bool)status.activeItem)
		{
			status.activeItem = null;
			EndLayout();
		}
	}

	private void BeginItem()
	{
		status.activeItem = BeginVerticalLayout();
		status.activeItem.SetLayoutSpacing(internalItemSpacing);
		status.activeItem.SetPreferredWidth(itemWidth);
	}

	private void DoEmpty(params string[] elements)
	{
		Object.Instantiate(emptyPrefab, CurrentTransform).Setup(elements);
	}

	private void DoInstructor(string text)
	{
		string[] array = text.Split(' ');
		if (array.Length >= 1)
		{
			switch (array[0])
			{
			case "Horizontal":
				BeginHorizontalLayout(array);
				break;
			case "Vertical":
				BeginVerticalLayout(array);
				break;
			case "End":
				EndLayout();
				break;
			case "s":
				status.s = true;
				break;
			case "l":
				status.l = true;
				break;
			case "b":
				status.b = true;
				break;
			case "color":
				DoColor(array);
				break;
			case "image":
				DoImage(array);
				break;
			case "Space":
				DoEmpty(array);
				break;
			}
		}
	}

	private void DoImage(string[] elements)
	{
		if (status.activeItem == null)
		{
			BeginItem();
		}
		Object.Instantiate(imagePrefab, CurrentTransform).Setup(elements);
	}

	private void DoColor(string[] elements)
	{
		if (elements.Length >= 2)
		{
			ColorUtility.TryParseHtmlString(elements[1], out var color);
			status.color = color;
		}
	}

	private void DoText(string text)
	{
		if (status.activeItem == null)
		{
			BeginItem();
		}
		TextEntry textEntry = Object.Instantiate(textPrefab, CurrentTransform);
		int size = 30;
		if (status.s)
		{
			size = 20;
		}
		if (status.l)
		{
			size = 40;
		}
		textEntry.Setup(bold: status.b, text: text, color: status.color, size: size);
		status.Flush();
	}

	private Transform GetCurrentTransform()
	{
		if (status == null)
		{
			return rootContentTransform;
		}
		if (status.transforms.Count == 0)
		{
			return rootContentTransform;
		}
		return status.transforms.Peek();
	}

	public void PushTransform(Transform trans)
	{
		if (status == null)
		{
			Debug.LogError("Status not found. Credits Display functions should be called after initialization.", this);
		}
		else
		{
			status.transforms.Push(trans);
		}
	}

	public Transform PopTransform()
	{
		if (status == null)
		{
			Debug.LogError("Status not found. Credits Display functions should be called after initialization.", this);
			return null;
		}
		if (status.transforms.Count == 0)
		{
			Debug.LogError("Nothing to pop. Makesure to match push and pop.", this);
			return null;
		}
		return status.transforms.Pop();
	}

	private void Awake()
	{
		if (setupOnAwake)
		{
			ParseAndDisplay();
		}
	}

	private void Reset()
	{
		while (base.transform.childCount > 0)
		{
			Transform child = base.transform.GetChild(0);
			child.SetParent(null);
			if (Application.isPlaying)
			{
				Object.Destroy(child.gameObject);
			}
			else
			{
				Object.DestroyImmediate(child.gameObject);
			}
		}
		status = new GenerationStatus();
	}

	private VerticalEntry BeginVerticalLayout(params string[] args)
	{
		VerticalEntry verticalEntry = Object.Instantiate(verticalPrefab, CurrentTransform);
		verticalEntry.Setup(args);
		verticalEntry.SetLayoutSpacing(mainSpacing);
		PushTransform(verticalEntry.transform);
		return verticalEntry;
	}

	private void EndLayout(params string[] args)
	{
		if (status.activeItem != null)
		{
			EndItem();
		}
		PopTransform();
	}

	private HorizontalEntry BeginHorizontalLayout(params string[] args)
	{
		HorizontalEntry horizontalEntry = Object.Instantiate(horizontalPrefab, CurrentTransform);
		horizontalEntry.Setup(args);
		PushTransform(horizontalEntry.transform);
		return horizontalEntry;
	}
}
