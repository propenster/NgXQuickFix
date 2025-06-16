
# NgXQuickFix - SelfService FIX Trading Client using Nigeria Stock Exchange (NGX) FIX5.0SP1 specification

**SelfService FIX Trading Client using Nigeria Stock Exchange (NGX) FIX5.0SP1 specification**

NgXQuickFix is a C++/C# FIX engine integration project aimed at enabling direct, programmable trading access to the Nigeria Stock Exchange (NGX) via the FIX protocol. Built on the robust [QuickFIX/N](https://github.com/connamara/quickfixn) engine, this project provides a foundation for automated or algorithmic trading systems and HFT (High-Frequency Trading) strategies tailored for the NGX ecosystem.


## 📈 Features

- ✅ Supports NGX FIX 5.0SP1 protocol specification  
- ✅ Self-service configuration and onboarding  
- ✅ Full FIX session lifecycle: login, heartbeat, trading messages  
- ✅ Secure authentication with `SenderCompID`, `Username`, and `Password`  
- ✅ Console-based QuickFIX/N client with modular architecture  
- ✅ Ready for simulation, testing, and live trading environments (with NGX test certs)


## 🔧 Technologies Used

- **Language**: C# (.NET)  
- **FIX Engine**: [QuickFIX/N](https://github.com/connamara/quickfixn)  
- **Configuration**: JSON (`appsettings.json`) + `.cfg` hybrid  
- **Transport**: TCP over FIX 5.0SP1  

## ⚙️ Getting Started

### Prerequisites

- .NET 6+ (for C# client)  
- Visual Studio / dotnet CLI  
- NGX certification test environment access  
- FIX protocol documentation for NGX (includes tag requirements)

### Clone & Setup

```bash
git clone https://github.com/propenster/NgXQuickFix.git
cd NgXQuickFix
```

# Configure

## 1. FIX Session Config (`tradeclient.cfg`)
** You get this from NGX or NGX-recognized licensed brokers **

<sub>Ini, TOML</sub>

```ini
[DEFAULT]
ConnectionType=initiator
ReconnectInterval=2
FileStorePath=store
FileLogPath=log
StartTime=01:00:00
EndTime=23:59:00
UseDataDictionary=Y
DataDictionary=FIX50SP1.xml
HttpAcceptPort=9911
#ClientCertificateFile=
#ClientCertificateKeyFile=
SSLProtocol = +SSLv3 +TLSv1 -SSLv2
TimestampPrecision=SECONDS
PreserveMessageFieldsOrder=N
DefaultApplVerID=FIX.5.0SP1

# standard config elements

[SESSION]
# inherit ConnectionType, ReconnectInterval and SenderCompID from default
BeginString=FIXT.1.1
SenderCompID=CLIENT1
TargetCompID=EXECUTOR
SocketConnectHost=127.0.0.1
SocketConnectPort=5001
SocketConnectHost1=127.0.0.1
SocketConnectPort1=5002
SocketConnectHost2=127.0.0.1
SocketConnectPort2=5003
HeartBtInt=30 

```

## 2. App Settings ('appsettings.json')
```json
{
  "FixClientConfig": {
    "Username": "username",
    "Password": "password",
    "CfgPath": "D:\\source\\repos\\NgXQuickFix\\tradeclient.cfg"
  }
}

```

## 2. Run the client
```bash

dotnet run --project NgXQuickFix

```

## 3. The Client is now CLI
**The NgXQuickFix tradeclient has been modified into a command line application with subcommands for order, marketdata etc**
### General Help
```bash
NgXQuickFix.exe -h
```
```bash
Description:
  NgXQuickFix FIX protocol trading client for Nigeria Stock Exchange

Usage:
  NgXQuickFix [command] [options]

Options:
  --fix-version <fix-version> (REQUIRED)  FIX protocol version (default: FIX.5.0SP1) e.g FIX.4.0 FIX.4.1 FIX.4.2 FIX.4.3 FIX.4.5 FIXT.1.1 (FIX.5.0) [default: FIX.5.0SP1]
  --version                               Show version information
  -?, -h, --help                          Show help and usage information

Commands:
  order        Order processing commands
  market-data  Get market data

```

### order help
```bash
NgXQuickFix.exe order -h
```
```bash
Description:
  Order processing commands

Usage:
  NgXQuickFix order [command] [options]

Options:
  --fix-version <fix-version> (REQUIRED)  FIX protocol version (default: FIX.5.0SP1) e.g FIX.4.0 FIX.4.1 FIX.4.2 FIX.4.3 FIX.4.5 FIXT.1.1 (FIX.5.0) [default: FIX.5.0SP1]
  -?, -h, --help                          Show help and usage information

Commands:
  create   create an order
  cancel   cancel an order
  replace  cancel replace an order
```

### order create help
```bash
NgXQuickFix.exe order create -h
```
```bash
Description:
  create an order

Usage:
  NgXQuickFix order create [options]

Options:
  --clord-id <clord-id> (REQUIRED)                                                                Client order ID
  --side <BUY|CROSS|CROSS_SHORT|CROSS_SHORT_EXEMPT|SELL|SELL_SHORT|SELL_SHORT_EXEMPT> (REQUIRED)  Order side: BUY, SELL, SELL_SHORT, SELL_SHORT_EXEMPT, CROSS, CROSS_SHORT, or CROSS_SHORT_EXEMPT.
  --symbol <symbol> (REQUIRED)                                                                    Trading symbol or ticker for the instrument (e.g., AIRTELAFRI, ZENITHBANK, MTNN, GTCO, DANGCEM).
  --ord-type <LIMIT|MARKET|STOP|STOP_LIMIT> (REQUIRED)                                            Order type: MARKET, LIMIT, STOP, or STOP_LIMIT.
  --ord-qty <ord-qty> (REQUIRED)                                                                  Quantity of the instrument to trade (e.g., 1000, 5000).
  --price <price>                                                                                 Price at which to execute the order (required for LIMIT and STOP_LIMIT orders).
  --time-in-force <DAY|GTC|GTX|IOC|OPG>                                                           Time in force: DAY, IOC (Immediate or Cancel), OPG (At the Opening), GTC (Good Till Cancel), or GTX (Good Till
                                                                                                  Crossing). [default: DAY]
  --stop-px <stop-px>                                                                             Price at which to stop the order (required for LIMIT and STOP_LIMIT orders).
  --use-target-sub-id                                                                             Whether to use/collect target subID [default: False]
  --sender-comp-id <sender-comp-id>                                                               Sender CompID
  --target-comp-id <target-comp-id>                                                               Target CompID
  --target-sub-id <target-sub-id>                                                                 Target SubID
  --fix-version <fix-version> (REQUIRED)                                                          FIX protocol version (default: FIX.5.0SP1) e.g FIX.4.0 FIX.4.1 FIX.4.2 FIX.4.3 FIX.4.5 FIXT.1.1 (FIX.5.0)
                                                                                                  [default: FIX.5.0SP1]
  -?, -h, --help                                                                                  Show help and usage information

```

### order cancel help
```bash
NgXQuickFix.exe order cancel -h
```
```bash
Description:
  cancel an order

Usage:
  NgXQuickFix order cancel [options]

Options:
  --clord-id <clord-id> (REQUIRED)                                                                Client order ID
  --side <BUY|CROSS|CROSS_SHORT|CROSS_SHORT_EXEMPT|SELL|SELL_SHORT|SELL_SHORT_EXEMPT> (REQUIRED)  Order side: BUY, SELL, SELL_SHORT, SELL_SHORT_EXEMPT, CROSS, CROSS_SHORT, or CROSS_SHORT_EXEMPT.
  --symbol <symbol> (REQUIRED)                                                                    Trading symbol or ticker for the instrument (e.g., AIRTELAFRI, MTNN, DANGCEM).
  --ord-qty <ord-qty> (REQUIRED)                                                                  Quantity of the instrument to trade (e.g., 1000, 5000).
  --fix-version <fix-version> (REQUIRED)                                                          FIX protocol version (default: FIX.5.0SP1) e.g FIX.4.0 FIX.4.1 FIX.4.2 FIX.4.3 FIX.4.5 FIXT.1.1 (FIX.5.0)
                                                                                                  [default: FIX.5.0SP1]
  -?, -h, --help                                                                                  Show help and usage information
```

### market data help
```bash
NgXQuickFix.exe market-data -h
```
```bash
Description:
  Get market data

Usage:
  NgXQuickFix market-data [options]

Options:
  --symbol <symbol> (REQUIRED)                                                      Trading symbol or ticker for the instrument (e.g., AIRTELAFRI, MTNN, DANGCEM).
  --subscription-type <DISABLE_PREVIOUS|SNAPSHOT|SNAPSHOT_PLUS_UPDATES> (REQUIRED)  Market data subscription type e.g SNAPSHOT [default: SNAPSHOT]
  --market-depth <market-depth>                                                     Market data depth [default: 0]
  --use-target-sub-id                                                               Whether to use/collect target subID [default: False]
  --sender-comp-id <sender-comp-id>                                                 Sender CompID
  --target-comp-id <target-comp-id>                                                 Target CompID
  --target-sub-id <target-sub-id>                                                   Target SubID
  --fix-version <fix-version> (REQUIRED)                                            FIX protocol version (default: FIX.5.0SP1) e.g FIX.4.0 FIX.4.1 FIX.4.2 FIX.4.3 FIX.4.5 FIXT.1.1 (FIX.5.0) [default:
                                                                                    FIX.5.0SP1]
  -?, -h, --help                                                                    Show help and usage information

```


## 📤 **Message Flow Supported**

- **Logon** (A)
- **NewOrderSingle** (D)
- **ExecutionReport** (8)
- **OrderCancelRequest** (F)
- Heartbeat, TestRequest, ResendRequest, etc.

## 📚 **Use Case: NGX Simulator**

- Connect to the NGX certification test server
- Submit limit/market orders
- Receive execution reports
- Validate message compliance and roundtrip
- Prepare for production certification

## 🛡️ **Disclaimer**

This project is for educational and integration purposes only. Trading on real markets involves financial risk. Always verify with NGX's official documentation and test environment before live deployment.