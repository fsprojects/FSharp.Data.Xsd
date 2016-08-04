module XsdInferenceTests

open FSharp.Data
open ProviderImplementation
open FSharp.Data.Runtime.StructuralTypes
open FSharp.Data.Runtime.StructuralInference
open System.Xml
open System.Xml.Linq
open System.Xml.Schema
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

let isValid xsd =
    let xmlSchemaSet = XsdParsing.parseSchema "" xsd
    fun xml -> 
        let settings = XmlReaderSettings(ValidationType = ValidationType.Schema)
        settings.Schemas <- xmlSchemaSet
        use reader = XmlReader.Create(new System.IO.StringReader(xml), settings)
        try
            while reader.Read() do ()
            true
        with :? XmlSchemaException as e -> 
            printfn "%s/n%s" xml e.Message
            false


let print xsd xmlSamples =
    let isValid = isValid xsd
    for xml in xmlSamples do
        xml.ToString() |> isValid |> should equal true

    let inferedTypeFromSchema = getInferedTypeFromSchema xsd
    printfn "%A" inferedTypeFromSchema

    let inferedTypeFromSamples = getInferedTypeFromSamples xmlSamples
    printfn "%A" inferedTypeFromSamples

    inferedTypeFromSchema, inferedTypeFromSamples



let check xsd xmlSamples =
    let inferedTypeFromSchema, inferedTypeFromSamples = print xsd xmlSamples
    inferedTypeFromSchema = inferedTypeFromSamples
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
    let sample2 = "<root2 bar='aa' baz='2017-12-22' />"
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


[<Test>]
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
    
[<Test>]
let ``sequence of sequence``() =
    let xsd = """
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
      elementFormDefault="qualified" attributeFormDefault="unqualified">
      <xs:element name="foo">
        <xs:complexType>
		  <xs:sequence maxOccurs='unbounded'>
            <xs:sequence maxOccurs='1'>
			  <xs:element name="bar" type="xs:int"/>
			  <xs:element name="baz" type="xs:int"/>
            </xs:sequence>
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

[<Test>]
let ``sequence of sequences``() =
    let xsd = """
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
      elementFormDefault="qualified" attributeFormDefault="unqualified">
      <xs:element name="foo">
        <xs:complexType>
		  <xs:sequence maxOccurs='1'>
            <xs:sequence maxOccurs='unbounded'>
			  <xs:element name="bar" type="xs:int"/>
			  <xs:element name="baz" type="xs:int"/>
            </xs:sequence>
		  </xs:sequence>
		</xs:complexType>
	  </xs:element>
    </xs:schema>    """
    let sample = """
    <foo>
        <bar>2</bar><baz>5</baz>
        <bar>3</bar><baz>6</baz>
    </foo>"""
    print xsd [| sample |]
    |> ignore


[<Test>]
let ``element with simple content``() =
    let xsd = """
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
      elementFormDefault="qualified" attributeFormDefault="unqualified">
      <xs:element name="foo">
		<xs:complexType>
		  <xs:simpleContent>
			  <xs:extension base="xs:date">
				  <xs:attribute name="bar" type="xs:string" />
				  <xs:attribute name="baz" type="xs:int" />
			  </xs:extension>
		  </xs:simpleContent>
		</xs:complexType>
	  </xs:element>
    </xs:schema>"""
    let sample1 = """<foo bar="hello">1957-08-13</foo>"""
    let sample2 = """<foo bar="hello" baz="2">1957-08-13</foo>"""
    print xsd [| sample1; sample2 |]
    |> ignore


