namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharp.Data.Xsd.DesignTime")>]
[<assembly: AssemblyProductAttribute("FSharp.Data.Xsd")>]
[<assembly: AssemblyDescriptionAttribute("Xml Type Provider based on Xml Schema")>]
[<assembly: AssemblyVersionAttribute("0.0.5")>]
[<assembly: AssemblyFileVersionAttribute("0.0.5")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.5"
    let [<Literal>] InformationalVersion = "0.0.5"
