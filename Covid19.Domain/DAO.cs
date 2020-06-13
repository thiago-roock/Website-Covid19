using CsvHelper;
using Covid19.Domain.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Covid19.Domain
{
    public class DAO
    {
        string pathString;

        public DAO(string _pathString)
        {
            pathString = _pathString;
        }
        private DataCovid19 MontarObj(string dia, string mes, string ano)
        {


            if (File.Exists(pathString + mes + "-" + dia + "-" + ano + ".csv"))
            {
                var reader = EncontrarArquivo(pathString + mes + "-" + dia + "-" + ano + ".csv");

                if (reader != null)
                {
                    var resultado = PopularListaDataCovid19(reader);
                    return EncontrarPais(resultado);
                }
            }
            return null;
        }
        public async Task<Covid19Mensal> CasosDoMesAtual()
        {
            Covid19Mensal obj = new Covid19Mensal()
            {
                Mes = ReplacePorNomeDoMes(DateTime.Now.Month)
            };

            var quantidadeDeDias = DateTime.Now.Day == 1 ? DateTime.Now.Day : DateTime.Now.Day - 1;

            for (int qtdeDeAnalises = 0; qtdeDeAnalises < 3; qtdeDeAnalises++)
            {
                double primeiroCaso = 0;
                double ultimoCaso = 0;
                for (int i = 1; i <= quantidadeDeDias; i++)
                {
                    var objResultado = await ConstruirObjetoDeHoje(
                        AdicionarZero(i.ToString()),
                        AdicionarZero(DateTime.Now.Month.ToString()),
                        DateTime.Now.Year.ToString());

                    var InfectadosCurados = string.IsNullOrEmpty(objResultado.Recovered) ? 0.0 : double.Parse(objResultado.Recovered);
                    var Mortos = string.IsNullOrEmpty(objResultado.Deaths) ? 0.0 : double.Parse(objResultado.Deaths);
                    var InfectadosEmTratamento = string.IsNullOrEmpty(objResultado.Active) ? 0.0 : double.Parse(objResultado.Active);


                    switch (qtdeDeAnalises)
                    {
                        case 0:
                            {
                                if (InfectadosCurados > primeiroCaso && primeiroCaso == 0)
                                    primeiroCaso = InfectadosCurados;
                                else if (InfectadosCurados > ultimoCaso && primeiroCaso > 0)
                                    ultimoCaso = InfectadosCurados;


                                obj.InfectadosCurados = ultimoCaso <= primeiroCaso ? primeiroCaso : Total(primeiroCaso, ultimoCaso);
                                continue;
                            }
                        case 1:
                            {
                                if (Mortos > primeiroCaso && primeiroCaso == 0)
                                    primeiroCaso = Mortos;
                                else if (Mortos > ultimoCaso && primeiroCaso > 0)
                                    ultimoCaso = Mortos;


                                obj.Mortos = ultimoCaso <= primeiroCaso ? primeiroCaso : Total(primeiroCaso, ultimoCaso);
                                continue;
                            }
                        case 2:
                            {
                                if (InfectadosEmTratamento > primeiroCaso && primeiroCaso == 0)
                                    primeiroCaso = InfectadosEmTratamento;
                                else if (InfectadosEmTratamento > ultimoCaso && primeiroCaso > 0)
                                    ultimoCaso = InfectadosEmTratamento;


                                obj.InfectadosEmTratamento = ultimoCaso <= primeiroCaso ? primeiroCaso : Total(primeiroCaso, ultimoCaso);
                                continue;
                            }
                    }
                }

            }
            return obj;
        }
        public async Task<Covid19Mensal> CasosDoMes(int mes, int ano = 2020)
        {

            Covid19Mensal obj = new Covid19Mensal()
            {
                Mes = ReplacePorNomeDoMes(mes)
            };

            var quantidadeDeDias = DateTime.DaysInMonth(ano, mes);

            for (int qtdeDeAnalises = 0; qtdeDeAnalises < 3; qtdeDeAnalises++)
            {
                double primeiroCaso = 0;
                double ultimoCaso = 0;
                for (int i = 1; i <= quantidadeDeDias; i++)
                {
                    var objResultado = await ConstruirObjetoDeHoje(
                        AdicionarZero(i.ToString()),
                        AdicionarZero(mes.ToString()),
                        ano.ToString());

                    var InfectadosCurados = string.IsNullOrEmpty(objResultado.Recovered) ? 0.0 : double.Parse(objResultado.Recovered);
                    var Mortos = string.IsNullOrEmpty(objResultado.Deaths) ? 0.0 : double.Parse(objResultado.Deaths);
                    var InfectadosEmTratamento = string.IsNullOrEmpty(objResultado.Active) ? 0.0 : double.Parse(objResultado.Active);


                    switch (qtdeDeAnalises)
                    {
                        case 0:
                            {
                                if (InfectadosCurados > primeiroCaso && primeiroCaso == 0)
                                    primeiroCaso = InfectadosCurados;
                                else if (InfectadosCurados > ultimoCaso && primeiroCaso > 0)
                                    ultimoCaso = InfectadosCurados;


                                obj.InfectadosCurados = ultimoCaso <= primeiroCaso ? primeiroCaso : Total(primeiroCaso, ultimoCaso);
                                continue;
                            }
                        case 1:
                            {
                                if (Mortos > primeiroCaso && primeiroCaso == 0)
                                    primeiroCaso = Mortos;
                                else if (Mortos > ultimoCaso && primeiroCaso > 0)
                                    ultimoCaso = Mortos;


                                obj.Mortos = ultimoCaso <= primeiroCaso ? primeiroCaso : Total(primeiroCaso, ultimoCaso);
                                continue;
                            }
                        case 2:
                            {
                                if (InfectadosEmTratamento > primeiroCaso && primeiroCaso == 0)
                                    primeiroCaso = InfectadosEmTratamento;
                                else if (InfectadosEmTratamento > ultimoCaso && primeiroCaso > 0)
                                    ultimoCaso = InfectadosEmTratamento;


                                obj.InfectadosEmTratamento = ultimoCaso <= primeiroCaso ? primeiroCaso : Total(primeiroCaso, ultimoCaso);
                                continue;
                            }
                    }
                }

            }
            return obj;
        }
        public double Total(double primeiro, double ultimo)
        {
            return ultimo - primeiro;
        }
        public async Task<DataCovid19> ConstruirObjetoDeHoje(string dia, string mes, string ano)
        {
            var objDataCovid19 = MontarObj(dia, mes, ano);
            if (objDataCovid19 != null)
                return objDataCovid19;

            var client = new HttpClient();
            string queryString = "https://raw.githubusercontent.com/CSSEGISandData/COVID-19/master/csse_covid_19_data/csse_covid_19_daily_reports/" + mes + "-" + dia + "-" + ano + ".csv";
            //https://raw.githubusercontent.com/CSSEGISandData/COVID-19/master/csse_covid_19_data/csse_covid_19_daily_reports/05-20-2020.csv
            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(queryString);
                if (response.IsSuccessStatusCode)
                {
                    var rc = await response.Content.ReadAsStreamAsync();
                    using (var reader = new StreamReader(rc))
                    {
                        var resultado = PopularListaDataCovid19(reader);
                        
                        using (var sr = new StreamWriter(pathString + mes + "-" + dia + "-" + ano + ".csv", false, Encoding.UTF8))
                        {
                            using (var csv = new CsvWriter(sr, CultureInfo.InvariantCulture))
                            {
                                csv.WriteRecords(resultado);
                            }
                        }

                        return EncontrarPais(resultado);
                    }
                }
            }
            catch (Exception)
            {
                return new DataCovid19() { Deaths = "0", Active = "0", Confirmed = "0", Recovered = "0" };
            }

            return new DataCovid19() { Deaths = "0", Active = "0", Confirmed = "0", Recovered = "0" };
        }
        public List<DataCovid19> PopularListaDataCovid19(StreamReader reader)
        {
            return new CsvReader(reader, CultureInfo.InvariantCulture)
                            .GetRecords<DataCovid19>()
                            .ToList();
        }
        public StreamReader EncontrarArquivo(string NomeArquivo)
        {
            return new StreamReader(NomeArquivo);
        }
        public DataCovid19 EncontrarPais(List<DataCovid19> listCovid, string NomeDoPais = "Brazil")
        {
            List<DataCovid19> listCovidRetorno = new List<DataCovid19>();
            DataCovid19 dtCovid19 = new DataCovid19();
            if (listCovid != null)
                foreach (DataCovid19 pais in listCovid)
                    if (pais.Country_Region.Equals(NomeDoPais))
                        listCovidRetorno.Add(pais);

            int active = 0;
            int confirmed = 0;
            int deaths = 0;
            int recovered = 0;
            
            foreach (DataCovid19 dtC in listCovidRetorno) 
            {
                active += int.Parse(dtC.Active);
                confirmed += int.Parse(dtC.Confirmed);
                deaths += int.Parse(dtC.Deaths);
                recovered += int.Parse(dtC.Recovered);
                dtCovid19.Last_Update = dtC.Last_Update;
            }

            dtCovid19.Active = active.ToString();
            dtCovid19.Confirmed = confirmed.ToString();
            dtCovid19.Deaths = deaths.ToString();
            dtCovid19.Recovered = recovered.ToString();
            
            return dtCovid19;
        }
        public string AdicionarZero(string numero)
        {
            if (int.Parse(numero) <= 9)
                numero = "0" + numero;

            return numero;
        }
        public string ReplacePorNomeDoMes(int mes)
        {
            switch (mes)
            {
                case 1:
                    return "Janeiro";
                case 2:
                    return "Fevereiro";
                case 3:
                    return "Março";
                case 4:
                    return "Abril";
                case 5:
                    return "Maio";
                case 6:
                    return "Junho";
                case 7:
                    return "Julho";
                case 8:
                    return "Agosto";
                case 9:
                    return "Setembro";
                case 10:
                    return "Outubro";
                case 11:
                    return "Novembro";
                case 12:
                    return "Dezembro";
                default:
                    return "SwitchError";


            }
        }
        public async Task<Covid19Data> ConstruirListaParaGraficos()
        {
            string _dia = null;
            string _mes = null;
            string _ano = null;

            Covid19Data CovidData = new Covid19Data()
            {
                ListaMes = new List<Covid19Mensal>(),
                ListaDias = new List<Covid19Dia>()
            };

            for (int ano = 2020; ano <= DateTime.Now.Year; ano++)
            {
                _ano = ano.ToString();
                for (int mes = 1; mes <= DateTime.Now.Month; mes++)
                {
                    var quantidadeDeDias = 0;
                    if (DateTime.Now.Hour >= 21)
                        quantidadeDeDias = mes == DateTime.Now.Month ? DateTime.Now.Day : DateTime.DaysInMonth(ano, mes);
                    else
                        quantidadeDeDias = mes == DateTime.Now.Month ? DateTime.Now.Day - 1 : DateTime.DaysInMonth(ano, mes);

                    _mes = AdicionarZero(mes.ToString());
                    string MesNome = ReplacePorNomeDoMes(mes);

                    for (int dia = 1; dia <= quantidadeDeDias; dia++)
                    {
                        _dia = AdicionarZero(dia.ToString());
                        var objHj = await ConstruirObjetoDeHoje(_dia, _mes, _ano);

                        if (dia + 1 > quantidadeDeDias)
                        {
                            CovidData.ListaMes.Add(new Covid19Mensal()
                            {
                                Mes = MesNome,
                                Mortos = double.Parse(objHj.Deaths),
                                InfectadosEmTratamento = double.Parse(objHj.Active),
                                InfectadosCurados = double.Parse(objHj.Recovered),
                                CasosConfirmados = double.Parse(objHj.Confirmed)
                            });
                        }

                        CovidData.ListaDias.Add(new Covid19Dia()
                        {
                            Mes = MesNome,
                            Dia = dia,
                            Mortos = double.Parse(objHj.Deaths),
                            InfectadosEmTratamento = double.Parse(objHj.Active),
                            InfectadosCurados = double.Parse(objHj.Recovered),
                            CasosConfirmados = double.Parse(objHj.Confirmed)
                        });
                    }
                }
            }

            return CovidData;
        }
        public List<rssChannelItem> ListaNoticiasRssPorArquivo(string dia, string mes, string ano)
        {
            CrawlerXmlConvert cr = new CrawlerXmlConvert();

            List<rssChannelItem> lista = null;

            string caminho = pathString;
            string fileName = caminho + mes + "-" + dia + "-" + ano + ".xml";
            string resultado;

            if (File.Exists(fileName))
            {

                resultado = File.ReadAllText(fileName);

                lista = cr.DeserializeObject<List<rssChannelItem>>(resultado);
                int cont = 0;
                foreach(var i in lista)
                {
                    lista[cont].pubDate = Convert.ToDateTime(i.pubDate).ToString("D",
                    CultureInfo.CreateSpecificCulture("pt-BR"));
                    cont++;
                }


            }

            return lista;
        }
        public List<rssChannelItem> ListaNoticiasRssPorHttp()
        {
            string _dia = AdicionarZero((DateTime.Now.Day).ToString());
            string _mes = AdicionarZero(DateTime.Now.Month.ToString());
            string _ano = DateTime.Now.Year.ToString();
            List<rssChannelItem> listaNoticias = ListaNoticiasRssPorArquivo(_dia, _mes, _ano);
            try
            {
                

                CrawlerXmlConvert cr = new CrawlerXmlConvert();
                var resultado = cr.GetWebText("https://getpocket.com/users/*sso1574625684970c2f/feed/read");

                var dataR = cr.DeserializeObject<rss>(resultado);

               var listaNoticias2 = dataR.channel.item
                //.Where(l => DateTime.Parse(l.pubDate) >= DateTime.Now.AddDays(-1))
                .Select(cl => new rssChannelItem
                {
                    title = cl.title,
                    category = cl.category,
                    guid = cl.guid,
                    link = cl.link,
                    pubDate = Convert.ToDateTime(cl.pubDate).ToString("D",
                    CultureInfo.CreateSpecificCulture("pt-BR"))
                }).ToList();

                if (listaNoticias?.Count == listaNoticias2?.Count)
                    return listaNoticias;
                else
                    listaNoticias = listaNoticias2;

                var xml = cr.SerializeObject(dataR.channel.item);
                var caminho = pathString + _mes + "-" + _dia + "-" + _ano + ".xml";
                System.IO.File.WriteAllText(caminho, xml);
                
            }
            catch (Exception ex)
            {
             
              

            }

            return listaNoticias;
        }



       

    }
}
