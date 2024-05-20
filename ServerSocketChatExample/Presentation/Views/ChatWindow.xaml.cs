using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ServerSocketChatExample.Presentation.CustomControl;

namespace ServerSocketChatExample.Presentation.Views;

/// <summary>
///     Interaction logic for ChatWindow.xaml
/// </summary>
public partial class ChatWindow : Window
{
    private StreamReader _reader;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private Socket _socket;
    private StreamWriter _writer;
    private DispatcherTimer _timer;

    public ChatWindow()
    {
        InitializeComponent();
        ConnectToServer();
        InitializeTimer();
    }

    private void InitializeTimer()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _timer.Tick += async (sender, e) => await GetMessageFromServer();
        _timer.Start();
    }

    private async Task GetMessageFromServer()
    {
        await _semaphore.WaitAsync();
        try
        {
            var message = await _reader.ReadLineAsync();
            if (message != null)
                Dispatcher.Invoke(() =>
                {
                    var messageControl = new Message
                    {
                        Text = message
                    };
                    MessagePanel.Children.Add(messageControl);
                });
        }
        catch (Exception ex)
        {
            // Handle exceptions (log, show message, etc.)
            Dispatcher.Invoke(() => { MessageBox.Show($"Error: {ex.Message}"); });
            DisposeResources();
        }
        finally
        {
            _semaphore.Release(); // Release the semaphore
        }
    }

    private void DisposeResources()
    {
        _reader.Dispose();
        _writer.Dispose();
        _socket.Close();
        _semaphore.Dispose();
        _timer.Stop();
    }

    private void ConnectToServer()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect("localhost", 13337);
        var networkStream = new NetworkStream(_socket);
        _writer = new StreamWriter(networkStream) { AutoFlush = true };
        _reader = new StreamReader(networkStream);
    }


    private async void SendMessageBtn_OnClick(object sender, RoutedEventArgs e)
    {
        await SendMessage(UserInput.Text);
    }

    private async Task SendMessage(string message)
    {
        message = message.TrimStart();
        if (!CheckMessage(message))
            return;

        await _writer.WriteLineAsync(message);
        UserInput.Clear();
        await _semaphore.WaitAsync();
        //var response = await _reader.ReadLineAsync();
        /*if (!string.IsNullOrEmpty(response))
        {
            Dispatcher.Invoke(() =>
            {
                var messageControl = new Message
                {
                    Text = response
                };
                MessagePanel.Children.Add(messageControl);
                
            });

        }*/
    }

    private static bool CheckMessage(string message)
    {
        return message is not "";
    }

    private void UserInput_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter) SendMessage(UserInput.Text);
    }
}