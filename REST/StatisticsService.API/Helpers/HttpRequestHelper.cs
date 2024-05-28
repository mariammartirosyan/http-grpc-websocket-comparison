using System;
namespace StatisticsService.API.Helpers
{
	public class HttpRequestHelper
	{
		public static async Task<Response> SendGetRequest(string destinationUrl, IHeaderDictionary headers)
		{
            var request = new HttpRequestMessage(HttpMethod.Get, destinationUrl);

            //copy all the headers
            foreach (var header in headers)
            {
                if (!header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                }
            }
            var response = await new HttpClient().SendAsync(request);
            response.EnsureSuccessStatusCode();

            return new Response(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        }
	}
}

