using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ToolsFrameWork.Tools
{
    public class ApiWithCert
    {
        public ApiWithCert()
        {

        }
        /// <summary>
        /// Metodo para poder enviar peticion POST a API
        /// Esta petición implementa el uso de certificado 
        /// </summary>
        public void ApiPost()
        {
            // url de servicio
            string urlApi = "";
            //data en este caso json
            string strContent = "{'prop':'datavalue'}";
            //var stringContent = new StringContent(strContent);
            //segmento donde se prepara el contenido o data para adjuntarlo en la petición post
            var stringContent = new StringContent(strContent, Encoding.UTF8, "application/json");
            //apartado donde se lee y carga el certificado en x5092
            X509Certificate2 clientCert = new X509Certificate2("");
            //segmento para adjuntar el certificado en handler
            WebRequestHandler requestHandler = new WebRequestHandler();
            requestHandler.ClientCertificates.Add(clientCert);

            //using para carga de httpclient y ejecución de petición a api  
            //se adjunta el certificado por medio del handler en el constructor
            using (HttpClient client = new HttpClient(requestHandler))
            {
                //se ejecuta la petición post a api
                HttpResponseMessage response = client.PostAsync(urlApi, stringContent).Result;
                response.EnsureSuccessStatusCode();
                var responseContent = response.Content.ReadAsStringAsync().Result;


            }
        }
    }
}
