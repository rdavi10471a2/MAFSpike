using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace CodexFileQuery;

public partial class MainForm : Form
{
    private string? _wsUrl;
    private bool _isConnected;

    public MainForm()
    {
        InitializeComponent();
        UpdateStatus("Ready - Close Chrome first, then click Connect");
    }

    private async void BtnConnect_Click(object sender, EventArgs e)
    {
        try
        {
            UpdateStatus("Starting Chrome with debugging...");
            btnConnect.Enabled = false;
            btnSend.Enabled = false;

            // Kill any existing Chrome with debugging port
            foreach (var p in Process.GetProcessesByName("chrome"))
            {
                try { p.Kill(); } catch { }
            }
            await Task.Delay(1000);

            // Find Chrome
            string chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
            if (!File.Exists(chromePath))
                chromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";

            // Launch Chrome with remote debugging on specific port
            Process.Start(new ProcessStartInfo
            {
                FileName = chromePath,
                Arguments = "--remote-debugging-port=9222 --new-window https://chat.openai.com/",
                UseShellExecute = true
            });

            UpdateStatus("Waiting for Chrome...");
            await Task.Delay(4000);

            // Get WebSocket URL from Chrome
            using var client = new HttpClient();
            var response = await client.GetStringAsync("http://localhost:9222/json");
            var tabs = JsonDocument.Parse(response);
            
            var firstTab = tabs.RootElement.EnumerateArray().FirstOrDefault();
            if (firstTab.TryGetProperty("webSocketDebuggerUrl", out var wsUrlProp))
            {
                _wsUrl = wsUrlProp.GetString();
                _isConnected = true;
                btnSend.Enabled = true;
                UpdateStatus("Connected to Chrome!");
                MessageBox.Show("Chrome opened!\n\nClick Send to upload file to ChatGPT.", "Connected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                UpdateStatus("Failed to connect");
                MessageBox.Show("Could not connect to Chrome debugging port.\n\nClose all Chrome windows and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnConnect.Enabled = true;
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error: {ex.Message}");
            MessageBox.Show($"Failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnConnect.Enabled = true;
        }
    }

    private void BtnBrowseFile_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Select file",
            Filter = "All files (*.*)|*.*|C# (*.cs)|*.cs|Python (*.py)|*.py"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            txtFilePath.Text = dialog.FileName;
        }
    }

    private async void BtnSend_Click(object sender, EventArgs e)
    {
        if (!_isConnected || string.IsNullOrEmpty(_wsUrl))
        {
            MessageBox.Show("Click 'Connect' first.", "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var filePath = txtFilePath.Text.Trim();
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            MessageBox.Show("Select a valid file.", "No File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            btnSend.Enabled = false;
            UpdateStatus("Reading file...");

            string fileContent = File.ReadAllText(filePath);
            string fileName = Path.GetFileName(filePath);
            string question = string.IsNullOrWhiteSpace(txtQuestion.Text) 
                ? "Analyze this code." 
                : txtQuestion.Text.Trim();

            string prompt = $"```{fileName}\n{fileContent}\n```\n\n{question}";

            UpdateStatus("Sending to ChatGPT...");
            await SendToChatGPT(prompt);
            UpdateStatus("Sent! Check ChatGPT for response.");
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error: {ex.Message}");
            MessageBox.Show($"Failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnSend.Enabled = true;
        }
    }

    private async Task SendToChatGPT(string text)
    {
        using var ws = new ClientWebSocket();
        await ws.ConnectAsync(new Uri(_wsUrl!), CancellationToken.None);

        // Inject the text into textarea
        string escaped = text.Replace("`", "\\`").Replace("\n", "\\n").Replace("'", "\\'");
        string js = $"document.querySelector('textarea').value = `{escaped}`; document.querySelector('textarea').dispatchEvent(new Event('input', {{bubbles:true}}));";
        
        var cmd = JsonSerializer.Serialize(new { id = 1, method = "Runtime.evaluate", @params = new { expression = js } });
        await ws.SendAsync(Encoding.UTF8.GetBytes(cmd), WebSocketMessageType.Text, true, CancellationToken.None);
        
        await Task.Delay(500);
        
        // Press Enter
        string enterJs = "document.querySelector('textarea').dispatchEvent(new KeyboardEvent('keydown', {key:'Enter', code:'Enter', keyCode:13, bubbles:true}));";
        var enterCmd = JsonSerializer.Serialize(new { id = 2, method = "Runtime.evaluate", @params = new { expression = enterJs } });
        await ws.SendAsync(Encoding.UTF8.GetBytes(enterCmd), WebSocketMessageType.Text, true, CancellationToken.None);
        
        await Task.Delay(1000);
        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
    }

    private void UpdateStatus(string message)
    {
        lblStatus.Text = message;
        lblStatus.Invalidate();
        lblStatus.Update();
        Application.DoEvents();
    }
}
