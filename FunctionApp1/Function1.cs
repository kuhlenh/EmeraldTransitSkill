using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using AlexaSkillsKit;
using AlexaSkillsKit.Speechlet;
using AlexaSkillsKit.UI;
using BusInfo;
using System.Threading.Tasks;
using System.Text;

namespace FunctionApp1
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info($"Request {req}");

            // parse query parameter
            //string name = req.GetQueryNameValuePairs()
            //    .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
            //    .Value;

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();
            log.Info($"Content= {data}");

            // Mock data to test
            MyStopInfo busInfo = new MyStopInfo(new BusLocator(), new TimeZoneConverter());


            var results = await busInfo.GetArrivalTimesForRouteName("545", "47.611959", "-122.332893");
            var minutes = results.Select(x => x.Minute);
            
            string CreateMsg()
            {
                var sb = new StringBuilder();
                foreach(var m in minutes)
                {
                    var units = m == 1 ? "minute" : "minutes";
                    sb.Append($"{m} {units},");
                }
                return $"The next 545 bus comes in {sb}";
            }


            //string intentName = data.request.intent.name;
            //log.Info($"intentName={intentName}");


            var speechletResponse = new SpeechletResponse()
            {
                OutputSpeech = new PlainTextOutputSpeech() { Text = $"Hello {CreateMsg()}" },
                ShouldEndSession = true
            };

            var card = new SimpleCard() { Content = "card contents", Title = "card title" };


            return req.CreateResponse(HttpStatusCode.OK, speechletResponse);
        }
    }


    public class AlexaConstants
    {
        public static string AppId = "amzn1.ask.skill.ec6756e1-14d0-4a79-8975-4e153cb5ec7a";
        public static string AppName = "Transit King";
        public static string AppErrorMessage = "Sorry, something went wrong. Please try again";
    }


}
