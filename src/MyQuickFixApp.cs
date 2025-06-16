using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX50SP1;

namespace NgXQuickFix
{
    public interface IMyQuickFixApp : IApplication
    {
        public void Run();
    }
    public class MyQuickFixApp : MessageCracker, IMyQuickFixApp
    {
        private readonly string _username = string.Empty;
        private readonly string _password = string.Empty;
        private SessionID _sessionId;
        public MyQuickFixApp(string username, string password)
        {
            _username = username;
            _password = password;
        }
        public void FromAdmin(QuickFix.FIX50SP1.Message message, SessionID sessionID)
        {
            throw new NotImplementedException();
        }

        public void FromApp(QuickFix.FIX50SP1.Message message, SessionID sessionID)
        {
            try
            {
                Crack(message, sessionID);
            }
            catch (UnsupportedMessageType e)
            {
                Console.WriteLine("Unsupported message: " + e.Message);
            }
            catch (FieldNotFoundException) { }
            catch (IncorrectDataFormat) { }
            catch (IncorrectTagValue) { }
            catch (Exception ex)
            {
                Console.WriteLine("Error in FromApp: " + ex.Message);
            }
        }
        public void Run()
        {
            //we listen from the console....

            while (true)
            {
                Console.WriteLine("welcome initiator running");
                try
                {
                    byte action = QueryAction();
                    switch (action)
                    {
                        case 1:
                            QueryEnterOrder();
                            break;

                        case 2:
                            QueryCancelOrder();
                            break;
                        case 3:
                            QueryReplaceOrder();
                            break;
                        case 4:
                            QueryMarketDataRequest();
                            break;
                        case 5:
                            Console.WriteLine("Exiting...");
                            break;
                        default:
                            throw new Exception("Invalid action");
                    }

                }
                catch (Exception ex)
                {

                    Console.WriteLine("Error while processing initiator command >>> {0}", ex.Message);
                }
            }
        }

        private void QueryMarketDataRequest()
        {
            throw new NotImplementedException();
        }

        private void QueryReplaceOrder()
        {
            throw new NotImplementedException();
        }

        private void QueryCancelOrder()
        {
            byte version = QueryVersion();
            Console.WriteLine("\nOrderCancelRequest");
            var order = version switch
            {
                //43 => QueryOrderCancelRequest43(),
                50 => QueryOrderCancelRequest50(),
                _ => throw new ArgumentOutOfRangeException(nameof(version), "Invalid version detected..."),
            };

            if (QueryConfirm("Send order")) Session.SendToTarget(order);
        }

        private QuickFix.Message QueryOrderCancelRequest50()
        {
            TransactTime transactTime = new TransactTime();
            QuickFix.FIX50SP1.OrderCancelRequest request = new QuickFix.FIX50SP1.OrderCancelRequest(QueryClOrdID(), QuerySide(), transactTime);
            request.Set(QuerySymbol());
            request.Set(QueryOrderQty());
            QueryHeader(request.Header);
            return request;
        }

        //private Message QueryOrderCancelRequest43()
        //{
        //    TransactTime transactTime = new TransactTime();
        //    FIX43.OrderCancelRequest request = new FIX43.OrderCancelRequest(QueryOrigClOrdID(), QueryClOrdID(), QuerySymbol(), QuerySide(), transactTime);
        //    request.Set(QueryOrderQty());
        //    QueryHeader(request.Header);
        //    return request;
        //}

        private void QueryEnterOrder()
        {
            byte version = QueryVersion();
            Console.WriteLine("\nNewOrderSingle");
            var order = version switch
            {
                //43 => QueryNewOrderSingle43(),
                50 => QueryNewOrderSingle50(),
                _ => throw new ArgumentOutOfRangeException(nameof(version), "Invalid version detected..."),
            };

            if (QueryConfirm("Send order")) Session.SendToTarget(order);
        }
        //private FIX43.NewOrderSingle QueryNewOrderSingle43()
        //{
        //    ClOrdID clOrdID = QueryClOrdID();
        //    HandlInst handlInst = new HandlInst('1');
        //    Side side = QuerySide();
        //    TransactTime transactTime = new TransactTime();
        //    OrdType ordType = QueryOrdType();
        //    Symbol symbol = QuerySymbol();
        //    FIX43.NewOrderSingle newOrder = new FIX43.NewOrderSingle(clOrdID, handlInst, symbol, side, transactTime, ordType);
        //    newOrder.Set(QueryOrderQty());
        //    newOrder.Set(QueryTimeInForce());

        //    if (ordType.Value == OrdType.LIMIT || ordType.Value == OrdType.STOP_LIMIT)
        //        newOrder.Set(QueryPrice());

        //    if (ordType.Value == OrdType.STOP || ordType.Value == OrdType.STOP_LIMIT)
        //        newOrder.Set(QueryStopPx());

        //    QueryHeader(newOrder.Header);

