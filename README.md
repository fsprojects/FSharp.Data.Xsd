
# FSharp.Data.Xsd

[FSharp.Data.Xsd](http://giacomociti.github.io/FSharp.Data.Xsd/) augments the XML type provider 
from [F# Data](http://fsharp.github.io/FSharp.Data/) with schema support.

Please contribute, report issues and provide any kind of feedback.

## Project status and Roadmap

The XSD support is sufficiently complete. Before considering it ready for production, it is worth testing
it in real world use cases with as many schemas as possible.
Minor improvements (related to caching, error messages and support for primitive types) may be addressed
in the context of F# Data, because the end goal for this project is to get integrated into F# Data. 
In fact the implementation is directly based on its source code (thanks to [Paket GitHub references](https://fsprojects.github.io/Paket/github-dependencies.html)).
Moreover the design aims to follow the main ideas of F# Data, favoring ease of use and an *open world* approach to the shape of data.

XML is in decline but many enterprises still use it heavily. That's why a type provider with schema support may help popularizing F# as a great tool also for boring line of business applications (BLOBAs). 


## Design notes

main points (to be elaborated)
- Since BLOBAs often rely on big but simple schemas, there's no need to cover all the (huge) XSD specs.
- Many advanced (and sometimes controversial) XSD features have to do with flexibility and extensibility.
    When providing typed views on elements becomes too challenging, the underlying `XElement` is still available;
    
    
- complex types are not mapped to F# provided types, only elements are mapped.
- Code reuse is not the main concern.
- Type providers are expected to just *grab* external data as easily and safely as possible. 
- This is not a X/O binding.
- Provided types are not intended for domain modeling.
    
      
      
      

