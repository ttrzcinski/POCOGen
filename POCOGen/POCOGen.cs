using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics.Contracts;
using System;

namespace POCOGeneratorFunction
{
    /// <summary>
    /// Creates new POCO Generate function, which after passing name of class and variables, generates simple POCO.
    /// </summary>
    public static class POCOGen
    {
        [FunctionName("POCOGen")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            var dateStart = DateTime.Now;
            log.Info("C# HTTP trigger POCOGen function started processing a request.");

            string name = req.Query["name"];
            string vars = req.Query["vars"];

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            var response = name != null && vars != null
                ? (ActionResult)new OkObjectResult(GoodResult(name, vars))
                : new BadRequestObjectResult(BadResult());
            var dateEnd = DateTime.Now;
            log.Info($"C# HTTP trigger POCOGen function finished processing a request in {dateEnd-dateStart}");
            return response;
        }

        static string GoodResult(string name, string varLine)
        {
            Contract.Requires(string.IsNullOrEmpty(name) == false);
            Contract.Requires(string.IsNullOrEmpty(varLine) == false);
            StringBuilder builder = new StringBuilder();
            builder.Append("// Class generated with POCO Generator Function.").AppendLine();
            builder.Append("using System;").AppendLine().AppendLine();
            builder.Append("namespace POCOGeneratorFunction").AppendLine();
            builder.Append("{").AppendLine();
            builder.Append($"    public class {name}").AppendLine();
            builder.Append("    {").AppendLine();

            string[] vars = varLine.Split("=;, ".ToCharArray());
            foreach (string var in vars)
            {
                if (var.Contains("id"))
                {
                    builder.Append($"        private long {var} {{ get; set; }}").AppendLine();
                }
                else if (var.Contains("list"))
                {
                    builder.Append($"        private List<T> {var} {{ get; set; }}").AppendLine();
                }
                else if (var.Contains("array"))
                {
                    builder.Append($"        private T[] {var} {{ get; set; }}").AppendLine();
                }
                else if (var.Contains("count"))
                {
                    builder.Append($"        private int {var} {{ get; set; }}").AppendLine();
                }
                else
                {
                    builder.Append($"        private object {var} {{ get; set; }}").AppendLine();
                }
            }

            builder.Append("    }").AppendLine();
            builder.Append("}").AppendLine();

            return builder.ToString();
        }

        static string BadResult() => "Please pass a name on the query string or in the request body";
    }
}