        //    return newOrder;
        //}
        private NewOrderSingle QueryNewOrderSingle50()
        {
            ClOrdID clOrdID = QueryClOrdID();
            HandlInst handlInst = new HandlInst('1');
            Side side = QuerySide();
            TransactTime transactTime = new TransactTime();
            OrdType ordType = QueryOrdType();
            Symbol symbol = QuerySymbol();
            NewOrderSingle newOrder = new NewOrderSingle(clOrdID, side, transactTime, ordType);
            newOrder.Set(handlInst);
            newOrder.Set(symbol);
            newOrder.Set(QueryOrderQty());
            newOrder.Set(QueryTimeInForce());

            if (ordType.Value == OrdType.LIMIT || ordType.Value == OrdType.STOP_LIMIT)
                newOrder.Set(QueryPrice());

            if (ordType.Value == OrdType.STOP || ordType.Value == OrdType.STOP_LIMIT)
                newOrder.Set(QueryStopPx());

            QueryHeader(newOrder.Header);

            return newOrder;
        }
        private void QueryHeader(Header header)
        {
            header.SetField(QuerySenderCompID());
            header.SetField(QueryTargetCompID());
            if (QueryConfirm("Use TargetSubID")) header.SetField(QueryTargetSubID());
        }
        public bool SendMessage(QuickFix.FIX50SP1.Message message)
        {
            if (_sessionId == null) return false;
            return Session.SendToTarget(message, _sessionId);
        }

        private byte QueryVersion()
        {
            Console.WriteLine("1) FIX.4.0\n2) FIX.4.1\n3) FIX.4.2\n4) FIX.4.3\n5) FIX.4.5\n6) FIXT.1.1 (FIX.5.0)");
            Console.WriteLine("Enter option: ");
            if (!int.TryParse(Console.ReadLine(), out var option))
                throw new ArgumentException("Input was not a valid number.");
            return option switch
            {
                1 => 40,
                2 => 41,
                3 => 42,
                4 => 43,
                5 => 44,
                6 => 50,
                _ => throw new ArgumentOutOfRangeException(nameof(option), "invalid option selected")
            };
        }
        private byte QueryAction()
        {
            Console.WriteLine();
            Console.WriteLine("1) Enter Order");
            Console.WriteLine("2) Cancel Order");
            Console.WriteLine("3) Replace Order");
            Console.WriteLine("4) Market data test");
            Console.WriteLine("5) Quit");
            Console.Write("Action: ");

            if (!byte.TryParse(Console.ReadLine(), out var option))
                throw new ArgumentException("Input was not a valid number.");
            if (option is >= 1 and <= 5)
                return option;

            throw new ArgumentOutOfRangeException(nameof(option), "invalid option selected choose 1-5");

        }
        private static bool QueryConfirm(string query)
        {
            Console.WriteLine("\n{0}?: ", query);
            return Console.ReadLine()?.ToUpper() == "Y";
        }
        private SenderCompID QuerySenderCompID()
        {
            Console.WriteLine("\nSenderCompID: ");
            string value = Console.ReadLine();
            return new SenderCompID(value);
        }
        private TargetCompID QueryTargetCompID()
        {
            Console.WriteLine("\nTargetCompID: ");
            string value = Console.ReadLine();
            return new TargetCompID(value);
        }
        private TargetSubID QueryTargetSubID()
        {
            Console.WriteLine("\nTargetSubID: ");
            string value = Console.ReadLine();
            return new TargetSubID(value);
        }
        private ClOrdID QueryClOrdID()
        {
            Console.WriteLine("\nClOrdID: ");
            string value = Console.ReadLine();
            return new ClOrdID(value);
        }
        private OrigClOrdID QueryOrigClOrdID()
        {
            Console.WriteLine("\nOrigClOrdID: ");
            string value = Console.ReadLine();
            return new OrigClOrdID(value);
        }
        private Symbol QuerySymbol()
        {
            Console.WriteLine("\nSymbol: ");
            string value = Console.ReadLine();
            return new Symbol(value);
        }
        private Side QuerySide()
        {
            Console.WriteLine();
            Console.WriteLine("1) Buy");
            Console.WriteLine("2) Sell");
            Console.WriteLine("3) Sell Short");
            Console.WriteLine("4) Sell Short Exempt");
            Console.WriteLine("5) Cross");
            Console.WriteLine("6) Cross Short");
            Console.WriteLine("7) Cross Short Exempt");
            Console.Write("Side: ");

            if (!int.TryParse(Console.ReadLine(), out var option))
                throw new ArgumentException("Input was not a valid number.");
            return option switch
            {
                1 => new Side(Side.BUY),
                2 => new Side(Side.SELL),
                3 => new Side(Side.SELL_SHORT),
                4 => new Side(Side.SELL_SHORT_EXEMPT),
                5 => new Side(Side.CROSS),
                6 => new Side(Side.CROSS_SHORT),
                7 => new Side('A'),
                _ => throw new ArgumentOutOfRangeException(nameof(option), "invalid option selected")
            };
        }
        private OrderQty QueryOrderQty()
        {
            Console.WriteLine("\nOrderQty: ");
            if (!decimal.TryParse(Console.ReadLine(), out var value))
                throw new ArgumentException("Input was not a valid number.");
            return new OrderQty(value);
        }
        private OrdType QueryOrdType()
        {
            Console.WriteLine();
            Console.WriteLine("1) Market");
            Console.WriteLine("2) Limit");
            Console.WriteLine("3) Stop");
            Console.WriteLine("4) Stop Limit");
            Console.Write("OrdType: ");

            if (!int.TryParse(Console.ReadLine(), out var option))
                throw new ArgumentException("Input was not a valid number.");
            return option switch
            {
                1 => new OrdType(OrdType.MARKET),
                2 => new OrdType(OrdType.LIMIT),
                3 => new OrdType(OrdType.STOP),
                4 => new OrdType(OrdType.STOP_LIMIT),
                _ => throw new ArgumentOutOfRangeException(nameof(option), "invalid option selected")
            };
        }
        private Price QueryPrice()
        {
            Console.WriteLine("\nPrice: ");
            if (!decimal.TryParse(Console.ReadLine(), out var value))
                throw new ArgumentException("Input was not a valid number.");
            return new Price(value);
        }
        private StopPx QueryStopPx()
        {
            Console.WriteLine("\nStopPx: ");
            if (!decimal.TryParse(Console.ReadLine(), out var value))
                throw new ArgumentException("Input was not a valid number.");
            return new StopPx(value);
        }
        private TimeInForce QueryTimeInForce()
        {
            Console.WriteLine();
            Console.WriteLine("1) Day");
            Console.WriteLine("2) IOC");
            Console.WriteLine("3) OPG");
            Console.WriteLine("4) GTC");
            Console.WriteLine("5) GTX");
            Console.Write("TimeInForce: ");

            if (!int.TryParse(Console.ReadLine(), out var option))
                throw new ArgumentException("Input was not a valid number.");
            return option switch
            {
                1 => new TimeInForce(TimeInForce.DAY),
                2 => new TimeInForce(TimeInForce.IMMEDIATE_OR_CANCEL),
                3 => new TimeInForce(TimeInForce.AT_THE_OPENING),
                4 => new TimeInForce(TimeInForce.GOOD_TILL_CANCEL),
                5 => new TimeInForce(TimeInForce.GOOD_TILL_CROSSING),
                _ => throw new ArgumentOutOfRangeException(nameof(option), "invalid option selected")
            };
        }
        public void OnCreate(SessionID sessionID)
        {
            Console.WriteLine("Session created successfully! {0}", sessionID);
            _sessionId = sessionID;

        }

