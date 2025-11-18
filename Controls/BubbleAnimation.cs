using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MoyuApp.Controls
{
    public class BubbleAnimation : Canvas
    {
        private readonly Random _random = new();
        private readonly DispatcherTimer _animationTimer;
        private int _bubbleCount = 16;

        public BubbleAnimation()
        {
            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _animationTimer.Tick += OnAnimationTick;
            
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            CreateBubbles();
            _animationTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _animationTimer.Stop();
            Children.Clear();
        }

        private void CreateBubbles()
        {
            Children.Clear();

            for (int i = 0; i < _bubbleCount; i++)
            {
                var bubble = CreateBubble();
                SetLeft(bubble, _random.NextDouble() * ActualWidth);
                SetTop(bubble, ActualHeight + _random.Next(50, 150));
                Children.Add(bubble);
            }
        }

        private Ellipse CreateBubble()
        {
            var size = _random.Next(10, 46);
            var bubble = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = new SolidColorBrush(Color.FromArgb(64, 255, 255, 255)),
                Stroke = new SolidColorBrush(Color.FromArgb(32, 255, 255, 255)),
                StrokeThickness = 1
            };

            // 添加发光效果
            var glowEffect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.White,
                Direction = 0,
                ShadowDepth = 0,
                BlurRadius = size / 4,
                Opacity = 0.6
            };
            bubble.Effect = glowEffect;

            return bubble;
        }

        private void OnAnimationTick(object? sender, EventArgs e)
        {
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                if (Children[i] is Ellipse bubble)
                {
                    var currentTop = GetTop(bubble);
                    var currentLeft = GetLeft(bubble);
                    
                    // 上升动画
                    var newTop = currentTop - _random.Next(1, 4);
                    
                    // 轻微的水平摆动
                    var horizontalDrift = Math.Sin(DateTime.Now.Millisecond * 0.01 + i) * 0.5;
                    var newLeft = currentLeft + horizontalDrift;
                    
                    // 边界检查
                    if (newLeft < -bubble.Width)
                        newLeft = ActualWidth;
                    if (newLeft > ActualWidth)
                        newLeft = -bubble.Width;
                    
                    SetTop(bubble, newTop);
                    SetLeft(bubble, newLeft);
                    
                    // 如果气泡飘出顶部，重置到底部
                    if (newTop < -bubble.Height)
                    {
                        SetTop(bubble, ActualHeight + _random.Next(50, 150));
                        SetLeft(bubble, _random.NextDouble() * ActualWidth);
                        
                        // 随机改变大小
                        var newSize = _random.Next(10, 46);
                        bubble.Width = newSize;
                        bubble.Height = newSize;
                        
                        if (bubble.Effect is System.Windows.Media.Effects.DropShadowEffect glow)
                        {
                            glow.BlurRadius = newSize / 4;
                        }
                    }
                    
                    // 透明度变化
                    var opacity = 0.3 + Math.Sin(DateTime.Now.Millisecond * 0.02 + i) * 0.2;
                    bubble.Opacity = Math.Max(0.2, Math.Min(0.8, opacity));
                }
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            
            if (sizeInfo.WidthChanged || sizeInfo.HeightChanged)
            {
                CreateBubbles();
            }
        }

        public void SetBubbleCount(int count)
        {
            _bubbleCount = Math.Max(5, Math.Min(50, count));
            CreateBubbles();
        }

        public void StartAnimation()
        {
            _animationTimer.Start();
        }

        public void StopAnimation()
        {
            _animationTimer.Stop();
        }
    }
}