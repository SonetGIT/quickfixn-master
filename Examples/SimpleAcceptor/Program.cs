using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickFix;
using QuickFix.Transport;

namespace SimpleAcceptor
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //Console.WriteLine("=============");
            //Console.WriteLine("This is only an example program.");
            //Console.WriteLine("This is only an example program.");
            //Console.WriteLine("It's a simple server (e.g. Acceptor) app that will let clients (e.g. Initiators)");
            //Console.WriteLine("connect to it.  It will accept and display any application-level messages that it receives.");
            //Console.WriteLine("Connecting clients should set TargetCompID to 'SIMPLE' and SenderCompID to 'CLIENT1' or 'CLIENT2'.");
            //Console.WriteLine("Port is 5001.");
            //Console.WriteLine("(see simpleacc.cfg for configuration details)");
            //Console.WriteLine("=============");

            Console.WriteLine("=============");
            //Console.WriteLine("Это простое серверное (например, Acceptor) приложение, которое позволит клиентам (например, инициаторам)");
            Console.WriteLine("Запущен серверное приложение, которое позволит клиентам подключиться к нему.");
            Console.WriteLine("Он будет принимать и отображать все полученные сообщения уровня приложения.");
            Console.WriteLine("При подключении клиентов необходимо установить для TargetCompID (Идентификатор получателя) значение «1» и для SenderCompID (Идентификатор отправителя) значение «9db19bf0-6070-416c-9529-44b246ac4df4».");
            Console.WriteLine("Порт - 5001");
            Console.WriteLine("(Подробности конфигурации см. в simpleacc.cfg, tradeclient.cfg)");
            Console.WriteLine("=============");
            /*if (args.Length != 1)
            {
                Console.WriteLine("usage: SimpleAcceptor CONFIG_FILENAME");
                System.Environment.Exit(2);
            }*/

            try
            {
                SessionSettings settings = new SessionSettings("simpleacc.cfg"/*args[0]*/);
                IApplication app = new SimpleAcceptorApp();
                IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
                ILogFactory logFactory = new FileLogFactory(settings);
                IAcceptor acceptor = new ThreadedSocketAcceptor(app, storeFactory, settings, logFactory);                

                acceptor.Start();
                Console.WriteLine("Нажмите <enter>, чтобы выйти");
                Console.Read();
                acceptor.Stop();
            }
            catch (System.Exception e)
            {
                Console.WriteLine("==Критическая ошибка==");
                Console.WriteLine(e.ToString());
            }
        }
    }
}
