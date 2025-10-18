using ItemStatsSystem.Stats;

namespace Duckov.MiniGames.GoldMiner.Artifacts;

public class GMA_001 : GoldMinerArtifactBehaviour
{
	private Modifier staminaModifier;

	private Modifier scoreFactorModifier;

	private GoldMinerRunData cachedRun;

	protected override void OnAttached(GoldMinerArtifact artifact)
	{
		if (base.Run != null)
		{
			cachedRun = base.Run;
			staminaModifier = new Modifier(ModifierType.Add, 1f, this);
			scoreFactorModifier = new Modifier(ModifierType.Add, 1f, this);
			cachedRun.staminaDrain.AddModifier(staminaModifier);
			cachedRun.scoreFactorBase.AddModifier(scoreFactorModifier);
		}
	}

	protected override void OnDetached(GoldMinerArtifact artifact)
	{
		if (cachedRun != null)
		{
			cachedRun.staminaDrain.RemoveModifier(staminaModifier);
			cachedRun.scoreFactorBase.RemoveModifier(scoreFactorModifier);
		}
	}
}
