// the sole purpose of this test is to check the package downloaded from NuGet

open FSharp.Data

type Person = XmlProvider<Schema = """
  <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" 
    elementFormDefault="qualified" attributeFormDefault="unqualified">
    <xs:element name="person">
      <xs:complexType>
        <xs:sequence>
          <xs:element name="surname" type="xs:string"/>
          <xs:element name="birthDate" type="xs:date"/>
        </xs:sequence>
      </xs:complexType>
    </xs:element>
  </xs:schema>""">

[<EntryPoint>]
let main argv = 
    let turing = Person.Parse """
    <person>
      <surname>Turing</surname>
      <birthDate>1912-06-23</birthDate>
    </person>
    """ 
    printfn "%s was born in %d" turing.Surname turing.BirthDate.Year
    0
