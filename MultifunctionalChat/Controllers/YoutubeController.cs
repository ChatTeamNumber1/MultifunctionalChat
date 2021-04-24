using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MultifunctionalChat.Controllers
{
    public class YoutubeController : Controller
    {
        //TODO Хранить в настройках
        private const string myKey = "AIzaSyDfd7fLx9lePxtrtkQE2A0NZdoqRLdV_SA";
        
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
                dynamic channelData = dataFromYoutube.items[0];
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

        public static Dictionary<string, string> GetRandomComment(string videoId)
        {
            Dictionary<string, string> commentInfo = new Dictionary<string, string>();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                "https://youtube.googleapis.com/youtube/v3/commentThreads?" +
                "part=snippet,id&maxResults=25&videoId=" + videoId + "&key=" + myKey);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string sReadData = sr.ReadToEnd();
            response.Close();

            //Извините за названия, но это что-то очень абстрактное
            try
            {
                dynamic dataFromYoutube = JsonConvert.DeserializeObject(sReadData);
                int n = dataFromYoutube.items.Count;
                if (n > 25)
                    n = 25;

                Random rnd = new Random();
                int commentNumber = rnd.Next() % n;
                dynamic commentData = dataFromYoutube.items[commentNumber];
                commentInfo.Add("user", commentData.snippet.topLevelComment.snippet.authorDisplayName.ToString());
                commentInfo.Add("text", commentData.snippet.topLevelComment.snippet.textOriginal.ToString());
            }
            catch (Exception) { }

            return commentInfo;
        }

        public static string GetVideoIdByNameAndChannel(string channelId, string videoName)
        {
            string videoId = "";
            videoName = videoName.Replace("#", "");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                "https://youtube.googleapis.com/youtube/v3/search?" +
                "q=" + videoName + "&part=id&order=date&maxResults=1&channelId=" + channelId + "&key=" + myKey);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string sReadData = sr.ReadToEnd();
            response.Close();

            try
            {
                dynamic dataFromYoutube = JsonConvert.DeserializeObject(sReadData);
                dynamic videoData = dataFromYoutube.items[0];
                videoId = videoData.id.videoId;
            }
            catch (Exception) { }

            return videoId;
        }

        public static Dictionary<string, string> GetVideoInfo(string videoId)
        {
            Dictionary<string, string> videoInfo = new Dictionary<string, string>
            {
                { "address", "https://www.youtube.com/watch?v=" + videoId }
            };

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                "https://youtube.googleapis.com/youtube/v3/videos?" +
                "part=statistics&id=" + videoId + "&key=" + myKey);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string sReadData = sr.ReadToEnd();
            response.Close();

            //Извините за названия, но это что-то очень абстрактное
            try
            {
                dynamic dataFromYoutube = JsonConvert.DeserializeObject(sReadData);
                dynamic videoData = dataFromYoutube.items[0];
                videoInfo.Add("likes", videoData.statistics.likeCount.ToString());
                videoInfo.Add("views", videoData.statistics.viewCount.ToString());
            }
            catch (Exception ) { }

            return videoInfo;
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

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
                "https://youtube.googleapis.com/youtube/v3/search?" +
                "part=snippet,id&order=date&maxResults=5&channelId=" + channel + "&key=" + myKey);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            StreamReader sr = new StreamReader(stream);
            string sReadData = sr.ReadToEnd();
            response.Close();

            try
            {
                dynamic dataFromYoutube = JsonConvert.DeserializeObject(sReadData);
                foreach (dynamic videoData in dataFromYoutube.items)
                    videos.Add("https://www.youtube.com/watch?v=" + videoData.id.videoId);
            }
            catch (Exception) { }

            return videos;
        }        
    }
}
