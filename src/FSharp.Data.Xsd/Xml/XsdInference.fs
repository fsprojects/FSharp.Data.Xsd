// --------------------------------------------------------------------------------------
// Implements XML type inference from XSD
// --------------------------------------------------------------------------------------

// The XML Provider infers a type from sample documents: an instance of InferedType 
// represents elements having a structure compatible with the given samples.
// When a schema is availbale we can use it to derive an InferedType representing
// valid elements according to the definitions in the given schema.
// The InferedType derived from a schema should be essentialy the same as one
// infered from a significant set of valid samples.
// This is an easy way to support some XSD leveraging the existing functionalities.
// The implementation uses a simplfied XSD model to split the task of deriving an InferedType:
// - element definitions in xsd files map to this simplified xsd model
// - instances of this xsd model map to InferedType.




namespace ProviderImplementation

open System.Xml
open System.Xml.Schema

/// Simplified model to represent schemas (XSD).
module XsdModel =

    type XsdElement = { Name: XmlQualifiedName; Type: XsdType; IsNillable: bool }

    and XsdType = SimpleType of XmlTypeCode | ComplexType of XsdComplexType

    and XsdComplexType = 
        { Attributes: (XmlQualifiedName * XmlTypeCode * bool) list // bool is for optional
          Contents: XsdContent 
          IsMixed: bool }

    and XsdContent = SimpleContent of XmlTypeCode | ComplexContent of XsdParticle

    and Occurs = decimal * decimal

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

    /// A custom XmlResolver is needed for included files because we get the contents of the main file 
    /// directly as a string from the FSharp.Data infrastructure. Hence the default XmlResolver is not
    /// able to find the location of included schema files.
    type ResolutionFolderResolver(resolutionFolder) =
        inherit XmlUrlResolver()
        override _this.ResolveUri(_, relativeUri) = 
            System.Uri(System.IO.Path.Combine(resolutionFolder, relativeUri))

    
    open XsdModel

    let ofType<'a> (sequence: System.Collections.IEnumerable) =
        sequence
        |> Seq.cast<obj>
        |> Seq.filter (fun x -> x :? 'a)
        |> Seq.cast<'a>
        

    let hasCycles x = 
        let items = System.Collections.Generic.HashSet<XmlSchemaObject>()
        let rec closure (obj: XmlSchemaObject) =
            let nav innerObj =
                if items.Add innerObj then closure innerObj
            match obj with
            | :? XmlSchemaElement as e -> 
                nav e.ElementSchemaType 
            | :? XmlSchemaComplexType as c -> 
                nav c.ContentTypeParticle
            | :? XmlSchemaGroupRef as r -> 
                nav r.Particle
            | :? XmlSchemaGroupBase as x -> 
                x.Items 
                |> ofType<XmlSchemaObject> 
                |> Seq.iter nav
            | _ -> ()
        closure x
        items.Contains x


    let rec parseElement (elm: XmlSchemaElement) =  

        if hasCycles elm then failwith "Recursive schemas are not supported yet."

        { Name = elm.QualifiedName
          Type = 
            match elm.ElementSchemaType with
            | :? XmlSchemaSimpleType  as x -> SimpleType x.Datatype.TypeCode
            | :? XmlSchemaComplexType as x -> ComplexType (parseComplexType x)
            | x -> failwithf "unknown ElementSchemaType: %A" x
          IsNillable = elm.IsNillable }

    and parseComplexType (x: XmlSchemaComplexType) =
        { Attributes = 
            x.AttributeUses.Values |> ofType<XmlSchemaAttribute>
            |> Seq.filter (fun a -> a.Use <> XmlSchemaUse.Prohibited)
            |> Seq.map (fun a -> a.QualifiedName,
                                    a.AttributeSchemaType.Datatype.TypeCode, 
                                    a.Use <> XmlSchemaUse.Required)
            |> List.ofSeq
          Contents = 
                match x.ContentType with
                | XmlSchemaContentType.TextOnly -> SimpleContent x.Datatype.TypeCode
                | XmlSchemaContentType.Mixed 
                | XmlSchemaContentType.Empty 
                | XmlSchemaContentType.ElementOnly -> 
                    x.ContentTypeParticle |> parseParticle |> ComplexContent
                | _ -> failwithf "Unknown content type: %A." x.ContentType

          IsMixed = x.IsMixed }


    and parseParticle (par: XmlSchemaParticle) =

        let occurs = par.MinOccurs, par.MaxOccurs

        let parseParticles (group: XmlSchemaGroupBase) =
            let particles = 
                group.Items
                |> ofType<XmlSchemaParticle> 
                |> Seq.map parseParticle
                |> List.ofSeq // beware of recursive schemas
            match group with
            | :? XmlSchemaAll      -> All (occurs, particles)
            | :? XmlSchemaChoice   -> Choice (occurs, particles)
            | :? XmlSchemaSequence -> Sequence (occurs, particles)
            | _ -> failwithf "unknown group base: %A" group

        match par with
        | :? XmlSchemaAny -> Any occurs
        | :? XmlSchemaGroupBase as grp -> parseParticles grp
        | :? XmlSchemaGroupRef as grpRef -> parseParticle grpRef.Particle
        | :? XmlSchemaElement as elm -> Element (occurs, parseElement elm)
        | _ -> Empty // XmlSchemaParticle.EmptyParticle



    let parseSchema resolutionFolder xsdText =
        let schemaSet = XmlSchemaSet()
        if resolutionFolder <> "" then
            schemaSet.XmlResolver <- ResolutionFolderResolver(resolutionFolder)
        use reader = new XmlTextReader(new System.IO.StringReader(xsdText))
        XmlSchema.Read(reader, null) |> schemaSet.Add |> ignore
        schemaSet.Compile()
        schemaSet


    let getElements (schema: XmlSchemaSet) =
        schema.GlobalElements.Values 
        |> ofType<XmlSchemaElement>
        |> Seq.filter (fun x -> x.ElementSchemaType :? XmlSchemaComplexType )
        |> Seq.map parseElement


/// Element definitions in a schema are mapped to InferedType instances
module XsdInference =
    open XsdModel
    open FSharp.Data.Runtime.StructuralTypes

    let getType = function
        | XmlTypeCode.Int -> typeof<int>
        | XmlTypeCode.Date -> typeof<System.DateTime>
        | XmlTypeCode.DateTime -> typeof<System.DateTime>
        | XmlTypeCode.Boolean -> typeof<bool>
        | XmlTypeCode.Decimal -> typeof<decimal>
        | XmlTypeCode.Double -> typeof<double>
        | XmlTypeCode.Float -> typeof<single>
        // fallback to string
        | _ -> typeof<string>

    let getMultiplicity = function
        | 1M, 1M -> Single
        | 0M, 1M -> OptionalSingle
        | _ -> Multiple

//    let mult' parentMultiplicity occurs = 
//        if parentMultiplicity = Multiple 
//        then Multiple 
//        else getMultiplicity occurs

    // how multiplicity is affected when nesting particles
    let combineMultiplicity = function
        | Single, x -> x
        | Multiple, _ -> Multiple
        | _, Multiple -> Multiple
        | OptionalSingle, _ -> OptionalSingle
       
    // the effect of a choice is to make mandatory items optional 
    let makeOptional = function Single -> OptionalSingle | x -> x

    // collects element definitions in a particle
    let rec getElements parentMultiplicity = function
        | XsdParticle.Element(occ, elm) -> 
            [ (elm, combineMultiplicity(parentMultiplicity, getMultiplicity occ)) ]
        | XsdParticle.Sequence (occ, particles)
        | XsdParticle.All (occ, particles) -> 
            let m = combineMultiplicity(parentMultiplicity, getMultiplicity occ)
            particles |> List.collect (getElements m)
        | XsdParticle.Choice (occ, particles) -> 
            let mult' = makeOptional (getMultiplicity occ)
            let m = combineMultiplicity(parentMultiplicity, mult')
            particles |> List.collect (getElements m)
        | XsdParticle.Empty -> []
        | XsdParticle.Any occ -> 
            let name = XmlQualifiedName "{anyNs}anyElement"
            let typ = SimpleType XmlTypeCode.UntypedAtomic
            let elm = { Name = name; Type = typ; IsNillable = false }
            [ (elm, combineMultiplicity(parentMultiplicity, getMultiplicity occ))] 
        

    // derives an InferedType for an element definition
    let rec inferElementType (elm: XsdElement) =
        let name = Some elm.Name.Name
        match elm.Type with
        | SimpleType typeCode ->
            let ty = InferedType.Primitive (getType typeCode, None, elm.IsNillable)
            InferedType.Record(name, [{ Name = ""; Type = ty }], optional = false)
        | ComplexType cty -> 
            InferedType.Record(name, inferProperties cty, optional = false)


    and inferElements (elms: XsdElement list) =
        match elms with
        | [] -> failwith "No suitable element definition found in the schema."
        | [elm] -> inferElementType elm
        | _ -> 
            elms 
            |> List.map (fun elm -> 
                InferedTypeTag.Record (Some elm.Name.Name), inferElementType elm)
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
        let getRecordTag (e:XsdElement) = InferedTypeTag.Record(Some e.Name.Name)
        match getElements parentMultiplicity particle with
        | [] -> 
            InferedTypeTag.Null, 
            (InferedMultiplicity.OptionalSingle, InferedType.Null)
//        | [ (elm, mul) ] ->
//            InferedTypeTag.Record(Some elm.Name.Name), 
//            (mul, inferElementType elm)
        | items ->
            let tags = items |> List.map (fst >> getRecordTag)
            let types = 
                items 
                |> Seq.zip tags
                |> Seq.map (fun (tag, (e, m)) -> tag, (m, inferElementType e))
                |> Map.ofSeq
            InferedTypeTag.Collection, 
            (parentMultiplicity, InferedType.Collection(tags, types))
            



//    and inferParticle2 (parentMultiplicity: InferedMultiplicity) = function
//        | Element (occurs, e) -> 
//            InferedTypeTag.Record(Some e.Name.Name), 
//            (mult' parentMultiplicity occurs, inferElement e)
//        | All (occurs, items) 
//        | Sequence (occurs, items) -> 
//            let mul = mult' parentMultiplicity occurs
//            let inner = items |> List.map (inferParticle mul)
//            let tags  = inner |> List.map fst
//            let types = inner |> Map.ofList
//            InferedTypeTag.Collection, 
//            (mul, InferedType.Collection(tags, types))
//        | Choice (occurs, items) ->  
//            let mul = mult' parentMultiplicity occurs
//            let makeOptional = function Single -> OptionalSingle | x -> x
//            let inner = items |> List.map (inferParticle mul)
//            let tags  = inner |> List.map fst
//            let types = 
//                inner 
//                |> List.map (fun (tag, (mult, ty)) -> tag, (makeOptional mult, ty))
//                |> Map.ofList
//            InferedTypeTag.Collection, 
//            (mul, InferedType.Collection(tags, types))
//        | Empty -> 
//            InferedTypeTag.Null, 
//            (InferedMultiplicity.OptionalSingle, InferedType.Null)
//        | Any occurs -> 
//            let name = Some "{anyNs}anyElement"
//            InferedTypeTag.Record(name), 
//            (mult' parentMultiplicity occurs, 
//                InferedType.Record (name, [{Name = ""; Type = InferedType.Top}], false))
