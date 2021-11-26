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

        //Инициирует сессию и соединение.
        public void OnLogon(SessionID sessionID) { Console.WriteLine("Logon - " + sessionID.ToString()); }

        //Инициирует или подтверждает разрыв соединения.
        public void OnLogout(SessionID sessionID) { Console.WriteLine("Logout - " + sessionID.ToString()); }

        public void FromAdmin(Message message, SessionID sessionID) { }
        public void ToAdmin(Message message, SessionID sessionID) { }

        public void FromApp(Message message, SessionID sessionID)
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
            catch (FieldNotFoundException) { }

            Console.WriteLine("  ");
            Console.WriteLine("TradeClientApp.cs (ф-я FromApp):" + message);
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
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " trace: " + e.StackTrace);
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
            catch (FieldNotFoundException) { }

            Console.WriteLine("  ");
            Console.WriteLine("TradeClientApp.cs (ф-я ToApp):" + message);
            var sections = message.ToString().Split(Message.SOH);
            Console.WriteLine("  ");
            Console.WriteLine("tag = value: РАСШИФРОВКА ДАННЫХ КЛИЕНТА ПЕРЕДАННЫХ В КФБ");
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

        public void OnMessage(QuickFix.FIX44.MarketDataIncrementalRefresh m, SessionID s)
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
                        QueryEnterOrder(); 
                    else if (action == '1')
                        QueryCancelOrder(); 
                    else if (action == '2')
                        QueryQuoteRequest();
                    else if (action == '3')
                        QueryDealsRequest();
                    else if (action == '4')
                        QueryMarketDataRequest();
                    else if (action == 'g')
                    {
                        if (this.MyInitiator.IsStopped)
                        {                            
                            Console.WriteLine("Инициатор перезапуска...");
                            this.MyInitiator.Start();
                        }
                        else                            
                            Console.WriteLine("Уже начали.");
                    }
                    else if (action == 'x')
                    {
                        if (this.MyInitiator.IsStopped)                         
                            Console.WriteLine("Уже остановлено.");
                        else
                        {                            
                            Console.WriteLine("Остановка инициатора...");
                            this.MyInitiator.Stop();
                        }
                    }
                    else if (action == 'q' || action == 'Q')
                        break;
                }
                catch (System.Exception e)
                {
                    Console.WriteLine("Сообщение не отправлено: " + e.Message);                                        
                    Console.WriteLine("Трассировка стека: " + e.StackTrace);
                    /*Трассировка стека — это отчёт о действующих кадрах стека в определённый момент времени во время выполнения программы. 
                      Когда программа запускается, память обычно динамически выделяется в двух местах; на стеке и в куче.
                      Память постоянно выделяется на стеке, но не обязательно в куче*/
                }
            }            
            Console.WriteLine("Завершение работы программ.");            
        }

        private void SendMessage(Message m)
        {
            if (_session != null)
                _session.Send(m);
            else
            {             
                Console.WriteLine("Не удается отправить сообщение: сеанс не создан.");                
            }
        }

        private char QueryAction()
        {
            // Команды 'g' и 'x' намеренно скрыты.
            Console.Write("\n"
               + "0) Ввод заявки\n"
               + "1) Отменить заявку\n"
               + "2) Котировка\n"
               + "3) Запрос на сделки\n"
               //+ "3) Заменить заявку\n"
               //+ "4) Котировка\n"
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

            if (m != null && QueryConfirm("Отправить заявку"))
            {
                m.Header.GetString(Tags.BeginString);
                SendMessage(m);
            }
        }

        private void QueryCancelOrder()
        {
            Console.WriteLine("\nOrderCancelRequest");
            QuickFix.FIX44.OrderCancelRequest m = QueryOrderCancelRequest44();
            
            if (m != null && QueryConfirm("Отменить заявку"))
                SendMessage(m);
        }

        private void QueryReplaceOrder()
        {
            Console.WriteLine("\nCancelReplaceRequest");
            QuickFix.FIX44.OrderCancelReplaceRequest m = QueryCancelReplaceRequest44();
            if (m != null && QueryConfirm("Отправить замену"))
                SendMessage(m);
        }

        private void QueryMarketDataRequest()
        {
            Console.WriteLine("\nMarketDataRequest");

            QuickFix.FIX44.MarketDataRequest m = QueryMarketDataRequest44();
            if (m != null && QueryConfirm("Отправить запрос рыночных данных"))
                SendMessage(m);
        }
        
        //КОТИРОВКА action = 2 
        private void QueryQuoteRequest()
        {
            Console.WriteLine("\nQuoteRequest");
            Console.WriteLine("Укажите свой торговый счет:");
            string accountNo = Console.ReadLine();
            Console.WriteLine("Укажите код инструмента:");
            string inctrumentCode = Console.ReadLine();
            QuickFix.FIX44.QuoteRequest m = QueryQuoteRequest44(accountNo, inctrumentCode);
            if (m != null && QueryConfirm("Отправить запрос котировок"))
                SendMessage(m);
        }
        private void QueryDealsRequest()
        {
            Console.WriteLine("\nDealsReques");
            Console.WriteLine("Укажите код инструмента:");
            string inctrumentCode = Console.ReadLine();
            QuickFix.FIX44.ExecutionReport m = QueryDealsRequest44(inctrumentCode);
            if (m != null && QueryConfirm("Отправить запрос на сделки"))
                SendMessage(m);
        }

        private bool QueryConfirm(string query)
        {
            Console.WriteLine();
            Console.WriteLine(query + "?: ");
            string line = Console.ReadLine().Trim();
            return (line.Equals("да") || line.Equals("Да"));
        }

        #region Message creation functions
        //НОВАЯ ЗАЯВКА
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

        //ОТМЕНА ЗАЯВКИ
        private QuickFix.FIX44.OrderCancelRequest QueryOrderCancelRequest44()
        {
            QuickFix.FIX44.OrderCancelRequest orderCancelRequest = new QuickFix.FIX44.OrderCancelRequest(
                QueryClOrdID(),
                QueryOrigClOrdID(),                
                QuerySymbol(),
                QuerySide(),
                new TransactTime(DateTime.Now));

            orderCancelRequest.Set(QueryOrderQty());
            return orderCancelRequest;
        }

        //ЗАМЕНА ЗАЯВКИ
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
            if (QueryConfirm("Новая цена"))                    
                ocrr.Set(QueryPrice());
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
            QuickFix.FIX44.QuoteRequest message = new QuickFix.FIX44.QuoteRequest(QuoteReqID);            
            QuickFix.Group group = new QuickFix.Group(QuickFix.Fields.Tags.NoRelatedSym, QuickFix.Fields.Tags.Symbol);
            group.SetField(new QuickFix.Fields.Symbol(instrumentCode));
            group.SetField(new QuickFix.Fields.OrderQty(500));
            group.SetField(new QuickFix.Fields.Account(accountNo));
            message.AddGroup(group);

            return message;
        }
        //СДЕЛКИ
        private QuickFix.FIX44.ExecutionReport QueryDealsRequest44(string instrumentCode = "KGZSb")
        {
            string qrid = new Random().Next(111111111, 999999999).ToString();
            QuickFix.Fields.OrderID OrderID = new QuickFix.Fields.OrderID(qrid);
            var HandlInst = new QuickFix.Fields.HandlInst(QuickFix.Fields.HandlInst.AUTOMATED_EXECUTION_ORDER_PRIVATE_NO_BROKER_INTERVENTION);
            var Symbol = new QuickFix.Fields.Symbol(instrumentCode);
            var Side = new Side('B');
            var LeavesQty = new QuickFix.Fields.LeavesQty(0);
            var CumQty = new QuickFix.Fields.CumQty(0);
            var AvgPx = new QuickFix.Fields.AvgPx(0);
            var TransactTime = new QuickFix.Fields.TransactTime(DateTime.Now);
            var Text = new QuickFix.Fields.Text("-");
            var ExecType = new QuickFix.Fields.ExecType(QuickFix.Fields.ExecType.NEW);
            QuickFix.FIX44.ExecutionReport message = new QuickFix.FIX44.ExecutionReport(OrderID, new ExecID("1"), ExecType, new OrdStatus('0'), Symbol, Side, LeavesQty, CumQty, AvgPx);                   

            return message;
        }
        #endregion

        #region field query private methods
        // ClOrdID Уникальный идентификатор сообщения Order Cancel Request (F) - запроса на снятие заявки
        private ClOrdID QueryClOrdID()
        {
            Console.WriteLine();
            //Console.Write("ClOrdID? ");
            Console.Write("Уникальный пользовательский идентификатор заявки? ");
            return new ClOrdID(Console.ReadLine().Trim());
        }

        /* OrigClOrdID - Пользовательский идентификатор заявки, которую надо снять. Условно
            обязательное, если не указан OrderID. */
        private OrigClOrdID QueryOrigClOrdID()
        {
            Console.WriteLine();
            //Console.Write("OrigClOrdID? ");
            Console.Write("Ориг. уникальный пользовательский идентификатор заявки? ");
            return new OrigClOrdID(Console.ReadLine().Trim());
        }
        private Symbol QuerySymbol()
        {
            Console.WriteLine();            
            Console.Write("Идентификатор/Символ? ");
            return new Symbol(Console.ReadLine().Trim());
        }
        private Side QuerySide()
        {
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
            Console.Write("Цена за кол-во? ");
            return new StopPx(Convert.ToDecimal(Console.ReadLine().Trim()));
        }
        #endregion
    }
}



