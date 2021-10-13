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
            Console.WriteLine("IN:  " + message.ToString());
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
            Console.WriteLine("OUT: " + message.ToString());
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
        #endregion


        public void Run()
        {
            while (true)
            {
                try
                {
                    char action = QueryAction();
                    if (action == '1')
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
                + "1) Ввести заказ\n"
                + "2) Отменить заказ\n"
                + "3) Заменить заказ\n"
                //+ "4) Тест рыночных данных\n"
                + "Q) Выход\n"
                + "Действие:"
            );

            HashSet<string> validActions = new HashSet<string>("1,2,3,4,q,Q,g,x".Split(','));

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
            if (m != null && QueryConfirm("Отправить заказ"))
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
            if (m != null && QueryConfirm("Отменить заказ"))
                SendMessage(m);
        }

        private void QueryReplaceOrder()
        {
            Console.WriteLine("\nCancelReplaceRequest");

            QuickFix.FIX44.OrderCancelReplaceRequest m = QueryCancelReplaceRequest44();

            //if (m != null && QueryConfirm("Send replace"))
            if (m != null && QueryConfirm("Отправить замену"))
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



