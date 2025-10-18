using TMPro;
using UnityEngine;

public class DPSDisplayer : MonoBehaviour
{
	[SerializeField]
	private TextMeshPro dpsText;

	private bool empty;

	private float totalDamage;

	private float firstTimeMarker;

	private float lastTimeMarker;

	private void Awake()
	{
		Health.OnHurt += OnHurt;
	}

	private void Update()
	{
		if (Time.time - lastTimeMarker > 3f)
		{
			empty = true;
			totalDamage = 0f;
			RefreshDisplay();
		}
	}

	private void OnDestroy()
	{
		Health.OnHurt -= OnHurt;
	}

	private void OnHurt(Health health, DamageInfo dmgInfo)
	{
		if ((bool)dmgInfo.fromCharacter && dmgInfo.fromCharacter.IsMainCharacter)
		{
			totalDamage += dmgInfo.finalDamage;
			if (empty)
			{
				firstTimeMarker = Time.time;
				lastTimeMarker = Time.time;
				empty = false;
			}
			else
			{
				lastTimeMarker = Time.time;
				RefreshDisplay();
			}
		}
	}

	private void RefreshDisplay()
	{
		float num = CalculateDPS();
		dpsText.text = num.ToString("00000");
	}

	private float CalculateDPS()
	{
		if (empty)
		{
			return 0f;
		}
		float num = lastTimeMarker - firstTimeMarker;
		if (num <= 0f)
		{
			return 0f;
		}
		return totalDamage / num;
	}
}
