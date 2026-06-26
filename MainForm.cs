using System.Diagnostics;
using System.Text.RegularExpressions;
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
    private string _lastQuestion = "";

    public MainForm()
    {
        InitializeComponent();
        
        // Store browser session in user's AppData (persists login)
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
            // Already connected - just refresh
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

            // Install Chromium if needed
            if (!PlaywrightClientFactory.IsChromiumInstalled)
            {
                UpdateStatus("Installing Chromium (one-time download)...");
                var progress = new Progress<int>(p => UpdateStatus($"Installing... {p}%"));
                await Task.Run(() => 
                {
                    var installTask = Microsoft.Playwright.Playwright.InstallAsync();
                    installTask.Wait();
                });
            }

            UpdateStatus("Launching browser...");

            // Launch with persistent context (keeps your login session)
            _context = await Microsoft.Playwright.ChromiumBrowserType
                .LaunchPersistentContextAsync(
                    _browserSessionPath,
                    new BrowserContextLaunchOptions
                    {
                        Headless = false,
                        Args = new[] { "--disable-blink-features=AutomationControlled" },
                        NoViewport = true
                    });

            _page = _context.Pages.FirstOrDefault() ?? await _context.NewPageAsync();
            
            UpdateStatus("Opening ChatGPT...");
            await _page.GotoAsync("https://chat.openai.com/", 
                new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            // Check login status
            await CheckLoginStatusAsync();

            _isConnected = true;
            btnConnect.Text = "Refresh ChatGPT";
            btnSend.Enabled = true;
            
            UpdateStatus("Connected! Drag & drop files or click 'Upload & Ask'");
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
            // Wait a moment for page to settle
            await Task.Delay(1000);
            
            // Check for login elements
            var loginLocator = _page.GetByText("Log in").FirstOrDefaultAsync().AsTask();
            var completed = await Task.WhenAny(loginLocator, Task.Delay(3000));
            
            if (!ReferenceEquals(completed, loginLocator))
            {
                UpdateStatus("Already logged in to ChatGPT!");
                return;
            }

            if (loginLocator.Result != null)
            {
                var result = MessageBox.Show(
                    "You need to log in to ChatGPT.\n\n" +
                    "1. Click 'OK' to continue\n" +
                    "2. Log in with your account (including MFA)\n" +
                    "3. Click 'New chat' if needed\n" +
                    "4. Return here and click 'Connect' again",
                    "Login Required",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Information);

                if (result == DialogResult.OK)
                {
                    UpdateStatus("Waiting for login...");
                    // Open login page
                    await _page.GotoAsync("https://chat.openai.com/auth/login");
                }
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

        // Validate files exist
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
            question = "Analyze this code. Explain what it does and suggest any improvements.";
        }
        _lastQuestion = question;

        await UploadAndAskAsync(filePaths, question);
    }

    private async Task UploadAndAskAsync(List<string> filePaths, string question)
    {
        try
        {
            btnSend.Enabled = false;
            UpdateStatus("Preparing upload...");

            // Make sure we're on ChatGPT
            if (!_page!.Url.Contains("chat.openai.com"))
            {
                await _page.GotoAsync("https://chat.openai.com/", 
                    new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
                await Task.Delay(1000);
            }

            UpdateStatus($"Uploading {filePaths.Count} file(s)...");

            // Find the file input element - ChatGPT has a hidden file input
            var fileInput = _page.Locator("input[type='file']").First;
            
            // Build the file paths string for Playwright
            var filePathsString = string.Join("\n", filePaths);
            
            // Set files on the input
            await fileInput.SetInputFilesAsync(filePaths);
            
            UpdateStatus("Waiting for upload to complete...");
            
            // Wait for files to be attached (ChatGPT shows attachment chips)
            try
            {
                await _page.WaitForSelector("[data-attached-files]", new PageWaitForSelectorOptions { Timeout = 30000 });
            }
            catch
            {
                // Try alternate selector
                await Task.Delay(3000);
            }

            UpdateStatus("Files uploaded! Typing question...");

            // Wait for textarea to be ready
            await WaitForTextareaAsync();
            
            // Find the textarea and type question
            var textarea = _page.Locator("textarea").First;
            await textarea.FillAsync(question);

            UpdateStatus("Submitting to ChatGPT...");

            // Try to find and click send button
            try
            {
                var sendButton = _page.Locator("button").Filter(new LocatorFilterOptions 
                { 
                    HasText = new Regex("send|submit|arrow", RegexOptions.IgnoreCase) 
                }).First;
                await sendButton.ClickAsync(new LocatorClickOptions { Timeout = 5000 });
            }
            catch
            {
                // Press Enter as fallback
                await _page.Keyboard.PressAsync("Enter");
            }

            UpdateStatus("Waiting for ChatGPT response...\n(This takes 10-30 seconds)");
            
            // Wait for response
            _cts = new CancellationTokenSource(TimeSpan.FromSeconds(180));
            
            try
            {
                // Wait for response to appear
                await _page.WaitForSelector(".markdown", 
                    new PageWaitForSelectorOptions { Timeout = 180000 });
                
                await Task.Delay(3000); // Wait for full response
                
                // Get the response
                var responseElement = _page.Locator(".markdown").Last;
                var responseText = await responseElement.InnerTextAsync();
                
                txtResponse.Text = responseText;
                UpdateStatus("✅ Response received!");
                
                // Copy to clipboard
                Clipboard.SetText(responseText);
                MessageBox.Show("Response copied to clipboard!", "Done", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                txtResponse.Text = "⏱️ Response timed out.\n\nCheck the browser window for the response.";
                UpdateStatus("⏱️ Response timed out");
            }
        }
        catch (Exception ex)
        {
            txtResponse.Text = $"❌ Error: {ex.Message}";
            UpdateStatus($"Error: {ex.Message}");
        }
        finally
        {
            btnSend.Enabled = _isConnected;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private async Task WaitForTextareaAsync()
    {
        if (_page == null) return;

        var selectors = new[]
        {
            "textarea[data-id='root']",
            "textarea[id='prompt-textarea']", 
            "div[contenteditable='true']",
            "textarea[name='prompt']",
            "textarea"
        };

        foreach (var selector in selectors)
        {
            try
            {
                var element = _page.Locator(selector).First;
                await element.WaitForAsync(new LocatorWaitForOptions { Timeout = 10000 });
                return;
            }
            catch
            {
                continue;
            }
        }

        throw new Exception("Could not find ChatGPT input textarea");
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
