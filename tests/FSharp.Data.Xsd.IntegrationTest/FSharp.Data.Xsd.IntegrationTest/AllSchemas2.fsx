
//#r"../packages/FSharp.Data.Xsd.0.0.5/lib/net45/FSharp.Data.Xsd.dll"
#r"../../../bin/FSharp.Data.Xsd.dll"
#r"System.Xml"
#r"System.Xml.Linq"

open FSharp.Data
open System.Xml.Linq
type t = XmlProvider<Schema="""http://www.beniculturali.it/mibac/xsd/MibacSchema-1.2.xsd""">

// dbunico20.beniculturali.it
// http://151.12.58.144:8080
[<Literal>]
let req ="""http://dbunico20.beniculturali.it/DBUnicoManagerWeb/dbunicomanager/searchPlace?regione=Toscana&provincia=Livorno"""

type tt = XmlProvider<req>
let xml = tt.GetSample()

xml.Mibacs.Length

let xml = t.Load req
xml.XElement.Elements() |> Seq.map (fun x -> x.Name) |> Seq.take 3
xml.XElement.Element(XName.Get "{http://www.beniculturali.it/MibacSchema}Query")
xml.Query

let valid = xml.Mibacs |> Array.filter (fun x -> x.Metainfo.Workflow.Stato = Some "Validato")
let foo =
    valid 
    |> Array.choose (fun x -> x.Luogodellacultura)
    |> Array.map (fun z -> z.Proprieta)
    


