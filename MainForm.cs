using System.Diagnostics;
using System.Windows.Forms;

namespace CodexFileQuery;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        UpdateStatus("Ready - Click 'Open ChatGPT' to begin");
    }

    private void BtnConnect_Click(object sender, EventArgs e)
    {
        try
        {
            UpdateStatus("Launching Chrome with your profile...");
            
            // Find Chrome
            string chromePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Google", "Chrome", "Application", "chrome.exe");
            
            if (!File.Exists(chromePath))
            {
                chromePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder["ProgramFilesX86"]),
                    "Google", "Chrome", "Application", "chrome.exe");
            }

            string userDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Google", "Chrome", "User Data");

            var psi = new ProcessStartInfo
            {
                FileName = chromePath,
                Arguments = $"--profile-directory=Default \"https://chat.openai.com/\"",
                UseShellExecute = true
            };

            // Try to use existing profile (may fail if Chrome is running)
            try
            {
                Process.Start(psi);
            }
            catch
            {
                // Chrome already running - just open new tab
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://chat.openai.com/",
                    UseShellExecute = true
                });
            }

            UpdateStatus("Chrome opened! Log in if needed, then start a new chat.");
            MessageBox.Show(
                "Chrome should be open with ChatGPT.\n\n" +
                "1. Log in if prompted (MFA works)\n" +
                "2. Click 'New chat'\n" +
                "3. Come back here and paste file path + question",
                "ChatGPT Opened",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error: {ex.Message}");
            MessageBox.Show($"Failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnBrowseFile_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select file",
            Multiselect = true,
            Filter = "All files (*.*)|*.*|C# files (*.cs)|*.cs|Python (*.py)|*.py"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtFilePath.Text = string.Join("; ", dialog.FileNames);
        }
    }

    private void BtnSend_Click(object sender, EventArgs e)
    {
        var filePaths = txtFilePath.Text.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToList();

        if (filePaths.Count == 0)
        {
            MessageBox.Show("Select a file first.", "No File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var missing = filePaths.Where(p => !File.Exists(p)).ToList();
        if (missing.Count > 0)
        {
            MessageBox.Show($"Files not found:\n{string.Join("\n", missing)}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Read file and copy to clipboard
        string fileContent = File.ReadAllText(filePaths[0]);
        string fileName = Path.GetFileName(filePaths[0]);
        string question = txtQuestion.Text.Trim();
        
        if (string.IsNullOrEmpty(question))
        {
            question = "Analyze this code and suggest improvements.";
        }

        string prompt = $"Here's the file `{fileName}`:\n\n```\n{fileContent}\n```\n\n{question}";
        
        Clipboard.SetText(prompt);
        
        MessageBox.Show(
            $"Prompt copied to clipboard!\n\n" +
            "1. Switch to Chrome/ChatGPT\n" +
            "2. Paste (Ctrl+V)\n" +
            "3. Send",
            "Ready to Paste",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        
        UpdateStatus("Prompt copied! Switch to Chrome and paste.");
    }

    private void UpdateStatus(string message)
    {
        lblStatus.Text = message;
        lblStatus.Invalidate();
        lblStatus.Update();
    }
}
