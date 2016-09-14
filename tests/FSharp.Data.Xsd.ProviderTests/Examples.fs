module Tests 

open FSharp.Data
open System.Xml.Linq
open NUnit.Framework
open FsUnit

type elemWithAttrs = XmlProvider<Schema = """
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
      elementFormDefault="qualified" attributeFormDefault="unqualified">
      <xs:element name="foo">
	    <xs:complexType>
		  <xs:attribute name="bar" type="xs:string" use="required" />
          <xs:attribute name="baz" type="xs:int" />
	    </xs:complexType>
	  </xs:element>
    </xs:schema>""">

[<Test>]
let ``attributes are parsed``() =
    let elm = elemWithAttrs.Parse "<foo bar='aa' baz='2' />"
    elm.Bar |> should equal "aa"
    elm.Baz |> should equal (Some 2)
    let elm = elemWithAttrs.Parse "<foo bar='aa' />"
    elm.Bar |> should equal "aa"
    elm.Baz |> should equal None
    

type twoElems = XmlProvider<Schema = """
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
    elementFormDefault="qualified" attributeFormDefault="unqualified">
    <xs:element name="foo">
	    <xs:complexType>
		    <xs:attribute name="bar" type="xs:string" use="required" />
            <xs:attribute name="baz" type="xs:int" />
	    </xs:complexType>
	</xs:element>
    <xs:element name = 'azz'>
        <xs:complexType>
		    <xs:attribute name="foffolo" type="xs:string" use="required" />
            <xs:attribute name="fuffola" type="xs:date" />
	    </xs:complexType>
    </xs:element>
    </xs:schema>    
""">

[<Test>]
let ``multiple root elements are handled``() =
    let elm = twoElems.Parse "<foo bar='aa' baz='2' />"
    match elm.Foo, elm.Azz with
    | Some x, None -> 
        x.Bar |> should equal "aa"
        x.Baz |> should equal (Some 2)
    | _ -> failwith "Invalid"
    let elm = twoElems.Parse "<azz foffolo='aa' fuffola='2017-12-22' />"
    match elm.Foo, elm.Azz with
    | None, Some x -> 
        x.Foffolo |> should equal "aa"
        x.Fuffola |> should equal (Some <| System.DateTime(2017, 12, 22))
    | _ -> failwith "Invalid"




type attrsAndSimpleContent = XmlProvider<Schema = """
  <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
    elementFormDefault="qualified" attributeFormDefault="unqualified">
      <xs:element name="foo">
		<xs:complexType>
		  <xs:simpleContent>
			  <xs:extension base="xs:date">
				  <xs:attribute name="bar" type="xs:string" use="required"/>
				  <xs:attribute name="baz" type="xs:int"/>
			  </xs:extension>
		  </xs:simpleContent>
		</xs:complexType>
	  </xs:element>
  </xs:schema>""">

[<Test>]
let ``element with attributes can have simple content``() =
    let date = System.DateTime(1957, 8, 13)
    let foo = attrsAndSimpleContent.Parse("""<foo bar="hello">1957-08-13</foo>""")
    foo.Value |> should equal date
    foo.Bar |> should equal "hello"
    foo.Baz |> should equal None
    let foo = attrsAndSimpleContent.Parse("""<foo bar="hello" baz="2">1957-08-13</foo>""")
    foo.Value |> should equal date
    foo.Bar |> should equal "hello"
    foo.Baz |> should equal (Some 2)



type untypedElement = XmlProvider<Schema = """
  <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
    elementFormDefault="qualified" attributeFormDefault="unqualified">
      <xs:element name="foo" />
  </xs:schema>""">

[<Test>]
let ``untyped elements have only the XElement property``() =
  let foo = untypedElement.Parse """
  <foo>
    <anything />
    <greetings>hi</greetings>
  </foo>"""
  printfn "%A" foo.XElement
  foo.XElement.Element(XName.Get "greetings").Value
  |> should equal "hi"


type wildcard = XmlProvider<Schema = """ <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
      elementFormDefault="qualified" attributeFormDefault="unqualified">
    <xs:element name="foo">
      <xs:complexType>
        <xs:sequence>
          <xs:element name="id" type="xs:string"/>
          <xs:any minOccurs="0"/>
        </xs:sequence>
      </xs:complexType>
    </xs:element>
    </xs:schema>    
    """>

