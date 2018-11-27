using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace pInvoice
{
    class Program
    {
       
        static private string path = @"F:\ProyectoFactura33\";

        static private string pathXML = path + @"miPrimerXML.xml";


        static void Main(string[] args)
        {
            //Obtener numero certificado
            string pathCer = @"F:\ProyectoFactura33\CSDAAA010101AAA\CSD01_AAA010101AAA.cer";
            string pathKey = @"F:\ProyectoFactura33\CSDAAA010101AAA\CSD01_AAA010101AAA.key";
            string clavePrivada = "12345678a";


            //Obtenemos el numero de certificado
            string numeroCertificado, aa, b, c;
            SelloDigital.leerCER(pathCer, out aa, out b, out c, out numeroCertificado);


            //Llamamos la clase comprobante
            Comprobante oComprobante = new Comprobante();
            oComprobante.Version = "3.3";
            oComprobante.Serie = "PF";
            oComprobante.Folio = "0002584";
            oComprobante.Fecha = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            //oComprobante.Sello = "Faltante"; 
            oComprobante.FormaPago = "99";
            oComprobante.NoCertificado = numeroCertificado;
            //oComprobante.Certificado = "Faltante"; 
            oComprobante.SubTotal = 10m;
            oComprobante.Descuento = 1;
            oComprobante.Moneda = "MXN";
            //oComprobante.TipoCambio = 17.20m;
            oComprobante.Total = 9;
            oComprobante.TipoDeComprobante = "I";
            oComprobante.MetodoPago = "PUE";
            oComprobante.LugarExpedicion = "27000";
            

            ComprobanteEmisor oEmisor = new ComprobanteEmisor();

            oEmisor.Rfc = "XXXXXXXX";
            oEmisor.Nombre = "Marathon group";
            oEmisor.RegimenFiscal = "01";
            

            ComprobanteReceptor oReceptor = new ComprobanteReceptor();
            oReceptor.Nombre = "Pepe";
            oReceptor.Rfc = "7777777777777";
            oReceptor.UsoCFDI = "P01";


            oComprobante.Emisor = oEmisor;
            oComprobante.Receptor = oReceptor;


            List<ComprobanteConcepto> listConceptos = new List<ComprobanteConcepto>();

            ComprobanteConcepto oConcepto = new ComprobanteConcepto();

            oConcepto.Importe = 12.58m;
            oConcepto.ClaveProdServ = "1220";

            oConcepto.Descuento = 1;
            listConceptos.Add(oConcepto);

            oComprobante.Conceptos = listConceptos.ToArray();

            //Creamos el xml selleado
            CreateXML(oComprobante);

            string cadenaOriginal = "";
            string pathxsl = path + @"cadenaoriginal_3_3.xslt";

            System.Xml.Xsl.XslCompiledTransform transformador = new System.Xml.Xsl.XslCompiledTransform(true);
            transformador.Load(pathxsl);

            using (StringWriter sw = new StringWriter())
            {
                using (XmlWriter xwo = XmlWriter.Create(sw,transformador.OutputSettings))
                {
                    transformador.Transform(pathXML,xwo);
                    cadenaOriginal = sw.ToString();
                }
            }

            SelloDigital oSelloDigital = new SelloDigital();
            oComprobante.Certificado = oSelloDigital.Certificado(pathCer);
            oComprobante.Sello = oSelloDigital.Sellar(cadenaOriginal, pathKey, clavePrivada);

            CreateXML(oComprobante);

            //Timbre del XML

            ServiceReference.RespuestaCFDi respuestaCFDI = new ServiceReference.RespuestaCFDi();

            byte[] bXML = System.IO.File.ReadAllBytes(pathXML);

            ServiceReference.TimbradoClient oTimbrado = new ServiceReference.TimbradoClient();


            respuestaCFDI = oTimbrado.TimbrarTest("TEST010101ST1", "aaaaa", bXML);

            if (respuestaCFDI.Documento == null)
            {
                Console.WriteLine(respuestaCFDI.Mensaje);
            }
            else
            {
                System.IO.File.WriteAllBytes(pathXML, respuestaCFDI.Documento);
            }
        }

        private static void CreateXML(Comprobante oComprobante)
        {
            //SERIALIZAMOS
            XmlSerializerNamespaces xmlNameSpace = new XmlSerializerNamespaces();
            xmlNameSpace.Add("cfdi", "http://www.sat.gob.mx/cfd/3");
            xmlNameSpace.Add("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");
            xmlNameSpace.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");


            XmlSerializer oXmlSerializar = new XmlSerializer(typeof(Comprobante));

            string sxml = "";

            using (var sw = new StringWriterWithEncoding(Encoding.UTF8) /*StringWriter()*/)
            {
                using (XmlWriter writter = XmlWriter.Create(sw))
                {
                    oXmlSerializar.Serialize(writter, oComprobante,xmlNameSpace);
                    sxml = sw.ToString();
                }
            }

            //Guardamos el string en un archivo
            System.IO.File.WriteAllText(pathXML, sxml);
        }
    }
}
