using System;
using System.Collections.Generic;
using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.RepresentationModel;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument
{
	private class AnchorAssigningVisitor : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlVisitorBase
	{
		private readonly HashSet<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName> existingAnchors = new HashSet<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName>();

		private readonly Dictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, bool> visitedNodes = new Dictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, bool>();

		public void AssignAnchors(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument document)
		{
			existingAnchors.Clear();
			visitedNodes.Clear();
			document.Accept(this);
			Random random = new Random();
			foreach (KeyValuePair<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode, bool> visitedNode in visitedNodes)
			{
				if (!visitedNode.Value)
				{
					continue;
				}
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName;
				if (!visitedNode.Key.Anchor.IsEmpty && !existingAnchors.Contains(visitedNode.Key.Anchor))
				{
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName = visitedNode.Key.Anchor;
				}
				else
				{
					do
					{
						_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName(random.Next().ToString(CultureInfo.InvariantCulture));
					}
					while (existingAnchors.Contains(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName));
				}
				existingAnchors.Add(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName);
				visitedNode.Key.Anchor = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName;
			}
		}

		private bool VisitNodeAndFindDuplicates(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode node)
		{
			if (visitedNodes.TryGetValue(node, out var value))
			{
				if (!value)
				{
					visitedNodes[node] = true;
				}
				return !value;
			}
			visitedNodes.Add(node, value: false);
			return false;
		}

		public override void Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode scalar)
		{
			VisitNodeAndFindDuplicates(scalar);
		}

		public override void Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlMappingNode mapping)
		{
			if (!VisitNodeAndFindDuplicates(mapping))
			{
				base.Visit(mapping);
			}
		}

		public override void Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSequenceNode sequence)
		{
			if (!VisitNodeAndFindDuplicates(sequence))
			{
				base.Visit(sequence);
			}
		}
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode RootNode { get; private set; }

	public IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode> AllNodes => RootNode.AllNodes;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode rootNode)
	{
		RootNode = rootNode;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument(string rootNode)
	{
		RootNode = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlScalarNode(rootNode);
	}

	internal _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState2 = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState();
		parser.Consume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart>();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd @event;
		while (!parser.TryConsume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd>(out @event))
		{
			RootNode = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlNode.ParseNode(parser, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState2);
			if (RootNode is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAliasNode)
			{
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException("A document cannot contain only an alias");
			}
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentLoadingState2.ResolveAliases();
		if (RootNode == null)
		{
			throw new ArgumentException("Atempted to parse an empty document");
		}
	}

	private void AssignAnchors()
	{
		AnchorAssigningVisitor anchorAssigningVisitor = new AnchorAssigningVisitor();
		anchorAssigningVisitor.AssignAnchors(this);
	}

	internal void Save(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter, bool assignAnchors = true)
	{
		if (assignAnchors)
		{
			AssignAnchors();
		}
		emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart());
		RootNode.Save(emitter, new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState());
		emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd(isImplicit: false));
	}

	public void Accept(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlVisitor visitor)
	{
		visitor.Visit(this);
	}
}
