#if INTERACTIVE
#r "../../../bin/FSharp.Data.Xsd.dll"
#r "../../../packages/test/NUnit/lib/nunit.framework.dll"
#r "../../../packages/test/FSUnit/lib/net45/FsUnit.NUnit.dll"
#r "System.Xml.Linq.dll"
#else
module TypeProviderTests
#endif

open NUnit.Framework
open FSharp.Data
open FsUnit
open System.Xml.Linq

//type schema = XmlProvider<Schema = """
//<schema xmlns="http://www.w3.org/2001/XMLSchema" targetNamespace="https://github.com/FSharp.Data/" xmlns:tns="https://github.com/FSharp.Data/" attributeFormDefault="unqualified" >
//  <element name="rootElement" type="root" />
//  <complexType name="root">
//    <sequence>
//      <element name="elem" type="string" >
//        <annotation>
//          <documentation>This is an identification of the preferred language</documentation>
//        </annotation>
//      </element>
//      <element name="elem1" type="tns:foo" />
//      <element name="choice" type="tns:bar" maxOccurs="2" />
//      <element name="anonymousTyped">
//        <complexType>
//          <sequence>
//            <element name="covert" type="boolean" />
//          </sequence>
//          <attribute name="attr" type="string" />
//          <attribute name="windy">
//            <simpleType>
//              <restriction base="string">
//                <maxLength value="10" />
//              </restriction>
//            </simpleType>
//          </attribute>
//        </complexType>
//      </element>
//    </sequence>
//  </complexType>
//  <complexType name="bar">
//    <choice>
//      <element name="language" type="string" >
//        <annotation>
//          <documentation>This is an identification of the preferred language</documentation>
//        </annotation>
//      </element>
//      <element name="country" type="integer" />
//      <element name="snur">
//        <complexType>
//          <sequence>
//            <element name ="baz" type ="string"/>
//          </sequence>
//        </complexType>
//      </element>
//    </choice>
//  </complexType>
//  <complexType name="foo">
//    <sequence>
//      <element name="fooElem" type="boolean" />
//      <element name="ISO639Code">
//        <annotation>
//          <documentation>This is an ISO 639-1 or 639-2 identifier</documentation>
//        </annotation>
//        <simpleType>
//          <restriction base="string">
//            <maxLength value="10" />
//          </restriction>
//        </simpleType>
//      </element>
//    </sequence>
//  </complexType>
//</schema>""">
//
//[<Test>]
//let ``Simple schema``() =
//    let xml = 
//         """<?xml version="1.0" encoding="utf-8"?>
//            <schema>
//              <root>
//                <elem>it starts with a number</elem>
//                <elem1>
//                  <fooElem>false</fooElem>
//                  <ISO639Code>dk-DA</ISO639Code>
//                </elem1>
//                <choice>
//                  <language>Danish</language>
//                </choice>
//                <choice>
//                  <country>1</country>
//                </choice>
//                <anonymousTyped attr="fish" windy="strong" >
//                  <covert>True</covert>
//                </anonymousTyped>
//              </root>
//            </schema>
//         """ 
//    let data = schema.Parse xml
//    let root = data.Root
//    let choices = root.Choices
//    choices.[1].Country.Value     |> should equal 1
//    choices.[0].Language.Value    |> should equal "Danish"
//    root.AnonymousTyped.Covert    |> should equal true
//    root.AnonymousTyped.Attr      |> should equal "fish"
//    root.AnonymousTyped.Windy     |> should equal "strong"
////    root.Metadata.TargetNamespace |> should equal "https://github.com/FSharp.Data/"
////    root.Metadata.TypeName        |> should equal "root"
//

