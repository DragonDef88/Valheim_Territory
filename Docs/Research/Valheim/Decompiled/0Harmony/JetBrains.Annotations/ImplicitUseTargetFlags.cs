using System;

namespace JetBrains.Annotations;

[Flags]
internal enum ImplicitUseTargetFlags
{
	Default = 1,
	Itself = 1,
	Members = 2,
	WithInheritors = 4,
	WithMembers = 3
}
