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

using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using FE = AsciiFBXExporter.FBXExporter;

namespace AsciiFBXExporter
{
	public class FBXUnityMaterialGetter
	{
		/// <summary> Class to keep track of the textures that are being written </summary>
		private class TextureReference
		{
			/// <summary> Class reference to the texture </summary>
			public Texture Texture;

			/// <summary> Id used in the FBX formatted file </summary>
			public long TextureId;

			/// <summary> Relative texture path compared to the FBX file location </summary>
			public string TextureRelativePath;

			/// <summary> Absolute path to the main drive that this texture is on </summary>
			public string TextureAbsolutePath;
		}

		/// <summary>
		/// Finds all materials in a gameobject and writes them to a string that can be read by the FBX writer
		/// </summary>
		/// <param name="gameObj">Parent GameObject being exported.</param>
		/// <param name="newPath">The path to export to.</param>
		/// <param name="materials">Materials which were written to this fbx file.</param>
		/// <param name="matObjects">The material objects to write to the file.</param>
		/// <param name="connections">The connections to write to the file.</param>
		public static void GetAllMaterialsToString(GameObject gameObj, 
												  	string newPath, 
													bool copyMaterials,
													bool copyTextures,
													out Material[] materials,
													out string matObjects,
													out string connections,
													string relativeTexturesPath = "Textures/")
		{
			StringBuilder tempObjectSb = new StringBuilder();
			StringBuilder tempConnectionsSb = new StringBuilder();

            // Need to get all unique materials for the submesh here and then write them in
            //@cartzhang modify.As meshrender and skinnedrender is same level in inherit relation shape.
            // if not check,skinned render ,may lost some materials.
            Renderer[] meshRenders = gameObj.GetComponentsInChildren<Renderer>();

			List<Material> uniqueMaterials = new List<Material>();

			// Gets all the unique materials within this GameObject Hierarchy
			for(int i = 0; i < meshRenders.Length; i++)
			{
				for(int n = 0; n < meshRenders[i].sharedMaterials.Length; n++)
				{
					Material mat = meshRenders[i].sharedMaterials[n];
					
					if(uniqueMaterials.Contains(mat) == false && mat != null)
					{
						uniqueMaterials.Add(mat);
					}
				}
			}

			// a list of textures that are created or linked from writing the textures to disk or grabbing them in the asset database
			List<TextureReference> textureReferences = new List<TextureReference>(uniqueMaterials.Count * 2);

            for (int i = 0; i < uniqueMaterials.Count; i++)
			{
				Material mat = uniqueMaterials[i];

				// We rename the material if it is being copied
				string materialName = mat.name;
				if(copyMaterials)
					materialName = gameObj.name + "_" + mat.name;

				int referenceId = Mathf.Abs(mat.GetInstanceID());

				tempObjectSb.AppendLine();
				tempObjectSb.AppendLine("\tMaterial: " + referenceId + ", \"Material::" + materialName + "\", \"\" {");
				tempObjectSb.AppendLine("\t\tVersion: 102");
				tempObjectSb.AppendLine("\t\tShadingModel: \"phong\"");
				tempObjectSb.AppendLine("\t\tMultiLayer: 0");
				tempObjectSb.AppendLine("\t\tProperties70:  {");
				tempObjectSb.AppendFormat("\t\t\tP: \"Diffuse\", \"Vector3D\", \"Vector\", \"\",{0},{1},{2}", FE.FBXFormat(mat.color.r), FE.FBXFormat(mat.color.g), FE.FBXFormat(mat.color.b));
				tempObjectSb.AppendLine();
				tempObjectSb.AppendFormat("\t\t\tP: \"DiffuseColor\", \"Color\", \"\", \"A\",{0},{1},{2}", FE.FBXFormat(mat.color.r), FE.FBXFormat(mat.color.g), FE.FBXFormat(mat.color.b));
				tempObjectSb.AppendLine();

				// TODO: Figure out if this property can be written to the FBX file
				//if(mat.HasProperty("_MetallicGlossMap"))
				//{
				//	Debug.Log("has metallic gloss map");
				//	Color color = mat.GetColor("_Color");
				//	tempObjectSb.AppendFormat("\t\t\tP: \"Specular\", \"Vector3D\", \"Vector\", \"\",{0},{1},{2}", color.r, color.g, color.r);
				//	tempObjectSb.AppendLine();
				//	tempObjectSb.AppendFormat("\t\t\tP: \"SpecularColor\", \"ColorRGB\", \"Color\", \" \",{0},{1},{2}", color.r, color.g, color.b);
				//	tempObjectSb.AppendLine();
				//}

				if(mat.HasProperty("_SpecColor"))
				{
					Color color = mat.GetColor("_SpecColor");
					tempObjectSb.AppendFormat("\t\t\tP: \"Specular\", \"Vector3D\", \"Vector\", \"\",{0},{1},{2}", FE.FBXFormat(color.r), FE.FBXFormat(color.g), FE.FBXFormat(color.r));
					tempObjectSb.AppendLine();
					tempObjectSb.AppendFormat("\t\t\tP: \"SpecularColor\", \"ColorRGB\", \"Color\", \" \",{0},{1},{2}", FE.FBXFormat(color.r), FE.FBXFormat(color.g), FE.FBXFormat(color.b));
					tempObjectSb.AppendLine();
				}

				if(mat.HasProperty("_Mode"))
				{
					Color color = Color.white;

                    switch ((int)mat.GetFloat("_Mode"))
                    {
                        case 0: // Map is opaque

                            break;

                        case 1: // Map is a cutout
                                //  TODO: Add option if it is a cutout
                            break;

                        case 2: // Map is a fade
                            color = mat.GetColor("_Color");

                            tempObjectSb.AppendFormat("\t\t\tP: \"TransparentColor\", \"Color\", \"\", \"A\",{0},{1},{2}", FE.FBXFormat(color.r), FE.FBXFormat(color.g), FE.FBXFormat(color.b));
                            tempObjectSb.AppendLine();
                            tempObjectSb.AppendFormat("\t\t\tP: \"Opacity\", \"double\", \"Number\", \"\",{0}", FE.FBXFormat(color.a));
                            tempObjectSb.AppendLine();
                            break;

                        case 3: // Map is transparent
                            color = mat.GetColor("_Color");

                            tempObjectSb.AppendFormat("\t\t\tP: \"TransparentColor\", \"Color\", \"\", \"A\",{0},{1},{2}", FE.FBXFormat(color.r), FE.FBXFormat(color.g), FE.FBXFormat(color.b));
                            tempObjectSb.AppendLine();
                            tempObjectSb.AppendFormat("\t\t\tP: \"Opacity\", \"double\", \"Number\", \"\",{0}", FE.FBXFormat(color.a));
                            tempObjectSb.AppendLine();
                            break;
                    }
                }

				// NOTE: Unity doesn't currently import this information (I think) from an FBX file.
				if(mat.HasProperty("_EmissionColor"))
				{
					Color color = mat.GetColor("_EmissionColor");

					tempObjectSb.AppendFormat("\t\t\tP: \"Emissive\", \"Vector3D\", \"Vector\", \"\",{0},{1},{2}", FE.FBXFormat(color.r), FE.FBXFormat(color.g), FE.FBXFormat(color.b));
					tempObjectSb.AppendLine();

					float averageColor = (color.r + color.g + color.b) / 3f;

					tempObjectSb.AppendFormat("\t\t\tP: \"EmissiveFactor\", \"Number\", \"\", \"A\",{0}", FE.FBXFormat(averageColor));
					tempObjectSb.AppendLine();
				}

				// TODO: Add these to the file based on their relation to the PBR files
				//tempObjectSb.AppendLine("\t\t\tP: \"AmbientColor\", \"Color\", \"\", \"A\",0,0,0");
				//tempObjectSb.AppendLine("\t\t\tP: \"ShininessExponent\", \"Number\", \"\", \"A\",6.31179285049438");
				//tempObjectSb.AppendLine("\t\t\tP: \"Ambient\", \"Vector3D\", \"Vector\", \"\",0,0,0");
				//tempObjectSb.AppendLine("\t\t\tP: \"Shininess\", \"double\", \"Number\", \"\",6.31179285049438");
				//tempObjectSb.AppendLine("\t\t\tP: \"Reflectivity\", \"double\", \"Number\", \"\",0");

				tempObjectSb.AppendLine("\t\t}");
				tempObjectSb.AppendLine("\t}");

				string textureObjects;
				string textureConnections;

				SerializedTextures(gameObj, newPath, mat, materialName, copyTextures, ref textureReferences, out textureObjects, out textureConnections, relativeTexturesPath);

				tempObjectSb.Append(textureObjects);
				tempConnectionsSb.Append(textureConnections);
			}

			materials = uniqueMaterials.ToArray<Material>();

			matObjects = tempObjectSb.ToString();
			connections = tempConnectionsSb.ToString();
		}

