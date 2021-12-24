using System.Net;
using System.Threading.Tasks;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;

namespace SpotifAds
{
    public class ProxyHandler
    {
        private ProxyServer proxyServer = new ProxyServer(userTrustRootCertificate: true, rootCertificateName: "SpotifAds Root Certificate", rootCertificateIssuerName: "SpotifAds Issuer Certificate");
        private ExplicitProxyEndPoint explicitProxyEndPoint = new ExplicitProxyEndPoint(IPAddress.Parse("127.0.0.1"), 2468, true);

        public async Task EnableProxy(int port)
        {
            explicitProxyEndPoint = new ExplicitProxyEndPoint(IPAddress.Parse("127.0.0.1"), port, true);
            proxyServer.AddEndPoint(explicitProxyEndPoint);
            explicitProxyEndPoint.BeforeTunnelConnectRequest += OnBeforeTunnelConnectRequest;
            proxyServer.BeforeRequest += OnBeforeRequest;

            proxyServer.Start();
            proxyServer.SetAsSystemProxy(explicitProxyEndPoint, ProxyProtocolType.AllHttp);

            await Task.Delay(10);
        }

        public async Task DisableProxy()
        {
            explicitProxyEndPoint.BeforeTunnelConnectRequest -= OnBeforeTunnelConnectRequest;
            proxyServer.BeforeRequest -= OnBeforeRequest;

            proxyServer.RemoveEndPoint(explicitProxyEndPoint);
            proxyServer.Stop();
            proxyServer.DisableAllSystemProxies();

            await Task.Delay(10);
        }

        private async Task OnBeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs e)
        {
            string hostname = e.HttpClient.Request.RequestUri.Host;

            if (!hostname.Contains("spotify"))
            {
                e.DecryptSsl = false;
            }
            await Task.CompletedTask;
        }

        private async Task OnBeforeRequest(object sender, SessionEventArgs e)
        {
            string absoluteUri = e.HttpClient.Request.RequestUri.AbsoluteUri;

            if (absoluteUri.Contains("spotify") && absoluteUri.Contains("ads"))
            {
                e.Respond(new Response());
            }
            await Task.CompletedTask;
        }
    }
}
