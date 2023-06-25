// ===============================================================================================
//  The MIT License (MIT) for AsciiFBXExporter
// 
//  ASCII FBX Exporter (formerly called the Unity FBX Exporter) was created for 
//  Building Crafter (http://u3d.as/ovC), a tool to rapidly create high quality 
//  buildings right in Unity with no need to use 3D modeling programs.
//
//  Copyright (c) 2016 - 2023 | 8Bit Goose Games, Inc.
//  	
//  Permission is hereby granted, free of charge, to any person obtaining a copy 
//  of this software and associated documentation files (the "Software"), to deal 
//  in the Software without restriction, including without limitation the rights 
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
//  of the Software, and to permit persons to whom the Software is furnished to do so, 
//  subject to the following conditions:
//  	
//  The above copyright notice and this permission notice shall be included in all 
//  copies or substantial portions of the Software.
//  	
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//  INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
//  PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
//  HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
//  OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
//  OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//  
// ===============================================================================================

//#define International // Uncomment if provides internationalization for countries that use commas instead of decimals to denote the break point

using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Linq;

namespace AsciiFBXExporter
{
	public class FBXExporter
	{
		public static string VersionInformation
		{
			get { return "Ascii FBX Exporter version 2.0.2 (Originally created for the Unity Asset, Building Crafter)"; }
		}

		public static bool ExportGameObjToFBX(GameObject gameObj, string newPath, bool copyMaterials = false, bool copyTextures = false)
		{
			return ExportGameObjToFBX(gameObj, newPath, copyMaterials, "/Materials/", copyTextures, "/Textures/");
		}

		public static bool ExportGameObjToFBX(GameObject gameObj,
											string newFileNameWithFullPath,
											bool copyMaterials,
											string relativeMaterialFolderName,
											bool copyTextures,
											string relativeTextureFolderName)
		{
			if(gameObj == null)
				return false;

			// Check to see if the extension is right
			if (Path.GetExtension(newFileNameWithFullPath).ToLower() != ".fbx")
			{
				Debug.LogError("The end of the path wasn't \".fbx\"");
				return false;
			}

            if(copyMaterials)
            {
                CopyComplexMaterialsToPath(gameObj, newFileNameWithFullPath, copyTextures, relativeMaterialFolderName, relativeTextureFolderName);
            }

			string buildMesh = MeshToString(gameObj, newFileNameWithFullPath, copyMaterials, copyTextures, relativeMaterialFolderName, relativeTextureFolderName);

			if(File.Exists(newFileNameWithFullPath))
				File.Delete(newFileNameWithFullPath);

			File.WriteAllText(newFileNameWithFullPath, buildMesh);

#if UNITY_EDITOR
			// Import the model properly so it looks for the material instead of by the texture name
			// TODO: By calling refresh, it imports the model with the wrong materials, but we can't find the model to import without
			// refreshing the database. A chicken and the egg issue
			AssetDatabase.Refresh();
			string stringLocalPath = newFileNameWithFullPath.Remove(0, newFileNameWithFullPath.LastIndexOf("/Assets") + 1);
			ModelImporter modelImporter = ModelImporter.GetAtPath(stringLocalPath) as ModelImporter;
			if(modelImporter != null)
			{
				ModelImporterMaterialName modelImportOld = modelImporter.materialName;
				modelImporter.materialName = ModelImporterMaterialName.BasedOnMaterialName;
                modelImporter.importNormals = ModelImporterNormals.Import;

                if(copyMaterials == false)
                    modelImporter.materialSearch = ModelImporterMaterialSearch.Everywhere;
                else
                {
                    modelImporter.materialSearch = ModelImporterMaterialSearch.RecursiveUp;
                    modelImporter.materialLocation = ModelImporterMaterialLocation.External;
                }
				
				AssetDatabase.ImportAsset(stringLocalPath, ImportAssetOptions.ForceUpdate);
			}
			else
			{
				Debug.Log("Model Importer is null and can't import");
			}

			AssetDatabase.Refresh();
#endif

			Debug.Log("Successfully exported game object [" + gameObj.name + "] to \"" + newFileNameWithFullPath + "\"");

            return true;
		}

