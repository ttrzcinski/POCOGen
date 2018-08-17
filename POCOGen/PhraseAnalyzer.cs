using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace POCOGen
{
    public static class PhraseAnalyzer
    {
        [FunctionName("PhraseAnalyzer")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger PhraseAnalyzer function started processing a request.");

            string phrase = req.Query["phrase"];

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            phrase = phrase ?? data?.phrase;

            return string.IsNullOrEmpty(phrase)
                ? (ActionResult)new OkObjectResult(GoodResult(phrase))
                : new BadRequestObjectResult(BadResult());
        }

        private static object GoodResult(string phrase)
        {
            Contract.Requires(string.IsNullOrEmpty(phrase) == false);

            StringBuilder builder = new StringBuilder();
            var words = phrase.Split(' ');
            var wordsCount = words.Length;
            builder.Append($"There are {wordsCount} words.").AppendLine();

            return builder.ToString();
        }

        private static object BadResult() => "What can be done with empty string?";
    }
}