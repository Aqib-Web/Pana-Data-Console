using System;
using System.Net.Http;
using System.Text;

namespace Pana_Data_Console
{
    public class ApiClient
    {
        public static void PostEvidenceToEVM4(string jsonPayload)
        {
            var url = "https://dev-evm4-m.irsavideo.com/api/v1/Evidences";
            var bearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJUZW5hbnRJZCI6IjE5IiwiVXNlcklkIjoiMSIsIkFzc2lnbmVkR3JvdXBzIjoiW3tcIklkXCI6MSxcIlBlcm1pc3Npb25cIjoxfV0iLCJBc3NpZ25lZE1vZHVsZXMiOiIxLDIsMyw0LDUsNiw3LDgsOSwxMCwxMSwxMiwxMywxNCwxNSwxNywxOCwxOSwyMCwyMSwyMiwyMywyNCwyNSwyNiwyNywyOCwyOSwzMCwzMSwzMiwzMywzNCwzNSwzNiwzNywzOCwzOSw0MCw0MSw0Miw0Myw0NCw0NSw0Niw0Nyw0OCw0OSw1MCw1MSw1Miw1Myw1NCw1NSw1Niw1Nyw2MCw2MSw2Miw2Myw2NCw3MCw3MSw3Miw3Myw3NCw3NSw3Nyw3OCw3OSw4MCw4MSw4Miw4Myw4NCw4NSw4Niw4Nyw4OCw4OSw5MCw5MSw5Miw5Myw5NCw5NSw5Niw5Nyw5OCw5OSwxMDEsMTAyLDEwMywxMDQsMTA1LDEwNiwxMDcsMTA5LDExMCwxMTIsMTEzLDExNCwxMTUsMTE2LDExNywxMTgsMTE5LDEyMCwxMjEsMTIyIiwiTG9naW5JZCI6ImFkbWluQGdldGFjLmNvbSIsIkZOYW1lIjoiU3VwZXIiLCJMTmFtZSI6IjEyMyIsIldvcmtzcGFjZUlkIjoiIiwiU3F1YWRJbmZvcyI6IltdIiwibmJmIjoxNzUwODYzMTUxLCJleHAiOjE3NTA4NjY3NTEsImlhdCI6MTc1MDg2MzE1MX0.GVl8MaPm766QEfjXfBcWWo5FThnxs-CqBHOhYqbQoeI";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = client.PostAsync(url, content).Result;

                Console.WriteLine("Status: " + response.StatusCode);
                Console.WriteLine("Response: " + response.Content.ReadAsStringAsync().Result);
            }
        }
    }
}
