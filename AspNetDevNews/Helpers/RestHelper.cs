using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;


namespace AspNetDevNews.Helpers
{
    public static class RestHelper
    {
        public static T Get<T>(string URL, string urlParameters)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                var json = response.Content.ReadAsStringAsync().Result;
                var globalobject = JsonConvert.DeserializeObject<T>(json);

                return globalobject;
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                return default(T);
            }
        }

        public static bool Post<T>(string URL, string urlParameters, T payLoad)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            string jsonPayload = JsonConvert.SerializeObject(payLoad);

            // List data response.
            HttpResponseMessage response = client.PostAsJsonAsync(urlParameters, payLoad).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                //var json = response.Content.ReadAsStringAsync().Result;
                //var globalobject = JsonConvert.DeserializeObject<T>(json);

                return true;
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                return false;
            }
        }

        public static bool Put<T>(string URL, string urlParameters, T payLoad)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            string jsonPayload = JsonConvert.SerializeObject(payLoad);

            // List data response.
            HttpResponseMessage response = client.PutAsJsonAsync(urlParameters, payLoad).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                //var json = response.Content.ReadAsStringAsync().Result;
                //var globalobject = JsonConvert.DeserializeObject<T>(json);

                return true;
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                return false;
            }
        }

        public static bool Delete<T>(string URL, string urlParameters)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            HttpResponseMessage response = client.DeleteAsync(urlParameters).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                //var json = response.Content.ReadAsStringAsync().Result;
                //var globalobject = JsonConvert.DeserializeObject<T>(json);

                return true;
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                return false;
            }
        }
    }
}
