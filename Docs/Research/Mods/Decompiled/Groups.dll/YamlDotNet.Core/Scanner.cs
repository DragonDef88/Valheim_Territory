using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Core.Tokens;

namespace YamlDotNet.Core;

internal class Scanner : IScanner
{
	private const int MaxVersionNumberLength = 9;

	private static readonly IDictionary<char, char> SimpleEscapeCodes = new SortedDictionary<char, char>
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

	private readonly Stack<int> indents = new Stack<int>();

	private readonly InsertionQueue<Token> tokens = new InsertionQueue<Token>();

	private readonly Stack<SimpleKey> simpleKeys = new Stack<SimpleKey>();

	private readonly CharacterAnalyzer<LookAheadBuffer> analyzer;

	private readonly Cursor cursor;

	private bool streamStartProduced;

	private bool streamEndProduced;

	private bool plainScalarFollowedByComment;

	private int flowSequenceStartLine;

	private int indent = -1;

	private bool simpleKeyAllowed;

	private int flowLevel;

	private int tokensParsed;

	private bool tokenAvailable;

	private Token? previous;

	private Anchor? previousAnchor;

	private static readonly byte[] EmptyBytes = new byte[0];

	public bool SkipComments { get; private set; }

	public Token? Current { get; private set; }

	public Mark CurrentPosition => cursor.Mark();

	private bool IsDocumentStart()
	{
		if (!analyzer.EndOfInput && cursor.LineOffset == 0 && analyzer.Check('-') && analyzer.Check('-', 1) && analyzer.Check('-', 2))
		{
			return analyzer.IsWhiteBreakOrZero(3);
		}
		return false;
	}

