(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin/"

(**
Tutorial
========================

Once the needed libraries have been referenced and the namespace is open:

*)

#r "System.Xml.Linq"
#r "FSharp.Data.Xsd.dll"

open FSharp.Data

(**
an XML schema can be specified either as plain text:
*)

type Foo = XmlProvider<Schema = """
  <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
    elementFormDefault="qualified" attributeFormDefault="unqualified">
    <xs:element name="foo">
      <xs:complexType>
        <xs:attribute name="bar" type="xs:string" use="required" />
        <xs:attribute name="baz" type="xs:int" />
      </xs:complexType>
    </xs:element>
  </xs:schema>""">

(**

or as a file:

*)

type Foo' = XmlProvider<Schema="c:/temp/foo.xsd">

(**

When the file includes other schema files, the `ResolutionFolder` parameter can help locating them:

*)

type Foo'' = XmlProvider<Schema="foo.xsd", ResolutionFolder="c:/temp">

(**

A schema may define multiple root elements:

*)

type TwoRoots = XmlProvider<Schema = """
  <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
    elementFormDefault="qualified" attributeFormDefault="unqualified">
    <xs:element name="root1">
      <xs:complexType>
        <xs:attribute name="foo" type="xs:string" use="required" />
        <xs:attribute name="fow" type="xs:int" />
      </xs:complexType>
    </xs:element>
    <xs:element name="root2">
      <xs:complexType>
        <xs:attribute name="bar" type="xs:string" use="required" />
        <xs:attribute name="baz" type="xs:date" use="required" />
      </xs:complexType>
    </xs:element>
  </xs:schema>    
""">
(**

In this case the provided type has an optional property for each alternative:

*)

let e1 = TwoRoots.Parse "<root1 foo='aa' fow='2' />"
match e1.Root1, e1.Root2 with
| Some x, None -> 
    assert(x.Foo = "aa")
    assert(x.Fow = Some 2)
| _ -> failwith "Unexpected"
let e2 = TwoRoots.Parse "<root2 bar='aa' baz='2017-12-22' />"
match e2.Root1, e2.Root2 with
| None, Some x -> 
    assert(x.Bar = "aa")
    assert(x.Baz = System.DateTime(2017, 12, 22))
| _ -> failwith "Unexpected"
