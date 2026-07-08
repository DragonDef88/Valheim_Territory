using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Core.Events;
using YamlDotNet.Core.ObjectPool;
using YamlDotNet.Core.Tokens;

namespace YamlDotNet.Core;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitter : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter
{
	private class AnchorData
	{
		public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName Anchor;

		public bool IsAlias;
	}

	private class TagData
	{
		public string? Handle;

		public string? Suffix;
	}

	private class ScalarData
	{
		public string Value = string.Empty;

		public bool IsMultiline;

		public bool IsFlowPlainAllowed;

		public bool IsBlockPlainAllowed;

		public bool IsSingleQuotedAllowed;

		public bool IsBlockAllowed;

		public bool HasSingleQuotes;

		public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle Style;
	}

	private static readonly Regex UriReplacer = new Regex("[^0-9A-Za-z_\\-;?@=$~\\\\\\)\\]/:&+,\\.\\*\\(\\[!]", RegexOptions.Compiled | RegexOptions.Singleline);

	private static readonly string[] NewLineSeparators = new string[3] { "\r\n", "\r", "\n" };

	private readonly TextWriter output;

	private readonly bool outputUsesUnicodeEncoding;

	private readonly int maxSimpleKeyLength;

	private readonly bool isCanonical;

	private readonly bool skipAnchorName;

	private readonly int bestIndent;

	private readonly int bestWidth;

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState state;

	private readonly Stack<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState> states = new Stack<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState>();

	private readonly Queue<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent> events = new Queue<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>();

	private readonly Stack<int> indents = new Stack<int>();

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection tagDirectives = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection();

	private int indent;

	private int flowLevel;

	private bool isMappingContext;

	private bool isSimpleKeyContext;

	private int column;

	private bool isWhitespace;

	private bool isIndentation;

	private readonly bool forceIndentLess;

	private readonly bool useUtf16SurrogatePair;

	private bool isDocumentEndWritten;

	private readonly AnchorData anchorData = new AnchorData();

	private readonly TagData tagData = new TagData();

	private readonly ScalarData scalarData = new ScalarData();

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitter(TextWriter output)
		: this(output, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterSettings.Default)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitter(TextWriter output, int bestIndent)
		: this(output, bestIndent, int.MaxValue)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitter(TextWriter output, int bestIndent, int bestWidth)
		: this(output, bestIndent, bestWidth, isCanonical: false)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitter(TextWriter output, int bestIndent, int bestWidth, bool isCanonical)
		: this(output, new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterSettings(bestIndent, bestWidth, isCanonical, 1024))
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitter(TextWriter output, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterSettings settings)
	{
		bestIndent = settings.BestIndent;
		bestWidth = settings.BestWidth;
		isCanonical = settings.IsCanonical;
		maxSimpleKeyLength = settings.MaxSimpleKeyLength;
		skipAnchorName = settings.SkipAnchorName;
		forceIndentLess = !settings.IndentSequences;
		useUtf16SurrogatePair = settings.UseUtf16SurrogatePairs;
		this.output = output;
		this.output.NewLine = settings.NewLine;
		outputUsesUnicodeEncoding = IsUnicode(output.Encoding);
	}

