using System.IO;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SmertMuzika
{

    public partial class MainWindow : Window
    {
        private string[] audioFiles;
        private string[] originalAudioFiles;
        private int currentAudioIndex = 0;
        private bool isPlaying = false;
        private bool isRepeating = false;
        private bool isShuffling = false;
        private double currentPosition = 0;
        private double totalDuration = 0;
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        public MainWindow()
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        {
            InitializeComponent();
        }
        private void OpenMusicFloder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                Filter = "Audio Files|*.mp3;*.m4a;*.wav|All Files|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
#pragma warning disable CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
                audioFiles = Directory.GetFiles(Path.GetDirectoryName(dialog.FileName), "*.mp3");
#pragma warning restore CS8604 // Возможно, аргумент-ссылка, допускающий значение NULL.
                originalAudioFiles = audioFiles.ToArray();
                AudioFilesListBox.Items.Clear();

                foreach (var file in audioFiles)
                {
                    AudioFilesListBox.Items.Add(Path.GetFileName(file));
                }
                if (audioFiles.Length > 0)
                {
                    Media.Source = new Uri(audioFiles[currentAudioIndex]);
                    Media.Play();
                    UpdateUI();
                    AudioPlayer.AddToHistory(audioFiles[currentAudioIndex]);
                }
            }
        }
        private void AudioFilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AudioFilesListBox.SelectedItem != null)
            {
                currentAudioIndex = AudioFilesListBox.SelectedIndex;

                if (audioFiles != null && currentAudioIndex < audioFiles.Length)
                {
                    string selectedAudioFile = audioFiles[currentAudioIndex];
                    AudioPlayer.AddToHistory(selectedAudioFile);
                    Media.Source = new Uri(selectedAudioFile);
                    Media.Play();                   
                }
            }
        }
        private void AudioSilder_ValueChanded(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Abs(AudioSilder.Value - Media.Position.TotalSeconds) > 1)
            {
                Media.Position = TimeSpan.FromSeconds(AudioSilder.Value);
            }
        }
        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {         
            if (isRepeating)
            {
                Media.Position = TimeSpan.Zero;
                Media.Play();
            }
            else
            {               
                PlayNextAudio();
            }
        }
        private void Media_MediaOpened(object sender, RoutedEventArgs e)
        {
            Media.MediaEnded += Media_MediaEnded;
            AudioSilder.Maximum = Media.NaturalDuration.TimeSpan.TotalSeconds;
#pragma warning disable CS8622 // Допустимость значений NULL для ссылочных типов в типе параметра не соответствует целевому объекту делегирования (возможно, из-за атрибутов допустимости значений NULL).
            CompositionTarget.Rendering += UpdateSliderPosition;
#pragma warning restore CS8622 // Допустимость значений NULL для ссылочных типов в типе параметра не соответствует целевому объекту делегирования (возможно, из-за атрибутов допустимости значений NULL).
        }
        private void UpdateSliderPosition(object sender, EventArgs e)
        {
            if (Media.Source != null && Media.NaturalDuration.HasTimeSpan)
            {
                AudioSilder.Value = Media.Position.TotalSeconds;
            }
        }  
        private void AudioVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Media.Volume = AudioVolume.Value;
        }
        private void PlayBackAudio()
        {
            if (audioFiles != null && audioFiles.Length > 0)
            {
                currentAudioIndex--;
                if (currentAudioIndex < 0)
                {
                    currentAudioIndex = audioFiles.Length - 1;
                }
                string nextAudioFile = audioFiles[currentAudioIndex];
                Media.Source = new Uri(nextAudioFile);
                Media.Play();
                AudioPlayer.AddToHistory(audioFiles[currentAudioIndex]);
            }
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            PlayBackAudio();
        }
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                Media.Pause();
                isPlaying = false;
                Play.Content = "Play";
            }
            else
            {
                Media.Play();
                isPlaying = true;
                Play.Content = "Pause"; 
            }
        }
        private void PlayNextAudio()
        {
            if (audioFiles != null && audioFiles.Length > 0)
            {
                currentAudioIndex++;
                if (currentAudioIndex >= audioFiles.Length)
                {
                    currentAudioIndex = 0;
                }
                string nextAudioFile = audioFiles[currentAudioIndex];

                Media.Source = new Uri(nextAudioFile);
                Media.Play();
                AudioPlayer.AddToHistory(audioFiles[currentAudioIndex]);
            }
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            PlayNextAudio();
        }
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
        private async Task ShufflePlaylistAsync()
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
        {
            if (audioFiles != null && audioFiles.Length > 0)
            {
                List<string> shuffledPlaylist = audioFiles.OrderBy(x => Guid.NewGuid()).ToList();
                audioFiles = shuffledPlaylist.ToArray();

                AudioFilesListBox.Items.Clear();
                foreach (var item in shuffledPlaylist)
                {
                    AudioFilesListBox.Items.Add(Path.GetFileName(item));
                }

                currentAudioIndex = 0;
                string nextAudioFile = audioFiles[currentAudioIndex];
                Media.Source = new Uri(nextAudioFile);
                Media.Play();
                AudioPlayer.AddToHistory(audioFiles[currentAudioIndex]);
            }
        }
