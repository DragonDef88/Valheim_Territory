namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
[ExcludeFromCodeCoverage]
[DebuggerNonUserCode]
internal sealed class _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENotNullWhenAttribute : Attribute
{
	public bool ReturnValue { get; }

	public _003Ccba6ec40_002D2546_002D4a46_002D9577_002D18d27987fd9e_003ENotNullWhenAttribute(bool returnValue)
	{
		ReturnValue = returnValue;
	}
}
