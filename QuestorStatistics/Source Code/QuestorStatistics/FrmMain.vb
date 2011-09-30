Public Class FrmMain

    Dim LocalRuta As String = My.Application.Info.DirectoryPath


    Private Sub Cmd1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        OFD1.ShowDialog()


    End Sub

    Private Sub FrmMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Dim o As New System.IO.DirectoryInfo(LocalRuta)
        Dim myfiles() As System.IO.FileInfo

        myfiles = o.GetFiles("*.statistics.log")
        For y As Integer = 0 To myfiles.Length - 1
            cmb1.Items.Add(myfiles(y).Name())
        Next
        With Lst1
            .View = View.Details
            .FullRowSelect = True
            .GridLines = True
            .LabelEdit = False
            .Columns.Clear()
            .Items.Clear()

            .Columns.Add("", 0, HorizontalAlignment.Right)
            .Columns.Add("Date", 145, HorizontalAlignment.Left)
            .Columns.Add("Mission", 190, HorizontalAlignment.Left)
            .Columns.Add("Time", 50, HorizontalAlignment.Right)
            .Columns.Add("Isk Bounty", 100, HorizontalAlignment.Right)
            .Columns.Add("Bounty/Min", 80, HorizontalAlignment.Right)
            .Columns.Add("Isk Loot", 100, HorizontalAlignment.Right)
            .Columns.Add("Loot/Min", 80, HorizontalAlignment.Right)
            .Columns.Add("LP", 80, HorizontalAlignment.Right)
            .Columns.Add("LP/Min", 60, HorizontalAlignment.Right)
            .Columns.Add("Total ISK/Min", 80, HorizontalAlignment.Right)

        End With

        With LstMision
            .View = View.Details
            .FullRowSelect = True
            .GridLines = True
            .LabelEdit = False
            .Columns.Clear()
            .Items.Clear()

            .Columns.Add("", 0, HorizontalAlignment.Right)
            .Columns.Add("Mission", 240, HorizontalAlignment.Left)
            .Columns.Add("Repet %", 60, HorizontalAlignment.Right)
            .Columns.Add("Time", 100, HorizontalAlignment.Right)
            .Columns.Add("Media Isk Bounty", 100, HorizontalAlignment.Right)
            .Columns.Add("Media Isk Loot", 100, HorizontalAlignment.Right)
            .Columns.Add("Media LP", 100, HorizontalAlignment.Right)
            .Columns.Add("Media ISK/Min", 100, HorizontalAlignment.Right)

        End With


    End Sub


    Private Sub cmb1_SelectionChangeCommitted(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmb1.SelectionChangeCommitted
        Dim fic As String = LocalRuta + "\" + cmb1.SelectedItem.ToString
        Dim Datos
        Dim Cabecera As Boolean
        Dim sr As New System.IO.StreamReader(fic)
        Cabecera = True
        Limpieza()


        Do While Not sr.Peek = -1
            Datos = Split(sr.ReadLine, ";")

            If Cabecera Then
                Cabecera = False
            Else

                Dim item As New ListViewItem("")

                ' cargar los datos y las propiedades  
                With item
                    .SubItems.Add(Datos(0))
                    .SubItems.Add(Datos(1))
                    .SubItems.Add(Datos(2))
                    .SubItems.Add(Format(CDbl(Datos(3)), "###,###,##0"))
                    .SubItems.Add(Format(CDbl(Datos(3)) / CDbl(Datos(2)), "###,###,##0"))
                    .SubItems.Add(Format(CDbl(Datos(4)), "###,###,##0"))
                    .SubItems.Add(Format(CDbl(Datos(4)) / CDbl(Datos(2)), "###,###,##0"))
                    .SubItems.Add(Format(CDbl(Datos(5)), "###,###,##0"))
                    .SubItems.Add(Format(CDbl(Datos(5)) / CDbl(Datos(2)), "###,###,##0"))
                    .SubItems.Add(Format((CDbl(Datos(3)) + CDbl(Datos(4))) / CDbl(Datos(2)), "###,###,##0"))
                    Lst1.Items.Add(item) ' añadir el item   
                End With
            End If
        Loop
        sr.Close()

        Precarga()
        StatsMision()
    End Sub


    Private Sub CmbDia_SelectionChangeCommitted(ByVal sender As Object, ByVal e As System.EventArgs) Handles CmbDia.SelectionChangeCommitted
        ListadoUP()
        Dia(CmbDia.SelectedItem.ToString)
        GraficaDia(CmbDia.SelectedItem.ToString)
    End Sub

   
    Private Sub cmb1_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmb1.SelectedIndexChanged

    End Sub

    Private Sub Lst1_ColumnClick(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles Lst1.ColumnClick

        Dim oCompare As New ListViewColumnSort()

        If Lst1.Sorting = SortOrder.Ascending Then
            oCompare.Sorting = SortOrder.Descending
        Else
            oCompare.Sorting = SortOrder.Ascending
        End If
        Lst1.Sorting = oCompare.Sorting
        oCompare.ColumnIndex = e.Column

        Select Case e.Column
            Case 1
                oCompare.CompararPor = ListViewColumnSort.TipoCompare.Fecha
            Case 2
                oCompare.CompararPor = ListViewColumnSort.TipoCompare.Cadena
            Case 3
                oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero
            Case 4
                oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero
            Case 5
                oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero
            Case 6
                oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero
            Case 7
                oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero
            Case 8
                oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero
            Case 9
                oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero
            Case 10
                oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero

        End Select

        Lst1.ListViewItemSorter = oCompare

    End Sub

    Private Sub LstMision_ColumnClick(ByVal sender As Object, ByVal e As System.Windows.Forms.ColumnClickEventArgs) Handles LstMision.ColumnClick

        Dim oCompare As New ListViewColumnSort()

        If LstMision.Sorting = SortOrder.Ascending Then
            oCompare.Sorting = SortOrder.Descending
        Else
            oCompare.Sorting = SortOrder.Ascending
        End If
        LstMision.Sorting = oCompare.Sorting
        oCompare.ColumnIndex = e.Column

        Select Case e.Column
            Case 1
                oCompare.CompararPor = ListViewColumnSort.TipoCompare.Cadena
            Case 2
                oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero
            Case 3
                oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero
            Case 4
                oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero
            Case 5
                oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero
            Case 6
                oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero
            Case 7
                oCompare.CompararPor = ListViewColumnSort.TipoCompare.Numero


        End Select
        LstMision.ListViewItemSorter = oCompare

    End Sub

    Private Sub cmbMes_SelectionChangeCommitted(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmbMes.SelectionChangeCommitted
        ListadoUP()
        Mes(cmbMes.SelectedItem.ToString)
        GraficaMes(cmbMes.SelectedItem.ToString)
    End Sub
End Class