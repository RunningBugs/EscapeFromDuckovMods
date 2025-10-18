using System;
using UnityEngine;

public class MiningMachineCardDisplay : MonoBehaviour
{
	public enum CardTypes
	{
		normal,
		potato
	}

	public GameObject activeVisual;

	public GameObject deactiveVisual;

	public GameObject normalGPU;

	public GameObject potatoGPU;

	public void SetVisualActive(bool active, CardTypes cardType)
	{
		activeVisual.SetActive(active);
		deactiveVisual.SetActive(!active);
		switch (cardType)
		{
		case CardTypes.normal:
			normalGPU.SetActive(value: true);
			potatoGPU.SetActive(value: false);
			break;
		case CardTypes.potato:
			normalGPU.SetActive(value: false);
			potatoGPU.SetActive(value: true);
			break;
		default:
			throw new ArgumentOutOfRangeException("cardType", cardType, null);
		}
	}
}