	private bool IsDocumentEnd()
	{
		if (!analyzer.EndOfInput && cursor.LineOffset == 0 && analyzer.Check('.') && analyzer.Check('.', 1) && analyzer.Check('.', 2))
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

	public Scanner(TextReader input, bool skipComments = true)
	{
		analyzer = new CharacterAnalyzer<LookAheadBuffer>(new LookAheadBuffer(input, 1024));
		cursor = new Cursor();
		SkipComments = skipComments;
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
				foreach (SimpleKey simpleKey in simpleKeys)
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
		foreach (SimpleKey simpleKey in simpleKeys)
		{
			if (simpleKey.IsPossible && (simpleKey.Line < cursor.Line || simpleKey.Index + 1024 < cursor.Index))
			{
				if (simpleKey.IsRequired)
				{
					Mark mark = cursor.Mark();
					tokens.Enqueue(new Error("While scanning a simple key, could not find expected ':'.", mark, mark));
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
			FetchStreamEnd();
			return;
		}
		if (cursor.LineOffset == 0 && analyzer.Check('%'))
		{
			FetchDirective();
			return;
		}
		if (IsDocumentStart())
		{
			FetchDocumentIndicator(isStartToken: true);
			return;
		}
		if (IsDocumentEnd())
		{
			FetchDocumentIndicator(isStartToken: false);
			return;
		}
		if (analyzer.Check('['))
		{
			FetchFlowCollectionStart(isSequenceToken: true);
			return;
		}
		if (analyzer.Check('{'))
		{
			FetchFlowCollectionStart(isSequenceToken: false);
			return;
		}
		if (analyzer.Check(']'))
		{
			FetchFlowCollectionEnd(isSequenceToken: true);
			return;
		}
		if (analyzer.Check('}'))
		{
			FetchFlowCollectionEnd(isSequenceToken: false);
			return;
		}
		if (analyzer.Check(','))
		{
			FetchFlowEntry();
			return;
		}
		if (analyzer.Check('-') && analyzer.IsWhiteBreakOrZero(1))
		{
			FetchBlockEntry();
			return;
		}
		if (analyzer.Check('?') && (flowLevel > 0 || analyzer.IsWhiteBreakOrZero(1)))
		{
			FetchKey();
			return;
		}
		if (analyzer.Check(':') && (flowLevel > 0 || analyzer.IsWhiteBreakOrZero(1)) && (!simpleKeyAllowed || flowLevel <= 0))
		{
			FetchValue();
			return;
		}
		if (analyzer.Check('*'))
		{
			FetchAnchor(isAlias: true);
			return;
		}
		if (analyzer.Check('&'))
		{
			FetchAnchor(isAlias: false);
			return;
		}
		if (analyzer.Check('!'))
		{
			FetchTag();
			return;
		}
		if (analyzer.Check('|') && flowLevel == 0)
		{
			FetchBlockScalar(isLiteral: true);
			return;
		}
		if (analyzer.Check('>') && flowLevel == 0)
		{
			FetchBlockScalar(isLiteral: false);
			return;
		}
		if (analyzer.Check('\''))
		{
			FetchFlowScalar(isSingleQuoted: true);
			return;
		}
		if (analyzer.Check('"'))
		{
			FetchFlowScalar(isSingleQuoted: false);
			return;
		}
		if ((!analyzer.IsWhiteBreakOrZero() && !analyzer.Check("-?:,[]{}#&*!|>'\"%@`")) || (analyzer.Check('-') && !analyzer.IsWhite(1)) || (flowLevel == 0 && analyzer.Check("?:") && !analyzer.IsWhiteBreakOrZero(1)) || (simpleKeyAllowed && flowLevel > 0 && analyzer.Check("?:")))
		{
			if (plainScalarFollowedByComment)
			{
				Mark mark = cursor.Mark();
				tokens.Enqueue(new Error("While scanning plain scalar, found a comment between adjacent scalars.", mark, mark));
			}
			plainScalarFollowedByComment = false;
			FetchPlainScalar();
			return;
		}
		if (simpleKeyAllowed && indent >= cursor.LineOffset && analyzer.IsTab())
		{
			throw new SyntaxErrorException("While scanning a mapping, found invalid tab as indentation.");
		}
		if (analyzer.IsWhiteBreakOrZero())
		{
			Skip();
			return;
		}
		Mark start = cursor.Mark();
		Skip();
		Mark end = cursor.Mark();
		throw new SyntaxErrorException(start, end, "While scanning for the next token, found character that cannot start any token.");
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
		if (analyzer.Check('#'))
		{
			Mark mark = cursor.Mark();
			Skip();
			while (analyzer.IsSpace())
			{
				Skip();
			}
			StringBuilder stringBuilder = new StringBuilder();
			while (!analyzer.IsBreakOrZero())
			{
				stringBuilder.Append(ReadCurrentCharacter());
			}
			if (!SkipComments)
			{
				bool isInline = previous != null && previous.End.Line == mark.Line && previous.End.Column != 1 && !(previous is StreamStart);
				tokens.Enqueue(new Comment(stringBuilder.ToString(), isInline, mark, cursor.Mark()));
			}
		}
	}

	private void FetchStreamStart()
	{
		simpleKeys.Push(new SimpleKey());
		simpleKeyAllowed = true;
		streamStartProduced = true;
		Mark mark = cursor.Mark();
		tokens.Enqueue(new StreamStart(mark, mark));
	}

	private void UnrollIndent(int column)
	{
		if (flowLevel == 0)
		{
			while (indent > column)
			{
				Mark mark = cursor.Mark();
				tokens.Enqueue(new BlockEnd(mark, mark));
				indent = indents.Pop();
			}
		}
	}

	private void FetchStreamEnd()
	{
		cursor.ForceSkipLineAfterNonBreak();
		UnrollIndent(-1);
		RemoveSimpleKey();
		simpleKeyAllowed = false;
		streamEndProduced = true;
		Mark mark = cursor.Mark();
		tokens.Enqueue(new StreamEnd(mark, mark));
	}

	private void FetchDirective()
	{
		UnrollIndent(-1);
		RemoveSimpleKey();
		simpleKeyAllowed = false;
		Token token = ScanDirective();
		if (token != null)
		{
			tokens.Enqueue(token);
		}
	}

	private Token? ScanDirective()
	{
		Mark start = cursor.Mark();
		Skip();
		string text = ScanDirectiveName(start);
		Token result;
		if (!(text == "YAML"))
		{
			if (!(text == "TAG"))
			{
				while (!analyzer.Check('#') && !analyzer.IsBreak())
				{
					Skip();
				}
				return null;
			}
			result = ScanTagDirectiveValue(start);
		}
		else
		{
			if (!(previous is DocumentStart) && !(previous is StreamStart) && !(previous is DocumentEnd))
			{
				throw new SemanticErrorException(start, cursor.Mark(), "While scanning a version directive, did not find preceding <document end>.");
			}
			result = ScanVersionDirectiveValue(start);
		}
		while (analyzer.IsWhite())
		{
			Skip();
		}
		ProcessComment();
		if (!analyzer.IsBreakOrZero())
		{
			throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a directive, did not find expected comment or line break.");
		}
		if (analyzer.IsBreak())
		{
			SkipLine();
		}
		return result;
	}

	private void FetchDocumentIndicator(bool isStartToken)
	{
		UnrollIndent(-1);
		RemoveSimpleKey();
		simpleKeyAllowed = false;
		Mark mark = cursor.Mark();
		Skip();
		Skip();
		Skip();
		if (isStartToken)
		{
			tokens.Enqueue(new DocumentStart(mark, cursor.Mark()));
			return;
		}
		Token token = null;
		while (!analyzer.EndOfInput && !analyzer.IsBreak() && !analyzer.Check('#'))
		{
			if (!analyzer.IsWhite())
			{
				token = new Error("While scanning a document end, found invalid content after '...' marker.", mark, cursor.Mark());
				break;
			}
			Skip();
		}
		tokens.Enqueue(new DocumentEnd(mark, mark));
		if (token != null)
		{
			tokens.Enqueue(token);
		}
	}

	private void FetchFlowCollectionStart(bool isSequenceToken)
	{
		SaveSimpleKey();
		IncreaseFlowLevel();
		simpleKeyAllowed = true;
		Mark mark = cursor.Mark();
		Skip();
		Token token;
		if (isSequenceToken)
		{
			token = new FlowSequenceStart(mark, mark);
			flowSequenceStartLine = token.Start.Line;
		}
		else
		{
			token = new FlowMappingStart(mark, mark);
		}
		tokens.Enqueue(token);
	}

	private void IncreaseFlowLevel()
	{
		simpleKeys.Push(new SimpleKey());
		flowLevel++;
	}

	private void FetchFlowCollectionEnd(bool isSequenceToken)
	{
		RemoveSimpleKey();
		DecreaseFlowLevel();
		simpleKeyAllowed = false;
		Mark mark = cursor.Mark();
		Skip();
		Token token = null;
		Token item;
		if (isSequenceToken)
		{
			if (analyzer.Check('#'))
			{
				token = new Error("While scanning a flow sequence end, found invalid comment after ']'.", mark, mark);
			}
			if (previous is StreamStart && flowSequenceStartLine != mark.Line)
			{
				tokens.Enqueue(new Error("While scanning a flow sequence end, found mapping key spanning across multiple lines.", mark, mark));
			}
			item = new FlowSequenceEnd(mark, mark);
		}
		else
		{
			item = new FlowMappingEnd(mark, mark);
		}
		tokens.Enqueue(item);
		if (token != null)
		{
			tokens.Enqueue(token);
		}
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
		Mark start = cursor.Mark();
		Skip();
		Mark end = cursor.Mark();
		if (analyzer.Check('#'))
		{
			tokens.Enqueue(new Error("While scanning a flow entry, found invalid comment after comma.", start, end));
		}
		else
		{
			tokens.Enqueue(new FlowEntry(start, end));
		}
	}

	private void FetchBlockEntry()
	{
		if (flowLevel == 0)
		{
			if (!simpleKeyAllowed)
			{
				if (previousAnchor != null && previousAnchor.End.Line == cursor.Line)
				{
					throw new SemanticErrorException(previousAnchor.Start, previousAnchor.End, "Anchor before sequence entry on same line is not allowed.");
				}
				Mark mark = cursor.Mark();
				tokens.Enqueue(new Error("Block sequence entries are not allowed in this context.", mark, mark));
			}
			RollIndent(cursor.LineOffset, -1, isSequence: true, cursor.Mark());
		}
		RemoveSimpleKey();
		simpleKeyAllowed = true;
		Mark start = cursor.Mark();
		Skip();
		tokens.Enqueue(new BlockEntry(start, cursor.Mark()));
	}

	private void FetchKey()
	{
		if (flowLevel == 0)
		{
			if (!simpleKeyAllowed)
			{
				Mark mark = cursor.Mark();
				throw new SyntaxErrorException(mark, mark, "Mapping keys are not allowed in this context.");
			}
			RollIndent(cursor.LineOffset, -1, isSequence: false, cursor.Mark());
		}
		RemoveSimpleKey();
		simpleKeyAllowed = flowLevel == 0;
		Mark start = cursor.Mark();
		Skip();
		tokens.Enqueue(new Key(start, cursor.Mark()));
	}

	private void FetchValue()
	{
		SimpleKey simpleKey = simpleKeys.Peek();
		if (simpleKey.IsPossible)
		{
			tokens.Insert(simpleKey.TokenNumber - tokensParsed, new Key(simpleKey.Mark, simpleKey.Mark));
			RollIndent(simpleKey.LineOffset, simpleKey.TokenNumber, isSequence: false, simpleKey.Mark);
			simpleKey.MarkAsImpossible();
			simpleKeyAllowed = false;
		}
		else
		{
			bool flag = flowLevel == 0;
			if (flag)
			{
				if (!simpleKeyAllowed)
				{
					Mark mark = cursor.Mark();
					tokens.Enqueue(new Error("Mapping values are not allowed in this context.", mark, mark));
					return;
				}
				RollIndent(cursor.LineOffset, -1, isSequence: false, cursor.Mark());
				if (cursor.LineOffset == 0 && simpleKey.LineOffset == 0)
				{
					tokens.Insert(tokens.Count, new Key(simpleKey.Mark, simpleKey.Mark));
					flag = false;
				}
			}
			simpleKeyAllowed = flag;
		}
		Mark start = cursor.Mark();
		Skip();
		tokens.Enqueue(new Value(start, cursor.Mark()));
	}

	private void RollIndent(int column, int number, bool isSequence, Mark position)
	{
		if (flowLevel <= 0 && indent < column)
		{
			indents.Push(indent);
			indent = column;
			Token item = ((!isSequence) ? ((Token)new BlockMappingStart(position, position)) : ((Token)new BlockSequenceStart(position, position)));
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

	private Token ScanAnchor(bool isAlias)
	{
		Mark start = cursor.Mark();
		Skip();
		bool flag = false;
		if (isAlias)
		{
			SimpleKey simpleKey = simpleKeys.Peek();
			flag = simpleKey.IsRequired && simpleKey.IsPossible;
		}
		StringBuilder stringBuilder = new StringBuilder();
		while (!analyzer.IsWhiteBreakOrZero() && !analyzer.Check("[]{},") && (!flag || !analyzer.Check(':') || !analyzer.IsWhiteBreakOrZero(1)))
		{
			stringBuilder.Append(ReadCurrentCharacter());
		}
		if (stringBuilder.Length == 0 || (!analyzer.IsWhiteBreakOrZero() && !analyzer.Check("?:,]}%@`")))
		{
			throw new SyntaxErrorException(start, cursor.Mark(), "While scanning an anchor or alias, found value containing disallowed: []{},");
		}
		AnchorName value = new AnchorName(stringBuilder.ToString());
		if (isAlias)
		{
			return new AnchorAlias(value, start, cursor.Mark());
		}
		return previousAnchor = new Anchor(value, start, cursor.Mark());
	}

	private void FetchTag()
	{
		SaveSimpleKey();
		simpleKeyAllowed = false;
		tokens.Enqueue(ScanTag());
	}

	private Token ScanTag()
	{
		Mark start = cursor.Mark();
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
				throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a tag, did not find the expected '>'.");
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
			throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a tag, did not find expected whitespace, comma or line break.");
		}
		return new Tag(text, text2, start, cursor.Mark());
	}

	private void FetchBlockScalar(bool isLiteral)
	{
		RemoveSimpleKey();
		simpleKeyAllowed = true;
		tokens.Enqueue(ScanBlockScalar(isLiteral));
	}

	private Token ScanBlockScalar(bool isLiteral)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		StringBuilder stringBuilder3 = new StringBuilder();
		int num = 0;
		int num2 = 0;
		int currentIndent = 0;
		bool flag = false;
		bool? isFirstLine = null;
		Mark start = cursor.Mark();
		Skip();
		if (analyzer.Check("+-"))
		{
			num = (analyzer.Check('+') ? 1 : (-1));
			Skip();
			if (analyzer.IsDigit())
			{
				if (analyzer.Check('0'))
				{
					throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a block scalar, found an indentation indicator equal to 0.");
				}
				num2 = analyzer.AsDigit();
				Skip();
			}
		}
		else if (analyzer.IsDigit())
		{
			if (analyzer.Check('0'))
			{
				throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a block scalar, found an indentation indicator equal to 0.");
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
			throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a block scalar, found a comment without whtespace after '>' indicator.");
		}
		while (analyzer.IsWhite())
		{
			Skip();
		}
		ProcessComment();
		if (!analyzer.IsBreakOrZero())
		{
			throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a block scalar, did not find expected comment or line break.");
		}
		if (analyzer.IsBreak())
		{
			SkipLine();
			if (!isFirstLine.HasValue)
			{
				isFirstLine = true;
			}
			else if (isFirstLine == true)
			{
				isFirstLine = false;
			}
		}
		Mark end = cursor.Mark();
		if (num2 != 0)
		{
			currentIndent = ((indent >= 0) ? (indent + num2) : num2);
		}
		currentIndent = ScanBlockScalarBreaks(currentIndent, stringBuilder3, isLiteral, ref end, ref isFirstLine);
		isFirstLine = false;
		while (cursor.LineOffset == currentIndent && !analyzer.IsZero() && !IsDocumentEnd())
		{
			bool flag2 = analyzer.IsWhite();
			if (!isLiteral && StartsWith(stringBuilder2, '\n') && !flag && !flag2)
			{
				if (stringBuilder3.Length == 0)
				{
					stringBuilder.Append(' ');
				}
				stringBuilder2.Length = 0;
			}
			else
			{
				stringBuilder.Append(stringBuilder2.ToString());
				stringBuilder2.Length = 0;
			}
			stringBuilder.Append(stringBuilder3.ToString());
			stringBuilder3.Length = 0;
			flag = analyzer.IsWhite();
			while (!analyzer.IsBreakOrZero())
			{
				stringBuilder.Append(ReadCurrentCharacter());
			}
			char c = ReadLine();
			if (c != 0)
			{
				stringBuilder2.Append(c);
			}
			currentIndent = ScanBlockScalarBreaks(currentIndent, stringBuilder3, isLiteral, ref end, ref isFirstLine);
		}
		if (num != -1)
		{
			stringBuilder.Append((object?)stringBuilder2);
		}
		if (num == 1)
		{
			stringBuilder.Append((object?)stringBuilder3);
		}
		ScalarStyle style = (isLiteral ? ScalarStyle.Literal : ScalarStyle.Folded);
		return new Scalar(stringBuilder.ToString(), style, start, end);
	}

	private int ScanBlockScalarBreaks(int currentIndent, StringBuilder breaks, bool isLiteral, ref Mark end, ref bool? isFirstLine)
	{
		int num = 0;
		int num2 = -1;
		end = cursor.Mark();
		while (true)
		{
			if ((currentIndent == 0 || cursor.LineOffset < currentIndent) && analyzer.IsSpace())
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
			if (isFirstLine == true)
			{
				isFirstLine = false;
				num2 = cursor.LineOffset;
			}
			breaks.Append(ReadLine());
			end = cursor.Mark();
		}
		if (isLiteral && isFirstLine == true)
		{
			int num3 = cursor.LineOffset;
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
			throw new SemanticErrorException(end, cursor.Mark(), "While scanning a literal block scalar, found extra spaces in first line.");
		}
		if (!isLiteral && num > cursor.LineOffset && num2 > -1)
		{
			throw new SemanticErrorException(end, cursor.Mark(), "While scanning a literal block scalar, found more spaces in lines above first content line.");
		}
		if (currentIndent == 0 && (cursor.LineOffset > 0 || indent > -1))
		{
			currentIndent = Math.Max(num, Math.Max(indent + 1, 1));
		}
		return currentIndent;
	}

	private void FetchFlowScalar(bool isSingleQuoted)
	{
		SaveSimpleKey();
		simpleKeyAllowed = false;
		tokens.Enqueue(ScanFlowScalar(isSingleQuoted));
		if (!isSingleQuoted && analyzer.Check('#'))
		{
			Mark mark = cursor.Mark();
			tokens.Enqueue(new Error("While scanning a flow sequence end, found invalid comment after double-quoted scalar.", mark, mark));
		}
	}

	private Token ScanFlowScalar(bool isSingleQuoted)
	{
		Mark start = cursor.Mark();
		Skip();
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		StringBuilder stringBuilder3 = new StringBuilder();
		StringBuilder stringBuilder4 = new StringBuilder();
		bool flag = false;
		while (true)
		{
			if (IsDocumentIndicator())
			{
				throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a quoted scalar, found unexpected document indicator.");
			}
			if (analyzer.IsZero())
			{
				throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a quoted scalar, found unexpected end of stream.");
			}
			if (flag && !isSingleQuoted && indent >= cursor.LineOffset)
			{
				throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a multi-line double-quoted scalar, found wrong indentation.");
			}
			flag = false;
			while (!analyzer.IsWhiteBreakOrZero())
			{
				if (isSingleQuoted && analyzer.Check('\'') && analyzer.Check('\'', 1))
				{
					stringBuilder.Append('\'');
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
							stringBuilder.Append(value);
							break;
						}
						throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a quoted scalar, found unknown escape character.");
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
							throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a quoted scalar, did not find expected hexadecimal number.");
						}
						num2 = (num2 << 4) + analyzer.AsHex(i);
					}
					if ((num2 >= 55296 && num2 <= 57343) || num2 > 1114111)
					{
						throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a quoted scalar, found invalid Unicode character escape code.");
					}
					stringBuilder.Append(char.ConvertFromUtf32(num2));
					for (int j = 0; j < num; j++)
					{
						Skip();
					}
				}
				else
				{
					stringBuilder.Append(ReadCurrentCharacter());
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
						stringBuilder2.Append(ReadCurrentCharacter());
					}
					else
					{
						Skip();
					}
				}
				else if (!flag)
				{
					stringBuilder2.Length = 0;
					stringBuilder3.Append(ReadLine());
					flag = true;
				}
				else
				{
					stringBuilder4.Append(ReadLine());
				}
			}
			if (flag)
			{
				if (StartsWith(stringBuilder3, '\n'))
				{
					if (stringBuilder4.Length == 0)
					{
						stringBuilder.Append(' ');
					}
					else
					{
						stringBuilder.Append(stringBuilder4.ToString());
					}
				}
				else
				{
					stringBuilder.Append(stringBuilder3.ToString());
					stringBuilder.Append(stringBuilder4.ToString());
				}
				stringBuilder3.Length = 0;
				stringBuilder4.Length = 0;
			}
			else
			{
				stringBuilder.Append(stringBuilder2.ToString());
				stringBuilder2.Length = 0;
			}
		}
		Skip();
		return new Scalar(stringBuilder.ToString(), isSingleQuoted ? ScalarStyle.SingleQuoted : ScalarStyle.DoubleQuoted, start, cursor.Mark());
	}

