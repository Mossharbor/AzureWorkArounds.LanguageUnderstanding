using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Mossharbor.AzureWorkArounds.LanguageUnderstanding
{
    public class LuisModel
    {
        string ocpAcimSubscriptionKey;
        string luisAppID;
        string path;
        static string appVersion = "0.1";
        static string host = "https://westus.api.cognitive.microsoft.com";
        internal string UriRoot { get; set; }
        
        public LuisModel(string luisAppID, string ocpAcimSubscriptionKey)
        {
            this.ocpAcimSubscriptionKey = ocpAcimSubscriptionKey;
            this.luisAppID = luisAppID;
            this.path = "/luis/api/v2.0/apps/" + luisAppID + "/versions/" + appVersion + "/";
            UriRoot = host + path;
        }

        public QueryResponse Query(string queryPhrase)
        {
            using (var client = new HttpClient())
            {
                // The request header contains your subscription key
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ocpAcimSubscriptionKey);

                var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

                // The "q" parameter contains the utterance to send to LUIS
                queryString["q"] = queryPhrase;

                // These optional request parameters are set to their default values
                queryString["timezoneOffset"] = "0";
                queryString["verbose"] = "true";
                queryString["spellCheck"] = "false";
                queryString["staging"] = "false";

                var uri = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/" + luisAppID + "?" + queryString;
                var response = client.GetAsync(uri).Result;
                response.EnsureSuccessStatusCode();
                QueryResponse result = JsonConvert.DeserializeObject<QueryResponse>(response.Content.ReadAsStringAsync().Result);

                return result;
            }
        }

        public LuisModelBuilder Modify()
        {
            return new LuisModelBuilder(this);
        }

        public void DeleteIntent(string intent)
        {
            this.Modify().DeleteIntent(intent).Update();
        }

        public void DeleteEntity(string entity)
        {
            var entityList = GetEntities();
            this.Modify().DeleteEntity(entityList[entity]).Update();
        }

        public IDictionary<string, Guid> GetIntents()
        {
            // Response 429 Rate limit is exceeded.
            // Response 400 This error can be returned if the request's parameters are incorrect meaning the required parameters are missing, malformed, or too large.
            // Response 401 You do not have access.  See https://westus.dev.cognitive.microsoft.com/docs/services/5890b47c39e2bb17b84a55ff/operations/5890b47c39e2bb052c5b9c0d
            string uri = UriRoot + "intents";
            return ParseOutIntents(SendGet(uri, true).Result);
        }

        static IDictionary<string, Guid> ParseOutIntents(HttpResponseMessage response)
        {
            var names = new Dictionary<string, Guid>();
            dynamic dynObj = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
            foreach (var t in dynObj)
            {
                names.Add(t.name.ToString(), Guid.Parse(t.id.ToString()));
            }
            return names;
        }

        public IDictionary<string, Guid> GetEntities()
        {
            var names = new Dictionary<string, Guid>();

            // Response 429 Rate limit is exceeded.
            // Response 400 This error can be returned if the request's parameters are incorrect meaning the required parameters are missing, malformed, or too large.
            // Response 401 You do not have access.  See https://westus.dev.cognitive.microsoft.com/docs/services/5890b47c39e2bb17b84a55ff/operations/5890b47c39e2bb052c5b9c0d
            string uri = UriRoot+ "entities";
            ParseOutEntities(Retry(SendGet, uri).Result, names);
            uri = UriRoot + "compositeentities";
            ParseOutEntities(Retry(SendGet, uri).Result, names);
            uri = UriRoot + "closedlists";
            ParseOutEntities(Retry(SendGet, uri).Result, names);
            uri = UriRoot + "hierarchicalentities";
            ParseOutEntities(Retry(SendGet, uri).Result, names);

            return names;
        }

        internal IDictionary<string,Guid> GetEntities(string specificType)
        {
            var names = new Dictionary<string, Guid>();
            string uri = UriRoot + specificType;
            ParseOutEntities(Retry(SendGet, uri).Result, names);
            return names;
        }

        static void ParseOutEntities(HttpResponseMessage response, IDictionary<string, Guid> names)
        {
            dynamic dynObj = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
            foreach (var t in dynObj)
            {
                names.Add(t.name.ToString(), Guid.Parse(t.id.ToString()));
            }
            return;
        }

        public string GetTrainingStatus()
        {
            var response = SendGet(host + path + "train", true).Result;
            var result = response.Content.ReadAsStringAsync().Result;

            throw new NotImplementedException(); // TODO need to parse response
        }

        public bool ModelNeedsTraining()
        {
            string uri = host + path;
            HttpResponseMessage response = SendGet(uri, true).Result;

            dynamic dynObj = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
            if (dynObj.trainingStatus == "NeedsTraining")
                return true;

            return false;
        }

        public string Train()
        {
            string uri = host + path + "train";
            var response = SendPost(uri, String.Empty, true).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            System.Diagnostics.Debug.WriteLine("Sent training request.");
            System.Diagnostics.Debug.WriteLine(JsonPrettyPrint(result));
            return GetTrainingStatus();
        }

        internal async Task<HttpResponseMessage> SendPost(string uri, string requestBody, bool ensureSuccess)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "text/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", ocpAcimSubscriptionKey);
                var ret = await client.SendAsync(request);
                if (ensureSuccess)
                    ret.EnsureSuccessStatusCode();
                return ret;
            }
        }

        internal async Task<HttpResponseMessage> Retry(Func<string, bool, Task<HttpResponseMessage>> f, string input)
        {
            HttpResponseMessage response;
            do
            {
                response = await f(input, false);
                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode != (429)) // this is the Too many requests throttle response
                        return response;

                    System.Threading.Thread.Sleep(1000);
                }
            } while (!response.IsSuccessStatusCode);

            return response;
        }

        internal async Task<HttpResponseMessage> Retry(Func<string, string, bool, Task<HttpResponseMessage>> f, string input1, string input2)
        {
            HttpResponseMessage response;
            do
            {
                response = await f(input1, input2, false);
                if (!response.IsSuccessStatusCode)
                {
                    if ((int)response.StatusCode != (429)) // this is the Too many requests throttle response
                        return response;

                    System.Threading.Thread.Sleep(1000);
                }
            } while (!response.IsSuccessStatusCode);

            return response;
        }

        internal async Task<HttpResponseMessage> SendGet(string uri, bool ensureSuccess)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(uri);
                request.Headers.Add("Ocp-Apim-Subscription-Key", ocpAcimSubscriptionKey);
                var ret = await client.SendAsync(request);
                if (ensureSuccess)
                    ret.EnsureSuccessStatusCode();
                return ret;
            }
        }

        internal async Task<HttpResponseMessage> SendPut(string uri, string requestBody, bool ensureSuccess)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Put;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "text/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", ocpAcimSubscriptionKey);
                var ret = await client.SendAsync(request);
                if (ensureSuccess)
                    ret.EnsureSuccessStatusCode();
                return ret;
            }
        }

        internal async Task<HttpResponseMessage> SendDelete(string uri, bool ensureSuccess)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Delete;
                request.RequestUri = new Uri(uri);
                request.Headers.Add("Ocp-Apim-Subscription-Key", ocpAcimSubscriptionKey);
                var ret = await client.SendAsync(request);
                if (ensureSuccess)
                    ret.EnsureSuccessStatusCode();
                return ret;
            }
        }

        static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\'':
                        if (quote) ignore = !ignore;
                        break;
                }

                if (quote)
                    sb.Append(ch);
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (ch != ' ') sb.Append(ch);
                            break;
                    }
                }
            }

            return sb.ToString().Trim();
        }
    }
}
