#r"../packages/FSharp.Data.Xsd.0.0.7/lib/net45/FSharp.Data.Xsd.dll"
#r"System.IO"
#r"System.Xml"
#r"System.Xml.Linq"

open System.IO
open System.Xml
open System.Xml.Schema

let parseSchema (inputUri: string) =
    let schemaSet = XmlSchemaSet()
    let reader = XmlReader.Create(inputUri)
    XmlSchema.Read(reader, null) |> schemaSet.Add |> ignore
    schemaSet.Compile()
    reader.Dispose()
    schemaSet


let hasElements inputUri =
    (parseSchema inputUri).GlobalElements.Values 
    |> Seq.cast<obj>
    |> Seq.filter (fun x -> x :? XmlSchemaElement)
    |> Seq.cast<XmlSchemaElement>
    |> Seq.exists (fun x -> x.ElementSchemaType :? XmlSchemaComplexType )


let folder = "c:/temp/schemas"
let writer = File.CreateText(Path.Combine(__SOURCE_DIRECTORY__, "AllSchemas.fsx"))
writer.WriteLine """
#r"../packages/FSharp.Data.Xsd.0.0.7/lib/net45/FSharp.Data.Xsd.dll"
#r"System.Xml"
#r"System.Xml.Linq"

open FSharp.Data

"""
let mutable num = 0
let errorFile = File.CreateText(Path.Combine(__SOURCE_DIRECTORY__, "errors.txt"))
for xsd in Directory.EnumerateFiles(folder, "*.xsd", SearchOption.AllDirectories) do
    num <- num + 1
    try
        if hasElements xsd then
            let fi = FileInfo xsd
            let tripleQuote = "\"\"\""
            let resFolder = tripleQuote + fi.Directory.FullName + tripleQuote
            let line = sprintf """//type t%i = XmlProvider<Schema ="%s", ResolutionFolder=%s>""" num fi.Name resFolder
            writer.WriteLine line
    with e -> errorFile.WriteLine (sprintf "ERROR for %s:\n%s" xsd e.Message)
writer.Dispose()
errorFile.Dispose()