	private void FetchPlainScalar()
	{
		SaveSimpleKey();
		simpleKeyAllowed = false;
		bool isMultiline = false;
		Scalar item = ScanPlainScalar(ref isMultiline);
		if (isMultiline && analyzer.Check(':') && flowLevel == 0 && indent < cursor.LineOffset)
		{
			tokens.Enqueue(new Error("While scanning a multiline plain scalar, found invalid mapping.", cursor.Mark(), cursor.Mark()));
		}
		tokens.Enqueue(item);
	}

	private Scalar ScanPlainScalar(ref bool isMultiline)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		StringBuilder stringBuilder3 = new StringBuilder();
		StringBuilder stringBuilder4 = new StringBuilder();
		bool flag = false;
		int num = indent + 1;
		Mark mark = cursor.Mark();
		Mark end = mark;
		SimpleKey simpleKey = simpleKeys.Peek();
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
			bool flag2 = analyzer.Check('*') && (!simpleKey.IsPossible || !simpleKey.IsRequired);
			while (!analyzer.IsWhiteBreakOrZero())
			{
				if ((analyzer.Check(':') && !flag2 && (analyzer.IsWhiteBreakOrZero(1) || (flowLevel > 0 && analyzer.Check(',', 1)))) || (flowLevel > 0 && analyzer.Check(",?[]{}")))
				{
					if (flowLevel == 0 && !simpleKey.IsPossible)
					{
						tokens.Enqueue(new Error("While scanning a plain scalar value, found invalid mapping.", cursor.Mark(), cursor.Mark()));
					}
					break;
				}
				if (flag || stringBuilder2.Length > 0)
				{
					if (flag)
					{
						if (StartsWith(stringBuilder3, '\n'))
						{
							if (stringBuilder4.Length == 0)
							{
								stringBuilder.Append(' ');
							}
							else
							{
								stringBuilder.Append((object?)stringBuilder4);
							}
						}
						else
						{
							stringBuilder.Append((object?)stringBuilder3);
							stringBuilder.Append((object?)stringBuilder4);
						}
						stringBuilder3.Length = 0;
						stringBuilder4.Length = 0;
						flag = false;
					}
					else
					{
						stringBuilder.Append((object?)stringBuilder2);
						stringBuilder2.Length = 0;
					}
				}
				if (flowLevel > 0 && cursor.LineOffset < num)
				{
					throw new Exception();
				}
				stringBuilder.Append(ReadCurrentCharacter());
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
						throw new SyntaxErrorException(mark, cursor.Mark(), "While scanning a plain scalar, found a tab character that violate indentation.");
					}
					if (!flag)
					{
						stringBuilder2.Append(ReadCurrentCharacter());
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
						stringBuilder2.Length = 0;
						stringBuilder3.Append(ReadLine());
						flag = true;
					}
					else
					{
						stringBuilder4.Append(ReadLine());
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
		return new Scalar(stringBuilder.ToString(), ScalarStyle.Plain, mark, end);
	}