//type schemaWithExtension = XmlProvider<Schema = """<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
//  <xs:element name="items" type="ItemsType"/>
//  <xs:complexType name="ItemsType">
//    <xs:choice minOccurs="0" maxOccurs="unbounded">
//      <xs:element name="hat" type="ProductType"/>
//      <xs:element name="umbrella" type="RestrictedProductType"/>
//      <xs:element name="shirt" type="ShirtType"/>
//    </xs:choice>
//  </xs:complexType>
//  <!--Empty Content Type-->
//  <xs:complexType name="ItemType" abstract="true">
//    
//  </xs:complexType>
//  <!--Empty Content Extension (with Attribute Extension)-->
//  <xs:complexType name="ProductType">
//        <xs:sequence>
//          <xs:element name="number" type="xs:integer"/>
//          <xs:element name="name" type="xs:string"/>
//          <xs:element name="description"
//                       type="xs:string" minOccurs="0"/>
//        </xs:sequence>
//  </xs:complexType>
//  <!--Complex Content Restriction-->
//  <xs:complexType name="RestrictedProductType">
//    <xs:complexContent>
//     <xs:restriction base="ProductType">
//        <xs:sequence>
//          <xs:element name="number" type="xs:integer"/>
//          <xs:element name="name" type="xs:token"/>
//        </xs:sequence>
//        <xs:attribute name="routingNum"
//                       type="xs:short" use="required"/>
//        <xs:attribute name="effDate"
//                       type="xs:date" default="1900-01-01"/>
//        <xs:attribute name="lang" use="prohibited"/>
//      </xs:restriction>
//    </xs:complexContent>
//  </xs:complexType>
//  <!--Complex Content Extension-->
//  <xs:complexType name="ShirtType">
//    <xs:complexContent>
//      <xs:extension base="ProductType">
//        <xs:choice maxOccurs="unbounded">
//          <xs:element name="size" type="SmallSizeType"/>
//          <xs:element name="color" type="ColorType"/>
//        </xs:choice>
//        <xs:attribute name="sleeve" type="xs:integer"/>
//      </xs:extension>
//    </xs:complexContent>
//  </xs:complexType>
//  <!--Simple Content Extension-->
//  <xs:complexType name="SizeType">
//    <xs:simpleContent>
//      <xs:extension base="xs:integer">
//        <xs:attribute name="system" type="xs:token"/>
//      </xs:extension>
//    </xs:simpleContent>
//  </xs:complexType>
//  <!--Simple Content Restriction-->
//  <xs:complexType name="SmallSizeType">
//    <xs:simpleContent>
//      <!--<xs:restriction base="SizeType">
//        <xs:minInclusive value="2"/>
//        <xs:maxInclusive value="6"/>
//        <xs:attribute  name="system" type="xs:token"
//                        use="required"/>
//      </xs:restriction>-->
//    </xs:simpleContent>
//  </xs:complexType>
//  <xs:complexType name="ColorType">
//    <xs:attribute name="value" type="xs:string"/>
//  </xs:complexType>
//</xs:schema>""">
////setting FailOnUnsupported = true should create a compile error
////because restrictions are not supported
//
//
//[<Test>]
//let ``Extension on complex types``() =
//    let xml = 
//        """<?xml version="1.0"?>
//           <items>
//             <!--You have a CHOICE of the next 3 items at this level-->
//             <hat routingNum="100" effDate="2008-09-29" lang="string">
//               <number>100</number>
//               <name>string</name>
//               <!--Optional:-->
//               <description>string</description>
//             </hat>
//             <umbrella routingNum="1" effDate="1900-01-01">
//               <number>100</number>
//               <name>token</name>
//             </umbrella>
//             <shirt routingNum="1" effDate="1900-01-01" sleeve="100">
//               <number>100</number>
//               <name>token</name>
//               <!--You have a CHOICE of the next 2 items at this level-->
//               <size system="token">6</size>
//               <color value="string"/>
//             </shirt>
//           </items>"""
//    let items = schemaWithExtension.ParseItems xml
//    items.Hat.IsSome             |> should equal true
//    items.Hat.Value.Number       |> should equal 100
//    items.Hat.Value.Name         |> should equal "string"
//    items.Shirt.Value.Sleeve     |> should equal 100
//    items.Umbrella.Value.EffDate |> should equal <| new System.DateTime(1900,1,1)
//
type simpleUnqualified = XmlProvider<Schema = """<schema xmlns="http://www.w3.org/2001/XMLSchema" 
  targetNamespace="https://github.com/FSharp.Data/" xmlns:tns="https://github.com/FSharp.Data/" 
  attributeFormDefault="unqualified" elementFormDefault="unqualified">
  <element name="A" type="tns:root" />
  <complexType name="root">
    <sequence>
      <element name="elem" type="string" />
      <element name="elem1" type="tns:foo" />
    </sequence>
  </complexType>
  <complexType name="foo">
    <sequence>
      <element name="fooElem" type="boolean" />
      <element name="s" type="string" />
    </sequence>
  </complexType>
</schema>""">

type simpleQualified = XmlProvider<Schema = """<schema xmlns="http://www.w3.org/2001/XMLSchema" 
  targetNamespace="https://github.com/FSharp.Data/" xmlns:tns="https://github.com/FSharp.Data/" 
  attributeFormDefault="unqualified" elementFormDefault="qualified">
  <element name="A" type="tns:root" />
  <complexType name="foo">
    <sequence>
      <element name="fooElem" type="boolean" />
      <element name="s" type="string" />
    </sequence>
  </complexType>
  <complexType name="root">
    <sequence>
      <element name="elem" type="string" />
      <element name="elem1" type="tns:foo" />
    </sequence>
  </complexType>

</schema>""">

