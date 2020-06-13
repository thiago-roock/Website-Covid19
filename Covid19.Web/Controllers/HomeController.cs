using Covid19.Domain;
using Covid19.Domain.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Covid19.Web.Controllers
{
    public class HomeController : Controller
    {
        DAO dao;
        [Route("{dataBusca:string}")]
        public async Task<ActionResult> Index()
        {
            dao = new DAO(Server.MapPath("~/files/"));

            ViewBag.listaNoticias = dao.ListaNoticiasRssPorHttp();

            var _dia = dao.AdicionarZero((DateTime.Now.Day).ToString());
            var _mes = dao.AdicionarZero(DateTime.Now.Month.ToString());
            var  _ano = DateTime.Now.Year.ToString();
            
            DataCovid19 covidData;
            int subtrair = 0;
            int diaLoop = 0;
            bool mesanterior = false;
            do
            {
                if (!mesanterior)
                    diaLoop = string.IsNullOrEmpty(_dia) ? 0 : int.Parse(_dia) - subtrair;

                covidData = Task.Run(() => dao.ConstruirObjetoDeHoje(dao.AdicionarZero(diaLoop.ToString()), _mes, _ano)).Result;
                subtrair += 1;

                if (diaLoop == 0)
                {
                    _mes = dao.AdicionarZero((int.Parse(_mes) - 1).ToString());
                    diaLoop = DateTime.DaysInMonth(int.Parse(_ano), int.Parse(_mes));
                    mesanterior = true;
                }
                else if (covidData.Last_Update != null)
                    break;
            } while (covidData.Last_Update == null);


            covidData.Last_Update = Convert.ToDateTime(covidData.Last_Update).ToString("D",
            CultureInfo.CreateSpecificCulture("pt-BR"));

            ViewBag.CovidData = covidData;

            var casosDoMesAtual = await dao.CasosDoMesAtual();
            casosDoMesAtual.CasosConfirmados = Convert.ToDouble(covidData.Confirmed);
            ViewBag.CovidMesAtual = casosDoMesAtual;
            int mesAnterior = DateTime.Now.Month - 1;

            var casosDoMes = await dao.CasosDoMes(mesAnterior, DateTime.Now.Year);

            ViewBag.CovidMesAnterior = casosDoMes;
            return View();
        }

        public async Task<JsonResult> GetDataGraficoDias()
        {
            dao = new DAO(Server.MapPath("~/files/"));
            var lista = await dao.ConstruirListaParaGraficos();
            return Json(lista, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetDataGraficoCasosEstados()
        {
            List<ResultLine> listaEstadosFull = await CarregarPorHttp();
            return Json(listaEstadosFull, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> GetDataGraficoMortosEstados()
        {
            List<ResultLine> listaEstadosFull = await CarregarPorHttp();

            return Json(listaEstadosFull, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ListaNoticiasRss()
        {
            dao = new DAO(Server.MapPath("~/files/"));
            List<rssChannelItem> listaNoticias = dao.ListaNoticiasRssPorHttp();

            return Json(listaNoticias, JsonRequestBehavior.AllowGet);
        }
        public async Task<List<ResultLine>> CarregarPorHttp()
        {
            dao = new DAO(Server.MapPath("~/files/"));

            string _dia = dao.AdicionarZero((DateTime.Now.Day).ToString());
            string _mes = dao.AdicionarZero(DateTime.Now.Month.ToString());
            string _ano = DateTime.Now.Year.ToString();

            List<ResultLine> listaEstadosFull = CarregarPorArquivo(_dia, _mes, _ano);

            if (listaEstadosFull != null)
                return listaEstadosFull;

            var client = new HttpClient();
            string queryString = "https://brasil.io/api/dataset/covid19/caso/data/";

            HttpResponseMessage response;
            try
            {
                response = await client.GetAsync(queryString);
                if (response.IsSuccessStatusCode)
                {
                    var resultado = response.Content.ReadAsStringAsync().Result;
                    EstadosCovid dataJ = JsonConvert.DeserializeObject<EstadosCovid>(resultado.ToString());

                    listaEstadosFull = dataJ.results
                    .GroupBy(l => l.state)
                    .Select(cl => new ResultLine
                    {
                        Estado = cl.First().state,
                        Casos = cl.Sum(a => a.confirmed),
                        Mortos = cl.Sum(b => b.deaths).ToString(),
                    }).ToList();

                    var json = JsonConvert.SerializeObject(listaEstadosFull);
                    var caminho = Server.MapPath("~/files/") + _mes + "-" + _dia + "-" + _ano + ".json";
                    System.IO.File.WriteAllText(caminho, json);

                }

            }
            catch (Exception ex)
            {

            }

            return listaEstadosFull;
        }

        public List<ResultLine> CarregarPorArquivo(string dia, string mes, string ano)
        {
            string caminho = Server.MapPath("~/files/");
            string fileName = caminho + mes + "-" + dia + "-" + ano + ".json";
            string json;
            List<ResultLine> lista = null;
            if (System.IO.File.Exists(fileName))
            {
                json = System.IO.File.ReadAllText(fileName);

                lista = JsonConvert.DeserializeObject<List<ResultLine>>(json);
            }

            return lista;
        }

    }
}