using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Core;

namespace YamlDotNet.RepresentationModel;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState
{
	private readonly Dictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> anchors = new Dictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode>();

	private readonly List<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> nodesWithUnresolvedAliases = new List<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode>();

	public void AddAnchor(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode node)
	{
		if (node.Anchor.IsEmpty)
		{
			throw new ArgumentException("The specified node does not have an anchor");
		}
		anchors[node.Anchor] = node;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode GetNode(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName anchor, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end)
	{
		if (anchors.TryGetValue(anchor, out _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode value))
		{
			return value;
		}
		throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorNotFoundException(in start, in end, $"The anchor '{anchor}' does not exists");
	}

	public bool TryGetNode(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName anchor, [_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENotNullWhen(true)] out _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode? node)
	{
		return anchors.TryGetValue(anchor, out node);
	}

	public void AddNodeWithUnresolvedAliases(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode node)
	{
		nodesWithUnresolvedAliases.Add(node);
	}

	public void ResolveAliases()
	{
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode nodesWithUnresolvedAlias in nodesWithUnresolvedAliases)
		{
			nodesWithUnresolvedAlias.ResolveAliases(this);
		}
	}
}
