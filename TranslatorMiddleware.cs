using Microsoft.Bot.Builder;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BotBuilder.Luis.TranslatorMiddleware
{

    public class TranslatorMiddlewareOptions
    {

        public string SubscriptionKey { get; set; }

        public string From { get; set; }

        public string To { get; set;  }
    }

    public class TranslatorMiddleware : IMiddleware
    {

        private const string OcpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";

        private const string TranslateUrlTemplate = "http://api.microsofttranslator.com/v2/http.svc/translate?text={0}&from={1}&to={2}&category={3}";

        private TranslatorMiddlewareOptions options = new TranslatorMiddlewareOptions();

        public TranslatorMiddleware(TranslatorMiddlewareOptions options) {
            this.options = options;
        }


        public async Task OnTurn(ITurnContext context, MiddlewareSet.NextDelegate next)
        {
            var text = context.Activity.Text;
            var fromLanguage = this.options.From;
            var toLanguage = this.options.To;
            context.Activity.Text = await translate(text, fromLanguage, toLanguage);
            await next().ConfigureAwait(false);
        }

        private async Task<string> translate(string text, string fromLanguage, string toLanguage)
        {
            var translateResponse = await TranslateRequest(string.Format(TranslateUrlTemplate, text,
                fromLanguage, toLanguage, "general"));

            var translateResponseContent = await translateResponse.Content.ReadAsStringAsync();

            if (translateResponse.IsSuccessStatusCode)

            {
                var gt = translateResponseContent.IndexOf('>');
                var lt = translateResponseContent.IndexOf('<', gt);
                var transtext = translateResponseContent.Substring(gt + 1, lt - gt - 1);
                Console.WriteLine("Translation result: {0}", translateResponseContent);
                return transtext;
            }
            else
            {
                Console.Error.WriteLine("Failed to translate. Response: {0}", translateResponseContent);
                return text;
            }
        }

        public async Task<HttpResponseMessage> TranslateRequest(string url)

        {

            using (HttpClient client = new HttpClient())

            {

                client.DefaultRequestHeaders.Add(OcpApimSubscriptionKeyHeader, this.options.SubscriptionKey);

                return await client.GetAsync(url);

            }

        }

        
    }
}