using JetBrains.Annotations;

namespace StatManager;

[PublicAPI]
internal static class StatExtensions
{
	public static void IncrementPlayerStat(this Game game, string name, float amount = 1f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		game.IncrementPlayerStat(Stat.fromName(name), 1f);
	}

	public static float GetPlayerStat(this PlayerStats playerStats, string name)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return playerStats[Stat.fromName(name)];
	}

	public static float GetPlayerStat(this Game game, PlayerStatType statType)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return game.GetPlayerProfile().m_playerStats[statType];
	}

	public static float GetPlayerStat(this Game game, string name)
	{
		return game.GetPlayerProfile().m_playerStats.GetPlayerStat(name);
	}
}
