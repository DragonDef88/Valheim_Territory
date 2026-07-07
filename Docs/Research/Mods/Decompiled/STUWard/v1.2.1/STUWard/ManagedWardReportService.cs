using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace STUWard;

internal static class ManagedWardReportService
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static ConsoleEvent _003C_003E9__8_0;

		internal void _003CEnsureConsoleCommandRegistered_003Eb__8_0(ConsoleEventArgs args)
		{
			TryHandleConsoleCommand(args.Context, args.FullLine);
		}
	}

	private const string RequestWardReportRpc = "STUWard_RequestWardReport";

	private const string ReceiveWardReportRpc = "STUWard_ReceiveWardReport";

	private const string WardReportConsoleCommand = "stuw_wardreport";

	private static bool _rpcsRegistered;

	private static bool _consoleCommandRegistered;

	internal static void OnZNetAwake()
	{
		_rpcsRegistered = false;
	}

	internal static void RegisterRpcs()
	{
		ZRoutedRpc instance = ZRoutedRpc.instance;
		if (!_rpcsRegistered && instance != null)
		{
			instance.Register("STUWard_RequestWardReport", (Action<long>)HandleRequestWardReport);
			instance.Register<ZPackage>("STUWard_ReceiveWardReport", (Action<long, ZPackage>)HandleReceiveWardReport);
			_rpcsRegistered = true;
		}
	}

	internal static bool TryHandleConsoleCommand(Terminal? terminal, string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return false;
		}
		if (!(text?.Trim() ?? string.Empty).Equals("stuw_wardreport", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if ((Object)(object)ZNet.instance == (Object)null)
		{
			if (terminal != null)
			{
				terminal.AddString("STUWard: ward report is not available right now.");
			}
			return true;
		}
		if (ZNet.instance.IsServer())
		{
			WriteWardReportToTerminal(terminal);
			return true;
		}
		WardOwnership.RegisterRpcs();
		if (terminal != null)
		{
			terminal.AddString("STUWard: requested ward report generation on the server.");
		}
		ZRoutedRpc instance = ZRoutedRpc.instance;
		if (instance != null)
		{
			instance.InvokeRoutedRPC("STUWard_RequestWardReport", Array.Empty<object>());
		}
		return true;
	}

	internal static void EnsureConsoleCommandRegistered(Terminal? terminal)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected O, but got Unknown
		if (_consoleCommandRegistered)
		{
			AddCommandToAutocomplete(terminal);
			return;
		}
		object obj = _003C_003Ec._003C_003E9__8_0;
		if (obj == null)
		{
			ConsoleEvent val = delegate(ConsoleEventArgs args)
			{
				TryHandleConsoleCommand(args.Context, args.FullLine);
			};
			_003C_003Ec._003C_003E9__8_0 = val;
			obj = (object)val;
		}
		new ConsoleCommand("stuw_wardreport", "Generate the STUWard ward ownership/count report.", (ConsoleEvent)obj, false, false, false, false, false, (ConsoleOptionsFetcher)null, false, false, false);
		_consoleCommandRegistered = true;
		AddCommandToAutocomplete(terminal);
	}

	private static void AddCommandToAutocomplete(Terminal? terminal)
	{
		if (!((Object)(object)terminal == (Object)null) && terminal.m_commandList != null && !terminal.m_commandList.Contains("stuw_wardreport"))
		{
			terminal.m_commandList.Add("stuw_wardreport");
			terminal.m_commandList.Sort(StringComparer.OrdinalIgnoreCase);
		}
	}

	private static void WriteWardReportToTerminal(Terminal? terminal)
	{
		if (WardOwnership.TryWriteWardCountReport(out string reportPath, out int trackedAccounts, out int totalWards, out int unresolvedOwners))
		{
			if (terminal != null)
			{
				terminal.AddString("STUWard: wrote ward report to " + reportPath);
			}
			if (terminal != null)
			{
				terminal.AddString(string.Format("{0}: tracked accounts={1}, total wards={2}, unresolved owner wards={3}", "STUWard", trackedAccounts, totalWards, unresolvedOwners));
			}
		}
		else if (terminal != null)
		{
			terminal.AddString("STUWard: failed to write ward report. Check the log for details.");
		}
	}

	private static void HandleRequestWardReport(long sender)
	{
		if ((Object)(object)ZNet.instance == (Object)null || !ZNet.instance.IsServer())
		{
			return;
		}
		if (!WardOwnership.TryResolveAuthoritativePlayerIdFromSender(sender, "WardReport.Request", out var playerId))
		{
			SendWardReportResponse(sender, success: false, string.Empty, 0, 0, 0, "Could not resolve the requesting player on the server.");
			return;
		}
		string playerAccountId = WardOwnership.GetPlayerAccountId(playerId);
		string reportContents;
		int trackedAccounts;
		int totalWards;
		int unresolvedOwners;
		if (!WardAdminDebugAccess.IsAdminAccountId(playerAccountId))
		{
			Plugin.Log.LogWarning((object)$"Rejected ward report request from non-admin playerId={playerId} accountId='{playerAccountId}'.");
			SendWardReportResponse(sender, success: false, string.Empty, 0, 0, 0, "Ward report is only available to server admins.");
		}
		else if (WardOwnership.TryBuildWardCountReport(out reportContents, out trackedAccounts, out totalWards, out unresolvedOwners))
		{
			Plugin.Log.LogInfo((object)$"Prepared ward report for admin playerId={playerId}. tracked accounts={trackedAccounts}, total wards={totalWards}, unresolved owner wards={unresolvedOwners}");
			SendWardReportResponse(sender, success: true, reportContents, trackedAccounts, totalWards, unresolvedOwners, string.Empty);
		}
		else
		{
			Plugin.Log.LogWarning((object)$"Failed to build ward report for admin playerId={playerId}.");
			SendWardReportResponse(sender, success: false, string.Empty, 0, 0, 0, "Failed to generate ward report on the server. Check the server log for details.");
		}
	}

	private static void HandleReceiveWardReport(long _, ZPackage pkg)
	{
		bool num = pkg.ReadBool();
		int num2 = pkg.ReadInt();
		int num3 = pkg.ReadInt();
		int num4 = pkg.ReadInt();
		string text = pkg.ReadString();
		string contents = pkg.ReadString();
		if (!num)
		{
			Plugin.Log.LogWarning((object)("STUWard: " + text));
			return;
		}
		string reportFilePath = WardOwnership.GetReportFilePath();
		try
		{
			File.WriteAllText(reportFilePath, contents);
			Plugin.Log.LogInfo((object)("STUWard: wrote ward report to " + reportFilePath));
			Plugin.Log.LogInfo((object)string.Format("{0}: tracked accounts={1}, total wards={2}, unresolved owner wards={3}", "STUWard", num2, num3, num4));
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning((object)("STUWard: failed to write ward report to " + reportFilePath + ": " + ex.Message));
		}
	}

	private static void SendWardReportResponse(long receiverUid, bool success, string reportContents, int trackedAccounts, int totalWards, int unresolvedOwners, string message)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		ZRoutedRpc instance = ZRoutedRpc.instance;
		if (instance != null)
		{
			ZPackage val = new ZPackage();
			val.Write(success);
			val.Write(trackedAccounts);
			val.Write(totalWards);
			val.Write(unresolvedOwners);
			val.Write(message ?? string.Empty);
			val.Write(reportContents ?? string.Empty);
			instance.InvokeRoutedRPC(receiverUid, "STUWard_ReceiveWardReport", new object[1] { val });
		}
	}
}