// Qualified did not work out of order, & multiple references before definition also failed
type simpleQualifiedOrdered = XmlProvider<Schema = """<schema xmlns="http://www.w3.org/2001/XMLSchema" 
  targetNamespace="https://github.com/FSharp.Data/" xmlns:tns="https://github.com/FSharp.Data/" 
  attributeFormDefault="unqualified" elementFormDefault="qualified">
  <element name="A" type="tns:root" />
  <complexType name="root">
    <sequence>
      <element name="elem" type="string" />
      <element name="elem1" type="tns:foo" />
      <element name="elem2" type="tns:foo" />
    </sequence>
  </complexType>
  <complexType name="foo">
    <sequence>
      <element name="fooElem" type="boolean" />
      <element name="s" type="string" />
    </sequence>
  </complexType>
</schema>""">

let simpleQualifiedXml = 
     """<?xml version="1.0" encoding="utf-8"?>
          <fsd:root xmlns:fsd="https://github.com/FSharp.Data/">
            <fsd:elem>blah</fsd:elem>
            <fsd:elem1>
              <fsd:fooElem>false</fsd:fooElem>
              <fsd:s>zzz</fsd:s>
            </fsd:elem1>
          </fsd:root>
     """ 
let simpleUnqualifiedXml = """<?xml version="1.0" encoding="utf-8"?>
              <root xmlns:fsd="https://github.com/FSharp.Data/">
                <elem>blah</elem>
                <elem1>
                  <fooElem>false</fooElem>
                  <s>zzz</s>
                </elem1>
              </root>
         """ 
let simpleDefaultQualifiedXml = """<?xml version="1.0" encoding="utf-8"?>
              <root xmlns="https://github.com/FSharp.Data/">
                <elem>blah</elem>
                <elem1>
                  <fooElem>false</fooElem>
                  <s>zzz</s>
                </elem1>
              </root>
         """ 

[<Test>]
let ``Unqualified schema parses with unqualified elements``() =
    let data = simpleUnqualified.Parse(simpleUnqualifiedXml)
    data.Elem |> should equal "blah"
    data.Elem1.S |> should equal "zzz"

[<Test>]
let ``Unqualified schema fails with qualified elements``() =
    (fun () ->
          let root = simpleUnqualified.Parse(simpleQualifiedXml)
          root.Elem1.S |> ignore) 
    |> should throw typeof<System.Exception>

[<Test>]
let ``Unqualified schema fails with default-namespace qualified elements``() =
   (fun () -> 
          let root = simpleUnqualified.Parse(simpleDefaultQualifiedXml)
          root.Elem1.S |> ignore)
   |> should throw typeof<System.Exception>

[<Test>]
let ``Qualified schema parses with qualified elements``() =
    let data = simpleQualified.Parse(simpleQualifiedXml)
    data.Elem |> should equal "blah"
    data.Elem1.S |> should equal "zzz"

[<Test>]
let ``Qualified schema (reordered) parses with qualified elements``() =
    let data = simpleQualifiedOrdered.Parse(simpleQualifiedXml)
    data.Elem |> should equal "blah"
    data.Elem1.S |> should equal "zzz"


[<Test>]
let ``Qualified schema parses with default-namespace qualified elements``() =
    let data = simpleQualified.Parse(simpleDefaultQualifiedXml)
    data.Elem |> should equal "blah"
    data.Elem1.S |> should equal "zzz"

[<Test>]
let ``Qualified schema fails with unqualified elements``() =
    (fun () -> let root = simpleQualified.Parse(simpleUnqualifiedXml)
               root.Elem1.S |> ignore) 
    |> should throw typeof<System.Exception>


type unqualifiedWithOverride = XmlProvider<Schema = """<schema xmlns="http://www.w3.org/2001/XMLSchema" 
  targetNamespace="https://github.com/FSharp.Data/" xmlns:tns="https://github.com/FSharp.Data/" 
  attributeFormDefault="unqualified" elementFormDefault="unqualified">
  <element name="A" type="tns:root" />
  <complexType name="root">
    <sequence>
      <element name="elem" type="string" />
      <element form="qualified" name="elem1" type="tns:foo" />
    </sequence>
  </complexType>
  <complexType name="foo">
    <sequence>
      <element name="fooElem" type="boolean" />
      <element name="s" type="string" />
    </sequence>
  </complexType>
</schema>""">

