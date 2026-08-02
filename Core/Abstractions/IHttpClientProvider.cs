using System.Net.Http;

namespace RetroLauncher.Core.Abstractions
{
    public interface IHttpClientProvider
    {
        HttpClient GetClient(string name);
    }
}
