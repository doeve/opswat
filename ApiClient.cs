using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Threading;
using System.Security.Policy;


namespace scanner
{
    internal class ApiClient
    {
        private string _apiKey = "";
        /*in case you want to change the api key you have 2 options:
            - start the program anyway, you'll be able to paste it
            - just add a value to it above this comment
         */
        private string api = "https://api.metadefender.com/v4";
        private string fileName = "";
        private string dataId = "";
        private JObject fileInfo;
        private HttpClient client = new HttpClient(); 


        public string GetHash(string filePath)
        {
            fileName = filePath;
            var sha256 = SHA256.Create();
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] hashValue = sha256.ComputeHash(fileStream);
            return BitConverter.ToString(hashValue).Replace("-", "");
        }

        public async void HashLookup(string hash)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(api + "/hash/" + hash),
                Headers =
                  {
                    { "apikey", _apiKey }
                  }
            };
            using (var response = await client.SendAsync(request)) 
            {
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    JObject resultJson = JObject.Parse(result);
                    dataId = (string)resultJson["data_id"];
                    getResult();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine("Hash not found. Uploading. ");
                    await Upload();
                } 
                else
                {
                    Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                }
            }
        }

        private async Task Upload()
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{api}/file"),
                Headers =
                {
                    { "apikey", _apiKey }
                },
                Content = new StreamContent(new FileStream(fileName, FileMode.Open, FileAccess.Read))
            };
            request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            using (var response = await client.SendAsync(request))
            {
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    JObject resultJson = JObject.Parse(result);
                    dataId = (string) resultJson["data_id"];
                    getResult();
                }
                else
                {
                    Console.WriteLine($"File upload failed with status code: {response.StatusCode}");
                }
            }
        }

        async private void getResult() 
        {
            try
            {
                while (true)
                {
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri($"{api}/file/{dataId}"),
                        Headers =
                    {
                        { "apikey", _apiKey }
                    }
                    };

                    using (var response = await client.SendAsync(request))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var result = await response.Content.ReadAsStringAsync();
                            JObject resultJson = JObject.Parse(result);
                            if ((int) resultJson["process_info"]["progress_percentage"] == 100)
                            {
                                fileInfo = resultJson;
                                DisplayInfo();
                                break;
                            } 
                            else
                            {
                                Thread.Sleep(10000);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Request failed with status code: {response.StatusCode}");
                        }
                    }
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }
        
        private void DisplayInfo()
        {
            JObject scanDetails = (JObject)fileInfo["scan_results"]["scan_details"];
            string status = (string)fileInfo["scan_results"]["scan_all_result_a"];
            Console.WriteLine($"Filename: {fileName}");
            Console.WriteLine($"OverallStatus: {status}");

            foreach (var engine in scanDetails)
            {
                string engineName = engine.Key;
                var details = engine.Value;

                string threatFound = String.IsNullOrEmpty((string) details["threat_found"])? "clean" : (string) details["threat_found"];
                string scanResult = (string) details["scan_result_i"];
                string defTime = (string) details["def_time"];

                Console.WriteLine($"Engine: {engineName}");
                Console.WriteLine($"ThreatFound: {threatFound}");
                Console.WriteLine($"ScanResult: {scanResult}");
                Console.WriteLine($"DefTime: {defTime}\n");
            }
            Console.WriteLine("END");
        }
    }
}
