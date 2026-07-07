using System;
using System.Collections.Generic;

public class SaveWithBackups
{
	private List<SaveFile> m_saveFiles = new List<SaveFile>();

	private Action m_modifiedCallback;

	private bool m_isDirty;

	private SaveFile m_primaryFile;

	private List<SaveFile> m_backupFiles = new List<SaveFile>();

	private Dictionary<string, SaveFile> m_saveFilesByNameAndSource = new Dictionary<string, SaveFile>();

	public SaveFile PrimaryFile
	{
		get
		{
			EnsureSortedAndPrimaryFileDetermined();
			return m_primaryFile;
		}
	}

	public SaveFile[] BackupFiles
	{
		get
		{
			EnsureSortedAndPrimaryFileDetermined();
			return m_backupFiles.ToArray();
		}
	}

	public SaveFile[] AllFiles => m_saveFiles.ToArray();

	public ulong SizeWithBackups
	{
		get
		{
			ulong num = 0uL;
			for (int i = 0; i < m_saveFiles.Count; i++)
			{
				num += m_saveFiles[i].Size;
			}
			return num;
		}
	}

	public bool IsDeleted => PrimaryFile == null;

	public string m_name { get; private set; }

	public SaveCollection ParentSaveCollection { get; private set; }

	public SaveWithBackups(string name, SaveCollection parentSaveCollection, Action modifiedCallback)
	{
		m_name = name;
		ParentSaveCollection = parentSaveCollection;
		m_modifiedCallback = modifiedCallback;
	}

	public SaveFile AddSaveFile(string filePath, FileSource fileSource)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		SaveFile saveFile = new SaveFile(filePath, fileSource, this, OnModified);
		string key = saveFile.FileName + "_" + ((object)(FileSource)(ref saveFile.m_source)).ToString();
		if (m_saveFiles.Count > 0 && m_saveFilesByNameAndSource.TryGetValue(key, out var value))
		{
			value.AddAssociatedFiles(saveFile.AllPaths);
		}
		else
		{
			m_saveFiles.Add(saveFile);
			m_saveFilesByNameAndSource.Add(key, saveFile);
		}
		OnModified();
		return saveFile;
	}

	public SaveFile AddSaveFile(string[] filePaths, FileSource fileSource)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		SaveFile saveFile = new SaveFile(filePaths, fileSource, this, OnModified);
		string key = saveFile.FileName + "_" + ((object)(FileSource)(ref saveFile.m_source)).ToString();
		if (m_saveFiles.Count > 0 && m_saveFilesByNameAndSource.TryGetValue(key, out var value))
		{
			value.AddAssociatedFiles(saveFile.AllPaths);
		}
		else
		{
			m_saveFiles.Add(saveFile);
			m_saveFilesByNameAndSource.Add(key, saveFile);
		}
		OnModified();
		return saveFile;
	}

	public void RemoveSaveFile(SaveFile saveFile)
	{
		m_saveFiles.Remove(saveFile);
		string key = saveFile.FileName + "_" + ((object)(FileSource)(ref saveFile.m_source)).ToString();
		m_saveFilesByNameAndSource.Remove(key);
		OnModified();
	}

	private void EnsureSortedAndPrimaryFileDetermined()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Invalid comparison between Unknown and I4
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Invalid comparison between Unknown and I4
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Invalid comparison between Unknown and I4
		if (!m_isDirty)
		{
			return;
		}
		m_saveFiles.Sort(new SaveFileComparer());
		m_primaryFile = null;
		for (int i = 0; i < m_saveFiles.Count; i++)
		{
			if (SaveSystem.GetSaveInfo(m_saveFiles[i].PathPrimary, out var _, out var saveFileType, out var _, out var _) && saveFileType == SaveFileType.Single && (m_primaryFile == null || (int)m_saveFiles[i].m_source == 2 || ((int)m_saveFiles[i].m_source == 1 && (int)m_primaryFile.m_source == 3)))
			{
				m_primaryFile = m_saveFiles[i];
			}
		}
		if (m_primaryFile != null)
		{
			m_name = m_primaryFile.FileName;
		}
		m_backupFiles.Clear();
		if (m_primaryFile == null)
		{
			m_backupFiles.AddRange(m_saveFiles);
		}
		else
		{
			for (int j = 0; j < m_saveFiles.Count; j++)
			{
				if (m_saveFiles[j] != m_primaryFile)
				{
					m_backupFiles.Add(m_saveFiles[j]);
				}
			}
		}
		m_isDirty = false;
	}

	private void OnModified()
	{
		SetDirty();
		m_modifiedCallback?.Invoke();
	}

	private void SetDirty()
	{
		m_isDirty = true;
	}
}
