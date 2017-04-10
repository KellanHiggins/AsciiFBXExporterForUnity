using UnityEngine;
using System.Collections;
using UnityEditor;

public class ProceduralTest : MonoBehaviour 
{

	[MenuItem("Assets/FBX Exporter/Create Object With Procedural Texture", false, 43)]
	public static void CreateObject()
	{
		GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		Texture2D texture = new Texture2D(128, 128);
		for (int x = 0; x < 128; ++x)
			for (int y = 0; y < 128; ++y)
				texture.SetPixel(x, y, (x-64)*(x-64) + (y-64)*(y-64) < 1000 ? Color.white : Color.black);
		texture.Apply();
		Material mat = new Material(Shader.Find("Standard"));
		mat.mainTexture = texture;
		cube.GetComponent<MeshRenderer>().sharedMaterial = mat;
	}
}
