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

let getInferedTypeFromSamples samples =
    let culture = System.Globalization.CultureInfo.InvariantCulture
    samples
    |> Array.map XElement.Parse
    |> XmlInference.inferType true culture false false 
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
            printfn "%s/n%s" e.Message xml
            false


let getInferedTypes xsd xmlSamples =
    //printfn "%s/n---------------------------------------------" xsd
    let isValid = isValid xsd
    for xml in xmlSamples do
        //printfn "%A/n---------------------------------------------" xml
        xml.ToString() |> isValid |> should equal true

    let inferedTypeFromSchema = getInferedTypeFromSchema xsd
    //printfn "%A" inferedTypeFromSchema
    let inferedTypeFromSamples = getInferedTypeFromSamples xmlSamples
    //printfn "%A" inferedTypeFromSamples
    inferedTypeFromSchema, inferedTypeFromSamples



let check xsd xmlSamples =
    //printfn "checking schema and samples"
    let inferedTypeFromSchema, inferedTypeFromSamples = getInferedTypes xsd xmlSamples
    inferedTypeFromSchema |> should equal inferedTypeFromSamples
    

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
    let ty, _ = getInferedTypes xsd [| sample1; sample2 |]
    ty |> should equal (InferedType.Record (Some "foo", [], false))


[<Test>]
let ``names can be qualified``() =
    let xsd = """
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
      targetNamespace="http://test.001"
      elementFormDefault="qualified" attributeFormDefault="qualified">
      <xs:element name="foo">
        <xs:complexType>
          <xs:attribute name="bar" type="xs:string" use="required" form="qualified" />
          <xs:attribute name="baz" type="xs:int" use="required" form="unqualified" />
        </xs:complexType>
      </xs:element>
    </xs:schema>"""
    let sample = """<foo xmlns='http://test.001' xmlns:t='http://test.001' t:bar='aa' baz='2' />"""
    check xsd [| sample |] 

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
    //printfn "%A" inferedTypeFromSchema
    
    let xsd = """<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
      elementFormDefault="qualified" attributeFormDefault="unqualified">
        <xs:element name="Section" type="SectionType" />
        <xs:complexType name="SectionType">
          <xs:sequence>
            <xs:element name="Title" type="xs:string" />
            <xs:element name="Section" type="SectionType" minOccurs="0"/>
          </xs:sequence>
        </xs:complexType>
        </xs:schema>"""
    let inferedTypeFromSchema = getInferedTypeFromSchema xsd
    //printfn "%A" inferedTypeFromSchema
    inferedTypeFromSchema |> ignore


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


[<Test>]
let ``can import namespaces``() =
    let xsd = """
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
	    <xs:import namespace="http://www.w3.org/XML/1998/namespace" schemaLocation="http://www.w3.org/2001/03/xml.xsd"/>
        <xs:element name="test">
		    <xs:complexType>
			    <xs:attribute ref="xml:base"/>
		    </xs:complexType>
	    </xs:element>
    </xs:schema>"""
    let inferedTypeFromSchema = getInferedTypeFromSchema xsd
    //printfn "%A" inferedTypeFromSchema
    inferedTypeFromSchema |> ignore
    
[<Test>]
let ``simple elements can be nillable``() =
    let xsd = """
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
      elementFormDefault="qualified" attributeFormDefault="unqualified">
	    <xs:element name="author">
            <xs:complexType>
                <xs:sequence>
                    <xs:element name="name" type="xs:string" nillable="true" />
                    <xs:element name="surname" type="xs:string" />
                </xs:sequence>
            </xs:complexType>
        </xs:element>
    </xs:schema>"""
    let sample1 = """
    <author>
        <name>Stefano</name>
        <surname>Benny</surname>
    </author>
    """
    let sample2 = """
    <author>
        <name xsi:nil="true"
            xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"/>
        <surname>Benny</surname>
    </author>
    """
    check xsd [| sample1; sample2 |]