	public void Emit(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent @event)
	{
		events.Enqueue(@event);
		while (!NeedMoreEvents())
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent evt = events.Peek();
			try
			{
				AnalyzeEvent(evt);
				StateMachine(evt);
			}
			finally
			{
				events.Dequeue();
			}
		}
	}

	private bool NeedMoreEvents()
	{
		if (events.Count == 0)
		{
			return true;
		}
		int num;
		switch (events.Peek().Type)
		{
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.DocumentStart:
			num = 1;
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.SequenceStart:
			num = 2;
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.MappingStart:
			num = 3;
			break;
		default:
			return false;
		}
		if (events.Count > num)
		{
			return false;
		}
		int num2 = 0;
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent @event in events)
		{
			switch (@event.Type)
			{
			case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.DocumentStart:
			case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.SequenceStart:
			case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.MappingStart:
				num2++;
				break;
			case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.DocumentEnd:
			case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.SequenceEnd:
			case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.MappingEnd:
				num2--;
				break;
			}
			if (num2 == 0)
			{
				return false;
			}
		}
		return true;
	}

	private void AnalyzeEvent(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent evt)
	{
		anchorData.Anchor = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName.Empty;
		tagData.Handle = null;
		tagData.Suffix = null;
		if (evt is YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias)
		{
			AnalyzeAnchor(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias.Value, isAlias: true);
		}
		else if (evt is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent)
		{
			if (evt is YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar scalar)
			{
				AnalyzeScalar(scalar);
			}
			AnalyzeAnchor(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent.Anchor, isAlias: false);
			if (!_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent.Tag.IsEmpty && (isCanonical || _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent.IsCanonical))
			{
				AnalyzeTag(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent.Tag);
			}
		}
	}

	private void AnalyzeAnchor(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName anchor, bool isAlias)
	{
		anchorData.Anchor = anchor;
		anchorData.IsAlias = isAlias;
	}

	private void AnalyzeScalar(YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar scalar)
	{
		string value = scalar.Value;
		scalarData.Value = value;
		if (value.Length == 0)
		{
			if (scalar.Tag == "tag:yaml.org,2002:null")
			{
				scalarData.IsMultiline = false;
				scalarData.IsFlowPlainAllowed = false;
				scalarData.IsBlockPlainAllowed = true;
				scalarData.IsSingleQuotedAllowed = false;
				scalarData.IsBlockAllowed = false;
			}
			else
			{
				scalarData.IsMultiline = false;
				scalarData.IsFlowPlainAllowed = false;
				scalarData.IsBlockPlainAllowed = false;
				scalarData.IsSingleQuotedAllowed = true;
				scalarData.IsBlockAllowed = false;
			}
			return;
		}
		bool flag = false;
		bool flag2 = false;
		if (value.StartsWith("---", StringComparison.Ordinal) || value.StartsWith("...", StringComparison.Ordinal))
		{
			flag = true;
			flag2 = true;
		}
		StringLookAheadBufferPool.BufferWrapper bufferWrapper = StringLookAheadBufferPool.Rent(value);
		try
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringLookAheadBuffer> _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2 = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringLookAheadBuffer>(bufferWrapper.Buffer);
			bool flag3 = true;
			bool flag4 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.IsWhiteBreakOrZero(1);
			bool flag5 = false;
			bool flag6 = false;
			bool flag7 = false;
			bool flag8 = false;
			bool flag9 = false;
			bool flag10 = false;
			bool flag11 = false;
			bool flag12 = false;
			bool flag13 = false;
			bool flag14 = false;
			bool flag15 = false;
			bool flag16 = !ValueIsRepresentableInOutputEncoding(value);
			bool flag17 = false;
			bool flag18 = false;
			bool flag19 = true;
			while (!_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.EndOfInput)
			{
				if (flag19)
				{
					if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.Check("#,[]{}&*!|>\"%@`'"))
					{
						flag = true;
						flag2 = true;
						flag9 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.Check('\'');
						flag17 |= _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.Check('\'');
					}
					if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.Check("?:"))
					{
						flag = true;
						if (flag4)
						{
							flag2 = true;
						}
					}
					if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.Check('-') && flag4)
					{
						flag = true;
						flag2 = true;
					}
				}
				else
				{
					if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.Check(",?[]{}"))
					{
						flag = true;
					}
					if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.Check(':'))
					{
						flag = true;
						if (flag4)
						{
							flag2 = true;
						}
					}
					if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.Check('#') && flag3)
					{
						flag = true;
						flag2 = true;
					}
					flag17 |= _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.Check('\'');
				}
				if (!flag16 && !_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.IsPrintable())
				{
					flag16 = true;
				}
				if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.IsBreak())
				{
					flag15 = true;
				}
				if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.IsSpace())
				{
					if (flag19)
					{
						flag5 = true;
					}
					if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.Buffer.Position >= _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.Buffer.Length - 1)
					{
						flag7 = true;
					}
					if (flag13)
					{
						flag10 = true;
						flag14 = true;
					}
					flag12 = true;
					flag13 = false;
				}
				else if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.IsBreak())
				{
					if (flag19)
					{
						flag6 = true;
					}
					if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.Buffer.Position >= _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.Buffer.Length - 1)
					{
						flag8 = true;
					}
					if (flag12)
					{
						flag11 = true;
					}
					if (flag14)
					{
						flag18 = true;
					}
					flag12 = false;
					flag13 = true;
				}
				else
				{
					flag12 = false;
					flag13 = false;
					flag14 = false;
				}
				flag3 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.IsWhiteBreakOrZero();
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.Skip(1);
				if (!_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.EndOfInput)
				{
					flag4 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.IsWhiteBreakOrZero(1);
				}
				flag19 = false;
			}
			scalarData.IsFlowPlainAllowed = true;
			scalarData.IsBlockPlainAllowed = true;
			scalarData.IsSingleQuotedAllowed = true;
			scalarData.IsBlockAllowed = true;
			if (flag5 || flag6 || flag7 || flag8 || flag9)
			{
				scalarData.IsFlowPlainAllowed = false;
				scalarData.IsBlockPlainAllowed = false;
			}
			if (flag7)
			{
				scalarData.IsBlockAllowed = false;
			}
			if (flag10)
			{
				scalarData.IsFlowPlainAllowed = false;
				scalarData.IsBlockPlainAllowed = false;
				scalarData.IsSingleQuotedAllowed = false;
			}
			if (flag11 || flag16)
			{
				scalarData.IsFlowPlainAllowed = false;
				scalarData.IsBlockPlainAllowed = false;
				scalarData.IsSingleQuotedAllowed = false;
			}
			if (flag18)
			{
				scalarData.IsBlockAllowed = false;
			}
			scalarData.IsMultiline = flag15;
			if (flag15)
			{
				scalarData.IsFlowPlainAllowed = false;
				scalarData.IsBlockPlainAllowed = false;
			}
			if (flag)
			{
				scalarData.IsFlowPlainAllowed = false;
			}
			if (flag2)
			{
				scalarData.IsBlockPlainAllowed = false;
			}
			scalarData.HasSingleQuotes = flag17;
		}
		finally
		{
			((IDisposable)bufferWrapper).Dispose();
		}
	}

	private bool ValueIsRepresentableInOutputEncoding(string value)
	{
		if (outputUsesUnicodeEncoding)
		{
			return true;
		}
		try
		{
			byte[] bytes = output.Encoding.GetBytes(value);
			string @string = output.Encoding.GetString(bytes, 0, bytes.Length);
			return @string.Equals(value);
		}
		catch (EncoderFallbackException)
		{
			return false;
		}
		catch (ArgumentOutOfRangeException)
		{
			return false;
		}
	}

	private static bool IsUnicode(Encoding encoding)
	{
		if (!(encoding is UTF8Encoding) && !(encoding is UnicodeEncoding))
		{
			return encoding is UTF7Encoding;
		}
		return true;
	}

	private void AnalyzeTag(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName tag)
	{
		tagData.Handle = tag.Value;
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective tagDirective in tagDirectives)
		{
			if (tag.Value.StartsWith(tagDirective.Prefix, StringComparison.Ordinal))
			{
				tagData.Handle = tagDirective.Handle;
				tagData.Suffix = tag.Value.Substring(tagDirective.Prefix.Length);
				break;
			}
		}
	}

	private void StateMachine(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent evt)
	{
		if (evt is YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EComment comment)
		{
			EmitComment(comment);
			return;
		}
		switch (state)
		{
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.StreamStart:
			EmitStreamStart(evt);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.FirstDocumentStart:
			EmitDocumentStart(evt, isFirst: true);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.DocumentStart:
			EmitDocumentStart(evt, isFirst: false);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.DocumentContent:
			EmitDocumentContent(evt);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.DocumentEnd:
			EmitDocumentEnd(evt);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.FlowSequenceFirstItem:
			EmitFlowSequenceItem(evt, isFirst: true);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.FlowSequenceItem:
			EmitFlowSequenceItem(evt, isFirst: false);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.FlowMappingFirstKey:
			EmitFlowMappingKey(evt, isFirst: true);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.FlowMappingKey:
			EmitFlowMappingKey(evt, isFirst: false);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.FlowMappingSimpleValue:
			EmitFlowMappingValue(evt, isSimple: true);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.FlowMappingValue:
			EmitFlowMappingValue(evt, isSimple: false);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.BlockSequenceFirstItem:
			EmitBlockSequenceItem(evt, isFirst: true);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.BlockSequenceItem:
			EmitBlockSequenceItem(evt, isFirst: false);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.BlockMappingFirstKey:
			EmitBlockMappingKey(evt, isFirst: true);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.BlockMappingKey:
			EmitBlockMappingKey(evt, isFirst: false);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.BlockMappingSimpleValue:
			EmitBlockMappingValue(evt, isSimple: true);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.BlockMappingValue:
			EmitBlockMappingValue(evt, isSimple: false);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.StreamEnd:
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException("Expected nothing after STREAM-END");
		default:
			throw new InvalidOperationException();
		}
	}

	private void EmitComment(YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EComment comment)
	{
		if (flowLevel > 0 || state == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.FlowMappingFirstKey || state == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.FlowSequenceFirstItem)
		{
			return;
		}
		string[] array = comment.Value.Split(NewLineSeparators, StringSplitOptions.None);
		if (comment.IsInline)
		{
			Write(" # ");
			Write(string.Join(" ", array));
		}
		else
		{
			bool flag = state == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.BlockMappingFirstKey;
			if (flag)
			{
				IncreaseIndent(isFlow: false, isIndentless: false);
			}
			string[] array2 = array;
			foreach (string value in array2)
			{
				WriteIndent();
				Write("# ");
				Write(value);
				WriteBreak();
			}
			if (flag)
			{
				indent = indents.Pop();
			}
		}
		isIndentation = true;
	}

	private void EmitStreamStart(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent evt)
	{
		if (!(evt is YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamStart))
		{
			throw new ArgumentException("Expected STREAM-START.", "evt");
		}
		indent = -1;
		column = 0;
		isWhitespace = true;
		isIndentation = true;
		state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.FirstDocumentStart;
	}

	private void EmitDocumentStart(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent evt, bool isFirst)
	{
		if (evt is YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart)
		{
			bool flag = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart.IsImplicit && isFirst && !isCanonical;
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection2 = NonDefaultTagsAmong(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart.Tags);
			if (!isFirst && !isDocumentEndWritten && (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart.Version != null || _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection2.Count > 0))
			{
				isDocumentEndWritten = false;
				WriteIndicator("...", needWhitespace: true, whitespace: false, indentation: false);
				WriteIndent();
			}
			if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart.Version != null)
			{
				AnalyzeVersionDirective(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart.Version);
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersion version = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart.Version.Version;
				flag = false;
				WriteIndicator("%YAML", needWhitespace: true, whitespace: false, indentation: false);
				WriteIndicator(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor), needWhitespace: true, whitespace: false, indentation: false);
				WriteIndent();
			}
			foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective item in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection2)
			{
				AppendTagDirectiveTo(item, allowDuplicates: false, tagDirectives);
			}
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective[] defaultTagDirectives = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EConstants.DefaultTagDirectives;
			foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective value in defaultTagDirectives)
			{
				AppendTagDirectiveTo(value, allowDuplicates: true, tagDirectives);
			}
			if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection2.Count > 0)
			{
				flag = false;
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective[] defaultTagDirectives2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EConstants.DefaultTagDirectives;
				foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective value2 in defaultTagDirectives2)
				{
					AppendTagDirectiveTo(value2, allowDuplicates: true, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection2);
				}
				foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective item2 in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection2)
				{
					WriteIndicator("%TAG", needWhitespace: true, whitespace: false, indentation: false);
					WriteTagHandle(item2.Handle);
					WriteTagContent(item2.Prefix, needsWhitespace: true);
					WriteIndent();
				}
			}
			if (CheckEmptyDocument())
			{
				flag = false;
			}
			if (!flag)
			{
				WriteIndent();
				WriteIndicator("---", needWhitespace: true, whitespace: false, indentation: false);
				if (isCanonical)
				{
					WriteIndent();
				}
			}
			state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.DocumentContent;
		}
		else
		{
			if (!(evt is YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd))
			{
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException("Expected DOCUMENT-START or STREAM-END");
			}
			state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.StreamEnd;
		}
	}

	private static _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection NonDefaultTagsAmong(IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective>? tagCollection)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection2 = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection();
		if (tagCollection == null)
		{
			return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection2;
		}
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective item2 in tagCollection)
		{
			AppendTagDirectiveTo(item2, allowDuplicates: false, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection2);
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective[] defaultTagDirectives = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EConstants.DefaultTagDirectives;
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective item in defaultTagDirectives)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection2.Remove(item);
		}
		return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection2;
	}

	private static void AnalyzeVersionDirective(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective versionDirective)
	{
		if (versionDirective.Version.Major != 1 || versionDirective.Version.Minor > 3)
		{
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException("Incompatible %YAML directive");
		}
	}

	private static void AppendTagDirectiveTo(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective value, bool allowDuplicates, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirectiveCollection tagDirectives)
	{
		if (tagDirectives.Contains(value))
		{
			if (!allowDuplicates)
			{
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException("Duplicate %TAG directive.");
			}
		}
		else
		{
			tagDirectives.Add(value);
		}
	}

	private void EmitDocumentContent(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent evt)
	{
		states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.DocumentEnd);
		EmitNode(evt, isMapping: false, isSimpleKey: false);
	}

	private void EmitNode(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent evt, bool isMapping, bool isSimpleKey)
	{
		isMappingContext = isMapping;
		isSimpleKeyContext = isSimpleKey;
		switch (evt.Type)
		{
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.Alias:
			EmitAlias();
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.Scalar:
			EmitScalar(evt);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.SequenceStart:
			EmitSequenceStart(evt);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.MappingStart:
			EmitMappingStart(evt);
			break;
		default:
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException($"Expected SCALAR, SEQUENCE-START, MAPPING-START, or ALIAS, got {evt.Type}");
		}
	}

	private void EmitAlias()
	{
		ProcessAnchor();
		state = states.Pop();
	}

	private void EmitScalar(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent evt)
	{
		SelectScalarStyle(evt);
		ProcessAnchor();
		ProcessTag();
		IncreaseIndent(isFlow: true, isIndentless: false);
		ProcessScalar();
		indent = indents.Pop();
		state = states.Pop();
	}

	private void SelectScalarStyle(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent evt)
	{
		YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar = (YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar)evt;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.Style;
		bool flag = tagData.Handle == null && tagData.Suffix == null;
		if (flag && !_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.IsPlainImplicit && !_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.IsQuotedImplicit)
		{
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException("Neither tag nor isImplicit flags are specified.");
		}
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle2 == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Any)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle2 = ((!scalarData.IsMultiline) ? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Plain : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Folded);
		}
		if (isCanonical)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.DoubleQuoted;
		}
		if (isSimpleKeyContext && scalarData.IsMultiline)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.DoubleQuoted;
		}
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle2 == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Plain)
		{
			if ((flowLevel != 0 && !scalarData.IsFlowPlainAllowed) || (flowLevel == 0 && !scalarData.IsBlockPlainAllowed))
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle2 = ((scalarData.IsSingleQuotedAllowed && !scalarData.HasSingleQuotes) ? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.SingleQuoted : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.DoubleQuoted);
			}
			if (string.IsNullOrEmpty(scalarData.Value) && (flowLevel != 0 || isSimpleKeyContext))
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.SingleQuoted;
			}
			if (flag && !_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.IsPlainImplicit)
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.SingleQuoted;
			}
		}
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle2 == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.SingleQuoted && !scalarData.IsSingleQuotedAllowed)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.DoubleQuoted;
		}
		if ((_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle2 == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Literal || _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle2 == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Folded) && (!scalarData.IsBlockAllowed || flowLevel != 0 || isSimpleKeyContext))
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.DoubleQuoted;
		}
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle2 == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.ForcePlain)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle2 = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Plain;
		}
		scalarData.Style = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle2;
	}

	private void ProcessScalar()
	{
		switch (scalarData.Style)
		{
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Plain:
			WritePlainScalar(scalarData.Value, !isSimpleKeyContext);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.SingleQuoted:
			WriteSingleQuotedScalar(scalarData.Value, !isSimpleKeyContext);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.DoubleQuoted:
			WriteDoubleQuotedScalar(scalarData.Value, !isSimpleKeyContext);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Literal:
			WriteLiteralScalar(scalarData.Value);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Folded:
			WriteFoldedScalar(scalarData.Value);
			break;
		default:
			throw new InvalidOperationException();
		}
	}

	private void WritePlainScalar(string value, bool allowBreaks)
	{
		if (!isWhitespace)
		{
			Write(' ');
		}
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if (IsSpace(c))
			{
				if (allowBreaks && !flag && column > bestWidth && i + 1 < value.Length && value[i + 1] != ' ')
				{
					WriteIndent();
				}
				else
				{
					Write(c);
				}
				flag = true;
				continue;
			}
			if (IsBreak(c, out var breakChar))
			{
				if (!flag2 && c == '\n')
				{
					WriteBreak();
				}
				WriteBreak(breakChar);
				isIndentation = true;
				flag2 = true;
				continue;
			}
			if (flag2)
			{
				WriteIndent();
			}
			Write(c);
			isIndentation = false;
			flag = false;
			flag2 = false;
		}
		isWhitespace = false;
		isIndentation = false;
	}

	private void WriteSingleQuotedScalar(string value, bool allowBreaks)
	{
		WriteIndicator("'", needWhitespace: true, whitespace: false, indentation: false);
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if (c == ' ')
			{
				if (allowBreaks && !flag && column > bestWidth && i != 0 && i + 1 < value.Length && value[i + 1] != ' ')
				{
					WriteIndent();
				}
				else
				{
					Write(c);
				}
				flag = true;
				continue;
			}
			if (IsBreak(c, out var breakChar))
			{
				if (!flag2 && c == '\n')
				{
					WriteBreak();
				}
				WriteBreak(breakChar);
				isIndentation = true;
				flag2 = true;
				continue;
			}
			if (flag2)
			{
				WriteIndent();
			}
			if (c == '\'')
			{
				Write(c);
			}
			Write(c);
			isIndentation = false;
			flag = false;
			flag2 = false;
		}
		WriteIndicator("'", needWhitespace: false, whitespace: false, indentation: false);
		isWhitespace = false;
		isIndentation = false;
	}

	private void WriteDoubleQuotedScalar(string value, bool allowBreaks)
	{
		WriteIndicator("\"", needWhitespace: true, whitespace: false, indentation: false);
		bool flag = false;
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if (IsPrintable(c) && !IsBreak(c, out var _))
			{
				switch (c)
				{
				case '"':
				case '\\':
					break;
				case ' ':
					if (allowBreaks && !flag && column > bestWidth && i > 0 && i + 1 < value.Length)
					{
						WriteIndent();
						if (value[i + 1] == ' ')
						{
							Write('\\');
						}
					}
					else
					{
						Write(c);
					}
					flag = true;
					continue;
				default:
					Write(c);
					flag = false;
					continue;
				}
			}
			Write('\\');
			switch (c)
			{
			case '\0':
				Write('0');
				break;
			case '\a':
				Write('a');
				break;
			case '\b':
				Write('b');
				break;
			case '\t':
				Write('t');
				break;
			case '\n':
				Write('n');
				break;
			case '\v':
				Write('v');
				break;
			case '\f':
				Write('f');
				break;
			case '\r':
				Write('r');
				break;
			case '\u001b':
				Write('e');
				break;
			case '"':
				Write('"');
				break;
			case '\\':
				Write('\\');
				break;
			case '\u0085':
				Write('N');
				break;
			case '\u00a0':
				Write('_');
				break;
			case '\u2028':
				Write('L');
				break;
			case '\u2029':
				Write('P');
				break;
			default:
			{
				ushort num = c;
				if (num <= 255)
				{
					Write('x');
					Write(num.ToString("X02", CultureInfo.InvariantCulture));
				}
				else if (IsHighSurrogate(c))
				{
					if (i + 1 >= value.Length || !IsLowSurrogate(value[i + 1]))
					{
						throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException("While writing a quoted scalar, found an orphaned high surrogate.");
					}
					if (useUtf16SurrogatePair)
					{
						Write('u');
						Write(num.ToString("X04", CultureInfo.InvariantCulture));
						Write('\\');
						Write('u');
						Write(((ushort)value[i + 1]).ToString("X04", CultureInfo.InvariantCulture));
					}
					else
					{
						Write('U');
						Write(char.ConvertToUtf32(c, value[i + 1]).ToString("X08", CultureInfo.InvariantCulture));
					}
					i++;
				}
				else
				{
					Write('u');
					Write(num.ToString("X04", CultureInfo.InvariantCulture));
				}
				break;
			}
			}
			flag = false;
		}
		WriteIndicator("\"", needWhitespace: false, whitespace: false, indentation: false);
		isWhitespace = false;
		isIndentation = false;
	}

	private void WriteLiteralScalar(string value)
	{
		bool flag = true;
		WriteIndicator("|", needWhitespace: true, whitespace: false, indentation: false);
		WriteBlockScalarHints(value);
		WriteBreak();
		isIndentation = true;
		isWhitespace = true;
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if (c == '\r' && i + 1 < value.Length && value[i + 1] == '\n')
			{
				continue;
			}
			if (IsBreak(c, out var breakChar))
			{
				WriteBreak(breakChar);
				isIndentation = true;
				flag = true;
				continue;
			}
			if (flag)
			{
				WriteIndent();
			}
			Write(c);
			isIndentation = false;
			flag = false;
		}
	}

	private void WriteFoldedScalar(string value)
	{
		bool flag = true;
		bool flag2 = true;
		WriteIndicator(">", needWhitespace: true, whitespace: false, indentation: false);
		WriteBlockScalarHints(value);
		WriteBreak();
		isIndentation = true;
		isWhitespace = true;
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			if (IsBreak(c, out var breakChar))
			{
				if (c == '\r' && i + 1 < value.Length && value[i + 1] == '\n')
				{
					continue;
				}
				if (!flag && !flag2 && breakChar == '\n')
				{
					int j;
					char breakChar2;
					for (j = 0; i + j < value.Length && IsBreak(value[i + j], out breakChar2); j++)
					{
					}
					if (i + j < value.Length && !IsBlank(value[i + j]) && !IsBreak(value[i + j], out breakChar2))
					{
						WriteBreak();
					}
				}
				WriteBreak(breakChar);
				isIndentation = true;
				flag = true;
			}
			else
			{
				if (flag)
				{
					WriteIndent();
					flag2 = IsBlank(c);
				}
				if (!flag && c == ' ' && i + 1 < value.Length && value[i + 1] != ' ' && column > bestWidth)
				{
					WriteIndent();
				}
				else
				{
					Write(c);
				}
				isIndentation = false;
				flag = false;
			}
		}
	}

	private static bool IsSpace(char character)
	{
		return character == ' ';
	}

	private static bool IsBreak(char character, out char breakChar)
	{
		switch (character)
		{
		case '\n':
		case '\r':
		case '\u0085':
			breakChar = '\n';
			return true;
		case '\u2028':
		case '\u2029':
			breakChar = character;
			return true;
		default:
			breakChar = '\0';
			return false;
		}
	}

	private static bool IsBlank(char character)
	{
		if (character != ' ')
		{
			return character == '\t';
		}
		return true;
	}

	private static bool IsPrintable(char character)
	{
		switch (character)
		{
		default:
			if (character != '\u0085' && (character < '\u00a0' || character > '\ud7ff'))
			{
				if (character >= '\ue000')
				{
					return character <= '\ufffd';
				}
				return false;
			}
			break;
		case '\t':
		case '\n':
		case '\r':
		case ' ':
		case '!':
		case '"':
		case '#':
		case '$':
		case '%':
		case '&':
		case '\'':
		case '(':
		case ')':
		case '*':
		case '+':
		case ',':
		case '-':
		case '.':
		case '/':
		case '0':
		case '1':
		case '2':
		case '3':
		case '4':
		case '5':
		case '6':
		case '7':
		case '8':
		case '9':
		case ':':
		case ';':
		case '<':
		case '=':
		case '>':
		case '?':
		case '@':
		case 'A':
		case 'B':
		case 'C':
		case 'D':
		case 'E':
		case 'F':
		case 'G':
		case 'H':
		case 'I':
		case 'J':
		case 'K':
		case 'L':
		case 'M':
		case 'N':
		case 'O':
		case 'P':
		case 'Q':
		case 'R':
		case 'S':
		case 'T':
		case 'U':
		case 'V':
		case 'W':
		case 'X':
		case 'Y':
		case 'Z':
		case '[':
		case '\\':
		case ']':
		case '^':
		case '_':
		case '`':
		case 'a':
		case 'b':
		case 'c':
		case 'd':
		case 'e':
		case 'f':
		case 'g':
		case 'h':
		case 'i':
		case 'j':
		case 'k':
		case 'l':
		case 'm':
		case 'n':
		case 'o':
		case 'p':
		case 'q':
		case 'r':
		case 's':
		case 't':
		case 'u':
		case 'v':
		case 'w':
		case 'x':
		case 'y':
		case 'z':
		case '{':
		case '|':
		case '}':
		case '~':
			break;
		}
		return true;
	}

	private static bool IsHighSurrogate(char c)
	{
		if ('\ud800' <= c)
		{
			return c <= '\udbff';
		}
		return false;
	}

	private static bool IsLowSurrogate(char c)
	{
		if ('\udc00' <= c)
		{
			return c <= '\udfff';
		}
		return false;
	}

	private void EmitSequenceStart(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent evt)
	{
		ProcessAnchor();
		ProcessTag();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart = (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart)evt;
		if (flowLevel != 0 || isCanonical || _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart.Style == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStyle.Flow || CheckEmptySequence())
		{
			state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.FlowSequenceFirstItem;
		}
		else
		{
			state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.BlockSequenceFirstItem;
		}
	}

	private void EmitMappingStart(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent evt)
	{
		ProcessAnchor();
		ProcessTag();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart = (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart)evt;
		if (flowLevel != 0 || isCanonical || _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart.Style == _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStyle.Flow || CheckEmptyMapping())
		{
			state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.FlowMappingFirstKey;
		}
		else
		{
			state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.BlockMappingFirstKey;
		}
	}

	private void ProcessAnchor()
	{
		if (!anchorData.Anchor.IsEmpty && !skipAnchorName)
		{
			WriteIndicator(anchorData.IsAlias ? "*" : "&", needWhitespace: true, whitespace: false, indentation: false);
			WriteAnchor(anchorData.Anchor);
		}
	}

	private void ProcessTag()
	{
		if (tagData.Handle == null && tagData.Suffix == null)
		{
			return;
		}
		if (tagData.Handle != null)
		{
			WriteTagHandle(tagData.Handle);
			if (tagData.Suffix != null)
			{
				WriteTagContent(tagData.Suffix, needsWhitespace: false);
			}
		}
		else
		{
			WriteIndicator("!<", needWhitespace: true, whitespace: false, indentation: false);
			WriteTagContent(tagData.Suffix, needsWhitespace: false);
			WriteIndicator(">", needWhitespace: false, whitespace: false, indentation: false);
		}
	}

	private void EmitDocumentEnd(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent evt)
	{
		if (evt is YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd)
		{
			WriteIndent();
			if (!_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd.IsImplicit)
			{
				WriteIndicator("...", needWhitespace: true, whitespace: false, indentation: false);
				WriteIndent();
				isDocumentEndWritten = true;
			}
			state = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.DocumentStart;
			tagDirectives.Clear();
			return;
		}
		throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlException("Expected DOCUMENT-END.");
	}

	private void EmitFlowSequenceItem(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent evt, bool isFirst)
	{
		if (isFirst)
		{
			WriteIndicator("[", needWhitespace: true, whitespace: true, indentation: false);
			IncreaseIndent(isFlow: true, isIndentless: false);
			flowLevel++;
		}
		if (evt is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceEnd)
		{
			flowLevel--;
			indent = indents.Pop();
			if (isCanonical && !isFirst)
			{
				WriteIndicator(",", needWhitespace: false, whitespace: false, indentation: false);
				WriteIndent();
			}
			WriteIndicator("]", needWhitespace: false, whitespace: false, indentation: false);
			state = states.Pop();
		}
		else
		{
			if (!isFirst)
			{
				WriteIndicator(",", needWhitespace: false, whitespace: false, indentation: false);
			}
			if (isCanonical || column > bestWidth)
			{
				WriteIndent();
			}
			states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.FlowSequenceItem);
			EmitNode(evt, isMapping: false, isSimpleKey: false);
		}
	}

	private void EmitFlowMappingKey(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent evt, bool isFirst)
	{
		if (isFirst)
		{
			WriteIndicator("{", needWhitespace: true, whitespace: true, indentation: false);
			IncreaseIndent(isFlow: true, isIndentless: false);
			flowLevel++;
		}
		if (evt is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd)
		{
			flowLevel--;
			indent = indents.Pop();
			if (isCanonical && !isFirst)
			{
				WriteIndicator(",", needWhitespace: false, whitespace: false, indentation: false);
				WriteIndent();
			}
			WriteIndicator("}", needWhitespace: false, whitespace: false, indentation: false);
			state = states.Pop();
			return;
		}
		if (!isFirst)
		{
			WriteIndicator(",", needWhitespace: false, whitespace: false, indentation: false);
		}
		if (isCanonical || column > bestWidth)
		{
			WriteIndent();
		}
		if (!isCanonical && CheckSimpleKey())
		{
			states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.FlowMappingSimpleValue);
			EmitNode(evt, isMapping: true, isSimpleKey: true);
		}
		else
		{
			WriteIndicator("?", needWhitespace: true, whitespace: false, indentation: false);
			states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.FlowMappingValue);
			EmitNode(evt, isMapping: true, isSimpleKey: false);
		}
	}

	private void EmitFlowMappingValue(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent evt, bool isSimple)
	{
		if (isSimple)
		{
			WriteIndicator(":", needWhitespace: false, whitespace: false, indentation: false);
		}
		else
		{
			if (isCanonical || column > bestWidth)
			{
				WriteIndent();
			}
			WriteIndicator(":", needWhitespace: true, whitespace: false, indentation: false);
		}
		states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.FlowMappingKey);
		EmitNode(evt, isMapping: true, isSimpleKey: false);
	}

	private void EmitBlockSequenceItem(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent evt, bool isFirst)
	{
		if (isFirst)
		{
			IncreaseIndent(isFlow: false, isMappingContext && !isIndentation);
		}
		if (evt is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceEnd)
		{
			indent = indents.Pop();
			state = states.Pop();
			return;
		}
		WriteIndent();
		WriteIndicator("-", needWhitespace: true, whitespace: false, indentation: true);
		states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.BlockSequenceItem);
		EmitNode(evt, isMapping: false, isSimpleKey: false);
	}

	private void EmitBlockMappingKey(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent evt, bool isFirst)
	{
		if (isFirst)
		{
			IncreaseIndent(isFlow: false, isIndentless: false);
		}
		if (evt is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd)
		{
			indent = indents.Pop();
			state = states.Pop();
			return;
		}
		WriteIndent();
		if (CheckSimpleKey())
		{
			states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.BlockMappingSimpleValue);
			EmitNode(evt, isMapping: true, isSimpleKey: true);
			WriteIndicator(":", needWhitespace: false, whitespace: false, indentation: false);
		}
		else
		{
			WriteIndicator("?", needWhitespace: true, whitespace: false, indentation: true);
			states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.BlockMappingValue);
			EmitNode(evt, isMapping: true, isSimpleKey: false);
		}
	}

	private void EmitBlockMappingValue(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent evt, bool isSimple)
	{
		if (!isSimple)
		{
			WriteIndent();
			WriteIndicator(":", needWhitespace: true, whitespace: false, indentation: true);
		}
		states.Push(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitterState.BlockMappingKey);
		EmitNode(evt, isMapping: true, isSimpleKey: false);
	}

	private void IncreaseIndent(bool isFlow, bool isIndentless)
	{
		indents.Push(indent);
		if (indent < 0)
		{
			indent = (isFlow ? bestIndent : 0);
		}
		else if (!isIndentless || !forceIndentLess)
		{
			indent += bestIndent;
		}
	}

	private bool CheckEmptyDocument()
	{
		int num = 0;
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent @event in events)
		{
			num++;
			if (num == 2)
			{
				if (@event is YamlDotNet.Core.Events._003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar)
				{
					return string.IsNullOrEmpty(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.Value);
				}
				break;
			}
		}
		return false;
	}

	private bool CheckSimpleKey()
	{
		if (events.Count < 1)
		{
			return false;
		}
		int num;
		switch (events.Peek().Type)
		{
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.Alias:
			num = AnchorNameLength(anchorData.Anchor);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.Scalar:
			if (scalarData.IsMultiline)
			{
				return false;
			}
			num = AnchorNameLength(anchorData.Anchor) + SafeStringLength(tagData.Handle) + SafeStringLength(tagData.Suffix) + SafeStringLength(scalarData.Value);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.SequenceStart:
			if (!CheckEmptySequence())
			{
				return false;
			}
			num = AnchorNameLength(anchorData.Anchor) + SafeStringLength(tagData.Handle) + SafeStringLength(tagData.Suffix);
			break;
		case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEventType.MappingStart:
			if (!CheckEmptySequence())
			{
				return false;
			}
			num = AnchorNameLength(anchorData.Anchor) + SafeStringLength(tagData.Handle) + SafeStringLength(tagData.Suffix);
			break;
		default:
			return false;
		}
		return num <= maxSimpleKeyLength;
	}

	private static int AnchorNameLength(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName value)
	{
		if (!value.IsEmpty)
		{
			return value.Value.Length;
		}
		return 0;
	}

	private static int SafeStringLength(string? value)
	{
		return value?.Length ?? 0;
	}

	private bool CheckEmptySequence()
	{
		return CheckEmptyStructure<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceEnd>();
	}

	private bool CheckEmptyMapping()
	{
		return CheckEmptyStructure<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd>();
	}

	private bool CheckEmptyStructure<TStart, TEnd>() where TStart : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent where TEnd : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent
	{
		if (events.Count < 2)
		{
			return false;
		}
		using Queue<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent>.Enumerator enumerator = events.GetEnumerator();
		return enumerator.MoveNext() && enumerator.Current is TStart && enumerator.MoveNext() && enumerator.Current is TEnd;
	}

	private void WriteBlockScalarHints(string value)
	{
		StringLookAheadBufferPool.BufferWrapper bufferWrapper = StringLookAheadBufferPool.Rent(value);
		try
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringLookAheadBuffer> _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2 = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStringLookAheadBuffer>(bufferWrapper.Buffer);
			if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.IsSpace() || _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.IsBreak())
			{
				int num = bestIndent;
				string indicator = num.ToString(CultureInfo.InvariantCulture);
				WriteIndicator(indicator, needWhitespace: false, whitespace: false, indentation: false);
			}
			string text = null;
			if (value.Length == 0 || !_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.IsBreak(value.Length - 1))
			{
				text = "-";
			}
			else if (value.Length >= 2 && _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer2.IsBreak(value.Length - 2))
			{
				text = "+";
			}
			if (text != null)
			{
				WriteIndicator(text, needWhitespace: false, whitespace: false, indentation: false);
			}
		}
		finally
		{
			((IDisposable)bufferWrapper).Dispose();
		}
	}

	private void WriteIndicator(string indicator, bool needWhitespace, bool whitespace, bool indentation)
	{
		if (needWhitespace && !isWhitespace)
		{
			Write(' ');
		}
		Write(indicator);
		isWhitespace = whitespace;
		isIndentation &= indentation;
	}

	private void WriteIndent()
	{
		int num = Math.Max(indent, 0);
		if (!isIndentation || column > num || (column == num && !isWhitespace))
		{
			WriteBreak();
		}
		while (column < num)
		{
			Write(' ');
		}
		isWhitespace = true;
		isIndentation = true;
	}

	private void WriteAnchor(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName value)
	{
		Write(value.Value);
		isWhitespace = false;
		isIndentation = false;
	}

	private void WriteTagHandle(string value)
	{
		if (!isWhitespace)
		{
			Write(' ');
		}
		Write(value);
		isWhitespace = false;
		isIndentation = false;
	}

	private void WriteTagContent(string value, bool needsWhitespace)
	{
		if (needsWhitespace && !isWhitespace)
		{
			Write(' ');
		}
		Write(UrlEncode(value));
		isWhitespace = false;
		isIndentation = false;
	}

	private static string UrlEncode(string text)
	{
		return UriReplacer.Replace(text, delegate(Match match)
		{
			StringBuilderPool.BuilderWrapper builderWrapper = StringBuilderPool.Rent();
			try
			{
				StringBuilder builder = builderWrapper.Builder;
				byte[] bytes = Encoding.UTF8.GetBytes(match.Value);
				foreach (byte b in bytes)
				{
					builder.AppendFormat(CultureInfo.InvariantCulture, "%{0:X02}", b);
				}
				return builder.ToString();
			}
			finally
			{
				((IDisposable)builderWrapper).Dispose();
			}
		});
	}

	private void Write(char value)
	{
		output.Write(value);
		column++;
	}

	private void Write(string value)
	{
		output.Write(value);
		column += value.Length;
	}

	private void WriteBreak(char breakCharacter = '\n')
	{
		if (breakCharacter == '\n')
		{
			output.WriteLine();
		}
		else
		{
			output.Write(breakCharacter);
		}
		column = 0;
	}
}
