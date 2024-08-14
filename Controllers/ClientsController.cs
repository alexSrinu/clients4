using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Clients.Models;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Web;

namespace Clients.Controllers
{
    public class ClientsController : Controller
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("http://csadms.com/Devbox/DevboxAPI/")
        };
        private IEnumerable<object> homePageCounter;

        public object CommonHeader { get; private set; }
        public string Submit { get; private set; }
        public object ImagePath { get; private set; }
        public string Base64FileData { get; private set; }

        [HttpGet]
        public async Task<ActionResult> HomePageCounters()
        {
            try
            {
                
                HttpResponseMessage responseMessage = await client.PostAsJsonAsync("api/HomePageCounterAPI/NewHomePageCounterList", new object());

                if (responseMessage.IsSuccessStatusCode)
                {
                   
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(responseData);

                  //  TempData["HomePageCounters"] = apiResponse;
                    var homeClientsList = apiResponse?.Data ?? new List<HomeClients>();

                   
                    return View(homeClientsList);
                }
                else
                {
                   
                    ViewBag.ErrorMessage = $"API call failed with status code {responseMessage.StatusCode}";
                    return View("Error");
                }
            }
            catch (Exception ex)
            {
              
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View("Error"); 
            }
        }
        [HttpGet]
        public async Task<ActionResult> Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Class model, HttpPostedFileBase file)
        {
            // if (ModelState.IsValid)
            // {
            if (!ModelState.IsValid)
            {

                return View(model);
            }
            string baseAddress = "http://csadms.com/Devbox/DevboxAPI/";
            //   HomeClients obj = new HomeClients();
            // Class obj=new Class();
            // obj.FlagId = 1;
            //obj.Id = 0;
            Class obj = new Class
            {
                Id = model.Id,
                FlagId = 1,
                ImagePath = file.FileName,
                Value = model.Value,
                TextEn = model.TextEn,
            };

            using (HttpClient client = new HttpClient { BaseAddress = new Uri(baseAddress) })
            {

                if (file != null && file.ContentLength > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.InputStream.CopyToAsync(memoryStream);
                        byte[] fileBytes = memoryStream.ToArray();
                        string base64FileData = Convert.ToBase64String(fileBytes);

                        model.ImagePath = file.FileName;
                        model.divImagePath = Base64FileData;
                    }
                }
                else
                {
                    model.ImagePath = string.Empty;
                    model.divImagePath = string.Empty;
                }

            }

                HttpResponseMessage responseMessage = await client.PostAsJsonAsync("api/HomePageCounterAPI/NewAddHomePageCounter", obj);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var response = JObject.Parse(responseData);
                    bool isStatus = Convert.ToBoolean(response.SelectToken("Status"));
                    string message = response.SelectToken("Message").ToString();

                    TempData["Success"] = message;

                    return RedirectToAction("HomePageCounters");
                }
                else
                {

                    ModelState.AddModelError("", "An error occurred while creating the item.");
                }
            
            //}



            return View(model);
        }
    

  
        [HttpGet]


        public async Task<ActionResult> EditHomePageCounters(int id)
        {
            string baseAddress = "http://csadms.com/Devbox/DevboxAPI/";

            using (   HttpClient client = new HttpClient { BaseAddress = new Uri(baseAddress) })
            {
                  HomeClients obj = new HomeClients { Id = id };
              //  Class obj = new Class { Id = id };

                HttpResponseMessage responseMessage = await client.PostAsJsonAsync("api/HomePageCounterAPI/NewHomePageCounterList", obj);

                if (!responseMessage.IsSuccessStatusCode)
                {
                    return new HttpStatusCodeResult((int)responseMessage.StatusCode);
                }

                var responseData = await responseMessage.Content.ReadAsStringAsync();
                var responseJson = JObject.Parse(responseData);
                var isData = responseJson.SelectToken("Data").ToString();

                var homePageCounterList = JsonConvert.DeserializeObject<List<HomeClients>>(isData);

                var homePageCounter = homePageCounterList.FirstOrDefault();
                if (homePageCounter == null)
                {
                    return HttpNotFound("Home page counter not found.");
                }

             
                TempData["EditHomePageCounters"] = homePageCounter;

                return View(homePageCounter);
            }
        }



        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditHomePageCounters(HomeClients model, HttpPostedFileBase file)
        {
            string baseAddress = "http://csadms.com/Devbox/DevboxAPI/";

          
            var originalItem = TempData["EditHomePageCounters"] as HomeClients;
            if (originalItem == null)
            {
                ModelState.AddModelError("", "Original data not found.");
                return View(model);
            }

           
            HomeClients obj = new HomeClients
            {
                Id = model.Id,
                FlagId = 2,
                Value = model.Value ?? originalItem.Value,
                TextEn = model.TextEn ?? originalItem.TextEn,
                ImagePath = model.ImagePath ?? originalItem.ImagePath 
            };

          
            if (file != null && file.ContentLength > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.InputStream.CopyToAsync(memoryStream);
                    byte[] fileBytes = memoryStream.ToArray();
                    string base64FileData = Convert.ToBase64String(fileBytes);

                  
                    obj.ImagePath = file.FileName; 
                    obj.divImagePath = base64FileData; 
                }
            }

          
            using (HttpClient client = new HttpClient { BaseAddress = new Uri(baseAddress) })
            {
                HttpResponseMessage responseMessage = await client.PostAsJsonAsync("api/HomePageCounterAPI/NewAddHomePageCounter", obj);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var responseJson = JObject.Parse(responseData);

                    bool isStatus = Convert.ToBoolean(responseJson.SelectToken("Status"));
                    if (isStatus)
                    {
                        TempData["Success"] = "Home Page Counters updated successfully.";
                        return RedirectToAction("HomePageCounters");
                    }
                    else
                    {
                        ModelState.AddModelError("", responseJson.SelectToken("Message")?.ToString());
                    }
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred while updating the data.");
                }
            }

            return View(model);
        }

        /*  public async Task<ActionResult> EditHomePageCounters(Class model,HttpPostedFileBase file)
          {
              string baseAddress = "http://csadms.com/Devbox/DevboxAPI/";
              //if (!ModelState.IsValid)
              //{

              //    return View(model);
              //}

              var originalItem = TempData["EditHomePageCounters"] as HomeClients;
              if (originalItem == null)
              {
                  ModelState.AddModelError("", "Original data not found.");
                  return View(model);
              }
            /*  Class obj = new Class
              {
                  Id = model.Id,
                  FlagId = 1,
                  ImagePath = model.ImagePath,
                  Value = model.Value,
                  TextEn = model.TextEn,
              };


        Class obj = new Class
               {
                   Id = model.Id, 
                   FlagId = 2, 
                   ImagePath = model.ImagePath ?? originalItem.ImagePath,
                   Value = model.Value ?? originalItem.Value,
                   TextEn = model.TextEn ?? originalItem.TextEn
               };


            using (HttpClient client = new HttpClient { BaseAddress = new Uri(baseAddress) })
            {
                HttpResponseMessage responseMessage = await client.PostAsJsonAsync("api/HomePageCounterAPI/NewAddHomePageCounter", obj);
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseData = await responseMessage.Content.ReadAsStringAsync();
                    var responseJson = JObject.Parse(responseData);

                    bool isStatus = Convert.ToBoolean(responseJson.SelectToken("Status"));
                    if (isStatus)
                    {
                        TempData["Success"] = "Home Page Counters updated successfully.";
                        return RedirectToAction("HomePageCounters");
                    }
                    else
                    {
                        ModelState.AddModelError("", responseJson.SelectToken("Message")?.ToString());
                    }
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred while updating the data.");
                }
            }

            return View(model);
        }*/
       




          public async Task<ActionResult> DeleteHomePageCounters(int id)
          {
              string ses = Convert.ToString(Session["AdminUserId"]);
              HomeClients obj = new HomeClients();
            
              obj.FlagId = 3;
              obj.Id = id;
              string baseAddress = "http://csadms.com/Devbox/DevboxAPI/";
              using (HttpClient client = new HttpClient { BaseAddress = new Uri(baseAddress) })
              {
                
                HttpResponseMessage responseMessage = await client.PostAsJsonAsync("api/HomePageCounterAPI/NewAddHomePageCounter", obj);
                if (responseMessage.IsSuccessStatusCode)
                  {
                      var responseData = responseMessage.Content.ReadAsStringAsync().Result;
                      var Response = JObject.Parse(responseData);
                      bool isStatus = Convert.ToBoolean(Response.SelectToken("Status"));
                      string Message = Response.SelectToken("Message").ToString();


                  }

                  return RedirectToAction("HomePageClients");
              }
          }
        private byte[] StreamToBytes(Stream strm)
        {
            throw new NotImplementedException();
        }

        public class ApiResponse
        {
            public List<HomeClients> Data { get; set; }
            public bool Status { get; internal set; }
        }
    }
}