		/// <summary>
		/// Serializeds all the provided textures in this mesh and material.
		/// </summary>
		/// <param name="gameObj">The root game object</param>
		/// <param name="newPath">The texture path to write too</param>
		/// <param name="material">Material to export textures from</param>
		/// <param name="materialName">Name of the material to be looked at</param>
		/// <param name="copyTextures">If set to <c>true</c> copy textures</param>
		/// <param name="textureReferences">A list of textures so duplicate textures aren't copied</param>
		/// <param name="objects">String of the Objects to be written to the FBX file</param>
		/// <param name="connections">String of the Connections to be written to the FBX file</param>
		private static void SerializedTextures(GameObject gameObj,
												string newPath,
												Material material,
												string materialName,
												bool copyTextures,
												ref List<TextureReference> textureReferences,
												out string objects,
												out string connections,
												string relativeTexturesFolderName = "Textures/")
		{
			// TODO: FBX import currently only supports Diffuse Color and Normal Map
			// Because it is undocumented, there is no way to easily find out what other textures
			// can be attached to an FBX file so it is imported into the PBR shaders at the same time.

			if(material == null)
			{
				objects = "";
				connections = "";
				return;
			}

			StringBuilder objectsSb = new StringBuilder();
			StringBuilder connectionsSb = new StringBuilder();

			int materialId = Mathf.Abs(material.GetInstanceID());

			string newObjects;
			string newConnections;

			// attaches the main albdeo texture to the material
			if(SerializeOneTexture(newPath, "_MainTex", "DiffuseColor", copyTextures,
				material, materialName, materialId, ref textureReferences, out newObjects, out newConnections, relativeTexturesFolderName))
			{
				objectsSb.AppendLine(newObjects);
				connectionsSb.AppendLine(newConnections);
			}

			if(SerializeOneTexture(newPath, "_BumpMap", "NormalMap", copyTextures,
				material, materialName, materialId, ref textureReferences, out newObjects, out newConnections, relativeTexturesFolderName))
			{
				objectsSb.AppendLine(newObjects);
				connectionsSb.AppendLine(newConnections);
			}

			connections = connectionsSb.ToString();
			objects = objectsSb.ToString();
		}