#pragma warning disable CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
        private async Task RestorePlaylistOrderAsync()
#pragma warning restore CS1998 // В асинхронном методе отсутствуют операторы await, будет выполнен синхронный метод
        {
            if (originalAudioFiles != null && originalAudioFiles.Length > 0)
            {
                currentAudioIndex = 0;
                audioFiles = originalAudioFiles;
                AudioFilesListBox.Items.Clear();
                foreach (var item in originalAudioFiles)
                {
                    AudioFilesListBox.Items.Add(Path.GetFileName(item));
                }

                string nextAudioFile = audioFiles[currentAudioIndex];               
                Media.Source = new Uri(nextAudioFile);
                Media.Play();
                AudioPlayer.AddToHistory(audioFiles[currentAudioIndex]);
            }
        }
        private async void Random_Click(object sender, RoutedEventArgs e)
        {
            isShuffling = !isShuffling;
            Randomm.Content = isShuffling ? "Random On" : "Random Off";

            if (isShuffling)
            {
                await ShufflePlaylistAsync();
            }
            else
            {
                await RestorePlaylistOrderAsync();
            }
        }

        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            isRepeating = !isRepeating;
            Repeat.Content = isRepeating ? "Repeat On" : "Repeat Off";

            if (isRepeating)
            {
                Media.MediaEnded += Media_MediaEnded;
            }
            else
            {
                Media.MediaEnded -= Media_MediaEnded;
            }
        }

        public class AudioPlayer
        {
            private static List<string> listeningHistory = new List<string>();
            public static int currentAudioIndex = 0;
            public static void PlaySelectedAudio(string audio, MediaElement Media)
            {               
                Media.Source = new Uri(audio);
                Media.Play();
                AddToHistory(audio);
            }
            public static List<string> GetListeningHistory()
            {
                return listeningHistory;
            }
            public static void AddToHistory(string audio)
            {
                listeningHistory.Add(audio);
            }
        }
        private void UpdateUI()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (sender, e) =>
            {
                currentPosition = GetAudioPlayerCurrentPosition();
                totalDuration = GetAudioPlayerTotalDuration();

                TimeSpan currentTimeSpan = TimeSpan.FromSeconds(currentPosition);
                TimeSpan remainingTimeSpan = TimeSpan.FromSeconds(totalDuration - currentPosition);

                string currentTime = currentTimeSpan.ToString(@"mm\:ss");
                string remainingTime = remainingTimeSpan.ToString(@"mm\:ss");

                currentTimeTextBlock.Text = $"Current Time: {currentTime}";
                remainingTimeTextBlock.Text = $"Remaining Time: {remainingTime}";
            };
            timer.Start();
        }
        private double GetAudioPlayerCurrentPosition()
        {
            return Media.Position.TotalSeconds;
        }
        private double GetAudioPlayerTotalDuration()
        {
            if (Media.NaturalDuration.HasTimeSpan)
            {
                return Media.NaturalDuration.TimeSpan.TotalSeconds;
            }return 0;
        }

        
    }
}