		public static bool ExportGameObjAtRuntime(GameObject gameObject, string folder, string fbxName, string textureFolderName, bool deleteExisitingFolder = false)
		{
			string fullFileName = folder + fbxName;

			if(Path.GetExtension(fullFileName).ToLower() != ".fbx")
			{
				Debug.LogError("file name does not end in fbx, can't export");
				return false;
			}

			// ensure the texture folders are right
			if(textureFolderName.Length == 0)
				textureFolderName = "Textures/";

			if(textureFolderName[0] == '/')
				textureFolderName = textureFolderName.Remove(0, 1);

			if(textureFolderName[textureFolderName.Length - 1] != '/')
				textureFolderName = textureFolderName + "/";

			// delete the existing folder if wrong
			if(deleteExisitingFolder)
			{
				string fullTexturesPathName = folder;

				if(fullTexturesPathName[fullTexturesPathName.Length - 1] != '/')
					fullTexturesPathName += "/";

				fullTexturesPathName = fullTexturesPathName + textureFolderName;

				if(Directory.Exists(fullTexturesPathName))
				{
					Directory.Delete(fullTexturesPathName, true);
				}
			}

			return ExportGameObjToFBX(gameObject, fullFileName, false, "", true, textureFolderName);
		}

		public static bool ExportGameObjAtRuntime(GameObject gameObject, string newPath)
		{
			return ExportGameObjToFBX(gameObject, newPath, false, true);
		}

		public static long GetRandomFBXId()
		{
			return System.BitConverter.ToInt64(System.Guid.NewGuid().ToByteArray(), 0);
		}

		public static int GetRandomIntId()
		{
			return System.BitConverter.ToInt32(System.Guid.NewGuid().ToByteArray(), 0);
		}

