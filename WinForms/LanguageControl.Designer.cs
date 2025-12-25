namespace WinForms;

partial class LanguageControl
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        enabledCheckBox = new CheckBox();
        nameLabel = new Label();
        toggleButton = new Button();
        SuspendLayout();
        // 
        // enabledCheckBox
        // 
        enabledCheckBox.AutoSize = true;
        enabledCheckBox.Enabled = false;
        enabledCheckBox.Location = new Point(8, 12);
        enabledCheckBox.Name = "enabledCheckBox";
        enabledCheckBox.Size = new Size(18, 17);
        enabledCheckBox.TabIndex = 0;
        enabledCheckBox.UseVisualStyleBackColor = true;
        // 
        // nameLabel
        // 
        nameLabel.AutoSize = true;
        nameLabel.Location = new Point(143, 9);
        nameLabel.Name = "nameLabel";
        nameLabel.Size = new Size(58, 20);
        nameLabel.TabIndex = 1;
        nameLabel.Text = "Russian";
        // 
        // toggleButton
        // 
        toggleButton.Location = new Point(32, 5);
        toggleButton.Name = "toggleButton";
        toggleButton.Size = new Size(94, 29);
        toggleButton.TabIndex = 2;
        toggleButton.Text = "Enable";
        toggleButton.UseVisualStyleBackColor = true;
        toggleButton.Click += toggleButton_Click;
        // 
        // LanguageControl
        // 
        AutoScaleDimensions = new SizeF(8F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(toggleButton);
        Controls.Add(nameLabel);
        Controls.Add(enabledCheckBox);
        Name = "LanguageControl";
        Size = new Size(329, 41);
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private CheckBox enabledCheckBox;
    private Label nameLabel;
    private Button toggleButton;
}
