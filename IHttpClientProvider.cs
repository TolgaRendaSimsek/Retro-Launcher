using System.Net.Http;

namespace RetroLauncher
{
    public interface IHttpClientProvider
    {
        HttpClient GetClient(string name);
    }
}
