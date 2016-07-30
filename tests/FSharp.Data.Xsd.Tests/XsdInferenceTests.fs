module XsdInferenceTests

open FSharp.Data
open ProviderImplementation
open FSharp.Data.Runtime.StructuralTypes
open FSharp.Data.Runtime.StructuralInference
open System.Xml.Linq
open NUnit.Framework
open FsUnit

let infer = 
    let culture = System.Globalization.CultureInfo.InvariantCulture
    XmlInference.inferType true culture false false

let getInferedTypeFromSample xml =
    infer [| XElement.Parse xml |]
    |> Seq.exactlyOne

let getInferedTypeFromSamples samples =
    samples
    |> Array.map XElement.Parse
    |> infer 
    |> Seq.fold (subtypeInfered (*allowEmptyValues*)false) InferedType.Top

    
let getInferedTypeFromSchema xsd =
    xsd
    |> XsdParsing.parseSchema ""
    |> XsdParsing.getElements
    |> List.ofSeq
    |> XsdInference.inferElements

let rec compare = function
| InferedType.Record (name1, fields1, opt1), InferedType.Record (name2, fields2, opt2) ->
    name1 = name2 && opt1 = opt2 && fields1 = fields2
    //TODO compare fields

//| InferedType.Top, InferedType.Top -> true
//| InferedType.Null, InferedType.Null -> true
//| InferedType.Primitive (typ1, unit1, opt1), InferedType.Primitive (typ2, unit2, opt2) -> 
//    typ1 = typ2 && unit1 = unit2 && opt1 = opt2
//| InferedType.Json (typ1, opt1), InferedType.Json (typ2, opt2) -> 
//    opt1 = opt2 && compare(typ1, typ2)

| InferedType.Heterogeneous map1, InferedType.Heterogeneous map2 -> 
    true //TODO
| InferedType.Collection _, InferedType.Collection _ -> 
    true //TODO
| x, y -> x = y


let check xsd xmlSamples =
    let inferedTypeFromSchema = getInferedTypeFromSchema xsd
    printfn "%A" inferedTypeFromSchema

    let inferedTypeFromSamples = getInferedTypeFromSamples xmlSamples
    printfn "%A" inferedTypeFromSamples

    compare (inferedTypeFromSchema, inferedTypeFromSamples)
    |> should equal true


[<Test>]
let ``at least one global complex element is needed``() =
    let xsd = """
      <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
        elementFormDefault="qualified" attributeFormDefault="unqualified">
          <xs:element name="foo" type="xs:string" />
      </xs:schema>
    """
    let msg = "No suitable element definition found in the schema."
    (fun () -> getInferedTypeFromSchema xsd |> ignore) 
    |> should (throwWithMessage msg) typeof<System.Exception>
    
[<Test>]
let ``attributes become properties``() =
    let xsd = """
      <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
        elementFormDefault="qualified" attributeFormDefault="unqualified">
        <xs:element name="foo">
	       <xs:complexType>
		     <xs:attribute name="bar" type="xs:string" use="required" />
             <xs:attribute name="baz" type="xs:int" />
	      </xs:complexType>
	    </xs:element>
      </xs:schema>    """
    let sample1 = "<foo bar='aa' />"
    let sample2 = "<foo bar='aa' baz='2' />"
    check xsd [| sample1; sample2 |]
    
[<Test>]
let ``multiple root elements are allowed``() =
    let xsd = """
      <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
        elementFormDefault="qualified" attributeFormDefault="unqualified">
        <xs:element name="root1">
	      <xs:complexType>
		     <xs:attribute name="foo" type="xs:string" use="required" />
             <xs:attribute name="fow" type="xs:int" use="required" />
	      </xs:complexType>
	    </xs:element>
        <xs:element name="root2">
           <xs:complexType>
		     <xs:attribute name="bar" type="xs:string" use="required" />
             <xs:attribute name="baz" type="xs:date" use="required" />
	       </xs:complexType>
        </xs:element>
      </xs:schema>    
      """
    let sample1 = "<root1 foo='aa' fow='34' />"
    let sample2 = "<root2 bar='aa' baz='12-22-2017' />"
    check xsd [| sample1; sample2 |]
  

[<Test>]
let ``sequence of elements``() =
    let xsd = """
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
      elementFormDefault="qualified" attributeFormDefault="unqualified">
      <xs:element name="foo">
        <xs:complexType>
		  <xs:sequence>
			<xs:element name="bar" type="xs:int"/>
			<xs:element name="baz" type="xs:int"/>
		  </xs:sequence>
		</xs:complexType>
	  </xs:element>
    </xs:schema>    """
    let sample = "<foo><bar>2</bar><baz>5</baz></foo>"
    check xsd [| sample |]

[<Test>]
let ``sequence of multiple elements``() =
    let xsd = """
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
      elementFormDefault="qualified" attributeFormDefault="unqualified">
      <xs:element name="foo">
        <xs:complexType>
		  <xs:sequence>
			<xs:element name="bar" type="xs:int" maxOccurs='unbounded' />
			<xs:element name="baz" type="xs:int" maxOccurs='3' />
		  </xs:sequence>
		</xs:complexType>
	  </xs:element>
    </xs:schema>    """
    let sample = """
    <foo>
      <bar>2</bar>
      <bar>3</bar>
      <baz>5</baz>
      <baz>5</baz>
    </foo>"""
    check xsd [| sample |]

// this is a bit tricky
//[<Test>]
let ``repeated sequence``() =
    let xsd = """
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
      elementFormDefault="qualified" attributeFormDefault="unqualified">
      <xs:element name="foo">
        <xs:complexType>
		  <xs:sequence maxOccurs='unbounded'>
			<xs:element name="bar" type="xs:int"/>
			<xs:element name="baz" type="xs:int"/>
		  </xs:sequence>
		</xs:complexType>
	  </xs:element>
    </xs:schema>    """
    let sample = """
    <foo>
        <bar>2</bar><baz>5</baz>
        <bar>3</bar><baz>6</baz>
    </foo>"""
    check xsd [| sample |]
    
