using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class BuildProject
{
	[MenuItem("Project/Build/OSX")]
	static public void BuildMacOS()
	{
		Build(BuildTarget.StandaloneOSXUniversal, "exports/osx");
	}

	[MenuItem("Project/Build/Android")]
	static public void BuildAndroid()
	{
		Build(BuildTarget.Android, "exports/android.apk");
	}

	[MenuItem("Project/Build/iOS")]
	static public void BuildIOS()
	{
		Build(BuildTarget.iOS, "exports/ios");
	}

	static void Build(BuildTarget target, string output)
	{
		ResourcePath.instance.LoadXML();

		var directory = Path.GetDirectoryName(output);
		if (!Directory.Exists(directory)) {
			Directory.CreateDirectory(directory);
		}

		List<string> allScene = new List<string>();
		foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes) {
			if (scene.enabled) {
				allScene.Add(scene.path);
			}
		}
		BuildPipeline.BuildPlayer(
			allScene.ToArray(),
			output,
			target,
			BuildOptions.None
		);
	}
}
