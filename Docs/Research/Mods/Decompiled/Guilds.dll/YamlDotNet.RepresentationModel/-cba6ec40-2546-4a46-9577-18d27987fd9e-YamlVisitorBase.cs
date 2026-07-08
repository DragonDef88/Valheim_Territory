using System.Collections.Generic;

namespace YamlDotNet.RepresentationModel;

internal abstract class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlVisitorBase : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlVisitor
{
	public virtual void Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlStream stream)
	{
		VisitChildren(stream);
	}

	public virtual void Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument document)
	{
		VisitChildren(document);
	}

	public virtual void Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode scalar)
	{
	}

	public virtual void Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode sequence)
	{
		VisitChildren(sequence);
	}

	public virtual void Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode mapping)
	{
		VisitChildren(mapping);
	}

	protected virtual void VisitPair(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode key, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode value)
	{
		key.Accept(this);
		value.Accept(this);
	}

	protected virtual void VisitChildren(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlStream stream)
	{
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument document in stream.Documents)
		{
			document.Accept(this);
		}
	}

	protected virtual void VisitChildren(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument document)
	{
		if (document.RootNode != null)
		{
			document.RootNode.Accept(this);
		}
	}

	protected virtual void VisitChildren(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode sequence)
	{
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode child in sequence.Children)
		{
			child.Accept(this);
		}
	}

	protected virtual void VisitChildren(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode mapping)
	{
		foreach (KeyValuePair<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> child in mapping.Children)
		{
			VisitPair(child.Key, child.Value);
		}
	}
}
