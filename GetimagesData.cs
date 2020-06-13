using ImageMagick;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace GetImage
{
    public class GetimagesData
    {
        private Dictionary<string, bool> imageList;
        public DateTime SetupTimeStamp { get; set; }
        public string ApplicationName { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; }
        public string WebRootPath { get; set; }



        public string imageRootPath = string.Empty;

        public int ImageRefreshInterval { get; set; }

        public DateTime ImageNextRefresh { get; set; }

        public string ImageKeyLast { get; set; }
        public string ImageKeyCurrent { get; set; }
        public string ImageKeyCurrentFolder
        {
            get
            {
                var imagepath = ImageKeyCurrent.Split('\\');
                if (imagepath.Length > 2)
                {

                    return imagepath[imagepath.Length - 2];
                }
                else
                {
                    return "Root";
                }
            }
        }
        public string ImageKeyNext { get; set; }
        public Dictionary<string, bool> ImageList
        {
            get
            {
                if (imageList == null)
                {
                    imageList = new Dictionary<string, bool>();
                }
                return imageList;

            }
        }
        public void GetImagesList(string dirpath = null)
        {
            dirpath = dirpath ?? imageRootPath;
            foreach (var directory in System.IO.Directory.GetDirectories(dirpath).AsEnumerable())
            {
                GetImagesList(directory);
            }
            var files = System.IO.Directory.GetFiles(dirpath);
            foreach (var file in files)
            {
                ImageList.Add(file, false);
            }

        }
        public async void SetShown(string key)
        {
            ImageList.Remove(key);
            if (imageList.Count < 1)
            {
                GetImagesList();
            }
            var picid = new Random().Next(0, ImageList.Count());
            ImageKeyNext = ImageList.ElementAt(picid).Key;
            CreateJpeg(ImageKeyNext);

        }
        public async Task<bool> CreateJpeg(string sourceFile)
        {
            //D:\Apps\ImageMagick\magick.exe

            try
            {

                var dpath = ContentRootPath + "\\wwwroot\\Temp\\";
                if (!Directory.Exists(dpath))
                {
                    Directory.CreateDirectory(dpath);
                }
                foreach (string file in Directory.GetFiles(dpath))
                {
                    var cfile = ImageKeyCurrent.ToLower().Replace("nef", "jpg").Split('\\');
                    var nfile = ImageKeyNext.ToLower().Replace("nef", "jpg").Split('\\');

                    if (!file.ToLower().EndsWith(cfile[cfile.Length - 1]) && !file.ToLower().EndsWith(nfile[nfile.Length - 1]))
                    {
                        File.Delete(file);
                    }
                    //FileInfo fi = new FileInfo(file);
                    //if (fi.LastAccessTime < DateTime.Now.AddMinutes(-10))
                    //    fi.Delete();
                }
                var segmantes = sourceFile.Split('\\');
                var fname = segmantes[segmantes.Length - 1];


                fname = dpath + fname.Replace("nef", "jpg", StringComparison.CurrentCultureIgnoreCase);
                if (sourceFile.EndsWith("nef", StringComparison.CurrentCultureIgnoreCase))
                {
                    Savenef(sourceFile, fname);
                }
                else
                {
                    File.Copy(sourceFile, fname, true);
                }
                ResizeImage(fname);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public void Savenef(string sourcefile, string targetfile)
        {

            using (var image = new MagickImage(sourcefile))
            {

                image.Write(targetfile);

            }
        }
        public void ResizeImage(string targetfile)
        {
            var settings = new MagickReadSettings();
            using (var image = new MagickImage(targetfile))
            {
                // Save frame as jpg
                if (image.Height > image.Width)
                {
                    var sizeratio = image.Width / 1080;
                    settings.Height = 1080;
                    settings.Width = image.Width / sizeratio;
                }
                else
                {
                    var sizeratio = image.Width / 1920;
                    settings.Width = 1920;
                    settings.Height = image.Height / sizeratio;
                }


                image.Resize((int)settings.Width, (int)settings.Height);
                switch (image.Orientation)
                {
                    case OrientationType.TopLeft:
                        break;
                    case OrientationType.TopRight:
                        image.Flop();
                        break;
                    case OrientationType.BottomRight:
                        image.Rotate(180);
                        break;
                    case OrientationType.BottomLeft:
                        image.Flop();
                        image.Rotate(180);
                        break;
                    case OrientationType.LeftTop:
                        image.Flop();
                        image.Rotate(-90);
                        break;
                    case OrientationType.RightTop:
                        image.Rotate(90);
                        break;
                    case OrientationType.RightBottom:
                        image.Flop();
                        image.Rotate(90);
                        break;
                    case OrientationType.LeftBotom:
                        image.Rotate(-90);
                        break;
                    default: // Invalid orientation
                        break;
                }
                image.Orientation = OrientationType.TopLeft;
                image.Write(targetfile);
            }
        }
    }
}