		private static bool SerializeOneTexture(string absoluteNewPath,
												string materialTextureNameType,
												string textureFBXType,
												bool copyTextures,
												Material material,
												string materialName,
												int materialId,			
												ref List<TextureReference> textureReferences,
												out string objects,
												out string connections,
												string relativeTexturesFolderName = "Textures/")
		{
			// 1. Check to see if this texture has already been written
			Texture texture = material.GetTexture(materialTextureNameType);
			objects = "";
			connections = "";

			if(texture == null)
			{
				return false;
			}

			// HACK -> There has to be a better way to figure out if this texture is a normal map in the material
			bool isNormalMap = materialTextureNameType == "_BumpMap";

			if(relativeTexturesFolderName[relativeTexturesFolderName.Length - 1] != '/')
				relativeTexturesFolderName = relativeTexturesFolderName + "/";

			int textureIndex = -1;

			// now check the list to see if this texture has already been written at some point
			for(int i = 0, l = textureReferences.Count; i < l; i++)
			{
				if(textureReferences[i].Texture == texture)
					textureIndex = i;
			}

			TextureReference texReference = null;

			if(textureIndex == -1)
			{
#if UNITY_EDITOR
				string assetPath = AssetDatabase.GetAssetPath(texture);

				if(Application.isPlaying == false)
				{
					texReference = new TextureReference()
					{
						TextureId = FBXExporter.GetRandomFBXId(),
						Texture = texture,
						TextureAbsolutePath = FBXExporter.GetApplicationRoot() + assetPath,
					};
				}
#endif
				// note -> at runtime we always copy the textures because we can't get anything from the asset database
				if(Application.isPlaying)
				    copyTextures = true;

				if(copyTextures && Application.isPlaying == true)
				{
					// create the runtime texture
					string textureFullFileName;
					string textureName;

					string textureFolder = Path.GetDirectoryName(absoluteNewPath) + "/" + relativeTexturesFolderName;

					bool isTexCopied = SaveTextureRunTime((Texture2D)texture, textureFolder,
						out textureFullFileName, out textureName, isNormalMap);

					if(isTexCopied)
					{
						string relativePath = "/" + relativeTexturesFolderName + textureName;

						texReference = new TextureReference()
						{
							TextureId = FBXExporter.GetRandomFBXId(),
							Texture = texture,
							TextureAbsolutePath = textureFullFileName,
							TextureRelativePath = relativePath,
						};
					}
					else
					{
						Debug.LogError(textureName + " (a texture) was not written to the disk, the texture is probably not a read/write enabled texture");
						return false;
					}
				}

				if(texReference != null)
					textureReferences.Add(texReference);

			}
			else
			{
				texReference = textureReferences[textureIndex];
			}

			// cleans the texture file name to remove the extension and the path
			string textureFileName = texReference.TextureAbsolutePath.Remove(0, texReference.TextureAbsolutePath.LastIndexOf('/'));
			textureFileName = textureFileName.Remove(textureFileName.LastIndexOf('.'), textureFileName.Length - textureFileName.LastIndexOf('.'));

			StringBuilder objectsSb = new StringBuilder();
			StringBuilder connectionsSb = new StringBuilder();

			long textureReferenceId = texReference.TextureId;

			// TODO - test out different reference names to get one that doesn't load a _MainTex when importing.
			objectsSb.AppendLine("\tTexture: " + textureReferenceId + ", \"Texture::" + materialName + "\", \"\" {");
			objectsSb.AppendLine("\t\tType: \"TextureVideoClip\"");
			objectsSb.AppendLine("\t\tVersion: 202");
			objectsSb.AppendLine("\t\tTextureName: \"Texture::" + materialName + "\"");
			objectsSb.AppendLine("\t\tProperties70:  {");
			objectsSb.AppendLine("\t\t\tP: \"CurrentTextureBlendMode\", \"enum\", \"\", \"\",0");
			objectsSb.AppendLine("\t\t\tP: \"UVSet\", \"KString\", \"\", \"\", \"map1\"");
			objectsSb.AppendLine("\t\t\tP: \"UseMaterial\", \"bool\", \"\", \"\",1");
			objectsSb.AppendLine("\t\t}");
			objectsSb.AppendLine("\t\tMedia: \"Video::" + materialName + "\"");

			// Sets the absolute path for the copied texture
			objectsSb.Append("\t\tFileName: \"");
			objectsSb.Append(texReference.TextureAbsolutePath);
			objectsSb.AppendLine("\"");

			// Sets the relative path for the copied texture
			// TODO: If we don't copy the textures to a relative path, we must find a relative path to write down here
			if(copyTextures)
				objectsSb.AppendLine("\t\tRelativeFilename: \"" + texReference.TextureRelativePath + "\"");

			objectsSb.AppendLine("\t\tModelUVTranslation: 0,0"); // TODO: Figure out how to get the UV translation into here
			objectsSb.AppendLine("\t\tModelUVScaling: 1,1"); // TODO: Figure out how to get the UV scaling into here
			objectsSb.AppendLine("\t\tTexture_Alpha_Source: \"None\""); // TODO: Add alpha source here if the file is a cutout.
			objectsSb.AppendLine("\t\tCropping: 0,0,0,0");
			objectsSb.AppendLine("\t}");

			connectionsSb.AppendLine("\t;Texture::" + textureFileName + ", Material::" + materialName + "\"");
			connectionsSb.AppendLine("\tC: \"OP\"," + textureReferenceId + "," + materialId + ", \"" + textureFBXType + "\"");

			connectionsSb.AppendLine();

			objects = objectsSb.ToString();
			connections = connectionsSb.ToString();

			return true;
		}

