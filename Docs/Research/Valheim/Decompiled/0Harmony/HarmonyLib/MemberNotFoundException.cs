using System;

namespace HarmonyLib;

public class MemberNotFoundException : Exception
{
	public MemberNotFoundException(string message)
		: base(message)
	{
	}
}
