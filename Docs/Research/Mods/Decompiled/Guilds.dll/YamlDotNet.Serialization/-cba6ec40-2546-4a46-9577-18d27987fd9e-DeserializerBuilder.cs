using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Helpers;
using YamlDotNet.Serialization.BufferedDeserialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.NodeTypeResolvers;
using YamlDotNet.Serialization.ObjectFactories;
using YamlDotNet.Serialization.Schemas;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.TypeResolvers;
using YamlDotNet.Serialization.Utilities;
using YamlDotNet.Serialization.ValueDeserializers;

namespace YamlDotNet.Serialization;

internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBuilderSkeleton<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder>
{
	private Lazy<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory> objectFactory;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELazyComponentRegistrationList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer> nodeDeserializerFactories;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELazyComponentRegistrationList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver> nodeTypeResolverFactories;

	private readonly Dictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName, Type> tagMappings;

	private readonly Dictionary<Type, Type> typeMappings;

	private readonly ITypeConverter typeConverter;

	private bool ignoreUnmatched;

	private bool duplicateKeyChecking;

	private bool attemptUnknownTypeDeserialization;

	private bool enforceNullability;

	private bool caseInsensitivePropertyMatching;

	private bool enforceRequiredProperties;

	protected override _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder Self => this;

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder()
		: base((_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeResolver)new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EStaticTypeResolver())
	{
		typeMappings = new Dictionary<Type, Type>();
		objectFactory = new Lazy<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory>(() => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDefaultObjectFactory(typeMappings, settings), isThreadSafe: true);
		tagMappings = new Dictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName, Type>
		{
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFailsafeSchema.Tags.Map,
				typeof(Dictionary<object, object>)
			},
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EFailsafeSchema.Tags.Str,
				typeof(string)
			},
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EJsonSchema.Tags.Bool,
				typeof(bool)
			},
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EJsonSchema.Tags.Float,
				typeof(double)
			},
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EJsonSchema.Tags.Int,
				typeof(int)
			},
			{
				_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDefaultSchema.Tags.Timestamp,
				typeof(DateTime)
			}
		};
		typeInspectorFactories.Add(typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECachedTypeInspector), (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector inner) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECachedTypeInspector(inner));
		typeInspectorFactories.Add(typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENamingConventionTypeInspector), (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector inner) => (!(namingConvention is _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENullNamingConvention)) ? new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENamingConventionTypeInspector(inner, namingConvention) : inner);
		typeInspectorFactories.Add(typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributesTypeInspector), (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector inner) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributesTypeInspector(inner));
		typeInspectorFactories.Add(typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributeOverridesInspector), (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector inner) => (overrides == null) ? inner : new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributeOverridesInspector(inner, overrides.Clone()));
		typeInspectorFactories.Add(typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReadableAndWritablePropertiesTypeInspector), (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector inner) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReadableAndWritablePropertiesTypeInspector(inner));
		nodeDeserializerFactories = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELazyComponentRegistrationList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer>
		{
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlConvertibleNodeDeserializer),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlConvertibleNodeDeserializer(objectFactory.Value)
			},
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSerializableNodeDeserializer),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSerializableNodeDeserializer(objectFactory.Value)
			},
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETypeConverterNodeDeserializer),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETypeConverterNodeDeserializer(BuildTypeConverters())
			},
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENullNodeDeserializer),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENullNodeDeserializer()
			},
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarNodeDeserializer),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EScalarNodeDeserializer(attemptUnknownTypeDeserialization, typeConverter, BuildTypeInspector(), yamlFormatter, enumNamingConvention)
			},
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EArrayNodeDeserializer),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EArrayNodeDeserializer(enumNamingConvention, BuildTypeInspector())
			},
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDictionaryNodeDeserializer),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDictionaryNodeDeserializer(objectFactory.Value, duplicateKeyChecking)
			},
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECollectionNodeDeserializer),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECollectionNodeDeserializer(objectFactory.Value, enumNamingConvention, BuildTypeInspector())
			},
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEnumerableNodeDeserializer),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EEnumerableNodeDeserializer()
			},
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectNodeDeserializer),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectNodeDeserializer(objectFactory.Value, BuildTypeInspector(), ignoreUnmatched, duplicateKeyChecking, typeConverter, enumNamingConvention, enforceNullability, caseInsensitivePropertyMatching, enforceRequiredProperties, BuildTypeConverters())
			},
			{
				typeof(FsharpListNodeDeserializer),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new FsharpListNodeDeserializer(BuildTypeInspector(), enumNamingConvention)
			}
		};
		nodeTypeResolverFactories = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELazyComponentRegistrationList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver>
		{
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingNodeTypeResolver),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EMappingNodeTypeResolver(typeMappings)
			},
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlConvertibleTypeResolver),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlConvertibleTypeResolver()
			},
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSerializableTypeResolver),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSerializableTypeResolver()
			},
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagNodeTypeResolver),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagNodeTypeResolver(tagMappings)
			},
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EPreventUnknownTagsNodeTypeResolver),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EPreventUnknownTagsNodeTypeResolver()
			},
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDefaultContainersNodeTypeResolver),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDefaultContainersNodeTypeResolver()
			}
		};
		typeConverter = new ReflectionTypeConverter();
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector BuildTypeInspector()
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector2 = new WritablePropertiesTypeInspector(typeResolver, includeNonPublicProperties);
		if (!ignoreFields)
		{
			_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector2 = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ECompositeTypeInspector(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EReadableFieldsTypeInspector(typeResolver), _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector2);
		}
		return typeInspectorFactories.BuildComponentChain(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector2);
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithAttemptingUnquotedStringTypeDeserialization()
	{
		attemptUnknownTypeDeserialization = true;
		return this;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithObjectFactory(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory objectFactory)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory objectFactory2 = objectFactory;
		if (objectFactory2 == null)
		{
			throw new ArgumentNullException("objectFactory");
		}
		this.objectFactory = new Lazy<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIObjectFactory>(() => objectFactory2, isThreadSafe: true);
		return this;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithObjectFactory(Func<Type, object> objectFactory)
	{
		if (objectFactory == null)
		{
			throw new ArgumentNullException("objectFactory");
		}
		return WithObjectFactory(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELambdaObjectFactory(objectFactory));
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithNodeDeserializer(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer nodeDeserializer)
	{
		return WithNodeDeserializer(nodeDeserializer, delegate(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer> w)
		{
			w.OnTop();
		});
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithNodeDeserializer(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer nodeDeserializer, Action<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer>> where)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer nodeDeserializer2 = nodeDeserializer;
		if (nodeDeserializer2 == null)
		{
			throw new ArgumentNullException("nodeDeserializer");
		}
		if (where == null)
		{
			throw new ArgumentNullException("where");
		}
		where(nodeDeserializerFactories.CreateRegistrationLocationSelector(nodeDeserializer2.GetType(), (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => nodeDeserializer2));
		return this;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithNodeDeserializer<TNodeDeserializer>(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EWrapperFactory<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer, TNodeDeserializer> nodeDeserializerFactory, Action<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITrackingRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer>> where) where TNodeDeserializer : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EWrapperFactory<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer, TNodeDeserializer> nodeDeserializerFactory2 = nodeDeserializerFactory;
		if (nodeDeserializerFactory2 == null)
		{
			throw new ArgumentNullException("nodeDeserializerFactory");
		}
		if (where == null)
		{
			throw new ArgumentNullException("where");
		}
		where(nodeDeserializerFactories.CreateTrackingRegistrationLocationSelector(typeof(TNodeDeserializer), (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer wrapped, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => nodeDeserializerFactory2(wrapped)));
		return this;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithoutNodeDeserializer<TNodeDeserializer>() where TNodeDeserializer : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer
	{
		return WithoutNodeDeserializer(typeof(TNodeDeserializer));
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithoutNodeDeserializer(Type nodeDeserializerType)
	{
		if (nodeDeserializerType == null)
		{
			throw new ArgumentNullException("nodeDeserializerType");
		}
		nodeDeserializerFactories.Remove(nodeDeserializerType);
		return this;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithTypeDiscriminatingNodeDeserializer(Action<ITypeDiscriminatingNodeDeserializerOptions> configureTypeDiscriminatingNodeDeserializerOptions, int maxDepth = -1, int maxLength = -1)
	{
		TypeDiscriminatingNodeDeserializerOptions typeDiscriminatingNodeDeserializerOptions = new TypeDiscriminatingNodeDeserializerOptions();
		configureTypeDiscriminatingNodeDeserializerOptions(typeDiscriminatingNodeDeserializerOptions);
		TypeDiscriminatingNodeDeserializer nodeDeserializer = new TypeDiscriminatingNodeDeserializer(nodeDeserializerFactories.BuildComponentList(), typeDiscriminatingNodeDeserializerOptions.discriminators, maxDepth, maxLength);
		return WithNodeDeserializer(nodeDeserializer, delegate(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer> s)
		{
			s.Before<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDictionaryNodeDeserializer>();
		});
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithNodeTypeResolver(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver nodeTypeResolver)
	{
		return WithNodeTypeResolver(nodeTypeResolver, delegate(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver> w)
		{
			w.OnTop();
		});
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithNodeTypeResolver(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver nodeTypeResolver, Action<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver>> where)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver nodeTypeResolver2 = nodeTypeResolver;
		if (nodeTypeResolver2 == null)
		{
			throw new ArgumentNullException("nodeTypeResolver");
		}
		if (where == null)
		{
			throw new ArgumentNullException("where");
		}
		where(nodeTypeResolverFactories.CreateRegistrationLocationSelector(nodeTypeResolver2.GetType(), (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => nodeTypeResolver2));
		return this;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithNodeTypeResolver<TNodeTypeResolver>(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EWrapperFactory<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver, TNodeTypeResolver> nodeTypeResolverFactory, Action<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITrackingRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver>> where) where TNodeTypeResolver : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EWrapperFactory<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver, TNodeTypeResolver> nodeTypeResolverFactory2 = nodeTypeResolverFactory;
		if (nodeTypeResolverFactory2 == null)
		{
			throw new ArgumentNullException("nodeTypeResolverFactory");
		}
		if (where == null)
		{
			throw new ArgumentNullException("where");
		}
		where(nodeTypeResolverFactories.CreateTrackingRegistrationLocationSelector(typeof(TNodeTypeResolver), (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver wrapped, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => nodeTypeResolverFactory2(wrapped)));
		return this;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithCaseInsensitivePropertyMatching()
	{
		caseInsensitivePropertyMatching = true;
		return this;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithEnforceNullability()
	{
		enforceNullability = true;
		return this;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithEnforceRequiredMembers()
	{
		enforceRequiredProperties = true;
		return this;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithoutNodeTypeResolver<TNodeTypeResolver>() where TNodeTypeResolver : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver
	{
		return WithoutNodeTypeResolver(typeof(TNodeTypeResolver));
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithoutNodeTypeResolver(Type nodeTypeResolverType)
	{
		if (nodeTypeResolverType == null)
		{
			throw new ArgumentNullException("nodeTypeResolverType");
		}
		nodeTypeResolverFactories.Remove(nodeTypeResolverType);
		return this;
	}

	public override _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithTagMapping(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName tag, Type type)
	{
		if (tag.IsEmpty)
		{
			throw new ArgumentException("Non-specific tags cannot be maped");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (tagMappings.TryGetValue(tag, out Type value))
		{
			throw new ArgumentException($"Type already has a registered type '{value.FullName}' for tag '{tag}'", "tag");
		}
		tagMappings.Add(tag, type);
		return this;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithTypeMapping<TInterface, TConcrete>() where TConcrete : TInterface
	{
		Type typeFromHandle = typeof(TInterface);
		Type typeFromHandle2 = typeof(TConcrete);
		if (!typeFromHandle.IsAssignableFrom(typeFromHandle2))
		{
			throw new InvalidOperationException("The type '" + typeFromHandle2.Name + "' does not implement interface '" + typeFromHandle.Name + "'.");
		}
		if (!DictionaryExtensions.TryAdd(typeMappings, typeFromHandle, typeFromHandle2))
		{
			typeMappings[typeFromHandle] = typeFromHandle2;
		}
		return this;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithoutTagMapping(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName tag)
	{
		if (tag.IsEmpty)
		{
			throw new ArgumentException("Non-specific tags cannot be maped");
		}
		if (!tagMappings.Remove(tag))
		{
			throw new KeyNotFoundException($"Tag '{tag}' is not registered");
		}
		return this;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder IgnoreUnmatchedProperties()
	{
		ignoreUnmatched = true;
		return this;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializerBuilder WithDuplicateKeyChecking()
	{
		duplicateKeyChecking = true;
		return this;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIDeserializer Build()
	{
		if (FsharpHelper.Instance == null)
		{
			FsharpHelper.Instance = new DefaultFsharpHelper();
		}
		return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializer.FromValueDeserializer(BuildValueDeserializer());
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueDeserializer BuildValueDeserializer()
	{
		return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAliasValueDeserializer(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeValueDeserializer(nodeDeserializerFactories.BuildComponentList(), nodeTypeResolverFactories.BuildComponentList(), typeConverter, enumNamingConvention, BuildTypeInspector()));
	}
}
