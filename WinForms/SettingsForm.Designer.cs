namespace WinForms;

partial class SettingsForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        itemsPanel = new FlowLayoutPanel();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // statusStrip
        // 
        statusStrip.ImageScalingSize = new Size(20, 20);
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 428);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(800, 22);
        statusStrip.TabIndex = 0;
        statusStrip.Text = "statusStrip1";
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(0, 16);
        // 
        // itemsPanel
        // 
        itemsPanel.AutoScroll = true;
        itemsPanel.FlowDirection = FlowDirection.TopDown;
        itemsPanel.Location = new Point(12, 12);
        itemsPanel.Name = "itemsPanel";
        itemsPanel.Size = new Size(776, 413);
        itemsPanel.TabIndex = 1;
        itemsPanel.WrapContents = false;
        // 
        // SettingsForm
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(itemsPanel);
        Controls.Add(statusStrip);
        Name = "SettingsForm";
        Text = "Language deactivation";
        Load += SettingsForm_Load;
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private FlowLayoutPanel itemsPanel;
}