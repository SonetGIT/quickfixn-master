using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using QuickFix;
using QuickFix.Fields;

namespace SimpleAcceptor
{
    /// <summary>
    /// Just a simple server that will let you connect to it and ignore any
    /// application-level messages you send to it.
    /// Note that this app is *NOT* a message cracker.
    /// </summary>

    public class SimpleAcceptorApp : /*QuickFix.MessageCracker,*/ QuickFix.IApplication
    {
        #region QuickFix.Application Methods

        //передача запроса на биржу
        public void FromApp(Message message, SessionID sessionID)
        {
            Console.WriteLine("  ");
            Console.WriteLine("IN (передача запроса на биржу):  " + message);
            var sections = message.ToString().Split(Message.SOH);
            Console.WriteLine("  ");
            Console.WriteLine("TAG = Значения: РАСШИФРОВКА СООБЩЕНИЯ");
            bool isNewOrder = false;
            string instrumentCode = "";
            int orgId = 0;
            var bidDirection = 0;//0-BUY, 1-SELL
            var bidType = 1;//0-MARKET, 1-LIMITED
            double price = 0;
            double amount = 0;
            var userId = "";
            bool tag21 = false;
            try
            {
                foreach (var tagObj in sections)
                {
                    if (tagObj.Split('=').Length < 2) continue;
                    var tagId = tagObj.Split('=')[0];
                    var tagVal = tagObj.Split('=')[1];

                    if (tagId == 8.ToString())
                    {
                        tagId = "(8) Начало сообщения, версия протокола";
                        tagVal = "FIX-4.4";
                    }
                    else if (tagId == 9.ToString())
                    {
                        tagId = "(9) Размер сообщения (байт)"; //BodyLength
                    }
                    else if (tagId == 21.ToString())
                    {
                        tagId = "(21) Обработка заявки";
                        if (tagVal == "1")
                        {
                            tagVal = "Обработка заявки автоматически";
                            tag21 = true;
                        }
                    }
                    else if (tagId == 35.ToString())
                    {
                        tagId = "(35) Тип сообщения"; //MsgType
                        if (tagVal == "D")
                        {
                            tagVal = "Новая заявка";
                            isNewOrder = true;
                        }
                        else if (tagVal == "F")
                        {
                            tagVal = "Отменить заказ";
                        }
                        else if (tagVal == "G")
                        {
                            tagVal = "Заменить заказ";
                        }
                    }
                    else if (tagId == 34.ToString())
                    {
                        tagId = "(34) Номер сообщения"; //MsgSeqNum
                    }
                    else if (tagId == 49.ToString())
                    {
                        tagId = "(49) От брокера (user)"; //TargetCompID
                        userId = tagVal;
                    }
                    else if (tagId == 56.ToString())
                    {
                        tagId = "(56) Отправитель (фирма)"; //SenderCompID
                        if (!int.TryParse(tagVal, out orgId))
                            orgId = 2;//int.Parse(tagVal);
                    }
                    else if (tagId == 52.ToString())
                    {
                        tagId = "(52) Время отправки"; //SendingTime
                    }
                    else if (tagId == 11.ToString())
                    {
                        tagId = "(11) Номер заявки в торговой системе брокера";
                    }
                    else if (tagId == 54.ToString())
                    {
                        tagId = "(54) Направления заявки";
                        if (tagVal == "1")
                        {
                            tagVal = "На пакупку";
                            bidDirection = 0;
                        }
                        else if (tagVal == "2")
                        {
                            tagVal = "На продажу";
                            bidDirection = 1;
                        }
                    }
                    else if (tagId == 167.ToString())
                    {
                        tagId = "(167) Фьючерсов";
                    }
                    else if (tagId == 262.ToString())
                    {
                        tagId = "MDReqID"; //MARKETDATAID
                    }
                    else if (tagId == 263.ToString())
                    {
                        tagId = "(263) Тип запроса подписки"; //SubscriptionRequestType
                    }
                    else if (tagId == 264.ToString())
                    {
                        tagId = "(264) Глубина рынка"; //MarketDepth
                    }
                    else if (tagId == 267.ToString())
                    {
                        tagId = "(267) Тип ввода MD отсутствует"; //NoMDEntryTypes
                    }
                    else if (tagId == 269.ToString())
                    {
                        tagId = "(269) Тип входа MD"; //MDEntryType
                    }
                    else if (tagId == 146.ToString())
                    {
                        tagId = "(146) Нет связанных символов"; //NoRelatedSym
                    }
                    else if (tagId == 55.ToString())
                    {
                        tagId = "(55) На акции компании"; //Symbol
                        instrumentCode = tagVal;
                    }
                    else if (tagId == 38.ToString())
                    {
                        tagId = "(38) В объеме лотов (кол-во)";
                        if (!double.TryParse(tagVal, out amount))
                            amount = -1;
                    }
                    else if (tagId == 40.ToString())
                    {
                        tagId = "(40) Тип заявки";
                        if (tagVal == "1")
                        {
                            tagVal = "Рыночная заявка";
                            bidType = 0;
                        }
                        else if (tagVal == "2")
                        {
                            tagVal = "Лимитированная заявка";
                            bidType = 1;
                        }
                    }
                    else if (tagId == 41.ToString())
                    {
                        tagId = "(41) ID первичной заявки";
                    }
                    else if (tagId == 44.ToString())
                    {
                        tagId = "(44) Цена (стоимость)";
                        if (!double.TryParse(tagVal, out price))
                            price = -1;
                    }
                    else if (tagId == 59.ToString())
                    {
                        tagId = "(59) Заявка истекает в конце торгового дня";
                    }
                    else if (tagId == 60.ToString())
                    {
                        tagId = "(60) Время транзакции";
                    }
                    else if (tagId == 10.ToString())
                    {
                        tagId = "(10) Контрольная сумма"; //CheckSum
                    }
                    Console.WriteLine(string.Format("{0} - {1}", tagId, tagVal));
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message + " trace: " + e.StackTrace);
            }


            //SEND TO DB
            var _db = new EFDbContext();

            var all_instruments = _db.financeInstruments.ToList();
            if (isNewOrder)
            {
                int instrumentId = 0;
                foreach (var intr in all_instruments)
                {
                    if (intr.code == instrumentCode) instrumentId = intr.id;
                }
                int accountId = 0;
                foreach (var acc in _db.accounts.ToList())
                {
                    if (acc.organizationId == orgId && acc.accountTypeId == 300) accountId = acc.id;
                }
                Console.WriteLine("  ");
                Console.WriteLine("Поля и их значения");
                Console.WriteLine("ID ценной бумаги (ID инструемнта)");
                Console.WriteLine("instrumentId - " + instrumentId);
                Console.WriteLine("  ");
                Console.WriteLine("ID счета");
                Console.WriteLine("accountId - " + accountId);
                Console.WriteLine("  ");
                Console.WriteLine("ID организации");
                Console.WriteLine("orgId - " + orgId);
                Console.WriteLine("  ");
                Console.WriteLine("Направления заявки");
                Console.WriteLine("bidDirection - " + bidDirection);
                Console.WriteLine("  ");
                Console.WriteLine("Тип заявки");
                Console.WriteLine("bidType - " + bidType);
                Console.WriteLine("  ");
                Console.WriteLine("Цена");
                Console.WriteLine("price - " + price);
                Console.WriteLine("  ");
                Console.WriteLine("Стоимость");
                Console.WriteLine("amount - " + amount);
                Console.WriteLine("  ");
                Console.WriteLine("ID пользователя");
                Console.WriteLine("userId - " + userId);
                try
                {
                    sendToKse(new
                    {
                        organizationId = orgId,
                        financeInstrumentId = instrumentId,
                        userId = userId,
                        accountId,
                        bidDirection,
                        bidType,
                        lots = new[]
                    {
                        new
                        {
                            price,
                            amount
                        }
                    }
                    });
                }
                catch (Exception e)
                {

                }
            }
        }
        
