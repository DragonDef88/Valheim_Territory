using System;
using System.Collections.Generic;
using System.Linq;

namespace YamlDotNet.Serialization;

internal static class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELazyComponentRegistrationListExtensions
{
	public static TComponent BuildComponentChain<TComponent>(this _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELazyComponentRegistrationList<TComponent, TComponent> registrations, TComponent innerComponent)
	{
		return registrations.InReverseOrder.Aggregate(innerComponent, (TComponent inner, Func<TComponent, TComponent> factory) => factory(inner));
	}

	public static TComponent BuildComponentChain<TArgument, TComponent>(this _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELazyComponentRegistrationList<TArgument, TComponent> registrations, TComponent innerComponent, Func<TComponent, TArgument> argumentBuilder)
	{
		Func<TComponent, TArgument> argumentBuilder2 = argumentBuilder;
		return registrations.InReverseOrder.Aggregate(innerComponent, (TComponent inner, Func<TArgument, TComponent> factory) => factory(argumentBuilder2(inner)));
	}

	public static List<TComponent> BuildComponentList<TComponent>(this _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELazyComponentRegistrationList<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing, TComponent> registrations)
	{
		return registrations.Select((Func<_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing, TComponent> factory) => factory(default(_003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENothing))).ToList();
	}

	public static List<TComponent> BuildComponentList<TArgument, TComponent>(this _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ELazyComponentRegistrationList<TArgument, TComponent> registrations, TArgument argument)
	{
		TArgument argument2 = argument;
		return registrations.Select((Func<TArgument, TComponent> factory) => factory(argument2)).ToList();
	}
}
