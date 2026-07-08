using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Serialization.BufferedDeserialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.NodeDeserializers;
using YamlDotNet.Serialization.NodeTypeResolvers;
using YamlDotNet.Serialization.ObjectFactories;
using YamlDotNet.Serialization.Schemas;
using YamlDotNet.Serialization.TypeInspectors;
using YamlDotNet.Serialization.Utilities;
using YamlDotNet.Serialization.ValueDeserializers;

namespace YamlDotNet.Serialization;

internal sealed class StaticDeserializerBuilder : StaticBuilderSkeleton<StaticDeserializerBuilder>
{
	private readonly StaticContext context;

	private readonly StaticObjectFactory factory;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELazyComponentRegistrationList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer> nodeDeserializerFactories;

	private readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELazyComponentRegistrationList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver> nodeTypeResolverFactories;

	private readonly Dictionary<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName, Type> tagMappings;

	private readonly ITypeConverter typeConverter;

	private readonly Dictionary<Type, Type> typeMappings;

	private bool ignoreUnmatched;

	private bool duplicateKeyChecking;

	private bool attemptUnknownTypeDeserialization;

	private bool enforceNullability;

	private bool caseInsensitivePropertyMatching;

	protected override StaticDeserializerBuilder Self => this;

	public StaticDeserializerBuilder(StaticContext context)
		: base(context.GetTypeResolver())
	{
		this.context = context;
		factory = context.GetFactory();
		typeMappings = new Dictionary<Type, Type>();
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
		nodeDeserializerFactories = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELazyComponentRegistrationList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer>
		{
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlConvertibleNodeDeserializer),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlConvertibleNodeDeserializer(factory)
			},
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSerializableNodeDeserializer),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlSerializableNodeDeserializer(factory)
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
				typeof(StaticArrayNodeDeserializer),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new StaticArrayNodeDeserializer(factory)
			},
			{
				typeof(StaticDictionaryNodeDeserializer),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new StaticDictionaryNodeDeserializer(factory, duplicateKeyChecking)
			},
			{
				typeof(StaticCollectionNodeDeserializer),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new StaticCollectionNodeDeserializer(factory)
			},
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectNodeDeserializer),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EObjectNodeDeserializer(factory, BuildTypeInspector(), ignoreUnmatched, duplicateKeyChecking, typeConverter, enumNamingConvention, enforceNullability, caseInsensitivePropertyMatching, enforceRequiredProperties: false, BuildTypeConverters())
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
		typeConverter = new NullTypeConverter();
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector BuildTypeInspector()
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector typeInspector = context.GetTypeInspector();
		return typeInspectorFactories.BuildComponentChain(typeInspector);
	}

	public StaticDeserializerBuilder WithAttemptingUnquotedStringTypeDeserialization()
	{
		attemptUnknownTypeDeserialization = true;
		return this;
	}

	public StaticDeserializerBuilder WithNodeDeserializer(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer nodeDeserializer)
	{
		return WithNodeDeserializer(nodeDeserializer, delegate(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer> w)
		{
			w.OnTop();
		});
	}

	public StaticDeserializerBuilder WithNodeDeserializer(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer nodeDeserializer, Action<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer>> where)
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

	public StaticDeserializerBuilder WithNodeDeserializer<TNodeDeserializer>(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EWrapperFactory<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer, TNodeDeserializer> nodeDeserializerFactory, Action<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITrackingRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer>> where) where TNodeDeserializer : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer
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

	public StaticDeserializerBuilder WithCaseInsensitivePropertyMatching()
	{
		caseInsensitivePropertyMatching = true;
		return this;
	}

	public StaticDeserializerBuilder WithEnforceNullability()
	{
		enforceNullability = true;
		return this;
	}

	public StaticDeserializerBuilder WithoutNodeDeserializer<TNodeDeserializer>() where TNodeDeserializer : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer
	{
		return WithoutNodeDeserializer(typeof(TNodeDeserializer));
	}

	public StaticDeserializerBuilder WithoutNodeDeserializer(Type nodeDeserializerType)
	{
		if (nodeDeserializerType == null)
		{
			throw new ArgumentNullException("nodeDeserializerType");
		}
		nodeDeserializerFactories.Remove(nodeDeserializerType);
		return this;
	}

	public StaticDeserializerBuilder WithTypeDiscriminatingNodeDeserializer(Action<ITypeDiscriminatingNodeDeserializerOptions> configureTypeDiscriminatingNodeDeserializerOptions, int maxDepth = -1, int maxLength = -1)
	{
		TypeDiscriminatingNodeDeserializerOptions typeDiscriminatingNodeDeserializerOptions = new TypeDiscriminatingNodeDeserializerOptions();
		configureTypeDiscriminatingNodeDeserializerOptions(typeDiscriminatingNodeDeserializerOptions);
		TypeDiscriminatingNodeDeserializer nodeDeserializer = new TypeDiscriminatingNodeDeserializer(nodeDeserializerFactories.BuildComponentList(), typeDiscriminatingNodeDeserializerOptions.discriminators, maxDepth, maxLength);
		return WithNodeDeserializer(nodeDeserializer, delegate(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeDeserializer> s)
		{
			s.Before<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDictionaryNodeDeserializer>();
		});
	}

	public StaticDeserializerBuilder WithNodeTypeResolver(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver nodeTypeResolver)
	{
		return WithNodeTypeResolver(nodeTypeResolver, delegate(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver> w)
		{
			w.OnTop();
		});
	}

	public StaticDeserializerBuilder WithNodeTypeResolver(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver nodeTypeResolver, Action<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver>> where)
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

	public StaticDeserializerBuilder WithNodeTypeResolver<TNodeTypeResolver>(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EWrapperFactory<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver, TNodeTypeResolver> nodeTypeResolverFactory, Action<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITrackingRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver>> where) where TNodeTypeResolver : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver
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

	public StaticDeserializerBuilder WithoutNodeTypeResolver<TNodeTypeResolver>() where TNodeTypeResolver : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINodeTypeResolver
	{
		return WithoutNodeTypeResolver(typeof(TNodeTypeResolver));
	}

	public StaticDeserializerBuilder WithoutNodeTypeResolver(Type nodeTypeResolverType)
	{
		if (nodeTypeResolverType == null)
		{
			throw new ArgumentNullException("nodeTypeResolverType");
		}
		nodeTypeResolverFactories.Remove(nodeTypeResolverType);
		return this;
	}

	public override StaticDeserializerBuilder WithTagMapping(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName tag, Type type)
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

	public StaticDeserializerBuilder WithTypeMapping<TInterface, TConcrete>() where TConcrete : TInterface
	{
		Type typeFromHandle = typeof(TInterface);
		Type typeFromHandle2 = typeof(TConcrete);
		if (!typeFromHandle.IsAssignableFrom(typeFromHandle2))
		{
			throw new InvalidOperationException("The type '" + typeFromHandle2.Name + "' does not implement interface '" + typeFromHandle.Name + "'.");
		}
		typeMappings[typeFromHandle] = typeFromHandle2;
		return this;
	}

	public StaticDeserializerBuilder WithoutTagMapping(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName tag)
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

	public StaticDeserializerBuilder IgnoreUnmatchedProperties()
	{
		ignoreUnmatched = true;
		return this;
	}

	public StaticDeserializerBuilder WithDuplicateKeyChecking()
	{
		duplicateKeyChecking = true;
		return this;
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIDeserializer Build()
	{
		return _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EDeserializer.FromValueDeserializer(BuildValueDeserializer());
	}

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIValueDeserializer BuildValueDeserializer()
	{
		return new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EAliasValueDeserializer(new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENodeValueDeserializer(nodeDeserializerFactories.BuildComponentList(), nodeTypeResolverFactories.BuildComponentList(), typeConverter, enumNamingConvention, BuildTypeInspector()));
	}
}