type t1 = XmlProvider<Schema ="ImportCreditRegistry_v.3.xsd", ResolutionFolder="""c:\temp\schemas""">
type t2 = XmlProvider<Schema ="xmlopts.xsd", ResolutionFolder="""c:\temp\schemas\altova\files""">
type t3 = XmlProvider<Schema ="config.xsd", ResolutionFolder="""c:\temp\schemas\altova\files\raptorxml""">
type t5 = XmlProvider<Schema ="parameters.xsd", ResolutionFolder="""c:\temp\schemas\altova\files\raptorxml""">
type t6 = XmlProvider<Schema ="cmlCore.xsd", ResolutionFolder="""c:\temp\schemas\cml\files""">
type t45 = XmlProvider<Schema ="container.xsd", ResolutionFolder="""c:\temp\schemas\epub\files""">
//*DUP* type t46 = XmlProvider<Schema ="opf.xsd", ResolutionFolder="""c:\temp\schemas\epub\files""">
//*DUP* type t47 = XmlProvider<Schema ="opf2.xsd", ResolutionFolder="""c:\temp\schemas\epub\files""">
//type t48 = XmlProvider<Schema ="ApplicationAcknowledgement.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t49 = XmlProvider<Schema ="AssessmentCancelRequest.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t50 = XmlProvider<Schema ="AssessmentCatalog.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t51 = XmlProvider<Schema ="AssessmentCatalogQuery.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t52 = XmlProvider<Schema ="AssessmentOrderAcknowledgement.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t53 = XmlProvider<Schema ="AssessmentOrderRequest.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t54 = XmlProvider<Schema ="AssessmentResult.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t55 = XmlProvider<Schema ="AssessmentStatusRequest.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t56 = XmlProvider<Schema ="Assignment.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t57 = XmlProvider<Schema ="BackgroundCheck.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t58 = XmlProvider<Schema ="BackgroundCheckStatusRequest.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t59 = XmlProvider<Schema ="BackgroundReports.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t60 = XmlProvider<Schema ="Candidate.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t61 = XmlProvider<Schema ="Credit.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t62 = XmlProvider<Schema ="CustomerReportingRequirements.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t63 = XmlProvider<Schema ="Enrollment.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t64 = XmlProvider<Schema ="EPMDevelopmentPlan.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t65 = XmlProvider<Schema ="EPMObjectivesPlan.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t66 = XmlProvider<Schema ="EPMObjectivesResult.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t67 = XmlProvider<Schema ="EPMRemunerationResult.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t68 = XmlProvider<Schema ="EPMResult.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t69 = XmlProvider<Schema ="EPMSummaryResult.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t70 = XmlProvider<Schema ="ExerciseConfirmation.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t71 = XmlProvider<Schema ="ExerciseRequest.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t72 = XmlProvider<Schema ="Grant.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t73 = XmlProvider<Schema ="HumanResource.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t74 = XmlProvider<Schema ="IndicativeBatch.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t75 = XmlProvider<Schema ="IndicativeData.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t76 = XmlProvider<Schema ="Invoice.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t77 = XmlProvider<Schema ="MetricsInterchange.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t78 = XmlProvider<Schema ="NewHire.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t79 = XmlProvider<Schema ="PayrollBenefitContributions.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t80 = XmlProvider<Schema ="PayrollInstructions.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t81 = XmlProvider<Schema ="PositionOpening.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t82 = XmlProvider<Schema ="Rates.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t83 = XmlProvider<Schema ="RemoveExerciseConfirmations.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t84 = XmlProvider<Schema ="RemoveExerciseRequests.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t85 = XmlProvider<Schema ="RemoveGrants.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t86 = XmlProvider<Schema ="RemoveStockDeposits.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t87 = XmlProvider<Schema ="RemoveStockPlanParticipants.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t88 = XmlProvider<Schema ="RemoveStockPlans.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t89 = XmlProvider<Schema ="RemoveStockSaleConfirmations.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t90 = XmlProvider<Schema ="ResourceScreening.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t91 = XmlProvider<Schema ="Resume.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t92 = XmlProvider<Schema ="simpledc20021212.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t93 = XmlProvider<Schema ="StaffingAction.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t94 = XmlProvider<Schema ="StaffingOrder.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t95 = XmlProvider<Schema ="StaffingOrganization.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t96 = XmlProvider<Schema ="StaffingPosition.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t97 = XmlProvider<Schema ="StaffingShift.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t98 = XmlProvider<Schema ="StockDeposit.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t99 = XmlProvider<Schema ="StockPlan.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t100 = XmlProvider<Schema ="StockPlanParticipant.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t101 = XmlProvider<Schema ="StockPurchasePlanCoverage.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t102 = XmlProvider<Schema ="StockSaleConfirmation.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t103 = XmlProvider<Schema ="TimeCard.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t104 = XmlProvider<Schema ="TimeCardConfiguration.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//type t106 = XmlProvider<Schema ="xmldsig-core-schema.xsd", ResolutionFolder="""c:\temp\schemas\hr-xml""">
//*???* type t107 = XmlProvider<Schema ="mathml2.xsd", ResolutionFolder="""c:\temp\schemas\math\files""">
//type t127 = XmlProvider<Schema ="characters.xsd", ResolutionFolder="""c:\temp\schemas\math\files\presentation""">
//*DUP*type t141 = XmlProvider<Schema ="mathml3.xsd", ResolutionFolder="""c:\temp\schemas\mathml3\files""">
//type t142 = XmlProvider<Schema ="DeliveryOrder_NCA_V1R1.xsd", ResolutionFolder="""c:\temp\schemas\NCAXML""">
//type t143 = XmlProvider<Schema ="Invoice_NCA_V1R1.xsd", ResolutionFolder="""c:\temp\schemas\NCAXML""">
//type t144 = XmlProvider<Schema ="SampleOrder_NCA_V1R1.xsd", ResolutionFolder="""c:\temp\schemas\NCAXML""">
//type t145 = XmlProvider<Schema ="ShippingAdvice_NCA_V1R1.xsd", ResolutionFolder="""c:\temp\schemas\NCAXML""">
//type t146 = XmlProvider<Schema ="nitf-3-4-ruby-include.xsd", ResolutionFolder="""c:\temp\schemas\NITF\files""">
//*!!!!!!*type t147 = XmlProvider<Schema ="nitf-3-4.xsd", ResolutionFolder="""c:\temp\schemas\NITF\files""">
type t148 = XmlProvider<Schema ="P3Pv1.xsd", ResolutionFolder="""c:\temp\schemas\p3p\files""">
//type t151 = XmlProvider<Schema ="RIXML-2_2.xsd", ResolutionFolder="""c:\temp\schemas\rixml""">
//type t152 = XmlProvider<Schema ="RIXML-Common-2_2.xsd", ResolutionFolder="""c:\temp\schemas\rixml""">
//*****type t157 = XmlProvider<Schema ="UBL-CommonAggregateComponents-2.1.xsd", ResolutionFolder="""c:\temp\schemas\UBL\files\common""">
//type t158 = XmlProvider<Schema ="UBL-CommonBasicComponents-2.1.xsd", ResolutionFolder="""c:\temp\schemas\UBL\files\common""">
//type t165 = XmlProvider<Schema ="UBL-SignatureBasicComponents-2.1.xsd", ResolutionFolder="""c:\temp\schemas\UBL\files\common""">
type t235 = XmlProvider<Schema ="grammar-core.xsd", ResolutionFolder="""c:\temp\schemas\voicexml\files""">
type t236 = XmlProvider<Schema ="grammar.xsd", ResolutionFolder="""c:\temp\schemas\voicexml\files""">
type t237 = XmlProvider<Schema ="synthesis-core.xsd", ResolutionFolder="""c:\temp\schemas\voicexml\files""">
type t238 = XmlProvider<Schema ="synthesis.xsd", ResolutionFolder="""c:\temp\schemas\voicexml\files""">
//*LANG* type t241 = XmlProvider<Schema ="vxml-grammar-extension.xsd", ResolutionFolder="""c:\temp\schemas\voicexml\files""">
//*LANG* type t242 = XmlProvider<Schema ="vxml-grammar-restriction.xsd", ResolutionFolder="""c:\temp\schemas\voicexml\files""">
//type t244 = XmlProvider<Schema ="vxml-synthesis-restriction.xsd", ResolutionFolder="""c:\temp\schemas\voicexml\files""">
//type t245 = XmlProvider<Schema ="vxml.xsd", ResolutionFolder="""c:\temp\schemas\voicexml\files""">
//type t246 = XmlProvider<Schema ="encoding.xsd", ResolutionFolder="""c:\temp\schemas\wsdl\files""">
//type t247 = XmlProvider<Schema ="envelope.xsd", ResolutionFolder="""c:\temp\schemas\wsdl\files""">
type t250 = XmlProvider<Schema ="soap-envelope.xsd", ResolutionFolder="""c:\temp\schemas\wsdl\files""">
//type t252 = XmlProvider<Schema ="wsdl.xsd", ResolutionFolder="""c:\temp\schemas\wsdl\files""">
type t259 = XmlProvider<Schema ="xlink.xsd", ResolutionFolder="""c:\temp\schemas\xlink\files""">
type t260 = XmlProvider<Schema ="xlink11.xsd", ResolutionFolder="""c:\temp\schemas\xlink\files""">
//*DUP* type t262 = XmlProvider<Schema ="xlink.xsd", ResolutionFolder="""c:\temp\schemas\xmlspec\files""">
//*DUP* type t263 = XmlProvider<Schema ="xml.xsd", ResolutionFolder="""c:\temp\schemas\xmlspec\files""">
//*DUP* type t264 = XmlProvider<Schema ="xmlspec.xsd", ResolutionFolder="""c:\temp\schemas\xmlspec\files""">
//type t265 = XmlProvider<Schema ="analyze-string.xsd", ResolutionFolder="""c:\temp\schemas\xpath\files""">
//type t266 = XmlProvider<Schema ="schema-for-serialization-parameters.xsd", ResolutionFolder="""c:\temp\schemas\xpath\files""">
//type t267 = XmlProvider<Schema ="schema_for_serialization_parameters.xsd", ResolutionFolder="""c:\temp\schemas\xquery\files""">
//type t273 = XmlProvider<Schema ="xsl_fo.xsd", ResolutionFolder="""c:\temp\schemas\xsl\files""">