[<Test>]
let ``complex elements can be nillable``() =
    let xsd = """
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
      elementFormDefault="qualified" attributeFormDefault="unqualified">
	    <xs:element name="person">
            <xs:complexType>
                <xs:sequence>
	                <xs:element name="address" nillable="true">
                        <xs:complexType>
                            <xs:sequence>
                                <xs:element name="city" type="xs:string" />
                            </xs:sequence>
                        </xs:complexType>
                    </xs:element>
                </xs:sequence>
            </xs:complexType>
        </xs:element>
    </xs:schema>"""
    let sample1 = """
    <person>
        <address xsi:nil="true" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"/>
    </person>
    """
    let sample2 = """
    <person>
        <address>
            <city>Bologna</city>
        </address>
    </person>
    """
    check xsd [| sample1; sample2 |]

[<Test>]
let ``substitution groups are supported``() =
    let xsd = """
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
      elementFormDefault="qualified" attributeFormDefault="unqualified">
        <xs:element name="name" type="xs:string"/>
        <xs:element name="nome" substitutionGroup="name" type="xs:string" />
        <xs:element name="person">
          <xs:complexType>
            <xs:sequence>
              <xs:element ref="name"/>
            </xs:sequence>
          </xs:complexType>
        </xs:element>
        <xs:element name="persona" substitutionGroup="person"/>
    </xs:schema>"""
    let sample1 = """<person><name>Jim</name></person>"""
    let sample2 = """<persona><name>Jim</name></persona>"""
    let sample3 = """<person><nome>Jim</nome></person>"""
    let sample4 = """<persona><nome>Jim</nome></persona>"""

    check xsd [| sample1; sample2; sample3; sample4 |]

[<Test>]
let ``list fallback to string``() =
    let xsd = """
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
      elementFormDefault="qualified" attributeFormDefault="unqualified">
        <xs:simpleType name="listOfInts">
          <xs:list itemType="xs:int"/>
        </xs:simpleType>
        <xs:element name="nums">
          <xs:complexType>
		    <xs:simpleContent>
			  <xs:extension base="listOfInts" />
		    </xs:simpleContent>
		  </xs:complexType>
        </xs:element>
    </xs:schema>"""
    let sample = """<nums>40 50 60</nums>"""
    check xsd [| sample |]



open System.Xml.Schema

[<Test>]
let ``abstract elements can be recursive``() =

    // sample xsd with a recursive abstract element and substitution groups
    let formulaXsd = """
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
        elementFormDefault="qualified" attributeFormDefault="unqualified">
	    <xs:element name="Formula" abstract="true"/>
	    <xs:element name="Prop" type="xs:string" substitutionGroup="Formula"/>
	    <xs:element name="And" substitutionGroup="Formula">
		    <xs:complexType>
			    <xs:sequence>
				    <xs:element ref="Formula" minOccurs="2" maxOccurs="2"/>
			    </xs:sequence>
		    </xs:complexType>
	    </xs:element>
    </xs:schema>
    """

    let xsd = XsdParsing.parseSchema "" formulaXsd
    let elms = xsd.GlobalElements.Values |> XsdParsing.ofType<XmlSchemaElement>
    let andElm = elms |> Seq.filter (fun x -> x.Name = "And") |> Seq.exactlyOne
    let formElm = elms |> Seq.filter (fun x -> x.Name = "Formula") |> Seq.exactlyOne
    let formRefElm = // 'And' is a sequence whose only item is a reference to 'Formula'
        let formRefType = andElm.ElementSchemaType :?> XmlSchemaComplexType
        (formRefType.ContentTypeParticle :?> XmlSchemaSequence).Items 
        |> XsdParsing.ofType<XmlSchemaElement> 
        |> Seq.exactlyOne

    formRefElm.QualifiedName |> should equal formElm.QualifiedName 
    formRefElm.QualifiedName |> should equal formRefElm.RefName 
    formElm.ElementSchemaType |> should equal formRefElm.ElementSchemaType
    // this may be a bit surprising:
    formElm.IsAbstract |> should equal true
    formRefElm.IsAbstract |> should equal false

    let sample1 = """<Prop>p1</Prop>"""
    let sample2 = """
    <And>
        <Prop>p1</Prop>
        <And>
            <Prop>p2</Prop>
            <Prop>p3</Prop>
        </And>
    </And>
    """
    getInferedTypes formulaXsd [| sample1; sample2 |]
    |> ignore

