using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MultifunctionalChat
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //info
            string channelId = 
                Controllers.YoutubeController.GetChannelIdByName("���������� ��������� ����������������");
            if (channelId != "")
            {
                string name = Controllers.YoutubeController.GetChannelNameById(channelId);
                var Videos = Controllers.YoutubeController.GetVideosByChannel(channelId);
            }
            else 
            {
                //����� �� ������
            }

            //find
            string videoId = Controllers.YoutubeController.GetVideoIdByNameAndChannel(channelId, "������ � API");
            Dictionary<string, string> videoInfo = Controllers.YoutubeController.GetVideoInfo(videoId);

            //videoCommentRandom
            Dictionary<string, string> randomComment = Controllers.YoutubeController.GetRandomComment(videoId);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseWebRoot("wwwroot");
                });
    }
}
