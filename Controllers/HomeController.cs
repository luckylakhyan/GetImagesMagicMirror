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
        private GetMirrorData getimagesData;
        // public HomeController(ILogger<HomeController> logger, GetimagesData Imagesdata)

        public HomeController(GetMirrorData Imagesdata)
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
        public IActionResult RefreshImage()
        {
            getimagesData.ImageList.Clear();
            getimagesData.RefreshAll();
            return Json("true");
        }
        public IActionResult RefreshImageAddFolder(string id)
        {
            getimagesData.GetImagesList(id);
            return Json("true");
        }

        public ActionResult GetImageUrl()
        {
            var segmantes = getimagesData.GetRandomImage().Split('\\');
            byte[] imgg = null;
            var fname = segmantes[segmantes.Length - 1];
            fname = getimagesData.ContentRootPath + "\\wwwroot\\Temp\\" + fname.Replace("nef", "jpg", StringComparison.CurrentCultureIgnoreCase);
            imgg = System.IO.File.ReadAllBytes(fname);

            var base64 = Convert.ToBase64String(imgg);
            var imgSrc = String.Format("data:image/gif;base64,{0}", base64);

            return Json(new { result = imgSrc, Datetaken = getimagesData.ImageDateTakenCurrent });
        }
    }

}
