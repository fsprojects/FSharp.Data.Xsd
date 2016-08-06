
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

type credit = XmlProvider<Schema = """c:\temp\schemas\ImportCreditRegistry_v.3.xsd""">

type hr = XmlProvider<Schema =  """C:\temp\Schemas\hr-xml\HumanResource.xsd""", 
    ResolutionFolder = """C:\temp\Schemas\hr-xml\""">


// uncommenting this we have squiggles with message "Recursive schemas are not supported yet."
type recursiveSchema = XmlProvider<Schema = """
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
  </xs:schema>""">


