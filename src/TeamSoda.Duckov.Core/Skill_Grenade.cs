using Duckov.Utilities;
using ItemStatsSystem;
using UnityEngine;

public class Skill_Grenade : SkillBase
{
	public bool canControlCastDistance = true;

	public float delay = 1f;

	public bool delayFromCollide;

	public Grenade grenadePfb;

	public bool createPickup;

	public bool isLandmine;

	public float landmineTriggerRange = 0.5f;

	public bool createExplosion = true;

	public bool canHurtSelf = true;

	[Range(0f, 1f)]
	public float explosionShakeStrength = 1f;

	public DamageInfo damageInfo;

	public int blastCount = 1;

	public float blastAngle;

	[Tooltip("当有多个手雷时，delay的间隔")]
	public float blastDelayTimeSpace = 0.2f;

	public override void OnRelease()
	{
		if (fromCharacter == null)
		{
			return;
		}
		Vector3 position = fromCharacter.CurrentUsingAimSocket.position;
		Vector3 releasePoint = skillReleaseContext.releasePoint;
		float y = releasePoint.y;
		Vector3 vector = releasePoint - fromCharacter.transform.position;
		vector.y = 0f;
		float num = vector.magnitude;
		if (!canControlCastDistance)
		{
			num = skillContext.castRange;
		}
		vector.Normalize();
		float num2 = 0f;
		if (blastCount <= 1)
		{
			blastCount = 1;
		}
		if (blastCount > 1)
		{
			num2 = ((!(blastAngle < 359f)) ? (blastAngle / (float)blastCount) : (blastAngle / (float)(blastCount - 1)));
		}
		Debug.Log($"castDistance:{num}");
		for (int i = 0; i < blastCount; i++)
		{
			Vector3 vector2 = Quaternion.Euler(0f, (0f - blastAngle) * 0.5f + num2 * (float)i, 0f) * vector;
			Vector3 target = position + vector2 * num;
			target.y = y;
			Grenade grenade = Object.Instantiate(grenadePfb, position, fromCharacter.CurrentUsingAimSocket.rotation);
			damageInfo.fromCharacter = fromCharacter;
			grenade.damageInfo = damageInfo;
			Vector3 velocity = CalculateVelocity(position, target, skillContext.grenageVerticleSpeed);
			grenade.createExplosion = createExplosion;
			grenade.explosionShakeStrength = explosionShakeStrength;
			grenade.damageRange = skillContext.effectRange;
			grenade.delayFromCollide = delayFromCollide;
			grenade.delayTime = delay + blastDelayTimeSpace * (float)i;
			grenade.isLandmine = isLandmine;
			grenade.landmineTriggerRange = landmineTriggerRange;
			grenade.Launch(position, velocity, fromCharacter, canHurtSelf);
			if (fromItem != null)
			{
				grenade.SetWeaponIdInfo(fromItem.TypeID);
			}
			if (i == 0 && createPickup && fromItem != null)
			{
				Debug.Log("CreatePickup");
				fromItem.Detach();
				fromItem.AgentUtilities.ReleaseActiveAgent();
				ItemAgent itemAgent = fromItem.AgentUtilities.CreateAgent(GameplayDataSettings.Prefabs.PickupAgentNoRendererPrefab, ItemAgent.AgentTypes.pickUp);
				Debug.Log("newAgent Created:" + itemAgent.name);
				grenade.BindAgent(itemAgent);
			}
		}
	}

	public Vector3 CalculateVelocity(Vector3 start, Vector3 target, float verticleSpeed)
	{
		float num = Physics.gravity.magnitude;
		if (num <= 0f)
		{
			num = 1f;
		}
		float num2 = verticleSpeed / num;
		float num3 = Mathf.Sqrt(2f * Mathf.Abs(num2 * verticleSpeed * 0.5f + start.y - target.y) / num);
		float num4 = num2 + num3;
		if (num4 <= 0f)
		{
			num4 = 0.001f;
		}
		Vector3 vector = start;
		vector.y = 0f;
		Vector3 vector2 = target;
		vector2.y = 0f;
		float num5 = Vector3.Distance(vector, vector2);
		float num6 = 0f;
		Vector3 vector3 = vector2 - vector;
		if (vector3.magnitude > 0f)
		{
			vector3 = vector3.normalized;
			num6 = num5 / num4;
		}
		else
		{
			vector3 = Vector3.zero;
		}
		return vector3 * num6 + Vector3.up * verticleSpeed;
	}
}
