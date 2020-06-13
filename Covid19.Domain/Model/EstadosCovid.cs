using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Covid19.Domain.Model
{
        public class EstadosCovid
    {
            public int count { get; set; }
            public string next { get; set; }
            public object previous { get; set; }
            public Result[] results { get; set; }
        }

        public class Result
        {
            public string city { get; set; }
            public string city_ibge_code { get; set; }
            public int confirmed { get; set; }
            public float? confirmed_per_100k_inhabitants { get; set; }
            public string date { get; set; }
            public float? death_rate { get; set; }
            public int deaths { get; set; }
            public int? estimated_population_2019 { get; set; }
            public bool is_last { get; set; }
            public int order_for_place { get; set; }
            public string place_type { get; set; }
            public string state { get; set; }
        }

    
}