		/// <summary>
		/// Receives a mesh, and converts it to an FBX formatted string
		/// </summary>
		/// <param name="gameObj">The root gameobject</param>
		/// <param name="newPath">The path to write to</param>
		/// <param name="copyMaterials">Will materials copy</param>
		/// <param name="copyTextures">Will textures copy</param>
		/// <param name="materialsFolder">The exported material's folder</param>
		/// <param name="texturesFolder">The exported texture's folder</param>
		/// <returns></returns>
		public static string MeshToString(GameObject gameObj, string newPath, bool copyMaterials = false, bool copyTextures = false, string materialsFolder = "Materials/", string texturesFolder = "Textures/")
        {
            StringBuilder sb = new StringBuilder();

            StringBuilder objectProps = new StringBuilder();
            objectProps.AppendLine("; Object properties");
            objectProps.AppendLine(";------------------------------------------------------------------");
            objectProps.AppendLine("");
            objectProps.AppendLine("Objects:  {");

            StringBuilder objectConnections = new StringBuilder();
            objectConnections.AppendLine("; Object connections");
            objectConnections.AppendLine(";------------------------------------------------------------------");
            objectConnections.AppendLine("");
            objectConnections.AppendLine("Connections:  {");
            objectConnections.AppendLine("\t");

            Material[] materials = new Material[0];

            // First finds all unique materials and compiles them (and writes to the object connections) for funzies
            string materialsObjectSerialized = "";
            string materialConnectionsSerialized = "";

            FBXUnityMaterialGetter.GetAllMaterialsToString(gameObj, newPath, copyMaterials, copyTextures, out materials, out materialsObjectSerialized, out materialConnectionsSerialized, texturesFolder);

            // Run recursive FBX Mesh grab over the entire gameobject
            FBXUnityMeshGetter.GetMeshToString(gameObj, materials, ref objectProps, ref objectConnections);

            // write the materials to the objectProps here. Should not do it in the above as it recursive.

            objectProps.Append(materialsObjectSerialized);
            objectConnections.Append(materialConnectionsSerialized);

            // Close up both builders;
            objectProps.AppendLine("}");
            objectConnections.AppendLine("}");

            // ========= Create header ========

            // Intro
            sb.AppendLine("; FBX 7.3.0 project file");
            sb.AppendLine("; Copyright (C) 1997-2010 Autodesk Inc. and/or its licensors.");
            sb.AppendLine("; All rights reserved.");
            sb.AppendLine("; ----------------------------------------------------");
            sb.AppendLine();

            // The header
            sb.AppendLine("FBXHeaderExtension:  {");
            sb.AppendLine("\tFBXHeaderVersion: 1003");
            sb.AppendLine("\tFBXVersion: 7300");

            // Creationg Date Stamp
            System.DateTime currentDate = System.DateTime.Now;
            sb.AppendLine("\tCreationTimeStamp:  {");
            sb.AppendLine("\t\tVersion: 1000");
            sb.AppendLine("\t\tYear: " + currentDate.Year);
            sb.AppendLine("\t\tMonth: " + currentDate.Month);
            sb.AppendLine("\t\tDay: " + currentDate.Day);
            sb.AppendLine("\t\tHour: " + currentDate.Hour);
            sb.AppendLine("\t\tMinute: " + currentDate.Minute);
            sb.AppendLine("\t\tSecond: " + currentDate.Second);
            sb.AppendLine("\t\tMillisecond: " + currentDate.Millisecond);
            sb.AppendLine("\t}");

            // Info on the Creator
            sb.AppendLine("\tCreator: \"" + VersionInformation + "\"");
            sb.AppendLine("\tSceneInfo: \"SceneInfo::GlobalInfo\", \"UserData\" {");
            sb.AppendLine("\t\tType: \"UserData\"");
            sb.AppendLine("\t\tVersion: 100");
            sb.AppendLine("\t\tMetaData:  {");
            sb.AppendLine("\t\t\tVersion: 100");
            sb.AppendLine("\t\t\tTitle: \"\"");
            sb.AppendLine("\t\t\tSubject: \"\"");
            sb.AppendLine("\t\t\tAuthor: \"\"");
            sb.AppendLine("\t\t\tKeywords: \"\"");
            sb.AppendLine("\t\t\tRevision: \"\"");
            sb.AppendLine("\t\t\tComment: \"\"");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t\tProperties70:  {");

			// Information on how this item was originally generated
			string documentInfoPaths = newPath;
            sb.AppendLine("\t\t\tP: \"DocumentUrl\", \"KString\", \"Url\", \"\", \"" + documentInfoPaths + "\"");
            sb.AppendLine("\t\t\tP: \"SrcDocumentUrl\", \"KString\", \"Url\", \"\", \"" + documentInfoPaths + "\"");
            sb.AppendLine("\t\t\tP: \"Original\", \"Compound\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"Original|ApplicationVendor\", \"KString\", \"\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"Original|ApplicationName\", \"KString\", \"\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"Original|ApplicationVersion\", \"KString\", \"\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"Original|DateTime_GMT\", \"DateTime\", \"\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"Original|FileName\", \"KString\", \"\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"LastSaved\", \"Compound\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"LastSaved|ApplicationVendor\", \"KString\", \"\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"LastSaved|ApplicationName\", \"KString\", \"\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"LastSaved|ApplicationVersion\", \"KString\", \"\", \"\", \"\"");
            sb.AppendLine("\t\t\tP: \"LastSaved|DateTime_GMT\", \"DateTime\", \"\", \"\", \"\"");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            // The Global information
            sb.AppendLine("GlobalSettings:  {");
            sb.AppendLine("\tVersion: 1000");
            sb.AppendLine("\tProperties70:  {");
            sb.AppendLine("\t\tP: \"UpAxis\", \"int\", \"Integer\", \"\",1");
            sb.AppendLine("\t\tP: \"UpAxisSign\", \"int\", \"Integer\", \"\",1");
            sb.AppendLine("\t\tP: \"FrontAxis\", \"int\", \"Integer\", \"\",2");
            sb.AppendLine("\t\tP: \"FrontAxisSign\", \"int\", \"Integer\", \"\",1");
            sb.AppendLine("\t\tP: \"CoordAxis\", \"int\", \"Integer\", \"\",0");
            sb.AppendLine("\t\tP: \"CoordAxisSign\", \"int\", \"Integer\", \"\",1");
            sb.AppendLine("\t\tP: \"OriginalUpAxis\", \"int\", \"Integer\", \"\",-1");
            sb.AppendLine("\t\tP: \"OriginalUpAxisSign\", \"int\", \"Integer\", \"\",1");
            sb.AppendLine("\t\tP: \"UnitScaleFactor\", \"double\", \"Number\", \"\",100"); // NOTE: This sets the resize scale upon import
            sb.AppendLine("\t\tP: \"OriginalUnitScaleFactor\", \"double\", \"Number\", \"\",100");
            sb.AppendLine("\t\tP: \"AmbientColor\", \"ColorRGB\", \"Color\", \"\",0,0,0");
            sb.AppendLine("\t\tP: \"DefaultCamera\", \"KString\", \"\", \"\", \"Producer Perspective\"");
            sb.AppendLine("\t\tP: \"TimeMode\", \"enum\", \"\", \"\",11");
            sb.AppendLine("\t\tP: \"TimeSpanStart\", \"KTime\", \"Time\", \"\",0");
            sb.AppendLine("\t\tP: \"TimeSpanStop\", \"KTime\", \"Time\", \"\",479181389250");
            sb.AppendLine("\t\tP: \"CustomFrameRate\", \"double\", \"Number\", \"\",-1");
            sb.AppendLine("\t}");
            sb.AppendLine("}");

            // The Object definations
            sb.AppendLine("; Object definitions");
            sb.AppendLine(";------------------------------------------------------------------");
            sb.AppendLine("");
            sb.AppendLine("Definitions:  {");
            sb.AppendLine("\tVersion: 100");
            sb.AppendLine("\tCount: 4");

            sb.AppendLine("\tObjectType: \"GlobalSettings\" {");
            sb.AppendLine("\t\tCount: 1");
            sb.AppendLine("\t}");

            sb.AppendLine("\tObjectType: \"Model\" {");
            sb.AppendLine("\t\tCount: 1"); // TODO figure out if this count matters
            sb.AppendLine("\t\tPropertyTemplate: \"FbxNode\" {");
            sb.AppendLine("\t\t\tProperties70:  {");
            sb.AppendLine("\t\t\t\tP: \"QuaternionInterpolate\", \"enum\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationOffset\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"RotationPivot\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"ScalingOffset\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"ScalingPivot\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"TranslationActive\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"TranslationMin\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"TranslationMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"TranslationMinX\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"TranslationMinY\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"TranslationMinZ\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"TranslationMaxX\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"TranslationMaxY\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"TranslationMaxZ\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationOrder\", \"enum\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationSpaceForLimitOnly\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationStiffnessX\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationStiffnessY\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationStiffnessZ\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"AxisLen\", \"double\", \"Number\", \"\",10");
            sb.AppendLine("\t\t\t\tP: \"PreRotation\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"PostRotation\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"RotationActive\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationMin\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"RotationMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"RotationMinX\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationMinY\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationMinZ\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationMaxX\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationMaxY\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"RotationMaxZ\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"InheritType\", \"enum\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"ScalingActive\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"ScalingMin\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"ScalingMax\", \"Vector3D\", \"Vector\", \"\",1,1,1");
            sb.AppendLine("\t\t\t\tP: \"ScalingMinX\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"ScalingMinY\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"ScalingMinZ\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"ScalingMaxX\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"ScalingMaxY\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"ScalingMaxZ\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"GeometricTranslation\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"GeometricRotation\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"GeometricScaling\", \"Vector3D\", \"Vector\", \"\",1,1,1");
            sb.AppendLine("\t\t\t\tP: \"MinDampRangeX\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MinDampRangeY\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MinDampRangeZ\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MaxDampRangeX\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MaxDampRangeY\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MaxDampRangeZ\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MinDampStrengthX\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MinDampStrengthY\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MinDampStrengthZ\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MaxDampStrengthX\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MaxDampStrengthY\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"MaxDampStrengthZ\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"PreferedAngleX\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"PreferedAngleY\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"PreferedAngleZ\", \"double\", \"Number\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"LookAtProperty\", \"object\", \"\", \"\"");
            sb.AppendLine("\t\t\t\tP: \"UpVectorProperty\", \"object\", \"\", \"\"");
            sb.AppendLine("\t\t\t\tP: \"Show\", \"bool\", \"\", \"\",1");
            sb.AppendLine("\t\t\t\tP: \"NegativePercentShapeSupport\", \"bool\", \"\", \"\",1");
            sb.AppendLine("\t\t\t\tP: \"DefaultAttributeIndex\", \"int\", \"Integer\", \"\",-1");
            sb.AppendLine("\t\t\t\tP: \"Freeze\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"LODBox\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"Lcl Translation\", \"Lcl Translation\", \"\", \"A\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"Lcl Rotation\", \"Lcl Rotation\", \"\", \"A\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"Lcl Scaling\", \"Lcl Scaling\", \"\", \"A\",1,1,1");
            sb.AppendLine("\t\t\t\tP: \"Visibility\", \"Visibility\", \"\", \"A\",1");
            sb.AppendLine("\t\t\t\tP: \"Visibility Inheritance\", \"Visibility Inheritance\", \"\", \"\",1");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");

            // The geometry, this is IMPORTANT
            sb.AppendLine("\tObjectType: \"Geometry\" {");
            sb.AppendLine("\t\tCount: 1"); // TODO - this must be set by the number of items being placed.
            sb.AppendLine("\t\tPropertyTemplate: \"FbxMesh\" {");
            sb.AppendLine("\t\t\tProperties70:  {");
            sb.AppendLine("\t\t\t\tP: \"Color\", \"ColorRGB\", \"Color\", \"\",0.8,0.8,0.8");
            sb.AppendLine("\t\t\t\tP: \"BBoxMin\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"BBoxMax\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"Primary Visibility\", \"bool\", \"\", \"\",1");
            sb.AppendLine("\t\t\t\tP: \"Casts Shadows\", \"bool\", \"\", \"\",1");
            sb.AppendLine("\t\t\t\tP: \"Receive Shadows\", \"bool\", \"\", \"\",1");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");

            // The materials that are being placed. Has to be simple I think
            sb.AppendLine("\tObjectType: \"Material\" {");
            sb.AppendLine("\t\tCount: 1");
            sb.AppendLine("\t\tPropertyTemplate: \"FbxSurfacePhong\" {");
            sb.AppendLine("\t\t\tProperties70:  {");
            sb.AppendLine("\t\t\t\tP: \"ShadingModel\", \"KString\", \"\", \"\", \"Phong\"");
            sb.AppendLine("\t\t\t\tP: \"MultiLayer\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"EmissiveColor\", \"Color\", \"\", \"A\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"EmissiveFactor\", \"Number\", \"\", \"A\",1");
            sb.AppendLine("\t\t\t\tP: \"AmbientColor\", \"Color\", \"\", \"A\",0.2,0.2,0.2");
            sb.AppendLine("\t\t\t\tP: \"AmbientFactor\", \"Number\", \"\", \"A\",1");
            sb.AppendLine("\t\t\t\tP: \"DiffuseColor\", \"Color\", \"\", \"A\",0.8,0.8,0.8");
            sb.AppendLine("\t\t\t\tP: \"DiffuseFactor\", \"Number\", \"\", \"A\",1");
            sb.AppendLine("\t\t\t\tP: \"Bump\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"NormalMap\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"BumpFactor\", \"double\", \"Number\", \"\",1");
            sb.AppendLine("\t\t\t\tP: \"TransparentColor\", \"Color\", \"\", \"A\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"TransparencyFactor\", \"Number\", \"\", \"A\",0");
            sb.AppendLine("\t\t\t\tP: \"DisplacementColor\", \"ColorRGB\", \"Color\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"DisplacementFactor\", \"double\", \"Number\", \"\",1");
            sb.AppendLine("\t\t\t\tP: \"VectorDisplacementColor\", \"ColorRGB\", \"Color\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"VectorDisplacementFactor\", \"double\", \"Number\", \"\",1");
            sb.AppendLine("\t\t\t\tP: \"SpecularColor\", \"Color\", \"\", \"A\",0.2,0.2,0.2");
            sb.AppendLine("\t\t\t\tP: \"SpecularFactor\", \"Number\", \"\", \"A\",1");
            sb.AppendLine("\t\t\t\tP: \"ShininessExponent\", \"Number\", \"\", \"A\",20");
            sb.AppendLine("\t\t\t\tP: \"ReflectionColor\", \"Color\", \"\", \"A\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"ReflectionFactor\", \"Number\", \"\", \"A\",1");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");

            // Explanation of how textures work
            sb.AppendLine("\tObjectType: \"Texture\" {");
            sb.AppendLine("\t\tCount: 2"); // TODO - figure out if this texture number is important
            sb.AppendLine("\t\tPropertyTemplate: \"FbxFileTexture\" {");
            sb.AppendLine("\t\t\tProperties70:  {");
            sb.AppendLine("\t\t\t\tP: \"TextureTypeUse\", \"enum\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"Texture alpha\", \"Number\", \"\", \"A\",1");
            sb.AppendLine("\t\t\t\tP: \"CurrentMappingType\", \"enum\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"WrapModeU\", \"enum\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"WrapModeV\", \"enum\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"UVSwap\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"PremultiplyAlpha\", \"bool\", \"\", \"\",1");
            sb.AppendLine("\t\t\t\tP: \"Translation\", \"Vector\", \"\", \"A\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"Rotation\", \"Vector\", \"\", \"A\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"Scaling\", \"Vector\", \"\", \"A\",1,1,1");
            sb.AppendLine("\t\t\t\tP: \"TextureRotationPivot\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"TextureScalingPivot\", \"Vector3D\", \"Vector\", \"\",0,0,0");
            sb.AppendLine("\t\t\t\tP: \"CurrentTextureBlendMode\", \"enum\", \"\", \"\",1");
            sb.AppendLine("\t\t\t\tP: \"UVSet\", \"KString\", \"\", \"\", \"default\"");
            sb.AppendLine("\t\t\t\tP: \"UseMaterial\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t\tP: \"UseMipMap\", \"bool\", \"\", \"\",0");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");

            sb.AppendLine("}");
            sb.AppendLine("");

            sb.Append(objectProps.ToString());
            sb.Append(objectConnections.ToString());

            return sb.ToString();
        }

		/// <summary>
		/// Copies all the materials of a parent game object to a newly created path
		/// </summary>
		/// <param name="gameObj">The root gameobject</param>
		/// <param name="path">The path to write to</param>
		/// <param name="copyTextures">Will textures also be copied</param>
		/// <param name="materialsFolder">The exported material's folder</param>
		/// <param name="texturesFolder">The exported texture's folder</param>
		public static void CopyComplexMaterialsToPath(GameObject gameObj, string path, bool copyTextures, string materialsFolder = "/Materials", string texturesFolder = "/Textures")
		{
#if UNITY_EDITOR
			int folderIndex = path.LastIndexOf('/');
			path = path.Remove(folderIndex, path.Length - folderIndex);

			// 1. First create the directories that are needed
			string texturesPath = path + texturesFolder;
			string materialsPath = path + materialsFolder;
			
			if(Directory.Exists(path) == false)
				Directory.CreateDirectory(path);
			if(Directory.Exists(materialsPath) == false)
				Directory.CreateDirectory(materialsPath);

            // 2. Copy every distinct Material into the Materials folder
            // @cartzhang modify. As meshrender and skinnedrender is same level in inherit relation shape.
            // if not check, skinned render, may lost some materials.
            Renderer[] meshRenderers = gameObj.GetComponentsInChildren<Renderer>();
			List<Material> everyMaterial = new List<Material>();
			for(int i = 0; i < meshRenderers.Length; i++)
			{
				for(int n = 0; n < meshRenderers[i].sharedMaterials.Length; n++)
				{
					everyMaterial.Add(meshRenderers[i].sharedMaterials[n]);
				}
			}

            Material[] everyDistinctMaterial = everyMaterial.Distinct().ToArray<Material>();
			everyDistinctMaterial = everyDistinctMaterial.OrderBy(o => o.name).ToArray<Material>();

			// Log warning if there are multiple assets with the same name
			for(int i = 0; i < everyDistinctMaterial.Length; i++)
			{
				for(int n = 0; n < everyDistinctMaterial.Length; n++)
				{
					if(i == n)
						continue;

					if(everyDistinctMaterial[i].name == everyDistinctMaterial[n].name)
					{
						Debug.LogErrorFormat("Two distinct materials {0} and {1} have the same name, this will not work with the FBX Exporter", everyDistinctMaterial[i], everyDistinctMaterial[n]);
						return;
					}
				}
			}

			List<string> everyMaterialName = new List<string>();
			// Structure of materials naming, is used when packaging up the package
			// PARENTNAME_ORIGINALMATNAME.mat
			for(int i = 0; i < everyDistinctMaterial.Length; i++)
			{
				string newName = gameObj.name + "_" + everyDistinctMaterial[i].name;
				string fullPath = materialsPath + "/" + newName + ".mat";

				if(File.Exists(fullPath))
					File.Delete(fullPath);

                if(CopyAndRenameAsset(everyDistinctMaterial[i], newName, materialsPath))
                {
                    //Debug.Log("Copied and renamed asset: " + newName);
                    everyMaterialName.Add(newName);
                }
			}

			// 3. Go through newly moved materials and copy every texture and update the material
			AssetDatabase.Refresh();

			List<Material> allNewMaterials = new List<Material>();

			for (int i = 0; i < everyMaterialName.Count; i++) 
			{
				string assetPath = materialsPath;
				if(assetPath[assetPath.Length - 1] != '/')
					assetPath += "/";

				assetPath += everyMaterialName[i] + ".mat";

				Material sourceMat = (Material)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Material));

				if(sourceMat != null)
					allNewMaterials.Add(sourceMat);
			}

