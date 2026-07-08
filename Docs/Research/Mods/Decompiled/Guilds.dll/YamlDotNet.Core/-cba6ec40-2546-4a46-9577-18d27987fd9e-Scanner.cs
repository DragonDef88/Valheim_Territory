using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Core.ObjectPool;
using YamlDotNet.Core.Tokens;

namespace YamlDotNet.Core;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScanner : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIScanner
{
	private const int MaxVersionNumberLength = 9;

	private static readonly SortedDictionary<char, char> SimpleEscapeCodes = new SortedDictionary<char, char>
	{
		{ '0', '\0' },
		{ 'a', '\a' },
		{ 'b', '\b' },
		{ 't', '\t' },
		{ '\t', '\t' },
		{ 'n', '\n' },
		{ 'v', '\v' },
		{ 'f', '\f' },
		{ 'r', '\r' },
		{ 'e', '\u001b' },
		{ ' ', ' ' },
		{ '"', '"' },
		{ '\\', '\\' },
		{ '/', '/' },
		{ 'N', '\u0085' },
		{ '_', '\u00a0' },
		{ 'L', '\u2028' },
		{ 'P', '\u2029' }
	};

	private readonly Stack<long> indents = new Stack<long>();

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EInsertionQueue<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken> tokens = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EInsertionQueue<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken>();

	private readonly Stack<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey> simpleKeys = new Stack<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey>();

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELookAheadBuffer> analyzer;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECursor cursor;

	private bool streamStartProduced;

	private bool streamEndProduced;

	private bool plainScalarFollowedByComment;

	private bool flowCollectionFetched;

	private bool startFlowCollectionFetched;

	private long indent = -1L;

	private bool flowScalarFetched;

	private bool simpleKeyAllowed;

	private int flowLevel;

	private int tokensParsed;

	private bool tokenAvailable;

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken? previous;

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchor? previousAnchor;

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar? lastScalar;

	private readonly int maxKeySize;

	private static readonly byte[] EmptyBytes = Array.Empty<byte>();

