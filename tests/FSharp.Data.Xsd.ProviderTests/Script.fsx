
#r "../../bin/FSharp.Data.Xsd.dll"
#r "System.Xml.Linq.dll"

open FSharp.Data

type foo = XmlProvider<Schema = """
  <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
    elementFormDefault="qualified" attributeFormDefault="unqualified">
    <xs:element name="foo">
      <xs:complexType>
        <xs:attribute name="bar" type="xs:string" use="required" />
        <xs:attribute name="baz" type="xs:int" />
      </xs:complexType>
    </xs:element>
  </xs:schema>""">

let fooElement = foo.Parse("<foo bar='aa' />")
printfn "%s" fooElement.Bar



type xsd = XmlProvider<Schema = """
      <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
        elementFormDefault="qualified" attributeFormDefault="unqualified">
          <xs:element name="foo" type="SectionType" />
        <xs:complexType name="SectionType">
          <xs:sequence>
            <xs:element name="Title" type="xs:string" />
            <xs:element name="Section" type="SectionType" minOccurs="0"/>
          </xs:sequence>
        </xs:complexType>
      </xs:schema>
    """>

let doc = xsd.Parse """
    <foo>
      <Title>Hello world</Title>
      <Section>
        <Title>Section 1</Title>
        <Section>
          <Title>Section 2</Title>
          <Section>
            <Title>Section 3</Title>
          </Section>
        </Section>
      </Section>
    </foo>
"""
printfn "%s" doc.Title
let inner = doc.Section.Value
printfn "%A" inner.XElement

type subst = XmlProvider<Schema = """
    <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
        elementFormDefault="qualified" attributeFormDefault="unqualified">
        <xs:element name="Root">
            <xs:complexType>
			    <xs:sequence>
				    <xs:element ref="Formula" />
			    </xs:sequence>
		    </xs:complexType>
        </xs:element>
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
""">

let doc2 = subst.Parse """
<Root>
<And>
    <And>
        <Prop>p</Prop>
        <Prop>q</Prop>
    </And>
    <Prop>r</Prop>
</And>
</Root>
"""

match doc2.Formula, doc2.And, doc2.Root with
| None, None, Some root ->
    match root.And, root.Prop with
    | None, Some prop -> 
        printfn "%s" prop
    | Some andElem, None -> 
        // 'and' is a recursive element, hence we can only use the dynaic api
        let inner = andElem.XElement.Elements() |> Seq.map (fun x -> subst.Parse (x.ToString()))
        for item in inner do
            printfn "item %A" item
    | _ -> failwith "expected either and or prop"
| _ -> failwith "expected root"











