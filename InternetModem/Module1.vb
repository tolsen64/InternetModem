'***************************************************
'* This code belongs to Terry R. Olsen
'* You are free to use this code in whole or
'* in part for your own programs. But you may
'* not use this code in any way to create
'* the same type of program for public release.
'* My Email: tolsen64@hotmail.com
'***************************************************

Imports System.Net
Imports System.Net.Sockets
Imports System.Windows.forms
Imports System.Text.ASCIIEncoding
Imports System.IO
Imports System.IO.Ports

Module Module1

#Region "Telnet Commands"
    ' Telnet main commands  -  list copied from somewhere on the net - thank you to them
    Const TN_IAC As Int32 = 255   ' Interpret as command escape sequence, Prefix to all telnet commands. 1, 2 or sometimes more commands normally follow this character
    Const TN_DONT As Int32 = 254  ' You are not to use this option
    Const TN_DO As Int32 = 253    ' Please, you use this option
    Const TN_WONT As Int32 = 252  ' I won't use option
    Const TN_WILL As Int32 = 251  ' I will use option
    Const TN_SB As Int32 = 250    ' Subnegotiate, X number of commands follow
    Const TN_GA As Int32 = 249    ' Go ahead
    Const TN_EL As Int32 = 248    ' Erase line
    Const TN_EC As Int32 = 247    ' Erase character
    Const TN_AYT As Int32 = 246   ' Are you there
    Const TN_AO As Int32 = 245    ' Abort output
    Const TN_IP As Int32 = 244    ' Interrupt process
    Const TN_BRK As Int32 = 243   ' Break
    Const TN_DM As Int32 = 242    ' Data mark
    Const TN_NOP As Int32 = 241   ' No operation.
    Const TN_SE As Int32 = 240    ' End of subnegotiation, from above
    Const TN_EOR As Int32 = 239   ' End of record
    Const TN_ABORT As Int32 = 238 ' About process
    Const TN_SUSP As Int32 = 237  ' Suspend process
    Const TO_EOF As Int32 = 236  ' End of file

    ' Telnet (option) mainly return commands from above
    Const TN_BIN As Int32 = 0     ' Binary transmission
    Const TN_ECHO As Int32 = 1    ' Echo
    Const TN_RECN As Int32 = 2    ' Reconnection
    Const TN_SUPP As Int32 = 3    ' Suppress go ahead
    Const TN_APRX As Int32 = 4    ' Approx message size negotiation
    Const TN_STAT As Int32 = 5    ' Status
    Const TN_TIM As Int32 = 6     ' Timing mark
    Const TN_REM As Int32 = 7     ' Remote controlled trans/echo
    Const TN_OLW As Int32 = 8     ' Output line width
    Const TN_OPS As Int32 = 9     ' Output page size
    Const TN_OCRD As Int32 = 10   ' Out carriage-return disposition
    Const TN_OHT As Int32 = 11    ' Output horizontal tabstops
    Const TN_OHTD As Int32 = 12   ' Out horizontal tab disposition
    Const TN_OFD As Int32 = 13    ' Output formfeed disposition
    Const TN_OVT As Int32 = 14    ' Output vertical tabstops
    Const TN_OVTD As Int32 = 15   ' Output vertical tab disposition
    Const TN_OLD As Int32 = 16    ' Output linefeed disposition
    Const TN_EXT As Int32 = 17    ' Extended ascii character set
    Const TN_LOGO As Int32 = 18   ' Logout
    Const TN_BYTE As Int32 = 19   ' Byte macro
    Const TN_DATA As Int32 = 20   ' Data entry terminal
    Const TN_SUP As Int32 = 21    ' supdup protocol
    Const TN_SUPO As Int32 = 22   ' supdup output
    Const TN_SNDL As Int32 = 23   ' Send location
    Const TN_TERM As Int32 = 24   ' Terminal type
    Const TO_EOR As Int32 = 25    ' End of record
    Const TN_TACACS As Int32 = 26 ' Tacacs user identification
    Const TN_OM As Int32 = 27     ' Output marking
    Const TN_TLN As Int32 = 28    ' Terminal location number
    Const TN_3270 As Int32 = 29   ' Telnet 3270 regime
    Const TN_X3 As Int32 = 30     ' X.3 PAD
    Const TN_NAWS As Int32 = 31   ' Negotiate about window size
    Const TN_TS As Int32 = 32     ' Terminal speed
    Const TN_RFC As Int32 = 33    ' Remote flow control
    Const TN_LINE As Int32 = 34   ' Linemode
    Const TN_XDL As Int32 = 35    ' X display location
    Const TN_ENVIR As Int32 = 36  ' Telnet environment option
    Const TN_AUTH As Int32 = 37   ' Telnet authentication option
    Const TN_NENVIR As Int32 = 39 ' Telnet environment option
    Const TN_EXTOP As Int32 = 25  ' Extended-options-list