		/// <summary>
		/// Saves the texture in PNG format at runtime
		/// </summary>
		/// <param name="texture">Texture exported</param>
		/// <param name="folderPath">The path to export to</param>
		private static bool SaveTextureRunTime(Texture2D texture,
												string folderPath,
												out string resultPathName,
												out string textureName,
												bool isNormal = false)
		{
			resultPathName = "";
			textureName = "";
			if (!texture.isReadable)
			{
				Debug.LogError(texture.name + " is not set to readable in the editor, please enable 'Read/Write' in the import settings");
				return false; 
			}

			Texture2D decompressedTexture = DecompressTexture(texture);

			if(isNormal)
				decompressedTexture = DTXnm2RGBA(decompressedTexture);

			byte[] bytes = decompressedTexture.EncodeToPNG();

			if(folderPath[folderPath.Length - 1] != '/')
				folderPath = folderPath + "/";

			if(Directory.Exists(folderPath) == false)
				Directory.CreateDirectory(folderPath);

			resultPathName = folderPath + texture.name + ".png";
			textureName = texture.name + ".png";

			// ensures we don't overwrite anything
			if(File.Exists(resultPathName))
			{
				string newId = FBXExporter.GetRandomIntId().ToString();
				resultPathName = resultPathName.Insert(resultPathName.LastIndexOf('.'), newId);
				textureName = textureName.Insert(textureName.LastIndexOf('.'), newId);
			}

			File.WriteAllBytes(resultPathName, bytes);

			return true;
		}

