using System;
using System.Diagnostics;
using UnityEngine;

public class CustomLogger : MonoBehaviour
{
	private static string s_link = Application.persistentDataPath + "/Player.log";

	private static string s_target = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/Library/Logs/IronGate/Valheim/Player.log";

	public static void SetupSymbolicLink()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if ((int)Application.platform != 1 && (int)Application.platform != 0)
		{
			Debug.LogError((object)"Only use SetupSymbolicLink on MacOS in its current incarnation!");
			return;
		}
		try
		{
			Process.Start(new ProcessStartInfo("/bin/ln", "-sf \"" + s_target + "\" \"" + s_link + "\"")
			{
				UseShellExecute = false,
				CreateNoWindow = true
			});
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Error when trying to create symbolic link to log file in Application.persistentDataPath! " + ex.Message));
		}
	}
}
