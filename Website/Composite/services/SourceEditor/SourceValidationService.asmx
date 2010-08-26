<%@ WebService Language="C#" Class="SourceValidationService" %>

using System;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Composite.Core.Logging;
using Composite.Core.Xml;

[WebService(Namespace = "http://www.composite.net/ns/management")]
[SoapDocumentService(RoutingStyle = SoapServiceRoutingStyle.RequestElement)]
public class SourceValidationService  : System.Web.Services.WebService {
	
	/*
	 * Note that we return a string, not a boolean, in order 
	 * to relay the exception message if things go wrong.
	 */
    [WebMethod]
    public string IsWellFormedDocument ( string xml ) 
    {
    	string result = "True";
    	try {
            XDocument.Parse(xml);
		} catch ( XmlException exception ) {
			result = exception.Message;
		}
		return result;
    }
    
    
    [WebMethod]
    public string IsWellFormedFragment ( string xml ) 
    {
    	string result = "True";
    	try {
			XmlDocument doc = new XmlDocument ();
			XmlDocumentFragment frag = doc.CreateDocumentFragment ();
			frag.InnerXml = xml;
		} catch ( XmlException exception ) {
			result = exception.Message;
		}
		return result;
    }


    [WebMethod]
    public string ValidateSource(string sourceToValidate, string validatorName)
    {
        string result = "True";
        try
        {
            switch (validatorName)
            {
                case "http://www.composite.net/ns/function/1.0":
                    var reader = new XmlValidatingReader(sourceToValidate, XmlNodeType.Document, null);
                    var schema = GetSchema("~/Composite/schemas/Functions/XsltFunctionCalls.xsd");

                    if (schema != null)
                    {
                        reader.Schemas.Add(schema);
                        reader.ValidationType = ValidationType.Schema;
                    }

                    reader.ValidationEventHandler += ReaderValidationEventHandler;

                    while(reader.Read()) {}
                    break;
                     
                default:
                    throw new ArgumentException(string.Format("Unknown validatorName '{0}' - have the editor been misconfigured?", validatorName), "validatorName");
            }
        }
        catch (Exception exception)
        {
            result = exception.Message;
        }
        return result;
    }
    
     private static void ReaderValidationEventHandler(object sender, ValidationEventArgs e)
     {
         throw e.Exception;
     }
    
    
    static XmlSchema GetSchema(string filePath)
    {
        try
        {
            filePath = HttpContext.Current.Server.MapPath(filePath);
            string cacheKey = "XmlSchema " + filePath;

            XmlSchema schema = HttpRuntime.Cache[cacheKey] as XmlSchema;
            if (schema != null)
            {
                return schema;
            }

            using (var file = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                schema = XmlSchema.Read(file, null);
            }

            HttpRuntime.Cache.Add(cacheKey, schema, new CacheDependency(filePath), DateTime.MaxValue, TimeSpan.Zero, CacheItemPriority.Default, null);

            return schema;
        }
        catch(Exception e)
        {
            LoggingService.LogError(typeof(SourceValidationService).Name, new Exception("Failed to load XSD scheme", e));
            return null;
        }
    }
}