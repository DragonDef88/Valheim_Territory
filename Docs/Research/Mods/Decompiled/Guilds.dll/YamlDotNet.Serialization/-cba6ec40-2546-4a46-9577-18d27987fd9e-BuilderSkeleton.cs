using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using YamlDotNet.Core;
using YamlDotNet.Serialization.Converters;
using YamlDotNet.Serialization.NamingConventions;

namespace YamlDotNet.Serialization;

internal abstract class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBuilderSkeleton<TBuilder> where TBuilder : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBuilderSkeleton<TBuilder>
{
	internal _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention namingConvention = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENullNamingConvention.Instance;

	internal _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention enumNamingConvention = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENullNamingConvention.Instance;

	internal _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeResolver typeResolver;

	internal readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributeOverrides overrides;

	internal readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELazyComponentRegistrationList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter> typeConverterFactories;

	internal readonly _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELazyComponentRegistrationList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector> typeInspectorFactories;

	internal bool ignoreFields;

	internal bool includeNonPublicProperties;

	internal Settings settings;

	internal _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlFormatter yamlFormatter = _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlFormatter.Default;

	protected abstract TBuilder Self { get; }

	internal _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EBuilderSkeleton(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeResolver typeResolver)
	{
		overrides = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlAttributeOverrides();
		typeConverterFactories = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELazyComponentRegistrationList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter>
		{
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EGuidConverter),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EGuidConverter(jsonCompatible: false)
			},
			{
				typeof(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESystemTypeConverter),
				(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ESystemTypeConverter()
			}
		};
		typeInspectorFactories = new _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELazyComponentRegistrationList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector>();
		this.typeResolver = typeResolver ?? throw new ArgumentNullException("typeResolver");
		settings = new Settings();
	}

	public TBuilder IgnoreFields()
	{
		ignoreFields = true;
		return Self;
	}

	public TBuilder IncludeNonPublicProperties()
	{
		includeNonPublicProperties = true;
		return Self;
	}

	public TBuilder EnablePrivateConstructors()
	{
		settings.AllowPrivateConstructors = true;
		return Self;
	}

	public TBuilder WithNamingConvention(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention namingConvention)
	{
		this.namingConvention = namingConvention ?? throw new ArgumentNullException("namingConvention");
		return Self;
	}

	public TBuilder WithEnumNamingConvention(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EINamingConvention enumNamingConvention)
	{
		this.enumNamingConvention = enumNamingConvention;
		return Self;
	}

	public TBuilder WithTypeResolver(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeResolver typeResolver)
	{
		this.typeResolver = typeResolver ?? throw new ArgumentNullException("typeResolver");
		return Self;
	}

	public abstract TBuilder WithTagMapping(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ETagName tag, Type type);

	public TBuilder WithAttributeOverride<TClass>(Expression<Func<TClass, object>> propertyAccessor, Attribute attribute)
	{
		overrides.Add(propertyAccessor, attribute);
		return Self;
	}

	public TBuilder WithAttributeOverride(Type type, string member, Attribute attribute)
	{
		overrides.Add(type, member, attribute);
		return Self;
	}

	public TBuilder WithTypeConverter(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter typeConverter)
	{
		return WithTypeConverter(typeConverter, delegate(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter> w)
		{
			w.OnTop();
		});
	}

	public TBuilder WithTypeConverter(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter typeConverter, Action<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter>> where)
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter typeConverter2 = typeConverter;
		if (typeConverter2 == null)
		{
			throw new ArgumentNullException("typeConverter");
		}
		if (where == null)
		{
			throw new ArgumentNullException("where");
		}
		where(typeConverterFactories.CreateRegistrationLocationSelector(typeConverter2.GetType(), (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => typeConverter2));
		return Self;
	}

	public TBuilder WithTypeConverter<TYamlTypeConverter>(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EWrapperFactory<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter> typeConverterFactory, Action<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITrackingRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter>> where) where TYamlTypeConverter : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EWrapperFactory<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter> typeConverterFactory2 = typeConverterFactory;
		if (typeConverterFactory2 == null)
		{
			throw new ArgumentNullException("typeConverterFactory");
		}
		if (where == null)
		{
			throw new ArgumentNullException("where");
		}
		where(typeConverterFactories.CreateTrackingRegistrationLocationSelector(typeof(TYamlTypeConverter), (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter wrapped, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing _) => typeConverterFactory2(wrapped)));
		return Self;
	}

	public TBuilder WithoutTypeConverter<TYamlTypeConverter>() where TYamlTypeConverter : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter
	{
		return WithoutTypeConverter(typeof(TYamlTypeConverter));
	}

	public TBuilder WithoutTypeConverter(Type converterType)
	{
		if (converterType == null)
		{
			throw new ArgumentNullException("converterType");
		}
		typeConverterFactories.Remove(converterType);
		return Self;
	}

	public TBuilder WithTypeInspector<TTypeInspector>(Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector, TTypeInspector> typeInspectorFactory) where TTypeInspector : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector
	{
		return WithTypeInspector(typeInspectorFactory, delegate(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector> w)
		{
			w.OnTop();
		});
	}

	public TBuilder WithTypeInspector<TTypeInspector>(Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector, TTypeInspector> typeInspectorFactory, Action<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector>> where) where TTypeInspector : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector
	{
		Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector, TTypeInspector> typeInspectorFactory2 = typeInspectorFactory;
		if (typeInspectorFactory2 == null)
		{
			throw new ArgumentNullException("typeInspectorFactory");
		}
		if (where == null)
		{
			throw new ArgumentNullException("where");
		}
		where(typeInspectorFactories.CreateRegistrationLocationSelector(typeof(TTypeInspector), (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector inner) => typeInspectorFactory2(inner)));
		return Self;
	}

	public TBuilder WithTypeInspector<TTypeInspector>(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EWrapperFactory<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector, TTypeInspector> typeInspectorFactory, Action<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITrackingRegistrationLocationSelectionSyntax<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector>> where) where TTypeInspector : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector
	{
		_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EWrapperFactory<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector, TTypeInspector> typeInspectorFactory2 = typeInspectorFactory;
		if (typeInspectorFactory2 == null)
		{
			throw new ArgumentNullException("typeInspectorFactory");
		}
		if (where == null)
		{
			throw new ArgumentNullException("where");
		}
		where(typeInspectorFactories.CreateTrackingRegistrationLocationSelector(typeof(TTypeInspector), (_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector wrapped, _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector inner) => typeInspectorFactory2(wrapped, inner)));
		return Self;
	}

	public TBuilder WithoutTypeInspector<TTypeInspector>() where TTypeInspector : _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EITypeInspector
	{
		return WithoutTypeInspector(typeof(TTypeInspector));
	}

	public TBuilder WithoutTypeInspector(Type inspectorType)
	{
		if (inspectorType == null)
		{
			throw new ArgumentNullException("inspectorType");
		}
		typeInspectorFactories.Remove(inspectorType);
		return Self;
	}

	public TBuilder WithYamlFormatter(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EYamlFormatter formatter)
	{
		yamlFormatter = formatter ?? throw new ArgumentNullException("formatter");
		return Self;
	}

	protected IEnumerable<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003EIYamlTypeConverter> BuildTypeConverters()
	{
		return typeConverterFactories.BuildComponentList();
	}
}