	private void RemoveSimpleKey()
	{
		SimpleKey simpleKey = simpleKeys.Peek();
		if (simpleKey.IsPossible && simpleKey.IsRequired)
		{
			throw new SyntaxErrorException(simpleKey.Mark, simpleKey.Mark, "While scanning a simple key, could not find expected ':'.");
		}
		simpleKey.MarkAsImpossible();
	}

	private string ScanDirectiveName(Mark start)
	{
		StringBuilder stringBuilder = new StringBuilder();
		while (analyzer.IsAlphaNumericDashOrUnderscore())
		{
			stringBuilder.Append(ReadCurrentCharacter());
		}
		if (stringBuilder.Length == 0)
		{
			throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a directive, could not find expected directive name.");
		}
		if (!analyzer.IsWhiteBreakOrZero())
		{
			throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a directive, found unexpected non-alphabetical character.");
		}
		return stringBuilder.ToString();
	}

	private void SkipWhitespaces()
	{
		while (analyzer.IsWhite())
		{
			Skip();
		}
	}

	private Token ScanVersionDirectiveValue(Mark start)
	{
		SkipWhitespaces();
		int major = ScanVersionDirectiveNumber(start);
		if (!analyzer.Check('.'))
		{
			throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a %YAML directive, did not find expected digit or '.' character.");
		}
		Skip();
		int minor = ScanVersionDirectiveNumber(start);
		return new VersionDirective(new Version(major, minor), start, start);
	}

