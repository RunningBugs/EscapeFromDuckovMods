using Duckov.UI.Animations;
using TMPro;
using UnityEngine;

public class CostTakerHUD_Entry : MonoBehaviour
{
	private RectTransform rectTransform;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private CostDisplay costDisplay;

	[SerializeField]
	private FadeGroup fadeGroup;

	private const float HideDistance = 10f;

	private const float HideDistanceYLimit = 2.5f;

	public CostTaker Target { get; private set; }

	private void Awake()
	{
		rectTransform = base.transform as RectTransform;
	}

	private void LateUpdate()
	{
		UpdatePosition();
		UpdateFadeGroup();
	}

	internal void Setup(CostTaker cur)
	{
		Target = cur;
		nameText.text = cur.InteractName;
		costDisplay.Setup(cur.Cost);
		UpdatePosition();
	}

	private void UpdatePosition()
	{
		rectTransform.MatchWorldPosition(Target.transform.TransformPoint(Target.interactMarkerOffset), Vector3.up * 0.5f);
	}

	private void UpdateFadeGroup()
	{
		CharacterMainControl main = CharacterMainControl.Main;
		bool flag = false;
		if (!(Target == null) && !(main == null))
		{
			Vector3 vector = main.transform.position - Target.transform.position;
			if (!(Mathf.Abs(vector.y) > 2.5f) && !(vector.magnitude > 10f))
			{
				flag = true;
			}
		}
		if (flag && !fadeGroup.IsShown)
		{
			fadeGroup.Show();
		}
		else if (!flag && fadeGroup.IsShown)
		{
			fadeGroup.Hide();
		}
	}
}
