namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSharp.Data.Xsd")>]
[<assembly: AssemblyProductAttribute("FSharp.Data.Xsd")>]
[<assembly: AssemblyDescriptionAttribute("Xml Type Provider based on Xml Schema")>]
[<assembly: AssemblyVersionAttribute("0.0.3")>]
[<assembly: AssemblyFileVersionAttribute("0.0.3")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.0.3"
    let [<Literal>] InformationalVersion = "0.0.3"
