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
            Console.WriteLine("SimpleAcceptorApp.cs (ф-я FromApp):" + message);
            var sections = message.ToString().Split(Message.SOH);
            Console.WriteLine("  ");
            Console.WriteLine("tag = value: РАСШИФРОВКА ДАННЫХ КФБ ПЕРЕДАННЫХ КЛИЕНТУ");
            bool isNewOrder = false; //Новая заявка
            string orderId = "";
            bool isCancelOrder = false; //Отмена заявки
            string instrumentCode = "";
            int orgId = 0;
            var bidDirection = 0; //0-BUY, 1-SELL
            var bidType = 1; //0-MARKET, 1-LIMITED
            double price = 0;
            double amount = 0;
            var userId = "";
            bool tag21 = false;
            bool isQuoteRequest = false;
            bool deal = false;
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
                    if (tagId == 6.ToString())
                    {
                        tagId = "(6) Средняя цена сделок по заявке";
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
                    else if (tagId == 10.ToString())
                    {
                        tagId = "(10) Контрольная сумма"; //CheckSum
                    }
                    else if (tagId == 11.ToString())
                    {
                        tagId = "(11) Номер заявки в торговой системе брокера";
                        orderId = tagVal;
                    }
                    else if (tagId == 14.ToString())
                    {
                        tagId = "(14) Исполненная часть заявки";
                    }
                    else if (tagId == 15.ToString())
                    {
                        tagId = "(15) Код валюты цены";
                    }
                    else if (tagId == 17.ToString())
                    {
                        tagId = "(17) Уникальный идентификатор сделки";
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
                    else if (tagId == 34.ToString())
                    {
                        tagId = "(34) Номер сообщения"; //MsgSeqNum
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
                            tagVal = "Отменить заявку";
                            isCancelOrder = true;
                        }
                        else if (tagVal == "G")
                        {
                            tagVal = "Заменить заявка";
                        }
                        else if (tagVal == "8")
                        {
                            tagVal = "Запрос на сделку";
                            deal = true;
                        }
                    }
                    else if (tagId == 37.ToString())
                    {
                        tagId = "(37) Биржевой номер заявки";
                    }
                    else if (tagId == 38.ToString())
                    {
                        tagId = "(38) В объеме лотов (кол-во)";
                        if (!double.TryParse(tagVal, out amount))
                            amount = -1;
                    }
                    else if (tagId == 39.ToString())
                    {
                        tagId = "(39) Текущий статус заявки";
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
                    else if (tagId == 49.ToString())
                    {
                        tagId = "(49) От брокера (user)"; //TargetCompID
                        userId = tagVal;
                    }
                    else if (tagId == 52.ToString())
                    {
                        tagId = "(52) Время отправки"; //SendingTime
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
                    else if (tagId == 55.ToString())
                    {
                        tagId = "(55) На акции компании"; //Symbol
                        instrumentCode = tagVal;
                    }
                    else if (tagId == 56.ToString())
                    {
                        tagId = "(56) Отправитель (фирма)"; //SenderCompID
                        if (!int.TryParse(tagVal, out orgId))
                            orgId = 2;//int.Parse(tagVal);
                    }
                    else if (tagId == 58.ToString())
                    {
                        tagId = "(58) Ответ от биржи";
                    }
                    else if (tagId == 59.ToString())
                    {
                        tagId = "(59) Заявка истекает в конце торгового дня";
                    }
                    else if (tagId == 60.ToString())
                    {
                        tagId = "(60) Время транзакции";
                    }
                    else if (tagId == 131.ToString())
                    {
                        tagId = "(131) № запроса на котировку";
                    }
                    else if (tagId == 146.ToString())
                    {
                        tagId = "(146) Задает указанное количество повторяющихся символов"; //NoRelatedSym
                    }
                    else if (tagId == 150.ToString())
                    {
                        tagId = "(150) Тип отчета об исполнении заявки, который описывает назначение отчета";
                    }
                    else if (tagId == 151.ToString())
                    {
                        tagId = "(151) Неисполненная часть заявки";
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
                    else if (tagId == 268.ToString())
                    {
                        tagId = "(268) Кол-во записей в сообщении с рыночными данными"; //NoMDEntries
                    }
                    else if (tagId == 269.ToString())
                    {
                        tagId = "(269) Тип входа MD"; //MDEntryType
                    }
                    else if (tagId == 270.ToString())
                    {
                        tagId = "(270) Цена рыночных данных";
                    }
                    else if (tagId == 271.ToString())
                    {
                        tagId = "(271) Количество акций, представленных вводом рыночных данных";
                    }
                    else if (tagId == 278.ToString())
                    {
                        tagId = "(278) Уникальный идентификатор ввода рыночных данных";
                    }
                    else if (tagId == 279.ToString())
                    {
                        tagId = "(279) Тип действия обновления рыночных данных";
                    }
                    else if (tagId == 282.ToString())
                    {
                        tagId = "(282) Автор ввода рыночных данных";
                    }
                    else if (tagId == 448.ToString())
                    {
                        tagId = "(448) Идентификатор или код стороны. От чьего имени совершена сделка.";
                    }
                    else if (tagId == 453.ToString())
                    {
                        tagId = "(453) Количество элементов в группе";
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

            //НОВАЯ ЗАЯВКА
            if (isNewOrder)
            {
                int instrumentId = 0;
                foreach (var intr in all_instruments)
                {
                    if (intr.code == instrumentCode) instrumentId = intr.id;
                }
                int accountId = 0;
                var tradeAccountTypes = new int[] { 3001 };
                foreach (var acc in _db.accounts.ToList())
                {
                    if (acc.organizationId == orgId && tradeAccountTypes.Contains(acc.accountTypeId.Value)) accountId = acc.id;
                }
                try
                {
                    createSellBuyOrder(new responseNewOrderItem._item
                    {
                        fixOrderId = orderId,
                        organizationId = orgId,
                        financeInstrumentId = instrumentId,
                        userId = userId,
                        accountId = accountId,
                        bidDirection = bidDirection,
                        bidType = bidType,
                        lots = new []
                        {
                            new
                            {
                                price,
                                amount
                            }
                        }
                    }, sessionID, instrumentCode);
                }
                catch (Exception e)
                {

                }
            }
            //ОТМЕНА ЗАЯВКИ
            if (isCancelOrder)
            {
                int instrumentId = 0;
                foreach (var intr in all_instruments)
                {
                    if (intr.code == instrumentCode) instrumentId = intr.id;
                }
                int accountId = 0;
                var tradeAccountTypes = new int[] { 3001 };
                foreach (var acc in _db.accounts.ToList())
                {
                    if (acc.organizationId == orgId && tradeAccountTypes.Contains(acc.accountTypeId.Value)) accountId = acc.id;
                }
                try
                {
                    cancelSellBuyOrder(new responseCancelOrderItem._item
                    {
                        fixOrderId = orderId,
                        organizationId = orgId,
                        userId = userId,                       
                        bidDirection = bidDirection                        
                    }, sessionID, instrumentCode, orderId, userId);
                }
                catch (Exception e)
                {

                }
            }
            //КОТИРОВКА
            if (isQuoteRequest)
            {
                int instrumentId = 0;
                foreach (var intr in all_instruments)
                {
                    if (intr.code == instrumentCode) instrumentId = intr.id;
                }
                int accountId = 0;
                var tradeAccountTypes = new int[] { 3001 };
                foreach (var acc in _db.accounts.ToList())
                {
                    if (acc.organizationId == orgId && tradeAccountTypes.Contains(acc.accountTypeId.Value)) accountId = acc.id;
                }
                try
                {
                    getQuotationsFromKse(new responseQuoteItem._item[]
                    {
                         //leadingNo = bidDirection,
                         
                    }, sessionID, instrumentCode);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Котировки по данному инструменту отсутствуют");
                }                
            }
            //СДЕЛКИ
            if (deal)
            {
                //_session = Session.LookupSession(sessionID);               
                try
                {
                    getDealsFromKse(new responseDealsItem._item
                    {
                        userId = userId,
                        orderId = orderId,
                    }, sessionID, instrumentCode, userId);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Сделки по данному инструменту отсутствуют");
                }
            }
        }       

        //ГЕНЕРАЦИЯ ОТВЕТА НА НОВУЮ ЗАЯВКУ
        private QuickFix.FIX44.NewOrderSingle newOrder(string instrumentCode, responseNewOrderItem._item data, string text)
        {
            QuickFix.FIX44.NewOrderSingle newOrderSingle = new QuickFix.FIX44.NewOrderSingle(
            new ClOrdID(data.fixOrderId),
            new Symbol(instrumentCode),
            new Side(data.bidDirectionStr),
            new TransactTime(DateTime.Now),
            new OrdType(data.bidTypeStr));

            newOrderSingle.Set(new HandlInst('1'));
            newOrderSingle.Set(new OrderQty((decimal)data.lots[0].amount));
            newOrderSingle.Set(new TimeInForce('1'));
            newOrderSingle.Set(new Price((decimal)data.lots[0].price));
            var t = new QuickFix.Fields.Text(text);

            newOrderSingle.Set(t);
            return newOrderSingle;
        }

        //НОВАЯ ЗАЯВКА на SELL/BUY
        private void createSellBuyOrder(responseNewOrderItem._item reqObj, SessionID sessionId, string instrumentCode)
        {
            var jsonInString = JsonConvert.SerializeObject(reqObj);
            var createBidURL = "http://192.168.2.150:5002/api/Trading/CreateFixBid";
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.PostAsync(createBidURL, new StringContent(jsonInString, Encoding.UTF8, "application/json")).GetAwaiter().GetResult();

            _session = Session.LookupSession(sessionId);
            try
            {
                response.EnsureSuccessStatusCode();
                var res = JsonConvert.DeserializeObject<responseObj>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                if (res.isSuccess)
                {
                    Console.WriteLine("  ");
                    Console.WriteLine("Заявка клиента успешно принята в ТС!"); //Ответ для админ-а КФБ
                    _session.Send(newOrder(instrumentCode, reqObj, "Vasha zayavka uspeshno prinyata v TS!")); //Ответ для клиента от КФБ
                }
                else
                {
                    if (res.errors != null && res.errors.Length > 0)
                    {
                        Console.WriteLine("Заявка клиента не принята в ТС из-за причины: " + res.errors[0]);
                        _session.Send(newOrder(instrumentCode, reqObj, "Vasha zayavka ne prinyata v TS iz-za prichiny: " + res.errors[0]));
                    }
                }
            }
            catch (Exception e)
            {
                var s = "Ваша заявка не доставлена в ТС из-за причины: http-" + response.StatusCode.ToString() + "; " + e.Message + "; trace: " + e.StackTrace;
                Console.WriteLine(s);
                _session.Send(newOrder(instrumentCode, reqObj, s));
            }
        }

        //ГЕНЕРАЦИЯ ОТВЕТА НА ОТМЕНУ ЗАЯВКИ
        private QuickFix.FIX44.OrderCancelRequest cancelOrder(string instrumentCode, responseCancelOrderItem._item data, string orderId, string userId, string text)
        {
            QuickFix.FIX44.OrderCancelRequest orderCancelRequest = new QuickFix.FIX44.OrderCancelRequest(
            new ClOrdID(data.fixOrderId),
            new OrigClOrdID(data.fixOrderId),
            new Symbol(instrumentCode),
            new Side(data.bidDirectionStr),
            new TransactTime(DateTime.Now));
            var t = new QuickFix.Fields.Text(text);

            orderCancelRequest.Set(t);
            return orderCancelRequest;
        }

        //ОТМЕНА ЗАЯВКИ
        private void cancelSellBuyOrder(responseCancelOrderItem._item reqObj, SessionID sessionId, string instrumentCode, string orderId, string userId)
        {           
            var cancelOrderBidURL = "http://192.168.2.150:5002/api/Trading/TerminateFixBid?orderId=" + orderId + "&userId=" + userId;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.GetAsync(cancelOrderBidURL).GetAwaiter().GetResult();
            _session = Session.LookupSession(sessionId);
            try
            {
                response.EnsureSuccessStatusCode();
                var res = JsonConvert.DeserializeObject<responseObj>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                if (res.isSuccess)
                {
                    Console.WriteLine("  ");
                    Console.WriteLine("Заявка клиента успешно отменен в ТС!");
                    _session.Send(cancelOrder(instrumentCode, reqObj, userId, orderId, "Vasha zayavka na otmenu prinyata v TS!")); //Ответ для клиента от КФБ
                }
                else
                {
                    if (res.errors != null && res.errors.Length > 0)
                        Console.WriteLine("Заявка клиента на отмену не принята в ТС из-за причины: " + res.errors[0]);
                        _session.Send(cancelOrder(instrumentCode, reqObj, userId, orderId, "Vasha zayavka na otmenu ne prinyata v TS iz-za prichiny: " + res.errors[0]));
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("Ваша заявка на отмену не доставлена в ТС из-за причины: " + response.StatusCode.ToString());
                var s = "Ваша заявка не отменен в ТС из-за причины: http-" + response.StatusCode.ToString() + "; " + e.Message + "; trace: " + e.StackTrace;
                Console.WriteLine(s);
                _session.Send(cancelOrder(instrumentCode, reqObj, userId, orderId, s));
            }
        }

        //ГЕНЕРАЦИЯ ОТВЕТА НА КОТИРОВКИ                                                                     
        private QuickFix.FIX44.MarketDataIncrementalRefresh quotation(string instrumentCode, responseQuoteItem._item[] data, string text)
        {
            string qrid = new Random().Next(111111111, 999999999).ToString();
            QuickFix.Fields.QuoteRespID QuoteRespID = new QuickFix.Fields.QuoteRespID(qrid);
            QuickFix.Fields.QuoteRespType quoteRespType = new QuickFix.Fields.QuoteRespType(QuoteRespType.HIT_LIFT);

            // create QuoteRequest instance
            QuickFix.FIX44.MarketDataIncrementalRefresh message = new QuickFix.FIX44.MarketDataIncrementalRefresh();

            MDReqID mdreqid = new MDReqID();
            //Количество записей в сообщении с рыночными данными.
            NoMDEntries nomdentries = new NoMDEntries(data.Length);
            QuickFix.FIX44.MarketDataIncrementalRefresh.NoMDEntriesGroup group = new QuickFix.FIX44.MarketDataIncrementalRefresh.NoMDEntriesGroup();
            MDUpdateAction mdupdateaction = new MDUpdateAction('0');
            group.Set(mdupdateaction);
            DeleteReason deletereason = new DeleteReason();

            //int list = nomdentries.getValue();
            for (uint i = 0; i < data.Length; i++)
            {
                var dataItem = data[i];
                var gr = new QuickFix.FIX44.MarketDataIncrementalRefresh.NoMDEntriesGroup();
                mdupdateaction = new MDUpdateAction('0');
                gr.Set(mdupdateaction);

                MDEntryType mdentrytype = new MDEntryType();
                if (dataItem.leadingNo != null) //SELL = Offer(1)
                {
                    mdentrytype.setValue('1');
                }
                else if (dataItem.trailingNo != null) //BUY = Bid(0)
                {
                    mdentrytype.setValue('0');
                }
                gr.Set(mdentrytype);

                MDEntryID mdentryid = new MDEntryID((i + 1).ToString());
                gr.Set(mdentryid);

                //Автор ввода рыночных данных (tagId282)
                MDEntryOriginator mdentryoriginator = new MDEntryOriginator(dataItem.orgName.ToString());
                gr.Set(mdentryoriginator);

                //Цена рыночных данных.
                MDEntryPx mdentrypx = new MDEntryPx(dataItem.buyPrice != null ? (decimal)dataItem.buyPrice.Value : dataItem.sellPrice != null ? (decimal)dataItem.sellPrice.Value : 0m);
                if (mdupdateaction.getValue() == '0')
                    gr.Set(mdentrypx);

                //Код валюты цены
                Currency currency = new Currency("KGS");
                gr.Set(currency);

                //Количество акций, представленных вводом рыночных данных.
                MDEntrySize mdentrysize = new MDEntrySize(dataItem.buyAmount != null ? (decimal)dataItem.buyAmount.Value : dataItem.sellAmount != null ? (decimal)dataItem.sellAmount.Value : 0m);
                if (mdupdateaction.getValue() == '0')
                    gr.Set(mdentrysize);
                message.AddGroup(gr);

                /*Дата истечения срока действия ордера (последний день, когда ордер может торговать).*/
                ExpireDate expiredate = new ExpireDate();

                //Время / дата истечения срока действия заказа(всегда выражается в UTC (всемирное координированное время, также известное как «GMT»)
                ExpireTime expiretime = new ExpireTime();

                //Количество ордеров на рынке
                NumberOfOrders numberoforders = new NumberOfOrders();

                //Отображение позиции заявки или предложения
                MDEntryPositionNo mdentrypositionno = new MDEntryPositionNo();
            }
            message.AddGroup(group);
            return message;
        }

        //КОТИРОВКА
        private void getQuotationsFromKse(responseQuoteItem._item[] data, SessionID sessionId, string instrumentCode)
        {
            var quoteURL = "http://192.168.2.150:5002/api/Trading/GetInstrumentQuotation?instrumentCode=" + instrumentCode;
            HttpClient client = new HttpClient();
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.GetAsync(quoteURL).GetAwaiter().GetResult();
            _session = Session.LookupSession(sessionId);
            try
            {
                response.EnsureSuccessStatusCode();
                var res = JsonConvert.DeserializeObject<responseQuoteItem>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                if (res.isSuccess)
                {
                    Console.WriteLine("  ");
                    Console.WriteLine("Данные котировок из ТС в клиент");
                    _session.Send(quotation(instrumentCode, res.data, "Dannye kotirovok iz TS!")); //Ответ для клиента от КФБ
                    //return res.data;
                }
                else
                {
                    if (res.errors != null && res.errors.Length > 0)
                        Console.WriteLine("Запрос клиента на котировки не принят ТС из-за причины: " + res.errors[0]);                    
                        _session.Send(quotation(instrumentCode, res.data, "Vasha zapros na kotirovki ne prinyat TS iz-za prichiny: " + res.errors[0]));
                }
            }
            catch (Exception e)
            {
                var s = "Запрос на котировки не принят ТС из-за причины: http-" + response.StatusCode.ToString() + "; " + e.Message + "; trace: " + e.StackTrace;
                _session.Send(quotation(instrumentCode, data, s));
            }
            //return null;
        }

        //ГЕНЕРАЦИЯ ОТВЕТА НА СДЕЛКИ
        private QuickFix.FIX44.ExecutionReport deals(string instrumentCode, responseDealsItem._item data, string userId, string text)
        {
            if (data != null)
            {
                QuickFix.FIX44.ExecutionReport dealsExReport = new QuickFix.FIX44.ExecutionReport(
                new OrderID("321"),
                new ExecID("123"),
                new ExecType(ExecType.NEW),
                new OrdStatus('0'),
                new Symbol(instrumentCode),
                new Side(data.side),
                new LeavesQty(0),
                new CumQty(data.cumQty),
                new AvgPx(data.avgPx));
                var t = new QuickFix.Fields.Text(text);
                dealsExReport.Set(t);
                return dealsExReport;
            }
            else
            {
                QuickFix.FIX44.ExecutionReport dealsExReport = new QuickFix.FIX44.ExecutionReport(
                new OrderID("321"),
                new ExecID("123"),
                new ExecType(ExecType.NEW),
                new OrdStatus('0'),
                new Symbol(instrumentCode),
                new Side('1'),
                new LeavesQty(0),
                new CumQty(0),
                new AvgPx(0));
                var t = new QuickFix.Fields.Text("Sdelok po dannomu instrumentu ne najdeno!");
                dealsExReport.Set(t);
                return dealsExReport;
            }
        }

        //СДЕЛКИ
        private void getDealsFromKse (responseDealsItem._item data, SessionID sessionId, string instrumentCode, string userId)
        {
            var dealsURL = "http://192.168.2.150:5002/api/Trading/GetDealsFix?instrumentCode=" + instrumentCode + "&userId=" + userId;
            HttpClient client = new HttpClient();
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = client.GetAsync(dealsURL).GetAwaiter().GetResult();
            _session = Session.LookupSession(sessionId);
            try
            {
                response.EnsureSuccessStatusCode();
                var res = JsonConvert.DeserializeObject<responseDealsItem>(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                if (res.isSuccess)
                {
                    Console.WriteLine("  ");
                    Console.WriteLine("Данные сделок из ТС!");
                    _session.Send(deals (instrumentCode, res.data, userId, "Dannye sdelok iz TS!")); //Ответ для клиента от КФБ
                    //return res.data;
                }
                else
                {
                    if (res.errors != null && res.errors.Length > 0)
                        Console.WriteLine("Запрос клиента на сделки не принят ТС из-за причины: " + res.errors[0]);
                    _session.Send(deals(instrumentCode, res.data, userId, "Vash zapros na sdelki ne prinyat TS iz-za prichiny: " + res.errors[0]));
                }
            }
            catch (Exception e)
            {
                var s = "Ваш запрос на сделку не принят в ТС из-за причины: http-" + response.StatusCode.ToString() + "; " + e.Message + "; trace: " + e.StackTrace;
                Console.WriteLine(s);
                _session.Send(deals(instrumentCode, data, userId, s));
            }
            //return null;
        }
               
        //это функция обратного вызова, которая вызывается всякий раз, когда сообщение отправляется контрагенту
        public void ToApp(Message message, SessionID sessionID)
        {
        }

        public void FromAdmin(Message message, SessionID sessionID)
        {
            Console.WriteLine("FromAdmin IN:  " + message);
        }

        public void ToAdmin(Message message, SessionID sessionID)
        {
            Console.WriteLine("ToAdmin OUT:  " + message);
        }

        Session _session = null;
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

    //Item подачи заявок
    public class responseNewOrderItem : responseObj
    {
        public _item data { get; set; }
        public class _item
        {
            public string fixOrderId { get; set; }
            public int? organizationId { get; set; }
            public int? financeInstrumentId { get; set; }
            public string userId { get; set; }
            public int accountId { get; set; }
            public int bidDirection { get; set; }
            public char bidDirectionStr { get { return bidDirection == 0 ? '1' : '2'; } }
            public int bidType { get; set; }
            public char bidTypeStr { get { return bidType == 0 ? '1' : '2'; } }
            public dynamic[] lots;
        }
    }
    //Item отмены заявки
    public class responseCancelOrderItem : responseObj
    {
        public _item data { get; set; }
        public class _item
        {
            public string fixOrderId { get; set; }
            public int? organizationId { get; set; }
            public int? financeInstrumentId { get; set; }
            public string userId { get; set; }
            public string orderId { get; set; }
            public int accountId { get; set; }
            public int bidDirection { get; set; }
            public char bidDirectionStr { get { return bidDirection == 0 ? '1' : '2'; } }
        }
    }

    //Item котировок
    public class responseQuoteItem : responseObj
    {     
        public _item[] data { get; set; }
        public class _item
        {
            public int? leadingNo { get; set; }
            public char leadingNoStr { get { return leadingNo == 0 ? '1' : '2'; } }
            public double? sellAmount { get; set; }
            public double? sellPrice { get; set; }
            public int? trailingNo { get; set; }
            public double? buyAmount { get; set; }
            public double? buyPrice { get; set; }
            public string orgName { get; set; }
        }
    }

    //Item сделок
    public class responseDealsItem : responseObj
    {
        public _item data { get; set; }
        public class _item
        {
            public string orderId { get; set; }
            public string execId { get; set; }
            public string userId { get; set; }
            public char side { get; set; }
            public string text { get; set; }
            public decimal leavesQty { get; set; }
            public decimal cumQty { get; set; }
            public decimal avgPx { get; set; }
            public string partnerName { get; set; }
            public string instrumentCode { get; set; }
            public DateTime transactTime { get; set; }
        }
    }
}