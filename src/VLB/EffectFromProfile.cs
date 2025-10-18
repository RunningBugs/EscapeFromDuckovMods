using UnityEngine;

namespace VLB;

[HelpURL("http://saladgamer.com/vlb-doc/comp-effect-from-profile/")]
[AddComponentMenu("VLB/Common/Effect From Profile")]
public class EffectFromProfile : MonoBehaviour
{
	public const string ClassName = "EffectFromProfile";

	[SerializeField]
	private EffectAbstractBase m_EffectProfile;

	private EffectAbstractBase m_EffectInstance;

	public EffectAbstractBase effectProfile
	{
		get
		{
			return m_EffectProfile;
		}
		set
		{
			m_EffectProfile = value;
			InitInstanceFromProfile();
		}
	}

	public void InitInstanceFromProfile()
	{
		if ((bool)m_EffectInstance)
		{
			if ((bool)m_EffectProfile)
			{
				m_EffectInstance.InitFrom(m_EffectProfile);
			}
			else
			{
				m_EffectInstance.enabled = false;
			}
		}
	}

	private void OnEnable()
	{
		if ((bool)m_EffectInstance)
		{
			m_EffectInstance.enabled = true;
		}
		else if ((bool)m_EffectProfile)
		{
			m_EffectInstance = base.gameObject.AddComponent(m_EffectProfile.GetType()) as EffectAbstractBase;
			InitInstanceFromProfile();
		}
	}

	private void OnDisable()
	{
		if ((bool)m_EffectInstance)
		{
			m_EffectInstance.enabled = false;
		}
	}
}
