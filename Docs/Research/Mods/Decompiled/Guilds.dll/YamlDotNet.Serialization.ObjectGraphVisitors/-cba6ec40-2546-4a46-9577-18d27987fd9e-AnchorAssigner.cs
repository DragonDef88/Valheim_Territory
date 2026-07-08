using System;
using System.Collections.Generic;
using System.Globalization;
using YamlDotNet.Core;

namespace YamlDotNet.Serialization.ObjectGraphVisitors;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAssigner : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EPreProcessingPhaseObjectGraphVisitorSkeleton, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIAliasProvider
{
	private class AnchorAssignment
	{
		public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName Anchor;
	}

	private readonly Dictionary<object, AnchorAssignment> assignments = new Dictionary<object, AnchorAssignment>();

	private uint nextId;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorAssigner(IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter> typeConverters)
		: base(typeConverters)
	{
	}

	protected override bool Enter(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		if (value.Value != null && assignments.TryGetValue(value.Value, out AnchorAssignment value2))
		{
			if (value2.Anchor.IsEmpty)
			{
				value2.Anchor = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName("o" + nextId.ToString(CultureInfo.InvariantCulture));
				nextId++;
			}
			return false;
		}
		return true;
	}

	protected override bool EnterMapping(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor key, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		return true;
	}

	protected override bool EnterMapping(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIPropertyDescriptor key, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor value, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		return true;
	}

	protected override void VisitScalar(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor scalar, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
	}

	protected override void VisitMappingStart(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor mapping, Type keyType, Type valueType, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		VisitObject(mapping);
	}

	protected override void VisitMappingEnd(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor mapping, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
	}

	protected override void VisitSequenceStart(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor sequence, Type elementType, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
		VisitObject(sequence);
	}

	protected override void VisitSequenceEnd(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor sequence, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectSerializer serializer)
	{
	}

	private void VisitObject(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectDescriptor value)
	{
		if (value.Value != null)
		{
			assignments.Add(value.Value, new AnchorAssignment());
		}
	}

	_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIAliasProvider.GetAlias(object target)
	{
		if (target != null && assignments.TryGetValue(target, out AnchorAssignment value))
		{
			return value.Anchor;
		}
		return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAnchorName.Empty;
	}
}
