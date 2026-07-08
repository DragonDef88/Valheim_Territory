using System.Collections;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace YamlDotNet.RepresentationModel;

internal class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlStream : IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument>, IEnumerable
{
	private readonly List<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument> documents = new List<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument>();

	public IList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument> Documents => documents;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlStream()
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlStream(params _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument[] documents)
		: this((IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument>)documents)
	{
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlStream(IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument> documents)
	{
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument document in documents)
		{
			this.documents.Add(document);
		}
	}

	public void Add(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument document)
	{
		documents.Add(document);
	}

	public void Load(TextReader input)
	{
		Load(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EParser(input));
	}

	public void Load(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIParser parser)
	{
		documents.Clear();
		parser.Consume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamStart>();
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd @event;
		while (!parser.TryConsume<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd>(out @event))
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument item = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument(parser);
			documents.Add(item);
		}
	}

	public void Save(TextWriter output)
	{
		Save(output, assignAnchors: true);
	}

	public void Save(TextWriter output, bool assignAnchors)
	{
		Save(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEmitter(output), assignAnchors);
	}

	public void Save(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIEmitter emitter, bool assignAnchors)
	{
		emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamStart());
		foreach (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument document in documents)
		{
			document.Save(emitter, assignAnchors);
		}
		emitter.Emit(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStreamEnd());
	}

	public void Accept(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlVisitor visitor)
	{
		visitor.Visit(this);
	}

	public IEnumerator<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlDocument> GetEnumerator()
	{
		return documents.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
