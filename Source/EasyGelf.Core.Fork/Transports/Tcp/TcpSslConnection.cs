using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace EasyGelf.Core.Transports.Tcp
{
    public class TcpSslConnection : ITcpConnection
    {
        private readonly TcpTransportConfiguration configuration;

        private readonly TcpClient client;
        private SslStream sslStream;

        public TcpSslConnection(TcpTransportConfiguration configuration)
        {
            this.configuration = configuration;

            client = new TcpClient();
        }

        public void Open()
        {
            client.Connect(configuration.GetHost());
            sslStream = new SslStream(client.GetStream())
            {
                ReadTimeout = configuration.Timeout,
                WriteTimeout = configuration.Timeout
            };

            var x509CertificateCollection = new X509Certificate2Collection();

            if (!string.IsNullOrWhiteSpace(configuration.CertificatePath))
            {
                var certificate = new X509Certificate2(configuration.CertificatePath);
                x509CertificateCollection.Add(certificate);
            }

            sslStream.AuthenticateAsClient(configuration.GetServerNameInCertificate(), x509CertificateCollection,
                SslProtocols.Default, true);
        }

        public void Dispose()
        {
            if (sslStream != null)
            {
                sslStream.Close();
            }

            client.Close();
        }

        public Stream Stream
        {
            get { return sslStream; }
        }
    }
}