<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.tmrTest = New System.Windows.Forms.Timer(Me.components)
        Me.txtDebug = New System.Windows.Forms.TextBox()
        Me.txtCurAction = New System.Windows.Forms.TextBox()
        Me.btnStart = New System.Windows.Forms.Button()
        Me.btnStop = New System.Windows.Forms.Button()
        Me.tmrCountdown = New System.Windows.Forms.Timer(Me.components)
        Me.SuspendLayout()
        '
        'tmrTest
        '
        Me.tmrTest.Interval = 60000
        '
        'txtDebug
        '
        Me.txtDebug.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.txtDebug.Location = New System.Drawing.Point(0, 44)
        Me.txtDebug.Multiline = True
        Me.txtDebug.Name = "txtDebug"
        Me.txtDebug.ReadOnly = True
        Me.txtDebug.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtDebug.Size = New System.Drawing.Size(529, 168)
        Me.txtDebug.TabIndex = 0
        '
        'txtCurAction
        '
        Me.txtCurAction.Location = New System.Drawing.Point(283, 12)
        Me.txtCurAction.Name = "txtCurAction"
        Me.txtCurAction.ReadOnly = True
        Me.txtCurAction.Size = New System.Drawing.Size(234, 20)
        Me.txtCurAction.TabIndex = 1
        '
        'btnStart
        '
        Me.btnStart.Location = New System.Drawing.Point(12, 10)
        Me.btnStart.Name = "btnStart"
        Me.btnStart.Size = New System.Drawing.Size(75, 23)
        Me.btnStart.TabIndex = 2
        Me.btnStart.Text = "Start"
        Me.btnStart.UseVisualStyleBackColor = True
        '
        'btnStop
        '
        Me.btnStop.Enabled = False
        Me.btnStop.Location = New System.Drawing.Point(150, 10)
        Me.btnStop.Name = "btnStop"
        Me.btnStop.Size = New System.Drawing.Size(75, 23)
        Me.btnStop.TabIndex = 3
        Me.btnStop.Text = "Stop"
        Me.btnStop.UseVisualStyleBackColor = True
        '
        'tmrCountdown
        '
        Me.tmrCountdown.Interval = 1000
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(529, 212)
        Me.Controls.Add(Me.btnStop)
        Me.Controls.Add(Me.btnStart)
        Me.Controls.Add(Me.txtCurAction)
        Me.Controls.Add(Me.txtDebug)
        Me.Name = "Form1"
        Me.Text = "Al Tor"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents tmrTest As System.Windows.Forms.Timer
    Friend WithEvents txtDebug As System.Windows.Forms.TextBox
    Friend WithEvents txtCurAction As System.Windows.Forms.TextBox
    Friend WithEvents btnStart As System.Windows.Forms.Button
    Friend WithEvents btnStop As System.Windows.Forms.Button
    Friend WithEvents tmrCountdown As System.Windows.Forms.Timer

End Class