#End Region

    Dim ver As String = My.Application.Info.Version.ToString

    'Telnet Stuff
    Dim LineSize As Integer

    'IRC Stuff
    Dim IrcMode As Boolean = False
    Dim IrcInTxt As String = ""
    Dim IrcOutTxt As String = ""
    Dim IrcShowTimeStamp As Boolean = True
    Dim IrcUid As String = ""
    Dim IrcNick As String = ""
    Dim IrcPwd As String = ""
    Dim IrcBlockPrivMsgs As Boolean = False
    Dim IrcUserInfo As String = ""
    Dim IrcRoomName As String = ""
    Dim IrcShowRawInIrc As Boolean = False
    Dim IrcShowRawOutIrc As Boolean = False
    Dim IrcPorts As String = ""

    'Serial Port Stuff
    Dim WithEvents Ser As New SerialPort

    'Terminal Stuff
    Dim LineLength As Integer = 80
    Dim ContIndent As Integer = 4

    'Telnet Stuff
    Dim TcpListen As Boolean = True
    Dim TcpPort As Integer = 23
    Dim TcpSvr As Socket    'TCP Listener Socket, listens for incoming calls
    Dim TcpClt As Socket    'TCP Client Socket, handles incoming calls
    Dim TcpMsg As Socket    'Tells Clients that we're busy right now...
    'Dim TcpDial As Socket   'This is the dial out socket, for calling other BBS's

    'Send & Receive Buffers
    Dim RcvBuf() As Byte    'In from TCP/Out to Serial
    Dim XmtBuf() As Byte    'In from Serial/Out to TCP
    Dim SerByt As Integer   'Bytes to read from serial port
    Dim Throttle As Integer 'Some slower terminals may need to slow it down

    'BBS not available message
    Dim msg() As Byte = ASCII.GetBytes("The line is busy. Please try again later.")

    'Stopwatch for measuring +++ Guard Time
    Dim swGuardTimer As Timers.Timer

    'Application variables
    Dim logFile As String
    Dim iniFile As String
    Dim keepLog As Boolean = True

    'Modem Emulator Stuff
    Enum ModemEcho
        EchoOff = 0
        EchoOn = 1
    End Enum
    Enum ModemResultCodeFormat
        Numeric = 0
        Text = 1
    End Enum
    Enum ModemResultCodes
        Enabled = 0
        Disabled = 1
    End Enum
    Enum ModemRingMode
        SendRing = 0
        SendConnect = 1
    End Enum
    Enum ModemHookState
        OnHook = 0
        OffHook = 1
    End Enum

    Const OK As Integer = 0
    Const CONNECT As Integer = 1
    Const RING As Integer = 2
    Const NO_CARRIER As Integer = 3
    Const ERR As Integer = 4
    Const NO_DIAL_TONE As Integer = 6
    Const BUSY As Integer = 7
    Const NO_ANSWER As Integer = 8

    Dim MdmErr As Integer
    Dim MdmErrorCode() As String = {"OK", "CONNECT", "RING", "NO CARRIER", "ERROR", "", "NO DIAL TONE", "BUSY", "NO ANSWER"}
    Dim MdmCmdMode As Boolean = True  'Are we in command mode?
    Dim MdmCmdStr As String
    Dim MdmResultCodeFormat As ModemResultCodeFormat = ModemResultCodeFormat.Numeric
    Dim MdmEcho As ModemEcho = ModemEcho.EchoOn
    Dim MdmResultCodes As ModemResultCodes = ModemResultCodes.Enabled
    Dim MdmRingMode As ModemRingMode = ModemRingMode.SendConnect
    Dim MdmHookState As ModemHookState = ModemHookState.OnHook
    Dim MdmBaud As String
    Dim DropCarrierTerminateString As String = ""

    Dim LastATCmd As String
    Dim LastDialed As String

    Dim ShowModemCommands As Boolean = False
    Dim ShowResultCodes As Boolean = False

    Dim ASCIICode(255) As Byte
    'Dim NonASCIICode(255) As Byte
    Dim UseTranslation As Boolean = False

    Sub Main()
        'Display Program & Author Information
        Console.WriteLine("Internet Modem v" & ver & " by Terry R. Olsen")
        'Console.WriteLine("Call The BoycoT BBS: telnet://boycotbbs.ddns.net:9999")
        Console.WriteLine("Get the latest version of this program: http://boycot.no-ip.com/InternetModem")
        Console.WriteLine("If you like, donate! https://www.paypal.me/tolsen64")
        Console.WriteLine("Email: tolsen64@hotmail.com")
        Console.WriteLine("=============================================================================")

        'Load the INI file. This defines our variables.
        WriteStatus("Loading Initialization Data")
        Dim tmpPth As String = My.Application.Info.DirectoryPath
        If Right(tmpPth, 1) <> "\" Then tmpPth += "\"
        iniFile = tmpPth + "InternetModem.ini"
        logFile = tmpPth + "InternetModem.log"
        Dim r As New StreamReader(iniFile)
        Dim tmp() As String
        While r.Peek > 0
            tmp = Split(r.ReadLine, "=")
            Select Case tmp(0)
                Case "TcpListen"
                    TcpListen = CBool(tmp(1))
                Case "TcpPort"
                    TcpPort = CInt(tmp(1))
                Case "SerPort"
                    Ser.PortName = "COM" & Trim(tmp(1))
                Case "SerBaud"
                    Ser.BaudRate = CInt(tmp(1))
                    MdmBaud = Trim(tmp(1))
                Case "SerDataBits"
                    Ser.DataBits = CInt(tmp(1))
                Case "SerParity"
                    If tmp(1) = "None" Then Ser.Parity = Parity.None
                    If tmp(1) = "Odd" Then Ser.Parity = Parity.Odd
                    If tmp(1) = "Even" Then Ser.Parity = Parity.Even
                    If tmp(1) = "Mark" Then Ser.Parity = Parity.Mark
                    If tmp(1) = "Space" Then Ser.Parity = Parity.Space
                Case "SerStopBits"
                    If tmp(1) = 1 Then Ser.StopBits = StopBits.One
                    If tmp(1) = 1.5 Then Ser.StopBits = StopBits.OnePointFive
                    If tmp(1) = 2 Then Ser.StopBits = StopBits.Two
                Case "Handshake"
                    If tmp(1) = "None" Then Ser.Handshake = Handshake.None
                    If tmp(1) = "RTS" Then Ser.Handshake = Handshake.RequestToSend
                    If tmp(1) = "RTS/XonXoff" Then Ser.Handshake = Handshake.RequestToSendXOnXOff
                    If tmp(1) = "XonXoff" Then Ser.Handshake = Handshake.XOnXOff
                Case "RcvBuf"
                    Ser.WriteBufferSize = CInt(tmp(1))
                    ReDim RcvBuf(CInt(tmp(1)))
                Case "XmtBuf"
                    Ser.ReadBufferSize = CInt(tmp(1))
                    ReDim XmtBuf(CInt(tmp(1)))
                Case "LineLength"
                    LineLength = CInt(tmp(1))
                Case "ContIndent"
                    ContIndent = CInt(tmp(1))
                Case "KeepLog"
                    keepLog = CBool(tmp(1))
                Case "BBSName"
                    msg = ASCII.GetBytes(tmp(1) & " is currently in use." & vbCrLf & vbCrLf &
                          "Please try again soon!" & vbCrLf & vbCrLf)
                    '"While you're waiting...give The BoycoT BBS a call..." & vbCrLf & _
                    '"telnet://boycot.no-ip.com:9999")
                Case "Throttle"
                    Throttle = CInt(tmp(1))
                Case "LineSize"
                    LineSize = CInt(tmp(1))
                Case "ShowModemCommands"
                    ShowModemCommands = CBool(tmp(1))
                Case "ShowResultCodes"
                    ShowResultCodes = CBool(tmp(1))
                Case "IrcShowTimeStamp"
                    IrcShowTimeStamp = CBool(tmp(1))
                Case "IrcUid"
                    IrcUid = tmp(1)
                Case "IrcNick"
                    IrcNick = tmp(1)
                Case "IrcPwd"
                    IrcPwd = tmp(1)
                Case "IrcBlockPrivMsgs"
                    IrcBlockPrivMsgs = CBool(tmp(1))
                Case "IrcUserInfo"
                    IrcUserInfo = tmp(1)
                Case "IrcRoomName"
                    IrcRoomName = tmp(1)
                Case "IrcShowRawInIrc"
                    IrcShowRawInIrc = CBool(tmp(1))
                Case "IrcShowRawOutIrc"
                    IrcShowRawOutIrc = CBool(tmp(1))
                Case "IrcPorts"
                    IrcPorts = tmp(1)
                Case "DropCarrierTerminateString"
                    DropCarrierTerminateString = tmp(1)
            End Select
        End While
        r.Close()

        'Check for Character Translation file and load if it exists
        'It will have this format:
        'ASCIICode=NonASCIICode
        If File.Exists(tmpPth & "Translations.txt") Then
            WriteStatus("Loading Character Code Translations")
            UseTranslation = True
            'Fill Translation Arrays with default data
            For i As Integer = 0 To 255
                ASCIICode(i) = i
            Next
            r = New StreamReader(tmpPth & "Translations.txt")
            While r.Peek > 0
                tmp = Split(r.ReadLine, "=")
                If tmp(0).StartsWith(";") = False Then
                    ASCIICode(tmp(0)) = tmp(1)
                    ASCIICode(tmp(1)) = tmp(0)
                End If
            End While
            r.Close()
        End If

        Ser.ReceivedBytesThreshold = 1   'Raise an event for each character received
        'Open the serial port
        WriteStatus("Opening " & Ser.PortName & "," & Ser.BaudRate & "," & Ser.DataBits &
                    "," & Ser.Parity.ToString & "," & Ser.StopBits & "," & Ser.Handshake.ToString)
        Try
            Ser.Open()
            Ser.DtrEnable = True
            Ser.RtsEnable = True
        Catch ex As Exception
            WriteStatus(ex.Message)
            WriteStatus("Failed!")
            Console.WriteLine("Press ENTER to exit.")
            Console.ReadLine()
            Exit Sub
        End Try
        If TcpListen = True Then StartTCPServer()
        swGuardTimer = New Timers.Timer(999) With {.Enabled = False}
        AddHandler swGuardTimer.Elapsed, AddressOf swGuardTimerFired
        Application.Run()   'Keeps us running...
    End Sub

    '====================================================================
    'Our TCP Server Routines.
    Private Sub StartTCPServer()
        Dim addr As IPAddress = IPAddress.Parse("0.0.0.0")
        Dim ep As New IPEndPoint(addr, TcpPort)
        TcpSvr = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        TcpSvr.Bind(ep)
        TcpSvr.Listen(2)
        TcpSvr.BeginAccept(AddressOf AcceptCallback, TcpSvr)
        WriteStatus("Listening for calls on TCP Port " & TcpPort)
    End Sub

    Private Sub AcceptCallback(ByVal ar As IAsyncResult)
        'System.Threading.Thread.CurrentThread.Name = "AcceptCallback"
        Debug.WriteLine(Now.ToLongTimeString & " AcceptCallback")
        If TcpClt Is Nothing And MdmHookState = ModemHookState.OnHook Then
            Debug.WriteLine("TcpClt=Nothing, MdmHookState=" & MdmHookState.ToString)
            TcpClt = TcpSvr.EndAccept(ar)
            WriteStatus("Connected to " & CType(TcpClt.RemoteEndPoint, IPEndPoint).ToString)
            TcpClt.BeginReceive(RcvBuf, 0, RcvBuf.Length, SocketFlags.None, AddressOf ReceiveCallback, RcvBuf)
            MdmCmdMode = False
            MdmHookState = ModemHookState.OffHook
            ModemError(1)
        Else
            Debug.WriteLine(Now.ToLongTimeString & " TcpMsg started")
            TcpMsg = TcpSvr.EndAccept(ar)
            TcpMsg.Send(msg)
            WriteStatus("Sent busy message to " & CType(TcpMsg.RemoteEndPoint, IPEndPoint).ToString)
            TcpMsg.Shutdown(SocketShutdown.Both)
            TcpMsg.Close()
            TcpMsg = Nothing
        End If
        TcpSvr.BeginAccept(AddressOf AcceptCallback, TcpSvr)
        WriteStatus("Listening for calls on TCP Port " & TcpPort)
    End Sub

    Private Sub ReceiveCallback(ByVal ar As IAsyncResult)
        'System.Threading.Thread.CurrentThread.Name = "ReceiveCallback"
        RcvBuf = CType(ar.AsyncState, Byte())
        Dim numbytes As Int32
        Try
            numbytes = TcpClt.EndReceive(ar)
        Catch
            Exit Sub
        End Try
        If numbytes = 0 Then    'client has disconnected.
            Ser.DtrEnable = False
            Ser.RtsEnable = False
            HangUp(False)       'False = Remote initiated disconnect
            MdmHookState = ModemHookState.OnHook
            Ser.DtrEnable = True
            Ser.RtsEnable = True
            If DropCarrierTerminateString = "" Then
                ModemError(NO_CARRIER)
            Else
                SendToSerial(DropCarrierTerminateString)
            End If
            Exit Sub
        End If
        If MdmCmdMode = False Then ProcessIncoming(numbytes)

        TcpClt.BeginReceive(RcvBuf, 0, RcvBuf.Length, SocketFlags.None, AddressOf ReceiveCallback, RcvBuf)
    End Sub

    Sub ProcessIncoming(ByVal numbytes As Int32)
        For i As Integer = 0 To numbytes - 1
            Select Case RcvBuf(i)
                Case TN_IAC
                    'WriteStatus("Telnet Protocol in use.")
                    Dim byt(0 To 2) As Byte
                    byt(0) = TN_IAC
                    i += 1
                    Select Case RcvBuf(i)
                        Case TN_SB
                            'ToChatWindow("  TN_SB" & vbCrLf)
                            WriteStatus("TN_SB Received! Subnegotiate Incomplete.")
                            Do
                                i += 1
                            Loop Until RcvBuf(i) = TN_SE Or i = numbytes - 1

                        Case TN_DO      'Server asking you to do something
                            'ToChatWindow("  TN_DO " & Str(buffer(i + 1)) & vbCrLf)
                            i += 1
                            Select Case RcvBuf(i)
                                Case TN_NAWS
                                    byt(1) = TN_WILL
                                    byt(2) = RcvBuf(i)
                                    SendBytesToHost(byt)
                                    SetLineSize()
                                Case Else
                                    byt(1) = TN_WONT
                                    byt(2) = RcvBuf(i)
                                    SendBytesToHost(byt)
                            End Select
                        Case TN_DONT    'Server asking you to NOT do something
                            'ToChatWindow("  TN_DONT" & Str(buffer(i + 1)) & vbCrLf)
                            byt(1) = TN_WONT
                            i += 1
                            byt(2) = RcvBuf(i)
                            SendBytesToHost(byt)
                        Case TN_WILL    'Server telling you it will do something
                            'ToChatWindow("  TN_WILL" & Str(buffer(i + 1)) & vbCrLf)
                            byt(1) = TN_DONT
                            i += 1
                            byt(2) = RcvBuf(i)
                            SendBytesToHost(byt)
                        Case TN_WONT    'Server telling you it won't do someting
                            'ToChatWindow("  TN_WONT" & Str(buffer(i + 1)) & vbCrLf)
                            byt(1) = TN_DONT
                            i += 1
                            byt(2) = RcvBuf(i)
                            SendBytesToHost(byt)
                    End Select
                Case Else
                    Try
                        Threading.Thread.Sleep(Throttle)
                        If IrcMode = False Then
                            If UseTranslation = False Then
                                Ser.Write(RcvBuf, i, 1)
                            Else
                                SendToSerial(Chr(RcvBuf(i)))
                            End If
                        Else
                            ProcessIrc(RcvBuf(i))
                        End If
                    Catch ex As Exception
                        WriteStatus("----------------------------")
                        WriteStatus("Routine: ReceiveCallBack")
                        WriteStatus("Exception: " & ex.Message)
                        WriteStatus("Chat Msg: " & Chr(RcvBuf(i)))
                    End Try
            End Select
        Next
    End Sub

    Private Sub ProcessIrc(ByVal byt As Byte)
        IrcInTxt &= Chr(byt)
        If IrcInTxt.EndsWith(vbCrLf) = False Then Exit Sub

        If IrcShowRawInIrc = True Then WriteStatus("[In ] " & IrcInTxt)

        If IrcInTxt.StartsWith("PING") Then
            SendIrcToHost("PONG " & IrcInTxt.Split(" ")(1))
            IrcInTxt = ""
            Exit Sub
        End If

        If IrcInTxt.Contains(":Closing Link:") Then
            SendIrcToSerial("*** Disconnected.")
            IrcInTxt = ""
            Exit Sub
        End If

        'strTmp(0) = Sender Identifier (":a2central.com")
        'strTmp(1) = Msg Type ("NOTICE",Numeric Code,"JOIN","PRIVMSG")
        'strTmp(2) = Msg Destination (UserName, Group Name)
        'StrTmp(3)... Message Body (Re-Join)

        IrcInTxt = IrcInTxt.Replace(Chr(1), "{A}")  'This is for the CTCP stuff
        Dim strTmp() As String = IrcInTxt.Split(" ")
        Dim id As String = strTmp(0).Split("!")(0).TrimStart(":").TrimEnd(New Char() {vbCrLf, vbLf})

        Select Case strTmp(1)
            Case "NOTICE"
                If strTmp(2) = "*" Then
                    SendIrcToHost("user " & IrcUid & " localhost localhost :" & IrcUid)
                    SendIrcToHost("nick " & IrcNick)
                    IrcInTxt = JoinString(strTmp, 3)
                End If
                If strTmp(3).StartsWith(":{A}") Then
                    strTmp(3) = strTmp(3).Replace(":{A}", "")
                    Select Case strTmp(3)
                        Case "VERSION", "TIME", "SOURCE", "USERINFO", "FINGER", "CLIENTINFO"
                            IrcInTxt = "*** CTCP/" & strTmp(3) & " Reply from " & id & ": " & JoinString(strTmp, 4)
                            IrcInTxt = IrcInTxt.Remove(IrcInTxt.Length - 3)
                        Case Else
                            IrcInTxt = JoinString(strTmp, 3)
                    End Select
                Else
                    IrcInTxt = JoinString(strTmp, 3)
                End If
            Case "JOIN"
                IrcInTxt = "*** " & id & " has entered the room."
            Case "NICK"
                IrcInTxt = "*** " & id & " changed nick to " & strTmp(2)
            Case "PART"
                IrcInTxt = "*** " & id & " has left the room."
            Case "QUIT"
                IrcInTxt = "*** " & id & " has left the room (" & JoinString(strTmp, 2).TrimStart(" ") & ")"
            Case "PRIVMSG"
                If strTmp(2) = IrcRoomName Then
                    IrcInTxt = "<" & id & "> " & JoinString(strTmp, 3)
                    If IrcInTxt.Contains("{A}ACTION ") And IrcInTxt.EndsWith("{A}") Then
                        IrcInTxt = IrcInTxt.Replace("<" & id & ">", "*** " & id)
                        IrcInTxt = IrcInTxt.Replace("{A}ACTION ", "").Replace("{A}", "")
                    End If
                ElseIf strTmp(2).ToLower = IrcNick.ToLower Then
                    If IrcInTxt.ToUpper.Contains("{A}VERSION{A}") Then
                        Dim x As String = My.Computer.Info.OSFullName
                        SendCtcpToHost("NOTICE " & id & " :^VERSION Internet Modem : v" & ver & " : " & x & "^")
                        IrcInTxt = "*** " & id & " requested your client version information."
                    ElseIf IrcInTxt.ToUpper.Contains("{A}TIME{A}") Then
                        SendCtcpToHost("NOTICE " & id & " :^TIME " & Now.ToLongDateString & " " & Now.ToLongTimeString & "^")
                        IrcInTxt = "*** " & id & " requested your local time."
                    ElseIf IrcInTxt.ToUpper.Contains("{A}SOURCE{A}") Then
                        SendCtcpToHost("NOTICE " & id & " :^SOURCE http://boycot.no-ip.com/InternetModem^")
                        IrcInTxt = "*** " & id & " requested where to download this client."
                        'ElseIf IrcInTxt.ToUpper.Contains("{A}PING{A}") Then
                    ElseIf IrcInTxt.ToUpper.Contains("{A}USERINFO{A}") Then
                        SendCtcpToHost("NOTICE " & id & " :^USERINFO " & IrcUserInfo & "^")
                        IrcInTxt = "*** " & id & " requested your user info."
                    ElseIf IrcInTxt.ToUpper.Contains("{A}FINGER{A}") Then
                        SendCtcpToHost("NOTICE " & id & " :^FINGER How dare you give me the finger!^")
                        IrcInTxt = "*** " & id & " requested fingered you; I returned the finger."
                    ElseIf IrcInTxt.ToUpper.Contains("{A}CLIENTINFO{A}") Then
                        SendCtcpToHost("NOTICE " & id & " :^CLIENTINFO Try: VERSION, TIME, SOURCE, or USERINFO^")
                        IrcInTxt = "*** " & id & " requested Client Info."
                    Else
                        'Private Message
                        IrcInTxt = ">>> <" & id & "> " & JoinString(strTmp, 3)

                        If IrcBlockPrivMsgs = True Then
                            SendIrcToHost("NOTICE " & id & " :Private Messages are Blocked!")
                            'IrcInTxt = "*** " & id & " attempted to send you a private message. The message was: """ & IrcInTxt & """"
                            IrcInTxt = ""
                            Exit Select
                        End If


                    End If
                End If
            Case Else
                Select Case Val(strTmp(1))
                    Case 0 To 259       'General Messages
                        IrcInTxt = JoinString(strTmp, 3)
                    Case 301
                        IrcInTxt = "*** " & strTmp(3) & " is away: " & JoinString(strTmp, 4)
                    Case 305
                        IrcInTxt = "*** You are no longer marked as being away."
                    Case 306
                        IrcInTxt = "*** You have been marked as being away."
                    Case 331, 332       'Topic
                        If strTmp(3) = IrcRoomName Then
                            IrcInTxt = "*** The topic is: " & JoinString(strTmp, 4)
                        Else
                            IrcInTxt = "*** <" & strTmp(3) & "> has set the topic to: " & JoinString(strTmp, 4)
                        End If
                    Case 333
                        Dim dt As DateTime = DateTime.Parse("01/01/1970 12:00:00 AM").Add(TimeSpan.FromSeconds(strTmp(5))).ToLocalTime
                        IrcInTxt = "*** Set by <" & strTmp(4) & "> on " & dt.ToString
                    Case 353            'User List For Room
                        ShowUserList(strTmp)
                        IrcInTxt = ""
                    Case 366            'End of Names List
                        IrcInTxt = ""
                    Case 370 To 379     'More General Messages
                        IrcInTxt = JoinString(strTmp, 3)
                        If IrcInTxt = "End of /MOTD command." AndAlso IrcRoomName <> "" Then
                            SendIrcToHost("JOIN " & IrcRoomName)
                        End If
                    Case 401
                        IrcInTxt = "*** No such nick/channel."
                    Case 403
                        IrcInTxt = "*** Invalid channel name."
                    Case 421
                        IrcInTxt = "*** Unknown command."
                End Select
        End Select

        If IrcInTxt = "" Then Exit Sub
        If IrcInTxt.EndsWith(vbCrLf) = False Then IrcInTxt &= vbCrLf
        SendIrcToSerial(IrcInTxt)

        IrcInTxt = ""
    End Sub

    Private Sub ShowUserList(ByVal str() As String)
        Dim x As String = "*** Users: "
        For i As Integer = 5 To str.Length - 1
            str(i) = str(i).TrimEnd(New Char() {vbCrLf, vbLf})
            str(i) = str(i).TrimStart(New Char() {":", "@"})
            x &= str(i) & ", "
        Next
        x = x.Trim.TrimEnd(",")
        SendIrcToSerial(x & vbCrLf)
    End Sub

    Private Function JoinString(ByVal str() As String, ByVal start As Integer) As String
        Dim tmpStr As String = ""
        For i As Integer = start To str.Length - 1
            tmpStr &= str(i) & " "
        Next
        Return tmpStr.Trim.TrimStart(":")
    End Function

    Private Sub SendCtcpToHost(ByVal msg As String)
        Dim byt(msg.Length + 2) As Byte
        For i As Integer = 0 To msg.Length - 1
            If msg.Substring(i, 1) = "^" Then
                byt(i) = &H1
            Else
                byt(i) = Asc(msg.Substring(i, 1))
            End If
        Next
        byt(byt.Length - 2) = &HD
        byt(byt.Length - 1) = &HA
        TcpClt.Send(byt, byt.Length, SocketFlags.None)
        If IrcShowRawOutIrc = True Then WriteStatus(ASCII.GetString(byt, 0, byt.Length))
    End Sub

    Private Sub SendIrcToHost(ByVal txt As String)
        Dim buf() As Byte = ASCII.GetBytes(txt & vbCrLf)
        SendBytesToHost(buf)
        If IrcShowRawOutIrc = True Then WriteStatus("[OUT] " & txt)
    End Sub

    Private Sub SendIrcToSerial(ByVal txt As String)
        If IrcShowTimeStamp = True Then txt = "[" & Now.ToShortTimeString & "] " & txt
        If txt.Length > 80 Then
            Dim al As ArrayList = BreakTextLine(txt)
            For i As Integer = 0 To al.Count - 1
                Dim buf() As Byte = ASCII.GetBytes(al(i).ToString)
                For j As Integer = 0 To buf.Length - 1
                    If UseTranslation = False Then
                        Ser.Write(buf, j, 1)
                    Else
                        SendToSerial(Chr(buf(j)))
                    End If
                Next
            Next
        Else
            Dim buf() As Byte = ASCII.GetBytes(txt)
            For i As Integer = 0 To buf.Length - 1
                If UseTranslation = False Then
                    Ser.Write(buf, i, 1)
                Else
                    SendToSerial(Chr(buf(i)))
                End If
            Next
        End If
    End Sub

    Private Function BreakTextLine(ByVal txt As String) As ArrayList
        Dim al As New ArrayList
        Dim tmp() As String = txt.Trim.Split(" ")
        Dim tmp2 As String = ""
        For i As Integer = 0 To tmp.Length - 1
            If tmp2.Length + tmp(i).Length + 1 < LineLength Then
                tmp2 &= tmp(i) & " "
            Else
                al.Add(tmp2.TrimEnd & vbCrLf)
                tmp2 = Space(ContIndent) & tmp(i) & " "
            End If
        Next
        If tmp2.TrimEnd <> "" Then al.Add(tmp2.TrimEnd & vbCrLf)
        Return al
    End Function

    Private Sub SetLineSize()
        'The syntax for the subnegotiation is: IAC SB NAWS WIDTH[1] WIDTH[0] HEIGHT[1] HEIGHT[0] IAC SE
        Dim byt() As Byte = {TN_IAC, TN_SB, TN_NAWS, LineSize \ 256, LineSize Mod 256, 0, 24, TN_IAC, TN_SE}
        SendBytesToHost(byt)
    End Sub

    Private Sub SendBytesToHost(ByVal byt() As Byte)
        TcpClt.Send(byt, byt.Length, SocketFlags.None)
    End Sub

    Private Sub HangUp(ByVal local As Boolean, Optional ATZ As Boolean = False)
        MdmErr = OK
        If Not TcpClt Is Nothing Then
            Try
                TcpClt.Shutdown(SocketShutdown.Both)
                TcpClt.Close()
                TcpClt = Nothing
                If local = True Then
                    If Not ATZ Then WriteStatus("Locally initiated disconnect")
                Else
                    If Not ATZ Then WriteStatus("Remote initiated disconnect")
                End If
            Catch ex As Exception
                If Not ATZ Then WriteStatus("Locally initiated disconnect, already offline")
            End Try
        Else
            If Not ATZ Then WriteStatus("Locally initiated disconnect, already offline")
        End If
        MdmCmdMode = True
        Debug.WriteLine(MdmCmdMode.ToString)
    End Sub

    'Routine to write status to console window...
    'and to log file if set.
    Private Sub WriteStatus(ByVal msg As String)
        Dim dt As String = Date.Now.ToShortDateString & " " & Date.Now.ToShortTimeString
        msg = dt & " - " & msg
        Console.WriteLine(msg)
        Dim sw As StreamWriter
        If keepLog = True Then
            If logFile = "" Then Exit Sub
            sw = New StreamWriter(logFile, True)
            'sw = My.Computer.FileSystem.OpenTextFileWriter(logFile, True)
            sw.WriteLine(msg)
            sw.Close()
        End If
    End Sub

    Private Sub SerRcvEvent(ByVal sender As Object, ByVal e As SerialDataReceivedEventArgs) Handles Ser.DataReceived
        SerByt = Ser.BytesToRead
        Ser.Read(XmtBuf, 0, SerByt)

        If UseTranslation = True Then
            For i As Integer = 0 To SerByt - 1
                XmtBuf(i) = ASCIICode(XmtBuf(i))
            Next
        End If

        Dim rcvString As String = ASCII.GetString(XmtBuf, 0, SerByt)
        'Console.WriteLine("rcvStr: " & rcvString)

        ' --Did we get a Modem Esc Command?
        '   If so, start the 1 second timer.
        If rcvString.Substring(0, 1) = "+" Then

            'If InStr(rcvString, "+++") Then
            swGuardTimer.Enabled = True
            WriteStatus("GuardTimerEnabled")
        End If

        ' --If we're in command mode, send characters received to be processed
        If MdmCmdMode = True Then
            ProcessMdmCmd(rcvString)
            Exit Sub
        End If

        ' --Otherwise, send it out to our connected user.
        If TcpClt Is Nothing Then Exit Sub
        If IrcMode = False Then
            Try
                TcpClt.Send(XmtBuf, SerByt, SocketFlags.None)
            Catch
            End Try
        Else
            PrepareIrcOut(rcvString)
        End If
    End Sub

    Private Sub PrepareIrcOut(ByVal txt As String)
        IrcOutTxt &= txt

        If IrcOutTxt.EndsWith(vbCrLf) = False Then Exit Sub
        IrcOutTxt = IrcOutTxt.TrimEnd(vbLf).TrimEnd(vbCrLf)

        If IrcOutTxt.StartsWith("/") Then
            If IrcOutTxt.ToLower.StartsWith("/me ") Then
                IrcSendAction(IrcOutTxt.Substring(4))
            ElseIf IrcOutTxt.ToLower.StartsWith("/ctcp ") Then
                ' "/ctcp <TargetUser> <QueryVerb>"
                Dim msg() As String = IrcOutTxt.Split(" ")
                SendIrcToSerial("<<< <" & msg(1) & "> CTCP " & msg(2).ToUpper & vbCrLf)
                SendCtcpToHost("PRIVMSG " & msg(1) & " :^" & msg(2).ToUpper & "^")
            ElseIf IrcOutTxt.ToLower.StartsWith("/quit") Then
                SendIrcToHost(IrcOutTxt.Replace("/quit", "QUIT").Replace("QUIT ", "QUIT :"))
            ElseIf IrcOutTxt.ToLower.StartsWith("/join") Then
                If IrcRoomName <> "" Then SendIrcToHost("PART " & IrcRoomName)
                SendIrcToHost("JOIN " & IrcOutTxt.Substring(6))
                IrcRoomName = IrcOutTxt.Substring(6)
            ElseIf IrcOutTxt.ToLower.StartsWith("/part") Then
                SendIrcToHost("PART " & IrcRoomName)
                IrcRoomName = ""
            ElseIf IrcOutTxt.ToLower.StartsWith("/nick ") Then
                SendIrcToHost(IrcOutTxt.Replace("/nick ", "NICK "))
            ElseIf IrcOutTxt.ToLower.StartsWith("/msg ") Then
                Dim msg() As String = IrcOutTxt.Split(" ")
                SendIrcToSerial("<<< <" & msg(1) & "> " & JoinString(msg, 2) & vbCrLf)
                SendIrcToHost("PRIVMSG " & msg(1) & " :" & JoinString(msg, 2))
            ElseIf IrcOutTxt.ToLower.StartsWith("/ping ") Then
                SendIrcToHost(IrcOutTxt.Replace("/ping ", "PING "))
            ElseIf IrcOutTxt.ToLower.StartsWith("/help") Then
                IrcShowHelp()
            Else
                SendIrcToSerial("*** Command not supported at this time." & vbCrLf)
                IrcShowHelp()
            End If
            IrcOutTxt = ""
        End If

        If IrcOutTxt = "" Then Exit Sub
        SendIrcToHost("PRIVMSG " & IrcRoomName & " :" & IrcOutTxt)

        Dim x As String = ""
        x &= "<" & IrcNick & "> " & IrcOutTxt
        If x.EndsWith(vbCrLf) = False Then x &= vbCrLf
        SendIrcToSerial(x)
        IrcOutTxt = ""
    End Sub

    Private Sub IrcShowHelp()
        SendIrcToSerial("*** IRC Commands ***" & vbCrLf)
        SendIrcToSerial("/ME <Action performed by me>" & vbCrLf)
        SendIrcToSerial("/CTCP <TargetUser> <DesiredQuery>" & vbCrLf)
        SendIrcToSerial("/PING <TargetUser>" & vbCrLf)
        SendIrcToSerial("/QUIT [Parting message for the group]" & vbCrLf)
        SendIrcToSerial("/PART <RoomName>" & vbCrLf)
        SendIrcToSerial("/JOIN <RoomName>" & vbCrLf)
        SendIrcToSerial("/NICK <NewNickName>" & vbCrLf)
        SendIrcToSerial("/MSG <TargetUser> <PrivateMessage>" & vbCrLf)
    End Sub

    Private Sub IrcSendAction(ByVal action As String)
        SendCtcpToHost("PRIVMSG " & IrcRoomName & " :^ACTION " & action & " ^")
        SendIrcToSerial("*** " & IrcNick & " " & action & vbCrLf)
    End Sub

    Private Sub ProcessMdmCmd(ByVal CmdStr As String)
        MdmErr = OK
        CmdStr = CmdStr.Replace(Chr(0), "")
        MdmCmdStr += UCase(CmdStr)
        'If MdmEcho = ModemEcho.EchoOn Then Ser.Write(CmdStr)
        If MdmEcho = ModemEcho.EchoOn Then SendToSerial(CmdStr)

        '--Command is still coming in if we haven't received the CR
        If Right(MdmCmdStr, 1) <> Chr(13) Then Exit Sub
        '--CR only does not a command make...
        If MdmCmdStr.Equals(vbCr) OrElse MdmCmdStr.Equals(vbLf) OrElse MdmCmdStr.Equals(vbCrLf) Then
            MdmCmdStr = ""
            Exit Sub
        End If

        '--Get rid of the +'s from the ESC sequence, and trim off the CR
        MdmCmdStr = MdmCmdStr.Replace("+", "").Trim(Chr(13)).Trim(Chr(10)).Trim
        If MdmCmdStr = "A/" Then MdmCmdStr = LastATCmd Else LastATCmd = MdmCmdStr 'A/ means repeat last command
        If ShowModemCommands = True Then WriteStatus("ModemCommandString = " & MdmCmdStr)

        If MdmCmdStr = "" Then
            ModemError(OK)
            Exit Sub
        End If

        '--All good command strings begin with AT
        If Left(MdmCmdStr, 2) <> "AT" Then
            Debug.WriteLine("MdmCmdStr doesn't begin with AT")
            MdmCmdStr = ""
            ModemError(ERR)
            Exit Sub
        End If

        '--Remove all spaces
        MdmCmdStr = UCase(MdmCmdStr.Replace(" ", ""))

        '--Sometimes they just type AT to see if the modem is responding.
        If MdmCmdStr = "AT" Then
            MdmCmdStr = ""
            ModemError(OK)
            Exit Sub
        End If

        '--Remote the leading AT
        MdmCmdStr = MdmCmdStr.TrimStart("AT").Trim

        '--Now start dealing with the command string

        For i As Integer = 0 To MdmCmdStr.Length - 1
            Select Case MdmCmdStr.Substring(i, 1)
                Case "&"    'An Ampersand Command
                    i += 1
                    If MdmCmdStr.Substring(i, 1) = "V" Then
                        SendToSerial("CURRENT SETTINGS: " &
                                  "E" & MdmEcho & " Q" & MdmResultCodes & " S0=" & MdmRingMode & " V" & MdmResultCodeFormat & vbCrLf)
                        Continue For
                    End If
                Case "I"    'The Modem Information Command
                    SendToSerial("Internet Modem Emulator v" & ver & " by Terry Olsen" & vbCrLf &
                              "Email: tolsen64@hotmail.com" & vbCrLf &
                              "Web: http://boycot.no-ip.com/InternetModem" & vbCrLf)
                    Continue For
                Case "Z"    'The Reset Command
                    HangUp(True, True)
                    MdmErr = OK
                    MdmCmdMode = True
                    MdmHookState = ModemHookState.OnHook
                    Continue For
                Case "A"    'The Answer Command, we'll ignore this
                    Continue For
                Case "E"    'The Echo on/off Command
                    i += 1
                    If Val(MdmCmdStr.Substring(i, 1)) >= 1 Then
                        MdmEcho = ModemEcho.EchoOn
                    Else
                        MdmEcho = ModemEcho.EchoOff
                    End If
                    Continue For
                Case "H"    'The Modemhook Command
                    If i < (MdmCmdStr.Length - 1) AndAlso IsDigit(MdmCmdStr.Substring(i + 1, 1)) = True Then
                        i += 1
                        If Val(MdmCmdStr.Substring(i, 1)) >= 1 Then
                            MdmHookState = ModemHookState.OffHook
                        Else
                            MdmHookState = ModemHookState.OnHook
                            HangUp(True)
                        End If
                    Else
                        MdmHookState = ModemHookState.OnHook
                        HangUp(True)
                    End If
                    Continue For
                Case "Q"    'The Result Codes on/off command
                    If i < (MdmCmdStr.Length - 1) AndAlso IsDigit(MdmCmdStr.Substring(i + 1, 1)) = True Then
                        i += 1
                        If Val(MdmCmdStr.Substring(i, 1)) >= 1 Then
                            MdmResultCodes = ModemResultCodes.Disabled
                        Else
                            MdmResultCodes = ModemResultCodes.Enabled
                        End If
                    Else
                        MdmResultCodes = ModemResultCodes.Enabled
                    End If
                    Continue For
                Case "S"    'An S-Register command
                    Dim sreg As String = ""
                    While IsDigit(MdmCmdStr.Substring(i + 1, 1)) = True
                        i += 1
                        sreg &= MdmCmdStr.Substring(i, 1)
                    End While
                    If Val(sreg) = 0 Then
                        If MdmCmdStr.Substring(i + 1, 1) = "=" Then
                            i += 2  'Skip the '=' and go to the value
                            If Val(MdmCmdStr.Substring(i, 1)) >= 1 Then
                                MdmRingMode = ModemRingMode.SendConnect
                            Else
                                MdmRingMode = ModemRingMode.SendRing
                            End If
                        Else
                            MdmRingMode = ModemRingMode.SendRing
                        End If
                    End If
                    Continue For
                Case "V"    'Result Code Format
                    If i < (MdmCmdStr.Length - 1) AndAlso IsDigit(MdmCmdStr.Substring(i + 1, 1)) = True Then
                        i += 1
                        If Val(MdmCmdStr.Substring(i, 1)) >= 1 Then
                            MdmResultCodeFormat = ModemResultCodeFormat.Text
                        Else
                            MdmResultCodeFormat = ModemResultCodeFormat.Numeric
                        End If
                    Else
                        MdmResultCodeFormat = ModemResultCodeFormat.Numeric
                    End If
                    Continue For
                Case "D"    'Dial Command, the "T" is going to be mandatory, unless re-dialing last dialed system
                    i += 1
                    If i < (MdmCmdStr.Length - 1) AndAlso MdmCmdStr.Substring(i, 1) = "T" Then
                        i += 1
                        GoDialOut(MdmCmdStr.Substring(i))
                        MdmCmdStr = ""
                        Exit Sub
                    Else
                        If LastDialed <> "" Then GoDialOut(LastDialed) Else MdmErr = ERR
                    End If
            End Select
        Next
        MdmCmdStr = ""
        ModemError(MdmErr)
    End Sub

    Private Sub SendToSerial(ByVal txt As String)
        If UseTranslation = True Then
            Dim buf(0) As Byte
            For i As Integer = 0 To txt.Length - 1
                buf(0) = ASCIICode(Asc(txt.Substring(i, 1)))
                Ser.Write(buf, 0, 1)
            Next
        Else
            Ser.Write(txt)
        End If
    End Sub

    Private Sub ModemError(ByVal code As Integer)
        If MdmResultCodes = ModemResultCodes.Enabled Then
            If MdmResultCodeFormat = ModemResultCodeFormat.Text Then
                Dim tmp As String
                tmp = "Modem Error Code = " & MdmErrorCode(code)
                If code = 1 Then tmp &= " " & MdmBaud
                If ShowResultCodes = True Then WriteStatus(tmp)
                SendToSerial(vbCrLf & MdmErrorCode(code))
                If code = 1 Then SendToSerial(" " & MdmBaud)
                SendToSerial(vbCrLf)
            Else
                SendToSerial(vbCrLf & code & vbCrLf)
                If ShowResultCodes = True Then WriteStatus("Modem Error Code = " & code)
            End If
        End If
    End Sub

    Private Sub GoDialOut(ByVal cmd As String)
        Dim port As Integer
        Dim IP As IPAddress
        Dim host As String

        LastDialed = cmd

        cmd = cmd.Trim(Chr(13))
        'Debug.WriteLine("CMD=" & cmd)

        '--Next, see if a port is specified
        cmd = cmd.Replace(",", ":")   'Change comma separator to colon
        cmd = cmd.Replace(" ", ":")   'change space separator to colon
        'Debug.WriteLine("CMD=" & cmd)

        '--Define the port we'll use to dial out
        If cmd.Contains(":") Then
            port = Val(Mid(cmd, InStr(cmd, ":") + 1))
            cmd = cmd.Remove(cmd.IndexOf(":"))
        Else
            port = 23
        End If
        'Debug.WriteLine("CMD=" & cmd)
        'Debug.WriteLine("PORT=" & port)
        'Exit Sub

        '--Now we check what kind of address we have.  It could be:
        '   hostname.com, xx.xxx.xx.x, or WWWXXXYYYZZZ
        Dim IpNoDots As Boolean = True
        If Not cmd.Contains(".") Then
            If cmd.Length = 12 Then
                For i As Integer = 1 To 12
                    If Not (Mid(cmd, i, 1) >= "0" And Mid(cmd, i, 1) <= "9") Then
                        IpNoDots = False : Exit For
                    End If
                Next
                If IpNoDots = True Then
                    cmd = cmd.Insert(9, ".")
                    cmd = cmd.Insert(6, ".")
                    cmd = cmd.Insert(3, ".")
                End If
            End If
        End If
        'Debug.WriteLine("CMD=" & cmd)
        'Debug.WriteLine("PORT=" & port)
        'Exit Sub

        '--By the time we get here, we should have either a hostname or
        '   an IP address in the form of xxx.xx.xx.x
        Try
            IP = Dns.GetHostEntry(cmd).AddressList(0)
            Try
                host = Dns.GetHostEntry(IP).HostName
            Catch
                host = cmd
            End Try
            WriteStatus("Dialing: " & host & " (" & IP.ToString & ") on port " & port.ToString)
        Catch ex As SocketException
            WriteStatus("Error dialing " & cmd & ":" & port.ToString)
            WriteStatus("Error message:" & ex.Message)
            ModemError(NO_DIAL_TONE)
            Exit Sub
        End Try

        MdmCmdMode = False
        Dim IPE As New IPEndPoint(IP, port)

        Try
            TcpClt = New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            TcpClt.BeginConnect(IPE, AddressOf DialOutConnected, Nothing)
            If port = 6667 Then
                IrcMode = True
                WriteStatus("IRC Mode On.")
            Else
                IrcMode = False
            End If
        Catch ex As SocketException
            ModemError(NO_DIAL_TONE)
            WriteStatus(ex.Message)
        End Try

    End Sub

    Private Sub DialOutConnected(ByVal ar As IAsyncResult)
        Try
            TcpClt.EndConnect(ar)
        Catch ex As SocketException
            WriteStatus(ex.Message)
            MdmCmdMode = True
            ModemError(NO_ANSWER)
            Exit Sub
        End Try
        WriteStatus("Connection established.")
        ModemError(CONNECT)
        TcpClt.BeginReceive(RcvBuf, 0, RcvBuf.Length, SocketFlags.None, AddressOf ReceiveCallback, RcvBuf)
    End Sub

    Private Sub swGuardTimerFired(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs)
        swGuardTimer.Enabled = False
        If Ser.BytesToRead < 1 Then
            MdmCmdMode = True
            ModemError(OK)
        Else
            MdmCmdMode = False
            If TcpClt Is Nothing Then Exit Sub
            TcpClt.Send(XmtBuf, SerByt, SocketFlags.None)
        End If
        WriteStatus("GuardTimerFired")
    End Sub

    Private Function IsDigit(ByVal c As Char) As Boolean
        If c >= "0" And c <= "9" Then Return True
        Return False
    End Function

End Module
