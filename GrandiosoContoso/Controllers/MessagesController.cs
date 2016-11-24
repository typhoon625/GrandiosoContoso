using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using GrandiosoContoso.Models;
using System.Collections.Generic;
using static GrandiosoContoso.Models.ForeignExchangeRate;
using GrandiosoContoso.DataModels;

namespace GrandiosoContoso
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                StateClient stateClient = activity.GetStateClient();
                HttpClient httpClient = new HttpClient();
                Activity dataReply;
                BotData usersData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                var message = activity.Text;
                bool stockRequest = true;
                bool currencyRequest = false;
                bool gradeRequest = true;
                string dataResponse = "Hello there " + activity.From.Name + "! Welcome to Grandioso Contoso!\n\n------------------------------\n\n1) Type 1 to access Stock Data!" +
                "\n\n2) Type 2 to access Foreign Exchange Rate!\n\n3) Type 3 to Rate our app!\n\n4) Type 4 to Clear all user data and I will act as if it's our first time :)" +
                "\n\n------------------------------\n\n[Special Note]: If you want more information or want to search up stock symbols/ISO codes, simply press any of the buttons below!";

                //FOREIGN EXCHANGE RATE API
                string getForeignExchangeRate = await httpClient.GetStringAsync(new Uri("http://api.fixer.io/latest?base=USD"));
                ForeignExchangeRate.RootObject exchangeRoot;
                exchangeRoot = JsonConvert.DeserializeObject<ForeignExchangeRate.RootObject>(getForeignExchangeRate);
                Rates rates = exchangeRoot.rates;
                string[] currencies = new string[] {$"AUD: {rates.AUD}", $"BGN: {rates.BGN}", $"BRL: {rates.BRL}", $"CAD: {rates.CAD}",
                $"CHF: {rates.CHF}", $"CNY: {rates.CNY}", $"CZK: {rates.CZK}", $"DKK: {rates.DKK}", $"GBP: {rates.GBP}",
                $"HKD: {rates.HKD}", $"HRK: {rates.HRK}", $"HUF: {rates.HUF}", $"IDR: {rates.IDR}", $"ILS: {rates.ILS}",
                $"INR: {rates.INR}", $"JPY: {rates.JPY}", $"KRW: {rates.KRW}", $"MXN: {rates.MXN}", $"MYR: {rates.MYR}",
                $"NOK: {rates.NOK}", $"NZD: {rates.NZD}", $"PHP: {rates.PHP}", $"PLN: {rates.PLN}", $"RON: {rates.RON}",
                $"RUB: {rates.RUB}", $"SEK: {rates.SEK}", $"SGD: {rates.SGD}", $"THB: {rates.THB}", $"TRY: {rates.TRY}",
                $"ZAR: {rates.ZAR}", $"EUR: {rates.EUR}"};

                foreach (string currency in currencies)
                {
                    if (message.ToUpper().Equals(currency.Substring(0, 3)))
                    {
                        dataResponse = $"This is the foreign exchange rate for the currency that you chose relative to USD.\n\n{currency}\n\n--------------------------\n\n1) Type 1 to access Stock Data!" +
                         "\n\n2) Type 2 to access Foreign Exchange Rate!\n\n3) Type 3 to Rate our app!\n\n4) Type 4 to Clear all user data and I will act as if it's our first time :)";
                        currencyRequest = true;
                    }
                }

                if (message.Equals("1"))
                {
                    dataResponse = "I see. You want stock data, right? \n\nPlease type in the stock symbol if you wish to " +
                    "see the stock prices for the newest available date. If you want to see stock prices from before, type in " +
                    "the date in this format: YYYY-MM-DD \n\nEg) FB 2016-06-18 (to get Facebook stock prices on June 18th 2016)\n\n[Special Note]: Note that if you give us a date where we didn't gather the stock data on that day, " +
                    "we will just show you the stock prices for the company you chose on the newest available date.";
                    stockRequest = false;
                }
                else if (message.Equals("2"))
                {
                    dataResponse = "I see. You wish to know the foreign exchange rate, right? \n\nPlease type in a Currency ISO code " +
                    "to convert the stock prices that are in USD to your own country's currency or (if you are curious) another country's currency.\n\nEg) NZD (to get New Zealand's exchange rate relative to USD)";
                    stockRequest = false;
                }

                else if (message.Equals("3"))
                {
                    dataResponse = "I see. You want to rate our app right? :) Please state either 'A', 'B', 'C', 'D', 'E' or 'F' (A is highest, F is lowest grade!) ";
                    stockRequest = false;
                }

                else if (message.ToUpper().Equals("A"))
                {
                   List<GrandiosoContosoReview> reviews = await AzureManager.AzureInstance.GetReviews();
                   foreach (GrandiosoContosoReview row in reviews)
                    {
                        if (row.SkypeID == activity.From.Id)
                        {
                            row.Rating = "A";
                            await AzureManager.AzureInstance.UpdateReview(row);
                            dataResponse = "You gave us grade A!! Thank you, your valuable opinion matters a lot to us! We will continue to impress you!\n\n--------------------------\n\n1) Type 1 to access Stock Data!" +
                            "\n\n2) Type 2 to access Foreign Exchange Rate!\n\n3) Type 3 to Rate our app!\n\n4) Type 4 to Clear all user data and I will act as if it's our first time :)";
                            gradeRequest = false;
                        }
                    }
                   if (gradeRequest == true)
                    {
                        GrandiosoContosoReview review = new GrandiosoContosoReview()
                        {
                            createdDate = DateTime.Now,
                            SkypeID = activity.From.Id,
                            Rating = "A"
                        };
                        await AzureManager.AzureInstance.AddReview(review);
                        stockRequest = false;
                        dataResponse = "You gave us grade A!! Thank you, your valuable opinion matters a lot to us! We will continue to impress you!\n\n--------------------------\n\n1) Type 1 to access Stock Data!" +
                        "\n\n2) Type 2 to access Foreign Exchange Rate!\n\n3) Type 3 to Rate our app!\n\n4) Type 4 to Clear all user data and I will act as if it's our first time :)";
                    }
                }

                else if (message.ToUpper().Equals("B"))
                {
                    List<GrandiosoContosoReview> reviews = await AzureManager.AzureInstance.GetReviews();
                    foreach (GrandiosoContosoReview row in reviews)
                    {
                        if (row.SkypeID == activity.From.Id)
                        {
                            row.Rating = "B";
                            await AzureManager.AzureInstance.UpdateReview(row);
                            dataResponse = "You gave us grade B!! Thank you, your valuable opinion matters a lot to us! We will make sure your next rating of our app is an 'A'!\n\n--------------------------" +
                            "\n\n1) Type 1 to access Stock Data!\n\n2) Type 2 to access Foreign Exchange Rate!\n\n3) Type 3 to Rate our app!\n\n4) Type 4 to Clear all user data and I will act as if it's our first time :)";
                            gradeRequest = false;
                        }
                    }
                    if (gradeRequest == true)
                    {
                        GrandiosoContosoReview review = new GrandiosoContosoReview()
                        {
                            createdDate = DateTime.Now,
                            SkypeID = activity.From.Id,
                            Rating = "B"
                        };
                        await AzureManager.AzureInstance.AddReview(review);
                        stockRequest = false;
                        dataResponse = "You gave us grade B!! Thank you, your valuable opinion matters a lot to us! We will make sure your next rating of our app is an 'A'!\n\n--------------------------" +
                        "\n\n1) Type 1 to access Stock Data!\n\n2) Type 2 to access Foreign Exchange Rate!\n\n3) Type 3 to Rate our app!\n\n4) Type 4 to Clear all user data and I will act as if it's our first time :)";
                    }
                }

                else if (message.ToUpper().Equals("C"))
                {
                    List<GrandiosoContosoReview> reviews = await AzureManager.AzureInstance.GetReviews();
                    foreach (GrandiosoContosoReview row in reviews)
                    {
                        if (row.SkypeID == activity.From.Id)
                        {
                            row.Rating = "C";
                            await AzureManager.AzureInstance.UpdateReview(row);
                            dataResponse = "You gave us grade C!! Thank you, your valuable opinion matters a lot to us! I guess we have room to improve! ;) \n\n--------------------------" +
                            "\n\n1) Type 1 to access Stock Data!\n\n2) Type 2 to access Foreign Exchange Rate!\n\n3) Type 3 to Rate our app!\n\n4) Type 4 to Clear all user data and I will act as if it's our first time :)";
                            gradeRequest = false;
                        }
                    }
                    if (gradeRequest == true)
                    {
                        GrandiosoContosoReview review = new GrandiosoContosoReview()
                        {
                            createdDate = DateTime.Now,
                            SkypeID = activity.From.Id,
                            Rating = "C"
                        };
                        await AzureManager.AzureInstance.AddReview(review);
                        stockRequest = false;
                        dataResponse = "You gave us grade C!! Thank you, your valuable opinion matters a lot to us! I guess we have room to improve! ;) \n\n--------------------------" +
                        "\n\n1) Type 1 to access Stock Data!\n\n2) Type 2 to access Foreign Exchange Rate!\n\n3) Type 3 to Rate our app!\n\n4) Type 4 to Clear all user data and I will act as if it's our first time :)";
                    }
                }

                else if (message.ToUpper().Equals("D"))
                {
                    List<GrandiosoContosoReview> reviews = await AzureManager.AzureInstance.GetReviews();
                    foreach (GrandiosoContosoReview row in reviews)
                    {
                        if (row.SkypeID == activity.From.Id)
                        {
                            row.Rating = "D";
                            await AzureManager.AzureInstance.UpdateReview(row);
                            dataResponse = "You gave us grade D!! Thank you, your valuable opinion matters a lot to us! I guess we have a lot of room to improve! ;) \n\n--------------------------" +
                            "\n\n1) Type 1 to access Stock Data!\n\n2) Type 2 to access Foreign Exchange Rate!\n\n3) Type 3 to Rate our app!\n\n4) Type 4 to Clear all user data and I will act as if it's our first time :)";
                            gradeRequest = false;
                        }
                    }
                    if (gradeRequest == true)
                    {
                        GrandiosoContosoReview review = new GrandiosoContosoReview()
                        {
                            createdDate = DateTime.Now,
                            SkypeID = activity.From.Id,
                            Rating = "D"
                        };
                        await AzureManager.AzureInstance.AddReview(review);
                        stockRequest = false;
                        dataResponse = "You gave us grade D!! Thank you, your valuable opinion matters a lot to us! I guess we have a lot of room to improve! ;) \n\n--------------------------" +
                        "\n\n1) Type 1 to access Stock Data!\n\n2) Type 2 to access Foreign Exchange Rate!\n\n3) Type 3 to Rate our app!\n\n4) Type 4 to Clear all user data and I will act as if it's our first time :)";
                    }                    
                }

                else if (message.ToUpper().Equals("E"))
                {
                    List<GrandiosoContosoReview> reviews = await AzureManager.AzureInstance.GetReviews();
                    foreach (GrandiosoContosoReview row in reviews)
                    {
                        if (row.SkypeID == activity.From.Id)
                        {
                            row.Rating = "E";
                            await AzureManager.AzureInstance.UpdateReview(row);
                            dataResponse = "You gave us grade E!! Thank you, your valuable opinion matters a lot to us! Can't wait to change features to make this app worthy for you! ;) \n\n--------------------------" +
                            "\n\n1) Type 1 to access Stock Data!\n\n2) Type 2 to access Foreign Exchange Rate!\n\n3) Type 3 to Rate our app!\n\n4) Type 4 to Clear all user data and I will act as if it's our first time :)";
                            gradeRequest = false;
                        }
                    }
                    if (gradeRequest == true)
                    {
                        GrandiosoContosoReview review = new GrandiosoContosoReview()
                        {
                            createdDate = DateTime.Now,
                            SkypeID = activity.From.Id,
                            Rating = "E"
                        };
                        await AzureManager.AzureInstance.AddReview(review);
                        stockRequest = false;
                        dataResponse = "You gave us grade E!! Thank you, your valuable opinion matters a lot to us! Can't wait to change features to make this app worthy for you! ;) \n\n--------------------------" +
                        "\n\n1) Type 1 to access Stock Data!\n\n2) Type 2 to access Foreign Exchange Rate!\n\n3) Type 3 to Rate our app!\n\n4) Type 4 to Clear all user data and I will act as if it's our first time :)";
                    }                  
                }

                else if (message.ToUpper().Equals("F"))
                {
                    List<GrandiosoContosoReview> reviews = await AzureManager.AzureInstance.GetReviews();
                    foreach (GrandiosoContosoReview row in reviews)
                    {
                        if (row.SkypeID == activity.From.Id)
                        {
                            row.Rating = "F";
                            await AzureManager.AzureInstance.UpdateReview(row);
                            dataResponse = "You gave us grade F!! Thank you, your valuable opinion matters a lot to us! But please don't leave us :( We swear we will improve!! \n\n--------------------------" +
                            "\n\n1) Type 1 to access Stock Data!\n\n2) Type 2 to access Foreign Exchange Rate!\n\n3) Type 3 to Rate our app!\n\n4) Type 4 to Clear all user data and I will act as if it's our first time :)";
                            gradeRequest = false;
                        }

                    }
                    if (gradeRequest == true)
                    {
                        GrandiosoContosoReview review = new GrandiosoContosoReview()
                        {
                            createdDate = DateTime.Now,
                            SkypeID = activity.From.Id,
                            Rating = "F"
                        };
                        await AzureManager.AzureInstance.AddReview(review);
                        stockRequest = false;
                        dataResponse = "You gave us grade F!! Thank you, your valuable opinion matters a lot to us! But please don't leave us :( We swear we will improve!! \n\n--------------------------" +
                        "\n\n1) Type 1 to access Stock Data!\n\n2) Type 2 to access Foreign Exchange Rate!\n\n3) Type 3 to Rate our app!\n\n4) Type 4 to Clear all user data and I will act as if it's our first time :)";
                    }    
                }

                else if (message.Equals("4"))
                {
                    List<GrandiosoContosoReview> reviews = await AzureManager.AzureInstance.GetReviews();
                    foreach (GrandiosoContosoReview row in reviews)
                    {
                        if (row.SkypeID == activity.From.Id)
                        {
                            await AzureManager.AzureInstance.DeleteReview(row);
                        }
                    }
                    dataResponse = "User data has been cleared. We'll now act as if it's our first time!\n\n--------------------------\n\n1) Type 1 to access Stock Data!" +
                    "\n\n2) Type 2 to access Foreign Exchange Rate!\n\n3) Type 3 to Rate our app!\n\n4) Type 4 to Clear all user data and I will act as if it's our first time :)";
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                    stockRequest = false;
                }

                else if (message.ToUpper().Equals("GET REVIEWS") && activity.From.Name == "Kevin Lee" && activity.From.Id == "29:1YXJePpE9NgP60P-bYRWznS1RmHQv-mkMvjOWOpL7kpc")
                {
                    List<GrandiosoContosoReview> reviews = await AzureManager.AzureInstance.GetReviews();
                    dataResponse = "";
                    foreach (GrandiosoContosoReview review in reviews)
                    {
                        dataResponse += review.createdDate + ":  " + review.Rating + "\n\n";
                    }
                    stockRequest = false;
                }

                else if (currencyRequest == false && stockRequest == true)
                {
                    //STOCK API
                    try
                    {
                        string[] wordList = message.Split();
                        string getStock = await httpClient.GetStringAsync(new Uri("https://www.quandl.com/api/v3/datasets/WIKI/" + wordList[0] + ".json?api_key=yxxNcS5nuYkXdhzBwBWZ"));
                        StockData.RootObject stockRoot;
                        stockRoot = JsonConvert.DeserializeObject<StockData.RootObject>(getStock);

                        //Various fields for stock data
                        string companyName = stockRoot.dataset.name;
                        int endString = companyName.IndexOf("Prices");
                        companyName = companyName.Substring(0, endString - 1);

                        List<List<object>> stockData = stockRoot.dataset.data;

                        string currentDate = stockRoot.dataset.newest_available_date;
                        string openingPrice = "";
                        string highPrice = "";
                        string lowPrice = "";
                        string closingPrice = "";
                        foreach (List<object> data in stockData)
                        {
                            if (activity.Text.Contains((string)data[0]))
                            {
                                currentDate = (string)data[0];
                                openingPrice = string.Format("{0:0.00}", (double)data[1]);
                                highPrice = string.Format("{0:0.00}", (double)data[2]);
                                lowPrice = string.Format("{0:0.00}", (double)data[3]);
                                closingPrice = string.Format("{0:0.00}", (double)data[4]);
                                break;
                            }
                            else
                            {
                                openingPrice = string.Format("{0:0.00}", (double)stockData[0][1]);
                                highPrice = string.Format("{0:0.00}", (double)stockData[0][2]);
                                lowPrice = string.Format("{0:0.00}", (double)stockData[0][3]);
                                closingPrice = string.Format("{0:0.00}", (double)stockData[0][4]);
                            }
                        }

                        //Reply to user
                        dataResponse = ($"This is the stock data for {companyName} on {currentDate}." + "\n\n------------------------------------" +
                         $"\n\nOpening Price: {openingPrice} USD" + $"\n\nHigh Price: {highPrice} USD" + $"\n\nLow Price: {lowPrice} USD"
                         + $"\n\nClosing Price: {closingPrice} USD\n\n--------------------------\n\n1) Type 1 to access Stock Data!" +
                         "\n\n2) Type 2 to access Foreign Exchange Rate!\n\n3) Type 3 to Rate our app!\n\n4) Type 4 to Clear all user data and I will act as if it's our first time :)");
                    }
                    catch (Exception)
                    {
                        if (usersData.GetProperty<bool>("Greeting"))
                        {
                            if (currencyRequest == false && stockRequest == true)
                            {
                                dataResponse = "Hello again " + activity.From.Name + "! What can we do for you today?\n\n------------------------------\n\n1) Type 1 to access Stock Data!" +
                                "\n\n2) Type 2 to access Foreign Exchange Rate!\n\n3) Type 3 to Rate our app!\n\n4) Type 4 to Clear all user data and I will act as if it's our first time :)" +
                                "\n\n------------------------------\n\n[Special Note]: If you want more information or want to search up stock symbols/ISO codes, simply press the buttons below!";
                            }
                        }
                        else
                        {
                            usersData.SetProperty<bool>("Greeting", true);
                            await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, usersData);
                        }
                    }
                }

                connector.Conversations.ReplyToActivity(activity.CreateReply($"Please wait for a moment..."));
                dataReply = activity.CreateReply(dataResponse);
                dataReply.Recipient = activity.From;
                dataReply.Type = "message";
                dataReply.Attachments = new List<Attachment>();

                List<CardImage> image = new List<CardImage>();
                image.Add(new CardImage("http://i66.tinypic.com/jai6nq.jpg"));

                List<CardAction> buttons = new List<CardAction>();
                CardAction stockButton = new CardAction()
                {
                    Value = "https://finance.yahoo.com/",
                    Type = "openUrl",
                    Title = "Full stock data and financial news"
                };
                buttons.Add(stockButton);

                CardAction exchangeButton = new CardAction()
                {
                    Value = "https://www.travelex.com/rates#",
                    Type = "openUrl",
                    Title = "Full foreign exchange rate table"
                };
                buttons.Add(exchangeButton);

                CardAction stockSymbolsButton = new CardAction()
                {
                    Value = "https://www.nasdaq.com/screening/company-list.aspx",
                    Type = "openUrl",
                    Title = "Stock Symbols"
                };
                buttons.Add(stockSymbolsButton);

                CardAction isoButton = new CardAction()
                {
                    Value = "https://www.forexrealm.com/additional-info/currencies-codes.html",
                    Type = "openUrl",
                    Title = "Currency ISO Codes"
                };
                buttons.Add(isoButton);

                ThumbnailCard companyCard = new ThumbnailCard()
                {
                    Title = "Grandioso Contoso",
                    Subtitle = "Change. That's what we follow.",
                    Images = image,
                    Buttons = buttons
                };

                Attachment companyAttachment = companyCard.ToAttachment();
                dataReply.Attachments.Add(companyAttachment);
                await connector.Conversations.SendToConversationAsync(dataReply);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}