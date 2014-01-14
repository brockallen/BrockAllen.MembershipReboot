using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Client;

namespace ClientApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var oauthClient = new OAuth2Client(new Uri("http://localhost:5034/token"), "client", "secret");
            try
            {
                var oauthresult = oauthClient.RequestResourceOwnerPasswordAsync("alice", "pass", "foo bar").Result;
                if (oauthresult.AccessToken != null)
                {
                    Console.WriteLine(oauthresult.AccessToken);
                    Console.WriteLine();

                    var client = new HttpClient();
                    client.SetBearerToken(oauthresult.AccessToken);
                    var result = client.GetAsync("http://localhost:5034/test").Result;
                    var json = result.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(json);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error, {0}", ex.Message);
            }
        }
    }
}
