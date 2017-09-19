namespace ForeignExchangeMac.Services
{
	using System.Threading.Tasks;
	using ForeignExchangeMac.Models;
    using System.Net.Http;
    using System;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using Plugin.Connectivity;
    using ForeignExchangeMac.Helpers;

    public class ApiService
    {
        public async Task<Response> CheckConnection()
        {
            if(!CrossConnectivity.Current.IsConnected)
            {
                return new Response
                { 
                    IsSuccess = false,
                    Messages = Lenguages.TitleSettingsInternet,
                    Result = null,
                };
            }

            var response = await CrossConnectivity.Current.IsRemoteReachable("google.com");
            if(!response)
            {
				return new Response
				{
					IsSuccess = false,
					Messages = Lenguages.TitleAccessInternet,
					Result = null,
				};
            }

    		return new Response
    		{
    			IsSuccess = true,
    			Messages = "Ok",
    			Result = null,
    		};
        }

        /// <summary>
        /// Metodo generico que devuelve un objeto List
        /// </summary>
        /// <returns>Object List</returns>
        /// <param name="urlBase">String URL base.</param>
        /// <param name="controller">String Controller.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public async Task<Response> GetList<T>(string urlBase, string controller)
        {
			try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(urlBase);
                var response = await client.GetAsync(controller);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Messages = result,
                        Result = null,
                    };
                }

                var list = JsonConvert.DeserializeObject<List<T>>(result);
                return new Response
                {
                    IsSuccess = true,
                    Messages = "Ok",
                    Result = list,
                };
			} catch (Exception ex) 
            {
                return new Response
				{
					IsSuccess = false,
                    Messages = ex.Message,
					Result = null,
				};
			} 
        }
    }
}
