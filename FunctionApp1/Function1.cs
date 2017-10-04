using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using BusInfo;
using System.Threading.Tasks;
using System.Text;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;

namespace FunctionApp1
{
    public static class Function1
    {

        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            dynamic data = await req.Content.ReadAsAsync<object>();
            var request = data.request;
            var requestType = request.GetRequestType();

            if (requestType == typeof(IntentRequest))
            {
                var intentRequest = request as IntentRequest;
                var context = data.Context.System.Device;
                var deviceId = context.DeviceID;
                log.Info($"Device: {deviceId}");

                if (intentRequest.Intent.Name.Equals("Transit"))
                {
                    // get the slots
                    var firstValue = intentRequest.Intent.Slots["RouteName"].Value;
                    log.Info($"firstValue: {firstValue}");
                    MyStopInfo busInfo = new MyStopInfo(new BusLocator(), new TimeZoneConverter());
                    var results = await busInfo.GetArrivalTimesForRouteName(firstValue, "47.611959", "-122.332893");
                    var minutes = results.Select(x => x.Minute);
                    string CreateMsg()
                    {
                        var sb = new StringBuilder();
                        foreach (var min in minutes)
                        {
                            var units = min == 1 ? "minute" : "minutes";
                            sb.Append($"{min} {units},");
                        }
                        return $"The next {firstValue} bus comes in {sb}";
                    }


                    // create the speech response - you most likely will still have this
                    var speech = new PlainTextOutputSpeech();
                    speech.Text = CreateMsg();

                    // create the response
                    var responseBody = new ResponseBody();
                    responseBody.OutputSpeech = speech;
                    responseBody.ShouldEndSession = true;

                    var skillResponse = new SkillResponse();
                    skillResponse.Response = responseBody;
                    skillResponse.Version = "1.1";


                    var m = new HttpRequestMessage();

                    return m.CreateResponse(HttpStatusCode.OK, skillResponse) ;


                }
            }
            else if (requestType == typeof(LaunchRequest))
            {
            }
            else
            {

            }
            var speech2 = new PlainTextOutputSpeech();
            speech2.Text = "Hello";

            // create the response
            var responseBody2= new ResponseBody();
            responseBody2.OutputSpeech = speech2;
            responseBody2.ShouldEndSession = true;

            var skillResponse2 = new SkillResponse();
            skillResponse2.Response = responseBody2;
            skillResponse2.Version = "1.1";

            var m2 = new HttpRequestMessage();

            return m2.CreateResponse(HttpStatusCode.OK, new { skillResponse2 });
        }

    }

    public class AlexaConstants
    {
        public static string AppId = "amzn1.ask.skill.ec6756e1-14d0-4a79-8975-4e153cb5ec7a";
        public static string AppName = "Transit King";
        public static string AppErrorMessage = "Sorry, something went wrong. Please try again";
    }


}
