# Graph PCG
This package contains a **Procedural Generation Framework** for Unity.  
The framework uses a **Gamespace Graph** to describe scenes and levels. The graph determines the property of objects and spaces in the game world.  
Custom **Procedural Generators** can utilize the graph allowing for better results and easier configuration.

## Installation
**Before installing make sure to have all required [Dependencies](!Required_Dependencies) installed.**

Add the package in Unity use the Package Manager:

![image](https://github.com/user-attachments/assets/9ff0cf0a-d38c-460f-abe5-726ef5cb6922)
```
https://github.com/AchimBunke/Graph_PCG.git
```
Warnings or errors may appear after importing. They should not impact the installation.

### Required Dependencies
**This project will not work unless following dependencies are imported through the Package Manager:**
- [Achioto Custom Utilities (v0.4.0)](https://github.com/AchimBunke/Unity_CustomUtilities.git) Unity and .NET tools for Unity Game development.  
  Package Manager URL: `https://github.com/AchimBunke/Unity_CustomUtilities.git`
- [Json.NET Converters of Unity types (v.1.6.3)](https://github.com/applejag/Newtonsoft.Json-for-Unity.Converters.git#1.6.3) JSON Serialization for Unity types such as Color.  
  Package Manager URL: `https://github.com/applejag/Newtonsoft.Json-for-Unity.Converters.git#1.6.3`
- [UniRx (v.7.1.0)](https://github.com/neuecc/UniRx.git?path=Assets/Plugins/UniRx/Scripts) Reactive programming for Unity.  
  Package Manager URL: `https://github.com/neuecc/UniRx.git?path=Assets/Plugins/UniRx/Scripts`

## Samples
This package provides samples to demonstrate the graph and PCG.  

![image](https://github.com/user-attachments/assets/33f4b59d-f3fe-489e-851f-8ce90984bb75)

I highly recommend to take a look a the provided Samples to understand the framework and tools.

**Some samples will only work if the ```USE_HGRAPH_MODULES``` Symbol is defined under `ProjectSettings/Player/Scripting Define Symbols`:**

![image](https://github.com/user-attachments/assets/0104297b-10c5-4e8b-b279-b39127aefca4)  

Activating this Symbol compiles the package with `PCGModules`. They are used to define **Categories** in C# and includes them into loaded **Gamespace Graphs**.  

Custom **PCG Modules** can still be defined without it.

## Acknowledgements
Checkout the **Wave Function Collapse** game on [itch.io](https://marian42.itch.io/wfc) or [Github](https://github.com/marian42/wavefunctioncollapse) by ***marian42***.

Thanks to [Oskar Sigvardsson](https://github.com/OskarSigvardsson/unity-delaunay) for providing Delaunay and Voronoi interpolation implementations.   


## Project Info
Author:  Achim Bunke  
Github: [Achioto](https://github.com/AchimBunke)  
E-Mail: achim.bunke.dev@gmail.com  


