using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace CodexFileQuery;

public partial class MainForm : Form
{
    private ChromeDriver? _driver;
    private bool _isConnected;

    public MainForm()
    {
        InitializeComponent();
        UpdateStatus("Ready - Click 'Open ChatGPT' to begin");
    }

    private void BtnConnect_Click(object sender, EventArgs e)
    {
        try
        {
            if (_isConnected && _driver != null)
            {
                _driver.Navigate().GoToUrl("https://chat.openai.com/");
                UpdateStatus("ChatGPT refreshed");
                return;
            }

            UpdateStatus("Launching Chrome...");
            btnConnect.Enabled = false;

            var options = new ChromeOptions();
            
            // Use your existing Chrome profile
            string userDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Google", "Chrome", "User Data");
            
            // Use default profile (or change to a specific profile directory)
            options.AddArgument($"--user-data-dir={userDataDir}");
            options.AddArgument("--profile-directory=Default");
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            
            // Don't actually close Chrome if it's already running
            options.AddAdditionalOption("detach", true);

            try
            {
                _driver = new ChromeDriver(options);
            }
            catch
            {
                // Chrome might already be running - try without detach
                options.AddArgument("--new-window");
                _driver = new ChromeDriver(options);
            }

            _driver.Navigate().GoToUrl("https://chat.openai.com/");
            
            var result = MessageBox.Show(
                "If Chrome asked you to sign in:\n\n" +
                "1. Sign in to your account (including MFA)\n" +
                "2. Click 'New chat'\n" +
                "3. Come back here and click 'Connect' again\n\n" +
                "Click OK to continue.",
                "Check Chrome",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information);

            _isConnected = true;
            btnConnect.Text = "Refresh ChatGPT";
            btnSend.Enabled = true;
            UpdateStatus("Connected! Select files and click 'Upload & Ask'");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error: {ex.Message}");
            btnConnect.Enabled = true;
            MessageBox.Show($"Failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnBrowseFile_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select file to upload",
            Multiselect = true,
            Filter = "All files (*.*)|*.*|C# files (*.cs)|*.cs|Python files (*.py)|*.py"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtFilePath.Text = string.Join("; ", dialog.FileNames);
        }
    }

    private void BtnSend_Click(object sender, EventArgs e)
    {
        if (_driver == null || !_isConnected)
        {
            MessageBox.Show("Click 'Open ChatGPT' first.", "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var filePaths = txtFilePath.Text.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToList();

        if (filePaths.Count == 0)
        {
            MessageBox.Show("Select at least one file.", "No File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var missing = filePaths.Where(p => !File.Exists(p)).ToList();
        if (missing.Count > 0)
        {
            MessageBox.Show($"Files not found:\n{string.Join("\n", missing)}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var question = txtQuestion.Text.Trim();
        if (string.IsNullOrEmpty(question))
        {
            question = "Analyze this code. Explain what it does and suggest improvements.";
        }

        UploadAndAsk(filePaths, question);
    }

    private void UploadAndAsk(List<string> filePaths, string question)
    {
        try
        {
            btnSend.Enabled = false;
            UpdateStatus("Uploading files...");

            // Find file input
            var fileInput = _driver!.FindElement(By.CssSelector("input[type='file']"));
            
            // Upload files (Selenium can handle this)
            if (filePaths.Count == 1)
            {
                fileInput.SendKeys(filePaths[0]);
            }
            else
            {
                // For multiple files, send them one at a time
                foreach (var path in filePaths)
                {
                    fileInput.SendKeys(path + "\n");
                    System.Threading.Thread.Sleep(500);
                }
            }

            UpdateStatus("Waiting for upload...");
            System.Threading.Thread.Sleep(3000);

            UpdateStatus("Typing question...");
            
            // Find and fill textarea
            var textarea = _driver.FindElement(By.CssSelector("textarea"));
            textarea.Clear();
            textarea.SendKeys(question);

            UpdateStatus("Submitting...");
            textarea.SendKeys(Keys.Return);

            UpdateStatus("Waiting for response (10-30 seconds)...");
            
            // Wait for response
            System.Threading.Thread.Sleep(15000);
            
            try
            {
                var response = _driver.FindElement(By.CssSelector(".markdown"));
                var text = response.Text;
                
                txtResponse.Text = text;
                UpdateStatus("Response received!");
                Clipboard.SetText(text);
            }
            catch
            {
                txtResponse.Text = "Response not detected. Check browser.";
                UpdateStatus("Check browser for response");
            }
        }
        catch (Exception ex)
        {
            txtResponse.Text = $"Error: {ex.Message}";
            UpdateStatus($"Error: {ex.Message}");
        }
        finally
        {
            btnSend.Enabled = _isConnected;
        }
    }

    private void UpdateStatus(string message)
    {
        lblStatus.Text = message;
        lblStatus.Invalidate();
        lblStatus.Update();
        Application.DoEvents();
    }

    private void BtnCancel_Click(object sender, EventArgs e)
    {
        UpdateStatus("Cancelled");
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        _driver?.Dispose();
    }
}
