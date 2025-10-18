using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI.Animations;

public class MaterialPropertyFade : FadeElement
{
	[SerializeField]
	private Image renderer;

	[SerializeField]
	private string propertyName = "t";

	[SerializeField]
	private Vector2 propertyRange = new Vector2(0f, 1f);

	[SerializeField]
	private float duration = 0.5f;

	[SerializeField]
	private AnimationCurve showCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	private AnimationCurve hideCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private Material _material;

	public AnimationCurve ShowCurve
	{
		get
		{
			return showCurve;
		}
		set
		{
			showCurve = value;
		}
	}

	public AnimationCurve HideCurve
	{
		get
		{
			return hideCurve;
		}
		set
		{
			hideCurve = value;
		}
	}

	private Material Material
	{
		get
		{
			if (_material == null && renderer != null)
			{
				_material = Object.Instantiate(renderer.material);
				renderer.material = _material;
			}
			return _material;
		}
	}

	public float Duration
	{
		get
		{
			return duration;
		}
		internal set
		{
			duration = value;
		}
	}

	private void Awake()
	{
		if (renderer == null)
		{
			renderer = GetComponent<Image>();
		}
	}

	private void OnDestroy()
	{
		if ((bool)_material)
		{
			Object.Destroy(_material);
		}
	}

	protected override async UniTask HideTask(int token)
	{
		if (Material == null)
		{
			return;
		}
		if (duration <= 0f)
		{
			Material.SetFloat(propertyName, propertyRange.x);
			return;
		}
		float timeWhenFadeBegun = Time.unscaledTime;
		float startingValue = Material.GetFloat(propertyName);
		while (TimeSinceFadeBegun() < duration)
		{
			if (token != base.ActiveTaskToken || Material == null)
			{
				return;
			}
			float time = TimeSinceFadeBegun() / duration;
			Material.SetFloat(propertyName, Mathf.Lerp(startingValue, propertyRange.x, hideCurve.Evaluate(time)));
			await UniTask.NextFrame();
		}
		Material?.SetFloat(propertyName, propertyRange.x);
		float TimeSinceFadeBegun()
		{
			return Time.unscaledTime - timeWhenFadeBegun;
		}
	}

	protected override void OnSkipHide()
	{
		if (!(Material == null))
		{
			Material.SetFloat(propertyName, propertyRange.x);
		}
	}

	protected override void OnSkipShow()
	{
		if (!(Material == null))
		{
			Material.SetFloat(propertyName, propertyRange.y);
		}
	}

	protected override async UniTask ShowTask(int token)
	{
		if (Material == null)
		{
			return;
		}
		if (duration <= 0f)
		{
			Material.SetFloat(propertyName, propertyRange.y);
			return;
		}
		float timeWhenFadeBegun = Time.unscaledTime;
		float startingValue = Material.GetFloat(propertyName);
		while (TimeSinceFadeBegun() < duration)
		{
			if (token != base.ActiveTaskToken || Material == null)
			{
				return;
			}
			float time = TimeSinceFadeBegun() / duration;
			Material.SetFloat(propertyName, Mathf.Lerp(startingValue, propertyRange.y, showCurve.Evaluate(time)));
			await UniTask.NextFrame();
		}
		Material?.SetFloat(propertyName, propertyRange.y);
		float TimeSinceFadeBegun()
		{
			return Time.unscaledTime - timeWhenFadeBegun;
		}
	}
}
