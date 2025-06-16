using Microsoft.Extensions.Configuration;
using NgXQuickFix;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX50SP1;
using QuickFix.Logger;
using QuickFix.Store;
using QuickFix.Transport;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Drawing;
using System.Threading.Channels;
using Symbol = QuickFix.Fields.Symbol;

internal class Program
{
    public static IConfiguration Configuration { get; private set; }
    private static async Task<int> Main(string[] args)
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
        IMyQuickFixApp myQuickFixApp;
        SocketInitiator initiator = InitializeQuickFix();
        //initiator.Start();
        //myQuickFixApp.Run();
        //initiator.Stop();


        try
        {
            var rootCommand = new RootCommand("NgXQuickFix FIX protocol trading client for Nigeria Stock Exchange (NgX)");

            //var fixVersionOption = new Option<string>(
            //    name: "--version",
            //    description: "The FIX protocol version e.g FIX.4.0 FIX.4.1 FIX.4.2 FIX.4.3 FIX.4.5 FIXT.1.1 (FIX.5.0)"

            //    );
            var fixVersionOption = new Option<string>(
                new[] { "--fix-version" },
                description: "FIX protocol version (default: FIX.5.0SP1) e.g FIX.4.0 FIX.4.1 FIX.4.2 FIX.4.3 FIX.4.5 FIXT.1.1 (FIX.5.0)"
                )
            {
                IsRequired = true
            };
            fixVersionOption.SetDefaultValue("FIX.5.0SP1");
            rootCommand.AddGlobalOption(fixVersionOption);
            var orderCommand = new Command("order", "Order processing commands");
            rootCommand.AddCommand(orderCommand);


            var clOrdIdOption = new Option<string>(
                new[] { "--clord-id" },
                description: "Client order ID"
            )
            {
                IsRequired = true
            };

            var sideOption = new Option<string>(
                new[] { "--side" },
                description: "Order side: BUY, SELL, SELL_SHORT, SELL_SHORT_EXEMPT, CROSS, CROSS_SHORT, or CROSS_SHORT_EXEMPT."
            )
            {
                IsRequired = true
            };
            sideOption.AddCompletions("BUY", "SELL", "SELL_SHORT", "SELL_SHORT_EXEMPT", "CROSS", "CROSS_SHORT", "CROSS_SHORT_EXEMPT");

            var symbolOption = new Option<string>(
                name: "--symbol",
                description: "Trading symbol or ticker for the instrument (e.g., AIRTELAFRI, ZENITHBANK, MTNN, GTCO, DANGCEM)."
            )
            {
                IsRequired = true
            };

            var ordTypeOption = new Option<string>(
                name: "--ord-type",
                description: "Order type: MARKET, LIMIT, STOP, or STOP_LIMIT."
            )
            {
                IsRequired = true
            };
            ordTypeOption.AddCompletions("MARKET", "LIMIT", "STOP", "STOP_LIMIT");

            var ordQtyOption = new Option<decimal>(
                name: "--ord-qty",
                description: "Quantity of the instrument to trade (e.g., 1000, 5000)."
            )
            {
                IsRequired = true
            };

            var timeInForceOption = new Option<string>(
                name: "--time-in-force",
                description: "Time in force: DAY, IOC (Immediate or Cancel), OPG (At the Opening), GTC (Good Till Cancel), or GTX (Good Till Crossing).")
            {
                IsRequired = false
            };
            timeInForceOption.AddCompletions("DAY", "IOC", "OPG", "GTC", "GTX");
            timeInForceOption.SetDefaultValue("DAY");


            var priceOption = new Option<decimal?>(
                name: "--price",
                description: "Price at which to execute the order (required for LIMIT and STOP_LIMIT orders)."
            )
            {
                IsRequired = false
            };
            var stopPxOption = new Option<decimal?>(
               name: "--stop-px",
               description: "Price at which to stop the order (required for LIMIT and STOP_LIMIT orders)."
           )
            {
                IsRequired = false
            };
            var useTargetSubIDOption = new Option<bool>(
                new[] { "--use-target-sub-id" },
                description: "Whether to use/collect target subID")
            {
                IsRequired = false
            };
            useTargetSubIDOption.SetDefaultValue(false);

            var senderCompIdOption = new Option<string>(
                name: "--sender-comp-id",
                description: "Sender CompID")
            {
                IsRequired = false
            };
            var targetCompIdOption = new Option<string>(
                name: "--target-comp-id",
                description: "Target CompID")
            {
                IsRequired = false
            };
            var targetSubIdOption = new Option<string>(
                name: "--target-sub-id",
                description: "Target SubID")
            {
                IsRequired = false
            };

            var origClOrdIdOption = new Option<string>(
                name: "--orig-clord-id",
                description: "Original client order ID"
                )
            {
                IsRequired = false
            };

            var subscriptionTypeOption = new Option<string>(
                name: "--subscription-type",
                description: "Market data subscription type e.g SNAPSHOT"
                )
            {
                IsRequired = true
            };
            subscriptionTypeOption.SetDefaultValue("SNAPSHOT");
            subscriptionTypeOption.AddCompletions("SNAPSHOT", "SNAPSHOT_PLUS_UPDATES", "DISABLE_PREVIOUS");

            var marketDepthOption = new Option<int>(
                        name: "--market-depth",
                        description: "Market data depth"
                        )
            {
                IsRequired = false
            };
            marketDepthOption.SetDefaultValue(0);

           











            var orderCreateCommand = new Command("create", "create an order")
                {
                clOrdIdOption,
                sideOption,
                symbolOption,
                ordTypeOption,
                ordQtyOption,
                priceOption,
                timeInForceOption,
                stopPxOption,

                useTargetSubIDOption,
                senderCompIdOption,
                targetCompIdOption,
                targetSubIdOption
            };
            orderCommand.AddCommand(orderCreateCommand);
            var orderCancelCommand = new Command("cancel", "cancel an order")
            {
                clOrdIdOption,
                sideOption,
                symbolOption,
                ordQtyOption,

            };
            orderCommand.AddCommand(orderCancelCommand);

            var orderReplaceCommand = new Command("replace", "cancel replace an order")
            {
                origClOrdIdOption,
                clOrdIdOption,
                sideOption,
                symbolOption,
                ordQtyOption,
            };
            orderCommand.AddCommand(orderReplaceCommand);



            orderCreateCommand.SetHandler(async (context) =>
            {
                var fixVersion = context.ParseResult.GetValueForOption(fixVersionOption);
                var clOrdId = context.ParseResult.GetValueForOption(clOrdIdOption);
                var side = context.ParseResult.GetValueForOption(sideOption);
                var symbol = context.ParseResult.GetValueForOption(symbolOption);
                var ordType = context.ParseResult.GetValueForOption(ordTypeOption);
                var ordQty = context.ParseResult.GetValueForOption(ordQtyOption);
                var timeInForce = context.ParseResult.GetValueForOption(timeInForceOption);
                var price = context.ParseResult.GetValueForOption(priceOption);
                var stopPx = context.ParseResult.GetValueForOption(stopPxOption);

                var useTargetSubId = context.ParseResult.GetValueForOption(useTargetSubIDOption);
                var senderCompId = context.ParseResult.GetValueForOption(senderCompIdOption);
                var targetCompId = context.ParseResult.GetValueForOption(targetCompIdOption);
                var targetSubId = context.ParseResult.GetValueForOption(targetSubIdOption);


                await ProcessOrderCreateAsync(fixVersion, clOrdId, side, symbol, ordType, ordQty, timeInForce, price, stopPx, useTargetSubId, senderCompId, targetCompId, targetSubId);
            });
            orderCancelCommand.SetHandler(async (context) =>
            {
                var fixVersion = context.ParseResult.GetValueForOption(fixVersionOption);
                var clOrdId = context.ParseResult.GetValueForOption(clOrdIdOption);
                var side = context.ParseResult.GetValueForOption(sideOption);
                var symbol = context.ParseResult.GetValueForOption(symbolOption);
                var ordQty = context.ParseResult.GetValueForOption(ordQtyOption);

                var useTargetSubId = context.ParseResult.GetValueForOption(useTargetSubIDOption);
                var senderCompId = context.ParseResult.GetValueForOption(senderCompIdOption);
                var targetCompId = context.ParseResult.GetValueForOption(targetCompIdOption);
                var targetSubId = context.ParseResult.GetValueForOption(targetSubIdOption);

                await ProcessOrderCancelAsync(fixVersion, clOrdId, side, symbol, ordQty, useTargetSubId, senderCompId, targetCompId, targetSubId);

            });

            orderReplaceCommand.SetHandler(async (context) =>
            {
                var fixVersion = context.ParseResult.GetValueForOption(fixVersionOption);
                var origClOrdId = context.ParseResult.GetValueForOption(origClOrdIdOption);
                var clOrdId = context.ParseResult.GetValueForOption(clOrdIdOption);
                var side = context.ParseResult.GetValueForOption(sideOption);
                var symbol = context.ParseResult.GetValueForOption(symbolOption);
                var ordQty = context.ParseResult.GetValueForOption(ordQtyOption);
                var ordType = context.ParseResult.GetValueForOption(ordTypeOption);

                var price = context.ParseResult.GetValueForOption(priceOption);


                var useTargetSubId = context.ParseResult.GetValueForOption(useTargetSubIDOption);
                var senderCompId = context.ParseResult.GetValueForOption(senderCompIdOption);
                var targetCompId = context.ParseResult.GetValueForOption(targetCompIdOption);
                var targetSubId = context.ParseResult.GetValueForOption(targetSubIdOption);

                await ProcessOrderReplaceAsync(fixVersion, origClOrdId, clOrdId, side, symbol, ordQty, price, ordType, useTargetSubId, senderCompId, targetCompId, targetSubId);

            });


            var marketDataCommand = new Command("market-data", "Get market data")
            {
                symbolOption,
                subscriptionTypeOption,
                marketDepthOption,

                useTargetSubIDOption,
                senderCompIdOption,
                targetCompIdOption,
                targetSubIdOption

            };
            marketDataCommand.SetHandler(async (context) =>
            {
                var symbol = context.ParseResult.GetValueForOption(symbolOption);
                var subscriptionType = context.ParseResult.GetValueForOption(subscriptionTypeOption);
                var marketDepth = context.ParseResult.GetValueForOption(marketDepthOption);

                var useTargetSubId = context.ParseResult.GetValueForOption(useTargetSubIDOption);
                var senderCompId = context.ParseResult.GetValueForOption(senderCompIdOption);
                var targetCompId = context.ParseResult.GetValueForOption(targetCompIdOption);
                var targetSubId = context.ParseResult.GetValueForOption(targetSubIdOption);

                await ProcessMarketDataAsync(symbol, subscriptionType, marketDepth, useTargetSubId, senderCompId, targetCompId, targetSubId);
            });
            rootCommand.AddCommand(marketDataCommand);

            return rootCommand.InvokeAsync(args).Result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while running NgXQuickFixClient >>> {0}", ex.Message);
        }
        return -1;
    }
    private static SubscriptionRequestType GetSubscriptionRequestType(string subscriptionType) => subscriptionType.ToUpper() switch
    {
        "SNAPSHOT" => new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT),
        "SNAPSHOT_PLUS_UPDATES" => new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES),
        "DISABLE_PREVIOUS" => new SubscriptionRequestType(SubscriptionRequestType.DISABLE_PREVIOUS),
        _ => throw new ArgumentOutOfRangeException(nameof(subscriptionType), "invalid subscription type input")
    };
    private static async Task<bool> ProcessMarketDataAsync(string symbol, string subscriptionType, int? marketDepth, bool useTargetSubId, string senderCompId, string targetCompId, string targetSubId)
    {
        MDReqID mDReqID = new MDReqID(Guid.NewGuid().ToString().Replace("-", string.Empty));
        NoMDEntries noMDEntries = new NoMDEntries();
        MDEntryType mDEntryType = new MDEntryType(MDEntryType.BID);

        MarketDataRequest.NoMDEntryTypesGroup marketDataEntryGroup = new MarketDataRequest.NoMDEntryTypesGroup();
        marketDataEntryGroup.Set(mDEntryType);

        MarketDataRequest.NoRelatedSymGroup symbolGroup = new MarketDataRequest.NoRelatedSymGroup();
        Symbol fixSymbol = new Symbol(symbol);
        symbolGroup.Set(fixSymbol);

        MarketDataRequest message = new MarketDataRequest(mDReqID, GetSubscriptionRequestType(subscriptionType), new MarketDepth(marketDepth.GetValueOrDefault()));
        message.AddGroup(marketDataEntryGroup);
        message.AddGroup(symbolGroup);

        UpdateMessageHeaderForTargetSubId(useTargetSubId, senderCompId, targetSubId, message.Header);

        Console.WriteLine("Message from MarketData request in XML >>> {0}", message.ToXML());
        Console.WriteLine("Message from MarketData request in string >>> {0}", message.ToString());


        return true;
    }
    private static async Task<bool> ProcessOrderCancelAsync(string fixVersion, string clOrdId, string side, string symbol, decimal ordQty, bool useTargetSubId, string senderCompId, string targetCompId, string targetSubId)
    {
        TransactTime transactTime = new TransactTime();
        QuickFix.FIX50SP1.OrderCancelRequest request = new QuickFix.FIX50SP1.OrderCancelRequest(new ClOrdID(clOrdId), GetSide(side), transactTime);
        request.Set(new Symbol(symbol));
        request.Set(new OrderQty(ordQty));
        //TODO: extract this into a method...
        UpdateMessageHeaderForTargetSubId(useTargetSubId, senderCompId, targetSubId, request.Header);

        return true;
    }
    private static async Task<bool> ProcessOrderReplaceAsync(string fixVersion, string origClOrdId, string clOrdId, string side, string symbol, decimal ordQty, decimal? price, string ordType, bool useTargetSubId, string senderCompId, string targetCompId, string targetSubId)
    {
        TransactTime transactTime = new TransactTime();
        HandlInst handlInst = new HandlInst('1');
        OrderCancelReplaceRequest request = new OrderCancelReplaceRequest(new ClOrdID(origClOrdId), GetSide(side), transactTime, GetOrdType(ordType));
        request.Set(new Symbol(symbol));
        request.Set(new OrderQty(ordQty));
        request.Set(new OrigClOrdID(origClOrdId));
        request.Set(handlInst);
        request.Set(new Price(price.GetValueOrDefault()));
        //TODO: extract this into a method...
        UpdateMessageHeaderForTargetSubId(useTargetSubId, senderCompId, targetSubId, request.Header);

        return true;
    }
    private static Side GetSide(string side) => side.ToUpper() switch
    {
        "BUY" => new Side(Side.BUY),
        "SELL" => new Side(Side.SELL),
        "SELL_SHORT" => new Side(Side.SELL_SHORT),
        "SELL_SHORT_EXEMPT" => new Side(Side.SELL_SHORT_EXEMPT),
        "CROSS" => new Side(Side.CROSS),
        "CROSS_SHORT" => new Side(Side.CROSS_SHORT),
        "CROSS_SHORT_EXEMPT" => new Side(Side.CROSS_SHORT_EXEMPT),
        _ => throw new ArgumentOutOfRangeException(nameof(side), "invalid side input")

    };
    private static OrdType GetOrdType(string ordType) => ordType.ToUpper() switch
    {
        "MARKET" => new OrdType(OrdType.MARKET),
        "LIMIT" => new OrdType(OrdType.LIMIT),
        "STOP" => new OrdType(OrdType.STOP),
        "STOP_LIMIT" => new OrdType(OrdType.STOP_LIMIT),
        _ => throw new ArgumentOutOfRangeException(nameof(ordType), "invalid ordType input")
    };
    private static async Task<bool> ProcessOrderCreateAsync(string fixVersion, string clOrdId, string side, string symbol, string ordType, decimal ordQty, string timeInForce, decimal? price, decimal? stopPx, bool useTargetSubId, string senderCompId, string targetCompId, string targetSubId)
    {
        HandlInst handlInst = new HandlInst('1');
        var sideVal = GetSide(side);
        TransactTime transactTime = new TransactTime();
        OrdType ordTypeVal = GetOrdType(ordType);
        TimeInForce timeInForceVal = timeInForce.ToUpper() switch
        {
            "DAY" => new TimeInForce(TimeInForce.DAY),
            "IOC" => new TimeInForce(TimeInForce.IMMEDIATE_OR_CANCEL),
            "OPG" => new TimeInForce(TimeInForce.AT_THE_OPENING),
            "GTC" => new TimeInForce(TimeInForce.GOOD_TILL_CANCEL),
            "GTX" => new TimeInForce(TimeInForce.GOOD_TILL_CROSSING),
            _ => throw new ArgumentOutOfRangeException(nameof(timeInForce), "invalid timeInForce input")
        };
        Symbol symbolVal = new Symbol(symbol);
        NewOrderSingle newOrder = new NewOrderSingle(new ClOrdID(clOrdId), sideVal, transactTime, ordTypeVal);
        newOrder.Set(handlInst);
        newOrder.Set(symbolVal);
        newOrder.Set(new OrderQty(ordQty));
        newOrder.Set(timeInForceVal);

        if (ordTypeVal.Value == OrdType.LIMIT || ordTypeVal.Value == OrdType.STOP_LIMIT)
            newOrder.Set(new Price(price.GetValueOrDefault()));

        if (ordTypeVal.Value == OrdType.STOP || ordTypeVal.Value == OrdType.STOP_LIMIT)
            newOrder.Set(new StopPx(stopPx.GetValueOrDefault()));
        //TODO: extract this into a method...
        //even header is too big to casually fit into stack frame of the UpdateMessageHeader method... i'll find a better way later...
        UpdateMessageHeaderForTargetSubId(useTargetSubId, senderCompId, targetSubId, newOrder.Header);

        using (var initiator = InitializeQuickFix())
        {
            initiator.Start();
            
            var success = Session.SendToTarget(newOrder);
            if (!success)
            {
                Console.WriteLine("Failed to send order - no matching session");
                return false;
            }

            Console.WriteLine($"Order sent: ClOrdID={newOrder.ClOrdID.Value}");

            return true;
        }
    }

    private static void UpdateMessageHeaderForTargetSubId(bool useTargetSubId, string senderCompId, string targetSubId, Header header)
    {
        header.SetField(new SenderCompID(senderCompId));
        header.SetField(new TargetCompID(senderCompId));

        if (useTargetSubId && !string.IsNullOrWhiteSpace(targetSubId)) header.SetField(new TargetSubID(targetSubId));
    }
    private static SocketInitiator InitializeQuickFix()
    {
        var fixSettingsPath = Configuration["FixClientConfig:CfgPath"] ?? string.Empty;
        var username = Configuration["FixClientConfig:Username"];
        var password = Configuration["FixClientConfig:Password"];
        SessionSettings settings = new SessionSettings(fixSettingsPath);
        IMyQuickFixApp myQuickFixApp = new MyQuickFixApp(username, password);
        IMessageStoreFactory factory = new FileStoreFactory(settings);
        ILogFactory logger = new FileLogFactory(settings);
        return new SocketInitiator(myQuickFixApp, factory, settings, logger);
    }
}