	public bool SkipComments { get; private set; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken? Current { get; private set; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark CurrentPosition => cursor.Mark();

	private bool IsDocumentStart()
	{
		if (!analyzer.EndOfInput && cursor.LineOffset == 0L && analyzer.Check('-') && analyzer.Check('-', 1) && analyzer.Check('-', 2))
		{
			return analyzer.IsWhiteBreakOrZero(3);
		}
		return false;
	}

	private bool IsDocumentEnd()
	{
		if (!analyzer.EndOfInput && cursor.LineOffset == 0L && analyzer.Check('.') && analyzer.Check('.', 1) && analyzer.Check('.', 2))
		{
			return analyzer.IsWhiteBreakOrZero(3);
		}
		return false;
	}

	private bool IsDocumentIndicator()
	{
		if (!IsDocumentStart())
		{
			return IsDocumentEnd();
		}
		return true;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScanner(TextReader input, bool skipComments = true)
		: this(input, skipComments, 1024)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScanner(TextReader input, bool skipComments, int maxKeySize)
	{
		analyzer = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECharacterAnalyzer<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELookAheadBuffer>(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELookAheadBuffer(input, 1024));
		cursor = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECursor();
		SkipComments = skipComments;
		this.maxKeySize = maxKeySize;
	}

	public bool MoveNext()
	{
		if (Current != null)
		{
			ConsumeCurrent();
		}
		return MoveNextWithoutConsuming();
	}

	public bool MoveNextWithoutConsuming()
	{
		if (!tokenAvailable && !streamEndProduced)
		{
			FetchMoreTokens();
		}
		if (tokens.Count > 0)
		{
			Current = tokens.Dequeue();
			tokenAvailable = false;
			return true;
		}
		Current = null;
		return false;
	}

	public void ConsumeCurrent()
	{
		tokensParsed++;
		tokenAvailable = false;
		previous = Current;
		Current = null;
	}

	private char ReadCurrentCharacter()
	{
		char result = analyzer.Peek(0);
		Skip();
		return result;
	}

	private char ReadLine()
	{
		if (analyzer.Check("\r\n\u0085"))
		{
			SkipLine();
			return '\n';
		}
		char result = analyzer.Peek(0);
		SkipLine();
		return result;
	}

	private void FetchMoreTokens()
	{
		while (true)
		{
			bool flag = false;
			if (tokens.Count == 0)
			{
				flag = true;
			}
			else
			{
				foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey simpleKey in simpleKeys)
				{
					if (simpleKey.IsPossible && simpleKey.TokenNumber == tokensParsed)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				break;
			}
			FetchNextToken();
		}
		tokenAvailable = true;
	}

	private static bool StartsWith(StringBuilder what, char start)
	{
		if (what.Length > 0)
		{
			return what[0] == start;
		}
		return false;
	}

	private void StaleSimpleKeys()
	{
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey simpleKey in simpleKeys)
		{
			if (simpleKey.IsPossible && (simpleKey.Line < cursor.Line || simpleKey.Index + maxKeySize < cursor.Index))
			{
				if (simpleKey.IsRequired)
				{
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark2 = cursor.Mark();
					tokens.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError("While scanning a simple key, could not find expected ':'.", _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark2, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark2));
				}
				simpleKey.MarkAsImpossible();
			}
		}
	}

	private void FetchNextToken()
	{
		if (!streamStartProduced)
		{
			FetchStreamStart();
			return;
		}
		ScanToNextToken();
		StaleSimpleKeys();
		UnrollIndent(cursor.LineOffset);
		analyzer.Buffer.Cache(4);
		if (analyzer.Buffer.EndOfInput)
		{
			lastScalar = null;
			FetchStreamEnd();
		}
		if (cursor.LineOffset == 0L && analyzer.Check('%'))
		{
			lastScalar = null;
			FetchDirective();
			return;
		}
		if (IsDocumentStart())
		{
			lastScalar = null;
			FetchDocumentIndicator(isStartToken: true);
			return;
		}
		if (IsDocumentEnd())
		{
			lastScalar = null;
			FetchDocumentIndicator(isStartToken: false);
			return;
		}
		if (analyzer.Check('['))
		{
			lastScalar = null;
			FetchFlowCollectionStart(isSequenceToken: true);
			return;
		}
		if (analyzer.Check('{'))
		{
			lastScalar = null;
			FetchFlowCollectionStart(isSequenceToken: false);
			return;
		}
		if (analyzer.Check(']'))
		{
			lastScalar = null;
			FetchFlowCollectionEnd(isSequenceToken: true);
			return;
		}
		if (analyzer.Check('}'))
		{
			lastScalar = null;
			FetchFlowCollectionEnd(isSequenceToken: false);
			return;
		}
		if (analyzer.Check(','))
		{
			lastScalar = null;
			FetchFlowEntry();
			return;
		}
		if (analyzer.Check('-'))
		{
			if (analyzer.IsWhiteBreakOrZero(1))
			{
				FetchBlockEntry();
				return;
			}
			if (flowLevel > 0 && analyzer.Check(",[]{}", 1))
			{
				tokens.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError("Invalid key indicator format.", cursor.Mark(), cursor.Mark()));
			}
		}
		if (analyzer.Check('?') && (flowLevel > 0 || analyzer.IsWhiteBreakOrZero(1)) && analyzer.IsWhiteBreakOrZero(1))
		{
			FetchKey();
		}
		else if (analyzer.Check(':') && (flowLevel > 0 || analyzer.IsWhiteBreakOrZero(1)) && (!simpleKeyAllowed || flowLevel <= 0) && (!flowScalarFetched || !analyzer.Check(':', 1)) && (analyzer.IsWhiteBreakOrZero(1) || analyzer.Check(',', 1) || flowScalarFetched || flowCollectionFetched || startFlowCollectionFetched))
		{
			if (lastScalar != null)
			{
				lastScalar.IsKey = true;
				lastScalar = null;
			}
			FetchValue();
		}
		else if (analyzer.Check('*'))
		{
			FetchAnchor(isAlias: true);
		}
		else if (analyzer.Check('&'))
		{
			FetchAnchor(isAlias: false);
		}
		else if (analyzer.Check('!'))
		{
			FetchTag();
		}
		else if (analyzer.Check('|') && flowLevel == 0)
		{
			FetchBlockScalar(isLiteral: true);
		}
		else if (analyzer.Check('>') && flowLevel == 0)
		{
			FetchBlockScalar(isLiteral: false);
		}
		else if (analyzer.Check('\''))
		{
			FetchQuotedScalar(isSingleQuoted: true);
		}
		else if (analyzer.Check('"'))
		{
			FetchQuotedScalar(isSingleQuoted: false);
		}
		else if ((!analyzer.IsWhiteBreakOrZero() && !analyzer.Check("-?:,[]{}#&*!|>'\"%@`")) || (analyzer.Check('-') && !analyzer.IsWhite(1)) || (analyzer.Check("?:") && !analyzer.IsWhiteBreakOrZero(1)) || (simpleKeyAllowed && flowLevel > 0))
		{
			if (plainScalarFollowedByComment)
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark2 = cursor.Mark();
				tokens.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError("While scanning plain scalar, found a comment between adjacent scalars.", _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark2, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark2));
			}
			if ((flowScalarFetched || (flowCollectionFetched && !startFlowCollectionFetched)) && analyzer.Check(':'))
			{
				Skip();
			}
			flowScalarFetched = false;
			flowCollectionFetched = false;
			startFlowCollectionFetched = false;
			plainScalarFollowedByComment = false;
			FetchPlainScalar();
		}
		else
		{
			if (simpleKeyAllowed && indent >= cursor.LineOffset && analyzer.IsTab())
			{
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException("While scanning a mapping, found invalid tab as indentation.");
			}
			if (!analyzer.IsWhiteBreakOrZero())
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = cursor.Mark();
				Skip();
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning for the next token, found character that cannot start any token.");
			}
			Skip();
		}
	}

	private bool CheckWhiteSpace()
	{
		if (!analyzer.Check(' '))
		{
			if (flowLevel > 0 || !simpleKeyAllowed)
			{
				return analyzer.Check('\t');
			}
			return false;
		}
		return true;
	}

	private void Skip()
	{
		cursor.Skip();
		analyzer.Buffer.Skip(1);
	}

	private void SkipLine()
	{
		if (analyzer.IsCrLf())
		{
			cursor.SkipLineByOffset(2);
			analyzer.Buffer.Skip(2);
		}
		else if (analyzer.IsBreak())
		{
			cursor.SkipLineByOffset(1);
			analyzer.Buffer.Skip(1);
		}
		else if (!analyzer.IsZero())
		{
			throw new InvalidOperationException("Not at a break.");
		}
	}

	private void ScanToNextToken()
	{
		while (true)
		{
			if (CheckWhiteSpace())
			{
				Skip();
				continue;
			}
			ProcessComment();
			if (analyzer.IsBreak())
			{
				SkipLine();
				if (flowLevel == 0)
				{
					simpleKeyAllowed = true;
				}
				continue;
			}
			break;
		}
	}

	private void ProcessComment()
	{
		if (!analyzer.Check('#'))
		{
			return;
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = cursor.Mark();
		Skip();
		while (analyzer.IsSpace())
		{
			Skip();
		}
		StringBuilderPool.BuilderWrapper builderWrapper = StringBuilderPool.Rent();
		try
		{
			StringBuilder builder = builderWrapper.Builder;
			while (!analyzer.IsBreakOrZero())
			{
				builder.Append(ReadCurrentCharacter());
			}
			if (!SkipComments)
			{
				bool isInline = previous != null && previous.End.Line == start.Line && previous.End.Column != 1 && !(previous is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamStart);
				tokens.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EComment(builder.ToString(), isInline, start, cursor.Mark()));
			}
		}
		finally
		{
			((IDisposable)builderWrapper).Dispose();
		}
	}

	private void FetchStreamStart()
	{
		simpleKeys.Push(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey());
		simpleKeyAllowed = true;
		streamStartProduced = true;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = cursor.Mark();
		tokens.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamStart(in start, in start));
	}

	private void UnrollIndent(long column)
	{
		if (flowLevel == 0)
		{
			while (indent > column)
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = cursor.Mark();
				tokens.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEnd(in start, in start));
				indent = indents.Pop();
			}
		}
	}

	private void FetchStreamEnd()
	{
		cursor.ForceSkipLineAfterNonBreak();
		UnrollIndent(-1L);
		RemoveSimpleKey();
		simpleKeyAllowed = false;
		streamEndProduced = true;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = cursor.Mark();
		tokens.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd(in start, in start));
	}

	private void FetchDirective()
	{
		UnrollIndent(-1L);
		RemoveSimpleKey();
		simpleKeyAllowed = false;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = ScanDirective();
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken != null)
		{
			tokens.Enqueue(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken);
		}
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken? ScanDirective()
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = cursor.Mark();
		Skip();
		string text = ScanDirectiveName(in start);
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken result;
		if (!(text == "YAML"))
		{
			if (!(text == "TAG"))
			{
				while (!analyzer.EndOfInput && !analyzer.Check('#') && !analyzer.IsBreak())
				{
					Skip();
				}
				return null;
			}
			result = ScanTagDirectiveValue(in start);
		}
		else
		{
			if (!(previous is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart) && !(previous is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamStart) && !(previous is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd))
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start, in end, "While scanning a version directive, did not find preceding <document end>.");
			}
			result = ScanVersionDirectiveValue(in start);
		}
		while (analyzer.IsWhite())
		{
			Skip();
		}
		ProcessComment();
		if (!analyzer.IsBreakOrZero())
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a directive, did not find expected comment or line break.");
		}
		if (analyzer.IsBreak())
		{
			SkipLine();
		}
		return result;
	}

	private void FetchDocumentIndicator(bool isStartToken)
	{
		UnrollIndent(-1L);
		RemoveSimpleKey();
		simpleKeyAllowed = false;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = cursor.Mark();
		Skip();
		Skip();
		Skip();
		if (isStartToken)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EInsertionQueue<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken> obj = tokens;
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
			obj.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart(in start, in end));
			return;
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = null;
		while (!analyzer.EndOfInput && !analyzer.IsBreak() && !analyzer.Check('#'))
		{
			if (!analyzer.IsWhite())
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError("While scanning a document end, found invalid content after '...' marker.", start, cursor.Mark());
				break;
			}
			Skip();
		}
		tokens.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd(in start, in start));
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken != null)
		{
			tokens.Enqueue(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken);
		}
	}

	private void FetchFlowCollectionStart(bool isSequenceToken)
	{
		SaveSimpleKey();
		IncreaseFlowLevel();
		simpleKeyAllowed = true;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = cursor.Mark();
		Skip();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken item = ((!isSequenceToken) ? ((_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken)new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowMappingStart(in start, in start)) : ((_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken)new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowSequenceStart(in start, in start)));
		tokens.Enqueue(item);
		startFlowCollectionFetched = true;
	}

	private void IncreaseFlowLevel()
	{
		simpleKeys.Push(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey());
		flowLevel++;
	}

	private void FetchFlowCollectionEnd(bool isSequenceToken)
	{
		RemoveSimpleKey();
		DecreaseFlowLevel();
		simpleKeyAllowed = false;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = cursor.Mark();
		Skip();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = null;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken item;
		if (isSequenceToken)
		{
			if (analyzer.Check('#'))
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError("While scanning a flow sequence end, found invalid comment after ']'.", start, start);
			}
			item = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowSequenceEnd(in start, in start);
		}
		else
		{
			item = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowMappingEnd(in start, in start);
		}
		tokens.Enqueue(item);
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken != null)
		{
			tokens.Enqueue(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken);
		}
		flowCollectionFetched = true;
	}

	private void DecreaseFlowLevel()
	{
		if (flowLevel > 0)
		{
			flowLevel--;
			simpleKeys.Pop();
		}
	}

	private void FetchFlowEntry()
	{
		RemoveSimpleKey();
		simpleKeyAllowed = true;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = cursor.Mark();
		Skip();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
		if (analyzer.Check('#'))
		{
			tokens.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError("While scanning a flow entry, found invalid comment after comma.", start, end));
		}
		else
		{
			tokens.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFlowEntry(in start, in end));
		}
	}

	private void FetchBlockEntry()
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start;
		if (flowLevel == 0)
		{
			if (!simpleKeyAllowed)
			{
				if (previousAnchor != null && previousAnchor.End.Line == cursor.Line)
				{
					start = previousAnchor.Start;
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = previousAnchor.End;
					throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in start, in end, "Anchor before sequence entry on same line is not allowed.");
				}
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark2 = cursor.Mark();
				tokens.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError("Block sequence entries are not allowed in this context.", _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark2, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark2));
			}
			RollIndent(cursor.LineOffset, -1, isSequence: true, cursor.Mark());
		}
		RemoveSimpleKey();
		simpleKeyAllowed = true;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start2 = cursor.Mark();
		Skip();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EInsertionQueue<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken> obj = tokens;
		start = cursor.Mark();
		obj.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockEntry(in start2, in start));
	}

	private void FetchKey()
	{
		if (flowLevel == 0)
		{
			if (!simpleKeyAllowed)
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = cursor.Mark();
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in start, "Mapping keys are not allowed in this context.");
			}
			RollIndent(cursor.LineOffset, -1, isSequence: false, cursor.Mark());
		}
		RemoveSimpleKey();
		simpleKeyAllowed = flowLevel == 0;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start2 = cursor.Mark();
		Skip();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EInsertionQueue<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken> obj = tokens;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
		obj.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EKey(in start2, in end));
	}

	private void FetchValue()
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2 = simpleKeys.Peek();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start;
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.IsPossible)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EInsertionQueue<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken> obj = tokens;
			int index = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.TokenNumber - tokensParsed;
			start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.Mark;
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.Mark;
			obj.Insert(index, new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EKey(in start, in end));
			RollIndent(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.LineOffset, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.TokenNumber, isSequence: false, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.Mark);
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.MarkAsImpossible();
			simpleKeyAllowed = false;
		}
		else
		{
			bool flag = flowLevel == 0;
			if (flag)
			{
				if (!simpleKeyAllowed)
				{
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark2 = cursor.Mark();
					tokens.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError("Mapping values are not allowed in this context.", _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark2, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark2));
					return;
				}
				RollIndent(cursor.LineOffset, -1, isSequence: false, cursor.Mark());
				if (cursor.LineOffset == 0L && _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.LineOffset == 0L)
				{
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EInsertionQueue<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken> obj2 = tokens;
					int count = tokens.Count;
					start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.Mark;
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.Mark;
					obj2.Insert(count, new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EKey(in start, in end));
					flag = false;
				}
			}
			simpleKeyAllowed = flag;
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start2 = cursor.Mark();
		Skip();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EInsertionQueue<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken> obj3 = tokens;
		start = cursor.Mark();
		obj3.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EValue(in start2, in start));
	}

	private void RollIndent(long column, int number, bool isSequence, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark position)
	{
		if (flowLevel <= 0 && indent < column)
		{
			indents.Push(indent);
			indent = column;
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken item = ((!isSequence) ? ((_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken)new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockMappingStart(in position, in position)) : ((_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken)new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBlockSequenceStart(in position, in position)));
			if (number == -1)
			{
				tokens.Enqueue(item);
			}
			else
			{
				tokens.Insert(number - tokensParsed, item);
			}
		}
	}

	private void FetchAnchor(bool isAlias)
	{
		SaveSimpleKey();
		simpleKeyAllowed = false;
		tokens.Enqueue(ScanAnchor(isAlias));
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EToken ScanAnchor(bool isAlias)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = cursor.Mark();
		Skip();
		bool flag = false;
		if (isAlias)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2 = simpleKeys.Peek();
			flag = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.IsRequired && _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.IsPossible;
		}
		StringBuilderPool.BuilderWrapper builderWrapper = StringBuilderPool.Rent();
		try
		{
			StringBuilder builder = builderWrapper.Builder;
			while (!analyzer.IsWhiteBreakOrZero() && !analyzer.Check("[]{},") && (!flag || !analyzer.Check(':') || !analyzer.IsWhiteBreakOrZero(1)))
			{
				builder.Append(ReadCurrentCharacter());
			}
			if (builder.Length == 0 || (!analyzer.IsWhiteBreakOrZero() && !analyzer.Check("?:,]}%@`")))
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning an anchor or alias, found value containing disallowed: []{},");
			}
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName value = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName(builder.ToString());
			if (isAlias)
			{
				return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias(value, start, cursor.Mark());
			}
			return previousAnchor = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchor(value, start, cursor.Mark());
		}
		finally
		{
			((IDisposable)builderWrapper).Dispose();
		}
	}

	private void FetchTag()
	{
		SaveSimpleKey();
		simpleKeyAllowed = false;
		tokens.Enqueue(ScanTag());
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag ScanTag()
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = cursor.Mark();
		string text;
		string text2;
		if (analyzer.Check('<', 1))
		{
			text = string.Empty;
			Skip();
			Skip();
			text2 = ScanTagUri(null, start);
			if (!analyzer.Check('>'))
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a tag, did not find the expected '>'.");
			}
			Skip();
		}
		else
		{
			string text3 = ScanTagHandle(isDirective: false, start);
			if (text3.Length > 1 && text3[0] == '!' && text3[text3.Length - 1] == '!')
			{
				text = text3;
				text2 = ScanTagUri(null, start);
			}
			else
			{
				text2 = ScanTagUri(text3, start);
				text = "!";
				if (text2.Length == 0)
				{
					text2 = text;
					text = string.Empty;
				}
			}
		}
		if (!analyzer.IsWhiteBreakOrZero() && !analyzer.Check(','))
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a tag, did not find expected whitespace, comma or line break.");
		}
		return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETag(text, text2, start, cursor.Mark());
	}

	private void FetchBlockScalar(bool isLiteral)
	{
		SaveSimpleKey();
		simpleKeyAllowed = true;
		tokens.Enqueue(ScanBlockScalar(isLiteral));
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar ScanBlockScalar(bool isLiteral)
	{
		StringBuilderPool.BuilderWrapper builderWrapper = StringBuilderPool.Rent();
		try
		{
			StringBuilder builder = builderWrapper.Builder;
			StringBuilderPool.BuilderWrapper builderWrapper2 = StringBuilderPool.Rent();
			try
			{
				StringBuilder builder2 = builderWrapper2.Builder;
				StringBuilderPool.BuilderWrapper builderWrapper3 = StringBuilderPool.Rent();
				try
				{
					StringBuilder builder3 = builderWrapper3.Builder;
					int num = 0;
					int num2 = 0;
					long currentIndent = 0L;
					bool flag = false;
					bool? isFirstLine = null;
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = cursor.Mark();
					Skip();
					if (analyzer.Check("+-"))
					{
						num = (analyzer.Check('+') ? 1 : (-1));
						Skip();
						if (analyzer.IsDigit())
						{
							if (analyzer.Check('0'))
							{
								_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
								throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a block scalar, found an indentation indicator equal to 0.");
							}
							num2 = analyzer.AsDigit();
							Skip();
						}
					}
					else if (analyzer.IsDigit())
					{
						if (analyzer.Check('0'))
						{
							_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
							throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a block scalar, found an indentation indicator equal to 0.");
						}
						num2 = analyzer.AsDigit();
						Skip();
						if (analyzer.Check("+-"))
						{
							num = (analyzer.Check('+') ? 1 : (-1));
							Skip();
						}
					}
					if (analyzer.Check('#'))
					{
						_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
						throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a block scalar, found a comment without whtespace after '>' indicator.");
					}
					while (analyzer.IsWhite())
					{
						Skip();
					}
					ProcessComment();
					if (!analyzer.IsBreakOrZero())
					{
						_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
						throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a block scalar, did not find expected comment or line break.");
					}
					if (analyzer.IsBreak())
					{
						SkipLine();
						if (!isFirstLine.HasValue)
						{
							isFirstLine = true;
						}
						else if (isFirstLine.GetValueOrDefault())
						{
							isFirstLine = false;
						}
					}
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end2 = cursor.Mark();
					if (num2 != 0)
					{
						currentIndent = ((indent >= 0) ? (indent + num2) : num2);
					}
					currentIndent = ScanBlockScalarBreaks(currentIndent, builder3, isLiteral, ref end2, ref isFirstLine);
					isFirstLine = false;
					while (cursor.LineOffset == currentIndent && !analyzer.IsZero() && !IsDocumentEnd())
					{
						bool flag2 = analyzer.IsWhite();
						if (!isLiteral && StartsWith(builder2, '\n') && !flag && !flag2)
						{
							if (builder3.Length == 0)
							{
								builder.Append(' ');
							}
							builder2.Length = 0;
						}
						else
						{
							builder.Append((object?)builder2);
							builder2.Length = 0;
						}
						builder.Append((object?)builder3);
						builder3.Length = 0;
						flag = analyzer.IsWhite();
						while (!analyzer.IsBreakOrZero())
						{
							builder.Append(ReadCurrentCharacter());
						}
						char c = ReadLine();
						if (c != 0)
						{
							builder2.Append(c);
						}
						currentIndent = ScanBlockScalarBreaks(currentIndent, builder3, isLiteral, ref end2, ref isFirstLine);
					}
					if (num != -1)
					{
						builder.Append((object?)builder2);
					}
					if (num == 1)
					{
						builder.Append((object?)builder3);
					}
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle style = (isLiteral ? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Literal : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Folded);
					return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar(builder.ToString(), style, start, end2);
				}
				finally
				{
					((IDisposable)builderWrapper3).Dispose();
				}
			}
			finally
			{
				((IDisposable)builderWrapper2).Dispose();
			}
		}
		finally
		{
			((IDisposable)builderWrapper).Dispose();
		}
	}

	private long ScanBlockScalarBreaks(long currentIndent, StringBuilder breaks, bool isLiteral, ref _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end, ref bool? isFirstLine)
	{
		long num = 0L;
		long num2 = -1L;
		end = cursor.Mark();
		while (true)
		{
			if ((currentIndent == 0L || cursor.LineOffset < currentIndent) && analyzer.IsSpace())
			{
				Skip();
				continue;
			}
			if (cursor.LineOffset > num)
			{
				num = cursor.LineOffset;
			}
			if (!analyzer.IsBreak())
			{
				break;
			}
			if (isFirstLine.GetValueOrDefault())
			{
				isFirstLine = false;
				num2 = cursor.LineOffset;
			}
			breaks.Append(ReadLine());
			end = cursor.Mark();
		}
		if (isLiteral && isFirstLine.GetValueOrDefault())
		{
			long num3 = cursor.LineOffset;
			int num4 = 0;
			while (!analyzer.IsBreak(num4) && analyzer.IsSpace(num4))
			{
				num4++;
				num3++;
			}
			if (analyzer.IsBreak(num4) && num3 > cursor.LineOffset)
			{
				isFirstLine = false;
				num2 = num3;
			}
		}
		if (isLiteral && num2 > 1 && currentIndent < num2 - 1)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end2 = cursor.Mark();
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in end, in end2, "While scanning a literal block scalar, found extra spaces in first line.");
		}
		if (!isLiteral && num > cursor.LineOffset && num2 > -1)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end2 = cursor.Mark();
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESemanticErrorException(in end, in end2, "While scanning a literal block scalar, found more spaces in lines above first content line.");
		}
		if (currentIndent == 0L && (cursor.LineOffset > 0 || indent > -1))
		{
			currentIndent = Math.Max(num, Math.Max(indent + 1, 1L));
		}
		return currentIndent;
	}

	private void FetchQuotedScalar(bool isSingleQuoted)
	{
		SaveSimpleKey();
		simpleKeyAllowed = false;
		flowScalarFetched = flowLevel > 0;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar item = ScanFlowScalar(isSingleQuoted);
		tokens.Enqueue(item);
		lastScalar = item;
		if (!isSingleQuoted && analyzer.Check('#'))
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark2 = cursor.Mark();
			tokens.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError("While scanning a flow sequence end, found invalid comment after double-quoted scalar.", _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark2, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark2));
		}
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar ScanFlowScalar(bool isSingleQuoted)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = cursor.Mark();
		Skip();
		StringBuilderPool.BuilderWrapper builderWrapper = StringBuilderPool.Rent();
		try
		{
			StringBuilder builder = builderWrapper.Builder;
			StringBuilderPool.BuilderWrapper builderWrapper2 = StringBuilderPool.Rent();
			try
			{
				StringBuilder builder2 = builderWrapper2.Builder;
				StringBuilderPool.BuilderWrapper builderWrapper3 = StringBuilderPool.Rent();
				try
				{
					StringBuilder builder3 = builderWrapper3.Builder;
					StringBuilderPool.BuilderWrapper builderWrapper4 = StringBuilderPool.Rent();
					try
					{
						StringBuilder builder4 = builderWrapper4.Builder;
						bool flag = false;
						while (true)
						{
							if (IsDocumentIndicator())
							{
								_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
								throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a quoted scalar, found unexpected document indicator.");
							}
							if (analyzer.IsZero())
							{
								_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
								throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a quoted scalar, found unexpected end of stream.");
							}
							if (flag && !isSingleQuoted && indent >= cursor.LineOffset)
							{
								_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
								throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a multi-line double-quoted scalar, found wrong indentation.");
							}
							flag = false;
							while (!analyzer.IsWhiteBreakOrZero())
							{
								if (isSingleQuoted && analyzer.Check('\'') && analyzer.Check('\'', 1))
								{
									builder.Append('\'');
									Skip();
									Skip();
									continue;
								}
								if (analyzer.Check(isSingleQuoted ? '\'' : '"'))
								{
									break;
								}
								if (!isSingleQuoted && analyzer.Check('\\') && analyzer.IsBreak(1))
								{
									Skip();
									SkipLine();
									flag = true;
									break;
								}
								if (!isSingleQuoted && analyzer.Check('\\'))
								{
									int num = 0;
									char c = analyzer.Peek(1);
									switch (c)
									{
									case 'x':
										num = 2;
										break;
									case 'u':
										num = 4;
										break;
									case 'U':
										num = 8;
										break;
									default:
									{
										if (SimpleEscapeCodes.TryGetValue(c, out var value))
										{
											builder.Append(value);
											break;
										}
										_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
										throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a quoted scalar, found unknown escape character.");
									}
									}
									Skip();
									Skip();
									if (num <= 0)
									{
										continue;
									}
									int num2 = 0;
									for (int i = 0; i < num; i++)
									{
										if (!analyzer.IsHex(i))
										{
											_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
											throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a quoted scalar, did not find expected hexadecimal number.");
										}
										num2 = (num2 << 4) + analyzer.AsHex(i);
									}
									if (num2 >= 55296 && num2 <= 57343)
									{
										for (int j = 0; j < num; j++)
										{
											Skip();
										}
										if (analyzer.Peek(0) != '\\' || (analyzer.Peek(1) != 'u' && analyzer.Peek(1) != 'U'))
										{
											_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
											throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a quoted scalar, found invalid Unicode surrogates.");
										}
										Skip();
										num = ((analyzer.Peek(0) != 'u') ? 8 : 4);
										Skip();
										int num3 = 0;
										for (int k = 0; k < num; k++)
										{
											if (!analyzer.IsHex(0))
											{
												_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
												throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a quoted scalar, did not find expected hexadecimal number.");
											}
											num3 = (num3 << 4) + analyzer.AsHex(k);
										}
										for (int l = 0; l < num; l++)
										{
											Skip();
										}
										num2 = char.ConvertToUtf32((char)num2, (char)num3);
									}
									else
									{
										if (num2 > 1114111)
										{
											_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
											throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a quoted scalar, found invalid Unicode character escape code.");
										}
										for (int m = 0; m < num; m++)
										{
											Skip();
										}
									}
									builder.Append(char.ConvertFromUtf32(num2));
								}
								else
								{
									builder.Append(ReadCurrentCharacter());
								}
							}
							if (analyzer.Check(isSingleQuoted ? '\'' : '"'))
							{
								break;
							}
							while (analyzer.IsWhite() || analyzer.IsBreak())
							{
								if (analyzer.IsWhite())
								{
									if (!flag)
									{
										builder2.Append(ReadCurrentCharacter());
									}
									else
									{
										Skip();
									}
								}
								else if (!flag)
								{
									builder2.Length = 0;
									builder3.Append(ReadLine());
									flag = true;
								}
								else
								{
									builder4.Append(ReadLine());
								}
							}
							if (flag)
							{
								if (StartsWith(builder3, '\n'))
								{
									if (builder4.Length == 0)
									{
										builder.Append(' ');
									}
									else
									{
										builder.Append((object?)builder4);
									}
								}
								else
								{
									builder.Append((object?)builder3);
									builder.Append((object?)builder4);
								}
								builder3.Length = 0;
								builder4.Length = 0;
							}
							else
							{
								builder.Append((object?)builder2);
								builder2.Length = 0;
							}
						}
						Skip();
						return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar(builder.ToString(), isSingleQuoted ? _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.SingleQuoted : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.DoubleQuoted, start, cursor.Mark());
					}
					finally
					{
						((IDisposable)builderWrapper4).Dispose();
					}
				}
				finally
				{
					((IDisposable)builderWrapper3).Dispose();
				}
			}
			finally
			{
				((IDisposable)builderWrapper2).Dispose();
			}
		}
		finally
		{
			((IDisposable)builderWrapper).Dispose();
		}
	}

	private void FetchPlainScalar()
	{
		SaveSimpleKey();
		simpleKeyAllowed = false;
		bool isMultiline = false;
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar item = (lastScalar = ScanPlainScalar(ref isMultiline));
		if (isMultiline && analyzer.Check(':') && flowLevel == 0 && indent < cursor.LineOffset)
		{
			tokens.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError("While scanning a multiline plain scalar, found invalid mapping.", cursor.Mark(), cursor.Mark()));
		}
		tokens.Enqueue(item);
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar ScanPlainScalar(ref bool isMultiline)
	{
		StringBuilderPool.BuilderWrapper builderWrapper = StringBuilderPool.Rent();
		try
		{
			StringBuilder builder = builderWrapper.Builder;
			StringBuilderPool.BuilderWrapper builderWrapper2 = StringBuilderPool.Rent();
			try
			{
				StringBuilder builder2 = builderWrapper2.Builder;
				StringBuilderPool.BuilderWrapper builderWrapper3 = StringBuilderPool.Rent();
				try
				{
					StringBuilder builder3 = builderWrapper3.Builder;
					StringBuilderPool.BuilderWrapper builderWrapper4 = StringBuilderPool.Rent();
					try
					{
						StringBuilder builder4 = builderWrapper4.Builder;
						bool flag = false;
						long num = indent + 1;
						_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = cursor.Mark();
						_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = start;
						_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2 = simpleKeys.Peek();
						while (!IsDocumentIndicator())
						{
							if (analyzer.Check('#'))
							{
								if (indent < 0 && flowLevel == 0)
								{
									plainScalarFollowedByComment = true;
								}
								break;
							}
							bool flag2 = analyzer.Check('*') && (!_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.IsPossible || !_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.IsRequired);
							while (!analyzer.IsWhiteBreakOrZero())
							{
								if ((analyzer.Check(':') && !flag2 && (analyzer.IsWhiteBreakOrZero(1) || (flowLevel > 0 && analyzer.Check(',', 1)))) || (flowLevel > 0 && analyzer.Check(",[]{}")))
								{
									if (flowLevel == 0 && !_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.IsPossible)
									{
										tokens.Enqueue(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EError("While scanning a plain scalar value, found invalid mapping.", cursor.Mark(), cursor.Mark()));
									}
									break;
								}
								if (flag || builder2.Length > 0)
								{
									if (flag)
									{
										if (StartsWith(builder3, '\n'))
										{
											if (builder4.Length == 0)
											{
												builder.Append(' ');
											}
											else
											{
												builder.Append((object?)builder4);
											}
										}
										else
										{
											builder.Append((object?)builder3);
											builder.Append((object?)builder4);
										}
										builder3.Length = 0;
										builder4.Length = 0;
										flag = false;
									}
									else
									{
										builder.Append((object?)builder2);
										builder2.Length = 0;
									}
								}
								if (flowLevel > 0 && cursor.LineOffset < num)
								{
									throw new InvalidOperationException();
								}
								builder.Append(ReadCurrentCharacter());
								end = cursor.Mark();
							}
							if (!analyzer.IsWhite() && !analyzer.IsBreak())
							{
								break;
							}
							while (analyzer.IsWhite() || analyzer.IsBreak())
							{
								if (analyzer.IsWhite())
								{
									if (flag && cursor.LineOffset < num && analyzer.IsTab())
									{
										_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end2 = cursor.Mark();
										throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end2, "While scanning a plain scalar, found a tab character that violate indentation.");
									}
									if (!flag)
									{
										builder2.Append(ReadCurrentCharacter());
									}
									else
									{
										Skip();
									}
								}
								else
								{
									isMultiline = true;
									if (!flag)
									{
										builder2.Length = 0;
										builder3.Append(ReadLine());
										flag = true;
									}
									else
									{
										builder4.Append(ReadLine());
									}
								}
							}
							if (flowLevel == 0 && cursor.LineOffset < num)
							{
								break;
							}
						}
						if (flag)
						{
							simpleKeyAllowed = true;
						}
						return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar(builder.ToString(), _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Plain, start, end);
					}
					finally
					{
						((IDisposable)builderWrapper4).Dispose();
					}
				}
				finally
				{
					((IDisposable)builderWrapper3).Dispose();
				}
			}
			finally
			{
				((IDisposable)builderWrapper2).Dispose();
			}
		}
		finally
		{
			((IDisposable)builderWrapper).Dispose();
		}
	}

	private void RemoveSimpleKey()
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2 = simpleKeys.Peek();
		if (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.IsPossible && _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.IsRequired)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.Mark;
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.Mark;
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a simple key, could not find expected ':'.");
		}
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey2.MarkAsImpossible();
	}

	private string ScanDirectiveName(in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start)
	{
		StringBuilderPool.BuilderWrapper builderWrapper = StringBuilderPool.Rent();
		try
		{
			StringBuilder builder = builderWrapper.Builder;
			while (analyzer.IsAlphaNumericDashOrUnderscore())
			{
				builder.Append(ReadCurrentCharacter());
			}
			if (builder.Length == 0)
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a directive, could not find expected directive name.");
			}
			if (analyzer.EndOfInput)
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a directive, found unexpected end of stream.");
			}
			if (!analyzer.IsWhiteBreakOrZero())
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a directive, found unexpected non-alphabetical character.");
			}
			return builder.ToString();
		}
		finally
		{
			((IDisposable)builderWrapper).Dispose();
		}
	}

	private void SkipWhitespaces()
	{
		while (analyzer.IsWhite())
		{
			Skip();
		}
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective ScanVersionDirectiveValue(in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start)
	{
		SkipWhitespaces();
		int major = ScanVersionDirectiveNumber(in start);
		if (!analyzer.Check('.'))
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a %YAML directive, did not find expected digit or '.' character.");
		}
		Skip();
		int minor = ScanVersionDirectiveNumber(in start);
		return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersionDirective(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EVersion(major, minor), start, start);
	}

	private _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective ScanTagDirectiveValue(in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start)
	{
		SkipWhitespaces();
		string handle = ScanTagHandle(isDirective: true, start);
		if (!analyzer.IsWhite())
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a %TAG directive, did not find expected whitespace.");
		}
		SkipWhitespaces();
		string prefix = ScanTagUri(null, start);
		if (!analyzer.IsWhiteBreakOrZero())
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a %TAG directive, did not find expected whitespace or line break.");
		}
		return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagDirective(handle, prefix, start, start);
	}

	private string ScanTagUri(string? head, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start)
	{
		StringBuilderPool.BuilderWrapper builderWrapper = StringBuilderPool.Rent();
		try
		{
			StringBuilder builder = builderWrapper.Builder;
			if (head != null && head.Length > 1)
			{
				builder.Append(head.Substring(1));
			}
			while (analyzer.IsAlphaNumericDashOrUnderscore() || analyzer.Check(";/?:@&=+$.!~*'()[]%") || (analyzer.Check(',') && !analyzer.IsBreak(1)))
			{
				if (analyzer.Check('%'))
				{
					builder.Append(ScanUriEscapes(in start));
				}
				else if (analyzer.Check('+'))
				{
					builder.Append(' ');
					Skip();
				}
				else
				{
					builder.Append(ReadCurrentCharacter());
				}
			}
			if (builder.Length == 0)
			{
				return string.Empty;
			}
			string text = builder.ToString();
			if (Polyfills.EndsWith(text, ','))
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start2 = cursor.Mark();
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start2, in end, "Unexpected comma at end of tag");
			}
			return text;
		}
		finally
		{
			((IDisposable)builderWrapper).Dispose();
		}
	}

	private string ScanUriEscapes(in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start)
	{
		byte[] array = EmptyBytes;
		int count = 0;
		int num = 0;
		do
		{
			if (!analyzer.Check('%') || !analyzer.IsHex(1) || !analyzer.IsHex(2))
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a tag, did not find URI escaped octet.");
			}
			int num2 = (analyzer.AsHex(1) << 4) + analyzer.AsHex(2);
			if (num == 0)
			{
				num = (((num2 & 0x80) == 0) ? 1 : (((num2 & 0xE0) == 192) ? 2 : (((num2 & 0xF0) == 224) ? 3 : (((num2 & 0xF8) == 240) ? 4 : 0))));
				if (num == 0)
				{
					_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
					throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a tag, found an incorrect leading UTF-8 octet.");
				}
				array = new byte[num];
			}
			else if ((num2 & 0xC0) != 128)
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a tag, found an incorrect trailing UTF-8 octet.");
			}
			array[count++] = (byte)num2;
			Skip();
			Skip();
			Skip();
		}
		while (--num > 0);
		string @string = Encoding.UTF8.GetString(array, 0, count);
		if (@string.Length == 0 || @string.Length > 2)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a tag, found an incorrect UTF-8 sequence.");
		}
		return @string;
	}

	private string ScanTagHandle(bool isDirective, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start)
	{
		if (!analyzer.Check('!'))
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a tag, did not find expected '!'.");
		}
		StringBuilderPool.BuilderWrapper builderWrapper = StringBuilderPool.Rent();
		try
		{
			StringBuilder builder = builderWrapper.Builder;
			builder.Append(ReadCurrentCharacter());
			while (analyzer.IsAlphaNumericDashOrUnderscore())
			{
				builder.Append(ReadCurrentCharacter());
			}
			if (analyzer.Check('!'))
			{
				builder.Append(ReadCurrentCharacter());
			}
			else if (isDirective && (builder.Length != 1 || builder[0] != '!'))
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a tag directive, did not find expected '!'.");
			}
			return builder.ToString();
		}
		finally
		{
			((IDisposable)builderWrapper).Dispose();
		}
	}

	private int ScanVersionDirectiveNumber(in _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark start)
	{
		int num = 0;
		int num2 = 0;
		while (analyzer.IsDigit())
		{
			if (++num2 > 9)
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
				throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a %YAML directive, found extremely long version number.");
			}
			num = num * 10 + analyzer.AsDigit();
			Skip();
		}
		if (num2 == 0)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMark end = cursor.Mark();
			throw new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESyntaxErrorException(in start, in end, "While scanning a %YAML directive, did not find expected version number.");
		}
		return num;
	}

	private void SaveSimpleKey()
	{
		bool isRequired = flowLevel == 0 && indent == cursor.LineOffset;
		if (simpleKeyAllowed)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey item = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESimpleKey(isRequired, tokensParsed + tokens.Count, cursor);
			RemoveSimpleKey();
			simpleKeys.Pop();
			simpleKeys.Push(item);
		}
	}
}
