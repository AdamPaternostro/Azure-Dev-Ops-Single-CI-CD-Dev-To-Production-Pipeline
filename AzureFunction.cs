#r "Newtonsoft.Json"
using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    string responseBody = "";

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    string organization = data?.organization;
    log.Info("organization:" + organization);

    string project = data?.project;
    log.Info("project:" + project);

    string buildId = data?.buildId;
    log.Info("buildId:" + buildId);

    string requiredTag = data?.requiredTag;
    log.Info("requiredTag:" + requiredTag);

    string personalAcccessToken = data?.personalAcccessToken;
    //log.Info("personalAcccessToken:" + personalAcccessToken);

    try
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    System.Text.ASCIIEncoding.ASCII.GetBytes(
                        string.Format("{0}:{1}", "", personalAcccessToken))));

            using (HttpResponseMessage response = client.GetAsync(
                        $"https://dev.azure.com/{organization}/{project}/_apis/build/builds/{buildId}/tags?api-version=4.1").Result)
            {
                response.EnsureSuccessStatusCode();
                responseBody = await response.Content.ReadAsStringAsync();
                log.Info(responseBody);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }

    BuildTagResponse buildTagResponse = JsonConvert.DeserializeObject<BuildTagResponse>(responseBody);

    var returnValue = new { status = "failed"};

    if (buildTagResponse.value.Contains(requiredTag))
    {
        returnValue = new { status = "successful"};
    }

    return req.CreateResponse(
        HttpStatusCode.OK,
        returnValue,
        System.Net.Http.Formatting.JsonMediaTypeFormatter.DefaultMediaType);

}

public class BuildTagResponse
{
    public int count {get;set;}
    public string[] value {get;set;}
}