        private void sendToKse(object reqObj)
        {
            var jsonInString = JsonConvert.SerializeObject(reqObj);
            var createBidURL = "http://192.168.2.150:5002/api/Trading/CreateFixBid";
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.PostAsync(createBidURL, new StringContent(jsonInString, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();
            try
            {
                response.EnsureSuccessStatusCode();
                var res = JsonConvert.DeserializeObject<responseObj>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                if (res.isSuccess)
                {
                    Console.WriteLine("  ");
                    Console.WriteLine("OUT (ответ от биржи): Ваша заявка успешно принята в ТС!");
                }
                else
                {
                    if (res.errors != null && res.errors.Length > 0)
                        Console.WriteLine("Ваша заявка не принята в ТС из-за причины: " + res.errors[0]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ваша заявка не доставлена в ТС из-за причины: " + response.StatusCode.ToString());
            }
        }

        public void ToApp(Message message, SessionID sessionID)
        {
            //Console.WriteLine("OUT (ответ от биржи): " + message);
            //Console.WriteLine("OUT2:  HELLO WORLD-");
            Console.WriteLine("  ");
            Console.WriteLine("  ");
            Console.WriteLine("TEST OUT: " + message);
            Console.WriteLine("OUT2:  TEST-");
        }

        public void FromAdmin(Message message, SessionID sessionID) 
        {
            Console.WriteLine("IN (передача запроса на биржу):  " + message);
            Console.WriteLine("IN2:  HELLO ADMIN-");
        }

        public void ToAdmin(Message message, SessionID sessionID)
        {
            Console.WriteLine("OUT (ответ от биржи):  " + message);
            Console.WriteLine("OUT2:  HELLO ADMIN-");
        }

        public void OnCreate(SessionID sessionID) { }
        public void OnLogout(SessionID sessionID) { }
        public void OnLogon(SessionID sessionID) { }
        #endregion
    }
    public class responseObj
    {
        public bool isSuccess { get; set; }
        public string[] errors { get; set; }
    }
}