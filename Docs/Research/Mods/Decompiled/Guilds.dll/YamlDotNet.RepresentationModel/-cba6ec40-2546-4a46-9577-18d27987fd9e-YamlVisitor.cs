using System;
using System.Collections.Generic;

namespace YamlDotNet.RepresentationModel;

[Obsolete("Use YamlVisitorBase")]
internal abstract class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlVisitor : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlVisitor
{
	protected virtual void Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlStream stream)
	{
	}

	protected virtual void Visited(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlStream stream)
	{
	}

	protected virtual void Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument document)
	{
	}

	protected virtual void Visited(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument document)
	{
	}

	protected virtual void Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode scalar)
	{
	}

	protected virtual void Visited(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode scalar)
	{
	}

	protected virtual void Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode sequence)
	{
	}

	protected virtual void Visited(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode sequence)
	{
	}

	protected virtual void Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode mapping)
	{
	}

	protected virtual void Visited(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode mapping)
	{
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
			child.Key.Accept(this);
			child.Value.Accept(this);
		}
	}

	void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlVisitor.Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlStream stream)
	{
		Visit(stream);
		VisitChildren(stream);
		Visited(stream);
	}

	void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlVisitor.Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument document)
	{
		Visit(document);
		VisitChildren(document);
		Visited(document);
	}

	void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlVisitor.Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode scalar)
	{
		Visit(scalar);
		Visited(scalar);
	}

	void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlVisitor.Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode sequence)
	{
		Visit(sequence);
		VisitChildren(sequence);
		Visited(sequence);
	}

	void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlVisitor.Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode mapping)
	{
		Visit(mapping);
		VisitChildren(mapping);
		Visited(mapping);
	}
}