		/// <summary>
		/// Decompresses the texture at runtime
		/// </summary>
		/// <returns>Readable Texture</returns>
		/// <param name="source">Source Texture</param>
		public static Texture2D DecompressTexture(Texture2D source)
		{

			RenderTexture renderTex = RenderTexture.GetTemporary(
						source.width,
						source.height,
						0,
						RenderTextureFormat.Default,
						RenderTextureReadWrite.sRGB);

			Graphics.Blit(source, renderTex);
			RenderTexture previous = RenderTexture.active;
			RenderTexture.active = renderTex;
			Texture2D readableText = new Texture2D(source.width, source.height);
			readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
			readableText.Apply();
			RenderTexture.active = previous;
			RenderTexture.ReleaseTemporary(renderTex);
			return readableText;
		}

		/// <summary>
		/// Converts common normal map texture to RGBA
		/// </summary>
		/// <returns>RGBA version of the normal texture</returns>
		/// <param name="tex">Normal Texture in DTX format</param>
		private static Texture2D DTXnm2RGBA(Texture2D tex)
		{
			Color[] colors = tex.GetPixels();
			for(int i = 0; i < colors.Length; i++)
			{
				Color c = colors[i];
				c.r = c.a * 2 - 1;  //red<-alpha (x<-w)
				c.g = c.g * 2 - 1; //green is always the same (y)
				Vector2 xy = new Vector2(c.r, c.g); //this is the xy vector
				c.b = Mathf.Sqrt(1 - Mathf.Clamp01(Vector2.Dot(xy, xy))); //recalculate the blue channel (z)
				colors[i] = new Color(c.r * 0.5f + 0.5f, c.g * 0.5f + 0.5f, c.b * 0.5f + 0.5f); //back to 0-1 range
			}
			tex.SetPixels(colors); //apply pixels to the texture
			tex.Apply();
			return tex;
		}
	}
}