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
		// Dropdown
		[MenuItem("GameObject/FBX Exporter/Only GameObject", false, 40)]
		public static void ExportDropdownGameObjectToFBX()
		{
			ExportCurrentGameObject(false, false);
		}

		[MenuItem("GameObject/FBX Exporter/With new Materials", false, 41)]
		public static void ExportDropdownGameObjectAndMaterialsToFBX()
		{
			ExportCurrentGameObject(true, false);
		}

		[MenuItem("GameObject/FBX Exporter/With new Materials and Textures", false, 42)]
		public static void ExportDropdownGameObjectAndMaterialsTexturesToFBX()
		{
			ExportCurrentGameObject(true, true);
		}

		// Assets
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
			
			ExportGameObject(currentGameObject, copyMaterials, copyTextures);
		}

		/// <summary>
		/// Exports ANY Game Object given to it. Will provide a dialog and return the path of the newly exported file
		/// </summary>
		/// <returns>The path of the newly exported FBX file</returns>
		/// <param name="gameObj">Game object to be exported</param>
		/// <param name="copyMaterials">If set to <c>true</c> copy materials.</param>
		/// <param name="copyTextures">If set to <c>true</c> copy textures.</param>
		/// <param name="oldPath">Old path.</param>
		public static string ExportGameObject(GameObject gameObj, bool copyMaterials, bool copyTextures, string oldPath = null)
		{
			if(gameObj == null)
			{
				EditorUtility.DisplayDialog("Object is null", "Please select any GameObject to Export to FBX", "Okay");
				return null;
			}
			
			string newPath = GetNewPath(gameObj, oldPath);
			
			if(newPath != null && newPath.Length != 0)
			{
				bool isSuccess = FBXExporter.ExportGameObjToFBX(gameObj, newPath, copyMaterials, copyTextures);
				
				if(isSuccess)
				{
					return newPath;
				}
				else
					EditorUtility.DisplayDialog("Warning", "The extension probably wasn't an FBX file, could not export.", "Okay");
			}
			return null;
		}
		
		/// <summary>
		/// Creates save dialog window depending on old path or right to the /Assets folder no old path is given
		/// </summary>
		/// <returns>The new path.</returns>
		/// <param name="gameObject">Item to be exported</param>
		/// <param name="oldPath">The old path that this object was original at.</param>
		private static string GetNewPath(GameObject gameObject, string oldPath = null)
		{
			// NOTE: This must return a path with the starting "Assets/" or else textures won't copy right
			
			string name = gameObject.name;
			
			string newPath = null;
			if(oldPath == null)
				newPath = EditorUtility.SaveFilePanelInProject("Export FBX File", name + ".fbx", "fbx", "Export " + name + " GameObject to a FBX file");
			else
			{
				if(oldPath.StartsWith("/Assets"))
				{
					oldPath = Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets"), 7) + oldPath;
					oldPath = oldPath.Remove(oldPath.LastIndexOf('/'), oldPath.Length - oldPath.LastIndexOf('/'));
				}
				newPath = EditorUtility.SaveFilePanel("Export FBX File", oldPath, name + ".fbx", "fbx");
			}
			
			int assetsIndex = newPath.IndexOf("Assets");
			
			if(assetsIndex < 0)
				return null;
			
			if(assetsIndex > 0)
				newPath = newPath.Remove(0, assetsIndex);
			
			return newPath;
		}
	}
}