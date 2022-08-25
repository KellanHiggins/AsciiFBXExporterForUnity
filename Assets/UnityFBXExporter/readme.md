Unity FBX Exporter (1.4.0)
-------------------------

GitHub: https://github.com/KellanHiggins/UnityFBXExporter
Contact: unityfbxexporter@8bitgoose.com

The Unity FBX Exporter is a simple FBX writer designed to export static objects from Unity into the FBX format, preserving the materials, game object hierarchy and textures attached.

It was written for the Unity asset Building Crafter (http://u3d.as/ovC) which allows anyone using Unity to create buildings right in Unity without any modeling experience.

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


Editor Known Limitations
-------------------------

1. Minimum Tested Unity Version is 2021.3.36 (LTS).

2. FBX format will only recognize diffuse maps and normal maps when exporting. Can not include height maps, for example.

3. Textures only support PBR Unity 5 shaders, no URP or HDRP.

4. Sometimes the reimported FBX files don't find the right materials.

5. Exporting a prefab in the Project Tab sort of works. Needs more testing

6. This is not designed to export Skinned Mesh Renderers properly. It will export a Skinned Mesh into the form that it is currently in in (like a statue). Armatures won't be included.

7. Can't export with embedded media, this is a huge pain and I have no idea how the FBX format stores PNGs in their files.

8. FBX materials and Unity materials don't have a one-to-one relationship, so not much info comes from the base FBX materials.


Runtime Known Limitations
-------------------------

1. Runtime Exported Objects only include the Main Albedo Textures and the Normal Map. FBX won't store most of the extra information Unity can provide. If you know how to fix this, please let me know.

2. Runtime needs read/write texture's enabled in the editor.


Tutorial (editor)
-------------------------

It is very simple to use this exporter. You shouldn't have any problems, and if you do, please add an issue to the Github project

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

This was created without owning Maya, so please give feedback on how opening these FBX files work. You can give feedback at unityfbxexporter@8bitgoose.com.


Export to Blender
------------------------

Blender 2.70 doesn't take ASCII FBX files. So you'll need to download the converter from the FBX site. Then convert it to a binary file and then import it into blender. Because the relative texture names are correct, blender will import your albedo and normal texture. Pretty neat!

Known Issues
------------------------

1. Skinned mesh renderers may or may not be exporting materials correctly. Did not work on my test object.
2. Standard objects throw an error if you try and export them (like the sphere). Still seems to export fine. Also why are you exporting Unity's sphere???

Crediting This Project
------------------------

As a note, this project is an MIT license. Which means you can take this code, upload it to the Unity Asset store and charge money for it. BUT, you must include the license (including the bit about Building Crafter) in your project. If you have any questions about this, hit me up.

If you compact it into a DLL and hide all the code away, you still have to include the license somewhere. I'd much rather you come to me then find out you've taken in 4 months later. Don't be a dick, give back to the community!


Change log
------------------------

Version 1.4.0

1. Added UV2 export

2. Added Runtime export (Albedo and Normal Map only, this is a restriction of FBX)

3. Added message after successful export

4. Added generalized material texture getters

5. Released for free on the Asset Store

Version 1.3.0

1. Removed some extra procedural test code.

2. Added some null checks

3. Added check for countries which use commas for decimal points instead of periods. 

Version 1.2.0

1. Thanks to @tetreum for fixing my bad tutorial.

2. Also thanks to @tetreum for preventing copying non existent items

3. Thanks to @cartzhang for adding skinned mesh render exporter

4. Thanks to @mikelortega for fixing crash from trying to export materials that are procedurally generated

5. Thanks to @Totchinuko for adding awesome vertex colour export

6. Added an export option from the GameObject menu

7. And thanks to @MadmenAlliance for putting this up on the asset store without proper attribution which forced me to get my ass in gear and pull in all these changes

Version 1.1.1

1. Hotfix for something.

Version 1.1.0

1. Thanks to @quickfingerz for fixing the rotation export so it looks right in the FBX file.

Versin 1.0.0

1. Initial release











