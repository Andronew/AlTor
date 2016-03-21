Imports System.Net.Mail
Imports System.Text
Imports System.Net
Imports System.IO
Imports System.Diagnostics


Public Class Form1
    Dim email As String = ""
    Dim password As String = ""
    Dim outgoing As String = "smtp.gmail.com"
    Dim incoming As String = "pop.gmail.com"

    Dim firstLaunch As Boolean = True
    Dim firstActivate As Boolean = True

    Dim senderList() As String
    Dim whitelist(0) As String
    Dim whitelistBool(0) As Boolean

    Dim messageCount As Integer

    Dim recieved() As String
    Dim didRecieve As Boolean

    Dim response As String = ""
    Dim curSearch As String
    Dim searchURL As String
    Dim tempSourceString As String
    Dim torCompareStr() As String
    Dim torURLString As String

    Dim tempIntOne As Integer
    Dim tempIntTwo As Integer
    Dim torLinkIndex As Integer

    Dim downloadLink As String

    Dim tcpC As New System.Net.Sockets.TcpClient()

    Dim failedDL As Boolean = False
    Dim failResponse As String
    Dim successDL As Boolean = False
    Dim successResponse As String

    Dim tempCount As Integer = -1
    Dim tempInt As Integer

    Dim torName As String
    Dim curIntreval As Integer = 10000
    Dim countDown As Integer = 10

    Dim request() As System.Net.HttpWebRequest
    Dim responseWeb() As System.Net.HttpWebResponse
    Dim sr() As System.IO.StreamReader
    Dim source As String

    Dim tempString As String

    Dim countTo As Integer
    Dim preLength As Integer

    Dim badSearchPage As Boolean = False

    Dim rebootTime As Integer = 43200

    Private Sub tmrTest_Tick(sender As Object, e As EventArgs) Handles tmrTest.Tick
        tmrReboot.Stop()
        tmrCountdown.Stop()
        tmrTest.Stop()
        curIntreval = 300000 * Rnd() + 30000 'GENERATES RANDOM INT BETWEEN 30,000ms AND 60,000ms FOR THE CHECK INTREVAL
        countDown = (curIntreval / 1000) - 1 'CONVERTS THE ABOVE INT TO SECONDS
        tmrTest.Interval = curIntreval
        Try
            If recieveMail() Then
                txtDebug.AppendText(vbNewLine + "There's " + messageCount.ToString() + " new messages!")
                tempCount = -1
                For Each x In whitelistBool
                    tempCount += 1
                    If x Then
                        Dim curTime As Integer = System.DateTime.Now.ToString("HH")

                        If curTime > 17 And curTime < 24 Then
                            response = "Good evening!"

                        ElseIf curTime >= 0 And curTime < 12 Then
                            response = "Good morning!"

                        Else
                            response = "Good afternoon!"

                        End If

                        response += " I will begin looking for: "

                        If recieved.Length() <> 2 Then
                            For y As Integer = 1 To recieved.Length() - 1
                                If senderList(y).Equals(whitelist(tempCount)) Then
                                    response += recieved(y) + ", "
                                End If
                            Next
                        Else
                            response += recieved(1)
                        End If

                        txtDebug.AppendText(vbNewLine + "Sending response """ + response + """ to " + whitelist(tempCount) + " !")
                        sendMail(whitelist(tempCount), response)
                    End If
                Next

                getThatTorrent()
            Else
                txtDebug.AppendText(vbNewLine + "No new mail!")
            End If

            For i As Integer = 0 To whitelistBool.Length - 1
                whitelistBool(i) = False
            Next

        Catch
            txtDebug.AppendText(vbNewLine + "Failed to connect to gmail... retrying in 1 minute.")
            curIntreval = 60000
            countDown = 60
            tmrTest.Interval = curIntreval
        End Try
        tmrReboot.Interval = rebootTime * 1000
        tmrReboot.Start()
        tmrTest.Start()
        tmrCountdown.Start()
    End Sub

    Private Sub getThatTorrent()
        badSearchPage = False
        tempCount = -1
        ReDim request(recieved.Length * 2)
        ReDim responseWeb(recieved.Length * 2)
        ReDim sr(recieved.Length * 2)
        ReDim torCompareStr(3)
        countTo = recieved.Length() - 1
        preLength = recieved.Length() - 1

        For Each y In whitelistBool
            tempCount += 1
            failedDL = False
            successDL = False
            failResponse = "I failed to get torrents for the following: "
            successResponse = "I was able to get torrents for the following: "
            If y Then
                For x As Integer = 1 To recieved.Length() - 1
                    If senderList(x).Equals(whitelist(tempCount)) Then
                        Try
                            recieved(x) = recieved(x).Trim()
                            If recieved(x).Contains(",") Then
                                ReDim Preserve recieved(recieved.Length)
                                ReDim Preserve senderList(senderList.Length)
                                recieved(recieved.Length() - 1) = recieved(x).Substring(recieved(x).IndexOf(",") + 1, recieved(x).Length - (recieved(x).IndexOf(",") + 1))
                                recieved(x) = recieved(x).Substring(0, recieved(x).IndexOf(","))
                                senderList(senderList.Length() - 1) = senderList(x)
                                countTo += 1

                                Do Until recieved(countTo).Contains(",") = False
                                    ReDim Preserve recieved(recieved.Length)
                                    ReDim Preserve senderList(senderList.Length)
                                    recieved(recieved.Length() - 1) = recieved(countTo).Substring(recieved(countTo).IndexOf(",") + 1, recieved(countTo).Length - (recieved(countTo).IndexOf(",") + 1))
                                    recieved(countTo) = recieved(countTo).Substring(0, recieved(countTo).IndexOf(","))
                                    senderList(senderList.Length() - 1) = senderList(countTo)
                                    countTo += 1
                                Loop
                            End If
                            txtDebug.AppendText(vbNewLine + "Finding torrent for """ + recieved(x) + """")

                            'Creates the URL
                            searchURL = "https://kickass.to/usearch/"
                            curSearch = recieved(x)
                            curSearch = curSearch.Replace(" ", "%20")
                            searchURL += curSearch
                            searchURL += "/?field=seeders&sorder=desc.html"
                            txtDebug.AppendText(vbNewLine + "Found search URL @ " + searchURL)

                            'Gets the source of the URL
                            request(x) = System.Net.HttpWebRequest.Create(searchURL)
                            request(x).AutomaticDecompression = DecompressionMethods.GZip
                            responseWeb(x) = request(x).GetResponse()
                            sr(x) = New System.IO.StreamReader(responseWeb(x).GetResponseStream)
                            Dim source As String = sr(x).ReadToEnd.ToString()
                            txtDebug.AppendText(vbNewLine + "Retrieved the source of """ + recieved(x) + """ search page!")

                            'Finds the top seeded torrent URL
                            'Find this '<div class="iaconbox center floatright">' and the top torrent URL is one line below it
                            'The url will start at 'href=' and end at '>'
                            tempIntOne = source.IndexOf("<div class=""iaconbox center floatright"">")

                            tempIntTwo = source.IndexOf("href=""", tempIntOne)
                            torLinkIndex = tempIntTwo + 6
                            tempIntOne = source.IndexOf(".html", tempIntTwo) + 5
                            torURLString = source.Substring(torLinkIndex, (tempIntOne - torLinkIndex))
                            torURLString = "https://kickass.to" + torURLString
                            torCompareStr(0) = torURLString

                            If torURLString.Length() > 200 Then
                                badSearchPage = True
                                tempIntTwo = source.IndexOf("class=""torType filmType"">")
                                tempIntOne = source.LastIndexOf("a href", tempIntTwo, 200)
                                tempIntTwo = source.IndexOf("href=""", tempIntOne)
                                torLinkIndex = tempIntTwo + 6
                                tempIntOne = source.IndexOf(".html", tempIntTwo) + 5
                                torURLString = source.Substring(torLinkIndex, (tempIntOne - torLinkIndex))
                                torURLString = "https://kickass.to" + torURLString
                                torCompareStr(0) = torURLString
                            End If

                            For z As Integer = 1 To 2
                                If badSearchPage Then
                                    tempIntOne = source.IndexOf("class=""torType filmType"">")
                                    tempIntTwo = source.LastIndexOf("a href", tempIntOne, 200)
                                Else
                                    tempIntTwo = source.IndexOf("<div class=""iaconbox center floatright"">", tempIntOne)
                                End If

                                tempIntOne = source.IndexOf("href=""", tempIntTwo)
                                torLinkIndex = tempIntOne + 6
                                tempIntTwo = source.IndexOf(".html", tempIntOne) + 5
                                torURLString = source.Substring(torLinkIndex, (tempIntTwo - torLinkIndex))
                                torURLString = "https://kickass.to" + torURLString
                                torCompareStr(z) = torURLString
                            Next

                            torURLString = bestTorURL(torCompareStr)

                            txtDebug.AppendText(vbNewLine + "Best torrent URL located @" + torURLString)

                            request(x).Abort()
                            responseWeb(x).Close()
                            sr(x).Close()

                            'Gets the torrent download link
                            request(x + 1) = System.Net.HttpWebRequest.Create(torURLString)
                            request(x + 1).AutomaticDecompression = DecompressionMethods.GZip
                            responseWeb(x + 1) = request(x + 1).GetResponse()
                            sr(x + 1) = New System.IO.StreamReader(responseWeb(x + 1).GetResponseStream)
                            source = sr(x + 1).ReadToEnd.ToString()

                            request(x + 1).Abort()
                            responseWeb(x + 1).Close()
                            sr(x + 1).Close()

                            tempIntOne = source.IndexOf("""siteButton giantButton""")

                            tempIntTwo = source.IndexOf("href", tempIntOne) + 6
                            tempIntOne = source.IndexOf("""", tempIntTwo)
                            downloadLink = source.Substring(tempIntTwo, (tempIntOne - tempIntTwo))
                            tempIntOne = downloadLink.IndexOf("?")
                            torName = downloadLink.Substring(tempIntOne + 1)
                            tempIntTwo = torName.IndexOf("=") + 1
                            torName = torName.Substring(tempIntTwo)
                            downloadLink = downloadLink.Substring(0, tempIntOne)
                            txtDebug.AppendText(vbNewLine + "Found torrent file """ + torName + """")
                            txtDebug.AppendText(vbNewLine + "Found torrent download link @ """ + downloadLink + """")
                            txtDebug.AppendText(vbNewLine + "Downloading: " + recieved(x))

                            Process.Start("chrome.exe", downloadLink)
                            Threading.Thread.Sleep(5000)

                            txtDebug.AppendText(vbNewLine + "Complete")
                            successDL = True
                            successResponse += recieved(x) + ", "
                        Catch
                            txtDebug.AppendText(vbNewLine + "Failed on: " + recieved(x))
                            failedDL = True
                            failResponse += recieved(x) + ", "

                        End Try
                    End If
                    badSearchPage = False
                Next

                If preLength <> countTo Then
                    badSearchPage = False
                    ReDim request(countTo * 2)
                    ReDim responseWeb(countTo * 2)
                    ReDim sr(countTo * 2)
                    ReDim torCompareStr(3)
                    For x As Integer = preLength + 1 To countTo
                        If senderList(x).Equals(whitelist(tempCount)) Then
                            Try
                                recieved(x) = recieved(x).Trim()
                                txtDebug.AppendText(vbNewLine + "Finding torrent for """ + recieved(x) + """")
                                'Creates the URL
                                searchURL = "https://kickass.to/usearch/"
                                curSearch = recieved(x)
                                curSearch = curSearch.Replace(" ", "%20")
                                searchURL += curSearch
                                searchURL += "/?field=seeders&sorder=desc.html"
                                txtDebug.AppendText(vbNewLine + "Found search URL @ " + searchURL)
                                'Gets the source of the URL
                                request(x) = System.Net.HttpWebRequest.Create(searchURL)
                                request(x).AutomaticDecompression = DecompressionMethods.GZip
                                responseWeb(x) = request(x).GetResponse()
                                sr(x) = New System.IO.StreamReader(responseWeb(x).GetResponseStream)
                                Dim source As String = sr(x).ReadToEnd.ToString()
                                txtDebug.AppendText(vbNewLine + "Retrieved the source of """ + recieved(x) + """ search page!")

                                'Finds the top seeded torrent URL
                                'Find this '<div class="iaconbox center floatright">' and the top torrent URL is one line below it
                                'The url will start at 'href=' and end at '>'
                                tempIntOne = source.IndexOf("<div class=""iaconbox center floatright"">")

                                tempIntTwo = source.IndexOf("href=""", tempIntOne)
                                torLinkIndex = tempIntTwo + 6
                                tempIntOne = source.IndexOf(".html", tempIntTwo) + 5
                                torURLString = source.Substring(torLinkIndex, (tempIntOne - torLinkIndex))
                                torURLString = "https://kickass.to" + torURLString
                                torCompareStr(0) = torURLString

                                If torURLString.Length() > 200 Then
                                    badSearchPage = True
                                    tempIntTwo = source.IndexOf("class=""torType filmType"">")
                                    tempIntOne = source.LastIndexOf("a href", tempIntTwo, 200)
                                    tempIntTwo = source.IndexOf("href=""", tempIntOne)
                                    torLinkIndex = tempIntTwo + 6
                                    tempIntOne = source.IndexOf(".html", tempIntTwo) + 5
                                    torURLString = source.Substring(torLinkIndex, (tempIntOne - torLinkIndex))
                                    torURLString = "https://kickass.to" + torURLString
                                    torCompareStr(0) = torURLString
                                End If

                                For z As Integer = 1 To 2
                                    If badSearchPage Then
                                        tempIntOne = source.IndexOf("class=""torType filmType"">")
                                        tempIntTwo = source.LastIndexOf("a href", tempIntOne, 200)
                                    Else
                                        tempIntTwo = source.IndexOf("<div class=""iaconbox center floatright"">", tempIntOne)
                                    End If

                                    tempIntOne = source.IndexOf("href=""", tempIntTwo)
                                    torLinkIndex = tempIntOne + 6
                                    tempIntTwo = source.IndexOf(".html", tempIntOne) + 5
                                    torURLString = source.Substring(torLinkIndex, (tempIntTwo - torLinkIndex))
                                    torURLString = "https://kickass.to" + torURLString
                                    torCompareStr(z) = torURLString
                                Next

                                torURLString = bestTorURL(torCompareStr)
                                txtDebug.AppendText(vbNewLine + "Best torrent URL located @" + torURLString)

                                request(x).Abort()
                                responseWeb(x).Close()
                                sr(x).Close()

                                'Gets the torrent download link
                                request(x + 1) = System.Net.HttpWebRequest.Create(torURLString)
                                request(x + 1).AutomaticDecompression = DecompressionMethods.GZip
                                responseWeb(x + 1) = request(x + 1).GetResponse()
                                sr(x + 1) = New System.IO.StreamReader(responseWeb(x + 1).GetResponseStream)
                                source = sr(x + 1).ReadToEnd.ToString()

                                request(x + 1).Abort()
                                responseWeb(x + 1).Close()
                                sr(x + 1).Close()

                                tempIntOne = source.IndexOf("""siteButton giantButton""")
                                tempIntTwo = source.IndexOf("href", tempIntOne) + 6
                                tempIntOne = source.IndexOf("""", tempIntTwo)
                                downloadLink = source.Substring(tempIntTwo, (tempIntOne - tempIntTwo))
                                tempIntOne = downloadLink.IndexOf("?")
                                torName = downloadLink.Substring(tempIntOne + 1)
                                tempIntTwo = torName.IndexOf("=") + 1
                                torName = torName.Substring(tempIntTwo)
                                downloadLink = downloadLink.Substring(0, tempIntOne)
                                txtDebug.AppendText(vbNewLine + "Found torrent file """ + torName + """")
                                txtDebug.AppendText(vbNewLine + "Found torrent download link @ """ + downloadLink + """")
                                txtDebug.AppendText(vbNewLine + "Downloading: " + recieved(x))

                                Process.Start("chrome.exe", downloadLink)
                                Threading.Thread.Sleep(5000)

                                txtDebug.AppendText(vbNewLine + "Complete")
                                successDL = True
                                successResponse += recieved(x) + ", "
                            Catch
                                txtDebug.AppendText(vbNewLine + "Failed on: " + recieved(x))
                                failedDL = True
                                failResponse += recieved(x) + ", "

                            End Try
                        End If
                        badSearchPage = False
                    Next

                End If

                txtDebug.AppendText(vbNewLine + "Sleeping for 10 seconds...")
                Threading.Thread.Sleep(10000)

                If successDL Then
                    txtDebug.AppendText(vbNewLine + "Sending message """ + successResponse + """ to " + whitelist(tempCount) + " !")
                    sendMail(whitelist(tempCount), successResponse)
                End If

                If failedDL Then
                    txtDebug.AppendText(vbNewLine + "Sending message """ + failResponse + """ to " + whitelist(tempCount) + " !")
                    sendMail(whitelist(tempCount), failResponse)
                End If
            End If
        Next
        txtDebug.AppendText(vbNewLine + "All torrents completed!")
    End Sub

    Private Sub sendMail(reciever As String, reply As String)
        Dim client As New SmtpClient()
        Dim sendTo As New MailAddress(reciever)
        Dim from As MailAddress = New MailAddress(email)
        Dim message As New MailMessage(from, sendTo)

        message.IsBodyHtml = False
        message.Subject = ""
        message.Body = reply

        Dim basicAuthenticationInfo As New System.Net.NetworkCredential(email, password)

        client.Host = outgoing
        client.UseDefaultCredentials = False
        client.Credentials = basicAuthenticationInfo
        client.EnableSsl = True

        Try
            client.Send(message)
            txtDebug.AppendText(vbNewLine + "Sleeping for 5 seconds...")
            Threading.Thread.Sleep(5000)
            txtDebug.AppendText(vbNewLine + "Message sent to """ + reciever)

        Catch ex As Exception
            txtDebug.AppendText(vbNewLine + "Message not sent to " + reciever)

        End Try
    End Sub

    Private Function recieveMail()
        tempCount = -1
        didRecieve = False
        Dim client As OpenPop.Pop3.Pop3Client = New OpenPop.Pop3.Pop3Client()
        client.Connect(incoming, 995, True)
        client.Authenticate(email, password)
        messageCount = client.GetMessageCount()

        ReDim senderList(messageCount)
        ReDim recieved(messageCount)

        For count As Integer = 1 To messageCount
            Try
                For Each x In whitelist
                    tempCount += 1
                    If client.GetMessageHeaders(count).From.Address.Equals(x) Then
                        didRecieve = True
                        whitelistBool(tempCount) = True
                        senderList(count) = client.GetMessageHeaders(count).From.Address
                        recieved(count) = (client.GetMessage(count).ToMailMessage.Body)
                    End If
                Next
            Catch ex As OpenPop.Pop3.Exceptions.PopServerException
                    recieved(count) = Nothing
            End Try
            tempCount = -1
        Next

        For count As Integer = 1 To messageCount
            Try
                client.DeleteMessage(count)
            Catch ex As OpenPop.Pop3.Exceptions.PopServerException
            End Try
        Next

        client.Dispose()
        Return didRecieve
    End Function

    Private Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        tmrTest.Start()
        tmrCountdown.Start()
        tmrReboot.Start()
        btnStart.Enabled = False
        btnStop.Enabled = True
    End Sub

    Private Sub btnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        tmrTest.Stop()
        tmrCountdown.Stop()
        tmrReboot.Stop()
        txtCurAction.Text = Nothing
        btnStart.Enabled = True
        btnStop.Enabled = False
    End Sub

    Private Sub tmrCountdown_Tick(sender As Object, e As EventArgs) Handles tmrCountdown.Tick
        countDown -= 1
        rebootTime -= 1
        If countDown <> 0 Then
            txtCurAction.Text = countDown.ToString() + " seconds until next check!"
        Else
            txtCurAction.Text = "Checking..."
        End If

        txtReboot.Text = "Reboot: " + rebootTime.ToString() + "s"
    End Sub

    Private Function bestTorURL(ByVal urlArray() As String) As String
        Dim kingURL As String = ""

        txtDebug.AppendText(vbNewLine + "Comparing the first 3 torrents...")

        For Each x In urlArray
            If x <> Nothing Then
                'OPTIONAL FOR-EACH LOOP
                'HERE, YOU CAN SEARCH FOR KEYWORDS IN THE TOP 3 TORRENTS IF YOU'D LIKE
                'LOOK AT VAR x AND SET kingURL TO x IF IT'S FOUND
            End If
        Next

        If kingURL = "" Then
            txtDebug.AppendText(vbNewLine + "The torrent """ + urlArray(1) + """ is the new king!")
            kingURL = urlArray(0)
        End If

        Return kingURL
    End Function

    Private Sub Form1_Activated(sender As Object, e As EventArgs) Handles Me.Activated
        If firstActivate Then
            firstActivate = False
            firstLaunch = False

            'NEED .txt FILE CALLED "config" IN THE DIRECTORY OF THE BOT; LINE 1 SHOULD BE THE
            'BOT'S EMAIL AND LINE 2 THE EMAIL'S PASSWORD
            If My.Computer.FileSystem.FileExists("config.txt") Then
                Dim cfgReader As New System.IO.StreamReader("config.txt")
                email = cfgReader.ReadLine()
                password = cfgReader.ReadLine()
            Else
                MessageBox.Show("config.txt is missing. Please make it and on the first line, put the bot's email, and the second line, the bot's email's password.", "No Config", MessageBoxButtons.OK)
                Me.Close()
            End If

            'NEED .txt FILE CALLED "whitelist" IN THE DIRECTORY OF THE BOT; EACH LINE IN THE .txt FILE SHOULD
            'HAVE THE ADDRESS(ES) YOU'D LIKE WHITELISTED
            If My.Computer.FileSystem.FileExists("whitelist.txt") Then
                Dim count As Integer = 0
                For Each white As String In IO.File.ReadAllLines("whitelist.txt")
                    ReDim Preserve whitelist(count)
                    ReDim Preserve whitelistBool(count)
                    count += 1
                    whitelist(count - 1) = white
                    whitelistBool(count - 1) = False
                Next
            Else
                MessageBox.Show("whitelist.txt is missing. Please make it and, on each line, put the address of the account you'd like to whitelist", "No Whitelist", MessageBoxButtons.OK)
                Me.Close()
            End If

            'IF YOU MAKE A .txt FILE CALLED altorauto IN THE SAME DIRECTORY AS THE BOT THE
            'BOT WILL AUTO-RUN WHEN LAUCNHED
            If My.Computer.FileSystem.FileExists("altorauto.txt") Then
                For i As Integer = 0 To whitelistBool.Length - 1
                    whitelistBool(i) = False
                Next
                tmrReboot.Start()
                tmrTest.Start()
                tmrCountdown.Start()
                btnStart.Enabled = False
                btnStop.Enabled = True
            End If
        End If
    End Sub

    'Reboot every 12 hours; recommended you have altorauto.txt in the directory so the bot starts again
    Private Sub tmrReboot_Tick(sender As Object, e As EventArgs) Handles tmrReboot.Tick
        tmrReboot.Stop()
        tmrTest.Stop()
        tmrCountdown.Stop()
        btnStart.Enabled = False
        btnStop.Enabled = False

        Me.Text = "Rebooting..."

        Process.Start("Al Tor.exe")

        Me.Close()
    End Sub
End Class
