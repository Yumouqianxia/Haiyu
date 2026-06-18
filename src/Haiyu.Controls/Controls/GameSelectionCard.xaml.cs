namespace Haiyu.Controls.Controls;

public sealed partial class GameSelectionCard : UserControl
{
    public GameSelectionCard()
    {
        InitializeComponent();
    }

    public ImageSource CardImage
    {
        get { return (ImageSource)GetValue(CardImageProperty); }
        set { SetValue(CardImageProperty, value); }
    }

    public static readonly DependencyProperty CardImageProperty = DependencyProperty.Register(
        nameof(CardImage),
        typeof(ImageSource),
        typeof(GameSelectionCard),
        new PropertyMetadata(null)
    );

    public object CardContent
    {
        get { return (object)GetValue(CardContentProperty); }
        set { SetValue(CardContentProperty, value); }
    }

    public static readonly DependencyProperty CardContentProperty = DependencyProperty.Register(
        nameof(CardContent),
        typeof(object),
        typeof(GameSelectionCard),
        new PropertyMetadata(null)
    );

    private void backGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        this.cardContet.Visibility = Visibility.Visible;
    }

    private void backGrid_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        this.cardContet.Visibility = Visibility.Collapsed;
    }
}
