using System;

namespace YamlDotNet.Core;

internal sealed class EmitterSettings
{
	public static readonly EmitterSettings Default = new EmitterSettings();

	public int BestIndent { get; } = 2;


	public int BestWidth { get; } = int.MaxValue;


	public bool IsCanonical { get; }

	public bool SkipAnchorName { get; private set; }

	public int MaxSimpleKeyLength { get; } = 1024;


	public bool IndentSequences { get; }

	public EmitterSettings()
	{
	}

	public EmitterSettings(int bestIndent, int bestWidth, bool isCanonical, int maxSimpleKeyLength, bool skipAnchorName = false, bool indentSequences = false)
	{
		if (bestIndent < 2 || bestIndent > 9)
		{
			throw new ArgumentOutOfRangeException("bestIndent", "BestIndent must be between 2 and 9, inclusive");
		}
		if (bestWidth <= bestIndent * 2)
		{
			throw new ArgumentOutOfRangeException("bestWidth", "BestWidth must be greater than BestIndent x 2.");
		}
		if (maxSimpleKeyLength < 0)
		{
			throw new ArgumentOutOfRangeException("maxSimpleKeyLength", "MaxSimpleKeyLength must be >= 0");
		}
		BestIndent = bestIndent;
		BestWidth = bestWidth;
		IsCanonical = isCanonical;
		MaxSimpleKeyLength = maxSimpleKeyLength;
		SkipAnchorName = skipAnchorName;
		IndentSequences = indentSequences;
	}

	public EmitterSettings WithBestIndent(int bestIndent)
	{
		return new EmitterSettings(bestIndent, BestWidth, IsCanonical, MaxSimpleKeyLength, SkipAnchorName);
	}

	public EmitterSettings WithBestWidth(int bestWidth)
	{
		return new EmitterSettings(BestIndent, bestWidth, IsCanonical, MaxSimpleKeyLength, SkipAnchorName);
	}

	public EmitterSettings WithMaxSimpleKeyLength(int maxSimpleKeyLength)
	{
		return new EmitterSettings(BestIndent, BestWidth, IsCanonical, maxSimpleKeyLength, SkipAnchorName);
	}

	public EmitterSettings Canonical()
	{
		return new EmitterSettings(BestIndent, BestWidth, isCanonical: true, MaxSimpleKeyLength, SkipAnchorName);
	}

	public EmitterSettings WithoutAnchorName()
	{
		return new EmitterSettings(BestIndent, BestWidth, IsCanonical, MaxSimpleKeyLength, skipAnchorName: true);
	}

	public EmitterSettings WithIndentedSequences()
	{
		return new EmitterSettings(BestIndent, BestWidth, IsCanonical, MaxSimpleKeyLength, SkipAnchorName, indentSequences: true);
	}
}