        public void OnLogon(SessionID sessionID)
        {
            Console.WriteLine("Logon successfully! {0}", sessionID);
            _sessionId = sessionID;
        }

        public void OnLogout(SessionID sessionID)
        {
            throw new NotImplementedException();
        }

        public void ToAdmin(QuickFix.Message message, SessionID sessionID)
        {
            if (message.Header.IsSetField(Tags.MsgType))
            {
                MsgType msgType = new MsgType();
                message.Header.GetField(msgType);

                if (msgType.Value == QuickFix.Fields.MsgType.LOGON)
                {
                    if (!string.IsNullOrWhiteSpace(_username) && !string.IsNullOrWhiteSpace(_username))
                    {

                        message.SetField(new Username(_username));
                        message.SetField(new Password(_password));
                        Console.WriteLine($"Added username and password to Logon message for session {sessionID}");
                    }

                }

            }
        }

        public void ToApp(QuickFix.Message message, SessionID sessionID)
        {
            try
            {
                //log the message we are trying to send to EXECUTOR...
                Console.WriteLine("Message to be sent to EXECUTOR >>> {0} with SessionID >>> {1}", message, sessionID);
                //throw new NotImplementedException();
                if (message.Header.IsSetField(Tags.PossDupFlag))
                {
                    PossDupFlag possDupFlag = new PossDupFlag();
                    message.Header.GetField(possDupFlag);

                    if (possDupFlag.Value == PossDupFlag.YES)
                    {
                        Console.WriteLine("This is a duplicate message...");
                        throw new DoNotSend();
                    }
                }
                Console.WriteLine("\nOUT: {0}", message);
            }
            catch (FileNotFoundException ex)
            {

                Console.WriteLine("Message file not found >>> {0}", ex.Message);
            }

        }

        public void FromAdmin(QuickFix.Message message, SessionID sessionID)
        {
            throw new NotImplementedException();
        }

        public void FromApp(QuickFix.Message message, SessionID sessionID)
        {
            throw new NotImplementedException();
        }
    }


}
