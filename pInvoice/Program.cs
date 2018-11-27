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
        static void Main(string[] args)
        {
            Comprobante oComprobante = new Comprobante();
            oComprobante.Version = "3.3";
            oComprobante.Serie = "PF";
            oComprobante.Folio = "0002584";
            oComprobante.Fecha = DateTime.Now;
            oComprobante.Sello = "Faltante"; //siguiente video
            oComprobante.FormaPago = "99";
            oComprobante.NoCertificado = "3213213132121falta25";
            oComprobante.Certificado = ""; //faltante siguiente video
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
            listConceptos.Add(oConcepto);

            oComprobante.Conceptos = listConceptos.ToArray();
            //SERIALIZAMOS
            string pathXML = @"C:\Users\AngelesOsorno(Marath\Documents\FENEW\facturas\miPrimerXML.xml";

            XmlSerializer oXmlSerializar = new XmlSerializer(typeof(Comprobante));

            string sxml = "";

            using (var sw = new StringWriter())
            {
                using (XmlWriter writter = XmlWriter.Create(sw))
                {
                    oXmlSerializar.Serialize(writter, oComprobante);
                    sxml = sw.ToString();
                }
            }

            //Guardamos el string en un archivo
            System.IO.File.WriteAllText(pathXML, sxml);
        }
    }
}
