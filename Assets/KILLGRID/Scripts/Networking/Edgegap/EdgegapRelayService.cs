using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Attic.Utilities;
using Newtonsoft.Json;

namespace KILLGRID.Networking.Edgegap
{
    public static class EdgegapRelayService
    {
        private const string ApiURL = "https://api.edgegap.com";

        private static readonly HttpClient HttpClient = new HttpClient();
        private static string relayProfileToken;

        public static void Initialize(string relayToken)
        {
            relayProfileToken = relayToken;
        }

        public async static Task<ApiResponse> CreateSessionAsync(int numPlayer)
        {
            // Set the authorization header
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", relayProfileToken);

            // Set the Ips for the request
            RootObject objectToSerialize = new RootObject
            {
                users = new List<Users>()
            };

            for (int i = 0; i < numPlayer; i++)
            {
                HttpResponseMessage ipRes = await HttpClient.GetAsync($"{ApiURL}/v1/ip");
                string ipContent = await ipRes.Content.ReadAsStringAsync();
                PublicIP ipData = JsonConvert.DeserializeObject<PublicIP>(ipContent);

                Users user = new Users
                {
                    ip = ipData.public_ip
                };

                objectToSerialize.users.Add(user);
            }

            // Serialize the IP array to JSON
            StringContent jsonContent =
                new StringContent(JsonConvert.SerializeObject(objectToSerialize), Encoding.UTF8, "application/json");

            // Send the POST request and get the response
            HttpResponseMessage response = await HttpClient.PostAsync($"{ApiURL}/v1/relays/sessions", jsonContent);

            string responseContent = await response.Content.ReadAsStringAsync();

            Log.Write(responseContent);

            // Deserialize the response of the API
            ApiResponse content = JsonConvert.DeserializeObject<ApiResponse>(responseContent);

            //Sends a loop to wait for a positive responce
            //The first answer of the API contain very few informations, but with the session_id that it gives us,
            //we can find our session and wait for it to be ready
            await PollDataAsync(HttpClient, content, content.session_id);

            // Reinitialize our content
            HttpResponseMessage newResponse = await HttpClient.GetAsync($"{ApiURL}/v1/relays/sessions/{content.session_id}");
            string newResponseContent = await newResponse.Content.ReadAsStringAsync();

            ApiResponse data = JsonConvert.DeserializeObject<ApiResponse>(newResponseContent);

            if (!data.ready)
            {
                throw new InvalidOperationException(
                    $"Error: {response.RequestMessage} - {response.ReasonPhrase}\nError: Couldn't find a session relay");
            }

            return data;
        }

        private async static Task PollDataAsync(HttpClient client, ApiResponse content, string sessionId)
        {
            // TODO: say something when waiting for too long
            while (!content.ready)
            {
                Log.Write("Waiting for data to be ready...");
                await Task.Delay(3000);

                HttpResponseMessage response = await client.GetAsync($"{ApiURL}/v1/relays/sessions/{sessionId}");
                string responseContent = await response.Content.ReadAsStringAsync();

                Log.Write("Response from client -----------" + responseContent);

                content = JsonConvert.DeserializeObject<ApiResponse>(responseContent);

                Log.Write("Is the game ready : " + content.ready);
            }

            // The "ready" property is now true, output a message
            Log.Write("Data is now ready!");
        }

        public async static Task<ApiResponse> JoinSessionAsync(string sessionId)
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", relayProfileToken);
            HttpResponseMessage response = await HttpClient.GetAsync($"{ApiURL}/v1/relays/sessions/{sessionId}");

            // Catch bad session ID
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Error: {response.RequestMessage} - {response.ReasonPhrase}");
            }

            string responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse>(responseContent);
        }

        public async static Task<ApiResponse> DeleteSessionAsync(string sessionId)
        {
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", relayProfileToken);
            HttpResponseMessage response = await HttpClient.DeleteAsync($"{ApiURL}/v1/relays/sessions/{sessionId}");

            // Catch bad session ID
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Error: {response.RequestMessage} - {response.ReasonPhrase}");
            }

            string responseContent = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ApiResponse>(responseContent);
        }

        public async static Task<ApiResponse> FindSessionAsync(List<string> ignoredSessionIds)
        {
            string[] filteredSessionIds = ignoredSessionIds.ToArray();

            Log.Write("Finding Edgegap Relay Session");

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", relayProfileToken);
            HttpResponseMessage response = await HttpClient.GetAsync($"{ApiURL}/v1/relays/sessions");

            // Catch bad session ID
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Error: {response.RequestMessage} - {response.ReasonPhrase}");
            }

            string responseContent = await response.Content.ReadAsStringAsync();

            FindSessionResponse result = JsonConvert.DeserializeObject<FindSessionResponse>(responseContent);

            if (result.sessions == null || result.sessions.Count == 0)
            {
                throw new InvalidOperationException("No available sessions found.");
            }

            List<string> eligibleSessions = new List<string>();

            foreach (ApiResponse session in result.sessions)
            {
                if (Array.Exists(filteredSessionIds, id => id == session.session_id))
                {
                    Log.Write($"Ignoring session ID: {session.session_id}");
                    continue;
                }

                eligibleSessions.Add(session.session_id);
            }

            // Get random eligible session
            if (eligibleSessions.Count == 0)
            {
                return null;
            }

            int randomIndex = new Random().Next(eligibleSessions.Count);
            string selectedSessionId = eligibleSessions[randomIndex];

            ApiResponse selectedSession = null;
            foreach (ApiResponse session in result.sessions)
            {
                if (session.session_id == selectedSessionId)
                {
                    selectedSession = session;
                    break;
                }
            }

            return selectedSession;
        }
    }
}
