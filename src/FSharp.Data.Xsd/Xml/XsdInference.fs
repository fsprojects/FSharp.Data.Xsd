// --------------------------------------------------------------------------------------
// Implements XML type inference from XSD
// --------------------------------------------------------------------------------------

// The XML Provider infers a type from sample documents: an instance of InferedType 
// represents elements having a structure compatible with the given samples.
// When a schema is available we can use it to derive an InferedType representing
// valid elements according to the definitions in the given schema.
// The InferedType derived from a schema should be essentialy the same as one
// infered from a significant set of valid samples.
// With this perspective we can support some XSD leveraging the existing functionalities.
// The implementation uses a simplfied XSD model to split the task of deriving an InferedType:
// - element definitions in xsd files map to this simplified xsd model
// - instances of this xsd model map to InferedType.




namespace ProviderImplementation

open System.Xml
open System.Xml.Schema

/// Simplified model to represent schemas (XSD).
module XsdModel =
    
    type IsOptional = bool
    type Occurs = decimal * decimal

    type XsdElement = { Name: XmlQualifiedName
                        Type: XsdType
                        SubstitutionGroup: XsdElement list
                        IsAbstract: bool
                        IsNillable: bool }

    and XsdType = SimpleType of XmlTypeCode | ComplexType of XsdComplexType

    and XsdComplexType = 
        { Attributes: (XmlQualifiedName * XmlTypeCode * IsOptional) list
          Contents: XsdContent }

    and XsdContent = SimpleContent of XmlTypeCode | ComplexContent of XsdParticle

    and XsdParticle = 
        | Empty
        | Any      of Occurs 
        | Element  of Occurs * XsdElement
        | All      of Occurs * XsdParticle list
        | Choice   of Occurs * XsdParticle list
        | Sequence of Occurs * XsdParticle list