			// Get all the textures from the mesh renderer

			if(copyTextures)
			{
				if(Directory.Exists(texturesPath) == false)
					Directory.CreateDirectory(texturesPath);

				AssetDatabase.Refresh();

				for(int i = 0; i < allNewMaterials.Count; i++)
				{
					allNewMaterials[i] = CopyTexturesAndAssignCopiesToMaterial(allNewMaterials[i], texturesPath);
				}
			}

			AssetDatabase.Refresh();
#endif
		}

		/// <summary>
		/// Takes ab object, renames it and copies it to the new location
		/// </summary>
		/// <param name="obj">Object to copy</param>
		/// <param name="newName">New name to give to this</param>
		/// <param name="newFolderPath">The folder to be copied to</param>
		/// <returns></returns>
		public static bool CopyAndRenameAsset(Object obj, string newName, string newFolderPath)
		{
#if UNITY_EDITOR
			string path = newFolderPath;
			
			if(path[path.Length - 1] != '/')
				path += "/";

			//string testPath = path.Remove(path.Length - 1);
			//if(AssetDatabase.IsValidFolder(testPath) == false)
			//{
			//	Debug.LogError("This folder does not exist " + testPath);
			//	return false;
			//}

			string assetPath = AssetDatabase.GetAssetPath(obj);
			string extension = Path.GetExtension(assetPath);

			string newFileName = path + newName + extension;

			if(File.Exists(newFileName))
				return false;

            return AssetDatabase.CopyAsset(assetPath, newFileName);
#else
			return false;
#endif
		}

