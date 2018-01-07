#r "Newtonsoft.Json"

using System;
using System.Data;
using System.Net;
using System.Text;

using Newtonsoft.Json;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

private static TelemetryClient telemetry = new TelemetryClient();
private static string key = TelemetryConfiguration.Active.InstrumentationKey = System.Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", EnvironmentVariableTarget.Process);
static HttpClient httpClient = new HttpClient();

//https://codehollow.com/2017/05/return-json-from-csharp-azure-function/
//https://dzimchuk.net/event-correlation-in-application-insights/


public class LanguageUnderstandingResponse
{
    public string Query { get; set; }
    public string TopScoringIntent { get; set; }
    public string Score { get; set; }
    public int BlogEntries { get; set; }
    public string Caller { get; set; }
}

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{

    var LUISappID = "b7bcd0f6-2560-43bf-a642-7b0c2e13782b";
    var LUISsubscriptionKey="fb3488ba06614b4985c1baa7a0af0376";
    var LUISendpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/";
    
    telemetry.TrackEvent("LUIS Function Started");
    log.Info("LUIS C# HTTP trigger function processed a request.");

    // Query string
    string query = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "query", true) == 0)
        .Value;

    // POST Body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Final LUIS Query
    query = query ?? data?.query;


    // If no query, return 204
    if( String.IsNullOrEmpty(query)){
        return new HttpResponseMessage(HttpStatusCode.NoContent);
    }

    telemetry.TrackEvent("Function Started");
    string LUISQueryResponse;


        // LUIS HTTP CALL
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", LUISsubscriptionKey);
        var response = await httpClient.GetAsync(LUISendpoint + LUISappID + "/?verbose=true&q=" + query);

        // If LUIS error, return error
        if (!response.IsSuccessStatusCode) {
            return new HttpResponseMessage(response.StatusCode );
        }

        // get LUIS response content as string
        LUISQueryResponse = await response.Content.ReadAsStringAsync();
    
    https://stackoverflow.com/questions/39397591/how-to-send-json-to-azure-appinsights-with-c-sharp-library
    log.Info("LUIS query - " + DateTime.Now);
    telemetry.TrackTrace(LUISQueryResponse);
 

    return LUISQueryResponse == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
        : req.CreateResponse(HttpStatusCode.OK, LUISQueryResponse);
}



