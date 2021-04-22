using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

//using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
//using Google.Apis.YouTube.v3;
//using Google.Apis.YouTube.v3.Data;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MultifunctionalChat.Controllers
{
    public class YoutubeController : Controller
    {
        //TODO Хранить в настройках
        private static string myKey = "AIzaSyDfd7fLx9lePxtrtkQE2A0NZdoqRLdV_SA";
        
        public static string GetChannelIdByName(string channel)
        {
            string result = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                "https://youtube.googleapis.com/youtube/v3/channels?" +
                "part=id&forUsername=" + channel + "&key=" + myKey);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string sReadData = sr.ReadToEnd();
            response.Close();
                        
            try
            {
                dynamic dataFromYoutube = JsonConvert.DeserializeObject(sReadData);
                foreach (dynamic channelData in dataFromYoutube.items)
                    result = channelData.id;
            }
            catch (Exception) { }

            //Если не получилось в лоб, пробуем поиском
            if (result == "")
            {
                request = (HttpWebRequest)WebRequest.Create(
                    "https://youtube.googleapis.com/youtube/v3/search?" + 
                    "part=id&q=" + channel + "&key=" + myKey);

                response = (HttpWebResponse)request.GetResponse();
                stream = response.GetResponseStream();
                sr = new StreamReader(stream);
                sReadData = sr.ReadToEnd();
                response.Close();

                try
                {
                    dynamic dataFromYoutube = JsonConvert.DeserializeObject(sReadData);
                    foreach (dynamic channelData in dataFromYoutube.items)
                    {
                        if (channelData.id.kind == "youtube#channel")
                        {
                            result = channelData.id.channelId;
                            break;
                        }
                    }
                }
                catch (Exception) { }
            }
            return result;
        }

        public static string GetChannelNameById(string id)
        {
            string result = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                "https://youtube.googleapis.com/youtube/v3/channels?" +
                "part=brandingSettings&id=" + id + "&key=" + myKey);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string sReadData = sr.ReadToEnd();
            response.Close();

            try
            {
                dynamic dataFromYoutube = JsonConvert.DeserializeObject(sReadData);
                foreach (dynamic channelData in dataFromYoutube.items)
                    result = channelData.brandingSettings.channel.title;
            }
            catch (Exception) { }

            return result;
        }

        public static List<string> GetVideosByChannel(string channel)
        {
            List<string> videos = new List<string>();

            string myKey = "AIzaSyDfd7fLx9lePxtrtkQE2A0NZdoqRLdV_SA";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                "https://youtube.googleapis.com/youtube/v3/search?" +
                "part=snippet,id&order=date&maxResults=5&channelId=" + channel + "&key=" + myKey);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader sr = new StreamReader(stream);

            string sReadData = sr.ReadToEnd();
            response.Close();

            //Извините за названия, но это что-то очень абстрактное
            try
            {
                dynamic dataFromYoutube = JsonConvert.DeserializeObject(sReadData);
                foreach (dynamic videoData in dataFromYoutube.items)
                {
                    videos.Add("https://www.youtube.com/watch?v=" + videoData.id.videoId);
                }
            }
            catch (Exception) { }

            return videos;
        }


        // GET: YoutubeController
        public ActionResult Index()
        {
            return View();
        }

        // GET: YoutubeController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: YoutubeController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: YoutubeController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: YoutubeController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: YoutubeController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: YoutubeController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: YoutubeController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
