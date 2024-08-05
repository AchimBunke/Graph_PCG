# Installation
## Required Dependencies
**This project will not work unless following dependencies are imported through the Package Manager:**
- [Achioto Custom Utilities (v0.4.0)](https://github.com/AchimBunke/Unity_CustomUtilities.git) Unity and .NET tools for Unity Game development.
- [Json.NET Converters of Unity types (v.1.6.3)](https://github.com/applejag/Newtonsoft.Json-for-Unity.Converters.git#1.6.3) JSON Serialization for Unity types such as Color.
- [UniRx (v.7.1.0)](https://github.com/neuecc/UniRx.git?path=Assets/Plugins/UniRx/Scripts) Reactive programming for Unity.
## Define Symbols
Settings for the PCG generators in the framework can be included in the graph as PCGModules.
To enable these modules, add the symbol ```USE_HGRAPH_MODULES``` to the "Scripting Define Symbols" in the project Player Settings.
