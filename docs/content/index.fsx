(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin/"

(**
FSharp.Data.Xsd
======================

FSharp.Data.Xsd augments the XML type provider from [FSharp.Data](http://fsharp.github.io/FSharp.Data/) with schema support.

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

Design notes
-----------------------
 
The design aims to follow the main ideas of F# Data, favoring ease of use and an *open world* approach to the shape of data.
The XML Schema specification also provides a fair degree of [openness](http://docstore.mik.ua/orelly/xml/schema/ch13_02.htm) 
which may be difficult to handle in some data binding tools; but in F# Data, when providing typed views on elements becomes 
too challenging, the underlying `XElement` is still available.

An important decision is to focus on elements and not on complex types; while the latter may be valuable in schema design,
our goal is simply to obtain an easy and safe way to access xml data. In other words the provided types are not intended for
domain modeling (it's one of the very few cases where optional properties are preferred to sum types).
Hence, we do not provide types corresponding to complex types in a schema but only corresponding to elements (of course
the underlying complex types still affect the resulting provided types but this happens only implicitly)


The XML Provider of F# Data infers a type from sample documents: an instance of `InferedType` 
represents elements having a structure compatible with the given samples.
When a schema is available, we can use it (instead of samples) to derive an `InferedType` representing
elements valid according to the definitions in the given schema.

*The `InferedType` derived from a schema should be essentially the same as one
inferred from a significant set of valid samples.*

Adopting this perspective we can support XSD leveraging the existing functionalities.
The implementation uses a simplified XSD model to split the task of deriving an `InferedType`:

- element definitions in xsd files map to this simplified xsd model
- instances of this xsd model map to `InferedType`.





Samples & documentation
-----------------------

The library comes with comprehensible documentation. 
It can include tutorials automatically generated from `*.fsx` files in [the content folder][content]. 
The API reference is automatically generated from Markdown comments in the library implementation.

 * [Tutorial](tutorial.html) contains a further explanation of this library.

 * [API Reference](reference/index.html) contains automatically generated documentation for all types, modules
   and functions in the library. This includes additional brief samples on using most of the
   functions.
 
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
