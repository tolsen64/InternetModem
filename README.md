# InternetModem
## Telnet Server / Modem Emulator for Windows

Download latest release [here](https://github.com/tolsen64/InternetModem/raw/master/LatestBuild.zip).
Donate [here](https://www.paypal.me/tolsen64)

![screenshot1](https://github.com/tolsen64/InternetModem/blob/master/Images/InetModemScreenShot.JPG?raw=true)

* Completely configurable via included [ini](https://github.com/tolsen64/InternetModem/blob/master/InternetModem/bin/InternetModem.ini) file.

* Listens for incoming telnet connections and connects to the serial port.

* Emulates a standard Hayes modem and accepts AT commands from the serial port.

* You configure the TCP port, COM port, BAUD rate, Handshake, Start/Stop/Data bits, etc.

* Informs callers that the BBS is busy if another connection is already active.

* Allows you to make outgoing telnet calls via the ATDT command (ATDT &lt;ip or addr&gt;[:port])

* Handles the telnet protocol transparently. You can connect to a telnet server from a non-compliant terminal.

* IRC compatible. InternetModem goes into IRC mode, making itself an IRC gateway for your terminal when you connect to a port defined as an IRC port in the ini file.

* Translates to/from a non-ascii character set by use of the translations.txt file.

* The AT command parser accepts multiple command and ignores those it doesn't need.

* You turn on various debug options in the ini file. KeepLog, ShowModemCommands, and ShowResultCodes.

|  Command  | Description |
|------------|-------------|
|ATDT &lt;ip or addr&gt;[:port]|Establishes a TCP connection to the specified host on the specified port. If port is not specified, port 23 is assumed.|
|ATE|Echo mode off (ATE0) or on (ATE1, the default)|
|ATH|Disconnects the network session|
|ATI|Displays modem version information|
|ATQ|Result codes on (ATQ0, the default) or off (ATQ1)|
|ATS0|ATS0=0 will cause the program to send RING to the serial port when it receives a network connection request. The connected serial device (BBS) must reply with ATA before the program sends the CONNECT message.<br><br>ATS0=1 will cause the program to automatically accept network connections (the default) and send the CONNECT code to the serial port.|
|ATV|Numeric codes (ATV0, the default) or text codes (ATV1)|
|AT&V|Displays current modem settings|
|A/|Repeat last AT command|
|ATD|Redial last system dialed|

My Apple II running ProTERM 3.1, chatting on irc.a2central.com:6667
![screenshot1](https://github.com/tolsen64/InternetModem/blob/master/Images/InetModemScreenA2C.JPG?raw=true)

View original project website [here](http://boycot.no-ip.com/InternetModem)

![Hit Counter](http://boycot.no-ip.com/HitCounter/default.aspx?id=GitHub/InternetModem "My Stupid Hit Counter!")
