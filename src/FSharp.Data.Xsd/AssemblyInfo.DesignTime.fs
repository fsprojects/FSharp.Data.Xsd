namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharp.Data.Xsd.DesignTime")>]
[<assembly: AssemblyProductAttribute("FSharp.Data.Xsd")>]
[<assembly: AssemblyDescriptionAttribute("Xml Type Provider based on Xml Schema")>]
[<assembly: AssemblyVersionAttribute("0.0.8")>]
[<assembly: AssemblyFileVersionAttribute("0.0.8")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.8"
    let [<Literal>] InformationalVersion = "0.0.8"
