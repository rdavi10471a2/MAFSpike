using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace CodexFileQuery;

public partial class MainForm : Form
{
    private readonly HttpListener? _listener;
    private readonly string _port = "9222";
    private string? _wsUrl;

    public MainForm()
    {
        InitializeComponent();
        UpdateStatus("Ready");
    }

    private async void BtnConnect_Click(object sender, EventArgs e)
    {
        try
        {
            UpdateStatus("Launching Chrome with debugging...");
            btnConnect.Enabled = false;
            btnSend.Enabled = false;

            // Find Chrome
            string chromePath = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
            if (!File.Exists(chromePath))
                chromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";

            // Launch Chrome with remote debugging
            var psi = new ProcessStartInfo
            {
                FileName = chromePath,
                Arguments = $"--remote-debugging-port={_port} --profile-directory=Default https://chat.openai.com/",
                UseShellExecute = true
            };

            Process.Start(psi);
            await Task.Delay(3000);

            // Get WebSocket URL
            var client = new HttpClient();
            var response = await client.GetStringAsync($"http://localhost:{_port}/json");
            var tabs = JsonDocument.Parse(response);
            var firstTab = tabs.RootElement.EnumerateArray().FirstOrDefault();
            
            if (firstTab.TryGetProperty("webSocketDebuggerUrl", out var wsUrl))
            {
                _wsUrl = wsUrl.GetString();
                _isConnected = true;
                btnSend.Enabled = true;
                UpdateStatus("Connected! Ready to send.");
                MessageBox.Show("Chrome opened and connected!\n\nSelect a file and click Send.", "Connected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                UpdateStatus("Could not connect to Chrome");
                MessageBox.Show("Could not connect to Chrome debugging port.\n\nMake sure Chrome is closed, then try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error: {ex.Message}");
            MessageBox.Show($"Failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnConnect.Enabled = true;
        }
    }

    private bool _isConnected;

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
                ? "Analyze this code and suggest improvements." 
                : txtQuestion.Text.Trim();

            string prompt = $"File `{fileName}`:\n\n```{fileContent}\n```\n\n{question}";

            UpdateStatus("Sending to ChatGPT...");

            await SendToChatGPT(prompt);

            UpdateStatus("Sent! Waiting for response...");
            
            // Give time for response
            await Task.Delay(10000);
            
            UpdateStatus("Done! Check ChatGPT for response.");
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

        // Get page info
        var infoCmd = CreateCdpCommand(1, "Page.getResourceTree");
        await ws.SendAsync(Encoding.UTF8.GetBytes(infoCmd), WebSocketMessageType.Text, true, CancellationToken.None);
        
        await Task.Delay(500);
        
        // Focus textarea and type
        var typeCmd = CreateCdpCommand(2, "Runtime.evaluate", 
            new { expression = @"
                (function() {
                    // Try to find textarea
                    let ta = document.querySelector('textarea');
                    if (!ta) {
                        // Try contenteditable
                        ta = document.querySelector('[contenteditable=""true""]');
                    }
                    if (ta) {
                        ta.focus();
                        ta.value = arguments[0];
                        ta.dispatchEvent(new Event('input', { bubbles: true }));
                        return 'found';
                    }
                    return 'not found';
                })('" + text.Replace("'", "\\'").Replace("\n", "\\n") + @"')
            " });
        await ws.SendAsync(Encoding.UTF8.GetBytes(typeCmd), WebSocketMessageType.Text, true, CancellationToken.None);
        
        await Task.Delay(1000);
        
        // Press Enter to send
        var enterCmd = CreateCdpCommand(3, "Runtime.evaluate",
            new { expression = @"
                (function() {
                    let ta = document.querySelector('textarea');
                    if (!ta) ta = document.querySelector('[contenteditable=""true""]');
                    if (ta) {
                        ta.dispatchEvent(new KeyboardEvent('keydown', { key: 'Enter', code: 'Enter', keyCode: 13, bubbles: true }));
                        ta.dispatchEvent(new KeyboardEvent('keyup', { key: 'Enter', code: 'Enter', keyCode: 13, bubbles: true }));
                        return 'sent';
                    }
                    return 'failed';
                })()
            " });
        await ws.SendAsync(Encoding.UTF8.GetBytes(enterCmd), WebSocketMessageType.Text, true, CancellationToken.None);

        await Task.Delay(500);
        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
    }

    private string CreateCdpCommand(int id, string method, object? args = null)
    {
        var cmd = new { id, method, @params = args ?? new { } };
        return JsonSerializer.Serialize(cmd);
    }

    private void UpdateStatus(string message)
    {
        lblStatus.Text = message;
        lblStatus.Invalidate();
        lblStatus.Update();
        Application.DoEvents();
    }
}
