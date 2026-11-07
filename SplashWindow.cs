using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GotinhaAgua
{
    public class SplashWindow : Window
    {
        private bool _iniciandoApp = false;
        private Border _btnEntrarBorder;
        private ScaleTransform _btnScale;
        private Storyboard _btnPulseStoryboard;
        private ScaleTransform _mascotScale;
        private TranslateTransform _mascotTranslate;
        private RotateTransform _mascotRotate;

        public SplashWindow()
        {
            Title = "Gotinha - Bem-vindo!";
            Width = 470;
            Height = 650;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;

            // Permite arrastar a janela livremente
            MouseLeftButtonDown += delegate(object s, MouseButtonEventArgs e)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    try { DragMove(); } catch { }
                }
            };

            ConstruirInterface();
        }

        private void ConstruirInterface()
        {
            Grid rootGrid = new Grid
            {
                Margin = new Thickness(16)
            };

            // Card principal arredondado com efeito de vidro suave e sombra celestial
            Border mainCard = new Border
            {
                CornerRadius = new CornerRadius(36),
                BorderThickness = new Thickness(2.5),
                BorderBrush = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)),
                ClipToBounds = true,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(2, 132, 199),
                    BlurRadius = 32,
                    ShadowDepth = 6,
                    Opacity = 0.28
                }
            };

            // Gradiente suave de fundo: Azul-bebê para celeste e branco fresco
            LinearGradientBrush cardBg = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1)
            };
            cardBg.GradientStops.Add(new GradientStop(Color.FromRgb(224, 242, 254), 0.0)); // #E0F2FE
            cardBg.GradientStops.Add(new GradientStop(Color.FromRgb(186, 230, 253), 0.5)); // #BAE6FD
            cardBg.GradientStops.Add(new GradientStop(Color.FromRgb(240, 249, 255), 1.0)); // #F0F9FF
            mainCard.Background = cardBg;

            // Canvas para bolhas animadas e partículas cintilantes no fundo
            Canvas canvasFundo = new Canvas
            {
                ClipToBounds = true,
                IsHitTestVisible = false
            };
            CriarBolhasEFundo(canvasFundo);
            mainCard.Child = canvasFundo;

            rootGrid.Children.Add(mainCard);

            // Camada de conteúdo por cima do fundo
            Grid contentGrid = new Grid();

            // Botão fechar discreto e moderno
            Button btnFechar = CriarBotaoFechar();
            contentGrid.Children.Add(btnFechar);

            // Container centralizado
            StackPanel stackCentro = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(20, 16, 20, 24)
            };

            // 1. Apresentação da Mascote com animação de pulo, aceno, flutuação e piscada
            FrameworkElement mascoteVisual = CriarMascoteAnimada();
            stackCentro.Children.Add(mascoteVisual);

            // 2. Textos de Apresentação
            FrameworkElement textosVisual = CriarTextosApresentacao();
            stackCentro.Children.Add(textosVisual);

            // 3. Botão "Entrar no app"
            FrameworkElement botaoVisual = CriarBotaoEntrar();
            stackCentro.Children.Add(botaoVisual);

            contentGrid.Children.Add(stackCentro);
            rootGrid.Children.Add(contentGrid);

            Content = rootGrid;

            // Animação de entrada da janela inteira (Fade in suave)
            Opacity = 0.0;
            Loaded += delegate
            {
                DoubleAnimation fadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(450))
                {
                    EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
                };
                BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };
        }

        // ==========================================
        // 1. MASCOTE GOTINHA (Pulo de entrada, flutuação, aceno e piscar de olhos)
        // ==========================================
        private FrameworkElement CriarMascoteAnimada()
        {
            Grid mascotContainer = new Grid
            {
                Width = 190,
                Height = 190,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 18),
                Cursor = Cursors.Hand
            };

            // Grupo de transformações: Translate (entrada/flutuação), Scale (bounce), Rotate (aceno alegre)
            TransformGroup tgMascote = new TransformGroup();
            _mascotScale = new ScaleTransform(1.0, 1.0);
            _mascotTranslate = new TranslateTransform(0, 0);
            _mascotRotate = new RotateTransform(0);

            tgMascote.Children.Add(_mascotScale);
            tgMascote.Children.Add(_mascotTranslate);
            tgMascote.Children.Add(_mascotRotate);

            mascotContainer.RenderTransform = tgMascote;
            mascotContainer.RenderTransformOrigin = new Point(0.5, 0.5);

            // Halo/Aura celestial suave por trás da mascote
            Ellipse glowAura = new Ellipse
            {
                Width = 190,
                Height = 190,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Fill = new RadialGradientBrush
                {
                    GradientOrigin = new Point(0.5, 0.5),
                    Center = new Point(0.5, 0.5),
                    RadiusX = 0.5,
                    RadiusY = 0.5,
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(170, 255, 255, 255), 0.0),
                        new GradientStop(Color.FromArgb(90, 56, 189, 248), 0.5),
                        new GradientStop(Color.FromArgb(0, 56, 189, 248), 1.0)
                    }
                },
                IsHitTestVisible = false
            };
            mascotContainer.Children.Add(glowAura);

            // Medalhão circular estilizado com a Mascote Gotinha
            string mascotPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mascote.png");
            if (File.Exists(mascotPath))
            {
                try
                {
                    Border imgBorder = new Border
                    {
                        Width = 170,
                        Height = 170,
                        CornerRadius = new CornerRadius(85), // Círculo perfeito
                        BorderThickness = new Thickness(3.5),
                        BorderBrush = Brushes.White,
                        ClipToBounds = true,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Effect = new DropShadowEffect
                        {
                            Color = Color.FromRgb(2, 132, 199),
                            BlurRadius = 24,
                            ShadowDepth = 5,
                            Opacity = 0.35
                        }
                    };

                    Image mascotImg = new Image
                    {
                        Width = 163,
                        Height = 163,
                        Stretch = Stretch.UniformToFill,
                        Clip = new EllipseGeometry(new Point(81.5, 81.5), 81.5, 81.5)
                    };
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(mascotPath, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    mascotImg.Source = bmp;

                    imgBorder.Child = mascotImg;
                    mascotContainer.Children.Add(imgBorder);
                }
                catch { }
            }

            // Pálpebras e olhos animados para o efeito de piscar (Eye Blink)
            Canvas canvasOlhos = new Canvas
            {
                Width = 170,
                Height = 170,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false
            };

            // Olho esquerdo piscando
            FrameworkElement olhoEsq = CriarEyelidElemento();
            Canvas.SetLeft(olhoEsq, 54);
            Canvas.SetTop(olhoEsq, 91);
            canvasOlhos.Children.Add(olhoEsq);

            // Olho direito piscando
            FrameworkElement olhoDir = CriarEyelidElemento();
            Canvas.SetLeft(olhoDir, 96);
            Canvas.SetTop(olhoDir, 91);
            canvasOlhos.Children.Add(olhoDir);

            mascotContainer.Children.Add(canvasOlhos);

            // ==========================================
            // ANIMAÇÕES: Entrada (Pulo), Flutuação, Aceno e Piscar
            // ==========================================

            // 1. Entrada suave como se estivesse pulando para dentro da tela
            _mascotTranslate.Y = -140;
            _mascotScale.ScaleX = 0.3;
            _mascotScale.ScaleY = 0.3;
            mascotContainer.Opacity = 0.0;

            DoubleAnimation animPuloY = new DoubleAnimation(-140, 0, TimeSpan.FromMilliseconds(780))
            {
                EasingFunction = new BounceEase { Bounces = 1, Bounciness = 2.4, EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation animScaleX = new DoubleAnimation(0.3, 1.0, TimeSpan.FromMilliseconds(650))
            {
                EasingFunction = new BackEase { Amplitude = 0.38, EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation animScaleY = new DoubleAnimation(0.3, 1.0, TimeSpan.FromMilliseconds(650))
            {
                EasingFunction = new BackEase { Amplitude = 0.38, EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation animMascotOpacity = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(420));

            // 2. Aceno amigável e flutuação contínua após a entrada
            animPuloY.Completed += delegate
            {
                // Sequência de aceno animado (waving gesture da Gotinha)
                DoubleAnimationUsingKeyFrames waveKeyFrames = new DoubleAnimationUsingKeyFrames();
                waveKeyFrames.KeyFrames.Add(new SplineDoubleKeyFrame(-8.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))));
                waveKeyFrames.KeyFrames.Add(new SplineDoubleKeyFrame(8.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(450))));
                waveKeyFrames.KeyFrames.Add(new SplineDoubleKeyFrame(-6.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(700))));
                waveKeyFrames.KeyFrames.Add(new SplineDoubleKeyFrame(6.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(950))));
                waveKeyFrames.KeyFrames.Add(new SplineDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1200))));

                waveKeyFrames.Completed += delegate
                {
                    // Flutuação contínua e suave (subindo e descendo delicadamente)
                    DoubleAnimation floatAnim = new DoubleAnimation(-9, 9, TimeSpan.FromSeconds(2.5))
                    {
                        AutoReverse = true,
                        RepeatBehavior = RepeatBehavior.Forever,
                        EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                    };
                    _mascotTranslate.BeginAnimation(TranslateTransform.YProperty, floatAnim);

                    // Movimento sutil de balanço/aceno contínuo
                    DoubleAnimation gentleWave = new DoubleAnimation(-3.5, 3.5, TimeSpan.FromSeconds(2.0))
                    {
                        AutoReverse = true,
                        RepeatBehavior = RepeatBehavior.Forever,
                        EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                    };
                    _mascotRotate.BeginAnimation(RotateTransform.AngleProperty, gentleWave);
                };

                _mascotRotate.BeginAnimation(RotateTransform.AngleProperty, waveKeyFrames);
            };

            _mascotTranslate.BeginAnimation(TranslateTransform.YProperty, animPuloY);
            _mascotScale.BeginAnimation(ScaleTransform.ScaleXProperty, animScaleX);
            _mascotScale.BeginAnimation(ScaleTransform.ScaleYProperty, animScaleY);
            mascotContainer.BeginAnimation(UIElement.OpacityProperty, animMascotOpacity);

            // 3. Piscar de olhos ocasional a cada 3.8 segundos
            Storyboard blinkSb = new Storyboard
            {
                RepeatBehavior = RepeatBehavior.Forever,
                Duration = new Duration(TimeSpan.FromSeconds(3.8))
            };

            DoubleAnimationUsingKeyFrames blinkEsq = CriarFramesPiscar();
            Storyboard.SetTarget(blinkEsq, olhoEsq);
            Storyboard.SetTargetProperty(blinkEsq, new PropertyPath(UIElement.OpacityProperty));
            blinkSb.Children.Add(blinkEsq);

            DoubleAnimationUsingKeyFrames blinkDir = CriarFramesPiscar();
            Storyboard.SetTarget(blinkDir, olhoDir);
            Storyboard.SetTargetProperty(blinkDir, new PropertyPath(UIElement.OpacityProperty));
            blinkSb.Children.Add(blinkDir);

            blinkSb.Begin();

            // Interação fofa ao passar o mouse ou clicar na Gotinha
            mascotContainer.MouseEnter += delegate
            {
                DoubleAnimation petScale = new DoubleAnimation(1.06, TimeSpan.FromMilliseconds(200))
                {
                    EasingFunction = new BackEase { Amplitude = 0.5, EasingMode = EasingMode.EaseOut }
                };
                _mascotScale.BeginAnimation(ScaleTransform.ScaleXProperty, petScale);
                _mascotScale.BeginAnimation(ScaleTransform.ScaleYProperty, petScale);
            };

            mascotContainer.MouseLeave += delegate
            {
                DoubleAnimation unpetScale = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(250))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                _mascotScale.BeginAnimation(ScaleTransform.ScaleXProperty, unpetScale);
                _mascotScale.BeginAnimation(ScaleTransform.ScaleYProperty, unpetScale);
            };

            mascotContainer.MouseLeftButtonDown += delegate
            {
                try { GeradorDeSom.TocarGota(); } catch { }
                DoubleAnimation jump = new DoubleAnimation(-16, TimeSpan.FromMilliseconds(180))
                {
                    AutoReverse = true,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                _mascotTranslate.BeginAnimation(TranslateTransform.YProperty, jump);
            };

            return mascotContainer;
        }

        private FrameworkElement CriarEyelidElemento()
        {
            Grid eyelidGrid = new Grid
            {
                Width = 24,
                Height = 22,
                Opacity = 0.0
            };

            // Cobertura do olho com o tom de azul celestial da Gotinha
            Ellipse lidCover = new Ellipse
            {
                Width = 24,
                Height = 22,
                Fill = new SolidColorBrush(Color.FromArgb(240, 130, 214, 253)) // Tom de água da gotinha
            };

            // Curva feliz de olho fechado (estilo kawaii)
            System.Windows.Shapes.Path eyeLine = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse("M 1,11 Q 12,-1 23,11"),
                Stroke = new SolidColorBrush(Color.FromRgb(26, 43, 76)), // Cor escura do contorno dos olhos
                StrokeThickness = 3.2,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };

            eyelidGrid.Children.Add(lidCover);
            eyelidGrid.Children.Add(eyeLine);

            return eyelidGrid;
        }

        private DoubleAnimationUsingKeyFrames CriarFramesPiscar()
        {
            DoubleAnimationUsingKeyFrames frames = new DoubleAnimationUsingKeyFrames();
            frames.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.0))));
            frames.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3.0))));
            frames.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3.1))));
            frames.KeyFrames.Add(new LinearDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3.28))));
            frames.KeyFrames.Add(new LinearDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3.38))));
            frames.KeyFrames.Add(new DiscreteDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3.8))));
            return frames;
        }

        // ==========================================
        // 2. TEXTOS DE BOAS-VINDAS
        // ==========================================
        private FrameworkElement CriarTextosApresentacao()
        {
            StackPanel stackTextos = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 24)
            };

            // Título "Gotinha" com tipografia arredondada, degradê marcante e sombra
            TextBlock txtTitulo = new TextBlock
            {
                Text = "Gotinha",
                FontFamily = new FontFamily("Segoe UI, Arial Rounded MT Bold, sans-serif"),
                FontSize = 38,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8),
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(186, 230, 253),
                    BlurRadius = 16,
                    ShadowDepth = 2,
                    Opacity = 0.7
                }
            };
            LinearGradientBrush tituloBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            tituloBrush.GradientStops.Add(new GradientStop(Color.FromRgb(2, 132, 199), 0.0));  // Sky 600
            tituloBrush.GradientStops.Add(new GradientStop(Color.FromRgb(3, 105, 161), 1.0));  // Sky 700
            txtTitulo.Foreground = tituloBrush;

            // Frase principal: "Oiê! Que bom ter você aqui! 💧"
            TextBlock txtMensagem = new TextBlock
            {
                Text = "Oi\u00EA! Que bom ter voc\u00EA aqui! \uD83D\uDCA7",
                FontFamily = new FontFamily("Segoe UI, sans-serif"),
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 42)), // Slate 900
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 6)
            };

            // Frase secundária: "Vamos cuidar da sua hidratação juntos?"
            TextBlock txtSubMensagem = new TextBlock
            {
                Text = "Vamos cuidar da sua hidrata\u00E7\u00E3o juntos?",
                FontFamily = new FontFamily("Segoe UI, sans-serif"),
                FontSize = 14,
                FontWeight = FontWeights.Normal,
                Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)), // Slate 600
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };

            stackTextos.Children.Add(txtTitulo);
            stackTextos.Children.Add(txtMensagem);
            stackTextos.Children.Add(txtSubMensagem);

            // Animação gradual e fluida de aparição dos textos
            AplicarAnimacaoEntradaTexto(txtTitulo, 320);
            AplicarAnimacaoEntradaTexto(txtMensagem, 580);
            AplicarAnimacaoEntradaTexto(txtSubMensagem, 800);

            return stackTextos;
        }

        private void AplicarAnimacaoEntradaTexto(UIElement elemento, int atrasoMs)
        {
            elemento.Opacity = 0.0;
            TranslateTransform tt = new TranslateTransform(0, 16);
            elemento.RenderTransform = tt;

            DoubleAnimation fadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(480))
            {
                BeginTime = TimeSpan.FromMilliseconds(atrasoMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation slideUp = new DoubleAnimation(16, 0, TimeSpan.FromMilliseconds(480))
            {
                BeginTime = TimeSpan.FromMilliseconds(atrasoMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            elemento.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            tt.BeginAnimation(TranslateTransform.YProperty, slideUp);
        }

        // ==========================================
        // 3. BOTÃO "ENTRAR NO APP"
        // ==========================================
        private FrameworkElement CriarBotaoEntrar()
        {
            Grid btnWrapper = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 6, 0, 0)
            };

            _btnEntrarBorder = new Border
            {
                Width = 264,
                Height = 56,
                CornerRadius = new CornerRadius(28),
                BorderThickness = new Thickness(2),
                BorderBrush = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                Cursor = Cursors.Hand,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(2, 132, 199),
                    BlurRadius = 20,
                    ShadowDepth = 5,
                    Opacity = 0.42
                }
            };

            // Fundo degradê vibrante azul-celeste
            LinearGradientBrush btnGradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            btnGradient.GradientStops.Add(new GradientStop(Color.FromRgb(56, 189, 248), 0.0)); // Sky 400
            btnGradient.GradientStops.Add(new GradientStop(Color.FromRgb(2, 132, 199), 0.55)); // Sky 600
            btnGradient.GradientStops.Add(new GradientStop(Color.FromRgb(3, 105, 161), 1.0)); // Sky 700
            _btnEntrarBorder.Background = btnGradient;

            // Brilho superior (Glossy)
            Border shineOverlay = new Border
            {
                Height = 26,
                CornerRadius = new CornerRadius(28, 28, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                IsHitTestVisible = false,
                Background = new LinearGradientBrush
                {
                    StartPoint = new Point(0.5, 0),
                    EndPoint = new Point(0.5, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(110, 255, 255, 255), 0.0),
                        new GradientStop(Color.FromArgb(0, 255, 255, 255), 1.0)
                    }
                }
            };

            // Conteúdo do botão
            StackPanel spBtn = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            TextBlock txtIcon = new TextBlock
            {
                Text = "\u2728",
                FontSize = 17,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0),
                Foreground = Brushes.White
            };

            TextBlock txtBtn = new TextBlock
            {
                Text = "Entrar no app",
                FontFamily = new FontFamily("Segoe UI, sans-serif"),
                FontSize = 17,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };

            spBtn.Children.Add(txtIcon);
            spBtn.Children.Add(txtBtn);

            Grid innerBtnGrid = new Grid();
            innerBtnGrid.Children.Add(shineOverlay);
            innerBtnGrid.Children.Add(spBtn);
            _btnEntrarBorder.Child = innerBtnGrid;

            // Transformação de Escala para Hover / Click / Pulse
            _btnScale = new ScaleTransform(1.0, 1.0);
            _btnEntrarBorder.RenderTransform = _btnScale;
            _btnEntrarBorder.RenderTransformOrigin = new Point(0.5, 0.5);

            // Animação de pulso contínuo convidativo
            _btnPulseStoryboard = new Storyboard
            {
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            DoubleAnimation pulseX = new DoubleAnimation(1.0, 1.035, TimeSpan.FromSeconds(1.6))
            {
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            DoubleAnimation pulseY = new DoubleAnimation(1.0, 1.035, TimeSpan.FromSeconds(1.6))
            {
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(pulseX, _btnScale);
            Storyboard.SetTargetProperty(pulseX, new PropertyPath(ScaleTransform.ScaleXProperty));
            Storyboard.SetTarget(pulseY, _btnScale);
            Storyboard.SetTargetProperty(pulseY, new PropertyPath(ScaleTransform.ScaleYProperty));
            _btnPulseStoryboard.Children.Add(pulseX);
            _btnPulseStoryboard.Children.Add(pulseY);
            _btnPulseStoryboard.Begin();

            // Interatividade com o Mouse: Hover e Click
            _btnEntrarBorder.MouseEnter += delegate
            {
                _btnPulseStoryboard.Pause();
                DoubleAnimation hoverScale = new DoubleAnimation(_btnScale.ScaleX, 1.06, TimeSpan.FromMilliseconds(180))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                _btnScale.BeginAnimation(ScaleTransform.ScaleXProperty, hoverScale);
                _btnScale.BeginAnimation(ScaleTransform.ScaleYProperty, hoverScale);
            };

            _btnEntrarBorder.MouseLeave += delegate
            {
                DoubleAnimation unhoverScale = new DoubleAnimation(_btnScale.ScaleX, 1.0, TimeSpan.FromMilliseconds(220))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                unhoverScale.Completed += delegate
                {
                    _btnPulseStoryboard.Resume();
                };
                _btnScale.BeginAnimation(ScaleTransform.ScaleXProperty, unhoverScale);
                _btnScale.BeginAnimation(ScaleTransform.ScaleYProperty, unhoverScale);
            };

            _btnEntrarBorder.MouseLeftButtonDown += delegate(object s, MouseButtonEventArgs e)
            {
                e.Handled = true;
                DoubleAnimation clickScale = new DoubleAnimation(0.94, TimeSpan.FromMilliseconds(90))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                _btnScale.BeginAnimation(ScaleTransform.ScaleXProperty, clickScale);
                _btnScale.BeginAnimation(ScaleTransform.ScaleYProperty, clickScale);
            };

            _btnEntrarBorder.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
            {
                e.Handled = true;
                DoubleAnimation releaseScale = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(120));
                _btnScale.BeginAnimation(ScaleTransform.ScaleXProperty, releaseScale);
                _btnScale.BeginAnimation(ScaleTransform.ScaleYProperty, releaseScale);

                IniciarTransicaoParaApp();
            };

            btnWrapper.Children.Add(_btnEntrarBorder);

            // Animação de entrada do botão
            btnWrapper.Opacity = 0.0;
            TranslateTransform ttBtn = new TranslateTransform(0, 20);
            btnWrapper.RenderTransform = ttBtn;

            DoubleAnimation fadeInBtn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(500))
            {
                BeginTime = TimeSpan.FromMilliseconds(920),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation slideUpBtn = new DoubleAnimation(20, 0, TimeSpan.FromMilliseconds(500))
            {
                BeginTime = TimeSpan.FromMilliseconds(920),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            btnWrapper.BeginAnimation(UIElement.OpacityProperty, fadeInBtn);
            ttBtn.BeginAnimation(TranslateTransform.YProperty, slideUpBtn);

            return btnWrapper;
        }

        // ==========================================
        // 4. BOLHAS E BRILHOS DECORATIVOS FLUTUANTES
        // ==========================================
        private void CriarBolhasEFundo(Canvas canvas)
        {
            Random rnd = new Random();

            int totalBolhas = 14;
            for (int i = 0; i < totalBolhas; i++)
            {
                double tamanho = rnd.Next(14, 32);
                Ellipse bolha = new Ellipse
                {
                    Width = tamanho,
                    Height = tamanho,
                    Fill = new RadialGradientBrush
                    {
                        GradientOrigin = new Point(0.35, 0.35),
                        Center = new Point(0.5, 0.5),
                        RadiusX = 0.5,
                        RadiusY = 0.5,
                        GradientStops = new GradientStopCollection
                        {
                            new GradientStop(Color.FromArgb(170, 255, 255, 255), 0.0),
                            new GradientStop(Color.FromArgb(80, 224, 242, 254), 0.6),
                            new GradientStop(Color.FromArgb(20, 56, 189, 248), 1.0)
                        }
                    },
                    Stroke = new SolidColorBrush(Color.FromArgb(110, 255, 255, 255)),
                    StrokeThickness = 1,
                    IsHitTestVisible = false
                };

                double posX = rnd.Next(12, 410);
                double posY = rnd.Next(40, 620);
                Canvas.SetLeft(bolha, posX);
                Canvas.SetTop(bolha, posY);
                canvas.Children.Add(bolha);

                double duracao = rnd.Next(5000, 9000) / 1000.0;
                DoubleAnimation animSubida = new DoubleAnimation(640, -50, TimeSpan.FromSeconds(duracao))
                {
                    RepeatBehavior = RepeatBehavior.Forever,
                    BeginTime = TimeSpan.FromSeconds(rnd.Next(0, 3500) / 1000.0)
                };
                bolha.BeginAnimation(Canvas.TopProperty, animSubida);

                DoubleAnimation animDeriva = new DoubleAnimation(posX - 8, posX + 8, TimeSpan.FromSeconds(duracao / 2.0))
                {
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                };
                bolha.BeginAnimation(Canvas.LeftProperty, animDeriva);
            }

            // Brilhos cintilantes decorativos
            CriarBrilhoEstrelar(canvas, 60, 100, 1.5);
            CriarBrilhoEstrelar(canvas, 370, 85, 2.0);
            CriarBrilhoEstrelar(canvas, 48, 275, 1.8);
            CriarBrilhoEstrelar(canvas, 390, 285, 2.3);
            CriarBrilhoEstrelar(canvas, 75, 460, 1.6);
            CriarBrilhoEstrelar(canvas, 355, 480, 2.1);
        }

        private void CriarBrilhoEstrelar(Canvas canvas, double x, double y, double delaySec)
        {
            System.Windows.Shapes.Path estrela = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse("M 7,0 Q 7,7 14,7 Q 7,7 7,14 Q 7,7 0,7 Q 7,7 7,0"),
                Fill = Brushes.White,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(56, 189, 248),
                    BlurRadius = 8,
                    ShadowDepth = 0,
                    Opacity = 0.8
                },
                Opacity = 0.3,
                IsHitTestVisible = false
            };

            ScaleTransform st = new ScaleTransform(0.8, 0.8);
            estrela.RenderTransform = st;
            estrela.RenderTransformOrigin = new Point(0.5, 0.5);

            Canvas.SetLeft(estrela, x);
            Canvas.SetTop(estrela, y);
            canvas.Children.Add(estrela);

            DoubleAnimation pulseOpacity = new DoubleAnimation(0.2, 0.95, TimeSpan.FromSeconds(1.4))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = TimeSpan.FromSeconds(delaySec),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            DoubleAnimation pulseScale = new DoubleAnimation(0.7, 1.25, TimeSpan.FromSeconds(1.4))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = TimeSpan.FromSeconds(delaySec),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            estrela.BeginAnimation(UIElement.OpacityProperty, pulseOpacity);
            st.BeginAnimation(ScaleTransform.ScaleXProperty, pulseScale);
            st.BeginAnimation(ScaleTransform.ScaleYProperty, pulseScale);
        }

        private Button CriarBotaoFechar()
        {
            Button btnFechar = new Button
            {
                Content = "\u2715",
                Width = 32,
                Height = 32,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 18, 18, 0),
                Cursor = Cursors.Hand,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(16));
            borderFactory.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)));
            borderFactory.Name = "bd";

            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFactory.AppendChild(presenter);

            template.VisualTree = borderFactory;
            btnFechar.Template = template;

            btnFechar.MouseEnter += delegate
            {
                btnFechar.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68));
            };
            btnFechar.MouseLeave += delegate
            {
                btnFechar.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            };
            btnFechar.Click += delegate
            {
                if (Application.Current != null)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    Close();
                }
            };

            return btnFechar;
        }

        // ==========================================
        // 5. TRANSIÇÃO ELEGANTE PARA O APLICATIVO PRINCIPAL
        // ==========================================
        private void IniciarTransicaoParaApp()
        {
            if (_iniciandoApp) return;
            _iniciandoApp = true;

            try
            {
                GeradorDeSom.TocarGota();
            }
            catch { }

            // Fade-out suave da janela de boas-vindas
            DoubleAnimation fadeOut = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(260))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            fadeOut.Completed += delegate
            {
                try
                {
                    MainWindow mainWindow = new MainWindow();
                    if (Application.Current != null)
                    {
                        Application.Current.MainWindow = mainWindow;
                        Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    }

                    // Transição suave de abertura da janela principal
                    mainWindow.Opacity = 0.0;
                    mainWindow.Show();

                    DoubleAnimation fadeInMain = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(300))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    mainWindow.BeginAnimation(UIElement.OpacityProperty, fadeInMain);

                    Close();
                }
                catch (Exception ex)
                {
                    try { File.WriteAllText("error.log", ex.ToString()); } catch { }
                }
            };

            BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }
}
