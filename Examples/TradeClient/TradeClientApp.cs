using System;
using QuickFix;
using QuickFix.Fields;
using System.Collections.Generic;

namespace TradeClient
{
    public class TradeClientApp : QuickFix.MessageCracker, QuickFix.IApplication
    {
        Session _session = null;

        // This variable is a kludge for developer test purposes.  Don't do this on a production application.
        public IInitiator MyInitiator = null;

        #region IApplication interface overrides

        public void OnCreate(SessionID sessionID)
        {
            _session = Session.LookupSession(sessionID);
        }

        public void OnLogon(SessionID sessionID) { Console.WriteLine("Logon - " + sessionID.ToString()); }
        public void OnLogout(SessionID sessionID) { Console.WriteLine("Logout - " + sessionID.ToString()); }

        public void FromAdmin(Message message, SessionID sessionID) { }
        public void ToAdmin(Message message, SessionID sessionID) { }

        public void FromApp(Message message, SessionID sessionID)
        {
            Console.WriteLine("FromApp IN TradeClient:  " + message.ToString());
            try
            {
                Crack(message, sessionID);
            }
            catch (Exception ex)
            {
                Console.WriteLine("==Cracker exception==");
                Console.WriteLine(ex.ToString());
                Console.WriteLine(ex.StackTrace);
            }
        }

        
        public void ToApp(Message message, SessionID sessionID)
        {
            try
            {
                bool possDupFlag = false;
                if (message.Header.IsSetField(QuickFix.Fields.Tags.PossDupFlag))
                {
                    possDupFlag = QuickFix.Fields.Converters.BoolConverter.Convert(
                        message.Header.GetString(QuickFix.Fields.Tags.PossDupFlag)); /// FIXME
                }
                if (possDupFlag)
                    throw new DoNotSend();
            }
            catch (FieldNotFoundException)
            { }

            Console.WriteLine();
            Console.WriteLine("ToApp OUT TradeClient: " + message.ToString());
            var sections = message.ToString().Split(Message.SOH);
            Console.WriteLine("  ");
            Console.WriteLine("TAG TradeClient = Значения: РАСШИФРОВКА СООБЩЕНИЯ");
            bool isNewOrder = false;
            string instrumentCode = "";
            int orgId = 0;
            var bidDirection = 0; //0-BUY, 1-SELL
            var bidType = 1; //0-MARKET, 1-LIMITED
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
                            orgId = 2; //int.Parse(tagVal);
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " trace: " + e.StackTrace);
            }
        }
        #endregion


        #region MessageCracker handlers
        public void OnMessage(QuickFix.FIX44.ExecutionReport m, SessionID s)
        {
            //Console.WriteLine("Received execution report");
            Console.WriteLine("Получен отчет об исполнении");            
        }

        public void OnMessage(QuickFix.FIX44.OrderCancelReject m, SessionID s)
        {
            //Console.WriteLine("Received order cancel reject");
            Console.WriteLine("Полученный заказ отменить/отклонить");
        }

        public void OnMessage(QuickFix.FIX44.QuoteResponse m, SessionID s)
        {
            //Console.WriteLine("Received order cancel reject");
            Console.WriteLine("!!!Получен запрос на котировки!!!!!!");
        }
        #endregion


        public void Run()
        {
            while (true)
            {
                try
                {
                    char action = QueryAction();
                    if (action == '0')
                        QueryQuoteRequest();
                    else if (action == '1')
                        QueryEnterOrder();
                    else if (action == '2')
                        QueryCancelOrder();
                    else if (action == '3')
                        QueryReplaceOrder();
                    else if (action == '4')
                        QueryMarketDataRequest();
                    else if (action == 'g')
                    {
                        if (this.MyInitiator.IsStopped)
                        {
                            //Console.WriteLine("Restarting initiator...");
                            Console.WriteLine("Инициатор перезапуска...");
                            this.MyInitiator.Start();
                        }
                        else
                            //Console.WriteLine("Already started.");
                            Console.WriteLine("Уже начали.");
                    }
                    else if (action == 'x')
                    {
                        if (this.MyInitiator.IsStopped)
                            //Console.WriteLine("Already stopped.");
                            Console.WriteLine("Уже остановлено.");
                        else
                        {
                            //Console.WriteLine("Stopping initiator...");
                            Console.WriteLine("Остановка инициатора...");
                            this.MyInitiator.Stop();
                        }
                    }
                    else if (action == 'q' || action == 'Q')
                        break;
                }
                catch (System.Exception e)
                {
                    //Console.WriteLine("Message Not Sent: " + e.Message);
                    Console.WriteLine("Сообщение не отправлено: " + e.Message);
                    
                    //Console.WriteLine("StackTrace: " + e.StackTrace);
                    Console.WriteLine("Трассировка стека: " + e.StackTrace);
                    /*Трассировка стека — это отчёт о действующих кадрах стека в определённый момент времени во время выполнения программы. 
                      Когда программа запускается, память обычно динамически выделяется в двух местах; на стеке и в куче.
                      Память постоянно выделяется на стеке, но не обязательно в куче*/
                }
            }
            //Console.WriteLine("Program shutdown.");
            Console.WriteLine("Завершение работы программ.");            
        }

        private void SendMessage(Message m)
        {
            if (_session != null)
                _session.Send(m);
            else
            {
                // This probably won't ever happen.
                //Console.WriteLine("Can't send message: session not created.");
                Console.WriteLine("Не удается отправить сообщение: сеанс не создан.");                
            }
        }

        private char QueryAction()
        {
            // Commands 'g' and 'x' are intentionally hidden.
            //Console.Write("\n"
            //    + "1) Enter Order\n"
            //    + "2) Cancel Order\n"
            //    + "3) Replace Order\n"
            //    + "4) Market data test\n"
            //    + "Q) Quit\n"
            //    + "Action: "
            //);
            Console.Write("\n"
                + "0) Котировка\n"
                + "1) Ввод заявки\n"
                + "2) Отменить заявку\n"
                + "3) Заменить заявку\n"
                //+ "4) Тест рыночных данных\n"
                + "Q) Выход\n"
                + "Действие:"
            );

            HashSet<string> validActions = new HashSet<string>("0,1,2,3,4,q,Q,g,x".Split(','));

            string cmd = Console.ReadLine().Trim();
            if (cmd.Length != 1 || validActions.Contains(cmd) == false)
                throw new System.Exception("Invalid action");

            return cmd.ToCharArray()[0];
        }

        private void QueryEnterOrder()
        {
            Console.WriteLine("\nNewOrderSingle");

            QuickFix.FIX44.NewOrderSingle m = QueryNewOrderSingle44();

            //if (m != null && QueryConfirm("Send order"))
            if (m != null && QueryConfirm("Отправить заказ?"))
            {
                m.Header.GetString(Tags.BeginString);

                SendMessage(m);
            }
        }

        private void QueryCancelOrder()
        {
            Console.WriteLine("\nOrderCancelRequest");

            QuickFix.FIX44.OrderCancelRequest m = QueryOrderCancelRequest44();

            //if (m != null && QueryConfirm("Cancel order"))
            if (m != null && QueryConfirm("Отменить заказ?"))
                SendMessage(m);
        }

        private void QueryReplaceOrder()
        {
            Console.WriteLine("\nCancelReplaceRequest");

            QuickFix.FIX44.OrderCancelReplaceRequest m = QueryCancelReplaceRequest44();

            //if (m != null && QueryConfirm("Send replace"))
            if (m != null && QueryConfirm("Отправить замену?"))
                SendMessage(m);
        }

        private void QueryMarketDataRequest()
        {
            Console.WriteLine("\nMarketDataRequest");

            QuickFix.FIX44.MarketDataRequest m = QueryMarketDataRequest44();

            //if (m != null && QueryConfirm("Send market data request"))
            if (m != null && QueryConfirm("Отправить запрос рыночных данных"))
                SendMessage(m);
        }

        private void QueryQuoteRequest()
        {
            Console.WriteLine("\nQuoteRequest");
            Console.WriteLine("Укажите свой торговый счет:");
            string accountNo = Console.ReadLine();
            Console.WriteLine("Укажите код инструмента:");
            string inctrumentCode = Console.ReadLine();
            QuickFix.FIX44.QuoteRequest m = QueryQuoteRequest44(accountNo, inctrumentCode);

            //if (m != null && QueryConfirm("Send market data request"))
            if (m != null && QueryConfirm("Отправить запрос котировок?"))
                SendMessage(m);
        }

        private bool QueryConfirm(string query)
        {
            Console.WriteLine();
            Console.WriteLine(query + "?: ");
            string line = Console.ReadLine().Trim();
            return (line.Equals("да") || line.Equals("Да"));
            //return (line[0].Equals('y') || line[0].Equals('Y'));
        }

        #region Message creation functions
        private QuickFix.FIX44.NewOrderSingle QueryNewOrderSingle44()
        {
            QuickFix.Fields.OrdType ordType = null;

            QuickFix.FIX44.NewOrderSingle newOrderSingle = new QuickFix.FIX44.NewOrderSingle(
                QueryClOrdID(),
                QuerySymbol(),
                QuerySide(),
                new TransactTime(DateTime.Now),
                ordType = QueryOrdType());

            newOrderSingle.Set(new HandlInst('1'));
            newOrderSingle.Set(QueryOrderQty());
            newOrderSingle.Set(QueryTimeInForce());
            if (ordType.getValue() == OrdType.LIMIT || ordType.getValue() == OrdType.STOP_LIMIT)
                newOrderSingle.Set(QueryPrice());
            if (ordType.getValue() == OrdType.STOP || ordType.getValue() == OrdType.STOP_LIMIT)
                newOrderSingle.Set(QueryStopPx());

            return newOrderSingle;
        }

        private QuickFix.FIX44.OrderCancelRequest QueryOrderCancelRequest44()
        {
            QuickFix.FIX44.OrderCancelRequest orderCancelRequest = new QuickFix.FIX44.OrderCancelRequest(
                QueryOrigClOrdID(),
                QueryClOrdID(),
                QuerySymbol(),
                QuerySide(),
                new TransactTime(DateTime.Now));

            orderCancelRequest.Set(QueryOrderQty());
            return orderCancelRequest;
        }

        private QuickFix.FIX44.OrderCancelReplaceRequest QueryCancelReplaceRequest44()
        {
            QuickFix.FIX44.OrderCancelReplaceRequest ocrr = new QuickFix.FIX44.OrderCancelReplaceRequest(
                QueryOrigClOrdID(),
                QueryClOrdID(),
                QuerySymbol(),
                QuerySide(),
                new TransactTime(DateTime.Now),
                QueryOrdType());

            ocrr.Set(new HandlInst('1'));
            //if (QueryConfirm("New price"))
            if (QueryConfirm("Новая цена"))                    
                ocrr.Set(QueryPrice());
            //if (QueryConfirm("New quantity"))
            if (QueryConfirm("Новое количество"))                    
                ocrr.Set(QueryOrderQty());

            return ocrr;
        }

        private QuickFix.FIX44.MarketDataRequest QueryMarketDataRequest44()
        {
            MDReqID mdReqID = new MDReqID("MARKETDATAID");
            SubscriptionRequestType subType = new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT);
            MarketDepth marketDepth = new MarketDepth(0);

            QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup marketDataEntryGroup = new QuickFix.FIX44.MarketDataRequest.NoMDEntryTypesGroup();
            marketDataEntryGroup.Set(new MDEntryType(MDEntryType.BID));

            QuickFix.FIX44.MarketDataRequest.NoRelatedSymGroup symbolGroup = new QuickFix.FIX44.MarketDataRequest.NoRelatedSymGroup();
            symbolGroup.Set(new Symbol("LNUX"));

            QuickFix.FIX44.MarketDataRequest message = new QuickFix.FIX44.MarketDataRequest(mdReqID, subType, marketDepth);
            message.AddGroup(marketDataEntryGroup);
            message.AddGroup(symbolGroup);

            return message;
        }
        private QuickFix.FIX44.QuoteRequest QueryQuoteRequest44(string accountNo, string instrumentCode = "KGZSb")
        {
            string qrid = new Random().Next(111111111, 999999999).ToString();
            QuickFix.Fields.QuoteReqID QuoteReqID = new QuickFix.Fields.QuoteReqID(qrid);

            // create QuoteRequest instance
            QuickFix.FIX44.QuoteRequest message = new QuickFix.FIX44.QuoteRequest(QuoteReqID);

            // Symbol, OrderQty and Account are in a repeating groups
            QuickFix.Group group = new QuickFix.Group(QuickFix.Fields.Tags.NoRelatedSym, QuickFix.Fields.Tags.Symbol);
            group.SetField(new QuickFix.Fields.Symbol(instrumentCode));
            group.SetField(new QuickFix.Fields.OrderQty(500));
            group.SetField(new QuickFix.Fields.Account(accountNo));

            // add this group to message
            message.AddGroup(group);

            return message;
        }
        #endregion

        #region field query private methods
        private ClOrdID QueryClOrdID()
        {
            Console.WriteLine();
            //Console.Write("ClOrdID? ");
            Console.Write("Идентификатор заказа клиента? ");
            return new ClOrdID(Console.ReadLine().Trim());
        }

        private OrigClOrdID QueryOrigClOrdID()
        {
            Console.WriteLine();
            //Console.Write("OrigClOrdID? ");
            Console.Write("Ориг. Идентификатор заказа клиента? ");
            return new OrigClOrdID(Console.ReadLine().Trim());
        }

        private Symbol QuerySymbol()
        {
            Console.WriteLine();
            //Console.Write("Symbol? ");
            Console.Write("Идентификатор/Символ? ");
            return new Symbol(Console.ReadLine().Trim());
        }

        private Side QuerySide()
        {
            //Console.WriteLine();
            //Console.WriteLine("1) Buy");
            //Console.WriteLine("2) Sell");
            //Console.WriteLine("3) Sell Short");
            //Console.WriteLine("4) Sell Short Exempt");
            //Console.WriteLine("5) Cross");
            //Console.WriteLine("6) Cross Short");
            //Console.WriteLine("7) Cross Short Exempt");
            //Console.Write("Side? ");
            
            Console.WriteLine();
            Console.WriteLine("1) Купить");
            Console.WriteLine("2) Продать");
            //Console.WriteLine("3) Продать на короткую позицию");
            //Console.WriteLine("4) Sell Short Exempt");
            //Console.WriteLine("5) Cross");
            //Console.WriteLine("6) Cross Short");
            //Console.WriteLine("7) Cross Short Exempt");
            Console.Write("Потвердить? ");
            string s = Console.ReadLine().Trim();

            char c = ' ';
            switch (s)
            {
                case "1": c = Side.BUY; break;
                case "2": c = Side.SELL; break;
                //case "3": c = Side.SELL_SHORT; break;
                //case "4": c = Side.SELL_SHORT_EXEMPT; break;
                //case "5": c = Side.CROSS; break;
                //case "6": c = Side.CROSS_SHORT; break;
                case "3": c = 'A'; break;
                //default: throw new Exception("unsupported input");
                default: throw new Exception("Неправильный ввод");
            }
            return new Side(c);
        }

        private OrdType QueryOrdType()
        {
            //Console.WriteLine();
            //Console.WriteLine("1) Market");
            //Console.WriteLine("2) Limit");
            //Console.WriteLine("3) Stop");
            //Console.WriteLine("4) Stop Limit");
            //Console.Write("OrdType? ");

            Console.WriteLine();
            Console.WriteLine("1) Рыночная заявка");
            Console.WriteLine("2) Лимитированная заявка");
            //Console.WriteLine("3) Stop");
            //Console.WriteLine("4) Stop Limit");
            Console.Write("Тип заявки? ");
            string s = Console.ReadLine().Trim();

            char c = ' ';
            switch (s)
            {
                case "1": c = OrdType.MARKET; break;
                case "2": c = OrdType.LIMIT; break;
                //case "3": c = OrdType.STOP; break;
                //case "4": c = OrdType.STOP_LIMIT; break;
                default: throw new Exception("Неправильный ввод");
            }
            return new OrdType(c);
        }

        private OrderQty QueryOrderQty()
        {
            Console.WriteLine();
            //Console.Write("OrderQty? ");
            Console.Write("Кол-во заказа? ");
            return new OrderQty(Convert.ToDecimal(Console.ReadLine().Trim()));
        }
        //TimeInForce - Срок действия сделки
        private TimeInForce QueryTimeInForce()
        {
            Console.WriteLine();
            Console.WriteLine("1) День");
            //Console.WriteLine("2) IOC"); //immediate-or-cancel - немедленная отмена
            //Console.WriteLine("3) OPG"); //fill-or-kill - заполнение или уничтожение
            //Console.WriteLine("4) GTC"); //good-'til-canceled - добро до отмены
            //Console.WriteLine("5) GTX");
            //Console.Write("TimeInForce? ");
            Console.Write("Срок действия заявки? ");
            string s = Console.ReadLine().Trim();

            char c = ' ';
            switch (s)
            {
                case "1": c = TimeInForce.DAY; break;
                //case "2": c = TimeInForce.IMMEDIATE_OR_CANCEL; break;
                //case "3": c = TimeInForce.AT_THE_OPENING; break;
                //case "4": c = TimeInForce.GOOD_TILL_CANCEL; break;
                //case "5": c = TimeInForce.GOOD_TILL_CROSSING; break;
                default: throw new Exception("Неправильный ввод");
            }
            return new TimeInForce(c);
        }

        private Price QueryPrice()
        {
            Console.WriteLine();
            Console.Write("Цена? ");
            return new Price(Convert.ToDecimal(Console.ReadLine().Trim()));
        }

        private StopPx QueryStopPx()
        {
            Console.WriteLine();
            //Console.Write("StopPx? ");
            Console.Write("Цена за кол-во? ");
            return new StopPx(Convert.ToDecimal(Console.ReadLine().Trim()));
        }

        #endregion
    }
}



