
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