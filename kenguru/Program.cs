using CSCore.CoreAudioAPI;
using System.Diagnostics;
using System;
using System.Threading;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Timers;
using System.Linq;

namespace kenguru
{
    class Program
    {
        private static string token;
        private static string id;
        private static string url;
        private static void RequestPost(string status, string soobsh, string token, string id, string url)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"token\":\"" + token + "\", \"id\":" + id + ",\"status\": " + status + "}";

                streamWriter.Write(json);
            }
            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Console.WriteLine($"Время: {DateTime.Now}, результат: {result}, {soobsh}");
                }
            }
            catch 
            {
                Console.WriteLine($"Время: {DateTime.Now}, результат: ошибка обращение к CRM");
            }
           
        }

        private static void GetStatus(string token, string id, string url)
        {
            string status;
            string soobsh;
            using (var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render))
            {
                using (var sessionEnumerator = sessionManager.GetSessionEnumerator())
                {
                    float sum = 0;

                    foreach (var session in sessionEnumerator)
                    {
                        using (var audioMeterInformation = session.QueryInterface<AudioMeterInformation>())
                        {
                            var audio = audioMeterInformation.GetPeakValue();
                            sum += audio;

                        }
                    }
                    if (sum == 0)
                    {
                        status = "5";
                        soobsh = "Нет звука";
                    }
                    else
                    {
                        status = "3";
                        soobsh = "Есть звук";
                    }
                }
            }

            RequestPost(status, soobsh, token, id, url);
        }

        private static AudioSessionManager2 GetDefaultAudioSessionManager2(DataFlow dataFlow)
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                using (var device = enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia))
                {
                    Debug.WriteLine("DefaultDevice: " + device.FriendlyName);
                    var sessionManager = AudioSessionManager2.FromMMDevice(device);
                    return sessionManager;
                }
            }
        }

        static async Task Main(string[] args)
        {
            // Чтение файла конфигурации
            string file = "config.ini";
            try
            {
                string[] list = File.ReadAllLines(file);
                token = list[0].Split("=")[1];
                id = list[1].Split("=")[1];
                url = list[2].Split("=")[1];
                int time = Convert.ToInt32(list[3].Split("=")[1]);
                /*foreach(string s in File.ReadAllLines(file))
                {
                    Console.WriteLine(s);
                }*/

                GetStatus(token, id, url);

                // Создаем таймер с интервалом в 1 минуту
                System.Timers.Timer timer = new System.Timers.Timer(time);

                // Добавляем обработчик события Elapsed, который будет вызываться каждую минуту
                timer.Elapsed += OnTimerElapsed;

                // Запускаем таймер
                timer.Start();

                // Чтобы программа не завершилась сразу после запуска таймера, добавим бесконечный цикл
                while (true)
                {
                    continue;
                }
            }
            catch
            {
                Console.WriteLine($"Ошибка чтения файла {file}");
            }
        }

        // Обработчик события Elapsed
        private static void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Здесь можно добавить код, который нужно выполнить каждую минуту
            GetStatus(token, id, url);
        }

    }
}
