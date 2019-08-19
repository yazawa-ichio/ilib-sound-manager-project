using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class ReleaseTool
{
	[MenuItem("ILib/Audio/Release SoundManager")]
	static void ReleaseSoundManager()
	{
		var name = "Assets/SoundControl/Extension/ILib.SoundManager.unitypackage";
		var paths = AssetDatabase.GetAllAssetPaths().Where(x => x.StartsWith("Assets/SoundManager"));
		AssetDatabase.ExportPackage(paths.ToArray(), name, ExportPackageOptions.Default);
	}
}
