using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TinnyMagicMiror.Controllers;

namespace GetImage
{
    public class GetMirrorData
    {
        #region "application vars"
        public DateTime SetupTimeStamp { get; set; }
        public string ApplicationName { get; set; }
        public string ContentRootPath { get; set; }
        public string EnvironmentName { get; set; }
        public string WebRootPath { get; set; }

        #endregion

        #region "Imagelist"

        public string imageRootPath = string.Empty;

        private Dictionary<string, bool> imageList;
        public async Task<bool> GetImagesList(string dirpath = null)
        {
            dirpath ??= imageRootPath;
            foreach (var directory in Directory.GetDirectories(dirpath).AsEnumerable())
            {
                GetImagesList(directory).GetAwaiter().GetResult();
            }
            var files = Directory.GetFiles(dirpath).Where(w =>
            w.ToLower().EndsWith("jpeg")
            || w.ToLower().EndsWith("jpg")
            || w.ToLower().EndsWith("nef"));
            foreach (var file in files)
            {
                ImageList.Add(file, false);
            }
            return true;
        }
        public void RefreshAll()
        {
            imageList.Clear();
            GetImagesList(null);
        }
        public void RefreshAddFolder(string folder)
        {
            GetImagesList(folder);
        }
        #endregion

        #region "tempfolder"
        public string TempFolder
        {
            get
            {
                var dpath = ContentRootPath + "\\wwwroot\\Temp\\";
                if (!Directory.Exists(dpath))
                {
                    Directory.CreateDirectory(dpath);
                }
                return dpath;
            }
        }
        public async void CleanTempFolder()
        {
            foreach (string file in Directory.GetFiles(TempFolder))
            {
                try
                {
                    var cfile = (ImageKeyCurrent ?? "").ToLower().Replace("nef", "jpg").Split('\\');
                    var nfile = (ImageKeyNext ?? "").ToLower().Replace("nef", "jpg").Split('\\');

                    if (!file.ToLower().EndsWith(cfile[cfile.Length - 1]) && !file.ToLower().EndsWith(nfile[nfile.Length - 1]))
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception) { }
            }
        }
        #endregion
        public string GetRandomImage()
        {
            try
            {
                if (ImageNextRefresh < DateTime.Now)
                {
                    if (string.IsNullOrEmpty(ImageKeyNext))
                    {
                        ImageKeyNext = GetRandomImageNext().GetAwaiter().GetResult();
                    }
                    ImageKeyCurrent = ImageKeyNext;
                    ImageDateTakenCurrent = ImageDateTakenNext;
                    var nextTimesetup = DateTime.Now.AddSeconds(-1 * DateTime.Now.Second);
                    ImageNextRefresh = nextTimesetup.AddMinutes(ImageRefreshInterval);
                    GetRandomImageNext();
                }
            }
            catch (Exception)
            {

                return null;
            }
            return ImageKeyCurrent;
        }

        public async Task<string> GetRandomImageNext()
        {
            try
            {
                if (!string.IsNullOrEmpty(ImageKeyCurrent))
                {
                    imageList.Remove(ImageKeyCurrent);
                    if (imageList.Count() < 1)
                    {
                        GetImagesList();
                    }
                }
                var picIndex = new Random().Next(0, imageList.Count());
                ImageKeyNext = imageList.ElementAt(picIndex).Key;
                var fname = SaveLocalJpeg(ImageKeyNext);
                ResizeImage(fname);

                return ImageKeyNext;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public string SaveLocalJpeg(string sourceFile)
        {
            CleanTempFolder();
            var segmantes = sourceFile.Split('\\');
            var fname = segmantes[segmantes.Length - 1];
            fname = TempFolder + fname.Replace("nef", "jpg", StringComparison.CurrentCultureIgnoreCase);
            if (sourceFile.EndsWith("nef", StringComparison.CurrentCultureIgnoreCase))
            {
                using var image = new MagickImage(sourceFile);
                image.Write(fname);
            }
            else
            {
                File.Copy(sourceFile, fname, true);
            }

            return fname;
        }
        List<string> KnownFolderNames = new List<string>()
            {
               "raw",
               "folder",
               "jpg",
               "jpeg" ,
               "100ncd90",
               "101nd700",
               "d90",
               "d700"

            };
        public int ImageRefreshInterval { get; set; }
        public DateTime ImageNextRefresh { get; set; }
        public string ImageKeyCurrent { get; set; }
        public string ImageDateTakenCurrent { get; set; }
        public string ImageKeyNext { get; set; }
        public string ImageDateTakenNext { get; set; }
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
                try
                {
                    ImageDateTakenNext = image.GetAttribute("exif: DateTimeDigitized");
                    if (string.IsNullOrEmpty(ImageDateTakenNext))
                    {

                        ImageDateTakenNext = GetImageDatefromFolderName();
                    }
                }
                catch (Exception) { }
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
        public string GetImageDatefromFolderName()
        {
            var imagepath = ImageKeyNext.Split('\\');
            var valuetoreturn = imagepath[imagepath.Length - 2];
            if (KnownFolderNames.Contains(valuetoreturn.ToLower()))
            {
                valuetoreturn = imagepath[imagepath.Length - 3];
            }
            return valuetoreturn;
        }

        private List<Feeds> _MirrorNewsFeed;
        public List<Feeds> MirrorNewsFeed {
            get
            {
                if (_MirrorNewsFeed == null)
                {
                    _MirrorNewsFeed = new List<Feeds>();
                }
                return _MirrorNewsFeed;
            } 
        }
        public int CurrentIndexNews { get; set; }
    }
}
