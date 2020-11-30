Class MainWindow

    Private Shared xlsxFile As EventXLSX
    Private Shared itemList As Collection
    Private Shared et As SCevent
    Private Shared _Rnd = -2
    Private Shared _TotalRnd
    Private Shared _Hth '健康
    Private Shared _Crd
    Private Shared _XueFen '学分
    Private Shared _O_M '欠债
    Private Shared _Srg
    Private Shared _NTC
    Private Shared _Row_Cnt
    Private Shared _Borrow_Max
    Private Shared _Borrow_Min
    Private Shared _Hos_Base_Mny
    Private Shared rateOfEvtMnyInc '事件加减钱增加率

    Private rateOfBorrowUnavb As Single
    Private rateOfOM As Single
    Private rateOfHos As Single
    Private rateOfHosHarm As Single
    Private rateOfHosNoOpend As Single

    Private Shared msgtxt As Collection
    Public Function messege(key As String)
        Return msgtxt(key)(Random(0, msgtxt(key).length - 1))
    End Function
    Private Sub Form_Closed(sender As Object, e As RoutedEventArgs) Handles Form.Closed
        xlsxFile.CloseFile()
    End Sub
    Private Sub BEGIN_Click(sender As Object, e As RoutedEventArgs) Handles BEGIN.Click
        If _Rnd = -2 Then
            Using sr As New IO.StreamReader("properties.txt")
                Dim line As String
                Dim f
                Do
                    line = sr.ReadLine()
                    f = Split(line, "=")
                    If Not (line Is Nothing) Then
                        Select Case f(0)
                            Case "RoundCount"
                                _Rnd = Int(f(1))
                                _TotalRnd = Int(f(1))
                            Case "Health"
                                _Hth = Int(f(1))
                            Case "Card"
                                _Crd = Int(f(1))
                            Case "Credit"
                                _XueFen = Int(f(1))
                            Case "Debt"
                                _O_M = Int(f(1))
                            Case "Storage"
                                _Srg = Int(f(1))
                            Case "RateOfDebt"
                                rateOfOM = Val(f(1))
                            Case "RateOfHospitalPrice"
                                rateOfHos = Val(f(1))
                            Case "RateOfHospitalDoesHarm"
                                rateOfHosHarm = Val(f(1))
                            Case "RateOfHospitalClosed"
                                rateOfHosNoOpend = Val(f(1))
                            Case "RowCount"
                                _Row_Cnt = Val(f(1))
                            Case "BorrowDebtMin"
                                _Borrow_Min = Val(f(1))
                            Case "BorrowDebtMax"
                                _Borrow_Max = Val(f(1))
                            Case "BorrowUnavailableRate"
                                rateOfBorrowUnavb = Val(f(1))
                            Case "HospitalBaseMoney"
                                _Hos_Base_Mny = Val(f(1))
                            Case "EventMoneyIncreaseRate"
                                rateOfEvtMnyInc = Val(f(1))
                        End Select
                    End If
                Loop Until line Is Nothing
            End Using

            Using sr As New IO.StreamReader("lang.txt")
                msgtxt = New Collection
                Dim line As String
                Dim f
                Do
                    line = sr.ReadLine()
                    f = Split(line, "=")
                    If Not (line Is Nothing) Then
                        Select Case f(0)
                            Case "HospitalUnavailable"
                                msgtxt.Add(Split(f(1), ";"), "HospitalUnavailable")
                            Case "HealthFull"
                                msgtxt.Add(Split(f(1), ";"), "HealthFull")
                            Case "HospitalDoesHarm"
                                msgtxt.Add(Split(f(1), ";"), "HospitalDoesHarm")
                            Case "HospitalCure"
                                msgtxt.Add(Split(f(1), ";"), "HospitalCure")
                            Case "HospitalNoMoney"
                                msgtxt.Add(Split(f(1), ";"), "HospitalNoMoney")
                            Case "BuyNothing"
                                msgtxt.Add(Split(f(1), ";"), "BuyNothing")
                            Case "SellNothing"
                                msgtxt.Add(Split(f(1), ";"), "SellNothing")
                            Case "BorrowUnavb"
                                msgtxt.Add(Split(f(1), ";"), "BorrowUnavb")
                            Case "BorrowSuccess"
                                msgtxt.Add(Split(f(1), ";"), "BorrowSuccess")
                        End Select
                    End If
                Loop Until line Is Nothing
            End Using

            _NTC = 0
            xlsxFile = New EventXLSX(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "events.xlsx")
            itemList = xlsxFile.ReadItems()
            Yes.IsEnabled = True
            No.IsEnabled = True
            Go_to_Hos.IsEnabled = True
            Buy.IsEnabled = True
            Sell.IsEnabled = True
            BEGIN.IsEnabled = True
            HZ_B.IsEnabled = True
            New_Round()
        Else
            Buy.IsEnabled = False
            Sell.IsEnabled = False
            New_Round()
        End If
    End Sub
    Public Class RndList
        Private c As List(Of RangeInt) = New List(Of RangeInt)
        Public Sub New(s As String)
            Dim spl = Split(s, ";")
            For Each a In spl
                c.Add(New RangeInt(a))
            Next
        End Sub
        Public Function GetVal(i As Integer)
            Return c(i)
        End Function
        Public Function Count()
            Return c.Count
        End Function
    End Class
    Public Class EventXLSX
        Private xlsxFile As Microsoft.Office.Interop.Excel.Application
        Private eventList As List(Of Integer) = New List(Of Integer)
        Private maxLine As Integer
        Public eventCount As Integer
        Public Sub New(filename As String)
            xlsxFile = New Microsoft.Office.Interop.Excel.Application With {
                .Visible = False
            }
            xlsxFile.Workbooks.Open(filename)
            Dim i As Integer
            i = 2
            maxLine = _Row_Cnt
            While (i <= maxLine)
                If (Not (Read(1, i, "A") = "")) Then
                    eventList.Add(i)
                End If
                i = i + 1
            End While
            eventCount = eventList.Count
        End Sub
        Private Function Read(sheet As Integer, Line As Integer, Row As String) As String
            Return xlsxFile.Sheets("Sheet" + sheet.ToString).Range(Row + Line.ToString).Value
        End Function
        Public Function ReadEvent(e As Integer) As SCevent
            Dim sce As SCevent = New SCevent
            Dim ln As Integer = eventList(e - 1)
            sce.txt = Read(1, ln, "A")
            If ln = eventList.Last Then
                While (ln <= maxLine)
                    AddFromLine(sce, ln)
                    ln = ln + 1
                End While
            Else
                While (ln < eventList(e))
                    AddFromLine(sce, ln)
                    ln = ln + 1
                End While
            End If
            Return sce
        End Function
        Public Function ReadItems() As Collection
            Dim i As Integer = 2
            Dim c As Collection = New Collection
            While (Not (Read(2, i, "A") = ""))
                Dim p As Integer = Random(1, 100)
                If p > Int(Read(2, i, "C")) Then
                    c.Add(New ITEM(Read(2, i, "A"), New RangeInt(Read(2, i, "B")).Rand, 0), (i - 1).ToString)
                Else
                    c.Add(New ITEM(Read(2, i, "A"), -1, 0), (i - 1).ToString)
                End If
                i = i + 1
            End While
            Return c
        End Function
        Public Function RefreshItems(il As Collection) As Collection
            Dim i As Integer = 2
            Dim c As Collection = New Collection
            Dim itm As ITEM
            While (i - 1 <= il.Count)
                itm = il.Item((i - 1).ToString)
                Dim p As Integer = Random(1, 100)
                Dim origPrice As RangeInt = New RangeInt(Read(2, i, "B"))
                Dim maxPrice, minPrice As Integer
                If p > Int(Read(2, i, "C")) Then
                    If origPrice.max < itm.price + Int(Read(2, i, "D")) Then
                        maxPrice = origPrice.max
                    Else
                        maxPrice = itm.price + Read(2, i, "D")
                    End If
                    If origPrice.min > itm.price - Int(Read(2, i, "D")) Then
                        minPrice = origPrice.min
                    Else
                        minPrice = itm.price - Read(2, i, "D")
                    End If
                    itm.price = Random(minPrice.ToString, maxPrice.ToString, True)
                Else
                    itm.price = -1
                End If
                c.Add(itm, i - 1)
                i = i + 1
            End While
            Return c
        End Function
        Private Sub AddFromLine(sce As SCevent, ln As Integer)
            sce.Y.Add(Read(1, ln, "B"))
            sce.N.Add(Read(1, ln, "C"))
            sce.MoneyY.Add(New RangeInt(Read(1, ln, "D")))
            sce.MoneyN.Add(New RangeInt(Read(1, ln, "E")))
            sce.CardY.Add(New RangeInt(Read(1, ln, "F")))
            sce.CardN.Add(New RangeInt(Read(1, ln, "G")))
            sce.HealthY.Add(New RangeInt(Read(1, ln, "H")))
            sce.HealthN.Add(New RangeInt(Read(1, ln, "I")))
            sce.O_M_Y.Add(New RangeInt(Read(1, ln, "J")))
            sce.O_M_N.Add(New RangeInt(Read(1, ln, "K")))
            sce.N_T_Y.Add(New RangeInt(Read(1, ln, "L")))
            sce.N_T_N.Add(New RangeInt(Read(1, ln, "M")))
            sce.RcvItmY.Add(New RndList(Read(1, ln, "N")))
            sce.RcvItmN.Add(New RndList(Read(1, ln, "O")))
            sce.RcvY.Add(New RndList(Read(1, ln, "P")))
            sce.RcvN.Add(New RndList(Read(1, ln, "Q")))
            sce.ChangeID.Add(New RndList(Read(1, ln, "R")))
            sce.ChgAOM.Add(New RndList(Read(1, ln, "S")))
        End Sub
        Public Sub CloseFile()
            xlsxFile.Quit()
        End Sub
    End Class
    Public Class SCevent
        Public txt As String
        Public Y As List(Of String)
        Public N As List(Of String)
        Public MoneyY As List(Of RangeInt)
        Public MoneyN As List(Of RangeInt)
        Public CardY As List(Of RangeInt)
        Public CardN As List(Of RangeInt)
        Public HealthY As List(Of RangeInt)
        Public HealthN As List(Of RangeInt)
        Public O_M_Y As List(Of RangeInt)
        Public O_M_N As List(Of RangeInt)
        Public N_T_Y As List(Of RangeInt)
        Public N_T_N As List(Of RangeInt)
        Public RcvItmY As List(Of RndList)
        Public RcvItmN As List(Of RndList)
        Public RcvY As List(Of RndList)
        Public RcvN As List(Of RndList)
        Public ChangeID As List(Of RndList)
        Public ChgAOM As List(Of RndList)
        Public Sub New()
            txt = ""
            Y = New List(Of String)
            N = New List(Of String)
            MoneyY = New List(Of RangeInt)
            MoneyN = New List(Of RangeInt)
            CardY = New List(Of RangeInt)
            CardN = New List(Of RangeInt)
            HealthY = New List(Of RangeInt)
            HealthN = New List(Of RangeInt)
            O_M_Y = New List(Of RangeInt)
            O_M_N = New List(Of RangeInt)
            N_T_Y = New List(Of RangeInt)
            N_T_N = New List(Of RangeInt)
            RcvItmY = New List(Of RndList)
            RcvItmN = New List(Of RndList)
            RcvY = New List(Of RndList)
            RcvN = New List(Of RndList)
            ChangeID = New List(Of RndList)
            ChgAOM = New List(Of RndList)
        End Sub
    End Class
    Public Class ITEM
        Public name As String
        Public price As Integer
        Public ifOwned As Integer
        Public Sub New(n As String, p As Integer, i As Integer)
            name = n
            price = p
            ifOwned = i
        End Sub
    End Class
    Public Class RangeInt
        Public min As Integer
        Public max As Integer
        Public Function Rand(Optional is_BM As Boolean = False) As Integer
            Return Random(min, max, is_BM)
        End Function
        Public Sub New(src As String)
            If src = "" Then
                src = "0"
            End If
            Dim spl = Split(src, ",")
            If spl.Count = 1 Then
                min = spl(0)
                max = spl(0)
            Else
                min = spl(0)
                max = spl(1)
            End If
        End Sub
    End Class
    Public Shared Function Random(min As Integer, max As Integer, Optional is_BM As Boolean = False) As Integer
        Dim r
        If (is_BM) Then
            Randomize((Now().Second() + Now().Millisecond() + Rnd().GetHashCode() - Now().Minute()).GetHashCode Mod (Now().Second() + Now().Millisecond().GetHashCode()))
            Dim u = Rnd()
            Randomize((Now().Second() + Now().Millisecond() + Rnd().GetHashCode() - Now().Minute()).GetHashCode Mod (Now().Second() + Now().Millisecond().GetHashCode()))
            Dim v = Rnd()
            r = (Math.Sqrt(-2 * Math.Log(u) * Math.Cos(2 * Math.PI * v)))
        End If
        Randomize((Now().Second() + Now().Millisecond() + Rnd().GetHashCode() - Now().Minute()).GetHashCode Mod (Now().Second() + Now().Millisecond().GetHashCode()))
        r = Rnd()
        Return Int(r * (max - min + 1)) + min
    End Function
    Public Sub New_Round()
        _O_M = Int(_O_M * rateOfOM)
        ItmLst.IsEnabled = False
        HZ_B.IsEnabled = False
        Go_to_Hos.IsEnabled = False
        Borrow.IsEnabled = False
        Sell.IsEnabled = False
        Buy.IsEnabled = False
        If _XueFen <= 0 Or _Hth <= 0 Then
            _Rnd = -1
            Refresh_Stats()
            If _XueFen <= 0 Then
                MsgBox("你被退学了")
                MsgBox("你的校园生活是你人生道路上一个重要的里程碑，这短暂而又充实的一瞬，你拥有了" + _Crd.ToString + "RMB的资金，欠了小混混" + _O_M.ToString + "RMB的债务。")
                End
            End If
            If _Hth <= 0 Then
                MsgBox("你去世了")
                MsgBox("你拥有的" + _Crd.ToString + "RMB现金成为了毫无意义的数字。小混混再也追不回你欠他" + _O_M.ToString + "RMB的债务。")
                End
            End If
            Yes.IsEnabled = False
            No.IsEnabled = False
            Go_to_Hos.IsEnabled = False
            Buy.IsEnabled = False
            Sell.IsEnabled = False
            HZ_B.IsEnabled = False
            BEGIN.IsEnabled = True
            Borrow.IsEnabled = False
            _Rnd = -2
        Else
            If _Rnd < 1 Then
                Refresh_Stats()
                MsgBox("学期结束了")
                MsgBox("你拥有" + _Crd.ToString + "RMB现金，侥幸没有落入他人的口袋。你这一学期的行径并没有足以达到使你被退学的程度。小混混下个学期还会向你要" + _O_M.ToString + "RMB的债务。")
                End
                _Rnd = -2
                Yes.IsEnabled = False
                No.IsEnabled = False
                Go_to_Hos.IsEnabled = False
                Buy.IsEnabled = False
                Sell.IsEnabled = False
                HZ_B.IsEnabled = False
                BEGIN.IsEnabled = True
            End If
            BEGIN.IsEnabled = False
            Yes.IsEnabled = True
            No.IsEnabled = True
            Buy.IsEnabled = False
            Sell.IsEnabled = False
            Go_to_Hos.IsEnabled = False
            et = xlsxFile.ReadEvent(Random(1, xlsxFile.eventCount))
            itemList = xlsxFile.RefreshItems(itemList)
            EventText.Text = et.txt
            If et.N(0) = "" Then
                No.IsEnabled = False
            Else
                No.IsEnabled = True
            End If
            If _NTC > 0 Then
                _NTC = _NTC - 1
                Buy.IsEnabled = False
                Sell.IsEnabled = False
            End If
            _Rnd -= 1
            Refresh_Stats()
        End If
    End Sub
    Private Sub Yes_Click(sender As Object, e As RoutedEventArgs) Handles Yes.Click
        ItmLst.IsEnabled = True
        HZ_B.IsEnabled = True
        Go_to_Hos.IsEnabled = True
        Borrow.IsEnabled = True
        Sell.IsEnabled = True
        Buy.IsEnabled = True
        Dim i = Random(0, et.Y.Count - 1)
        Dim d_Hth As Integer = et.MoneyY(i).Rand()
        Dim d_Crd As Integer = et.CardY(i).Rand() * (rateOfEvtMnyInc ^ (_TotalRnd - _Rnd))
        Dim d_XueFen As Integer = et.HealthY(i).Rand
        _Hth = _Hth + d_Hth
        _Crd = _Crd + d_Crd
        _XueFen = _XueFen + d_XueFen
        _O_M = _O_M + et.O_M_Y(i).Rand
        _NTC = _NTC + et.N_T_Y(i).Rand
        Dim msg As String = ""
        If d_Hth > 0 Then
            msg = "健康+" + d_Hth.ToString + vbCrLf
        ElseIf d_Hth < 0 Then
            msg = "健康" + d_Hth.ToString + vbCrLf
        End If
        If d_Crd > 0 Then
            msg += "校园卡+" + d_Crd.ToString + vbCrLf
        ElseIf d_Crd < 0 Then
            msg += "校园卡" + d_Crd.ToString + vbCrLf
        End If
        If d_XueFen > 0 Then
            msg += "学分+" + d_XueFen.ToString + vbCrLf
        ElseIf d_XueFen < 0 Then
            msg += "学分" + d_XueFen.ToString + vbCrLf
        End If
        If (et.Y(i) <> "") Then
            MsgBox(et.Y(i))
        End If
        If et.RcvItmY(i).GetVal(0).max <> 0 Then
            Dim avb As List(Of Integer) = New List(Of Integer)
            For p As Integer = 0 To et.RcvItmY(i).Count - 1
                Dim itmLeft As Integer = et.RcvY(i).GetVal(p).Rand()
                Dim contFlag As Boolean = True
                For q As Integer = et.RcvItmY(i).GetVal(p).min To et.RcvItmY(i).GetVal(p).max
                    If itemList.Item(q.ToString).ifOwned > 0 Then
                        avb.Add(q)
                    End If
                Next
                While (itmLeft <> 0 And contFlag)
                    If avb.Count > 0 Or et.RcvY(i).GetVal(p).min >= 0 Then
                        Dim j As Integer
                        If et.RcvY(i).GetVal(p).min < 0 Then
                            j = avb(Random(0, avb.Count - 1))
                        Else
                            j = et.RcvItmY(i).GetVal(p).Rand().ToString
                        End If
                        'Dim itmstr As String = et.RcvItmY(i).GetVal(j).Rand().ToString
                        Dim tmp1 As ITEM = itemList.Item(j.ToString)
                        itemList.Remove(j.ToString)
                        Dim rndval As Integer = itmLeft
                        If _Srg - rndval < 0 Then
                            rndval = _Srg
                            MsgBox("你柜子太小，放不下了")
                            contFlag = False
                        End If
                        If tmp1.ifOwned + rndval < 0 Then
                            rndval = -tmp1.ifOwned
                            tmp1.ifOwned = 0
                            itemList.Add(tmp1, j.ToString)
                        Else
                            tmp1.ifOwned = tmp1.ifOwned + rndval
                            itemList.Add(tmp1, j.ToString)
                        End If
                        If rndval > 0 Then
                            msg = msg + (vbCrLf & "你获得了" + tmp1.name + "*" + rndval.ToString)
                        ElseIf rndval < 0 Then
                            msg = msg + (vbCrLf & "你丢失了" + tmp1.name + "*" + (-rndval).ToString)
                        End If
                        _Srg = _Srg - rndval
                        itmLeft = itmLeft - rndval
                        avb.Remove(j)
                    Else
                        contFlag = False
                    End If
                End While
            Next
        End If
        If msg <> "" Then
            MsgBox(msg)
        End If
        If et.ChangeID(i).GetVal(0).max > 0 Then
            For j As Integer = 0 To et.ChangeID(i).Count - 1
                For k As Integer = et.ChangeID(i).GetVal(j).min To et.ChangeID(i).GetVal(j).max
                    Dim tmp2 As ITEM = itemList.Item(k.ToString)
                    itemList.Remove(k.ToString)
                    Dim rndval As Integer = et.ChgAOM(i).GetVal(j).Rand()
                    tmp2.price = rndval
                    itemList.Add(tmp2, k.ToString)
                Next
            Next
        End If
        Yes.IsEnabled = False
        No.IsEnabled = False
        BEGIN.IsEnabled = True
        Refresh_Stats()
    End Sub
    Private Sub No_Click(sender As Object, e As RoutedEventArgs) Handles No.Click
        ItmLst.IsEnabled = True
        HZ_B.IsEnabled = True
        Go_to_Hos.IsEnabled = True
        Borrow.IsEnabled = True
        Sell.IsEnabled = True
        Buy.IsEnabled = True
        Dim i = Random(0, et.N.Count - 1)
        Dim d_Hth As Integer = et.MoneyN(i).Rand()
        Dim d_Crd As Integer = et.CardN(i).Rand() * (rateOfEvtMnyInc ^ (_TotalRnd - _Rnd))
        Dim d_XueFen As Integer = et.HealthN(i).Rand
        _Hth = _Hth + d_Hth
        _Crd = _Crd + d_Crd
        _XueFen = _XueFen + d_XueFen
        _O_M = _O_M + et.O_M_N(i).Rand
        _NTC = _NTC + et.N_T_N(i).Rand
        Dim msg As String = ""
        If d_Hth > 0 Then
            msg = "健康+" + d_Hth.ToString + vbCrLf
        ElseIf d_Hth < 0 Then
            msg = "健康" + d_Hth.ToString + vbCrLf
        End If
        If d_Crd > 0 Then
            msg += "校园卡+" + d_Crd.ToString + vbCrLf
        ElseIf d_Crd < 0 Then
            msg += "校园卡" + d_Crd.ToString + vbCrLf
        End If
        If d_XueFen > 0 Then
            msg += "学分+" + d_XueFen.ToString + vbCrLf
        ElseIf d_XueFen < 0 Then
            msg += "学分" + d_XueFen.ToString + vbCrLf
        End If
        If (et.N(i) <> "") Then
            MsgBox(et.N(i))
        End If
        If et.RcvItmN(i).GetVal(0).max <> 0 Then
            Dim avb As List(Of Integer) = New List(Of Integer)
            For p As Integer = 0 To et.RcvItmN(i).Count - 1
                Dim itmLeft As Integer = et.RcvN(i).GetVal(p).Rand()
                Dim contFlag As Boolean = True
                For q As Integer = et.RcvItmN(i).GetVal(p).min To et.RcvItmN(i).GetVal(p).max
                    If itemList.Item(q.ToString).ifOwned > 0 Then
                        avb.Add(q)
                    End If
                Next
                While (itmLeft <> 0 And contFlag)
                    If avb.Count > 0 Or et.RcvN(i).GetVal(p).min >= 0 Then
                        Dim j As Integer
                        If et.RcvN(i).GetVal(p).min < 0 Then
                            j = avb(Random(0, avb.Count - 1))
                        Else
                            j = et.RcvItmN(i).GetVal(p).Rand().ToString
                        End If
                        'Dim itmstr As String = et.RcvItmN(i).GetVal(j).Rand().ToString
                        Dim tmp1 As ITEM = itemList.Item(j.ToString)
                        itemList.Remove(j.ToString)
                        Dim rndval As Integer = itmLeft
                        If _Srg - rndval < 0 Then
                            rndval = _Srg
                            MsgBox("你柜子太小，放不下了")
                            contFlag = False
                        End If
                        If tmp1.ifOwned + rndval < 0 Then
                            rndval = -tmp1.ifOwned
                            tmp1.ifOwned = 0
                            itemList.Add(tmp1, j.ToString)
                        Else
                            tmp1.ifOwned = tmp1.ifOwned + rndval
                            itemList.Add(tmp1, j.ToString)
                        End If
                        If rndval > 0 Then
                            msg = msg + (vbCrLf & "你获得了" + tmp1.name + "*" + rndval.ToString)
                        ElseIf rndval < 0 Then
                            msg = msg + (vbCrLf & "你丢失了" + tmp1.name + "*" + (-rndval).ToString)
                        End If
                        _Srg = _Srg - rndval
                        itmLeft = itmLeft - rndval
                        avb.Remove(j)
                    Else
                        contFlag = False
                    End If
                End While
            Next
        End If
        If msg <> "" Then
            MsgBox(msg)
        End If
        If et.ChangeID(i).GetVal(0).max > 0 Then
            For j As Integer = 0 To et.ChangeID(i).Count - 1
                For k As Integer = et.ChangeID(i).GetVal(j).min To et.ChangeID(i).GetVal(j).max
                    Dim tmp2 As ITEM = itemList.Item(k.ToString)
                    itemList.Remove(k.ToString)
                    Dim rndval As Integer = et.ChgAOM(i).GetVal(j).Rand()
                    tmp2.price = rndval
                    itemList.Add(tmp2, k.ToString)
                Next
            Next
        End If
        Yes.IsEnabled = False
        No.IsEnabled = False
        BEGIN.IsEnabled = True
        Refresh_Stats()
    End Sub
    Private Sub Refresh_Stats()
        If _Hth > 100 Then
            _Hth = 100
        End If
        If _O_M < 0 Then
            _O_M = 0
        End If
        If _Crd < 0 Then
            _Crd = 0
        End If
        ItmLst.Items.Clear()
        Dim i As Integer = 1
        Dim itm As ITEM
        While (i <= itemList.Count)
            itm = itemList.Item(i.ToString)
            If itm.price = -1 Then
                ItmLst.Items.Add(itm.name + ":交易监管:" + itm.ifOwned.ToString)
            Else
                ItmLst.Items.Add(itm.name + ":" + itm.price.ToString + ":" + itm.ifOwned.ToString)
            End If
            i = i + 1
        End While
        Round.Content = _Rnd.ToString
        Money.Content = _Hth.ToString
        Card.Content = _Crd.ToString
        Health.Content = _XueFen.ToString
        Owed_Money.Content = _O_M.ToString
        Storage.Content = _Srg.ToString
        If _O_M < _Crd Then
            HZ.Maximum = Int(_O_M / 500) + 1
        Else
            HZ.Maximum = Int(_Crd / 500) + 1
        End If
        If (_Hth <= 0 Or _XueFen <= 0) And _Rnd >= 0 Then
            New_Round()
        End If
    End Sub
    Private Sub ItmLst_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles ItmLst.SelectionChanged
        Buy.IsEnabled = True
        Sell.IsEnabled = True
        Dim sit As String = ItmLst.SelectedValue
        If sit = Nothing Then
            Sld_Buy.Maximum = 0
            Sld_Sell.Maximum = 0
        Else
            Sld_Sell.Maximum = Int(Split(sit, ":")(2))
            If Split(sit, ":")(1) = "交易监管" Then
                Sld_Buy.Maximum = 0
                Buy.IsEnabled = False
                Sell.IsEnabled = False
            Else
                Buy.IsEnabled = True
                Sell.IsEnabled = True
                Sld_Buy.Maximum = Math.Min(Int(_Crd / Int(Split(sit, ":")(1))), _Srg)
            End If
        End If
    End Sub
    Private Sub HZ_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles HZ.ValueChanged
        HZ_T.Content = (Int(HZ.Value) * 500).ToString
        If Int(HZ_T.Content) > _O_M Then
            HZ_T.Content = _O_M.ToString
        End If
        If Int(HZ_T.Content) > _Crd Then
            HZ_T.Content = _Crd
        End If
    End Sub
    Private Sub HZ_B_Click(sender As Object, e As RoutedEventArgs) Handles HZ_B.Click
        _O_M = _O_M - Int(HZ_T.Content)
        _Crd -= Int(HZ_T.Content)
        MsgBox("你还了" + Str(HZ_T.Content) + "元")
        Refresh_Stats()
        HZ_B.IsEnabled = True
    End Sub
    Private Sub Go_to_Hos_Click(sender As Object, e As RoutedEventArgs) Handles Go_to_Hos.Click
        If (Random(1, 100) <= rateOfHosNoOpend) Then
            Go_to_Hos.IsEnabled = False
            MsgBox(messege("HospitalUnavailable"))
            Return
        End If
        If (_Hth = 100) Then
            Go_to_Hos.IsEnabled = False
            MsgBox(messege("HealthFull"))
            Return
        End If
        Dim addHealth As Integer = Random(30, 100)
        Dim cost As Integer = Int(_Hos_Base_Mny * (rateOfHos ^ (_TotalRnd - _Rnd)))
        If cost < _Crd Then
            If Random(0, 100) <= rateOfHosHarm Then
                Dim rnd As Integer = Random(0, 20)
                _Hth = _Hth - rnd
                MsgBox(messege("HospitalDoesHarm") + "-" + rnd.ToString + "健康")
            Else
                _Hth = _Hth + addHealth
                If _Hth > 100 Then
                    _Hth = 100
                End If
                _Crd -= cost
                MsgBox(messege("HospitalCure"))
            End If
            Refresh_Stats()
        Else
            MsgBox(messege("HospitalNoMoney"))
        End If
        Go_to_Hos.IsEnabled = False
        Refresh_Stats()
    End Sub
    Private Sub Buy_Click(sender As Object, e As RoutedEventArgs) Handles Buy.Click
        If ItmLst.SelectedValue Is Nothing Then
            MsgBox(messege("BuyNothing"))
            Return
        End If
        If _NTC > 0 Then
            MsgBox("你被禁止交易，剩余" + _NTC.ToString + "回合")
            Return
        End If
        Dim s = Split(ItmLst.SelectedValue, ":")
        If Not (s(1) = "交易监管") Then
            _Crd = _Crd - Int(Lbl_Buy.Content) * Int(s(1))
            Dim j As Integer = 0
            Dim tmp1 As ITEM = New ITEM(0, 0, 0)
            While Not (tmp1.name = s(0))
                j = j + 1
                tmp1 = itemList.Item(j.ToString)
            End While
            itemList.Remove(j.ToString)
            tmp1.ifOwned = tmp1.ifOwned + Int(Lbl_Buy.Content)
            _Srg = _Srg - Int(Lbl_Buy.Content)
            itemList.Add(tmp1, j.ToString)
            Refresh_Stats()
        End If
    End Sub
    Private Sub Sell_Click(sender As Object, e As RoutedEventArgs) Handles Sell.Click
        If ItmLst.SelectedValue Is Nothing Then
            MsgBox(messege("SellNothing"))
            Return
        End If
        If _NTC > 0 Then
            MsgBox("你被禁止交易，剩余" + _NTC.ToString + "回合")
            Return
        End If
        Dim s = Split(ItmLst.SelectedValue, ":")
        If (s(1) = "0") Then
            Return
        ElseIf Not (s(1) = "交易监管") Then
            _Crd = _Crd + Int(Lbl_Sell.Content) * Int(Split(ItmLst.SelectedValue, ":")(1))
            Dim j As Integer = 0
            Dim tmp1 As ITEM = New ITEM(0, 0, 0)
            While Not (tmp1.name = s(0))
                j = j + 1
                tmp1 = itemList.Item(j.ToString)
            End While
            itemList.Remove(j.ToString)
            tmp1.ifOwned = tmp1.ifOwned - Int(Lbl_Sell.Content)
            _Srg = _Srg + Int(Lbl_Sell.Content)
            itemList.Add(tmp1, j.ToString)
            Refresh_Stats()
        End If
    End Sub
    Private Sub Sld_Buy_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles Sld_Buy.ValueChanged
        Lbl_Buy.Content = Int(Sld_Buy.Value).ToString
        If _Srg < Int(Sld_Buy.Value).ToString Then
            Lbl_Buy.Content = _Srg.ToString
        End If
    End Sub
    Private Sub Sld_Sell_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double)) Handles Sld_Sell.ValueChanged
        Lbl_Sell.Content = Int(Sld_Sell.Value).ToString
    End Sub
    Private Sub Borrow_Click(sender As Object, e As RoutedEventArgs) Handles Borrow.Click
        If (Random(0, 100) < rateOfBorrowUnavb) Then
            MsgBox(messege("BorrowUnavb"))
            Borrow.IsEnabled = False
            Return
        End If
        Dim mny As Integer = Random(_Borrow_Min, _Borrow_Max)
        _Crd += mny
        _O_M += mny
        Refresh_Stats()
        MsgBox(messege("BorrowSuccess") + vbCrLf + "你获得了" + mny.ToString + "现金以及债务")
        Borrow.IsEnabled = False
        HZ_B.IsEnabled = False
    End Sub '借款
End Class