// ===============================================================================================
//	The MIT License (MIT) for UnityFBXExporter
//
//  UnityFBXExporter was created for Building Crafter (http://u3d.as/ovC) a tool to rapidly 
//	create high quality buildings right in Unity with no need to use 3D modeling programs.
//
//  Copyright (c) 2016 | 8Bit Goose Games, Inc.
//		
//	Permission is hereby granted, free of charge, to any person obtaining a copy 
//	of this software and associated documentation files (the "Software"), to deal 
//	in the Software without restriction, including without limitation the rights 
//	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
//	of the Software, and to permit persons to whom the Software is furnished to do so, 
//	subject to the following conditions:
//		
//	The above copyright notice and this permission notice shall be included in all 
//	copies or substantial portions of the Software.
//		
//	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//	INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
//	PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
//	HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
//	OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
//	OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// ===============================================================================================

using UnityEngine;
using System.Collections;
using UnityEditor;

namespace UnityFBXExporter
{
	public class ExporterMenu : Editor 
	{

		[MenuItem("Assets/FBX Exporter/Only GameObject", false, 30)]
		public static void ExportGameObjectToFBX()
		{
			ExportCurrentGameObject(false, false);
		}

		[MenuItem("Assets/FBX Exporter/With new Materials", false, 31)]
		public static void ExportGameObjectAndMaterialsToFBX()
		{
			ExportCurrentGameObject(true, false);
		}

		[MenuItem("Assets/FBX Exporter/With new Materials and Textures", false, 32)]
		public static void ExportGameObjectAndMaterialsTexturesToFBX()
		{
			ExportCurrentGameObject(true, true);
		}

		private static void ExportCurrentGameObject(bool copyMaterials, bool copyTextures)
		{
			if(Selection.activeGameObject == null)
			{
				EditorUtility.DisplayDialog("No Object Selected", "Please select any GameObject to Export to FBX", "Okay");
				return;
			}

			GameObject currentGameObject = Selection.activeObject as GameObject;
			
			if(currentGameObject == null)
			{
				EditorUtility.DisplayDialog("Warning", "Item selected is not a GameObject", "Okay");
				return;
			}

			string newPath = GetNewPath(currentGameObject);



			if(newPath != null && newPath.Length != 0)
			{
				bool isSuccess = FBXExporter.ExportGameObjToFBX(currentGameObject, newPath, copyMaterials, copyTextures);

				if(isSuccess == false)
					EditorUtility.DisplayDialog("Warning", "The extension probably wasn't an FBX file, could not export.", "Okay");
			}
				
		}

		/// <summary>
		/// Returns null if the path is actually garbage
		/// </summary>
		/// <returns>The new path or NULL if not a proper path</returns>
		/// <param name="gameObject">Game object.</param>
		private static string GetNewPath(GameObject gameObject)
		{
			string name = gameObject.name;

			string newPath = EditorUtility.SaveFilePanelInProject("Export FBX File", name + ".fbx", "fbx", "Export " + name + " GameObject to a FBX file");

			return newPath;
		}
	}
}