ASCII FBX Exporter for Unity (2.0.2)
-------------------------

GitHub: https://github.com/KellanHiggins/AsciiFBXExporterForUnity
Contact: asciifbxexporter@8bitgoose.com or unityfbxexporter@8bitgoose.com, 

The ASCII FBX Exporter for Unity (formerly Unity FBX Exporter) is a simple FBX writer designed to export static objects from Unity into the Autodesk FBX format in their ASCII format (not binary), preserving the materials, game object hierarchy and textures attached.

It was written for the Unity asset Building Crafter (http://u3d.as/ovC) which allows anyone using Unity to create buildings right within the Unity editor without any modeling experience.

![Left is Unity](/Docs/ExampleExport.jpg?raw=true "Optional Title")


Features
-------------------------

1. Can export any GameObject into FBX format.

2. Supports FBX format 7.3, around 2013.

3. Exports materials into the FBX file.

4. Exports texture references into the FBX file.

5. Can make a copy of all materials and link them to newly minted FBX file.

6. Can make a copy of all textures and link them to newly create materials.

7. Export very deeply hierarchical Game Objects with just a few clicks.

8. Objects will export their rotation correctly throughout the hierarchy (thanks @quickfingerz on Github).

9. Objects will also export their scale correctly (thanks @quickfingerz on Github).

10. Textures can be exported at runtime (thanks to @Harti177 on Github)

11. Objects can be exported at runtime, (thank you Liam from addreality.co.uk).

12. There is no UI to determine runtime save location, you will need to write this yourself.

12. Objects will export their UV2 maps, (thank you @Ymiku on GitHub).

13. All shader texture materials are successfully extracted and written regardless of the shader (thank you @andysdds on GitHub)


Internationalization
-------------------------

IMPORTANT: If you use commas to denote decimals in your country, you must uncomment `#define International` in FBXExporter.cs. This will make sure objects are exported correctly for your computer.


Editor Known Limitations
-------------------------

1. Minimum Tested Unity Version is 2021.3.44f (LTS).

2. FBX format will only recognize diffuse maps and normal maps when exporting. Can not include height maps, for example.

3. Textures only support PBR Unity 5 shaders, no URP or HDRP.

4. Sometimes the reimported FBX files don't find the right materials.

5. Exporting a prefab in the project tab sort of works. Needs more testing

6. This is not designed to export Skinned Mesh Renderers properly. It will export a Skinned Mesh into the form that it is currently in in (like a statue). Armatures won't be included.

7. Can't export with embedded media, this is a huge pain and I have no idea how the FBX format stores PNGs in their files.

8. FBX materials and Unity materials don't have a one-to-one relationship, so not much info comes from the base FBX materials.

9. Standard objects throw an error if you try and export them (like the sphere). Still seems to export fine. Also why are you exporting Unity's sphere???


Runtime Known Limitations
-------------------------

1. Runtime Exported Objects only include the Main Albedo Textures and the Normal Map. FBX won't store most of the extra information Unity can provide. If you know how to fix this, please let me know.

2. Runtime needs read/write texture's enabled in the editor.

3. Can not export static meshes since they have been written to a big group and are readonly.


Blender Known Limitations
------------------------

1. Blender 2.70 doesn't take ASCII FBX files. So you'll need to download the converter from the FBX site. Then convert it to a binary file and then import it into blender. Because the relative texture names are correct, blender will import your albedo and normal texture. Pretty neat!


Tutorial (editor)
-------------------------

It is very simple to use this exporter. You shouldn't have any problems, and if you do, please add an issue to the Github project or send an email

1. Select any GameObject in the scene.

2. Select the type of export you'd like.

3. Go to Assets menu -> FBX Exporter -> and you have three options (described below).

3. "Only GameObject" will export a new FBX but not create any new materials or textures and use the original as reference

4. "With new Materials" will export a new FBX and create new materials with the GameObject name + _ + material name

5. "With new Materials and Textures" does 2b plus copying textures to a new folder. This one takes a while. Be patient.

6. Wait a bit

7. If you've selected new textures, wait a bit longer. Copying and reimporting the textures takes Unity's brainpower.

8. Check the folder.

9. Usually the materials will align, but if you have a FBX in the root area it may create new materials instead of finding the old ones.

10. Success! You now have a brand new FBX file with everything parented correctly. Remember rotations in children still don't work.

NOTE: Sometimes the fbx file imports the materials as recursive instead of project wide for GameObject only export. If this happens, delete all the materials and reimport the FBX file using Project-Wide for material search.


Tutorial (runtime)
-------------------------

1. Create an empty game object, put RuntimeExporterMono onto the game object

2. Drag the gameobject you want to export at runtime

3. Run scene, hit play and click on the button

4. The object will be exported to the provided Relative Folder path compared to Application.dataPath

5. NOTE -> this freezes the whole program when exporting and it just meant to be an example of exporting at runtime. Best to write your own using the example.


Maya
------------------------

This was created without owning Maya, so please give feedback on how opening these FBX files work. You can give feedback at asciifbxexporter@8bitgoose.com.


Crediting This Project
------------------------

As a note, this project is an MIT license. Which means you can take this code, upload it to the Unity Asset store and charge money for it. BUT, you must include the license (including the bit about Building Crafter) in your project. If you have any questions about this, hit me up.

If you compact it into a DLL and hide all the code away, you still have to include the license somewhere. I'd much rather you come to me then find out you've taken in 4 months later. Don't be a dick, give back to the community!