using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SyncedList
{
	private const float m_loadInterval = 10f;

	private float m_lastLoadCheckTime;

	private List<string> m_comments = new List<string>();

	private List<string> m_list = new List<string>();

	private FileHelpers.FileLocation m_fileLocation;

	private DateTime m_lastLoadDate = DateTime.MinValue;

	public SyncedList(FileHelpers.FileLocation fileLocation, string defaultFileComment)
	{
		m_fileLocation = fileLocation;
		if (FileHelpers.Exists(m_fileLocation.m_path, m_fileLocation.m_fileSource))
		{
			Load();
			return;
		}
		m_comments.Add("// " + defaultFileComment);
		Save();
	}

	public List<string> GetList()
	{
		CheckLoad();
		return m_list;
	}

	public int Count()
	{
		CheckLoad();
		return m_list.Count;
	}

	public bool Contains(string s)
	{
		CheckLoad();
		return m_list.Contains(s);
	}

	public void Add(string s)
	{
		Load();
		if (!m_list.Contains(s))
		{
			m_list.Add(s);
			Save();
		}
	}

	public void Remove(string s)
	{
		Load();
		if (m_list.Remove(s))
		{
			Save();
		}
	}

	private void Save()
	{
		string path = m_fileLocation.m_path;
		FileHelpers.FileSource fileSource = m_fileLocation.m_fileSource;
		FileWriter fileWriter = new FileWriter(path, FileHelpers.FileHelperType.Stream, fileSource);
		try
		{
			StreamWriter stream = fileWriter.m_stream;
			foreach (string comment in m_comments)
			{
				stream.WriteLine(comment);
			}
			foreach (string item in m_list)
			{
				stream.WriteLine(item);
			}
		}
		catch (Exception)
		{
			ZLog.LogError("Failed to save synced list!");
		}
		finally
		{
			fileWriter.Finish();
		}
		m_lastLoadDate = File.GetLastWriteTime(path);
	}

	private void CheckLoad()
	{
		if (Time.realtimeSinceStartup - m_lastLoadCheckTime > 10f)
		{
			Load();
			m_lastLoadCheckTime = Time.realtimeSinceStartup;
		}
	}

	private void Load()
	{
		FileReader fileReader = null;
		try
		{
			string path = m_fileLocation.m_path;
			FileHelpers.FileSource fileSource = m_fileLocation.m_fileSource;
			DateTime lastWriteTime = FileHelpers.GetLastWriteTime(path, fileSource);
			if (lastWriteTime <= m_lastLoadDate)
			{
				return;
			}
			m_lastLoadDate = lastWriteTime;
			m_comments.Clear();
			m_list.Clear();
			fileReader = new FileReader(path, fileSource, FileHelpers.FileHelperType.Stream);
			StreamReader stream = fileReader.m_stream;
			string text;
			while ((text = stream.ReadLine()) != null)
			{
				if (text.Length > 0)
				{
					if (text.StartsWith("//"))
					{
						m_comments.Add(text);
					}
					else
					{
						m_list.Add(text);
					}
				}
			}
		}
		catch (Exception)
		{
		}
		finally
		{
			fileReader?.Dispose();
		}
	}
}
