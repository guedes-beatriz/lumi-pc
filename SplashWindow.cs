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
            Title = "Lumi - Bem-vindo!";
            double workH = SystemParameters.WorkArea.Height;
            double workW = SystemParameters.WorkArea.Width;
            Width = Math.Min(350, Math.Max(300, workW - 40));
            Height = Math.Min(460, Math.Max(400, workH - 50));
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;

            // Otimizações de alta fidelidade visual (Renderização nítida sem blur/rasterização)
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
            TextOptions.SetTextHintingMode(this, TextHintingMode.Fixed);
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
            RenderOptions.SetClearTypeHint(this, ClearTypeHint.Enabled);

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
                Margin = new Thickness(12),
                SnapsToDevicePixels = true,
                UseLayoutRounding = true
            };
            RenderOptions.SetClearTypeHint(rootGrid, ClearTypeHint.Enabled);

            // Detecção suave do Modo Escuro das preferências salvas
            bool isDark = false;
            try
            {
                string dadosPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dados_agua.json");
                if (File.Exists(dadosPath))
                {
                    string j = File.ReadAllText(dadosPath);
                    isDark = j.Contains("\"ModoEscuro\":true") || j.Contains("\"ModoEscuro\": true");
                }
            }
            catch { }

            // Card principal com proporções refinadas, borda delicada e sombra etérea
            Border mainCard = new Border
            {
                CornerRadius = new CornerRadius(24),
                BorderThickness = new Thickness(1.0),
                ClipToBounds = true,
                SnapsToDevicePixels = true,
                UseLayoutRounding = true,
                Effect = new DropShadowEffect
                {
                    Color = isDark ? Colors.Black : Color.FromRgb(2, 132, 199),
                    BlurRadius = isDark ? 28 : 24,
                    ShadowDepth = 3,
                    Opacity = isDark ? 0.45 : 0.16
                }
            };
            RenderOptions.SetClearTypeHint(mainCard, ClearTypeHint.Enabled);

            // Gradiente de fundo elegante e suave (tons pastéis no claro / midnight no escuro)
            LinearGradientBrush cardBg = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1)
            };
            if (isDark)
            {
                cardBg.GradientStops.Add(new GradientStop(Color.FromRgb(15, 23, 42), 0.0));
                cardBg.GradientStops.Add(new GradientStop(Color.FromRgb(19, 32, 60), 0.55));
                cardBg.GradientStops.Add(new GradientStop(Color.FromRgb(10, 17, 36), 1.0));
                mainCard.BorderBrush = new SolidColorBrush(Color.FromArgb(90, 56, 189, 248));
            }
            else
            {
                cardBg.GradientStops.Add(new GradientStop(Color.FromRgb(248, 252, 255), 0.0));
                cardBg.GradientStops.Add(new GradientStop(Color.FromRgb(240, 249, 255), 0.45));
                cardBg.GradientStops.Add(new GradientStop(Color.FromRgb(224, 242, 254), 1.0));
                mainCard.BorderBrush = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255));
            }
            mainCard.Background = cardBg;

            // Canvas para bolhas animadas e partículas cintilantes no fundo
            Canvas canvasFundo = new Canvas
            {
                ClipToBounds = true,
                IsHitTestVisible = false
            };
            CriarBolhasEFundo(canvasFundo, isDark);
            mainCard.Child = canvasFundo;

            rootGrid.Children.Add(mainCard);

            // Camada de conteúdo por cima do fundo
            Grid contentGrid = new Grid
            {
                SnapsToDevicePixels = true,
                UseLayoutRounding = true
            };

            // Botão fechar delicado e minimalista
            Button btnFechar = CriarBotaoFechar(isDark);
            contentGrid.Children.Add(btnFechar);

            // Container centralizado com espaçamento harmônico
            StackPanel stackCentro = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(16, 20, 16, 20),
                SnapsToDevicePixels = true,
                UseLayoutRounding = true
            };

            // 1. Apresentação da Mascote com nitidez máxima e animação delicada
            FrameworkElement mascoteVisual = CriarMascoteAnimada();
            stackCentro.Children.Add(mascoteVisual);

            // 2. Textos de Apresentação com renderização vetorial cristalina
            FrameworkElement textosVisual = CriarTextosApresentacao(isDark);
            stackCentro.Children.Add(textosVisual);

            // 3. Botão "Entrar no app"
            FrameworkElement botaoVisual = CriarBotaoEntrar();
            stackCentro.Children.Add(botaoVisual);

            contentGrid.Children.Add(stackCentro);
            rootGrid.Children.Add(contentGrid);

            Content = rootGrid;

            // Animação de entrada da janela (Fade in sutil)
            Opacity = 0.0;
            Loaded += delegate
            {
                DoubleAnimation fadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(380))
                {
                    EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
                };
                BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };
        }

        // ==========================================
        // 1. MASCOTE GOTINHA (Ultra nítida, alta definição e animada)
        // ==========================================
        private FrameworkElement CriarMascoteAnimada()
        {
            Grid mascotContainer = new Grid
            {
                Width = 136,
                Height = 136,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10),
                Cursor = Cursors.Hand,
                SnapsToDevicePixels = true,
                UseLayoutRounding = true
            };

            TransformGroup tgMascote = new TransformGroup();
            _mascotScale = new ScaleTransform(1.0, 1.0);
            _mascotTranslate = new TranslateTransform(0, 0);
            _mascotRotate = new RotateTransform(0);

            tgMascote.Children.Add(_mascotScale);
            tgMascote.Children.Add(_mascotTranslate);
            tgMascote.Children.Add(_mascotRotate);

            mascotContainer.RenderTransform = tgMascote;
            mascotContainer.RenderTransformOrigin = new Point(0.5, 0.5);

            // Sombra de contato sutil no chão (vetorial pura, sem pixel shader no mascote)
            Ellipse groundShadow = new Ellipse
            {
                Width = 84,
                Height = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 4),
                Fill = new RadialGradientBrush
                {
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(45, 2, 132, 199), 0.0),
                        new GradientStop(Color.FromArgb(0, 2, 132, 199), 1.0)
                    }
                },
                IsHitTestVisible = false
            };
            mascotContainer.Children.Add(groundShadow);

            // Halo celestial suave por trás da mascote
            Ellipse glowAura = new Ellipse
            {
                Width = 130,
                Height = 130,
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
                        new GradientStop(Color.FromArgb(120, 255, 255, 255), 0.0),
                        new GradientStop(Color.FromArgb(40, 56, 189, 248), 0.55),
                        new GradientStop(Color.FromArgb(0, 56, 189, 248), 1.0)
                    }
                },
                IsHitTestVisible = false
            };
            mascotContainer.Children.Add(glowAura);

            // Imagem da Gotinha com decodificação de alta fidelidade e interpolação de alta qualidade
            string mascotPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mascote.png");
            if (File.Exists(mascotPath))
            {
                try
                {
                    Image mascotImg = new Image
                    {
                        Width = 124,
                        Height = 124,
                        Stretch = Stretch.Uniform,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        SnapsToDevicePixels = true,
                        UseLayoutRounding = true
                    };
                    RenderOptions.SetBitmapScalingMode(mascotImg, BitmapScalingMode.HighQuality);

                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(mascotPath, UriKind.Absolute);
                    bmp.DecodePixelWidth = 380; // Pré-amostragem lanczos nítida em alta densidade
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    mascotImg.Source = bmp;

                    mascotContainer.Children.Add(mascotImg);
                }
                catch { }
            }

            // Animação de entrada suave com pulo amigável
            _mascotTranslate.Y = -55;
            _mascotScale.ScaleX = 0.5;
            _mascotScale.ScaleY = 0.5;
            mascotContainer.Opacity = 0.0;

            DoubleAnimation animPuloY = new DoubleAnimation(-55, 0, TimeSpan.FromMilliseconds(650))
            {
                EasingFunction = new BounceEase { Bounces = 1, Bounciness = 2.0, EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation animScaleX = new DoubleAnimation(0.5, 1.0, TimeSpan.FromMilliseconds(550))
            {
                EasingFunction = new BackEase { Amplitude = 0.32, EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation animScaleY = new DoubleAnimation(0.5, 1.0, TimeSpan.FromMilliseconds(550))
            {
                EasingFunction = new BackEase { Amplitude = 0.32, EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation animMascotOpacity = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(350));

            // Aceno suave e flutuação contínua delicada
            animPuloY.Completed += delegate
            {
                DoubleAnimationUsingKeyFrames waveKeyFrames = new DoubleAnimationUsingKeyFrames();
                waveKeyFrames.KeyFrames.Add(new SplineDoubleKeyFrame(-5.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(180))));
                waveKeyFrames.KeyFrames.Add(new SplineDoubleKeyFrame(5.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400))));
                waveKeyFrames.KeyFrames.Add(new SplineDoubleKeyFrame(-3.5, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(620))));
                waveKeyFrames.KeyFrames.Add(new SplineDoubleKeyFrame(3.5, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(840))));
                waveKeyFrames.KeyFrames.Add(new SplineDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(1050))));

                waveKeyFrames.Completed += delegate
                {
                    // Flutuação sutil e serena
                    DoubleAnimation floatAnim = new DoubleAnimation(-4.0, 4.0, TimeSpan.FromSeconds(2.4))
                    {
                        AutoReverse = true,
                        RepeatBehavior = RepeatBehavior.Forever,
                        EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                    };
                    _mascotTranslate.BeginAnimation(TranslateTransform.YProperty, floatAnim);

                    // Inclinação sutil
                    DoubleAnimation gentleTilt = new DoubleAnimation(-2.0, 2.0, TimeSpan.FromSeconds(2.0))
                    {
                        AutoReverse = true,
                        RepeatBehavior = RepeatBehavior.Forever,
                        EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                    };
                    _mascotRotate.BeginAnimation(RotateTransform.AngleProperty, gentleTilt);
                };

                _mascotRotate.BeginAnimation(RotateTransform.AngleProperty, waveKeyFrames);
            };

            _mascotTranslate.BeginAnimation(TranslateTransform.YProperty, animPuloY);
            _mascotScale.BeginAnimation(ScaleTransform.ScaleXProperty, animScaleX);
            _mascotScale.BeginAnimation(ScaleTransform.ScaleYProperty, animScaleY);
            mascotContainer.BeginAnimation(UIElement.OpacityProperty, animMascotOpacity);

            // Interação charmosa ao passar o mouse ou clicar
            mascotContainer.MouseEnter += delegate
            {
                DoubleAnimation petScale = new DoubleAnimation(1.04, TimeSpan.FromMilliseconds(180))
                {
                    EasingFunction = new BackEase { Amplitude = 0.35, EasingMode = EasingMode.EaseOut }
                };
                _mascotScale.BeginAnimation(ScaleTransform.ScaleXProperty, petScale);
                _mascotScale.BeginAnimation(ScaleTransform.ScaleYProperty, petScale);
            };

            mascotContainer.MouseLeave += delegate
            {
                DoubleAnimation unpetScale = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(200))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                _mascotScale.BeginAnimation(ScaleTransform.ScaleXProperty, unpetScale);
                _mascotScale.BeginAnimation(ScaleTransform.ScaleYProperty, unpetScale);
            };

            mascotContainer.MouseLeftButtonDown += delegate
            {
                try { GeradorDeSom.TocarGota(); } catch { }
                DoubleAnimation jump = new DoubleAnimation(-8, TimeSpan.FromMilliseconds(160))
                {
                    AutoReverse = true,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                _mascotTranslate.BeginAnimation(TranslateTransform.YProperty, jump);
            };

            return mascotContainer;
        }

        // ==========================================
        // 2. TEXTOS DE BOAS-VINDAS (Tipografia nítida com ClearType nativo)
        // ==========================================
        private FrameworkElement CriarTextosApresentacao(bool isDark)
        {
            StackPanel stackTextos = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16),
                SnapsToDevicePixels = true,
                UseLayoutRounding = true
            };

            // Título "Lumi" nítido, sem rasterização de shader
            TextBlock txtTitulo = new TextBlock
            {
                Text = "Lumi",
                FontFamily = new FontFamily("Segoe UI, Arial Rounded MT Bold, sans-serif"),
                FontSize = 26,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4)
            };

            LinearGradientBrush tituloBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            if (isDark)
            {
                tituloBrush.GradientStops.Add(new GradientStop(Color.FromRgb(56, 189, 248), 0.0));
                tituloBrush.GradientStops.Add(new GradientStop(Color.FromRgb(125, 211, 252), 1.0));
            }
            else
            {
                tituloBrush.GradientStops.Add(new GradientStop(Color.FromRgb(2, 132, 199), 0.0));
                tituloBrush.GradientStops.Add(new GradientStop(Color.FromRgb(14, 165, 233), 1.0));
            }
            txtTitulo.Foreground = tituloBrush;

            // Frase principal: "Oiê! Que bom ter você aqui! 💧"
            TextBlock txtMensagem = new TextBlock
            {
                Text = "Oi\u00EA! Que bom ter voc\u00EA aqui! \uD83D\uDCA7",
                FontFamily = new FontFamily("Segoe UI, sans-serif"),
                FontSize = 13.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = isDark ? new SolidColorBrush(Color.FromRgb(241, 245, 249)) : new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4)
            };

            // Frase secundária: "Vamos cuidar da sua hidratação juntos?"
            TextBlock txtSubMensagem = new TextBlock
            {
                Text = "Vamos cuidar da sua hidrata\u00E7\u00E3o juntos?",
                FontFamily = new FontFamily("Segoe UI, sans-serif"),
                FontSize = 12,
                FontWeight = FontWeights.Normal,
                Foreground = isDark ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };

            stackTextos.Children.Add(txtTitulo);
            stackTextos.Children.Add(txtMensagem);
            stackTextos.Children.Add(txtSubMensagem);

            // Animação de aparição suave
            AplicarAnimacaoEntradaTexto(txtTitulo, 220);
            AplicarAnimacaoEntradaTexto(txtMensagem, 380);
            AplicarAnimacaoEntradaTexto(txtSubMensagem, 520);

            return stackTextos;
        }

        private void AplicarAnimacaoEntradaTexto(UIElement elemento, int atrasoMs)
        {
            elemento.Opacity = 0.0;
            TranslateTransform tt = new TranslateTransform(0, 10);
            elemento.RenderTransform = tt;

            DoubleAnimation fadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(400))
            {
                BeginTime = TimeSpan.FromMilliseconds(atrasoMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation slideUp = new DoubleAnimation(10, 0, TimeSpan.FromMilliseconds(400))
            {
                BeginTime = TimeSpan.FromMilliseconds(atrasoMs),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            elemento.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            tt.BeginAnimation(TranslateTransform.YProperty, slideUp);
        }

        // ==========================================
        // 3. BOTÃO "ENTRAR NO APP" (Pílula delicada, moderna e nítida)
        // ==========================================
        private FrameworkElement CriarBotaoEntrar()
        {
            Grid btnWrapper = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 4, 0, 0),
                SnapsToDevicePixels = true,
                UseLayoutRounding = true
            };

            _btnEntrarBorder = new Border
            {
                Width = 196,
                Height = 42,
                CornerRadius = new CornerRadius(21),
                BorderThickness = new Thickness(1.0),
                BorderBrush = new SolidColorBrush(Color.FromArgb(140, 255, 255, 255)),
                Cursor = Cursors.Hand,
                SnapsToDevicePixels = true,
                UseLayoutRounding = true,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(2, 132, 199),
                    BlurRadius = 14,
                    ShadowDepth = 2,
                    Opacity = 0.22
                }
            };

            // Degradê suave azul-celeste
            LinearGradientBrush btnGradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            btnGradient.GradientStops.Add(new GradientStop(Color.FromRgb(56, 189, 248), 0.0));  // Sky 400
            btnGradient.GradientStops.Add(new GradientStop(Color.FromRgb(2, 132, 199), 0.55)); // Sky 600
            btnGradient.GradientStops.Add(new GradientStop(Color.FromRgb(3, 105, 161), 1.0));  // Sky 700
            _btnEntrarBorder.Background = btnGradient;

            // Brilho sutil superior
            Border shineOverlay = new Border
            {
                Height = 20,
                CornerRadius = new CornerRadius(21, 21, 0, 0),
                VerticalAlignment = VerticalAlignment.Top,
                IsHitTestVisible = false,
                Background = new LinearGradientBrush
                {
                    StartPoint = new Point(0.5, 0),
                    EndPoint = new Point(0.5, 1),
                    GradientStops = new GradientStopCollection
                    {
                        new GradientStop(Color.FromArgb(70, 255, 255, 255), 0.0),
                        new GradientStop(Color.FromArgb(0, 255, 255, 255), 1.0)
                    }
                }
            };

            // Conteúdo do botão
            StackPanel spBtn = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                SnapsToDevicePixels = true,
                UseLayoutRounding = true
            };

            TextBlock txtIcon = new TextBlock
            {
                Text = "\u2728",
                FontSize = 13,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 6, 0),
                Foreground = Brushes.White
            };

            TextBlock txtBtn = new TextBlock
            {
                Text = "Entrar no app",
                FontFamily = new FontFamily("Segoe UI, sans-serif"),
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };

            spBtn.Children.Add(txtIcon);
            spBtn.Children.Add(txtBtn);

            Grid innerBtnGrid = new Grid();
            innerBtnGrid.Children.Add(shineOverlay);
            innerBtnGrid.Children.Add(spBtn);
            _btnEntrarBorder.Child = innerBtnGrid;

            _btnScale = new ScaleTransform(1.0, 1.0);
            _btnEntrarBorder.RenderTransform = _btnScale;
            _btnEntrarBorder.RenderTransformOrigin = new Point(0.5, 0.5);

            // Respiração muito sutil e convidativa (1.0 -> 1.018)
            _btnPulseStoryboard = new Storyboard
            {
                RepeatBehavior = RepeatBehavior.Forever,
                AutoReverse = true
            };
            DoubleAnimation pulseX = new DoubleAnimation(1.0, 1.018, TimeSpan.FromSeconds(2.0))
            {
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            DoubleAnimation pulseY = new DoubleAnimation(1.0, 1.018, TimeSpan.FromSeconds(2.0))
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

            // Hover suave
            _btnEntrarBorder.MouseEnter += delegate
            {
                _btnPulseStoryboard.Pause();
                DoubleAnimation hoverScale = new DoubleAnimation(_btnScale.ScaleX, 1.03, TimeSpan.FromMilliseconds(160))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                _btnScale.BeginAnimation(ScaleTransform.ScaleXProperty, hoverScale);
                _btnScale.BeginAnimation(ScaleTransform.ScaleYProperty, hoverScale);
            };

            _btnEntrarBorder.MouseLeave += delegate
            {
                DoubleAnimation unhoverScale = new DoubleAnimation(_btnScale.ScaleX, 1.0, TimeSpan.FromMilliseconds(180))
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
                DoubleAnimation clickScale = new DoubleAnimation(0.96, TimeSpan.FromMilliseconds(80))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                _btnScale.BeginAnimation(ScaleTransform.ScaleXProperty, clickScale);
                _btnScale.BeginAnimation(ScaleTransform.ScaleYProperty, clickScale);
            };

            _btnEntrarBorder.MouseLeftButtonUp += delegate(object s, MouseButtonEventArgs e)
            {
                e.Handled = true;
                DoubleAnimation releaseScale = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(100));
                _btnScale.BeginAnimation(ScaleTransform.ScaleXProperty, releaseScale);
                _btnScale.BeginAnimation(ScaleTransform.ScaleYProperty, releaseScale);

                IniciarTransicaoParaApp();
            };

            btnWrapper.Children.Add(_btnEntrarBorder);

            // Animação de entrada do botão
            btnWrapper.Opacity = 0.0;
            TranslateTransform ttBtn = new TranslateTransform(0, 12);
            btnWrapper.RenderTransform = ttBtn;

            DoubleAnimation fadeInBtn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(420))
            {
                BeginTime = TimeSpan.FromMilliseconds(620),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation slideUpBtn = new DoubleAnimation(12, 0, TimeSpan.FromMilliseconds(420))
            {
                BeginTime = TimeSpan.FromMilliseconds(620),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            btnWrapper.BeginAnimation(UIElement.OpacityProperty, fadeInBtn);
            ttBtn.BeginAnimation(TranslateTransform.YProperty, slideUpBtn);

            return btnWrapper;
        }

        // ==========================================
        // 4. BOLHAS E BRILHOS SERENOS (Vetoriais puros e nítidos)
        // ==========================================
        private void CriarBolhasEFundo(Canvas canvas, bool isDark)
        {
            Random rnd = new Random();

            int totalBolhas = 9;
            for (int i = 0; i < totalBolhas; i++)
            {
                double tamanho = rnd.Next(8, 16);
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
                            new GradientStop(Color.FromArgb(140, 255, 255, 255), 0.0),
                            new GradientStop(Color.FromArgb(40, 224, 242, 254), 0.6),
                            new GradientStop(Color.FromArgb(10, 56, 189, 248), 1.0)
                        }
                    },
                    Stroke = new SolidColorBrush(Color.FromArgb(90, 255, 255, 255)),
                    StrokeThickness = 0.8,
                    IsHitTestVisible = false
                };

                double posX = rnd.Next(12, 300);
                double posY = rnd.Next(20, 410);
                Canvas.SetLeft(bolha, posX);
                Canvas.SetTop(bolha, posY);
                canvas.Children.Add(bolha);

                double duracao = rnd.Next(7000, 12000) / 1000.0;
                DoubleAnimation animSubida = new DoubleAnimation(450, -30, TimeSpan.FromSeconds(duracao))
                {
                    RepeatBehavior = RepeatBehavior.Forever,
                    BeginTime = TimeSpan.FromSeconds(rnd.Next(0, 3000) / 1000.0)
                };
                bolha.BeginAnimation(Canvas.TopProperty, animSubida);

                DoubleAnimation animDeriva = new DoubleAnimation(posX - 5, posX + 5, TimeSpan.FromSeconds(duracao / 2.0))
                {
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                };
                bolha.BeginAnimation(Canvas.LeftProperty, animDeriva);
            }

            // Brilhos cintilantes vetoriais puros (sem shaders para não criar borrão)
            CriarBrilhoEstrelar(canvas, 36, 65, 1.2, isDark);
            CriarBrilhoEstrelar(canvas, 280, 55, 1.8, isDark);
            CriarBrilhoEstrelar(canvas, 30, 250, 1.5, isDark);
            CriarBrilhoEstrelar(canvas, 285, 260, 2.1, isDark);
        }

        private void CriarBrilhoEstrelar(Canvas canvas, double x, double y, double delaySec, bool isDark)
        {
            System.Windows.Shapes.Path estrela = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse("M 6,0 Q 6,6 12,6 Q 6,6 6,12 Q 6,6 0,6 Q 6,6 6,0"),
                Fill = isDark ? new SolidColorBrush(Color.FromArgb(220, 186, 230, 253)) : new SolidColorBrush(Color.FromArgb(230, 56, 189, 248)),
                Opacity = 0.35,
                IsHitTestVisible = false
            };

            ScaleTransform st = new ScaleTransform(0.75, 0.75);
            estrela.RenderTransform = st;
            estrela.RenderTransformOrigin = new Point(0.5, 0.5);

            Canvas.SetLeft(estrela, x);
            Canvas.SetTop(estrela, y);
            canvas.Children.Add(estrela);

            DoubleAnimation pulseOpacity = new DoubleAnimation(0.2, 0.85, TimeSpan.FromSeconds(1.5))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = TimeSpan.FromSeconds(delaySec),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            DoubleAnimation pulseScale = new DoubleAnimation(0.7, 1.15, TimeSpan.FromSeconds(1.5))
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

        private Button CriarBotaoFechar(bool isDark)
        {
            Button btnFechar = new Button
            {
                Content = "\u2715",
                Width = 26,
                Height = 26,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 12, 12, 0),
                Cursor = Cursors.Hand,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = isDark ? Brushes.White : new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                SnapsToDevicePixels = true,
                UseLayoutRounding = true
            };

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(13));
            borderFactory.SetValue(Border.BackgroundProperty, new SolidColorBrush(isDark ? Color.FromArgb(40, 255, 255, 255) : Color.FromArgb(40, 0, 0, 0)));
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
                btnFechar.Foreground = isDark ? Brushes.White : new SolidColorBrush(Color.FromRgb(148, 163, 184));
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
        // 5. TRANSIÇÃO ELEGANTE PARA O APP
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

            DoubleAnimation fadeOut = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(220))
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

                    mainWindow.Opacity = 0.0;
                    mainWindow.Show();

                    DoubleAnimation fadeInMain = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(280))
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
