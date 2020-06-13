using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GetImage.Models;
using System.Globalization;
using System.IO.Pipes;

namespace GetImage.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private GetimagesData getimagesData;
        // public HomeController(ILogger<HomeController> logger, GetimagesData Imagesdata)
        public HomeController(GetimagesData Imagesdata)

        {

            getimagesData = Imagesdata;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public FileContentResult Getimage()
        {
            try
            {

                if (getimagesData.ImageNextRefresh < DateTime.Now)
                {
                    if (string.IsNullOrEmpty(getimagesData.ImageKeyNext))
                    {
                        var pics = getimagesData.ImageList.Where(w => !w.Value);
                        var picid = new Random().Next(0, pics.Count());
                        getimagesData.ImageKeyCurrent = pics.ElementAt(picid).Key;
                        getimagesData.ImageKeyNext = getimagesData.ImageKeyCurrent;
                        //check if first file is nef
                        getimagesData.CreateJpeg(getimagesData.ImageKeyCurrent).GetAwaiter().GetResult() ;
                    }
                    else
                    {
                        getimagesData.ImageKeyCurrent = getimagesData.ImageKeyNext;
                    }

                    getimagesData.SetShown(getimagesData.ImageKeyCurrent);
                    var nextTimesetup = DateTime.Now.AddSeconds(-1 * DateTime.Now.Second);
                    getimagesData.ImageNextRefresh = nextTimesetup.AddMinutes(getimagesData.ImageRefreshInterval);

                }
                byte[] imgg = null;

                var segmantes = getimagesData.ImageKeyCurrent.Split('\\');
                var fname = segmantes[segmantes.Length - 1];
                fname = getimagesData.ContentRootPath + "\\wwwroot\\Temp\\" + fname.Replace("nef", "jpg", StringComparison.CurrentCultureIgnoreCase);
                imgg = System.IO.File.ReadAllBytes(fname);
                return File(imgg, "images/jpg");
            }
            catch (Exception ex)
            {

                return null;
            }

        }
        public IActionResult RefreshImage()
        {
            getimagesData.ImageList.Clear();
            getimagesData.GetImagesList();
            return Json("true");
        }
        public IActionResult RefreshImageAddFolder(string id)
        {
            getimagesData.GetImagesList(id);
            return Json("true");
        }
        public string GetImageFolder()
        {
            return getimagesData.ImageKeyCurrentFolder;
        }
    }

}
