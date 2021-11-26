// This is a generated file.  Don't edit it directly!

using QuickFix.Fields;
namespace QuickFix
{
    namespace FIX44 
    {
        public class Heartbeat : Message
        {
            /* ѕередаЄтс€ периодически дл€ контрол€ состо€ни€ соединени€.≈сли Heartbeat посылаетс€ в ответ на Test Request сообщение, то
                поле TestReqID должно содержать идентификатор Test Request, на который оно €вл€етс€ ответом. */
            public const string MsgType = "0";

            public Heartbeat() : base()
            {
                this.Header.SetField(new QuickFix.Fields.MsgType("0"));
            }


            public QuickFix.Fields.TestReqID TestReqID
            { 
                get 
                {
                    QuickFix.Fields.TestReqID val = new QuickFix.Fields.TestReqID();
                    GetField(val);
                    return val;
                }
                set { SetField(value); }
            }
            
            public void Set(QuickFix.Fields.TestReqID val) 
            { 
                this.TestReqID = val;
            }
            
            public QuickFix.Fields.TestReqID Get(QuickFix.Fields.TestReqID val) 
            { 
                GetField(val);
                return val;
            }
            
            public bool IsSet(QuickFix.Fields.TestReqID val) 
            { 
                return IsSetTestReqID();
            }
            
            public bool IsSetTestReqID() 
            { 
                return IsSetField(Tags.TestReqID);
            }
        }
    }
}
