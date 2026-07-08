using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core.Events;

namespace YamlDotNet.Core;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMergingParser : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser
{
	private sealed class ParsingEventCollection : IEnumerable<LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>>, IEnumerable
	{
		private readonly LinkedList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> events;

		private readonly HashSet<LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>> deleted;

		private readonly Dictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName, LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>> references;

		public ParsingEventCollection()
		{
			events = new LinkedList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>();
			deleted = new HashSet<LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>>();
			references = new Dictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName, LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>>();
		}

		public void AddAfter(LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> node, IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> items)
		{
			foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent item in items)
			{
				node = events.AddAfter(node, item);
			}
		}

		public void Add(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent item)
		{
			LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> node = events.AddLast(item);
			AddReference(item, node);
		}

		public void MarkDeleted(LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> node)
		{
			deleted.Add(node);
		}

		public bool IsDeleted(LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> node)
		{
			return deleted.Contains(node);
		}

		public void CleanMarked()
		{
			foreach (LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> item in deleted)
			{
				events.Remove(item);
			}
		}

		public IEnumerable<LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>> FromAnchor(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName anchor)
		{
			LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> next = references[anchor].Next;
			return Enumerate(next);
		}

		public IEnumerator<LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>> GetEnumerator()
		{
			return Enumerate(events.First).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private static IEnumerable<LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>> Enumerate(LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>? node)
		{
			while (node != null)
			{
				yield return node;
				node = node.Next;
			}
		}

		private void AddReference(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent item, LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> node)
		{
			if (item is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart { Anchor: { IsEmpty: false } anchor })
			{
				references[anchor] = node;
			}
		}
	}

	private sealed class ParsingEventCloner : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParsingEventVisitor
	{
		private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent? clonedEvent;

		public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent Clone(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent e)
		{
			e.Accept(this);
			if (clonedEvent == null)
			{
				throw new InvalidOperationException($"Could not clone event of type '{e.Type}'");
			}
			return clonedEvent;
		}

		void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParsingEventVisitor.Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias e)
		{
			clonedEvent = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias(e.Value, e.Start, e.End);
		}

		void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParsingEventVisitor.Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamStart e)
		{
			throw new NotSupportedException();
		}

		void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParsingEventVisitor.Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd e)
		{
			throw new NotSupportedException();
		}

		void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParsingEventVisitor.Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart e)
		{
			throw new NotSupportedException();
		}

		void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParsingEventVisitor.Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd e)
		{
			throw new NotSupportedException();
		}

		void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParsingEventVisitor.Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar e)
		{
			clonedEvent = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName.Empty, e.Tag, e.Value, e.Style, e.IsPlainImplicit, e.IsQuotedImplicit, e.Start, e.End);
		}

		void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParsingEventVisitor.Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart e)
		{
			clonedEvent = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName.Empty, e.Tag, e.IsImplicit, e.Style, e.Start, e.End);
		}

		void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParsingEventVisitor.Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceEnd e)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = e.Start;
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = e.End;
			clonedEvent = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceEnd(in start, in end);
		}

		void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParsingEventVisitor.Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart e)
		{
			clonedEvent = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName.Empty, e.Tag, e.IsImplicit, e.Style, e.Start, e.End);
		}

		void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParsingEventVisitor.Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd e)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = e.Start;
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = e.End;
			clonedEvent = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd(in start, in end);
		}

		void _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParsingEventVisitor.Visit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EComment e)
		{
			throw new NotSupportedException();
		}
	}

	private readonly ParsingEventCollection events;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser innerParser;

	private IEnumerator<LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>> iterator;

	private bool merged;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent? Current => iterator.Current?.Value;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMergingParser(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser innerParser)
	{
		events = new ParsingEventCollection();
		merged = false;
		iterator = events.GetEnumerator();
		this.innerParser = innerParser;
	}

	public bool MoveNext()
	{
		if (!merged)
		{
			Merge();
			events.CleanMarked();
			iterator = events.GetEnumerator();
			merged = true;
		}
		return iterator.MoveNext();
	}

	private void Merge()
	{
		while (innerParser.MoveNext())
		{
			events.Add(innerParser.Current);
		}
		foreach (LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> @event in events)
		{
			if (IsMergeToken(@event))
			{
				events.MarkDeleted(@event);
				if (!HandleMerge(@event.Next))
				{
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = @event.Value.Start;
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = @event.Value.End;
					throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start, in end, "Unrecognized merge key pattern");
				}
			}
		}
	}

	private bool HandleMerge(LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>? node)
	{
		if (node == null)
		{
			return false;
		}
		if (node.Value is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias anchorAlias)
		{
			return HandleAnchorAlias(node, node, anchorAlias);
		}
		if (node.Value is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart)
		{
			return HandleSequence(node);
		}
		return false;
	}

	private bool HandleMergeSequence(LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> sequenceStart, LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>? node)
	{
		if (node == null)
		{
			return false;
		}
		if (node.Value is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias anchorAlias)
		{
			return HandleAnchorAlias(sequenceStart, node, anchorAlias);
		}
		if (node.Value is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart)
		{
			return HandleSequence(node);
		}
		return false;
	}

	private static bool IsMergeToken(LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> node)
	{
		if (node.Value is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar)
		{
			return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.Value == "<<";
		}
		return false;
	}

	private bool HandleAnchorAlias(LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> node, LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> anchorNode, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias anchorAlias)
	{
		IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> mappingEvents = GetMappingEvents(anchorAlias.Value);
		events.AddAfter(node, mappingEvents);
		events.MarkDeleted(anchorNode);
		return true;
	}

	private bool HandleSequence(LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> node)
	{
		events.MarkDeleted(node);
		LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> linkedListNode = node;
		while (linkedListNode != null)
		{
			if (linkedListNode.Value is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceEnd)
			{
				events.MarkDeleted(linkedListNode);
				return true;
			}
			LinkedListNode<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> next = linkedListNode.Next;
			HandleMergeSequence(node, next);
			linkedListNode = next;
		}
		return true;
	}

	private IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> GetMappingEvents(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName anchor)
	{
		ParsingEventCloner @object = new ParsingEventCloner();
		int nesting = 0;
		return (from e in events.FromAnchor(anchor)
			where !events.IsDeleted(e)
			select e.Value).TakeWhile((_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent e) => (nesting += e.NestingIncrease) >= 0).Select(@object.Clone);
	}
}
