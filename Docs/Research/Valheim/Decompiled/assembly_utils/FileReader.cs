using System;
using System.IO;
using Splatform;

public class FileReader
{
	public BinaryReader m_binary;

	public StreamReader m_stream;

	private string m_path;

	private MemoryStream m_mem;

	private FileStream m_file;

	public FileHelpers.FileHelperType m_type { get; private set; }

	public FileHelpers.FileSource m_fileSource { get; private set; }

	public FileReader(string path, FileHelpers.FileSource fileSource, FileHelpers.FileHelperType type = FileHelpers.FileHelperType.Binary)
	{
		m_path = path;
		m_type = type;
		m_fileSource = fileSource;
		if (m_fileSource == FileHelpers.FileSource.Cloud)
		{
			if (FileHelpers.CloudStorageSupported)
			{
				if (!FileHelpers.Mount((SaveDataAccess)1))
				{
					throw new IOException("Mounting failed!");
				}
				bool flag = default(bool);
				if (!FileHelpers.CloudStorage.FileExists(path, ref flag) || !flag)
				{
					FileHelpers.Unmount();
					throw new FileNotFoundException();
				}
				byte[] buffer = default(byte[]);
				bool num = FileHelpers.CloudStorage.ReadFile(path, ref buffer);
				FileHelpers.Unmount();
				if (!num)
				{
					throw new Exception("Connected Storage file missing");
				}
				m_mem = new MemoryStream(buffer);
				if (m_type == FileHelpers.FileHelperType.Binary)
				{
					m_binary = new BinaryReader(m_mem);
					return;
				}
				if (m_type == FileHelpers.FileHelperType.Stream)
				{
					m_stream = new StreamReader(m_mem);
					return;
				}
				throw new NotImplementedException();
			}
			throw new FileNotFoundException();
		}
		m_file = File.OpenRead(m_path);
		if (m_type == FileHelpers.FileHelperType.Binary)
		{
			m_binary = new BinaryReader(m_file);
			return;
		}
		if (m_type == FileHelpers.FileHelperType.Stream)
		{
			m_stream = new StreamReader(m_file);
			return;
		}
		throw new NotImplementedException();
	}

	public void Dispose()
	{
		if (FileHelpers.CloudStorageSupported)
		{
			_ = m_fileSource;
			_ = 2;
		}
		m_binary?.Dispose();
		m_stream?.Dispose();
	}

	public static explicit operator BinaryReader(FileReader w)
	{
		return w.m_binary;
	}

	public static explicit operator StreamReader(FileReader w)
	{
		return w.m_stream;
	}
}
