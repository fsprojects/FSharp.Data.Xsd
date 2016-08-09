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
    printfn "%s" xsd
    printfn "-----------------------------------------------------"
    let isValid = isValid xsd
    for xml in xmlSamples do
        printfn "%A" xml
        printfn "-----------------------------------------------------"
        xml.ToString() |> isValid |> should equal true

    let inferedTypeFromSchema = getInferedTypeFromSchema xsd
    printfn "%A" inferedTypeFromSchema

    let inferedTypeFromSamples = getInferedTypeFromSamples xmlSamples
    printfn "%A" inferedTypeFromSamples

    inferedTypeFromSchema, inferedTypeFromSamples



let check xsd xmlSamples =
    printfn "checking schema and samples"
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
let ``untyped elements have no properties``() =
    let xsd = """
      <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
        elementFormDefault="qualified" attributeFormDefault="unqualified">
          <xs:element name="foo" />
      </xs:schema>
    """
    let sample1 = "<foo><a/></foo>"
    let sample2 = "<foo><b/></foo>"
    let ty, _ = print xsd [| sample1; sample2 |]
    ty |> should equal (InferedType.Record (Some "foo",[],false))


[<Test>]
let ``recursive schemas don't cause loops``() =
    let xsd = """ <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
      elementFormDefault="qualified" attributeFormDefault="unqualified">
	    <xs:complexType name="TextType" mixed="true">
		    <xs:choice minOccurs="0" maxOccurs="unbounded">
			    <xs:element ref="bold"/>
			    <xs:element ref="italic"/>
			    <xs:element ref="underline"/>
		    </xs:choice>
	    </xs:complexType>
	    <xs:element name="bold" type="TextType"/>
	    <xs:element name="italic" type="TextType"/>
	    <xs:element name="underline" type="TextType"/>
    </xs:schema>"""
    let inferedTypeFromSchema = getInferedTypeFromSchema xsd
    printfn "%A" inferedTypeFromSchema
    

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
let ``sequences can have elements``() =
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
let ``sequences can have multiple elements``() =
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
let ``sequences may occur multiple times``() =
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
let ``sequences can be nested``() =
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
let ``sequences can have multiple nested sequences``() =
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
    check xsd [| sample |]


[<Test>]
let ``simple content can be extended with attributes``() =
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
    let sample1 = """<foo>1957-08-13</foo>"""
    let sample2 = """<foo bar="hello" baz="2">1957-08-13</foo>"""
    check xsd [| sample1; sample2 |]

[<Test>]
let ``elements in a choice become optional``() =
    let xsd = """
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
      elementFormDefault="qualified" attributeFormDefault="unqualified">
      <xs:element name="foo">
		<xs:complexType>
		  <xs:choice>
            <xs:element name="bar" type="xs:int" />
            <xs:element name="baz" type="xs:date" />
		  </xs:choice>
		</xs:complexType>
	  </xs:element>
    </xs:schema>"""
    let sample1 = """<foo><bar>5</bar></foo>"""
    let sample2 = """<foo><baz>1957-08-13</baz></foo>"""
    check xsd [| sample1; sample2 |]

[<Test>]
let ``elements can reference attribute groups``() =
    let xsd = """
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
      elementFormDefault="qualified" attributeFormDefault="unqualified">
	    <xs:attributeGroup name="myAttributes">
		    <xs:attribute name="myNr" type="xs:int" use="required" />
		    <xs:attribute name="available" type="xs:boolean" use="required" />
	    </xs:attributeGroup>
	    <xs:element name="foo">
            <xs:complexType>
                <xs:attributeGroup ref="myAttributes"/>
                <xs:attribute name="lang" type="xs:language" use="required" />
            </xs:complexType>
	    </xs:element>
    </xs:schema>"""
    let sample1 = """
    <foo myNr="42" available="false" lang="en-US" />"""
    check xsd [| sample1 |]
    


