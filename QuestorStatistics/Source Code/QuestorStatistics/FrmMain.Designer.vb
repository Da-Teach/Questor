<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmMain
    Inherits System.Windows.Forms.Form

    'Form reemplaza a Dispose para limpiar la lista de componentes.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Requerido por el Diseñador de Windows Forms
    Private components As System.ComponentModel.IContainer

    'NOTA: el Diseñador de Windows Forms necesita el siguiente procedimiento
    'Se puede modificar usando el Diseñador de Windows Forms.  
    'No lo modifique con el editor de código.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim ChartArea3 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim Legend3 As System.Windows.Forms.DataVisualization.Charting.Legend = New System.Windows.Forms.DataVisualization.Charting.Legend()
        Dim Series4 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim Series5 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Dim ChartArea4 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim Legend4 As System.Windows.Forms.DataVisualization.Charting.Legend = New System.Windows.Forms.DataVisualization.Charting.Legend()
        Dim Series6 As System.Windows.Forms.DataVisualization.Charting.Series = New System.Windows.Forms.DataVisualization.Charting.Series()
        Me.OFD1 = New System.Windows.Forms.OpenFileDialog()
        Me.cmb1 = New System.Windows.Forms.ComboBox()
        Me.FBD1 = New System.Windows.Forms.FolderBrowserDialog()
        Me.Tab1 = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.Lst1 = New System.Windows.Forms.ListView()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.lbmediatotal = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.LBGanacia = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.CHDia = New System.Windows.Forms.DataVisualization.Charting.Chart()
        Me.LBNmision = New System.Windows.Forms.Label()
        Me.LBtotallp = New System.Windows.Forms.Label()
        Me.LBtotalloot = New System.Windows.Forms.Label()
        Me.lbTotalbounty = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.CmbDia = New System.Windows.Forms.ComboBox()
        Me.TabPage3 = New System.Windows.Forms.TabPage()
        Me.LbttMmedia = New System.Windows.Forms.Label()
        Me.lbttMganancia = New System.Windows.Forms.Label()
        Me.LbttMnmision = New System.Windows.Forms.Label()
        Me.LbttMlp = New System.Windows.Forms.Label()
        Me.LbttMloot = New System.Windows.Forms.Label()
        Me.lbttMbounty = New System.Windows.Forms.Label()
        Me.CHMes = New System.Windows.Forms.DataVisualization.Charting.Chart()
        Me.cmbMes = New System.Windows.Forms.ComboBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.TabPage4 = New System.Windows.Forms.TabPage()
        Me.LstMision = New System.Windows.Forms.ListView()
        Me.Tab1.SuspendLayout()
        Me.TabPage1.SuspendLayout()
        Me.TabPage2.SuspendLayout()
        CType(Me.CHDia, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage3.SuspendLayout()
        CType(Me.CHMes, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.TabPage4.SuspendLayout()
        Me.SuspendLayout()
        '
        'OFD1
        '
        Me.OFD1.FileName = "OpenFileDialog1"
        '
        'cmb1
        '
        Me.cmb1.FormattingEnabled = True
        Me.cmb1.Location = New System.Drawing.Point(0, 2)
        Me.cmb1.Name = "cmb1"
        Me.cmb1.Size = New System.Drawing.Size(134, 21)
        Me.cmb1.TabIndex = 2
        Me.cmb1.Text = "Select Char"
        '
        'Tab1
        '
        Me.Tab1.Controls.Add(Me.TabPage1)
        Me.Tab1.Controls.Add(Me.TabPage2)
        Me.Tab1.Controls.Add(Me.TabPage3)
        Me.Tab1.Controls.Add(Me.TabPage4)
        Me.Tab1.Location = New System.Drawing.Point(0, 38)
        Me.Tab1.Name = "Tab1"
        Me.Tab1.SelectedIndex = 0
        Me.Tab1.Size = New System.Drawing.Size(1005, 389)
        Me.Tab1.TabIndex = 6
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.Lst1)
        Me.TabPage1.Location = New System.Drawing.Point(4, 22)
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage1.Size = New System.Drawing.Size(997, 363)
        Me.TabPage1.TabIndex = 0
        Me.TabPage1.Text = "List"
        Me.TabPage1.UseVisualStyleBackColor = True
        '
        'Lst1
        '
        Me.Lst1.Location = New System.Drawing.Point(3, 2)
        Me.Lst1.Name = "Lst1"
        Me.Lst1.Size = New System.Drawing.Size(988, 355)
        Me.Lst1.TabIndex = 2
        Me.Lst1.UseCompatibleStateImageBehavior = False
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.lbmediatotal)
        Me.TabPage2.Controls.Add(Me.Label6)
        Me.TabPage2.Controls.Add(Me.LBGanacia)
        Me.TabPage2.Controls.Add(Me.Label5)
        Me.TabPage2.Controls.Add(Me.CHDia)
        Me.TabPage2.Controls.Add(Me.LBNmision)
        Me.TabPage2.Controls.Add(Me.LBtotallp)
        Me.TabPage2.Controls.Add(Me.LBtotalloot)
        Me.TabPage2.Controls.Add(Me.lbTotalbounty)
        Me.TabPage2.Controls.Add(Me.Label4)
        Me.TabPage2.Controls.Add(Me.Label3)
        Me.TabPage2.Controls.Add(Me.Label2)
        Me.TabPage2.Controls.Add(Me.Label1)
        Me.TabPage2.Controls.Add(Me.CmbDia)
        Me.TabPage2.Location = New System.Drawing.Point(4, 22)
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage2.Size = New System.Drawing.Size(997, 363)
        Me.TabPage2.TabIndex = 1
        Me.TabPage2.Text = "Day"
        Me.TabPage2.UseVisualStyleBackColor = True
        '
        'lbmediatotal
        '
        Me.lbmediatotal.AutoSize = True
        Me.lbmediatotal.Location = New System.Drawing.Point(99, 185)
        Me.lbmediatotal.Name = "lbmediatotal"
        Me.lbmediatotal.Size = New System.Drawing.Size(0, 13)
        Me.lbmediatotal.TabIndex = 13
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(3, 185)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(50, 13)
        Me.Label6.TabIndex = 12
        Me.Label6.Text = "Average:"
        '
        'LBGanacia
        '
        Me.LBGanacia.AutoSize = True
        Me.LBGanacia.Location = New System.Drawing.Point(99, 154)
        Me.LBGanacia.Name = "LBGanacia"
        Me.LBGanacia.Size = New System.Drawing.Size(0, 13)
        Me.LBGanacia.TabIndex = 11
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(3, 154)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(60, 13)
        Me.Label5.TabIndex = 10
        Me.Label5.Text = "Total profit:"
        '
        'CHDia
        '
        ChartArea3.Name = "ChartArea1"
        Me.CHDia.ChartAreas.Add(ChartArea3)
        Legend3.Alignment = System.Drawing.StringAlignment.Center
        Legend3.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom
        Legend3.Name = "Legend1"
        Me.CHDia.Legends.Add(Legend3)
        Me.CHDia.Location = New System.Drawing.Point(165, 3)
        Me.CHDia.Name = "CHDia"
        Series4.ChartArea = "ChartArea1"
        Series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line
        Series4.IsXValueIndexed = True
        Series4.Legend = "Legend1"
        Series4.Name = "Bounty Isk"
        Series5.ChartArea = "ChartArea1"
        Series5.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line
        Series5.Legend = "Legend1"
        Series5.Name = "Loot Isk"
        Me.CHDia.Series.Add(Series4)
        Me.CHDia.Series.Add(Series5)
        Me.CHDia.Size = New System.Drawing.Size(826, 354)
        Me.CHDia.TabIndex = 9
        Me.CHDia.Text = "Chart1"
        '
        'LBNmision
        '
        Me.LBNmision.AutoSize = True
        Me.LBNmision.Location = New System.Drawing.Point(99, 124)
        Me.LBNmision.Name = "LBNmision"
        Me.LBNmision.Size = New System.Drawing.Size(0, 13)
        Me.LBNmision.TabIndex = 8
        '
        'LBtotallp
        '
        Me.LBtotallp.AutoSize = True
        Me.LBtotallp.Location = New System.Drawing.Point(99, 90)
        Me.LBtotallp.Name = "LBtotallp"
        Me.LBtotallp.Size = New System.Drawing.Size(0, 13)
        Me.LBtotallp.TabIndex = 7
        '
        'LBtotalloot
        '
        Me.LBtotalloot.AutoSize = True
        Me.LBtotalloot.Location = New System.Drawing.Point(99, 56)
        Me.LBtotalloot.Name = "LBtotalloot"
        Me.LBtotalloot.Size = New System.Drawing.Size(0, 13)
        Me.LBtotalloot.TabIndex = 6
        '
        'lbTotalbounty
        '
        Me.lbTotalbounty.AutoSize = True
        Me.lbTotalbounty.Location = New System.Drawing.Point(99, 24)
        Me.lbTotalbounty.Name = "lbTotalbounty"
        Me.lbTotalbounty.Size = New System.Drawing.Size(0, 13)
        Me.lbTotalbounty.TabIndex = 5
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(3, 124)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(63, 13)
        Me.Label4.TabIndex = 4
        Me.Label4.Text = "Nº Mission: "
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(3, 90)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(53, 13)
        Me.Label3.TabIndex = 3
        Me.Label3.Text = "Total LP: "
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(3, 56)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(78, 13)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Total Loot Isk: "
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(3, 24)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(90, 13)
        Me.Label1.TabIndex = 1
        Me.Label1.Text = "Total Bounty Isk: "
        '
        'CmbDia
        '
        Me.CmbDia.FormattingEnabled = True
        Me.CmbDia.Location = New System.Drawing.Point(0, 0)
        Me.CmbDia.Name = "CmbDia"
        Me.CmbDia.Size = New System.Drawing.Size(93, 21)
        Me.CmbDia.TabIndex = 0
        '
        'TabPage3
        '
        Me.TabPage3.Controls.Add(Me.LbttMmedia)
        Me.TabPage3.Controls.Add(Me.lbttMganancia)
        Me.TabPage3.Controls.Add(Me.LbttMnmision)
        Me.TabPage3.Controls.Add(Me.LbttMlp)
        Me.TabPage3.Controls.Add(Me.LbttMloot)
        Me.TabPage3.Controls.Add(Me.lbttMbounty)
        Me.TabPage3.Controls.Add(Me.CHMes)
        Me.TabPage3.Controls.Add(Me.cmbMes)
        Me.TabPage3.Controls.Add(Me.Label7)
        Me.TabPage3.Controls.Add(Me.Label8)
        Me.TabPage3.Controls.Add(Me.Label9)
        Me.TabPage3.Controls.Add(Me.Label10)
        Me.TabPage3.Controls.Add(Me.Label11)
        Me.TabPage3.Controls.Add(Me.Label12)
        Me.TabPage3.Location = New System.Drawing.Point(4, 22)
        Me.TabPage3.Name = "TabPage3"
        Me.TabPage3.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage3.Size = New System.Drawing.Size(997, 363)
        Me.TabPage3.TabIndex = 2
        Me.TabPage3.Text = "Month"
        Me.TabPage3.UseVisualStyleBackColor = True
        '
        'LbttMmedia
        '
        Me.LbttMmedia.AutoSize = True
        Me.LbttMmedia.Location = New System.Drawing.Point(99, 184)
        Me.LbttMmedia.Name = "LbttMmedia"
        Me.LbttMmedia.Size = New System.Drawing.Size(0, 13)
        Me.LbttMmedia.TabIndex = 26
        '
        'lbttMganancia
        '
        Me.lbttMganancia.AutoSize = True
        Me.lbttMganancia.Location = New System.Drawing.Point(99, 153)
        Me.lbttMganancia.Name = "lbttMganancia"
        Me.lbttMganancia.Size = New System.Drawing.Size(0, 13)
        Me.lbttMganancia.TabIndex = 25
        '
        'LbttMnmision
        '
        Me.LbttMnmision.AutoSize = True
        Me.LbttMnmision.Location = New System.Drawing.Point(99, 123)
        Me.LbttMnmision.Name = "LbttMnmision"
        Me.LbttMnmision.Size = New System.Drawing.Size(0, 13)
        Me.LbttMnmision.TabIndex = 24
        '
        'LbttMlp
        '
        Me.LbttMlp.AutoSize = True
        Me.LbttMlp.Location = New System.Drawing.Point(99, 89)
        Me.LbttMlp.Name = "LbttMlp"
        Me.LbttMlp.Size = New System.Drawing.Size(0, 13)
        Me.LbttMlp.TabIndex = 23
        '
        'LbttMloot
        '
        Me.LbttMloot.AutoSize = True
        Me.LbttMloot.Location = New System.Drawing.Point(99, 55)
        Me.LbttMloot.Name = "LbttMloot"
        Me.LbttMloot.Size = New System.Drawing.Size(0, 13)
        Me.LbttMloot.TabIndex = 22
        '
        'lbttMbounty
        '
        Me.lbttMbounty.AutoSize = True
        Me.lbttMbounty.Location = New System.Drawing.Point(99, 23)
        Me.lbttMbounty.Name = "lbttMbounty"
        Me.lbttMbounty.Size = New System.Drawing.Size(0, 13)
        Me.lbttMbounty.TabIndex = 21
        '
        'CHMes
        '
        ChartArea4.Name = "ChartArea1"
        Me.CHMes.ChartAreas.Add(ChartArea4)
        Legend4.Alignment = System.Drawing.StringAlignment.Center
        Legend4.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom
        Legend4.Name = "Legend1"
        Me.CHMes.Legends.Add(Legend4)
        Me.CHMes.Location = New System.Drawing.Point(164, 3)
        Me.CHMes.Name = "CHMes"
        Series6.ChartArea = "ChartArea1"
        Series6.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line
        Series6.Legend = "Legend1"
        Series6.MarkerColor = System.Drawing.Color.Red
        Series6.Name = "ISK per Day"
        Me.CHMes.Series.Add(Series6)
        Me.CHMes.Size = New System.Drawing.Size(830, 354)
        Me.CHMes.TabIndex = 20
        Me.CHMes.Text = "Chart1"
        '
        'cmbMes
        '
        Me.cmbMes.FormattingEnabled = True
        Me.cmbMes.Location = New System.Drawing.Point(0, -1)
        Me.cmbMes.Name = "cmbMes"
        Me.cmbMes.Size = New System.Drawing.Size(93, 21)
        Me.cmbMes.TabIndex = 19
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(3, 184)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(50, 13)
        Me.Label7.TabIndex = 18
        Me.Label7.Text = "Average:"
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(3, 153)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(60, 13)
        Me.Label8.TabIndex = 17
        Me.Label8.Text = "Total profit:"
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(3, 123)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(63, 13)
        Me.Label9.TabIndex = 16
        Me.Label9.Text = "Nº Mission: "
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(3, 89)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(53, 13)
        Me.Label10.TabIndex = 15
        Me.Label10.Text = "Total LP: "
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(3, 55)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(78, 13)
        Me.Label11.TabIndex = 14
        Me.Label11.Text = "Total Loot Isk: "
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(3, 23)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(90, 13)
        Me.Label12.TabIndex = 13
        Me.Label12.Text = "Total Bounty Isk: "
        '
        'TabPage4
        '
        Me.TabPage4.Controls.Add(Me.LstMision)
        Me.TabPage4.Location = New System.Drawing.Point(4, 22)
        Me.TabPage4.Name = "TabPage4"
        Me.TabPage4.Padding = New System.Windows.Forms.Padding(3)
        Me.TabPage4.Size = New System.Drawing.Size(997, 363)
        Me.TabPage4.TabIndex = 3
        Me.TabPage4.Text = "Mission"
        Me.TabPage4.UseVisualStyleBackColor = True
        '
        'LstMision
        '
        Me.LstMision.Location = New System.Drawing.Point(3, 6)
        Me.LstMision.Name = "LstMision"
        Me.LstMision.Size = New System.Drawing.Size(960, 351)
        Me.LstMision.TabIndex = 0
        Me.LstMision.UseCompatibleStateImageBehavior = False
        '
        'FrmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1007, 454)
        Me.Controls.Add(Me.Tab1)
        Me.Controls.Add(Me.cmb1)
        Me.Name = "FrmMain"
        Me.Text = "Questor Statictics"
        Me.Tab1.ResumeLayout(False)
        Me.TabPage1.ResumeLayout(False)
        Me.TabPage2.ResumeLayout(False)
        Me.TabPage2.PerformLayout()
        CType(Me.CHDia, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage3.ResumeLayout(False)
        Me.TabPage3.PerformLayout()
        CType(Me.CHMes, System.ComponentModel.ISupportInitialize).EndInit()
        Me.TabPage4.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents OFD1 As System.Windows.Forms.OpenFileDialog
    Friend WithEvents cmb1 As System.Windows.Forms.ComboBox
    Friend WithEvents FBD1 As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents Tab1 As System.Windows.Forms.TabControl
    Friend WithEvents TabPage1 As System.Windows.Forms.TabPage
    Friend WithEvents Lst1 As System.Windows.Forms.ListView
    Friend WithEvents TabPage2 As System.Windows.Forms.TabPage
    Friend WithEvents TabPage3 As System.Windows.Forms.TabPage
    Friend WithEvents TabPage4 As System.Windows.Forms.TabPage
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents CmbDia As System.Windows.Forms.ComboBox
    Friend WithEvents LBNmision As System.Windows.Forms.Label
    Friend WithEvents LBtotallp As System.Windows.Forms.Label
    Friend WithEvents LBtotalloot As System.Windows.Forms.Label
    Friend WithEvents lbTotalbounty As System.Windows.Forms.Label
    Friend WithEvents CHDia As System.Windows.Forms.DataVisualization.Charting.Chart
    Friend WithEvents LBGanacia As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents lbmediatotal As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents LstMision As System.Windows.Forms.ListView
    Friend WithEvents cmbMes As System.Windows.Forms.ComboBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents LbttMmedia As System.Windows.Forms.Label
    Friend WithEvents lbttMganancia As System.Windows.Forms.Label
    Friend WithEvents LbttMnmision As System.Windows.Forms.Label
    Friend WithEvents LbttMlp As System.Windows.Forms.Label
    Friend WithEvents LbttMloot As System.Windows.Forms.Label
    Friend WithEvents lbttMbounty As System.Windows.Forms.Label
    Friend WithEvents CHMes As System.Windows.Forms.DataVisualization.Charting.Chart
End Class
