using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using VoxMate.MVVM.ViewModels;

namespace VoxMate.MVVM.Views;

public partial class AssistantPage : ContentPage
{
    private readonly AssistantViewModel _vm;

    enum SliderState { Idle, Listening }
    private SliderState _state = SliderState.Idle;

    private double _trackWidth = 0;
    private double _thumbWidth = 0;
    private double _halfRange = 0;   // half of full movable range (center-based)
    private double _fullRange = 0;   // full range = 2 * halfRange

    private const double StartThreshold = 0.6;
    private const double ActionThreshold = 0.45;

    public AssistantPage(AssistantViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;

        SwipeSliderTrack.SizeChanged += OnTrackSizeChanged;
        SwipeSliderThumb.SizeChanged += OnThumbSizeChanged;

        ResetTrackVisuals();
    }

    private void OnTrackSizeChanged(object? sender, EventArgs e)
    {
        _trackWidth = SwipeSliderTrack.Width;
        RecalcRanges();
    }

    private void OnThumbSizeChanged(object? sender, EventArgs e)
    {
        _thumbWidth = SwipeSliderThumb.Width;
        RecalcRanges();
    }

    private void RecalcRanges()
    {
        if (_trackWidth <= 0 || _thumbWidth <= 0) return;

        // half range measured from center to edge: (trackWidth - thumbWidth) / 2
        _halfRange = Math.Max(0, (_trackWidth - _thumbWidth) / 2.0);
        _fullRange = _halfRange * 2.0;

        // Idle state: place thumb at left extreme (-halfRange)
        if (_state == SliderState.Idle)
            SwipeSliderThumb.TranslationX = -_halfRange;
        else
            SwipeSliderThumb.TranslationX = 0;
    }

    private Color Lerp(Color a, Color b, double t)
    {
        t = Math.Clamp(t, 0, 1);
        return Color.FromRgba(
            (float)(a.Red + (b.Red - a.Red) * t),
            (float)(a.Green + (b.Green - a.Green) * t),
            (float)(a.Blue + (b.Blue - a.Blue) * t),
            (float)(a.Alpha + (b.Alpha - a.Alpha) * t)
        );
    }

