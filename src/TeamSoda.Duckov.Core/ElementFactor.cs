using System;

[Serializable]
public struct ElementFactor
{
	public ElementTypes elementType;

	public float factor;

	public ElementFactor(ElementTypes _type, float _factor)
	{
		elementType = _type;
		factor = _factor;
	}
}
