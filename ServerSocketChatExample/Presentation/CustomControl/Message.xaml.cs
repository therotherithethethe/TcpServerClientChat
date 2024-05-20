using System.Windows;
using System.Windows.Controls;

namespace ServerSocketChatExample.Presentation.CustomControl;

public partial class Message : UserControl
{
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(Message), new PropertyMetadata(string.Empty, OnTextChanged));

    public Message()
    {
        InitializeComponent();
    }
    private void OnTextChanged(DependencyPropertyChangedEventArgs e)
    {
        TextBlock.Text = e.NewValue as string;
    }   
    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as Message;
        control?.OnTextChanged(e);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}