[<Test>]
let ``wildcard elements have only the XElement property``() =
  let foo = wildcard.Parse """
  <foo>
    <id>XYZ</id>
    <anything name='abc' />
  </foo>"""
  printfn "%A" foo.XElement
  foo.Id |> should equal "XYZ"
  foo.XElement.Element(XName.Get "anything").FirstAttribute.Value
  |> should equal "abc"

type recursiveElements = XmlProvider<Schema = """ 
     <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
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
    </xs:schema>
    """>

[<Test>]
let ``recursive elements are supported``() =
  let doc = recursiveElements.Parse """
    <italic>
      <bold></bold>
      <underline></underline>
      <bold>
        <italic />
        <bold />
      </bold>
    </italic>
    """ 
  printfn "%A" doc.XElement
  match doc.Bold, doc.Italic, doc.Underline with
  | None, Some x, None -> 
    x.Bolds.Length |> should equal 2
    x.Italics.Length |>should equal 0
    x.Underlines.Length |> should equal 1
    x.Bolds.[1].Bolds.Length |> should equal 1
        
  | _ -> failwith "unexpected"
  

 

type elmWithChildChoice = XmlProvider<Schema = """
  <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
    elementFormDefault="qualified" attributeFormDefault="unqualified">
      <xs:element name="foo">
		<xs:complexType>
			<xs:choice>
				<xs:element name="bar" type="xs:int"/>
				<xs:element name="baz" type="xs:date"/>
			</xs:choice>
		</xs:complexType>
	</xs:element>
  </xs:schema>""">

[<Test>]
let ``choice makes properties optional``() =
    let foo = elmWithChildChoice.Parse "<foo><baz>1957-08-13</baz></foo>"
    foo.Bar |> should equal None
    foo.Baz |> should equal (Some <| System.DateTime(1957, 8, 13))  
    let foo = elmWithChildChoice.Parse "<foo><bar>5</bar></foo>"
    foo.Bar |> should equal (Some 5)
    foo.Baz |> should equal None


type elmWithMultipleChildElements = XmlProvider<Schema = """
  <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
    elementFormDefault="qualified" attributeFormDefault="unqualified">
      <xs:element name="foo">
		<xs:complexType>
			<xs:sequence minOccurs='0' >
				<xs:element name="bar" type="xs:int" maxOccurs='unbounded' />
				<xs:element name="baz" type="xs:date"/>
			</xs:sequence>
		</xs:complexType>
	</xs:element>
  </xs:schema>""">

[<Test>]
let ``multiple child elements become an array``() =
    let foo = elmWithMultipleChildElements.Parse """
    <foo>
        <bar>42</bar>
        <bar>43</bar>
        <baz>1957-08-13</baz>
    </foo>"""
    foo.Bars.Length |> should equal 2
    foo.Bars.[0] |> should equal 42
    foo.Bars.[1] |> should equal 43
    foo.Baz |> should equal (Some(System.DateTime(1957, 08, 13)))
        

type substGroup = XmlProvider<Schema = """
  <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
    elementFormDefault="qualified" attributeFormDefault="unqualified">
        <xs:element name="name" type="xs:string"/>
        <xs:element name="navn" substitutionGroup="name"/>
        <xs:complexType name="custinfo">
          <xs:sequence>
            <xs:element ref="name"/>
          </xs:sequence>
        </xs:complexType>
        <xs:element name="customer" type="custinfo"/>
        <xs:element name="kunde" substitutionGroup="customer"/>
  </xs:schema>""">

[<Test>]
let ``substitution groups are like choices``() =
  let doc = substGroup.Parse "<kunde><name>hello</name></kunde>"
  match doc.Customer, doc.Kunde with
  | None, Some x -> 
    x.Name |> should equal (Some "hello")
    x.Navn |> should equal None
  | _ -> failwith "unexpected"
  
  let doc = substGroup.Parse "<kunde><navn>hello2</navn></kunde>"
  match doc.Customer, doc.Kunde with
  | None, Some x -> 
    x.Navn |> should equal (Some "hello2")
    x.Name |> should equal None    
  | _ -> failwith "unexpected"



