using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RdPodcastingWeb.Model
{
 
    public class Covid19Status
    {
        public double CasosConfirmados { get; set; }

        public double InfectadosEmTratamento { get; set; }

        public double Mortos { get; set; }

        public double InfectadosCurados { get; set; }
    }

    public class Covid19Mensal:Covid19Status
    {
        public string Mes { get; set; }
    }

    public class Covid19Dia: Covid19Mensal
    {
        public int Dia { get; set; }
    }


    public class Covid19Data
    {
        public List<Covid19Dia> ListaDias { get; set; }

        public List<Covid19Mensal> ListaMes { get; set; }
    }

    }