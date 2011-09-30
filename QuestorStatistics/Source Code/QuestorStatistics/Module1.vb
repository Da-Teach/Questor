Module Module1
    Public Function GraficaMes(ByVal Seleccion As String)
        Dim GananciaTotal = 0
        Dim TotalDia = 0
        Dim SelectDia As String
        Dim AntDia As String = ""
        ListadoDown()
        FrmMain.CHMes.Series(0).Points.Clear()
        For i = 0 To FrmMain.Lst1.Items.Count - 1
            If Seleccion = Mid(FrmMain.Lst1.Items.Item(i).SubItems(1).Text, 4, 7) Then
                SelectDia = Left(FrmMain.Lst1.Items.Item(i).SubItems(1).Text, 10)

                For e = 0 To FrmMain.Lst1.Items.Count - 1
                    If SelectDia = Left(FrmMain.Lst1.Items.Item(e).SubItems(1).Text, 10) Then
                        GananciaTotal = GananciaTotal + CDbl(FrmMain.Lst1.Items.Item(e).SubItems(4).Text) + CDbl(FrmMain.Lst1.Items.Item(e).SubItems(6).Text)
                    End If
                Next

                If AntDia <> Left(FrmMain.Lst1.Items.Item(i).SubItems(1).Text, 10) Then
                    FrmMain.CHMes.Series(0).Points.AddXY(Left(FrmMain.Lst1.Items.Item(i).SubItems(1).Text, 10), GananciaTotal)
                    AntDia = Left(FrmMain.Lst1.Items.Item(i).SubItems(1).Text, 10)
                End If
                GananciaTotal = 0
            End If
        Next
        ListadoUP()
        Return True
    End Function
    Public Function Mes(ByVal Seleccion As String)

        Dim TotalBounty As Int64 = 0
        Dim TotalLoot As Int64 = 0
        Dim Nmission = 0
        Dim TotalLP = 0
        Dim cont1 = 0
        Dim cont2 = 0
        Dim var1 = ""

        For i = 0 To FrmMain.Lst1.Items.Count - 1
            If Seleccion = Mid(FrmMain.Lst1.Items.Item(i).SubItems(1).Text, 4, 7) Then
                TotalBounty = TotalBounty + CDbl(FrmMain.Lst1.Items.Item(i).SubItems(4).Text)
                TotalLoot = TotalLoot + CDbl(FrmMain.Lst1.Items.Item(i).SubItems(6).Text)
                TotalLP = TotalLP + CDbl(FrmMain.Lst1.Items.Item(i).SubItems(8).Text)
                Nmission = Nmission + 1
                cont1 = cont1 + 1
            End If
            If Seleccion = Mid(FrmMain.Lst1.Items.Item(i).SubItems(1).Text, 4, 7) Then
                If var1 <> Left(FrmMain.Lst1.Items.Item(i).SubItems(1).Text, 10) Then
                    var1 = Left(FrmMain.Lst1.Items.Item(i).SubItems(1).Text, 10)
                    cont2 = cont2 + 1
                End If
            End If
        Next

        FrmMain.lbttMbounty.Text = Format(TotalBounty, "###,###,###")
        FrmMain.LbttMloot.Text = Format(TotalLoot, "###,###,###")
        FrmMain.LbttMlp.Text = Format(TotalLP, "###,###,###")
        FrmMain.LbttMnmision.Text = Format(Nmission, "###,###,###")
        FrmMain.lbttMganancia.Text = Format(TotalBounty + TotalLoot, "###,###,###,###")
        FrmMain.LbttMmedia.Text = Format((TotalBounty + TotalLoot) / cont2, "###,###,###,###")

        Return True
    End Function
    Public Function GraficaDia(ByVal Seleccion As String)
        ListadoDown()
        FrmMain.CHDia.Series(0).Points.Clear()
        For i = 0 To FrmMain.Lst1.Items.Count - 1
            If Seleccion = Left(FrmMain.Lst1.Items.Item(i).SubItems(1).Text, 10) Then
                FrmMain.CHDia.Series(0).Points.AddXY(FrmMain.Lst1.Items.Item(i).SubItems(1).Text, FrmMain.Lst1.Items.Item(i).SubItems(4).Text)
            End If
        Next
        FrmMain.CHDia.Series(1).Points.Clear()
        For i = 0 To FrmMain.Lst1.Items.Count - 1
            If Seleccion = Left(FrmMain.Lst1.Items.Item(i).SubItems(1).Text, 10) Then
                FrmMain.CHDia.Series(1).Points.AddXY(FrmMain.Lst1.Items.Item(i).SubItems(1).Text, FrmMain.Lst1.Items.Item(i).SubItems(6).Text)
            End If
        Next
        ListadoUP()
        Return True
    End Function
    Public Function Dia(ByVal Seleccion As String)

        Dim TotalBounty = 0
        Dim TotalLoot = 0
        Dim Nmission = 0
        Dim TotalLP = 0
        Dim cont1 = 0

        For i = 0 To FrmMain.Lst1.Items.Count - 1
            If Seleccion = Left(FrmMain.Lst1.Items.Item(i).SubItems(1).Text, 10) Then
                TotalBounty = TotalBounty + CDbl(FrmMain.Lst1.Items.Item(i).SubItems(4).Text)
                TotalLoot = TotalLoot + CDbl(FrmMain.Lst1.Items.Item(i).SubItems(6).Text)
                TotalLP = TotalLP + CDbl(FrmMain.Lst1.Items.Item(i).SubItems(8).Text)
                Nmission = Nmission + 1
                cont1 = cont1 + 1
            End If
        Next
        FrmMain.lbTotalbounty.Text = Format(TotalBounty, "###,###,###")
        FrmMain.LBtotalloot.Text = Format(TotalLoot, "###,###,###")
        FrmMain.LBtotallp.Text = Format(TotalLP, "###,###,###")
        FrmMain.LBNmision.Text = Format(Nmission, "###,###,###")
        FrmMain.LBGanacia.Text = Format(TotalBounty + TotalLoot, "###,###,###")
        FrmMain.lbmediatotal.Text = Format((TotalBounty + TotalLoot) / cont1, "###,###,###")
        Return True
    End Function
    Public Function StatsMision()
        Dim Mision As String
        Dim Time As Long
        Dim Bounty As Long
        Dim Loot As Long
        Dim Lp As Long
        Dim Iskmedia As Long
        Dim Nmision As Long
        Dim Tmision As Long = FrmMain.Lst1.Items.Count
        Dim var1 As Boolean = False
        FrmMain.LstMision.Items.Clear()

        For i = 0 To FrmMain.Lst1.Items.Count - 1
            Mision = FrmMain.Lst1.Items.Item(i).SubItems(2).Text
            Nmision = 0
            Time = 0
            Bounty = 0
            Loot = 0
            Lp = 0
            Iskmedia = 0
            For e = 0 To FrmMain.Lst1.Items.Count - 1
                If Mision = FrmMain.Lst1.Items.Item(e).SubItems(2).Text Then
                    Nmision = Nmision + 1
                    Time = Time + FrmMain.Lst1.Items.Item(e).SubItems(3).Text
                    Bounty = Bounty + FrmMain.Lst1.Items.Item(e).SubItems(4).Text
                    Loot = Loot + FrmMain.Lst1.Items.Item(e).SubItems(6).Text
                    Lp = Lp + FrmMain.Lst1.Items.Item(e).SubItems(8).Text
                    Iskmedia = Iskmedia + FrmMain.Lst1.Items.Item(e).SubItems(10).Text
                End If
            Next

            For o = 0 To FrmMain.LstMision.Items.Count - 1
                If Mision = FrmMain.LstMision.Items.Item(o).SubItems(1).Text Then
                    var1 = True
                    Exit For
                Else
                    var1 = False
                End If
            Next
            If var1 = False Then
                Dim item1 As New ListViewItem("")
                With item1
                    .SubItems.Add(Mision)
                    .SubItems.Add(Format(CDbl((Nmision / Tmision) * 100), "0.#"))
                    .SubItems.Add(Format(CDbl(Time / Nmision), "###,###,##0"))
                    .SubItems.Add(Format(CDbl(Bounty / Nmision), "###,###,##0"))
                    .SubItems.Add(Format(CDbl(Loot / Nmision), "###,###,##0"))
                    .SubItems.Add(Format(CDbl(Lp / Nmision), "###,###,##0"))
                    .SubItems.Add(Format(CDbl(Iskmedia / Nmision), "###,###,##0"))
                    FrmMain.LstMision.Items.Add(item1)
                End With
                var1 = True
            End If
        Next
        Colocarmisiones()
        Return True

    End Function

    Public Function Precarga()
        Dim SelectDia As String
        Dim SelectMes As String
        Dim var1 As String = ""

        ListadoUP()
        FrmMain.CmbDia.Items.Clear()
        For i = 0 To FrmMain.Lst1.Items.Count - 1
            SelectDia = FrmMain.Lst1.Items.Item(i).SubItems(1).Text

            SelectDia = Left(SelectDia, 10)

            If var1 <> SelectDia Then

                FrmMain.CmbDia.Items.Add(SelectDia)
                var1 = SelectDia
            End If
        Next

        FrmMain.cmbMes.Items.Clear()
        For i = 0 To FrmMain.Lst1.Items.Count - 1
            SelectMes = FrmMain.Lst1.Items.Item(i).SubItems(1).Text

            SelectMes = Mid(SelectMes, 4, 7)

            If var1 <> SelectMes Then

                FrmMain.cmbMes.Items.Add(SelectMes)
                var1 = SelectMes
            End If
        Next

        Return True
    End Function

    Function ListadoUP()

        Dim oCompare As New ListViewColumnSort()
        oCompare.Sorting = SortOrder.Descending
        FrmMain.Lst1.Sorting = oCompare.Sorting
        oCompare.ColumnIndex = 1
        oCompare.CompararPor = ListViewColumnSort.TipoCompare.Fecha
        FrmMain.Lst1.ListViewItemSorter = oCompare
        Return True
    End Function
    Function ListadoDown()

        Dim oCompare As New ListViewColumnSort()
        oCompare.Sorting = SortOrder.Ascending
        FrmMain.Lst1.Sorting = oCompare.Sorting
        oCompare.ColumnIndex = 1
        oCompare.CompararPor = ListViewColumnSort.TipoCompare.Fecha
        FrmMain.Lst1.ListViewItemSorter = oCompare
        Return True
    End Function

    Function Colocarmisiones()

        Dim oCompare As New ListViewColumnSort()
        oCompare.Sorting = SortOrder.Descending
        FrmMain.LstMision.Sorting = oCompare.Sorting
        oCompare.ColumnIndex = 2
        oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero
        FrmMain.LstMision.ListViewItemSorter = oCompare
        Return True
    End Function

    Function Limpieza()
        FrmMain.Lst1.ResetText()
        FrmMain.Lst1.Items.Clear()
        FrmMain.LstMision.ResetText()
        FrmMain.LstMision.Items.Clear()
        FrmMain.CHDia.Series(0).Points.Clear()
        FrmMain.CHDia.Series(1).Points.Clear()
        FrmMain.CHMes.Series(0).Points.Clear()
        FrmMain.CmbDia.Text = ""
        FrmMain.cmbMes.Text = ""
        FrmMain.lbttMbounty.Text = ""
        FrmMain.lbttMganancia.Text = ""
        FrmMain.LbttMloot.Text = ""
        FrmMain.LbttMlp.Text = ""
        FrmMain.LbttMmedia.Text = ""
        FrmMain.LbttMnmision.Text = ""
        FrmMain.lbTotalbounty.Text = ""
        FrmMain.LBtotalloot.Text = ""
        FrmMain.LBtotallp.Text = ""
        FrmMain.LBNmision.Text = ""
        FrmMain.LBGanacia.Text = ""
        FrmMain.lbmediatotal.Text = ""
        Return True
    End Function

End Module
