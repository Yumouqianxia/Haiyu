namespace Waves.Core.Models.Handlers;

public class WavesGameHandler : HttpClientHandler
{
    public WavesGameHandler()
    {
        AutomaticDecompression = DecompressionMethods.All;
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
        {
            return true;
        };
    }
}