		/// <summary>
		/// Strips the full path of a file
		/// </summary>
		/// <returns>The file name.</returns>
		/// <param name="path">Path.</param>
		private static string GetFileName(string path)
		{
			string fileName = path.ToString();
			fileName = fileName.Remove(0, fileName.LastIndexOf('/') + 1);
			return fileName;
		}

		/// <summary>
		/// Copies a material to to a new string
		/// </summary>
		/// <param name="material">Material to copy</param>
		/// <param name="newPath">Path including the file name to copy to</param>
		/// <returns>Material that has been copied and created</returns>
		private static Material CopyTexturesAndAssignCopiesToMaterial(Material material, string newPath)
		{
			// 1. Get all the textures that may be used in this material
			string[] textureNames = material.GetTexturePropertyNames();

			// 2. Get references for each texture in the whole list
			Texture[] allTextures = new Texture[textureNames.Length];

			int textureCount = 0;
			for(int i = 0, l = textureNames.Length; i < l; i++)
			{
				Texture texture = material.GetTexture(textureNames[i]);

				if(texture != null)
				{
					allTextures[i] = texture;
					textureCount++;
				}
			}

			// 3. Get unique textures out of this
			List<Texture> uniqueTextures = new List<Texture>(textureCount);
			for(int i = 0, l = allTextures.Length; i < l; i++)
			{
				if(allTextures[i] != null && uniqueTextures.Contains(allTextures[i]) == false)
					uniqueTextures.Add(allTextures[i]);
			}

			// 4. Save a copy of each texture
			List<Texture> duplicatedTextures = new List<Texture>(uniqueTextures.Count);
			for(int i = 0, l = uniqueTextures.Count; i < l; i++)
				duplicatedTextures.Add(CopyAndRenameAssetReturnObject(uniqueTextures[i], material.name + uniqueTextures[i].name, newPath) as Texture);

			// 5. Write the textures back to the material so there are no duplicated saved textures
			for(int textureIndex = 0, textureLength = allTextures.Length; textureIndex < textureLength; textureIndex++)
			{	
				Texture texture = allTextures[textureIndex];
				if(texture != null)
				{
					for(int i = 0, l = uniqueTextures.Count; i < l; i++)
					{
						if(uniqueTextures[i] == texture)
							material.SetTexture(textureNames[textureIndex], duplicatedTextures[i]);
					}
				}
			}

			return material;
		}

