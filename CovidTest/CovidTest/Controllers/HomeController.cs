using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CovidTest.Models;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace CovidTest.Controllers
{
    public class SelectCountry { public string country { get; set; } }

    public enum MapLevel{
        Country=0,
        Administrative_area_level_1=1,
        Administrative_area_level_2=2,
        Administrative_area_level_3=3,
        Administrative_area_level_4=4,
        Administrative_area_level_5=5,
        postal_code=6
    }
    public class ResultMaps
    {
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
        public int Evaluation { get; set; }
        public int EvaluationGreen{ get; set; }
        public int EvaluationYellow { get; set; }
        public int EvaluationOrange { get; set; }
        public int EvaluationRed { get; set; }
    }
    public class ResultMapsDraw
    {
        public string Name { get; set; }
        public int Color { get; set; }
        public int Evaluations { get; set; }
        public int EvaluationGreen { get; set; }
        public int EvaluationYellow { get; set; }
        public int EvaluationOrange { get; set; }
        public int EvaluationRed { get; set; }
    }
    public class MapsShapes
    {
        public List<string> Maps { get; set; }
        public List<ResultMaps> ResultMaps { get; set; }
        public Dictionary<string,ResultMaps> ResultMapsDic { get; set; }
        public Dictionary<string, ResultMapsDraw> ResultMapsDrawDic { get; set; }
    }

    public class Latest
    {
        public int confirmed { get; set; }
        public int deaths { get; set; }
        public int recovered { get; set; }
    }

    public class Reservation
    {
        public Latest latest { get; set; }
    }

    public class UserIDs : TableEntity
    {
        public UserIDs()
        {
        }

        public UserIDs(int idUser, string Date)
        {
            this.PartitionKey = idUser.ToString();
            this.RowKey = Date;
        }

        public int IDACTUAL { get; set; }

    }
    public class Assesment : TableEntity
    {
        public Assesment()
        {
        }

        public Assesment(int idUser, string Date)
        {
            this.PartitionKey =  idUser.ToString();
            this.RowKey = Date;

        }
       
        public int Evaluation { get; set; }
        public bool Mobile { get; set; }

        public double LatBrowser { get; set; }
        public double LongBrowser { get; set; }
        public double LatPost { get; set; }
        public double LongPost { get; set; }

        public string Postal { get; set; }
        public string Country { get; set; }

        public string PreDiseases { get; set; }
        
        public int Age { get; set; }

        public bool Fever { get; set; }

        public bool Cough { get; set; }
        public bool DryCough { get; set; }

        public int Breath { get; set; }
        public int BreathTimes { get; set; }        
        public int BreathResults { get; set; }
        public string Condition { get; set; }

        public string Administrative_area_level_5 { get; set; }
        public string Administrative_area_level_4 { get; set; }
        public string Administrative_area_level_3 { get; set; }
        public string Administrative_area_level_2 { get; set; }
        public string Administrative_area_level_1 { get; set; }
    }
    public class Queue : TableEntity
    {
        public Queue()
        {
        }
        public Queue(string id, string DateTime)
        {
            this.PartitionKey = id.ToString();
            this.RowKey = DateTime;
        }
        public int State { get; set; }
        public DateTime Time { get; set; }

    }
    public class Map : TableEntity
    {
        public Map()
        {
        }

        public Map(string idMapa, string Date)
        {
            this.PartitionKey = idMapa.ToString();
            this.RowKey = Date;
        }

        public string IdCountry { get; set; }
        public string Country { get; set; }
        public string CountryNL { get; set; }
        public string IdAdministrative_area_level_1 { get; set; }
        public string Administrative_area_level_1 { get; set; }
        public string Administrative_area_level_1NL { get; set; }
        public string IdAdministrative_area_level_2 { get; set; }
        public string Administrative_area_level_2 { get; set; }
        public string Administrative_area_level_2NL { get; set; }
        public string IdAdministrative_area_level_3 { get; set; }
        public string Administrative_area_level_3 { get; set; }
        public string Administrative_area_level_3NL { get; set; }

        public string IdAdministrative_area_level_4 { get; set; }
        public string Administrative_area_level_4 { get; set; }
        public string Administrative_area_level_4NL { get; set; }

        public string IdAdministrative_area_level_5 { get; set; }
        public string Administrative_area_level_5 { get; set; }
        public string Administrative_area_level_5NL { get; set; }
    }


    public class Country : TableEntity
    {
        public Country()
        {
        }

        public Country(int Name, string Date)
        {
            this.PartitionKey = Name.ToString();
            this.RowKey = Date;
        }

        public string WRITELEVEL { get; set; }

        public string READLEVEL { get; set; }

        public string DRAWLEVEL { get; set; }

        public string QUERYLEVEL { get; set; }
    }

    public class HomeController : Controller
    {
        private IHttpContextAccessor _accessor;

        public HomeController(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        static string connectionString = "azureTablestorageaccountkey";
		
        public static string getTableAssessments()
        {
            return "AssessmentsEv";
        }
        public static string getTableUsers()
        {
            return "UserInfo";
        }
        public static string getTableMaps()
        {
            return "MapsJson";
        }
        public static string getTableUserIds()
        {
            return "UserIds";
        }
        public static string getTableQueues()
        {
            return "Queues";
        }

        public static string getTableCountries()
        {
            return "Countries";
        }
        public async Task<IActionResult> Index()
        {
            ViewData["Message"] = "HIA";
            Reservation reservationList = new Reservation();
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync("http://covid19api.xapix.io/v2/latest"))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    reservationList = JsonConvert.DeserializeObject<Reservation>(apiResponse);
                }
            }
            ViewData["Res"] = reservationList;
        

            return View();
        }

        public static async Task MergeAssesment(CloudTable table,Assesment ass)
        {
            TableOperation isertorMerge = TableOperation.InsertOrMerge(ass);
            TableResult res = await table.ExecuteAsync(isertorMerge);
            Assesment asse= res.Result as Assesment;

        }

        public static async Task MergeUserIDS(CloudTable table, UserIDs us)
        {
            TableOperation isertorMerge = TableOperation.InsertOrMerge(us);
            TableResult res = await table.ExecuteAsync(isertorMerge);
            UserIDs asse = res.Result as UserIDs;

        }

        public async Task<JsonResult> SaveAssesment(string UserID, 
                                                    string Mobile,
                                                    string Evaluation,
                                                    string LatBrowser,
                                                    string LongBrowser,
                                                    string LatPost,
                                                    string LongPost,
                                                    string Postal,
                                                    string Country,
                                                    string PreDiseases,
                                                    string Age,
                                                    string Fever,
                                                    string Cough,
                                                    string DryCough,
                                                    string Breath,
                                                    string BreathTimes,
                                                    string BreathResults,
                                                    string Condition,
                                                    string Administrative_area_level_5,
                                                    string Administrative_area_level_4,
                                                    string Administrative_area_level_3,
                                                    string Administrative_area_level_2,
                                                    string Administrative_area_level_1

            )
        {

            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient;
            tableClient = storageAccount.CreateCloudTableClient();
            int calculousuario = 0;
            if (string.IsNullOrEmpty(UserID))
            {
                CloudTable table = tableClient.GetTableReference(getTableUserIds());
                TableOperation tableOperation = TableOperation.Retrieve<UserIDs>("1", "1");
                TableResult bh = await table.ExecuteAsync(tableOperation);
                UserIDs usersids = bh.Result as UserIDs;
                usersids.IDACTUAL += 1;
                calculousuario = usersids.IDACTUAL;

                MergeUserIDS(table, usersids).Wait();

            }
            else
            {
                calculousuario = int.Parse(UserID);
            }

            bool MobileAss = Mobile.Equals("true") ? true : false;
            int EvaluationAss=int.Parse(Evaluation);
            double LatBrowserAss = double.Parse(LatBrowser, CultureInfo.InvariantCulture);
            double LongBrowserAss = double.Parse(LongBrowser, CultureInfo.InvariantCulture);
            double LatPostAss = double.Parse(LatPost, CultureInfo.InvariantCulture);
            double LongPostAss = double.Parse(LongPost, CultureInfo.InvariantCulture);
            string PostalAss = Postal;
            string CountryAss = Country;
            string PreDiseasesAss = PreDiseases;
            int AgeAss = Age.Equals("Age9") ? 9: Age.Equals("Age8")? 8 : Age.Equals("Age7") ? 7 : Age.Equals("Age6") ? 6 : Age.Equals("Age5") ? 5 :
                         Age.Equals("Age4") ? 4 : Age.Equals("Age3") ? 3 : Age.Equals("Age2") ? 2 : 1;
            bool FeverAss =  Fever.Equals("FeverYes") ? true : false;
            bool CoughAss =  Cough.Equals("CoughYes") ? true : false;
            bool DryCoughAss= DryCough.Equals("DryCoughYes") ? true : false;
            int BreathAss = int.Parse(Breath);
            int BreathTimesAss = int.Parse(BreathTimes);
            int BreathResultsAss = int.Parse(BreathResults);
            string ConditionAss = Condition;

            string fecha = DateTime.Now.Date.ToString("yyy-MM-dd");
            int resultado = int.Parse(Evaluation);

            if (Administrative_area_level_5 == null)
                Administrative_area_level_5 = "";
            if (Administrative_area_level_4 == null)
                Administrative_area_level_4 = "";
            if (Administrative_area_level_3 == null)
                Administrative_area_level_3 = "";
            if (Administrative_area_level_2 == null)
                Administrative_area_level_2 = "";
            if (Administrative_area_level_1 == null)
                Administrative_area_level_1 = "";

        
            Assesment asses = new Assesment(calculousuario, fecha)
            {
                Mobile = MobileAss,
                Evaluation = EvaluationAss,
                LatBrowser = LatBrowserAss,
                LongBrowser = LongBrowserAss,
                LatPost = LatPostAss,
                LongPost = LongPostAss,
                Postal = Postal,
                Country = Country,
                PreDiseases = PreDiseases,
                Age = AgeAss,
                Fever = FeverAss,
                Cough = CoughAss,
                DryCough = DryCoughAss,
                Breath = BreathAss,
                BreathTimes = BreathTimesAss,
                BreathResults = BreathResultsAss,
                Condition = Condition,
                Administrative_area_level_5 = Administrative_area_level_5,
                Administrative_area_level_4 = Administrative_area_level_4,
                Administrative_area_level_3 = Administrative_area_level_3,
                Administrative_area_level_2 = Administrative_area_level_2,
                Administrative_area_level_1 = Administrative_area_level_1

            };

            string root = "";
            try
            {

                CloudTable table = tableClient.GetTableReference(getTableAssessments());
                MergeAssesment(table, asses).Wait();

                root = calculousuario.ToString();
            }

            catch (Exception e)
            {
                root = "";

            }

            return Json(root);
        }

        public JsonResult GetInfo(string ZipCode, string fechaIni,string fechaFin)
        {
            List<ResultMaps> lista = new List<ResultMaps>();
            Random ran = new Random(DateTime.Now.Millisecond);
          
            for(int i=0;i <100;i++)
            {
            ResultMaps root = new ResultMaps();
            string latString = "0.000" + ran.Next(60).ToString();
                double latdo = double.Parse(latString, CultureInfo.InvariantCulture);
            root.Lat = 41.378633 + latdo;
            root.Long = 2.1523914;
            root.Evaluation = 1;
            lista.Add(root);
            }
            
            return Json(lista);
        }

        public async Task<JsonResult> SaveQueueInfo(string Queue, string Place)
        {
            bool res = false;
            try
            {
                CloudStorageAccount storageAccount;
                storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudTableClient tableClient;
                tableClient = storageAccount.CreateCloudTableClient();

                string fecha = DateTime.Now.Date.ToString("yyy-MM-dd");
                CloudTable table = tableClient.GetTableReference(getTableQueues());
                Queue que = new Queue(Place, fecha)
                {
                    State = int.Parse(Queue),
                    Time = DateTime.Now
                };
                TableOperation isertorMerge = TableOperation.InsertOrMerge(que);
                TableResult result = await table.ExecuteAsync(isertorMerge);
                Queue asse = result.Result as Queue;
                
                return Json(asse);
            }
            catch
            {
               
            }
            return Json(res);
        }

        public JsonResult GetServerTime()
        {
            DateTime date = DateTime.Now;
            return Json(date);
        }

        public JsonResult GetInfoQueues()
        {
            Dictionary<string, Queue> lista = new Dictionary<string, Queue>();

            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient;
            tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(getTableQueues());
            DateTime fechaquery = DateTime.Now;
            string fechaDatequery = fechaquery.Date.ToString("yyy-MM-dd");

            TableQuery<Queue> rangeQuery = new TableQuery<Queue>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, fechaDatequery));
            TableContinuationToken continuationToken = null;
            var entities = table.ExecuteQuerySegmentedAsync(rangeQuery, continuationToken).Result;

            foreach (Queue qu in entities)
            {
                if (lista.ContainsKey(qu.PartitionKey))
                {
                    if(qu.Time > lista[qu.PartitionKey].Time)
                    {
                        lista[qu.PartitionKey]= qu;
                    }
                }
                else
                {
                    lista.Add(qu.PartitionKey, qu);
                }               
            }


            return Json(lista);
        }
        public JsonResult GetInfoCountries()
        {
            Dictionary<string, Country> lista = new Dictionary<string, Country>();

            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient tableClient;
            tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(getTableCountries());
            TableQuery<Country> rangeQuery = new TableQuery<Country>();  
            TableContinuationToken continuationToken = null;
            var entities = table.ExecuteQuerySegmentedAsync(rangeQuery, continuationToken).Result;
            foreach (Country qu in entities)
            {
                if (lista.ContainsKey(qu.PartitionKey))
                {
                        lista[qu.PartitionKey] = qu;
                }
                else
                {
                    lista.Add(qu.PartitionKey, qu);
                }
            }


            return Json(lista);
        }


        public JsonResult GetInfoMaps(string LatCtr, string LngCtr, string Country, string Level)
        {
            CloudStorageAccount storageAccount;
            storageAccount = CloudStorageAccount.Parse(connectionString);
            MapsShapes lista = new MapsShapes();
            Dictionary<string, Country> CountryDefinition = new Dictionary<string, Country>();
            Dictionary<string, ResultMapsDraw> MapsIds = new Dictionary<string, ResultMapsDraw>();

            CloudTableClient tableClient;
            tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(getTableCountries());
            TableQuery<Country> rangeQueryCountry = new TableQuery<Country>();
            TableContinuationToken continuationToken = null;
            var entitiesCountry = table.ExecuteQuerySegmentedAsync(rangeQueryCountry, continuationToken).Result;
            foreach (Country qu in entitiesCountry)
            {
                if (CountryDefinition.ContainsKey(qu.PartitionKey))
                {
                    CountryDefinition[qu.PartitionKey] = qu;
                }
                else
                {
                    CountryDefinition.Add(qu.PartitionKey, qu);
                }
            }
            //ReadLevel:  ReadAssesment == mapa google
            //DrawLevel:  Agrupacion Mapas
            //QueryLevel: Google consulta y Assesment consulta
            //QueryLevel: Google consulta y Assesment consulta
            //WriteLevel: google almacena y assessment dibuja punto
            string readLevel = MapLevel.Administrative_area_level_3.ToString();
            if (CountryDefinition.ContainsKey(Country))
            {
                readLevel = CountryDefinition[Country].READLEVEL;
            }
            string drawLevel = MapLevel.Administrative_area_level_3.ToString();
            if (CountryDefinition.ContainsKey(Country))
            {
                drawLevel = CountryDefinition[Country].DRAWLEVEL;
            }
            string queryLevel = MapLevel.Administrative_area_level_2.ToString();
            if (CountryDefinition.ContainsKey(Country))
            {
                queryLevel = CountryDefinition[Country].QUERYLEVEL;
            }

            var PuntosLevel = MapLevel.postal_code.ToString();
            if (CountryDefinition.ContainsKey(Country))
            {
                PuntosLevel = CountryDefinition[Country].WRITELEVEL;
            }
            

            tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference(getTableMaps());
            TableQuery<Map> rangeQueryMap = new TableQuery<Map>()
            .Where(TableQuery.GenerateFilterCondition(queryLevel, QueryComparisons.Equal, Level));
            continuationToken = null;
            var entitiesMap = table.ExecuteQuerySegmentedAsync(rangeQueryMap, continuationToken).Result;
            lista.Maps = new List<string>();
            foreach (Map mp in entitiesMap)
            {
                string path = "https://"+_accessor.HttpContext.Request.Host.Value.ToString()+ "/Shapes/"+Country+"/";
                lista.Maps.Add(path + mp.PartitionKey + ".json");

                if(drawLevel== "Administrative_area_level_1" && !MapsIds.ContainsKey(mp.Administrative_area_level_1))
                {
                    MapsIds.Add(mp.Administrative_area_level_1, new ResultMapsDraw());
                }
                else if (drawLevel == "Administrative_area_level_2" && !MapsIds.ContainsKey(mp.Administrative_area_level_2))
                {
                    MapsIds.Add(mp.Administrative_area_level_2, new ResultMapsDraw());
                }
                else if (drawLevel == "Administrative_area_level_3" && !MapsIds.ContainsKey(mp.Administrative_area_level_3))
                {
                    MapsIds.Add(mp.Administrative_area_level_3, new ResultMapsDraw());
                }
                else if (drawLevel == "Administrative_area_level_4" && !MapsIds.ContainsKey(mp.Administrative_area_level_4))
                {
                    MapsIds.Add(mp.Administrative_area_level_4, new ResultMapsDraw());
                }
                else if (drawLevel == "Administrative_area_level_5" && !MapsIds.ContainsKey(mp.Administrative_area_level_5))
                {
                    MapsIds.Add(mp.Administrative_area_level_5, new ResultMapsDraw());
                }
            }

            tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference(getTableAssessments());
            TableQuery<Assesment> rangeQueryAssesments = new TableQuery<Assesment>()
            .Where(TableQuery.GenerateFilterCondition(queryLevel, QueryComparisons.Equal, Level));
            continuationToken = null;

            Dictionary<string, ResultMaps> CalculoMapas = new Dictionary<string, ResultMaps>();
            var entitiesAssesment = table.ExecuteQuerySegmentedAsync(rangeQueryAssesments, continuationToken).Result;
            foreach (Assesment mp in entitiesAssesment)
            {
                string nombreCirculo = mp.Postal;
                if(CalculoMapas.ContainsKey(nombreCirculo))
                {
                    CalculoMapas[nombreCirculo].Lat = mp.LatPost;
                    CalculoMapas[nombreCirculo].Long = mp.LongPost;
                    CalculoMapas[nombreCirculo].Name = mp.Postal;
                    if (mp.Evaluation==1)
                    {
                        CalculoMapas[nombreCirculo].EvaluationGreen++;
                    }
                    else if (mp.Evaluation == 2)
                    {
                        CalculoMapas[nombreCirculo].EvaluationYellow++;
                    }
                    else if (mp.Evaluation == 3)
                    {
                        CalculoMapas[nombreCirculo].EvaluationOrange++;
                    }
                    else if (mp.Evaluation == 4)
                    {
                        CalculoMapas[nombreCirculo].EvaluationRed++;
                    }
                }
                else
                {
                    CalculoMapas.Add(nombreCirculo, new ResultMaps());
                    CalculoMapas[nombreCirculo].Lat = mp.LatPost;
                    CalculoMapas[nombreCirculo].Long = mp.LongPost;
                    CalculoMapas[nombreCirculo].Name = mp.Postal;

                    if (mp.Evaluation == 1)
                    {
                        CalculoMapas[nombreCirculo].EvaluationGreen++;
                    }
                    else if (mp.Evaluation == 2)
                    {
                        CalculoMapas[nombreCirculo].EvaluationYellow++;
                    }
                    else if (mp.Evaluation == 3)
                    {
                        CalculoMapas[nombreCirculo].EvaluationOrange++;
                    }
                    else if (mp.Evaluation == 4)
                    {
                        CalculoMapas[nombreCirculo].EvaluationRed++;
                    }
                }

                string nombreMapa = mp.Administrative_area_level_3;

                if (readLevel == "Administrative_area_level_1")
                    nombreMapa = mp.Administrative_area_level_1;
                if (readLevel == "Administrative_area_level_2")
                    nombreMapa = mp.Administrative_area_level_2;
                if (readLevel == "Administrative_area_level_3")
                    nombreMapa = mp.Administrative_area_level_3;
                if (readLevel == "Administrative_area_level_4")
                    nombreMapa = mp.Administrative_area_level_4;
                if (readLevel == "Administrative_area_level_5")
                    nombreMapa = mp.Administrative_area_level_5;

                if (MapsIds.ContainsKey(nombreMapa))
                {
                    MapsIds[nombreMapa].Name = nombreMapa;
                    if (mp.Evaluation == 1)
                    {
                        MapsIds[nombreMapa].EvaluationGreen++;
                    }
                    else if (mp.Evaluation == 2)
                    {
                        MapsIds[nombreMapa].EvaluationYellow++;
                    }
                    else if (mp.Evaluation == 3)
                    {
                        MapsIds[nombreMapa].EvaluationOrange++;
                    }
                    else if (mp.Evaluation == 4)
                    {
                        MapsIds[nombreMapa].EvaluationRed++;
                    }
                    MapsIds[nombreMapa].Evaluations++;
                }
            }

            foreach(KeyValuePair<string, ResultMapsDraw> item in MapsIds)
            {
                double total = item.Value.Evaluations;
                if(total!=0)
                {
                    double porcenRisk = (item.Value.EvaluationOrange+ item.Value.EvaluationRed+ item.Value.EvaluationYellow) / total;

                    if (porcenRisk > 0.01)
                    {
                        int mayor = 4;
                        int cant = item.Value.EvaluationRed;

                        if(item.Value.EvaluationOrange >cant)
                        {
                            mayor = 3;
                            cant = item.Value.EvaluationOrange;
                        }
                        if (item.Value.EvaluationYellow > cant)
                        {
                            mayor = 2;
                            cant = item.Value.EvaluationYellow;
                        }
                        item.Value.Color = mayor;
                    }
                    else
                    {
                        item.Value.Color = 1;
                    }

                }

            }
            lista.ResultMaps = CalculoMapas.Values.ToList();
            lista.ResultMapsDic = CalculoMapas;
            lista.ResultMapsDrawDic = MapsIds;
            return Json(lista);
        }

        public IActionResult Decision()
        {
            ViewData["Message"] = "Your application description page.";
            ViewData["Construction"] = true;
            return View();
        }

        public IActionResult Assessment()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }
        public IActionResult Breathing()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }
        public IActionResult HeatMaps()
        {
            ViewData["Message"] = "Your application description page.";
            ViewData["Construction"] = true;
            Assesment user = getUser();
            ViewData["User"] = user;

            List<SelectListItem> Countries = new List<SelectListItem>();
            foreach (string country in CountryArrays.Names)
            {
                SelectListItem li = new SelectListItem();
                li.Text = country;
                li.Value = country;
                Countries.Add(li);
            }
            ViewData["Countries"] = Countries;
            return View();
        }
        public IActionResult TestMaps()
        {
            ViewData["Message"] = "Your application description page.";
            ViewData["Construction"] = true;
            Assesment user = getUser();
            ViewData["User"] = user;

            List<SelectListItem> Countries = new List<SelectListItem>();
            foreach (string country in CountryArrays.Names)
            {
                SelectListItem li = new SelectListItem();
                li.Text = country;
                li.Value = country;
                Countries.Add(li);
            }
            ViewData["Countries"] = Countries;
            return View();
        }

        public IActionResult Queues()
        {
            ViewData["Construction"] = false;
            ViewData["Message"] = "Your application description page.";
            Assesment user = getUser();
            ViewData["User"] = user;

            List<SelectListItem> Countries = new List<SelectListItem>();
            foreach(string country in CountryArrays.Names)
            {
                SelectListItem li = new SelectListItem();
                li.Text = country;
                li.Value = country;
                Countries.Add(li);
            }
            ViewData["Countries"] = Countries;
            return View();
        }

        public IActionResult MyNeighborhood()
        {
   

            ViewData["Construction"] = false;

            Assesment user = getUser();

            ViewData["User"] = user;
            
            return View();
        }

        public Assesment getUser()
        {
            if (HttpContext.Request.Cookies.ContainsKey("UserID"))
            {
                CloudStorageAccount storageAccount;
                storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudTableClient tableClient;
                tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference(getTableAssessments());
                string userId = HttpContext.Request.Cookies["UserID"];
                TableQuery<Assesment> rangeQuery = new TableQuery<Assesment>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));
                TableContinuationToken continuationToken = null;
                var entities = table.ExecuteQuerySegmentedAsync(rangeQuery, continuationToken).Result;
                Dictionary<DateTime, Assesment> listaAssesmentes = new Dictionary<DateTime, Assesment>();
                DateTime fecha = new DateTime();
                foreach (Assesment ass in entities)
                {
                    if (DateTime.Parse(ass.RowKey) > fecha)
                    {
                        fecha = DateTime.Parse(ass.RowKey);
                    }

                    listaAssesmentes.Add(DateTime.Parse(ass.RowKey), ass);
                }
                if (fecha != new DateTime())
                {
                    return listaAssesmentes[fecha];
                }
            }
            else
            {
                return new Assesment(0, "");
            }
            return new Assesment(0, "");
        }

        public async Task<IActionResult> Results()
        {

            if(HttpContext.Request.Cookies.ContainsKey("UserID"))
            {
                CloudStorageAccount storageAccount;
                storageAccount = CloudStorageAccount.Parse(connectionString);
                CloudTableClient tableClient;
                tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference(getTableAssessments());
                string userId= HttpContext.Request.Cookies["UserID"];

                TableQuery<Assesment> rangeQuery = new TableQuery<Assesment>()
      .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId));
                TableContinuationToken continuationToken = null;
                var entities = table.ExecuteQuerySegmentedAsync(rangeQuery, continuationToken).Result;

                Dictionary<DateTime,Assesment> listaAssesmentes = new Dictionary<DateTime,Assesment>();
                DateTime fecha = new DateTime();
                foreach (Assesment ass in entities)
                {
                    if(DateTime.Parse(ass.RowKey) > fecha)
                    {
                        fecha = DateTime.Parse(ass.RowKey);
                    }

                    listaAssesmentes.Add(DateTime.Parse(ass.RowKey), ass);
                }

                ViewData["UserID"] = HttpContext.Request.Cookies["UserID"];

                if(fecha!=new DateTime())
                ViewData["Results"] = listaAssesmentes[fecha];
                else
                    ViewData["Results"] = new Assesment(0,"");
            }
            return View();
        }

        public IActionResult Team()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }


        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Terms()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        private double distance(double lat1, double lon1, double lat2, double lon2, char unit)
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                return 0;
            }
            else
            {
                double theta = lon1 - lon2;
                double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
                dist = Math.Acos(dist);
                dist = rad2deg(dist);
                dist = dist * 60 * 1.1515;
                if (unit == 'K')
                {
                    dist = dist * 1.609344;
                }
                else if (unit == 'N')
                {
                    dist = dist * 0.8684;
                }
                return (dist);
            }
        }
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts decimal degrees to radians             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        //::  This function converts radians to decimal degrees             :::
        //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
        private double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }

    }

    static class CountryArrays
    {
        /// <summary>
        /// Country names
        /// </summary>
        public static string[] Names = new string[]
        {
    "Afghanistan",
    "Albania",
    "Algeria",
    "American Samoa",
    "Andorra",
    "Angola",
    "Anguilla",
    "Antarctica",
    "Antigua and Barbuda",
    "Argentina",
    "Armenia",
    "Aruba",
    "Australia",
    "Austria",
    "Azerbaijan",
    "Bahamas",
    "Bahrain",
    "Bangladesh",
    "Barbados",
    "Belarus",
    "Belgium",
    "Belize",
    "Benin",
    "Bermuda",
    "Bhutan",
    "Bolivia",
    "Bosnia and Herzegovina",
    "Botswana",
    "Bouvet Island",
    "Brazil",
    "British Indian Ocean Territory",
    "Brunei Darussalam",
    "Bulgaria",
    "Burkina Faso",
    "Burundi",
    "Cambodia",
    "Cameroon",
    "Canada",
    "Cape Verde",
    "Cayman Islands",
    "Central African Republic",
    "Chad",
    "Chile",
    "China",
    "Christmas Island",
    "Cocos (Keeling) Islands",
    "Colombia",
    "Comoros",
    "Congo",
    "Congo, the Democratic Republic of the",
    "Cook Islands",
    "Costa Rica",
    "Cote D'Ivoire",
    "Croatia",
    "Cuba",
    "Cyprus",
    "Czech Republic",
    "Denmark",
    "Djibouti",
    "Dominica",
    "Dominican Republic",
    "Ecuador",
    "Egypt",
    "El Salvador",
    "Equatorial Guinea",
    "Eritrea",
    "Estonia",
    "Ethiopia",
    "Falkland Islands (Malvinas)",
    "Faroe Islands",
    "Fiji",
    "Finland",
    "France",
    "French Guiana",
    "French Polynesia",
    "French Southern Territories",
    "Gabon",
    "Gambia",
    "Georgia",
    "Germany",
    "Ghana",
    "Gibraltar",
    "Greece",
    "Greenland",
    "Grenada",
    "Guadeloupe",
    "Guam",
    "Guatemala",
    "Guinea",
    "Guinea-Bissau",
    "Guyana",
    "Haiti",
    "Heard Island and Mcdonald Islands",
    "Holy See (Vatican City State)",
    "Honduras",
    "Hong Kong",
    "Hungary",
    "Iceland",
    "India",
    "Indonesia",
    "Iran, Islamic Republic of",
    "Iraq",
    "Ireland",
    "Israel",
    "Italy",
    "Jamaica",
    "Japan",
    "Jordan",
    "Kazakhstan",
    "Kenya",
    "Kiribati",
    "Korea, Democratic People's Republic of",
    "Korea, Republic of",
    "Kuwait",
    "Kyrgyzstan",
    "Lao People's Democratic Republic",
    "Latvia",
    "Lebanon",
    "Lesotho",
    "Liberia",
    "Libyan Arab Jamahiriya",
    "Liechtenstein",
    "Lithuania",
    "Luxembourg",
    "Macao",
    "Macedonia, the Former Yugoslav Republic of",
    "Madagascar",
    "Malawi",
    "Malaysia",
    "Maldives",
    "Mali",
    "Malta",
    "Marshall Islands",
    "Martinique",
    "Mauritania",
    "Mauritius",
    "Mayotte",
    "Mexico",
    "Micronesia, Federated States of",
    "Moldova, Republic of",
    "Monaco",
    "Mongolia",
    "Montserrat",
    "Morocco",
    "Mozambique",
    "Myanmar",
    "Namibia",
    "Nauru",
    "Nepal",
    "Netherlands",
    "Netherlands Antilles",
    "New Caledonia",
    "New Zealand",
    "Nicaragua",
    "Niger",
    "Nigeria",
    "Niue",
    "Norfolk Island",
    "Northern Mariana Islands",
    "Norway",
    "Oman",
    "Pakistan",
    "Palau",
    "Palestinian Territory, Occupied",
    "Panama",
    "Papua New Guinea",
    "Paraguay",
    "Peru",
    "Philippines",
    "Pitcairn",
    "Poland",
    "Portugal",
    "Puerto Rico",
    "Qatar",
    "Reunion",
    "Romania",
    "Russian Federation",
    "Rwanda",
    "Saint Helena",
    "Saint Kitts and Nevis",
    "Saint Lucia",
    "Saint Pierre and Miquelon",
    "Saint Vincent and the Grenadines",
    "Samoa",
    "San Marino",
    "Sao Tome and Principe",
    "Saudi Arabia",
    "Senegal",
    "Serbia and Montenegro",
    "Seychelles",
    "Sierra Leone",
    "Singapore",
    "Slovakia",
    "Slovenia",
    "Solomon Islands",
    "Somalia",
    "South Africa",
    "South Georgia and the South Sandwich Islands",
    "Spain",
    "Sri Lanka",
    "Sudan",
    "Suriname",
    "Svalbard and Jan Mayen",
    "Swaziland",
    "Sweden",
    "Switzerland",
    "Syrian Arab Republic",
    "Taiwan, Province of China",
    "Tajikistan",
    "Tanzania, United Republic of",
    "Thailand",
    "Timor-Leste",
    "Togo",
    "Tokelau",
    "Tonga",
    "Trinidad and Tobago",
    "Tunisia",
    "Turkey",
    "Turkmenistan",
    "Turks and Caicos Islands",
    "Tuvalu",
    "Uganda",
    "Ukraine",
    "United Arab Emirates",
    "United Kingdom",
    "United States",
    "United States Minor Outlying Islands",
    "Uruguay",
    "Uzbekistan",
    "Vanuatu",
    "Venezuela",
    "Viet Nam",
    "Virgin Islands, British",
    "Virgin Islands, US",
    "Wallis and Futuna",
    "Western Sahara",
    "Yemen",
    "Zambia",
    "Zimbabwe",
        };

        /// <summary>
        /// Country abbreviations
        /// </summary>
        public static string[] Abbreviations = new string[]
        {
    "AF",
    "AL",
    "DZ",
    "AS",
    "AD",
    "AO",
    "AI",
    "AQ",
    "AG",
    "AR",
    "AM",
    "AW",
    "AU",
    "AT",
    "AZ",
    "BS",
    "BH",
    "BD",
    "BB",
    "BY",
    "BE",
    "BZ",
    "BJ",
    "BM",
    "BT",
    "BO",
    "BA",
    "BW",
    "BV",
    "BR",
    "IO",
    "BN",
    "BG",
    "BF",
    "BI",
    "KH",
    "CM",
    "CA",
    "CV",
    "KY",
    "CF",
    "TD",
    "CL",
    "CN",
    "CX",
    "CC",
    "CO",
    "KM",
    "CG",
    "CD",
    "CK",
    "CR",
    "CI",
    "HR",
    "CU",
    "CY",
    "CZ",
    "DK",
    "DJ",
    "DM",
    "DO",
    "EC",
    "EG",
    "SV",
    "GQ",
    "ER",
    "EE",
    "ET",
    "FK",
    "FO",
    "FJ",
    "FI",
    "FR",
    "GF",
    "PF",
    "TF",
    "GA",
    "GM",
    "GE",
    "DE",
    "GH",
    "GI",
    "GR",
    "GL",
    "GD",
    "GP",
    "GU",
    "GT",
    "GN",
    "GW",
    "GY",
    "HT",
    "HM",
    "VA",
    "HN",
    "HK",
    "HU",
    "IS",
    "IN",
    "ID",
    "IR",
    "IQ",
    "IE",
    "IL",
    "IT",
    "JM",
    "JP",
    "JO",
    "KZ",
    "KE",
    "KI",
    "KP",
    "KR",
    "KW",
    "KG",
    "LA",
    "LV",
    "LB",
    "LS",
    "LR",
    "LY",
    "LI",
    "LT",
    "LU",
    "MO",
    "MK",
    "MG",
    "MW",
    "MY",
    "MV",
    "ML",
    "MT",
    "MH",
    "MQ",
    "MR",
    "MU",
    "YT",
    "MX",
    "FM",
    "MD",
    "MC",
    "MN",
    "MS",
    "MA",
    "MZ",
    "MM",
    "NA",
    "NR",
    "NP",
    "NL",
    "AN",
    "NC",
    "NZ",
    "NI",
    "NE",
    "NG",
    "NU",
    "NF",
    "MP",
    "NO",
    "OM",
    "PK",
    "PW",
    "PS",
    "PA",
    "PG",
    "PY",
    "PE",
    "PH",
    "PN",
    "PL",
    "PT",
    "PR",
    "QA",
    "RE",
    "RO",
    "RU",
    "RW",
    "SH",
    "KN",
    "LC",
    "PM",
    "VC",
    "WS",
    "SM",
    "ST",
    "SA",
    "SN",
    "CS",
    "SC",
    "SL",
    "SG",
    "SK",
    "SI",
    "SB",
    "SO",
    "ZA",
    "GS",
    "ES",
    "LK",
    "SD",
    "SR",
    "SJ",
    "SZ",
    "SE",
    "CH",
    "SY",
    "TW",
    "TJ",
    "TZ",
    "TH",
    "TL",
    "TG",
    "TK",
    "TO",
    "TT",
    "TN",
    "TR",
    "TM",
    "TC",
    "TV",
    "UG",
    "UA",
    "AE",
    "GB",
    "US",
    "UM",
    "UY",
    "UZ",
    "VU",
    "VE",
    "VN",
    "VG",
    "VI",
    "WF",
    "EH",
    "YE",
    "ZM",
    "ZW"
        };
    };
}
