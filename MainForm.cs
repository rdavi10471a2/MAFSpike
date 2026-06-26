using System.Windows.Forms;
using Microsoft.Playwright;

namespace CodexFileQuery;

public partial class MainForm : Form
{
    private readonly string _browserSessionPath;
    private IBrowserContext? _context;
    private IPage? _page;
    private bool _isConnected;
    private CancellationTokenSource? _cts;

    public MainForm()
    {
        InitializeComponent();
        
        _browserSessionPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "CodexFileQuery",
            "browser_session"
        );
        
        Directory.CreateDirectory(_browserSessionPath);
        UpdateStatus("Ready - Click 'Open ChatGPT' to begin");
    }

    private async void BtnConnect_Click(object sender, EventArgs e)
    {
        if (_isConnected && _page != null)
        {
            await _page.ReloadAsync();
            UpdateStatus("ChatGPT refreshed");
            return;
        }

        await ConnectToChatGPTAsync();
    }

    private async Task ConnectToChatGPTAsync()
    {
        try
        {
            UpdateStatus("Initializing...");
            btnConnect.Enabled = false;
            btnSend.Enabled = false;

            UpdateStatus("Launching browser...");
            
            _context = await Chromium.LaunchPersistentContextAsync(
                _browserSessionPath,
                new PersistentBrowserContextOptions
                {
                    Headless = false,
                    Args = new[] { "--disable-blink-features=AutomationControlled" }
                });

            _page = _context.Pages.FirstOrDefault() ?? await _context.NewPageAsync();
            
            UpdateStatus("Opening ChatGPT...");
            await _page.GotoAsync("https://chat.openai.com/", 
                new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            await CheckLoginStatusAsync();

            _isConnected = true;
            btnConnect.Text = "Refresh ChatGPT";
            btnSend.Enabled = true;
            
            UpdateStatus("Connected! Select files and click 'Upload & Ask'");
        }
        catch (PlaywrightException ex) when (ex.Message.Contains("browser"))
        {
            UpdateStatus("Installing Chromium (first time)...");
            await Task.Run(() => Playwright.InstallAsync());
            await ConnectToChatGPTAsync();
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error: {ex.Message}");
            btnConnect.Enabled = true;
            MessageBox.Show($"Failed to connect:\n{ex.Message}", "Connection Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CheckLoginStatusAsync()
    {
        if (_page == null) return;

        try
        {
            await Task.Delay(2000);
            
            var loginButton = _page.Locator("text=Log in");
            var count = await loginButton.CountAsync();
            
            if (count > 0)
            {
                var result = MessageBox.Show(
                    "You need to log in to ChatGPT.\n\n" +
                    "1. Click 'OK'\n" +
                    "2. Log in (including MFA)\n" +
                    "3. Start a new chat\n" +
                    "4. Come back and click 'Connect' again",
                    "Login Required",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Information);

                if (result == DialogResult.OK)
                {
                    await _page.GotoAsync("https://chat.openai.com/auth/login");
                }
            }
            else
            {
                UpdateStatus("Already logged in to ChatGPT!");
            }
        }
        catch
        {
            UpdateStatus("Ready - verify ChatGPT is loaded");
        }
    }

    private void BtnBrowseFile_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select file to upload to ChatGPT",
            Multiselect = true,
            Filter = "All files (*.*)|*.*|" +
                     "C# files (*.cs)|*.cs|" +
                     "Python files (*.py)|*.py|" +
                     "JavaScript files (*.js)|*.js|" +
                     "Text files (*.txt)|*.txt|" +
                     "JSON files (*.json)|*.json"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtFilePath.Text = string.Join("; ", dialog.FileNames);
        }
    }

    private async void BtnSend_Click(object sender, EventArgs e)
    {
        if (_page == null || !_isConnected)
        {
            MessageBox.Show("Please click 'Open ChatGPT' first.", "Not Connected", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var filePaths = txtFilePath.Text.Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .ToList();

        if (filePaths.Count == 0)
        {
            MessageBox.Show("Please select at least one file.", "No File Selected", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var missing = filePaths.Where(p => !File.Exists(p)).ToList();
        if (missing.Count > 0)
        {
            MessageBox.Show($"Files not found:\n{string.Join("\n", missing)}", "File Not Found", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var question = txtQuestion.Text.Trim();
        if (string.IsNullOrEmpty(question))
        {
            question = "Analyze this code. Explain what it does and suggest improvements.";
        }

        await UploadAndAskAsync(filePaths, question);
    }

    private async Task UploadAndAskAsync(List<string> filePaths, string question)
    {
        try
        {
            btnSend.Enabled = false;
            UpdateStatus("Preparing upload...");

            if (!_page!.Url.Contains("chat.openai.com"))
            {
                await _page.GotoAsync("https://chat.openai.com/", 
                    new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                await Task.Delay(1000);
            }

            UpdateStatus($"Uploading {filePaths.Count} file(s)...");

            var fileInput = _page.Locator("input[type='file']").First;
            await fileInput.SetInputFilesAsync(filePaths);
            
            UpdateStatus("Waiting for upload...");
            await Task.Delay(3000);

            UpdateStatus("Typing question...");

            var textarea = _page.Locator("textarea").First;
            await textarea.WaitForAsync();
            await textarea.FillAsync(question);

            UpdateStatus("Submitting...");

            await _page.Keyboard.PressAsync("Enter");

            UpdateStatus("Waiting for response (10-30 seconds)...");
            
            _cts = new CancellationTokenSource(TimeSpan.FromSeconds(180));
            
            try
            {
                await _page.Locator(".markdown").Last.WaitForAsync(
                    new LocatorWaitForOptions { Timeout = 180000 });
                
                await Task.Delay(3000);
                
                var responseElement = _page.Locator(".markdown").Last;
                var responseText = await responseElement.InnerTextAsync();
                
                txtResponse.Text = responseText;
                UpdateStatus("Response received!");
                Clipboard.SetText(responseText);
            }
            catch (OperationCanceledException)
            {
                txtResponse.Text = "Response timed out. Check browser window.";
                UpdateStatus("Timed out");
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
            _cts?.Dispose();
            _cts = null;
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
        _cts?.Cancel();
        UpdateStatus("Cancelled");
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        _cts?.Cancel();
        _context?.CloseAsync().Wait(1000);
    }
}
