(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin/"

(**
FSharp.Data.Xsd
======================

FSharp.Data.Xsd augments the XML type provider of [FSharp.Data](http://fsharp.github.io/FSharp.Data/) with schema support.

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The FSharp.Data.Xsd library can be <a href="https://nuget.org/packages/FSharp.Data.Xsd">installed from NuGet</a>:
      <pre>PM> Install-Package FSharp.Data.Xsd</pre>
    </div>
  </div>
  <div class="span1"></div>
</div>

Example
-------

The `Schema` parameter can be used (instead of `Sample`) to specify an XML schema.
The value of the parameter can be either the name of a schema file or 
plain text like in the following example:

*)

#r "System.Xml.Linq.dll"
#r "FSharp.Data.Xsd.dll"

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

let turing = Person.Parse """
  <person>
    <surname>Turing</surname>
    <birthDate>1912-06-23</birthDate>
  </person>
  """

printfn "%s was born in %d" turing.Surname turing.BirthDate.Year


(**

The properties of the provided type are derived from the schema instead of being inferred from samples.


Samples & documentation
-----------------------

The library comes with comprehensible documentation. 
It can include tutorials automatically generated from `*.fsx` files in [the content folder][content]. 

 * [Tutorial](tutorial.html) contains a further explanation of this library.

 * [Sequence and Choice](sequences.html) shows the library in action with the basic structures in a schema.

 * [Substitution Groups](substitutions.html) explains how to deal with extensible and abstract schemas.

 * [Design Notes](design.html) provide some context for implementation decisions.
 
Contributing and copyright
--------------------------

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork 
the project and submit pull requests. If you're adding a new public API, please also 
consider adding [samples][content] that can be turned into a documentation. You might
also want to read the [library design notes][readme] to understand how it works.

The library is available under Public Domain license, which allows modification and 
redistribution for both commercial and non-commercial purposes. For more information see the 
[License file][license] in the GitHub repository. 

  [content]: https://github.com/giacomociti/FSharp.Data.Xsd/tree/master/docs/content
  [gh]: https://github.com/giacomociti/FSharp.Data.Xsd
  [issues]: https://github.com/giacomociti/FSharp.Data.Xsd/issues
  [readme]: https://github.com/giacomociti/FSharp.Data.Xsd/blob/master/README.md
  [license]: https://github.com/giacomociti/FSharp.Data.Xsd/blob/master/LICENSE.txt
*)
