using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HouseFinderConsoleBot.BotClient.Extensions;
using Newtonsoft.Json;
using Nito.AsyncEx;
using PuppeteerSharp;
using Telegram.Bot;

namespace HouseFinderConsoleBot
{
    class Program
    {
        public static ITelegramBotClient BotClient;
        public static string ChatId;
        static void Main(string[] args)
        {
            //Thread.Sleep (30000);
            AsyncContext.Run(CallPuppeteer);
        }

        private static async Task SendNewApartmentsMessage(List<ApartmentInfo> apartments)
        {
            Console.WriteLine("Sending messages...");
            BotClient = new TelegramBotClient("bot_key_here");
            ChatId = "chat_id_here";

            //Just a example
            BotClient.OnMessage += BotClient_OnMessage;
            BotClient.StartReceiving();

            await SendInitialMessage(apartments);

            //Sleep two seconds to prevent other messages 
            //coming before the initial message
            Thread.Sleep(2000);
            await SendApartmentsMessages(apartments);

            BotClient.StopReceiving();
        }

        private static async Task SendApartmentsMessages(List<ApartmentInfo> apartments)
        {
            foreach (var apartment in apartments)   
            {
                Console.WriteLine("Sending apartment message...");
                await BotClient.SendApartmentMessages(ChatId, apartment);
            }
        }

        private static async Task SendInitialMessage(List<ApartmentInfo> apartments)
        {
            await BotClient.SendInitialMessage(ChatId, apartments);
        }

        private static async Task CallPuppeteer()
        {
            Console.WriteLine("Step 1...");
            var apartmentsDataFileName = @"ApartmentsData.txt";
            var oldApartments = new List<ApartmentInfo>();
            if (File.Exists(apartmentsDataFileName))
            {
                string oldApartmentsData = File.ReadAllText(apartmentsDataFileName);
                oldApartments = JsonConvert.DeserializeObject<List<ApartmentInfo>>(oldApartmentsData);
            }

            Console.WriteLine("Step 2...");
            
            List<ApartmentInfo> apartmentsFromPage = await GetAparmentsFromPage();
            System.IO.File.WriteAllText(apartmentsDataFileName, JsonConvert.SerializeObject(apartmentsFromPage));

            List<ApartmentInfo> newApartments = GetNewApartments(oldApartments, apartmentsFromPage);
            await SendNewApartmentsMessage(newApartments);
        }

        private static async Task<List<ApartmentInfo>> GetAparmentsFromPage()
        {
            await DowloadChromium();

            Browser browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false,
                DefaultViewport = new ViewPortOptions()
                {
                    Width = 1368,
                    Height = 768,
                }
            });

            // Create a new page and go to Bing Maps
            Page page = await browser.NewPageAsync();
            await page.GoToAsync("https://www.quintoandar.com.br/");

            await ScrollPageToBottom(page);

            //Wait for cities combobox load then click
            await page.WaitForSelectorAsync(".rsmey3-0");
            await page.ClickAsync(".rsmey3-0");

            //Wait for cities to load then click
            await page.WaitForSelectorAsync(@"[data-value=""belo-horizonte-mg-brasil""]");
            await page.ClickAsync(@"[data-value=""belo-horizonte-mg-brasil""]");

            //Wait apartments to load
            await page.WaitForTimeoutAsync(1000);

            //Slides the apartments to right until the button is disabled
            await SlideApartmentsToRight(page);

            List<ApartmentInfo> apartmentsFromPage = await GetApartmentsFromHtml(page);

            await browser.CloseAsync();
            return apartmentsFromPage;
        }

        private static List<ApartmentInfo> GetNewApartments(List<ApartmentInfo> oldApartments, List<ApartmentInfo> apartments)
        {
            return apartments.Where(w => !oldApartments.Contains(w)).ToList();
        }

        private static async Task<List<ApartmentInfo>> GetApartmentsFromHtml(Page page)
        {
            return await page.EvaluateExpressionAsync<List<ApartmentInfo>>(@"Array.from(getApartmentsInfo()).map(a => a);
                                                 function getApartmentsInfo() {
                                                      let apartments = [];
                                                      let elements = document.getElementsByClassName('eFOTEL')[1].getElementsByClassName('teva8h-2');
                                                      for (let element of elements)
                                                      {
                                                        var linkEl = element.getElementsByClassName('fHkake')[0];
                                                        var ruaEl = element.getElementsByClassName('falbBb')[0];
                                                              var bairroEl = element.getElementsByClassName('Ongdx')[0];
                                                              var areaEl = element.getElementsByClassName('ivMPuZ')[0];
                                                              var aluguelEl = element.getElementsByClassName('dfcRZz')[0];
                                                              var totalEl = element.getElementsByClassName('WCcfX')[0];

                                                              apartments.push({ 
                                                                href: linkEl.href,
                                                                rua: ruaEl.innerText,
                                                                bairro: bairroEl.innerText,
                                                                area: areaEl.innerText,
                                                                aluguel: aluguelEl.innerText,
                                                                total: totalEl.innerText 
                                                              });                                                                   
                                                      };

                                                      return apartments;
                                                 }");
        }

        private static async Task SlideApartmentsToRight(Page page)
        {
            var rightButton = @"document.getElementsByClassName('eFOTEL')[1].querySelector('[right]')";

            while (await page.EvaluateExpressionAsync<bool>(rightButton + ".disabled") != true)
            {
                await page.EvaluateExpressionAsync(rightButton + ".click()");
                await page.WaitForTimeoutAsync(100);
            }
        }

        private static async Task ScrollPageToBottom(Page page)
        {
            await page.EvaluateExpressionAsync(@"new Promise((resolve, reject) => {
                  var totalHeight = 0;
                  var distance = 100;
                  var timer = setInterval(() => {
                    var scrollHeight = document.body.scrollHeight;
                    window.scrollBy(0, distance);
                    totalHeight += distance;

                    if (totalHeight >= scrollHeight) {
                      clearInterval(timer);
                      resolve();
                    }
                  }, 100);
                });");
        }

        //Check if chromium is downloaded and download if not
        private static async Task DowloadChromium()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
        }

        private static async void BotClient_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                Console.WriteLine($"Received a text message in chat {e.Message.Chat.Id}.");

                await BotClient.SendTextMessageAsync(
                    chatId: e.Message.Chat,
                    text: "You said:\n" + e.Message.Text
                );
            }
        }
    }
}