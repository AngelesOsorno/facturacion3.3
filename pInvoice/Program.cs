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
            oComprobante.Folio = "026711";
            oComprobante.Fecha = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            //oComprobante.Sello = "Faltante"; 
            oComprobante.FormaPago = "99";
            oComprobante.NoCertificado = numeroCertificado;
            //oComprobante.Certificado = "Faltante"; 
            oComprobante.SubTotal = 336.00m;
            oComprobante.Descuento = 0;
            oComprobante.Moneda = "MXN";
            //oComprobante.TipoCambio = 17.20m;
            oComprobante.Total = 389.76m;
            oComprobante.TipoDeComprobante = "I";
            oComprobante.MetodoPago = "PPD";
            oComprobante.LugarExpedicion = "27000";
            oComprobante.CondicionesDePago = "30 DIAS DE CREDITO";
            

            ComprobanteEmisor oEmisor = new ComprobanteEmisor();

            oEmisor.Rfc = "MEP900430F74";
            oEmisor.Nombre = "MARATHON ELECTRICA DE PUEBLA S.A. DE C.V.";
            oEmisor.RegimenFiscal = "601";
            

            ComprobanteReceptor oReceptor = new ComprobanteReceptor();
            oReceptor.Nombre = "JOCAR INGENIERIA EN MANTENIMIENTO S.A. DE C.V.";
            oReceptor.Rfc = "JIM8410225I2";
            oReceptor.UsoCFDI = "G01";


            oComprobante.Emisor = oEmisor;
            oComprobante.Receptor = oReceptor;


            List<ComprobanteConcepto> listConceptos = new List<ComprobanteConcepto>();

            ComprobanteConcepto oConcepto = new ComprobanteConcepto();
            oConcepto.ValorUnitario = 3.36m;
            oConcepto.Unidad = "M";
            oConcepto.NoIdentificacion = "SLYZ54 0";
            oConcepto.Importe = 336.00m;
            oConcepto.Descripcion = "CABLE THMW NYLON ROJO 16 AWG";
            oConcepto.ClaveProdServ = "26121634";
            oConcepto.Cantidad = 100m;

            listConceptos.Add(oConcepto);

            oComprobante.Conceptos = listConceptos.ToArray();

            List<ComprobanteImpuestosTraslado> listImpuestosTraslado = new List<ComprobanteImpuestosTraslado>();

            ComprobanteImpuestosTraslado oImpuestosTraslado = new ComprobanteImpuestosTraslado();
            oImpuestosTraslado.Importe = 53.76m;

            listImpuestosTraslado.Add(oImpuestosTraslado);
           

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