type simpleSchema = XmlProvider<Schema = """
<schema xmlns="http://www.w3.org/2001/XMLSchema" targetNamespace="https://github.com/FSharp.Data/" 
  xmlns:tns="https://github.com/FSharp.Data/" attributeFormDefault="unqualified" >
  <complexType name="root">
    <sequence>
      <element name="elem" type="string" >
        <annotation>
          <documentation>This is an identification of the preferred language</documentation>
        </annotation>
      </element>
      <element name="elem1" type="tns:foo" />
      <element name="choice" type="tns:bar" maxOccurs="2" />
      <element name="anonymousTyped">
        <complexType>
          <sequence>
            <element name="covert" type="boolean" />
          </sequence>
          <attribute name="attr" type="string" />
          <attribute name="windy">
            <simpleType>
              <restriction base="string">
                <maxLength value="10" />
              </restriction>
            </simpleType>
          </attribute>
        </complexType>
      </element>
    </sequence>
  </complexType>
  <complexType name="bar">
    <choice>
      <element name="language" type="string" >
        <annotation>
          <documentation>This is an identification of the preferred language</documentation>
        </annotation>
      </element>
      <element name="country" type="integer" />
      <element name="snur">
        <complexType>
          <sequence>
            <element name ="baz" type ="string"/>
          </sequence>
        </complexType>
      </element>
    </choice>
  </complexType>
  <complexType name="foo">
    <sequence>
      <element name="fooElem" type="boolean" />
      <element name="ISO639Code">
        <annotation>
          <documentation>This is an ISO 639-1 or 639-2 identifier</documentation>
        </annotation>
        <simpleType>
          <restriction base="string">
            <maxLength value="10" />
          </restriction>
        </simpleType>
      </element>
    </sequence>
  </complexType>
  <element name="rootElm" type="tns:root" />
</schema>""">

[<Test>]
let ``simple schema``() =
    let xml = 
         """<?xml version="1.0" encoding="utf-8"?>
              <tns:rootElm xmlns:tns="https://github.com/FSharp.Data/">
                <elem>it starts with a number</elem>
                <elem1>
                  <fooElem>false</fooElem>
                  <ISO639Code>dk-DA</ISO639Code>
                </elem1>
                <choice>
                  <language>Danish</language>
                </choice>
                <choice>
                  <country>1</country>
                </choice>
                <anonymousTyped attr="fish" windy="strong" >
                  <covert>True</covert>
                </anonymousTyped>
              </tns:rootElm>
         """ 
    let root = simpleSchema.Parse xml
    let choices = root.Choices
    choices.[1].Country.Value     |> should equal "1" // an integer is mapped to string (BigInteger would be better)
    choices.[0].Language.Value    |> should equal "Danish"
    root.AnonymousTyped.Covert    |> should equal true
    root.AnonymousTyped.Attr      |> should equal (Some "fish")
    root.AnonymousTyped.Windy     |> should equal (Some "strong")

type Nums = XmlProvider<Schema = """
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
    elementFormDefault="qualified" >
    <xs:element name='A'>
        <xs:complexType>
            <xs:attribute name='integer' type='xs:integer' />
            <xs:attribute name='int' type='xs:int' />
            <xs:attribute name='long' type='xs:long' />
            <xs:attribute name='decimal' type='xs:decimal' />
            <xs:attribute name='float' type='xs:float' />
            <xs:attribute name='double' type='xs:double' />
        </xs:complexType>
    </xs:element>
</xs:schema>
""">

[<Test>]
let ``numeric types are partially supported``() =
    Nums.Parse("<A integer='2' />").Integer |> should equal (Some "2")
    Nums.Parse("<A int='2' />").Int |> should equal (Some 2) // int32
    Nums.Parse("<A long='2' />").Long |> should equal (Some 2L) // int64
    Nums.Parse("<A decimal='2' />").Decimal |> should equal (Some 2M) // decimal
    Nums.Parse("<A float='2' />").Float |> should equal (Some "2") // should be float32
    Nums.Parse("<A double='2' />").Double |> should equal (Some 2.0) // float





    

