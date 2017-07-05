
(**
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
the underlying complex types still affect the resulting provided types but this happens only implicitly).


The XML Provider of F# Data infers a type from sample documents: an instance of `InferedType` 
represents elements having a structure compatible with the given samples.
When a schema is available, we can use it (instead of samples) to derive an `InferedType` representing
elements valid according to the definitions in the given schema.

*The `InferedType` derived from a schema should be essentially the same as one
inferred from a significant set of valid samples.*

Adopting this perspective we can leverage existing functionalities to support XSD.
The implementation uses a simplified XSD model to split the task of deriving an `InferedType`:

- element definitions in xsd files map to this simplified xsd model
- instances of this xsd model map to `InferedType`.

*)