using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace JwtSample.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter Server url with backslash (http://46.146.232.216/JwtSample/): ");
            string url = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(url))
            {
                url = "http://46.146.232.216/JwtSample/";
            }

            Task<TokenInfo> getTokenTask = null;
            try
            {
                getTokenTask = GetTokenAsync(new Uri(new Uri(url), "token"), "RubenMisrahi");
                getTokenTask.Wait();
            }
            catch
            {
                Console.WriteLine("Invalid User ID or endpoint address.");
            }

            if (getTokenTask != null)
            {
                string jwt = getTokenTask.Result.token;

                Console.WriteLine($"Your token is {jwt}");

                Task<string> addChildTask = null;

                try
                {
                    addChildTask = AddChildAsync(new Uri(new Uri(url), "children"), jwt, "Anna", "2017-01-01");
                    addChildTask.Wait();
                }
                catch
                {
                    Console.WriteLine("Error on adding child.");
                }

                if (addChildTask != null)
                {
                    Console.WriteLine(addChildTask.Result);
                }
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        /// <summary>
        /// Get JWT token
        /// </summary>
        /// <param name="uri">Endpoint URI</param>
        /// <param name="facebookId">Person ID</param>
        /// <returns>Token Info</returns>
        static async Task<TokenInfo> GetTokenAsync(Uri uri, string facebookId)
        {
            // Create HTTP Client
            HttpClient client = new HttpClient();

            // Add parameters
            HttpContent content = new StringContent($"facebookId={facebookId}");
            // Add Content-Type header
            content.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";

            // Send request to server
            HttpResponseMessage response = await client.PostAsync(uri, content);

            if (response.IsSuccessStatusCode)
            {
                // if Success the return Token Info
                return await response.Content.ReadAsAsync<TokenInfo>();
            }
            else
            {
                throw new Exception("Not authorized");
            }
        }

        /// <summary>
        /// Add child
        /// </summary>
        /// <param name="uri">Endpoint URI</param>
        /// <param name="jwt">JWT token</param>
        /// <param name="childName">Child Name</param>
        /// <param name="birthDate">Birth Date</param>
        /// <returns>Text result from server</returns>
        static async Task<string> AddChildAsync(Uri uri, string jwt, string childName, string birthDate)
        {
            // Create HTTP Client
            HttpClient client = new HttpClient();
            // Add authorization Header with JWT token
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);

            // Add parameters
            HttpContent content = new StringContent($"childName={childName}&birthDate={birthDate}");
            // Add Content-Type header
            content.Headers.ContentType.MediaType = "application/x-www-form-urlencoded";

            // Send request to server
            HttpResponseMessage response = await client.PostAsync(uri, content);

            if (response.IsSuccessStatusCode)
            {
                // If Success then return text result from server
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception("Not authorized");
            }
        }
    }
}