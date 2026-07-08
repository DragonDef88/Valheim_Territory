using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core.Events;
using YamlDotNet.Core.Tokens;

namespace YamlDotNet.Core;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParser : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser
{
	private class EventQueue
	{
		private readonly Queue<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> highPriorityEvents = new Queue<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>();

		private readonly Queue<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> normalPriorityEvents = new Queue<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>();

		public int Count => highPriorityEvents.Count + normalPriorityEvents.Count;

		public void Enqueue(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent @event)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType type = @event.Type;
			if (type == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.StreamStart || type == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.DocumentStart)
			{
				highPriorityEvents.Enqueue(@event);
			}
			else
			{
				normalPriorityEvents.Enqueue(@event);
			}
		}

		public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent Dequeue()
		{
			if (highPriorityEvents.Count <= 0)
			{
				return normalPriorityEvents.Dequeue();
			}
			return highPriorityEvents.Dequeue();
		}
	}

	private readonly Stack<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState> states = new Stack<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState>();

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection tagDirectives = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection();

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState state;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIScanner scanner;

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken? currentToken;

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective? version;

	private readonly EventQueue pendingEvents = new EventQueue();

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent? Current { get; private set; }

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken? GetCurrentToken()
	{
		if (currentToken == null)
		{
			while (scanner.MoveNextWithoutConsuming())
			{
				currentToken = scanner.Current;
				if (!(currentToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EComment _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EComment))
				{
					break;
				}
				pendingEvents.Enqueue(new YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EComment(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EComment.Value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EComment.IsInline, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EComment.Start, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EComment.End));
				scanner.ConsumeCurrent();
			}
		}
		return currentToken;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParser(TextReader input)
		: this(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScanner(input))
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParser(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIScanner scanner)
	{
		this.scanner = scanner;
	}

	public bool MoveNext()
	{
		if (state == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.StreamEnd)
		{
			Current = null;
			return false;
		}
		if (pendingEvents.Count == 0)
		{
			pendingEvents.Enqueue(StateMachine());
		}
		Current = pendingEvents.Dequeue();
		return true;
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent StateMachine()
	{
		return state switch
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.StreamStart => ParseStreamStart(), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.ImplicitDocumentStart => ParseDocumentStart(isImplicit: true), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.DocumentStart => ParseDocumentStart(isImplicit: false), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.DocumentContent => ParseDocumentContent(), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.DocumentEnd => ParseDocumentEnd(), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.BlockNode => ParseNode(isBlock: true, isIndentlessSequence: false), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.BlockNodeOrIndentlessSequence => ParseNode(isBlock: true, isIndentlessSequence: true), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowNode => ParseNode(isBlock: false, isIndentlessSequence: false), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.BlockSequenceFirstEntry => ParseBlockSequenceEntry(isFirst: true), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.BlockSequenceEntry => ParseBlockSequenceEntry(isFirst: false), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.IndentlessSequenceEntry => ParseIndentlessSequenceEntry(), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.BlockMappingFirstKey => ParseBlockMappingKey(isFirst: true), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.BlockMappingKey => ParseBlockMappingKey(isFirst: false), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.BlockMappingValue => ParseBlockMappingValue(), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowSequenceFirstEntry => ParseFlowSequenceEntry(isFirst: true), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowSequenceEntry => ParseFlowSequenceEntry(isFirst: false), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowSequenceEntryMappingKey => ParseFlowSequenceEntryMappingKey(), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowSequenceEntryMappingValue => ParseFlowSequenceEntryMappingValue(), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowSequenceEntryMappingEnd => ParseFlowSequenceEntryMappingEnd(), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowMappingFirstKey => ParseFlowMappingKey(isFirst: true), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowMappingKey => ParseFlowMappingKey(isFirst: false), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowMappingValue => ParseFlowMappingValue(isEmpty: false), 
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowMappingEmptyValue => ParseFlowMappingValue(isEmpty: true), 
			_ => throw new InvalidOperationException(), 
		};
	}

	private void Skip()
	{
		if (currentToken != null)
		{
			currentToken = null;
			scanner.ConsumeCurrent();
		}
	}

	private YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamStart ParseStreamStart()
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end;
		if (!(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamStart _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamStart))
		{
			start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.Start ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
			end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.End ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start, in end, "Did not find expected <stream-start>.");
		}
		Skip();
		state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.ImplicitDocumentStart;
		start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamStart.Start;
		end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamStart.End;
		return new YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamStart(in start, in end);
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent ParseDocumentStart(bool isImplicit)
	{
		if (currentToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective)
		{
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException("While parsing a document start node, could not find document end marker before version directive.");
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
		if (!isImplicit)
		{
			while (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd)
			{
				Skip();
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
			}
		}
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken == null)
		{
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException("Reached the end of the stream while parsing a document start.");
		}
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar && (state == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.ImplicitDocumentStart || state == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.DocumentStart))
		{
			isImplicit = true;
		}
		if ((isImplicit && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd)) || _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockMappingStart)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection tags = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection();
			ProcessDirectives(tags);
			states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.DocumentEnd);
			state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.BlockNode;
			return new YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart(null, tags, isImplicit: true, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken.Start, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken.End);
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start2;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end;
		if (!(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd))
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken.Start;
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection tags2 = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection();
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective = ProcessDirectives(tags2);
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken() ?? throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException("Reached the end of the stream while parsing a document start");
			if (!(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart))
			{
				start2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken.Start;
				end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken.End;
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start2, in end, "Did not find expected <document start>.");
			}
			states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.DocumentEnd);
			state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.DocumentContent;
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken.End;
			Skip();
			return new YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective, tags2, isImplicit: false, start, end2);
		}
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd)
		{
			Skip();
		}
		state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.StreamEnd;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken() ?? throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException("Reached the end of the stream while parsing a document start");
		start2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken.Start;
		end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken.End;
		YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd result = new YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd(in start2, in end);
		if (scanner.MoveNextWithoutConsuming())
		{
			throw new InvalidOperationException("The scanner should contain no more tokens.");
		}
		return result;
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective? ProcessDirectives(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection tags)
	{
		bool flag = false;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective result = null;
		while (true)
		{
			if (GetCurrentToken() is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective)
			{
				if (version != null)
				{
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective.Start;
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective.End;
					throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start, in end, "Found duplicate %YAML directive.");
				}
				if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective.Version.Major != 1 || _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective.Version.Minor > 3)
				{
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective.Start;
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective.End;
					throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start, in end, "Found incompatible YAML document.");
				}
				result = (version = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective);
				flag = true;
			}
			else
			{
				if (!(GetCurrentToken() is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective))
				{
					break;
				}
				if (tags.Contains(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective.Handle))
				{
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective.Start;
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective.End;
					throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start, in end, "Found duplicate %TAG directive.");
				}
				tags.Add(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective);
				flag = true;
			}
			Skip();
		}
		if (GetCurrentToken() is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart && (version == null || (version.Version.Major == 1 && version.Version.Minor > 1)))
		{
			if (GetCurrentToken() is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart && version == null)
			{
				version = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersion(1, 2));
			}
			flag = true;
		}
		AddTagDirectives(tags, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EConstants.DefaultTagDirectives);
		if (flag)
		{
			tagDirectives.Clear();
		}
		AddTagDirectives(tagDirectives, tags);
		return result;
	}

	private static void AddTagDirectives(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection directives, IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective> source)
	{
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective item in source)
		{
			if (!directives.Contains(item))
			{
				directives.Add(item);
			}
		}
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent ParseDocumentContent()
	{
		if (GetCurrentToken() is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective || GetCurrentToken() is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective || GetCurrentToken() is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart || GetCurrentToken() is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd || GetCurrentToken() is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd)
		{
			state = states.Pop();
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark position = scanner.CurrentPosition;
			return ProcessEmptyScalar(in position);
		}
		return ParseNode(isBlock: true, isIndentlessSequence: false);
	}

	private static YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar ProcessEmptyScalar(in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark position)
	{
		return new YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName.Empty, string.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Plain, isPlainImplicit: true, isQuotedImplicit: false, position, position);
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent ParseNode(bool isBlock, bool isIndentlessSequence)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end;
		if (GetCurrentToken() is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError { Start: var start } _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError)
		{
			end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError.End;
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start, in end, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError.Value);
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken() ?? throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException("Reached the end of the stream while parsing a node");
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias)
		{
			state = states.Pop();
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent result = new YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias.Value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias.Start, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias.End);
			Skip();
			return result;
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken.Start;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName anchor = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName.Empty;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName tag = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName.Empty;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchor _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchor = null;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag = null;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start5;
		while (true)
		{
			if (anchor.IsEmpty && _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchor _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchor2)
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchor = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchor2;
				anchor = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchor2.Value;
				Skip();
			}
			else
			{
				if (!tag.IsEmpty || !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag2))
				{
					if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchor { Start: var start3 } _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchor3)
					{
						end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchor3.End;
						throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start3, in end, "While parsing a node, found more than one anchor.");
					}
					if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias { Start: var start4 } _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias2)
					{
						end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias2.End;
						throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start4, in end, "While parsing a node, did not find expected token.");
					}
					if (!(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError2))
					{
						break;
					}
					if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag != null && _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchor != null && !anchor.IsEmpty)
					{
						return new YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar(anchor, default(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName), string.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Any, isPlainImplicit: false, isQuotedImplicit: false, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchor.Start, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchor.End);
					}
					start5 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError2.Start;
					end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError2.End;
					throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start5, in end, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError2.Value);
				}
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag2;
				if (string.IsNullOrEmpty(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag2.Handle))
				{
					tag = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag2.Suffix);
				}
				else
				{
					if (!tagDirectives.Contains(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag2.Handle))
					{
						start5 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag2.Start;
						end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag2.End;
						throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start5, in end, "While parsing a node, found undefined tag handle.");
					}
					tag = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName(tagDirectives[_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag2.Handle].Prefix + _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag2.Suffix);
				}
				Skip();
			}
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken() ?? throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException("Reached the end of the stream while parsing a node");
		}
		bool isEmpty = tag.IsEmpty;
		if (isIndentlessSequence && GetCurrentToken() is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEntry)
		{
			state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.IndentlessSequenceEntry;
			return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart(anchor, tag, isEmpty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStyle.Block, start2, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken.End);
		}
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar)
		{
			bool isPlainImplicit = false;
			bool isQuotedImplicit = false;
			if ((_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.Style == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Plain && tag.IsEmpty) || tag.IsNonSpecific)
			{
				isPlainImplicit = true;
			}
			else if (tag.IsEmpty)
			{
				isQuotedImplicit = true;
			}
			state = states.Pop();
			Skip();
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent result2 = new YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar(anchor, tag, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.Value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.Style, isPlainImplicit, isQuotedImplicit, start2, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.End, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.IsKey);
			if (!anchor.IsEmpty && scanner.MoveNextWithoutConsuming())
			{
				currentToken = scanner.Current;
				if (currentToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError)
				{
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError3 = currentToken as _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError;
					start5 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError3.Start;
					end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError3.End;
					throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start5, in end, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError3.Value);
				}
			}
			if (state == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowMappingKey && !(scanner.Current is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowMappingEnd) && scanner.MoveNextWithoutConsuming())
			{
				currentToken = scanner.Current;
				if (currentToken != null && !(currentToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowEntry) && !(currentToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowMappingEnd))
				{
					start5 = currentToken.Start;
					end = currentToken.End;
					throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start5, in end, "While parsing a flow mapping, did not find expected ',' or '}'.");
				}
			}
			return result2;
		}
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowSequenceStart _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowSequenceStart)
		{
			state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowSequenceFirstEntry;
			return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart(anchor, tag, isEmpty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStyle.Flow, start2, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowSequenceStart.End);
		}
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowMappingStart _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowMappingStart)
		{
			state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowMappingFirstKey;
			return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart(anchor, tag, isEmpty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStyle.Flow, start2, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowMappingStart.End);
		}
		if (isBlock)
		{
			if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockSequenceStart _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockSequenceStart)
			{
				state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.BlockSequenceFirstEntry;
				return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart(anchor, tag, isEmpty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStyle.Block, start2, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockSequenceStart.End);
			}
			if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockMappingStart _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockMappingStart)
			{
				state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.BlockMappingFirstKey;
				return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart(anchor, tag, isEmpty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStyle.Block, start2, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockMappingStart.End);
			}
		}
		if (!anchor.IsEmpty || !tag.IsEmpty)
		{
			state = states.Pop();
			return new YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar(anchor, tag, string.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Plain, isEmpty, isQuotedImplicit: false, start2, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken.End);
		}
		start5 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken.Start;
		end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken.End;
		throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start5, in end, "While parsing a node, did not find expected node content.");
	}

	private YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd ParseDocumentEnd()
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken() ?? throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException("Reached the end of the stream while parsing a document end");
		bool isImplicit = true;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken.Start;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = start;
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd)
		{
			end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken.End;
			Skip();
			isImplicit = false;
		}
		else if (!(currentToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd) && !(currentToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart) && !(currentToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowSequenceEnd) && !(currentToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective) && (!(Current is YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar) || !(currentToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError)))
		{
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start, in end, "Did not find expected <document end>.");
		}
		if (version != null && version.Version.Major == 1 && version.Version.Minor > 1)
		{
			version = null;
		}
		state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.DocumentStart;
		return new YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd(isImplicit, start, end);
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent ParseBlockSequenceEntry(bool isFirst)
	{
		if (isFirst)
		{
			GetCurrentToken();
			Skip();
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEntry { End: var position })
		{
			Skip();
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
			if (!(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEntry) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEnd))
			{
				states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.BlockSequenceEntry);
				return ParseNode(isBlock: true, isIndentlessSequence: false);
			}
			state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.BlockSequenceEntry;
			return ProcessEmptyScalar(in position);
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end;
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEnd _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEnd)
		{
			state = states.Pop();
			start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEnd.Start;
			end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEnd.End;
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent result = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceEnd(in start, in end);
			Skip();
			return result;
		}
		start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.Start ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.End ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start, in end, "While parsing a block collection, did not find expected '-' indicator.");
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent ParseIndentlessSequenceEntry()
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEntry { End: var position })
		{
			Skip();
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
			if (!(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEntry) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EKey) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EValue) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEnd))
			{
				states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.IndentlessSequenceEntry);
				return ParseNode(isBlock: true, isIndentlessSequence: false);
			}
			state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.IndentlessSequenceEntry;
			return ProcessEmptyScalar(in position);
		}
		state = states.Pop();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.Start ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.End ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceEnd(in start, in end);
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent ParseBlockMappingKey(bool isFirst)
	{
		if (isFirst)
		{
			GetCurrentToken();
			Skip();
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EKey { End: var position })
		{
			Skip();
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
			if (!(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EKey) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EValue) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEnd))
			{
				states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.BlockMappingValue);
				return ParseNode(isBlock: true, isIndentlessSequence: true);
			}
			state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.BlockMappingValue;
			return ProcessEmptyScalar(in position);
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark position2;
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EValue _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EValue)
		{
			Skip();
			position2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EValue.End;
			return ProcessEmptyScalar(in position2);
		}
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias)
		{
			Skip();
			return new YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias.Value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias.Start, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias.End);
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end;
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEnd _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEnd)
		{
			state = states.Pop();
			position2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEnd.Start;
			end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEnd.End;
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent result = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd(in position2, in end);
			Skip();
			return result;
		}
		if (GetCurrentToken() is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError { Start: var start } _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError)
		{
			end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError.End;
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError.Value);
		}
		position2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.Start ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.End ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in position2, in end, "While parsing a block mapping, did not find expected key.");
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent ParseBlockMappingValue()
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EValue { End: var position })
		{
			Skip();
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
			if (!(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EKey) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EValue) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEnd))
			{
				states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.BlockMappingKey);
				return ParseNode(isBlock: true, isIndentlessSequence: true);
			}
			state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.BlockMappingKey;
			return ProcessEmptyScalar(in position);
		}
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError { Start: var start } _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError.End;
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start, in end, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError.Value);
		}
		state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.BlockMappingKey;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark position2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.Start ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		return ProcessEmptyScalar(in position2);
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent ParseFlowSequenceEntry(bool isFirst)
	{
		if (isFirst)
		{
			GetCurrentToken();
			Skip();
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent result;
		if (!(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowSequenceEnd))
		{
			if (!isFirst)
			{
				if (!(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowEntry))
				{
					start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.Start ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
					end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.End ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
					throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start, in end, "While parsing a flow sequence, did not find expected ',' or ']'.");
				}
				Skip();
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
			}
			if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EKey)
			{
				state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowSequenceEntryMappingKey;
				result = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName.Empty, isImplicit: true, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStyle.Flow);
				Skip();
				return result;
			}
			if (!(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowSequenceEnd))
			{
				states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowSequenceEntry);
				return ParseNode(isBlock: false, isIndentlessSequence: false);
			}
		}
		state = states.Pop();
		start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.Start ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.End ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		result = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceEnd(in start, in end);
		Skip();
		return result;
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent ParseFlowSequenceEntryMappingKey()
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
		if (!(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EValue) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowEntry) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowSequenceEnd))
		{
			states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowSequenceEntryMappingValue);
			return ParseNode(isBlock: false, isIndentlessSequence: false);
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark position = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.End ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		Skip();
		state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowSequenceEntryMappingValue;
		return ProcessEmptyScalar(in position);
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent ParseFlowSequenceEntryMappingValue()
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EValue)
		{
			Skip();
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
			if (!(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowEntry) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowSequenceEnd))
			{
				states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowSequenceEntryMappingEnd);
				return ParseNode(isBlock: false, isIndentlessSequence: false);
			}
		}
		state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowSequenceEntryMappingEnd;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark position = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.Start ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		return ProcessEmptyScalar(in position);
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd ParseFlowSequenceEntryMappingEnd()
	{
		state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowSequenceEntry;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.Start ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.End ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd(in start, in end);
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent ParseFlowMappingKey(bool isFirst)
	{
		if (isFirst)
		{
			GetCurrentToken();
			Skip();
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end;
		if (!(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowMappingEnd))
		{
			if (!isFirst)
			{
				if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowEntry)
				{
					Skip();
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
				}
				else if (!(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar))
				{
					start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.Start ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
					end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.End ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
					throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start, in end, "While parsing a flow mapping,  did not find expected ',' or '}'.");
				}
			}
			if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EKey)
			{
				Skip();
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
				if (!(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EValue) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowEntry) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowMappingEnd))
				{
					states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowMappingValue);
					return ParseNode(isBlock: false, isIndentlessSequence: false);
				}
				state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowMappingValue;
				start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.Start ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
				return ProcessEmptyScalar(in start);
			}
			if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar)
			{
				states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowMappingValue);
				return ParseNode(isBlock: false, isIndentlessSequence: false);
			}
			if (!(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowMappingEnd))
			{
				states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowMappingEmptyValue);
				return ParseNode(isBlock: false, isIndentlessSequence: false);
			}
		}
		state = states.Pop();
		Skip();
		start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.Start ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.End ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd(in start, in end);
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent ParseFlowMappingValue(bool isEmpty)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
		if (!isEmpty && _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EValue)
		{
			Skip();
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = GetCurrentToken();
			if (!(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowEntry) && !(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowMappingEnd))
			{
				states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowMappingKey);
				return ParseNode(isBlock: false, isIndentlessSequence: false);
			}
		}
		state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParserState.FlowMappingKey;
		if (!isEmpty && _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken is YamlDotNet.Core.Tokens._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar)
		{
			Skip();
			return new YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName.Empty, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.Value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.Style, isPlainImplicit: false, isQuotedImplicit: false, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken.Start, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.End);
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark position = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken?.Start ?? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark.Empty;
		return ProcessEmptyScalar(in position);
	}
}
