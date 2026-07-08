using System;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.RepresentationModel;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELibYamlEventStream
{
	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELibYamlEventStream(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser iParser)
	{
		parser = iParser ?? throw new ArgumentNullException("iParser");
	}

	public void WriteTo(TextWriter textWriter)
	{
		while (parser.MoveNext())
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParsingEvent current = parser.Current;
			if (!(current is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias))
			{
				if (!(current is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd))
				{
					if (!(current is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart))
					{
						if (!(current is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingEnd))
						{
							if (!(current is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingStart nodeEvent))
							{
								if (!(current is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar))
								{
									if (!(current is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceEnd))
									{
										if (!(current is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESequenceStart nodeEvent2))
										{
											if (!(current is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd))
											{
												if (current is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamStart)
												{
													textWriter.Write("+STR");
												}
											}
											else
											{
												textWriter.Write("-STR");
											}
										}
										else
										{
											textWriter.Write("+SEQ");
											WriteAnchorAndTag(textWriter, nodeEvent2);
										}
									}
									else
									{
										textWriter.Write("-SEQ");
									}
								}
								else
								{
									textWriter.Write("=VAL");
									WriteAnchorAndTag(textWriter, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar);
									switch (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.Style)
									{
									case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.DoubleQuoted:
										textWriter.Write(" \"");
										break;
									case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.SingleQuoted:
										textWriter.Write(" '");
										break;
									case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Folded:
										textWriter.Write(" >");
										break;
									case _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarStyle.Literal:
										textWriter.Write(" |");
										break;
									default:
										textWriter.Write(" :");
										break;
									}
									string value = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalar.Value;
									foreach (char c in value)
									{
										switch (c)
										{
										case '\b':
											textWriter.Write("\\b");
											break;
										case '\t':
											textWriter.Write("\\t");
											break;
										case '\n':
											textWriter.Write("\\n");
											break;
										case '\r':
											textWriter.Write("\\r");
											break;
										case '\\':
											textWriter.Write("\\\\");
											break;
										default:
											textWriter.Write(c);
											break;
										}
									}
								}
							}
							else
							{
								textWriter.Write("+MAP");
								WriteAnchorAndTag(textWriter, nodeEvent);
							}
						}
						else
						{
							textWriter.Write("-MAP");
						}
					}
					else
					{
						textWriter.Write("+DOC");
						if (!_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentStart.IsImplicit)
						{
							textWriter.Write(" ---");
						}
					}
				}
				else
				{
					textWriter.Write("-DOC");
					if (!_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDocumentEnd.IsImplicit)
					{
						textWriter.Write(" ...");
					}
				}
			}
			else
			{
				textWriter.Write("=ALI *");
				textWriter.Write(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAlias.Value);
			}
			textWriter.WriteLine();
		}
	}

	private static void WriteAnchorAndTag(TextWriter textWriter, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeEvent nodeEvent)
	{
		if (!nodeEvent.Anchor.IsEmpty)
		{
			textWriter.Write(" &");
			textWriter.Write(nodeEvent.Anchor);
		}
		if (!nodeEvent.Tag.IsEmpty)
		{
			textWriter.Write(" <");
			textWriter.Write(nodeEvent.Tag.Value);
			textWriter.Write(">");
		}
	}
}