	private Token ScanTagDirectiveValue(Mark start)
	{
		SkipWhitespaces();
		string handle = ScanTagHandle(isDirective: true, start);
		if (!analyzer.IsWhite())
		{
			throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a %TAG directive, did not find expected whitespace.");
		}
		SkipWhitespaces();
		string prefix = ScanTagUri(null, start);
		if (!analyzer.IsWhiteBreakOrZero())
		{
			throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a %TAG directive, did not find expected whitespace or line break.");
		}
		return new TagDirective(handle, prefix, start, start);
	}

	private string ScanTagUri(string? head, Mark start)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (head != null && head.Length > 1)
		{
			stringBuilder.Append(head.Substring(1));
		}
		while (analyzer.IsAlphaNumericDashOrUnderscore() || analyzer.Check(";/?:@&=+$.!~*'()[]%") || (analyzer.Check(',') && !analyzer.IsBreak(1)))
		{
			if (analyzer.Check('%'))
			{
				stringBuilder.Append(ScanUriEscapes(start));
			}
			else if (analyzer.Check('+'))
			{
				stringBuilder.Append(' ');
				Skip();
			}
			else
			{
				stringBuilder.Append(ReadCurrentCharacter());
			}
		}
		if (stringBuilder.Length == 0)
		{
			return string.Empty;
		}
		return stringBuilder.ToString();
	}

	private string ScanUriEscapes(Mark start)
	{
		byte[] array = EmptyBytes;
		int count = 0;
		int num = 0;
		do
		{
			if (!analyzer.Check('%') || !analyzer.IsHex(1) || !analyzer.IsHex(2))
			{
				throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a tag, did not find URI escaped octet.");
			}
			int num2 = (analyzer.AsHex(1) << 4) + analyzer.AsHex(2);
			if (num == 0)
			{
				num = (((num2 & 0x80) == 0) ? 1 : (((num2 & 0xE0) == 192) ? 2 : (((num2 & 0xF0) == 224) ? 3 : (((num2 & 0xF8) == 240) ? 4 : 0))));
				if (num == 0)
				{
					throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a tag, found an incorrect leading UTF-8 octet.");
				}
				array = new byte[num];
			}
			else if ((num2 & 0xC0) != 128)
			{
				throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a tag, found an incorrect trailing UTF-8 octet.");
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
			throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a tag, found an incorrect UTF-8 sequence.");
		}
		return @string;
	}

	private string ScanTagHandle(bool isDirective, Mark start)
	{
		if (!analyzer.Check('!'))
		{
			throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a tag, did not find expected '!'.");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(ReadCurrentCharacter());
		while (analyzer.IsAlphaNumericDashOrUnderscore())
		{
			stringBuilder.Append(ReadCurrentCharacter());
		}
		if (analyzer.Check('!'))
		{
			stringBuilder.Append(ReadCurrentCharacter());
		}
		else if (isDirective && (stringBuilder.Length != 1 || stringBuilder[0] != '!'))
		{
			throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a tag directive, did not find expected '!'.");
		}
		return stringBuilder.ToString();
	}

	private int ScanVersionDirectiveNumber(Mark start)
	{
		int num = 0;
		int num2 = 0;
		while (analyzer.IsDigit())
		{
			if (++num2 > 9)
			{
				throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a %YAML directive, found extremely long version number.");
			}
			num = num * 10 + analyzer.AsDigit();
			Skip();
		}
		if (num2 == 0)
		{
			throw new SyntaxErrorException(start, cursor.Mark(), "While scanning a %YAML directive, did not find expected version number.");
		}
		return num;
	}

	private void SaveSimpleKey()
	{
		bool isRequired = flowLevel == 0 && indent == cursor.LineOffset;
		if (simpleKeyAllowed)
		{
			SimpleKey item = new SimpleKey(isRequired, tokensParsed + tokens.Count, cursor);
			RemoveSimpleKey();
			simpleKeys.Pop();
			simpleKeys.Push(item);
		}
	}
}
