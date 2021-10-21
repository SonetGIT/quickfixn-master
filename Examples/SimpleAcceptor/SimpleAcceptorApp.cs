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
            Console.WriteLine("TAG SimpleAcceptor = Значения: РАСШИФРОВКА СООБЩЕНИЯ");
            bool isNewOrder = false;
            string instrumentCode = "";
            int orgId = 0;
            var bidDirection = 0; //0-BUY, 1-SELL
            var bidType = 1; //0-MARKET, 1-LIMITED
            double price = 0;
            double amount = 0;
            var userId = "";
            bool tag21 = false;
            bool isQuoteRequest = false;
            try
            {
                foreach (var tagObj in sections)
                {
                    if (tagObj.Split('=').Length < 2) continue;
                    var tagId = tagObj.Split('=')[0];
                    var tagVal = tagObj.Split('=')[1];

                    if (tagId == 1.ToString())
                    {
                        tagId = "(1) Торговый счет";
                    }
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
                        if (tagVal == "R")
                        {
                            tagVal = "Запрос на котировку";
                            isQuoteRequest = true;
                        }
                        if (tagVal == "D")
                        {
                            tagVal = "Новая заявка";
                            isNewOrder = true;
                        }
                        else if (tagVal == "F")
                        {
                            tagVal = "Отменить заявка";
                        }
                        else if (tagVal == "G")
                        {
                            tagVal = "Заменить заявка";
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
                    else if (tagId == 131.ToString())
                    {
                        tagId = "(131) № запроса на котировку";
                    }
                    else if (tagId == 146.ToString())
                    {
                        tagId = "(146) Задает указанное количество повторяющихся символов"; //NoRelatedSym
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
                    createSellBuyOrderToKse(new
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

            if (isQuoteRequest)
            {
                _session = Session.LookupSession(sessionID);
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
                    var data = getQuotationsFromKse(instrumentCode);
                    if (data != null && data.Length > 0)
                    {
                        _session.Send(CreateMarketDataIncrementalRefresh44(instrumentCode, data));
                    }
                    else
                        Console.WriteLine("Котировки по данному инструменту отсутствуют");
                }
                catch (Exception e)
                {

                }                
            }
                
        }
        //Генерация ответа на котировки
        private QuickFix.FIX44.MarketDataIncrementalRefresh CreateMarketDataIncrementalRefresh44(string instrumentCode, responseQuoteItem._item[] data)
        {
            string qrid = new Random().Next(111111111, 999999999).ToString();
            QuickFix.Fields.QuoteRespID QuoteRespID = new QuickFix.Fields.QuoteRespID(qrid);
            QuickFix.Fields.QuoteRespType quoteRespType = new QuickFix.Fields.QuoteRespType(QuoteRespType.HIT_LIFT);

            // create QuoteRequest instance
            QuickFix.FIX44.MarketDataIncrementalRefresh message = new QuickFix.FIX44.MarketDataIncrementalRefresh();

            MDReqID mdreqid = new MDReqID();
            //Количество записей в сообщении с рыночными данными.
            NoMDEntries nomdentries = new NoMDEntries();
            QuickFix.FIX44.MarketDataIncrementalRefresh.NoMDEntriesGroup group = new QuickFix.FIX44.MarketDataIncrementalRefresh.NoMDEntriesGroup();
            DeleteReason deletereason = new DeleteReason();

            int list = nomdentries.getValue();

            for (uint i = 0; i < data.Length; i++)
            {
                var dataItem = data[i];
                var gr = new QuickFix.FIX44.MarketDataIncrementalRefresh.NoMDEntriesGroup();
                MDUpdateAction mdupdateaction = new MDUpdateAction('0');
                gr.Set(mdupdateaction);
                /*if (mdupdateaction.getValue() == '2')
                    Console.WriteLine("Enter");*/
                //group.get(deletereason);
                MDEntryType mdentrytype = new MDEntryType();
                if(dataItem.leadingNo != null) //SELL = Offer(1)
                {
                    mdentrytype.setValue('1');
                }
                else if (dataItem.trailingNo != null) //BUY = Bid(0)
                {
                    mdentrytype.setValue('0');
                }
                gr.Set(mdentrytype);

                MDEntryID mdentryid = new MDEntryID((i+1).ToString());
                gr.Set(mdentryid);

                Symbol symbol = new Symbol(instrumentCode);
                gr.Set(symbol);

                //Автор ввода рыночных данных
                MDEntryOriginator mdentryoriginator = new MDEntryOriginator();
                gr.Set(mdentryoriginator);

                //Цена ввода рыночных данных.
                MDEntryPx mdentrypx = new MDEntryPx();
                if (mdupdateaction.getValue() == '0')
                    gr.Set(mdentrypx);

                Currency currency = new Currency();
                    gr.Set(currency);

                //Количество акций, представленных вводом рыночных данных.
                MDEntrySize mdentrysize = new MDEntrySize();
                if (mdupdateaction.getValue() == '0')
                    gr.Set(mdentrysize);
                gr.AddGroup(group);

                /*Дата истечения срока действия ордера (последний день, когда ордер может торговать).*/
                ExpireDate expiredate = new ExpireDate();

                //Время / дата истечения срока действия заказа(всегда выражается в UTC (всемирное координированное время, также известное как «GMT»)
                ExpireTime expiretime = new ExpireTime();

                //Количество ордеров на рынке
                NumberOfOrders numberoforders = new NumberOfOrders();

                //Отображение позиции заявки или предложения
                MDEntryPositionNo mdentrypositionno = new MDEntryPositionNo();
            }

            message.Set(nomdentries);
            message.AddGroup(group);


            // Symbol, OrderQty and Account are in a repeating groups
            //QuickFix.Group group = new QuickFix.Group(QuickFix.Fields.Tags.NoRelatedSym, QuickFix.Fields.Tags.Symbol);
            group.SetField(new QuickFix.Fields.Symbol(instrumentCode));
            group.SetField(new QuickFix.Fields.OrderQty(500));
            //group.SetField();

            message.AddGroup(group);

            return message;
        }
        //Метод подачи заявки на SELL/BUY
        private void createSellBuyOrderToKse(object reqObj)
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
                    Console.WriteLine("sendToKse OUT (ответ от биржи): Ваша заявка успешно принята в ТС!");
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

        private responseQuoteItem._item[] getQuotationsFromKse(string instrumentCode)
        {
            var quoteURL = "http://192.168.2.150:5002/api/Trading/GetInstrumentQuotation?instrumentCode=" + instrumentCode;
            HttpClient client = new HttpClient();
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.GetAsync(quoteURL).GetAwaiter().GetResult();
            try
            {
                response.EnsureSuccessStatusCode();
                var res = JsonConvert.DeserializeObject<responseQuoteItem>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                if (res.isSuccess)
                {
                    Console.WriteLine("  ");
                    Console.WriteLine("getQuotationsFromKse OUT (ответ от биржи): Данные котировок из ТС");
                    return res.data;
                }
                else
                {
                    if (res.errors != null && res.errors.Length > 0)
                        Console.WriteLine("Ваша запрос на котировки не принят ТС из-за причины: " + res.errors[0]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ваш запрос не доставлен в ТС из-за причины: " + response.StatusCode.ToString());
            }
            return null;
        }

        public void ToApp(Message message, SessionID sessionID)
        {
            Console.WriteLine("  ");
            Console.WriteLine("  ");
            Console.WriteLine("ToApp OUT: " + message);
        }

        public void FromAdmin(Message message, SessionID sessionID) 
        {
            Console.WriteLine("FromAdmin IN (передача запроса на биржу):  " + message);
        }

        public void ToAdmin(Message message, SessionID sessionID)
        {
            Console.WriteLine("ToAdmin OUT (ответ от биржи):  " + message);
        }
        Session _session = null;
        public void OnCreate(SessionID sessionID) {
            
        }
        public void OnLogout(SessionID sessionID) { }
        public void OnLogon(SessionID sessionID) { }
        #endregion
    }
    public class responseObj
    {
        public bool isSuccess { get; set; }
        public string[] errors { get; set; }
    }

    public class responseQuoteItem : responseObj
    {
     
        public _item[] data { get; set; }
        public class _item
        {
            public int? leadingNo { get; set; }
            public double? sellAmount { get; set; }
            public double? sellPrice { get; set; }

            public int? trailingNo { get; set; }
            public double? buyAmount { get; set; }
            public double? buyPrice { get; set; }
        }
    }
}