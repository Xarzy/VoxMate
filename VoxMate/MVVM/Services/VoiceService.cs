using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Media;
using System.Globalization;

namespace VoxMate.MVVM.Services
{
    public class VoiceService
    {
        private readonly ISpeechToText _speechToText;
        private CancellationTokenSource? _speechCts;
        private string? _lastPartialText;

        public bool IsListening { get; private set; }

        public event Action<string>? PartialTextReceived;
        public event Action<string>? FinalTextReceived;

        public VoiceService()
        {
            _speechToText = SpeechToText.Default;
        }

        public async Task StartListeningAsync()
        {
            _speechCts?.Cancel();
            _speechCts = new CancellationTokenSource();

            try
            {
                var granted = await _speechToText.RequestPermissions(_speechCts.Token);
                if (!granted)
                {
                    await Toast.Make("Sin permiso de micrófono").Show();
                    return;
                }

                _speechToText.RecognitionResultUpdated -= OnRecognitionUpdated;
                _speechToText.RecognitionResultCompleted -= OnRecognitionCompleted;

                _speechToText.RecognitionResultUpdated += OnRecognitionUpdated;
                _speechToText.RecognitionResultCompleted += OnRecognitionCompleted;

                IsListening = true;

                await _speechToText.StartListenAsync(
                    new SpeechToTextOptions
                    {
                        Culture = new CultureInfo("es-ES"),
                        ShouldReportPartialResults = true
                    },
                    _speechCts.Token);

            }
            catch
            {
                await Toast.Make("Error de reconocimiento de voz").Show();
            }
        }

        public async Task StopListeningAsync()
        {
            try
            {
                await Task.Delay(300);
                _speechCts?.Cancel();
                await _speechToText.StopListenAsync(CancellationToken.None);
            }
            catch { }
            finally
            {
                IsListening = false;
                _speechToText.RecognitionResultUpdated -= OnRecognitionUpdated;
                _speechToText.RecognitionResultCompleted -= OnRecognitionCompleted;
            }
        }

        private void OnRecognitionUpdated(object? sender,
            SpeechToTextRecognitionResultUpdatedEventArgs e)
        {
            _lastPartialText = e.RecognitionResult;
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PartialTextReceived?.Invoke(e.RecognitionResult);
            });
        }

        private void OnRecognitionCompleted(object? sender,
            SpeechToTextRecognitionResultCompletedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                FinalTextReceived?.Invoke(e.RecognitionResult.Text);
                IsListening = false;
            });
        }


        public void Cancel()
        {
            _speechCts?.Cancel();
        }
    }
}
