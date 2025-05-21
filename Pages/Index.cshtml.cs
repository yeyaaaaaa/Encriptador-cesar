using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace encriptador.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty] public string Remitente { get; set; }
        [BindProperty] public string Mensaje { get; set; }
        [BindProperty] public int Codigo { get; set; }
        [BindProperty] public DateTime Fecha { get; set; }
        [BindProperty] public IFormFile Archivo { get; set; }
        public string MensajeDescifrado { get; set; }

        public class MensajeSecreto
        {
            public string Remitente { get; set; }
            public string Mensaje { get; set; }
            public int Codigo { get; set; }
            public string Fecha { get; set; }
        }

        private string CifrarCesar(string texto, int desplazamiento)
        {
            StringBuilder resultado = new StringBuilder();

            foreach (char caracter in texto)
            {
                if (char.IsLetter(caracter))
                {
                    char baseLetra = char.IsUpper(caracter) ? 'A' : 'a';
                    resultado.Append((char)(baseLetra + (caracter - baseLetra + desplazamiento) % 26));
                }
                else
                {
                    resultado.Append(caracter);
                }
            }

            return resultado.ToString();
        }

        private string DescifrarCesar(string texto, int desplazamiento)
        {
            return CifrarCesar(texto, 26 - desplazamiento);
        }

        public void OnGet()
        {
            MensajeDescifrado = null;
        }

        public IActionResult OnPostGuardarXML()
        {
            string mensajeCifrado = CifrarCesar(Mensaje, 3);

            var mensajeSecreto = new MensajeSecreto
            {
                Remitente = Remitente,
                Mensaje = mensajeCifrado,
                Codigo = Codigo,
                Fecha = Fecha.ToString("dd-MM-yyyy")
            };

            var serializer = new XmlSerializer(typeof(MensajeSecreto));

            string carpetaMensajes = Path.Combine(Directory.GetCurrentDirectory(), "Mensajes");

            if (!Directory.Exists(carpetaMensajes))
            {
                Directory.CreateDirectory(carpetaMensajes);
            }

            string nombreArchivo = $"MensajeSecreto_{Remitente}.xml";
            string rutaCompleta = Path.Combine(carpetaMensajes, nombreArchivo);

            using (var fileStream = new FileStream(rutaCompleta, FileMode.Create))
            {
                serializer.Serialize(fileStream, mensajeSecreto);
            }

            TempData["Mensaje"] = "Mensaje secreto guardado correctamente.";
            return RedirectToPage(); 
            
        }

        public IActionResult OnPostCargarXML()
        {
            if (Archivo != null)
            {
                using (var stream = Archivo.OpenReadStream())
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MensajeSecreto));
                    var mensajeSecreto = (MensajeSecreto)serializer.Deserialize(stream);

                    string mensajeDescifrado = DescifrarCesar(mensajeSecreto.Mensaje,3);

                    TempData["MensajeDescifrado"] = $"Remitente: {mensajeSecreto.Remitente}<br/>" +
                                                    $"Mensaje: {mensajeDescifrado}<br/>" +
                                                    $"Código: {mensajeSecreto.Codigo}<br/>" +
                                                    $"Fecha: {mensajeSecreto.Fecha}";
                }
            }

            return RedirectToPage();
        }

    }
}