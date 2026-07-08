using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Guilds;

[PublicAPI]
public class AchievementData
{
	public float? progress = 0f;

	public List<DateTime> completed = new List<DateTime>();
}
