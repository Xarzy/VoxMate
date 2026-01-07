using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using VoxMate.MVVM.Services;

namespace VoxMate.MVVM.ViewModels
{
    public class AssistantViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly VoiceService _voiceService;
        private readonly IAssistantCommandProcessor _commandProcessor;

        // Buffer para construir la frase a partir de los deltas parciales
        private readonly StringBuilder _partialBuffer = new();
        // Último texto final notificado por el servicio (no procesado automáticamente)
        private string? _lastFinalText;

        // CancellationTokenSource para la síntesis de voz (permitir cancelación)
        private CancellationTokenSource? _speechSynthesisCts;

        private string _recognitionText = string.Empty;
        public string RecognitionText
        {
            get => _recognitionText;
            set
            {
                if (_recognitionText == value) return;
                _recognitionText = value;
                // Notificar en hilo UI
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RecognitionText)));
                });
            }
        }

        private string _assistantResponse = string.Empty;
        public string AssistantResponse
        {
            get => _assistantResponse;
            set
            {
                if (_assistantResponse == value) return;
                _assistantResponse = value;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AssistantResponse)));
                });
            }
        }

        private bool _isListening;
        public bool IsListening
        {
            get => _isListening;
            set
            {
                if (_isListening == value) return;
                _isListening = value;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsListening)));
                });
            }
        }

        public ObservableCollection<string> History { get; } = new();

        public ICommand StartListeningCommand { get; }
        public ICommand StopListeningCommand { get; }
        public ICommand CancelListeningCommand { get; }
        public ICommand ClearHistoryCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public AssistantViewModel(VoiceService voiceService, IAssistantCommandProcessor? commandProcessor = null)
        {
            _voiceService = voiceService;
            _commandProcessor = commandProcessor ?? new AssistantCommandProcessor();

            // Recibimos solo el "delta" de cada parcial (VoiceService ya envía deltas).
            // Acumulamos en _partialBuffer separando segmentos con un único espacio.
            _voiceService.PartialTextReceived += segment =>
            {
                if (string.IsNullOrWhiteSpace(segment))
                    return;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var s = segment.Trim();
                    if (_partialBuffer.Length == 0)
                        _partialBuffer.Append(s);
                    else
                    {
                        // evitar espacios dobles al concatenar
                        _partialBuffer.Append(' ').Append(s);
                    }

                    RecognitionText = _partialBuffer.ToString();
                });
            };

            // Cuando el servicio notifica el final, solo actualizamos el texto visible y
            // guardamos en _lastFinalText. NO procesamos ni añadimos al historial aquí.
            _voiceService.FinalTextReceived += text =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var final = (text ?? string.Empty).Trim();
                    _lastFinalText = string.IsNullOrEmpty(final) ? null : final;

                    // Reflejar el final en la UI (no procesar)
                    if (_lastFinalText != null)
                    {
                        _partialBuffer.Clear();
                        _partialBuffer.Append(_lastFinalText);
                        RecognitionText = _lastFinalText;
                    }
                });
            };

            StartListeningCommand = new Command(async () =>
            {
                // Preparar estado para nueva escucha
                _partialBuffer.Clear();
                _lastFinalText = null;
                RecognitionText = string.Empty;
                IsListening = true;
                await _voiceService.StartListeningAsync();
            });

            StopListeningCommand = new Command(async () =>
            {
                // El procesamiento del comando se realiza *solo* cuando el usuario detiene.
                await _voice_service_StopAndProcessAsync();
            });

            // Ahora detiene la escucha (StopListeningAsync) y limpia el estado sin procesar texto.
            CancelListeningCommand = new Command(async () =>
            {
                try
                {
                    await _voiceService.StopListeningAsync();
                }
                catch
                {
                    // intentar fallback a Cancel si Stop falla
                    try { _voiceService.Cancel(); } catch { }
                }
                finally
                {
                    // Cancelar cualquier síntesis en curso
                    _speechSynthesisCts?.Cancel();

                    // Limpiar estado relacionado con la escucha en curso en hilo UI
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _partialBuffer.Clear();
                        _lastFinalText = null;
                        RecognitionText = string.Empty;
                        IsListening = false;
                    });
                }
            });

            ClearHistoryCommand = new Command(() => History.Clear());
        }

        // Método auxiliar: detiene la escucha y procesa la última frase conocida.
        private async Task _voice_service_StopAndProcessAsync()
        {
            try
            {
                await _voiceService.StopListeningAsync();
            }
            catch
            {
                // ignorar errores de stop
            }
            finally
            {
                IsListening = false;
            }

            // Preferir el texto final notificado por el servicio si existe, si no, usar el buffer parcial.
            var text = (_lastFinalText ?? _partialBuffer.ToString()).Trim();
            if (string.IsNullOrEmpty(text))
            {
                // Limpiar estados
                _partialBuffer.Clear();
                _lastFinalText = null;
                return;
            }

            // Añadir al historial y procesar EL COMANDO sólo ahora (cuando el usuario ha detenido).
            MainThread.BeginInvokeOnMainThread(() =>
            {
                History.Add("👤 " + text);
                var response = _commandProcessor.Process(text);
                History.Add("🤖 " + response);
                AssistantResponse = response;

                // reproducir respuesta por altavoz (no bloquear UI)
                _ = SpeakResponseAsync(response);

                // limpiar estado para la siguiente grabación
                _partialBuffer.Clear();
                _lastFinalText = null;
                RecognitionText = text;
            });
        }

        // Sintetiza texto por el altavoz; cancela síntesis previa si existe.
        private async Task SpeakResponseAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            try
            {
                _speechSynthesisCts?.Cancel();
                _speechSynthesisCts = new CancellationTokenSource();

                Locale? locale = null;
                var locales = await TextToSpeech.Default.GetLocalesAsync();
                foreach (var l in locales)
                {
                    if (l.Id == "es-ES")
                    {
                        locale = l;
                        break;
                    }
                }

                var options = new SpeechOptions
                {
                    Locale = locale,
                    Pitch = 1.0f,
                    Volume = 1.0f
                };

                await TextToSpeech.Default.SpeakAsync(text, options, _speechSynthesisCts.Token);
            }
            catch (OperationCanceledException)
            {
                // síntesis cancelada: ignorar
            }
            catch
            {
                // ignorar errores de TTS
            }
            finally
            {
                // liberar si fue cancelado
                if (_speechSynthesisCts?.IsCancellationRequested == true)
                {
                    _speechSynthesisCts.Dispose();
                    _speechSynthesisCts = null;
                }
            }
        }

        public void OnAppClosing()
        {
            // cancelar escucha y cualquier síntesis en curso
            _voiceService.Cancel();
            _speechSynthesisCts?.Cancel();
        }
    }
}