(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin/"

#r "System.Xml.Linq.dll"
#r "FSharp.Data.Xsd.dll"

open FSharp.Data

(*** hide ***)
module Sequences =

(**
Sequence and Choice
========================

A `sequence` is the most common way of structuring elements in a schema.

the following xsd defines `foo` as a sequence made of an arbitrary number 
of `bar` elements followed by a single `baz` element.
*)

    type T = XmlProvider<Schema = """
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
          elementFormDefault="qualified" attributeFormDefault="unqualified">
            <xs:element name="foo">
              <xs:complexType>
                <xs:sequence>
                  <xs:element name="bar" type="xs:int" maxOccurs="unbounded" />
                  <xs:element name="baz" type="xs:date" minOccurs="1" />
                </xs:sequence>
              </xs:complexType>
            </xs:element>
        </xs:schema>""">

(**
here a valid xml element is parsed as an instance of the provided type, with two properties corresponding 
to `bar`and `baz` elements, where the former is an array in order to hold multiple elements:
*)

    let foo = T.Parse """
    <foo>
        <bar>42</bar>
        <bar>43</bar>
        <baz>1957-08-13</baz>
    </foo>"""

    assert(foo.Bars.[0] = 42)
    assert(foo.Bars.[1] = 43)
    assert(foo.Baz = System.DateTime(1957, 08, 13))

(*** hide ***)
module Choices =

(**
Instead of a sequence we may have a `choice`:
*)
    type T = XmlProvider<Schema = """
        <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
          elementFormDefault="qualified" attributeFormDefault="unqualified">
            <xs:element name="foo">
              <xs:complexType>
                <xs:choice>
                  <xs:element name="bar" type="xs:int" maxOccurs="unbounded" />
                  <xs:element name="baz" type="xs:date" minOccurs="1" />
                </xs:choice>
              </xs:complexType>
            </xs:element>
        </xs:schema>""">
(**
although a choice is akin to a union type in F#, the provided type still has
properties for `bar` and `baz` directly available on the `foo` object; in fact
the properties representing alternatives in a choice are simply made optional 
(notice that for arrays this is not even necessary because an array can be empty).
This decision is due to technical limitations (discriminated unions are not supported
in type providers) but also preferred by the authors because it improves discoverability:
intellisense can show both alternatives. There is a lack of precision but this is not the main goal.
*)
    let foo = T.Parse """
    <foo>
      <baz>1957-08-13</baz>
    </foo>"""
    assert(foo.Bars.Length = 0)
    assert(foo.Baz = Some (System.DateTime(1957, 08, 13)))

(**
Another xsd construct to model the content of an element is `all`, which is used less often and
is like a sequence where the order of elements does not matter. The corresponding provided type
in fact is essentially the same as for a sequence.

*)