    private Color GetColorResource(string key, Color fallback)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var obj) == true && obj is Color c)
            return c;
        return fallback;
    }

    private void OnThumbPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (_fullRange <= 0) return;

        switch (e.StatusType)
        {
            case GestureStatus.Running:
                // TotalX is relative to where the pan started. We use it adding to initial position.
                var proposed = SwipeSliderThumb.TranslationX + e.TotalX - e.TotalX; // keep as current translation + delta
                // But simpler: use e.TotalX relative to pan start. We need initial touch point translation:
                // We'll use e.TotalX plus starting translation captured on Started.
                // For simplicity, clamp by current formula: compute newX = startingTranslation + e.TotalX
                // We'll store startingTranslation in the Thumb's BindingContext during Started phase.
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                var finalX = SwipeSliderThumb.TranslationX;
                _ = HandleRelease(finalX);
                break;

            case GestureStatus.Started:
                // store start translation
                SwipeSliderThumb.BindingContext = SwipeSliderThumb.TranslationX;
                break;
        }

        // Note: we implement a per-update approach below to compute new translation safely
        if (e.StatusType == GestureStatus.Running)
        {
            if (!(SwipeSliderThumb.BindingContext is double startTx)) startTx = SwipeSliderThumb.TranslationX;
            var newX = startTx + e.TotalX;

            // clamp to [-halfRange, +halfRange]
            newX = Math.Clamp(newX, -_halfRange, _halfRange);
            SwipeSliderThumb.TranslationX = newX;
            UpdateVisualsByPosition(newX);
        }
    }

    private void UpdateVisualsByPosition(double dx)
    {
        // In Idle: dx ranges from -halfRange..+halfRange but initial is -halfRange.
        if (_halfRange == 0) return;

        if (_state == SliderState.Idle)
        {
            // progress 0..1 from leftmost (-halfRange) to rightmost (+halfRange)
            var progress = (dx + _halfRange) / _fullRange;
            var baseColor = GetColorResource("CardBackground", Colors.LightGray);
            var accent = GetColorResource("AccentColor", Colors.Blue);
            var t = Math.Clamp(progress, 0, 1);
            SwipeSliderTrack.BackgroundColor = Lerp(baseColor, accent, t * 0.95);

            CenterHint.Opacity = 1.0 - t;
            LeftHint.Opacity = 0;
            RightHint.Opacity = 0;
        }
        else // Listening
        {
            var norm = dx / _halfRange; // -1..1 center-based
            var baseColor = GetColorResource("CardBackground", Colors.LightGray);
            var green = Colors.Green;
            var red = Colors.Red;
            if (norm < 0)
            {
                var t = Math.Clamp(-norm, 0, 1);
                SwipeSliderTrack.BackgroundColor = Lerp(baseColor, green, t * 0.95);
                LeftHint.Opacity = t;
                RightHint.Opacity = 0;
                CenterHint.Opacity = 1 - t;
            }
            else
            {
                var t = Math.Clamp(norm, 0, 1);
                SwipeSliderTrack.BackgroundColor = Lerp(baseColor, red, t * 0.95);
                RightHint.Opacity = t;
                LeftHint.Opacity = 0;
                CenterHint.Opacity = 1 - t;
            }
        }
    }

    private async Task HandleRelease(double finalX)
    {
        if (_fullRange == 0) return;
        // Idle: compute progress from left->right
        if (_state == SliderState.Idle)
        {
            var progress = (finalX + _halfRange) / _fullRange;
            if (progress >= StartThreshold)
            {
                await AnimateTrigger(async () =>
                {
                    if (_vm?.StartListeningCommand?.CanExecute(null) == true)
                        _vm.StartListeningCommand.Execute(null);

                    _state = SliderState.Listening;
                    ThumbLabel.Text = "🔴";
                    CenterHint.Text = "Escuchando";
                    // move thumb to center
                    await SwipeSliderThumb.TranslateTo(0, 0, 200, Easing.CubicOut);
                });
            }
            else
            {
                await SwipeSliderThumb.TranslateTo(-_halfRange, 0, 200, Easing.CubicOut);
                ResetTrackVisuals();
            }
        }
        else // Listening: use center-based norm
        {
            var norm = (_halfRange == 0) ? 0 : finalX / _halfRange;
            if (norm <= -ActionThreshold)
            {
                await AnimateTrigger(async () =>
                {
                    if (_vm?.StopListeningCommand?.CanExecute(null) == true)
                        _vm.StopListeningCommand.Execute(null);

                    _state = SliderState.Idle;
                    ThumbLabel.Text = "🎤";
                    CenterHint.Text = "Desliza desde la izquierda para hablar";
                    await SwipeSliderThumb.TranslateTo(-_halfRange, 0, 200, Easing.CubicOut);
                    ResetTrackVisuals();
                });
            }
            else if (norm >= ActionThreshold)
            {
                await AnimateTrigger(async () =>
                {
                    if (_vm?.CancelListeningCommand?.CanExecute(null) == true)
                        _vm.CancelListeningCommand.Execute(null);

                    _state = SliderState.Idle;
                    ThumbLabel.Text = "🎤";
                    CenterHint.Text = "Desliza desde la izquierda para hablar";
                    await SwipeSliderThumb.TranslateTo(-_halfRange, 0, 200, Easing.CubicOut);
                    ResetTrackVisuals();
                });
            }
            else
            {
                await SwipeSliderThumb.TranslateTo(0, 0, 200, Easing.CubicOut);
                ResetTrackVisuals();
            }
        }
    }

    private void ResetTrackVisuals()
    {
        var baseColor = GetColorResource("CardBackground", Colors.LightGray);
        SwipeSliderTrack.BackgroundColor = baseColor;
        LeftHint.Opacity = 0;
        RightHint.Opacity = 0;
        CenterHint.Opacity = 1;
        CenterHint.Text = _state == SliderState.Listening ? "Escuchando" : "Desliza desde la izquierda para hablar";
    }

    private async Task AnimateTrigger(Func<Task> action)
    {
        await SwipeSliderThumb.ScaleTo(1.05, 80);
        try { await action(); } catch { }
        await SwipeSliderThumb.ScaleTo(1.0, 120);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        ResetTrackVisuals();
    }

    // Navegar a historial
    private async void OnOpenHistoryClicked(object sender, EventArgs e)
    {
        try
        {
            await Navigation.PushAsync(new HistoryPage(_vm));
        }
        catch
        {
            await Application.Current.MainPage.Navigation.PushAsync(new HistoryPage(_vm));
        }
    }
}