using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace HarmonyLib;

public static class FileLog
{
	private static readonly object fileLock;

	public static string logPath;

	public static char indentChar;

	public static int indentLevel;

	private static List<string> buffer;

	static FileLog()
	{
		fileLock = new object();
		indentChar = '\t';
		indentLevel = 0;
		buffer = new List<string>();
		string environmentVariable = Environment.GetEnvironmentVariable("HARMONY_LOG_FILE");
		if (!string.IsNullOrEmpty(environmentVariable))
		{
			logPath = environmentVariable;
			return;
		}
		string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		Directory.CreateDirectory(folderPath);
		logPath = Path.Combine(folderPath, "harmony.log.txt");
	}

	private static string IndentString()
	{
		return new string(indentChar, indentLevel);
	}

	public static void ChangeIndent(int delta)
	{
		lock (fileLock)
		{
			indentLevel = Math.Max(0, indentLevel + delta);
		}
	}

	public static void LogBuffered(string str)
	{
		lock (fileLock)
		{
			buffer.Add(IndentString() + str);
		}
	}

	public static void LogBuffered(List<string> strings)
	{
		lock (fileLock)
		{
			buffer.AddRange(strings);
		}
	}

	public static List<string> GetBuffer(bool clear)
	{
		lock (fileLock)
		{
			List<string> result = buffer;
			if (clear)
			{
				buffer = new List<string>();
			}
			return result;
		}
	}

	public static void SetBuffer(List<string> buffer)
	{
		lock (fileLock)
		{
			FileLog.buffer = buffer;
		}
	}

	public static void FlushBuffer()
	{
		lock (fileLock)
		{
			if (buffer.Count <= 0)
			{
				return;
			}
			using (StreamWriter streamWriter = File.AppendText(logPath))
			{
				foreach (string item in buffer)
				{
					streamWriter.WriteLine(item);
				}
			}
			buffer.Clear();
		}
	}

	public static void Log(string str)
	{
		lock (fileLock)
		{
			using StreamWriter streamWriter = File.AppendText(logPath);
			streamWriter.WriteLine(IndentString() + str);
		}
	}

	public static void Debug(string str)
	{
		if (Harmony.DEBUG)
		{
			Log(str);
		}
	}

	public static void Reset()
	{
		lock (fileLock)
		{
			File.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}{Path.DirectorySeparatorChar}harmony.log.txt");
		}
	}

	public unsafe static void LogBytes(long ptr, int len)
	{
		lock (fileLock)
		{
			byte* ptr2 = (byte*)ptr;
			string text = "";
			for (int i = 1; i <= len; i++)
			{
				if (text.Length == 0)
				{
					text = "#  ";
				}
				text += $"{*ptr2:X2} ";
				if (i > 1 || len == 1)
				{
					if (i % 8 == 0 || i == len)
					{
						Log(text);
						text = "";
					}
					else if (i % 4 == 0)
					{
						text += " ";
					}
				}
				ptr2++;
			}
			byte[] destination = new byte[len];
			Marshal.Copy((IntPtr)ptr, destination, 0, len);
			byte[] array = MD5.Create().ComputeHash(destination);
			StringBuilder stringBuilder = new StringBuilder();
			for (int j = 0; j < array.Length; j++)
			{
				stringBuilder.Append(array[j].ToString("X2"));
			}
			Log($"HASH: {stringBuilder}");
		}
	}
}