/// A simplified schema model is built from xsd. 
/// The actual parsing is done using BCL classes.
module XsdParsing =

    let ofType<'a> (sequence: System.Collections.IEnumerable) =
        sequence
        |> Seq.cast<obj>
        |> Seq.filter (fun x -> x :? 'a)
        |> Seq.cast<'a>
        

    type ParsingContext(xmlSchemaSet: XmlSchemaSet) =
        
        let getElm name = // lookup elements by name
            xmlSchemaSet.GlobalElements.Item name :?> XmlSchemaElement

        let subst = // lookup of substitution group members 
            xmlSchemaSet.GlobalElements.Values
            |> ofType<XmlSchemaElement>
            |> Seq.filter (fun e -> not e.SubstitutionGroup.IsEmpty)
            |> Seq.groupBy (fun e -> e.SubstitutionGroup)
            |> Seq.map (fun (name, values) -> getElm name, values |> List.ofSeq)
            |> dict

        let getSubst =     
            // deep lookup for trees of substitution groups, see 
            // http://docstore.mik.ua/orelly/xml/schema/ch12_01.htm#xmlschema-CHP-12-SECT-1       
            let collectSubst elm = 
                let items = System.Collections.Generic.HashSet()
                let rec collect elm =
                    if subst.ContainsKey elm then
                        for x in subst.Item elm do 
                            if items.Add x then collect x 
                collect elm 
                items |> List.ofSeq

            let subst' =
                subst.Keys
                |> Seq.map (fun x -> x, collectSubst x)
                |> dict

            fun elm -> if subst'.ContainsKey elm then subst'.Item elm else []
        

        // worth memoizing?
        let hasCycles element = 
            let items = System.Collections.Generic.HashSet<XmlSchemaObject>()
            let rec closure (obj: XmlSchemaObject) =
                let nav innerObj =
                    if items.Add innerObj then closure innerObj
                match obj with
                | :? XmlSchemaElement as e -> 
                    if e.RefName.IsEmpty then
                        nav e.ElementSchemaType
                        (getSubst e) |> Seq.iter nav
                    else nav (getElm e.RefName)
                | :? XmlSchemaComplexType as c -> 
                    nav c.ContentTypeParticle
                | :? XmlSchemaGroupRef as r -> 
                    nav r.Particle
                | :? XmlSchemaGroupBase as x -> 
                    x.Items 
                    |> ofType<XmlSchemaObject> 
                    |> Seq.iter nav
                | _ -> ()
            closure element
            items.Contains element


        member x.getElement name = getElm name
        member x.getSubstitutions elm = getSubst elm
        member x.isRecursive elm = hasCycles elm

    open XsdModel

    let rec parseElement (ctx: ParsingContext) elm = 
        let substitutionGroup = 
            ctx.getSubstitutions elm 
            |> List.filter (fun x -> x <> elm) 
            |> List.map (parseElement ctx)
        let elementType =
            if ctx.isRecursive elm
            then // empty content
                ComplexType { Attributes = []; Contents = ComplexContent Empty }
            else 
                match elm.ElementSchemaType with
                | :? XmlSchemaSimpleType  as x -> SimpleType x.Datatype.TypeCode
                | :? XmlSchemaComplexType as x -> ComplexType (parseComplexType ctx x)
                | x -> failwithf "unknown ElementSchemaType: %A" x
        { Name = elm.QualifiedName
          Type = elementType
          SubstitutionGroup = substitutionGroup
          IsAbstract = elm.IsAbstract
          IsNillable = elm.IsNillable }
        

    and parseComplexType (ctx: ParsingContext) (x: XmlSchemaComplexType) =  
        { Attributes = 
            x.AttributeUses.Values 
            |> ofType<XmlSchemaAttribute>
            |> Seq.filter (fun a -> a.Use <> XmlSchemaUse.Prohibited)
            |> Seq.map (fun a -> 
                a.QualifiedName,
                a.AttributeSchemaType.Datatype.TypeCode, 
                a.Use <> XmlSchemaUse.Required)
            |> List.ofSeq
          Contents = 
            match x.ContentType with
            | XmlSchemaContentType.TextOnly -> SimpleContent x.Datatype.TypeCode
            | XmlSchemaContentType.Mixed 
            | XmlSchemaContentType.Empty 
            | XmlSchemaContentType.ElementOnly -> 
                x.ContentTypeParticle |> parseParticle ctx |> ComplexContent
            | _ -> failwithf "Unknown content type: %A." x.ContentType }


    and parseParticle (ctx: ParsingContext) (par: XmlSchemaParticle) =  

        let occurs = par.MinOccurs, par.MaxOccurs

        let parseParticles (group: XmlSchemaGroupBase) =
            let particles = 
                group.Items
                |> ofType<XmlSchemaParticle> 
                |> Seq.map (parseParticle ctx)
                |> List.ofSeq // beware of recursive schemas
            match group with
            | :? XmlSchemaAll      -> All (occurs, particles)
            | :? XmlSchemaChoice   -> Choice (occurs, particles)
            | :? XmlSchemaSequence -> Sequence (occurs, particles)
            | _ -> failwithf "unknown group base: %A" group

        match par with
        | :? XmlSchemaAny -> Any occurs
        | :? XmlSchemaGroupBase as grp -> parseParticles grp
        | :? XmlSchemaGroupRef as grpRef -> parseParticle ctx grpRef.Particle
        | :? XmlSchemaElement as elm -> 
            let e = if elm.RefName.IsEmpty then elm else ctx.getElement elm.RefName
            Element (occurs, parseElement ctx e)
        | _ -> Empty // XmlSchemaParticle.EmptyParticle

    let getElements schema =
        let ctx = ParsingContext schema
        schema.GlobalElements.Values 
        |> ofType<XmlSchemaElement>
        |> Seq.filter (fun x -> x.ElementSchemaType :? XmlSchemaComplexType )
        |> Seq.map (parseElement ctx)


    
    open FSharp.Data.Runtime

    /// A custom XmlResolver is needed for included files because we get the contents of the main file 
    /// directly as a string from the FSharp.Data infrastructure. Hence the default XmlResolver is not
    /// able to find the location of included schema files.
    type ResolutionFolderResolver(resolutionFolder: string) =
        inherit XmlUrlResolver()

        let cache, _ = Caching.createInternetFileCache "XmlSchema" (System.TimeSpan.FromMinutes 30.0)

        let uri = // TODO improve this
            if resolutionFolder = "" then ""
            elif resolutionFolder.EndsWith "/" || resolutionFolder.EndsWith "\\"
            then resolutionFolder
            else resolutionFolder + "/"

        let useResolutionFolder (baseUri: System.Uri) = 
            resolutionFolder <> "" && (baseUri = null || baseUri.OriginalString = "")


        override _this.ResolveUri(baseUri, relativeUri) = 
            let u = System.Uri(relativeUri, System.UriKind.RelativeOrAbsolute)
            let result =
                if u.IsAbsoluteUri && (not <| u.IsFile)
                then base.ResolveUri(baseUri, relativeUri)
                elif useResolutionFolder baseUri
                then base.ResolveUri(System.Uri uri, relativeUri)
                else base.ResolveUri(baseUri, relativeUri)
            result

        override _this.GetEntity(absoluteUri, role, ofObjectToReturn) =
            if IO.isWeb absoluteUri 
            then
                let uri = absoluteUri.OriginalString
                match cache.TryRetrieve uri with
                | Some value -> value
                | None ->
                    let value = FSharp.Data.Http.RequestString uri //TODO async?
                    cache.Set(uri, value)
                    value
                |> fun value ->
                    // what if it's not UTF8?
                    let bytes = System.Text.Encoding.UTF8.GetBytes value
                    new System.IO.MemoryStream(bytes) :> obj
            else base.GetEntity(absoluteUri, role, ofObjectToReturn)

    
    let parseSchema resolutionFolder xsdText =
        let schemaSet = XmlSchemaSet() 
        schemaSet.XmlResolver <- ResolutionFolderResolver resolutionFolder
        let readerSettings = XmlReaderSettings()
        readerSettings.CloseInput <- true
        readerSettings.DtdProcessing <- DtdProcessing.Ignore
        use reader = XmlReader.Create(new System.IO.StringReader(xsdText), readerSettings)
        schemaSet.Add(null, reader) |> ignore
        schemaSet.Compile()
        schemaSet



/// Element definitions in a schema are mapped to InferedType instances
module XsdInference =
    open XsdModel
    open FSharp.Data.Runtime.StructuralTypes

    // for now we map only the types supported in F# Data
    let getType = function
        | XmlTypeCode.Int -> typeof<int>
        | XmlTypeCode.Long -> typeof<int64>
        | XmlTypeCode.Date -> typeof<System.DateTime>
        | XmlTypeCode.DateTime -> typeof<System.DateTime>
        | XmlTypeCode.Boolean -> typeof<bool>
        | XmlTypeCode.Decimal -> typeof<decimal>
        | XmlTypeCode.Double -> typeof<double>
        // fallback to string
        | _ -> typeof<string>

    let getMultiplicity = function
        | 1M, 1M -> Single
        | 0M, 1M -> OptionalSingle
        | _ -> Multiple

    // how multiplicity is affected when nesting particles
    let combineMultiplicity = function
        | Single, x -> x
        | Multiple, _ -> Multiple
        | _, Multiple -> Multiple
        | OptionalSingle, _ -> OptionalSingle
       
    // the effect of a choice is to make mandatory items optional 
    let makeOptional = function Single -> OptionalSingle | x -> x

    let getElementName (elm: XsdElement) =
        if elm.Name.Namespace = "" 
        then Some elm.Name.Name
        else Some (sprintf "{%s}%s" elm.Name.Namespace elm.Name.Name)

    let nil = { InferedProperty.Name = "{http://www.w3.org/2001/XMLSchema-instance}nil"
                Type = InferedType.Primitive(typeof<bool>, None, true) }

    // collects element definitions in a particle
    let rec getElements parentMultiplicity = function
        | XsdParticle.Element(occ, elm) -> 
            let mult = combineMultiplicity(parentMultiplicity, getMultiplicity occ)
            match elm.IsAbstract, elm.SubstitutionGroup with
            | _, [] -> [ (elm, mult) ]
            | true, [x] -> [ (x, mult) ]
            | true, x -> x |> List.map (fun e -> e,  makeOptional mult)
            | false, x -> elm::x |> List.map (fun e -> e,  makeOptional mult)
        | XsdParticle.Sequence (occ, particles)
        | XsdParticle.All (occ, particles) -> 
            let mult = combineMultiplicity(parentMultiplicity, getMultiplicity occ)
            particles |> List.collect (getElements mult)
        | XsdParticle.Choice (occ, particles) -> 
            let mult = makeOptional (getMultiplicity occ)
            let mult' = combineMultiplicity(parentMultiplicity, mult)
            particles |> List.collect (getElements mult')
        | XsdParticle.Empty -> []
        | XsdParticle.Any _ -> []


    // derives an InferedType for an element definition
    and inferElementType (elm: XsdElement) =
        let name = getElementName elm
        if elm.IsAbstract 
        then InferedType.Record(name, [], optional = false)
        else
        match elm.Type with
        | SimpleType typeCode ->
            let ty = InferedType.Primitive (getType typeCode, None, elm.IsNillable)
            let prop = { InferedProperty.Name = ""; Type = ty }
            let props = if elm.IsNillable then [prop; nil] else [prop]
            InferedType.Record(name, props, optional = false)
        | ComplexType cty -> 
            let props = inferProperties cty
            let props = 
                if elm.IsNillable then 
                    for prop in props do 
                        prop.Type <- prop.Type.EnsuresHandlesMissingValues false
                    nil::props 
                else props
            InferedType.Record(name, props, optional = false)

    and inferElements = function
        | [] -> failwith "No suitable element definition found in the schema."
        | [elm] -> inferElementType elm
        | elms -> 
            elms 
            |> List.map (fun elm -> 
                InferedTypeTag.Record (getElementName elm), inferElementType elm)
            |> Map.ofList
            |> InferedType.Heterogeneous


    and inferProperties cty =
        let attrs: InferedProperty list = 
            cty.Attributes
            |> List.map (fun (name, typeCode, optional) ->
                { Name = name.Name
                  Type = InferedType.Primitive (getType typeCode, None, optional) } )
        match cty.Contents with
        | SimpleContent typeCode -> 
            let ty = InferedType.Primitive (getType typeCode, None, false)
            { Name = ""; Type = ty }::attrs
        | ComplexContent xsdParticle ->
            match inferParticle InferedMultiplicity.Single xsdParticle with
            | InferedTypeTag.Null, _ -> attrs // empty content
            | _tag, (_mul, ty) -> { Name = ""; Type = ty }::attrs


    and inferParticle (parentMultiplicity: InferedMultiplicity) particle =
        let getRecordTag (e:XsdElement) = InferedTypeTag.Record(getElementName e)
        match getElements parentMultiplicity particle with
        | [] -> 
            InferedTypeTag.Null, 
            (InferedMultiplicity.OptionalSingle, InferedType.Null)
        | items ->
            let tags = items |> List.map (fst >> getRecordTag)
            
            let types = 
                items 
                |> Seq.zip tags
                |> Seq.map (fun (tag, (e, m)) -> tag, (m, inferElementType e))
                |> Map.ofSeq
            InferedTypeTag.Collection, 
            (parentMultiplicity, InferedType.Collection(tags, types))
            
