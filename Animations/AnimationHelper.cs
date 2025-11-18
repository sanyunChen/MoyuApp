using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace MoyuApp.Animations
{
    public static class AnimationHelper
    {
        public static DoubleAnimation CreateFadeAnimation(double from, double to, TimeSpan duration)
        {
            return new DoubleAnimation(from, to, duration)
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
        }

        public static DoubleAnimation CreateSlideAnimation(double from, double to, TimeSpan duration)
        {
            return new DoubleAnimation(from, to, duration)
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
        }

        public static DoubleAnimation CreateScaleAnimation(double from, double to, TimeSpan duration)
        {
            return new DoubleAnimation(from, to, duration)
            {
                EasingFunction = new ElasticEase 
                { 
                    EasingMode = EasingMode.EaseOut,
                    Oscillations = 1,
                    Springiness = 3
                }
            };
        }

        public static void AnimateProgressBar(System.Windows.Controls.ProgressBar progressBar, double targetValue, TimeSpan duration)
        {
            var currentValue = progressBar.Value;
            var animation = new DoubleAnimation(currentValue, targetValue, duration)
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            
            progressBar.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, animation);
        }

        public static void AnimateTextChange(System.Windows.Controls.TextBlock textBlock, string newText, TimeSpan duration)
        {
            var fadeOut = CreateFadeAnimation(1, 0, duration.Divide(2));
            
            fadeOut.Completed += (s, e) =>
            {
                textBlock.Text = newText;
                var fadeIn = CreateFadeAnimation(0, 1, duration.Divide(2));
                textBlock.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };
            
            textBlock.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        public static void AnimateCardEntrance(System.Windows.FrameworkElement card, TimeSpan delay = default)
        {
            card.Opacity = 0;
            card.RenderTransform = new System.Windows.Media.ScaleTransform(0.8, 0.8);
            
            var opacityAnimation = CreateFadeAnimation(0, 1, TimeSpan.FromMilliseconds(500));
            var scaleAnimation = CreateScaleAnimation(0.8, 1, TimeSpan.FromMilliseconds(500));
            
            opacityAnimation.BeginTime = delay;
            scaleAnimation.BeginTime = delay;
            
            card.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            card.RenderTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, scaleAnimation);
            card.RenderTransform.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, scaleAnimation);
        }

        public static void AnimateQuoteChange(System.Windows.Controls.TextBlock quoteTextBlock, string newQuote, TimeSpan duration)
        {
            var slideOut = CreateSlideAnimation(0, -20, duration.Divide(2));
            var fadeOut = CreateFadeAnimation(1, 0, duration.Divide(2));
            
            var storyboard = new System.Windows.Media.Animation.Storyboard();
            storyboard.Children.Add(slideOut);
            storyboard.Children.Add(fadeOut);
            
            System.Windows.Media.Animation.Storyboard.SetTarget(slideOut, quoteTextBlock);
            System.Windows.Media.Animation.Storyboard.SetTargetProperty(slideOut, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            
            System.Windows.Media.Animation.Storyboard.SetTarget(fadeOut, quoteTextBlock);
            System.Windows.Media.Animation.Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));
            
            storyboard.Completed += (s, e) =>
            {
                quoteTextBlock.Text = newQuote;
                
                var slideIn = CreateSlideAnimation(20, 0, duration.Divide(2));
                var fadeIn = CreateFadeAnimation(0, 1, duration.Divide(2));
                
                var newStoryboard = new System.Windows.Media.Animation.Storyboard();
                newStoryboard.Children.Add(slideIn);
                newStoryboard.Children.Add(fadeIn);
                
                System.Windows.Media.Animation.Storyboard.SetTarget(slideIn, quoteTextBlock);
                System.Windows.Media.Animation.Storyboard.SetTargetProperty(slideIn, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
                
                System.Windows.Media.Animation.Storyboard.SetTarget(fadeIn, quoteTextBlock);
                System.Windows.Media.Animation.Storyboard.SetTargetProperty(fadeIn, new PropertyPath(UIElement.OpacityProperty));
                
                newStoryboard.Begin();
            };
            
            // 确保有RenderTransform
            if (quoteTextBlock.RenderTransform == null || quoteTextBlock.RenderTransform is not System.Windows.Media.TranslateTransform)
            {
                quoteTextBlock.RenderTransform = new System.Windows.Media.TranslateTransform();
            }
            
            storyboard.Begin();
        }

        public static TimeSpan Divide(this TimeSpan timeSpan, int divisor)
        {
            return TimeSpan.FromMilliseconds(timeSpan.TotalMilliseconds / divisor);
        }
    }
}