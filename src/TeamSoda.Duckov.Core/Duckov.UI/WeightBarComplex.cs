using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Duckov.UI.Animations;
using ItemStatsSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Duckov.UI;

public class WeightBarComplex : MonoBehaviour
{
	[SerializeField]
	private CharacterMainControl target;

	[SerializeField]
	private RectTransform barArea;

	[SerializeField]
	private RectTransform mainBar;

	[SerializeField]
	private Graphic mainBarGraphic;

	[SerializeField]
	private RectTransform positiveBar;

	[SerializeField]
	private RectTransform negativeBar;

	[SerializeField]
	private RectTransform lightMark;

	[SerializeField]
	private RectTransform superHeavyMark;

	[SerializeField]
	private ToggleAnimation lightMarkToggle;

	[SerializeField]
	private ToggleAnimation superHeavyMarkToggle;

	[SerializeField]
	private Color superLightColor;

	[SerializeField]
	private Color lightColor;

	[SerializeField]
	private Color superHeavyColor;

	[SerializeField]
	private Color overweightColor;

	[SerializeField]
	private float animateDuration = 0.1f;

	[SerializeField]
	private AnimationCurve animationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private float targetRealBarTop;

	private int currentToken;

	private CharacterMainControl Target
	{
		get
		{
			if (!target)
			{
				target = LevelManager.Instance?.MainCharacter;
			}
			return target;
		}
	}

	private float LightPercentage => 0.25f;

	private float SuperHeavyPercentage => 0.75f;

	private float MaxWeight
	{
		get
		{
			if (Target == null)
			{
				return 0f;
			}
			return Target.MaxWeight;
		}
	}

	private float BarWidth
	{
		get
		{
			if (barArea == null)
			{
				return 0f;
			}
			return barArea.rect.width;
		}
	}

	private void OnEnable()
	{
		ItemUIUtilities.OnSelectionChanged += OnItemSelectionChanged;
		if ((bool)Target)
		{
			Target.CharacterItem.onChildChanged += OnTargetChildChanged;
		}
		RefreshMarkPositions();
		ResetMainBar();
		Animate().Forget();
	}

	private void OnDisable()
	{
		ItemUIUtilities.OnSelectionChanged -= OnItemSelectionChanged;
		if ((bool)Target)
		{
			Target.CharacterItem.onChildChanged -= OnTargetChildChanged;
		}
	}

	private void RefreshMarkPositions()
	{
		if (!(lightMark == null) && !(superHeavyMark == null))
		{
			float num = BarWidth * LightPercentage;
			float num2 = BarWidth * SuperHeavyPercentage;
			lightMark.anchoredPosition = Vector2.right * num;
			superHeavyMark.anchoredPosition = Vector2.right * num2;
		}
	}

	private void RefreshMarkStatus()
	{
		float num = 0f;
		if (MaxWeight > 0f)
		{
			num = Target.CharacterItem.TotalWeight / MaxWeight;
		}
		lightMarkToggle.SetToggle(num > LightPercentage);
		superHeavyMarkToggle.SetToggle(num > SuperHeavyPercentage);
	}

	private void OnTargetChildChanged(Item item)
	{
		Animate().Forget();
	}

	private void OnItemSelectionChanged()
	{
		Animate().Forget();
	}

	private async UniTask Animate()
	{
		RefreshMarkPositions();
		RefreshMarkStatus();
		ResetChangeBars();
		int token = (currentToken = Random.Range(int.MinValue, int.MaxValue));
		await AnimateMainBar(token);
		RefreshMarkPositions();
		if (token == currentToken)
		{
			UniTask uniTask = AnimatePositiveBar(token);
			UniTask uniTask2 = AnimateNegativeBar(token);
			RefreshMarkPositions();
			await UniTask.WhenAll(uniTask, uniTask2);
			RefreshMarkPositions();
		}
	}

	private void ResetChangeBars()
	{
		positiveBar.DOKill();
		negativeBar.DOKill();
		positiveBar.sizeDelta = new Vector2(positiveBar.sizeDelta.x, 0f);
		negativeBar.sizeDelta = new Vector2(negativeBar.sizeDelta.x, 0f);
	}

	private void ResetMainBar()
	{
		mainBar.DOKill();
		mainBar.sizeDelta = new Vector2(mainBar.sizeDelta.x, 0f);
	}

	private async UniTask AnimateMainBar(int token)
	{
		if (Target == null)
		{
			SetupInvalid();
			return;
		}
		await UniTask.NextFrame();
		if (token != currentToken)
		{
			return;
		}
		mainBar.DOKill();
		if (!(Target == null))
		{
			float totalWeight = Target.CharacterItem.TotalWeight;
			float x = WeightToRectHeight(totalWeight);
			_ = superLightColor;
			float num = 1f;
			if (MaxWeight > 0f)
			{
				num = totalWeight / MaxWeight;
			}
			TweenerCore<Color, Color, ColorOptions> tween = DOTweenModuleUI.DOColor(endValue: (num > 1f) ? overweightColor : ((num > SuperHeavyPercentage) ? superHeavyColor : ((!(num > LightPercentage)) ? superLightColor : lightColor)), target: mainBarGraphic, duration: animateDuration);
			TweenerCore<Vector2, Vector2, VectorOptions> tween2 = mainBar.DOSizeDelta(new Vector2(x, mainBar.sizeDelta.y), animateDuration).SetEase(animationCurve);
			await UniTask.WhenAll(tween.ToUniTask(), tween2.ToUniTask());
		}
	}

	private async UniTask AnimatePositiveBar(int token)
	{
		if (token == currentToken)
		{
			Item selectedItem = ItemUIUtilities.SelectedItem;
			float x = 0f;
			if (selectedItem != null && !selectedItem.IsInPlayerCharacter())
			{
				x = WeightToRectHeight(selectedItem.TotalWeight);
			}
			positiveBar.DOKill();
			await positiveBar.DOSizeDelta(new Vector2(x, positiveBar.sizeDelta.y), animateDuration).SetEase(animationCurve);
		}
	}

	private async UniTask AnimateNegativeBar(int token)
	{
		if (token == currentToken)
		{
			Item selectedItem = ItemUIUtilities.SelectedItem;
			float x = 0f;
			if (selectedItem != null && selectedItem.IsInPlayerCharacter())
			{
				x = WeightToRectHeight(selectedItem.TotalWeight);
			}
			negativeBar.DOKill();
			await negativeBar.DOSizeDelta(new Vector2(x, negativeBar.sizeDelta.y), animateDuration).SetEase(animationCurve);
		}
	}

	private void SetupInvalid()
	{
		SetSizeDeltaY(mainBar, 0f);
		SetSizeDeltaY(positiveBar, 0f);
		SetSizeDeltaY(negativeBar, 0f);
	}

	private static void SetSizeDeltaY(RectTransform rectTransform, float sizeDelta)
	{
		Vector2 sizeDelta2 = rectTransform.sizeDelta;
		sizeDelta2.y = sizeDelta;
		rectTransform.sizeDelta = sizeDelta2;
	}

	private static float GetSizeDeltaY(RectTransform rectTransform)
	{
		return rectTransform.sizeDelta.y;
	}

	private float WeightToRectHeight(float weight)
	{
		if (MaxWeight <= 0f)
		{
			return 0f;
		}
		float num = weight / MaxWeight;
		return BarWidth * num;
	}
}
