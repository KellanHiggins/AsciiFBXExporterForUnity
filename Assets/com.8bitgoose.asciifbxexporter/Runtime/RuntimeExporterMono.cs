using UnityEngine;

namespace AsciiFBXExporter
{
	/// <summary>
	/// Simple mono component that shows how to export an object at runtime.
	/// Attach this to any game object and assign RootObjectToExport
	/// </summary>
	public class RuntimeExporterMono : MonoBehaviour
	{
		public GameObject rootObjectToExport;
		public string RelativeFolderPath = "/Ignore/RuntimeExport/";
		public string FileName = "TestFBXExport.fbx";
		public string TextureFolderName = "FBXTextures/";
		public bool UseGUI = true;

		void OnGUI()
		{
			if(UseGUI == false)
				return;

			if(rootObjectToExport != null && GUI.Button(new Rect(10, 10, 150, 50), "Export FBX"))
			{
				this.ExportGameObject();
			}
		}

		public bool ExportGameObject()
		{
			return ExportGameObject(rootObjectToExport, RelativeFolderPath, FileName, TextureFolderName);
		}

		/// <summary>
		/// Will export to whatever folder path is provided within the Assets folder
		/// </summary>
		/// <param name="rootGameObject"></param>
		/// <param name="folderPath"></param>
		/// <param name="fileName"></param>
		/// <param name="textureFolderName"></param>
		/// <returns></returns>
		public static bool ExportGameObject(GameObject rootGameObject, string folderPath, string fileName, string textureFolderName)
		{
			if(rootGameObject == null)
			{
				Debug.Log("Root game object is null, please assign it");
				return false;
			}

			// forces use of forward slash for directory names
			folderPath = folderPath.Replace('\\', '/');
			textureFolderName = textureFolderName.Replace('\\', '/');

			folderPath = Application.dataPath + folderPath;

			if(System.IO.Directory.Exists(folderPath) == false)
			{
				System.IO.Directory.CreateDirectory(folderPath);
			}

			if(System.IO.Path.GetExtension(fileName).ToLower() != ".fbx")
			{
				Debug.LogError(fileName + " does not end in .fbx, please save a file with the extension .fbx");
				return false;
			}

			if(folderPath[folderPath.Length - 1] != '/')
				folderPath += "/";

			if(System.IO.File.Exists(folderPath + fileName))
				System.IO.File.Delete(folderPath + fileName);

			bool exported = FBXExporter.ExportGameObjAtRuntime(rootGameObject, folderPath, fileName, textureFolderName, true);

#if UNITY_EDITOR
			UnityEditor.AssetDatabase.Refresh();
#endif
			return exported;
		}
	}
}