[<Test>]
let ``Unqualified schema with qualified element fails if unqualified``() =
    (fun () -> let root = unqualifiedWithOverride.Parse(simpleUnqualifiedXml)
               root.Elem1.S |> ignore) 
    |> should throw typeof<System.Exception>

[<Test>]
let ``Unqualified schema with qualified element parses when qualified``() =
    let xml = """<?xml version="1.0" encoding="utf-8"?>
              <root xmlns:fsd="https://github.com/FSharp.Data/">
                <elem>blah</elem>
                <fsd:elem1>
                  <fooElem>false</fooElem>
                  <s>zzz</s>
                </fsd:elem1>
              </root>
         """ 
    let data = unqualifiedWithOverride.Parse(xml)
    data.Elem |> should equal "blah"
    data.Elem1.S |> should equal "zzz"

type qualifiedWithOverride = XmlProvider<Schema = """<schema xmlns="http://www.w3.org/2001/XMLSchema" 
  targetNamespace="https://github.com/FSharp.Data/" xmlns:tns="https://github.com/FSharp.Data/" 
  attributeFormDefault="unqualified" elementFormDefault="qualified">
  <element name="A" type="tns:root" />
  <complexType name="foo">
    <sequence>
      <element name="fooElem" type="boolean" />
      <element name="s" type="string" />
    </sequence>
  </complexType>
  <complexType name="root">
    <sequence>
      <element name="elem" type="string" />
      <element form="unqualified" name="elem1" type="tns:foo" />
    </sequence>
  </complexType>

</schema>""">

[<Test>]
let ``Qualified schema with unqualified element fails if qualified``() =
    (fun () -> let root = qualifiedWithOverride.Parse(simpleQualifiedXml)
               root.Elem1.S |> ignore) 
    |> should throw typeof<System.Exception>

[<Test>]
let ``Qualified schema with unqualified element parses when unqualified``() =
    let xml = """<?xml version="1.0" encoding="utf-8"?>
          <fsd:root xmlns:fsd="https://github.com/FSharp.Data/">
            <fsd:elem>blah</fsd:elem>
            <elem1>
              <fsd:fooElem>false</fsd:fooElem>
              <fsd:s>zzz</fsd:s>
            </elem1>
          </fsd:root>
     """ 
    let data = qualifiedWithOverride.Parse(xml)
    data.Elem |> should equal "blah"
    data.Elem1.S |> should equal "zzz"

type undeclaredTargetNamespace = XmlProvider<Schema = """<xsd:schema xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <xsd:element name="A" type="root" />
  <xsd:complexType name="root">
    <xsd:sequence>
      <xsd:element name="elem" type="xsd:string" />
      <xsd:element name="elem1" type="foo" />
    </xsd:sequence>
  </xsd:complexType>
  <xsd:complexType name="foo">
    <xsd:sequence>
      <xsd:element name="fooElem" type="xsd:boolean" />
      <xsd:element name="s" type="xsd:string" />
    </xsd:sequence>
  </xsd:complexType>
</xsd:schema>""">


[<Test>]
let ``Schema with undeclared target namespace``() =
    let xml =  """<?xml version="1.0" encoding="utf-8"?>
              <root>
                <elem>blah</elem>
                <elem1>
                  <fooElem>false</fooElem>
                  <s>zzz</s>
                </elem1>
              </root>
         """ 
    let data = undeclaredTargetNamespace.Parse(xml)
    data.Elem |> should equal "blah"
    data.Elem1.S |> should equal "zzz"


type defaultTargetNamespace = XmlProvider<Schema = """<xsd:schema xmlns:xsd="http://www.w3.org/2001/XMLSchema" 
  targetNamespace="https://github.com/FSharp.Data/" xmlns="https://github.com/FSharp.Data/" 
  attributeFormDefault="unqualified" elementFormDefault="qualified">
  <xsd:element name="A" type="root" />
  <xsd:complexType name="root">
    <xsd:sequence>
      <xsd:element name="elem" type="xsd:string" />
      <xsd:element name="elem1" type="foo" />
    </xsd:sequence>
  </xsd:complexType>
  <xsd:complexType name="foo">
    <xsd:sequence>
      <xsd:element name="fooElem" type="xsd:boolean" />
      <xsd:element name="s" type="xsd:string" />
    </xsd:sequence>
  </xsd:complexType>
</xsd:schema>""">


[<Test>]
let ``Schema with default target namespace``() =
    let data = defaultTargetNamespace.Parse(simpleQualifiedXml)
    data.Elem |> should equal "blah"
    data.Elem1.S |> should equal "zzz"