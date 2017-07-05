(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin/"

#r "System.Xml.Linq.dll"
#r "FSharp.Data.Xsd.dll"

open FSharp.Data

(*** hide ***)
module Substitutions =

(**
Substitution Groups
========================

XML Schema provides various extensibility mechanisms. The following example
is a terse summary mixing substitution groups with abstract recursive definitions. 
*)

    type T = XmlProvider<Schema = """
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
        </xs:schema>""">

    let formula = T.Parse """
        <And>
            <Prop>p1</Prop>
            <And>
                <Prop>p2</Prop>
                <Prop>p3</Prop>
            </And>
        </And>
        """
    assert(formula.Props.[0] = "p1")
    assert(formula.Ands.[0].Props.[0] = "p2")
    assert(formula.Ands.[0].Props.[1] = "p3")
    
(**
Substitution groups are like choices, and the type provider produces an optional
property for each alternative.
*)