# One Click to Build (OCB) :

![Revit API](https://img.shields.io/badge/Revit%20API-2021-blue.svg)
![.NET](https://img.shields.io/badge/.NET-4.8-blue.svg)

Revit plugin to generate a 3D BIM Model from 2D CAD Drawings.

## How to use:-
Test the plugin by Revit 2021 with all default family libraries installed. The default folder for Revit Add-in is `C:\ProgramData\Autodesk\Revit\Addins\2021\` , or `C:\Users\$username$\AppData\Roaming\Autodesk\Revit\Addins\2021\`.  
The build events of the Visual Studio project will copy all necessary files to that directory after you build the source code.

Then start Revit ([Plugin Demo](https://drive.google.com/drive/folders/1jsKybGRJiD8AFDBmYWJyiuT_sN-aej5Q)):  
- Insert -> Link CAD -> `*.dwg`
- OCB -> Settings (check if all .rfa files are loaded)
- OCB -> Settings (Pick the element from DWG to detect its layer name)
- OCB -> Build up the architectural elements such as (Columns, Walls, Windows, Doors) 
- Select the linked DWG in the floorplan view
- Create scheduling to the created elements and export it to Excel.


## Compile the source code
OCB add-in has been tested by Revit 2021. To apply it to other versions you need to rebuild it with correct .NET Framework.  
Revit 2022/2021 - .NET **4.8**  
Revit 2020/2019 - .NET 4.7  
Revit 2018      - .NET 4.6  
 
**REFERENCE** | The project hosts two external references, `RevitAPI.dll` and `RevitAPIUI.dll`.  

**BUILD EVENTS** | Set additional macros in post-build event to copy the built files to the Revit add-in folder.

`copy "$(TargetDir)"."" "$(AppData)\Autodesk\Revit\Addins\2021\"`

**DEBUG** | Within the project property, under DEBUG panel set external program as `...\Autodesk\Revit 2021\Revit.exe`


## About Plugin

This project uses Teigha for temporary development.  
  
A demo is online to build up the building model from CAD drawings. Only core components are covered (Columns, Walls, Windows, Doors). For now the project still needs more cunning & robust algorithms to create more elements, which will be the main enhance in the next-phase coding.  

<img src="/OCB Plugin Screenshot.png?raw=true">
