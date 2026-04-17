using System;

namespace SophiaWindowsService.Domain.Models
{
    public class AuditoriaResult
    {
        // col 0
        public int Id { get; set; }

        // col 1
        public string CodPreSgs { get; set; }

        // col 2
        public int CodSucur { get; set; }

        // col 3
        public string TipAdmision { get; set; }

        // col 4
        public int AnoAdm { get; set; }

        // col 5
        public int NumAdm { get; set; }

        // col 6
        public DateTime FechaTransaccion { get; set; }

        // col 7
        public int Reintentos { get; set; }

        // col 8
        public int Estado { get; set; }

        // col 9
        public string TipoRda { get; set; }

        // col 10
        public int Orden { get; set; }

        // col 11
        public string MetodoEnvio { get; set; }

        // col 12
        public string MensajeEnvio { get; set; }

        // col 13
        public string MensajeRespuesta { get; set; }

        // col 14
        public string Observacion { get; set; }

        // col 15
        public string BundleRda { get; set; }

        // col 16
        public string ClientId { get; set; }

        // col 17
        public string SecretId { get; set; }

        // col 18
        public string Localizacion { get; set; }

        // col 19
        public string CadenaConexion { get; set; }
    }
}