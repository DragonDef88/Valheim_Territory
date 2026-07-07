using System;

namespace HarmonyLib.Tools;

public static class Logger
{
	public class LogEventArgs : EventArgs
	{
		public LogChannel LogChannel { get; internal set; }

		public string Message { get; internal set; }
	}

	[Flags]
	public enum LogChannel
	{
		None = 0,
		Info = 2,
		IL = 4,
		Warn = 8,
		Error = 0x10,
		Debug = 0x20,
		All = 0x3E
	}

	public static LogChannel ChannelFilter { get; set; }

	public static event EventHandler<LogEventArgs> MessageReceived;

	internal static bool IsEnabledFor(LogChannel channel)
	{
		return (channel & ChannelFilter) != 0;
	}

	internal static void Log(LogChannel channel, Func<string> message, bool forcePropagation = false)
	{
		if (forcePropagation || IsEnabledFor(channel))
		{
			Logger.MessageReceived?.Invoke(null, new LogEventArgs
			{
				LogChannel = channel,
				Message = message()
			});
		}
	}

	internal static void LogText(LogChannel channel, string message, bool forcePropagation = false)
	{
		if (forcePropagation || IsEnabledFor(channel))
		{
			Logger.MessageReceived?.Invoke(null, new LogEventArgs
			{
				LogChannel = channel,
				Message = message
			});
		}
	}
}