		public static Object CopyAndRenameAssetReturnObject(Object obj, string newName, string newFolderPath)
		{
#if UNITY_EDITOR
			string path = newFolderPath;
			
			if(path[path.Length - 1] != '/')
				path += "/";
			string testPath = path.Remove(path.Length - 1);
			
			if(Directory.Exists(testPath) == false)
			{
				Debug.LogError("This folder does not exist " + testPath);
				return null;
			}
			
			string assetPath =  AssetDatabase.GetAssetPath(obj);
			string fileName = GetFileName(assetPath);
			string extension = fileName.Remove(0, fileName.LastIndexOf('.'));
			
			string newFullPathName = path + newName + extension;
			
			if(AssetDatabase.CopyAsset(assetPath, newFullPathName) == false)
				return null;
			
			AssetDatabase.Refresh();
			
			return AssetDatabase.LoadAssetAtPath(newFullPathName, typeof(Texture));
#else
			return null;
#endif
		}

		/// <summary>
		/// Gets the root path of the Unity folder
		/// </summary>
		/// <returns>Application.dataPath without Assets on the end</returns>
		public static string GetApplicationRoot()
		{
			return Application.dataPath.Remove(Application.dataPath.LastIndexOf("Assets"), 6);
		}

		/// <summary>
		/// Provides internationalization for countries that use commas instead of decimals to denote the break point
		/// </summary>
		/// <param name="val">the float value you wish to convert</param>
		/// <returns>a string that is formated always to be 1.0 and never 1,0</returns>
		public static string FBXFormat(float val)
		{
#if !International
			return val.ToString();
#else
			string stringValue = val.ToString();

			int index = CheckForCommaInsteadOfDecimal(ref stringValue);

			if(index > -1)
				stringValue = ReplaceCommasWithDecimals(stringValue, index);

			return stringValue;
#endif
		}

		/// <summary>
		/// Returns a positive value if the string has a comma in it
		/// </summary>
		private static int CheckForCommaInsteadOfDecimal(ref string vert)
		{
			int newIndex = -1;

			for(int i = 0, l = vert.Length; i < l; i++)
			{
				if(vert[i] == ',')
					return i;
			}

			return newIndex;
		}

		private static string ReplaceCommasWithDecimals(string vert, int breakIndex)
		{
			return vert.Remove(breakIndex, vert.Length - breakIndex) + "." + vert.Remove(0, breakIndex + 1);
		}
	}
}
