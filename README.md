
# FSharp.Data.Xsd

[FSharp.Data.Xsd](http://giacomociti.github.io/FSharp.Data.Xsd/) augments the XML type provider 
from [F# Data](http://fsharp.github.io/FSharp.Data/) with schema support.

Please contribute, report issues and provide any kind of feedback.

## Roadmap

The end goal for this project is to get integrated into F# Data. 
In fact the implementation is directly based on its source code (thanks to [Paket GitHub references](https://fsprojects.github.io/Paket/github-dependencies.html)).
Moreover the design aims to follow the main ideas of F# Data, favoring ease of use and an *open world* approach to the shape of data.

XML is in decline but many enterprises still use it heavily. That's why a type provider with schema support may help popularizing F# as a great tool also for boring line of business applications (BLOBAs). 
Maybe this project should be marked as experimental according to the [recommended guidelines](http://fsharp.github.io/2014/09/19/fsharp-libraries.html), but hopefully it's not far from being production ready.
Making it early available may speed up things; before XML disappears :)


## Design notes
 
The XML Provider of F# Data infers a type from sample documents: an instance of `InferedType` 
represents elements having a structure compatible with the given samples.
When a schema is available we can use it (instead of samples) to derive an `InferedType` representing
elements valid according to the definitions in the given schema.

*The `InferedType` derived from a schema should be essentialy the same as one
infered from a significant set of valid samples.*

Adopting this perspective we can support XSD leveraging the existing functionalities.
The implementation uses a simplfied XSD model to split the task of deriving an `InferedType`:
- element definitions in xsd files map to this simplified xsd model
- instances of this xsd model map to `InferedType`.



main points (to be elaborated)
- Since BLOBAs often rely on big but simple schemas, there's no need to cover all the (huge) XSD specs.
- Many advanced (and sometimes controversial) XSD features have to do with flexibility and extensibility.
    When providing typed views on elements becomes too challenging, the underlying `XElement` is still available;
    
    
- complex types are not mapped to F# provided types, only elements are mapped.
- Code reuse is not the main concern.
- Type providers are expected to just *grab* external data as easily and safely as possible. 
- This is not a X/O binding.
- Provided types are not intended for domain modeling.
    
      
      
      

