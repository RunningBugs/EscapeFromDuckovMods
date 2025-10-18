using System;

[Serializable]
public struct SkillContext
{
	public float castRange;

	public bool movableWhileAim;

	public float skillReadyTime;

	public float effectRange;

	public bool isGrenade;

	public float grenageVerticleSpeed;

	public bool checkObsticle;

	public bool releaseOnStartAim;
}
