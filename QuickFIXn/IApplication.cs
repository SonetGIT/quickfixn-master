
namespace QuickFix
{
    // <summary>
    //(Renamed per naming convention.)
    // </summary>
    [System.Obsolete("Use IApplication instead.")]
    public interface Application : IApplication { }

    // <summary == резюме>
    /* Обратные вызовы в приложении QuickFIX / N уведомляют нас о событиях - когда контрагент входит в систему, 
      когда отправляются сообщения администратора и, что наиболее важно, когда сообщения приложения получены */
    // </summary>
    public interface IApplication
    {
        // <summary>
        /* через этот метод будет проходить каждое входящее сообщение уровня приложения, 
           такое как приказы, исполнения, определения безопасности и рыночные данные */
        // </summary>
        /// <param name="message"></param>
        /// <param name="sessionID"></param>
        /// <exception cref="FieldNotFoundException"> выводим исключения, чтобы уведомить контрагента об отсутствии обязательного поля </exception>
        /// <exception cref="UnsupportedMessageType"> выводим исключения, чтобы уведомить контрагента, что мы не можем обработать это сообщение </exception>
        /// <exception cref="IncorrectTagValue"> выводим исключения, чтобы уведомить контрагента о том, что поле содержит неверное значение </exception>
        void FromApp(Message message, SessionID sessionID);

        // <summary>
        /* этот метод вызывается всякий раз, когда создается новый сеанс */
        // </summary>
        /// <param name="sessionID"></param>
        void OnCreate(SessionID sessionID);

        // <summary>
        /* уведомляет, когда сеанс находится в автономном режиме - либо из-за обмена сообщениями о выходе из системы, либо из-за потери сетевого подключения */
        // </summary>
        /// <param name="sessionID"></param>
        void OnLogout(SessionID sessionID);

        // <summary>
        /* уведомляет об успешном входе в систему */
        // </summary>
        /// <param name="sessionID"></param>
        void OnLogon(SessionID sessionID);

        // <summary>
        /* каждое входящее сообщение уровня администратора будет проходить через этот метод, например контрольные сообщения, вход в систему и выход из системы */
        // </summary>
        /// <param name="message"></param>
        /// <param name="sessionID"></param>
        /// <exception cref="RejectLogon">throw this to reject a login</exception>
        void FromAdmin(Message message, SessionID sessionID);

        // <summary>
        /* все исходящие сообщения уровня администратора проходят через этот обратный вызов */
        // </summary>
        /// <param name="message"></param>
        /// <param name="sessionID"></param>
        void ToAdmin(Message message, SessionID sessionID);
        
        // <summary>
        /* все исходящие сообщения уровня приложения перед отправкой проходят через этот обратный вызов.
           Если тег нужно добавлять к каждому исходящему сообщению, это хорошее место для этого */
        // </summary>
        /// <param name="message"></param>
        /// <param name="sessionID"></param>
        /// <exception cref="DoNotSend">throw this to abort sending the message</exception>
        void ToApp(Message message, SessionID sessionID);
    }
}
