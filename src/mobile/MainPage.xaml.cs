using System.Timers;
using Timer = System.Timers.Timer;

namespace mobile;

public partial class MainPage : ContentPage
{
    private readonly Random _random = new();
    private readonly ImageButton[] _moleButtons;
    
    private Timer? _gameTimer;
    private int _score = 0;

    public MainPage()
    {
        InitializeComponent();
        
        _moleButtons =
        [
            MoleButton0, MoleButton1, MoleButton2, MoleButton3, MoleButton4, MoleButton5, MoleButton6, MoleButton7, MoleButton8
        ];
        
        StartGame();
    }

    private void StartGame()
    {
        _gameTimer = new Timer(1000);
        _gameTimer.Elapsed += OnGameTimerElapsed;
        _gameTimer.Start();
    }

    private void OnGameTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.Dispatch(() =>
        {
            foreach (ImageButton button in _moleButtons)
            {
                button.Source = null;
            }

            int randomIndex = _random.Next(_moleButtons.Length);
            _moleButtons[randomIndex].Source = "mole.png";
        });
    }

    private void OnMoleClicked(object sender, EventArgs e)
    {
        ImageButton? clickedButton = sender as ImageButton;
        
        if (clickedButton?.Source != null)
        {
            clickedButton.Source = null;
            
            _score++;
            ScoreLabel.Text = $"Score: {_score}";
        }
    }
}