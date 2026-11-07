using System.Windows.Media.Imaging;
using System.Web.Script.Serialization;
using System;
using System.Runtime.InteropServices; // DPI awareness
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;
using Forms = System.Windows.Forms;
using Drawing = System.Drawing;

namespace GotinhaAgua
{
    // ==========================================
    // Modelos de Dados (compatíveis com C# 5 / .NET 4.0)
    // ==========================================
    public class RegistroAgua : INotifyPropertyChanged
    {
        private string _hora;
        private int _quantidade;
        private string _tipo;

        public string Hora
        {
            get { return _hora; }
            set { _hora = value; OnPropertyChanged("Hora"); }
        }

        public int Quantidade
        {
            get { return _quantidade; }
            set { _quantidade = value; OnPropertyChanged("Quantidade"); }
        }

        public string Tipo
        {
            get { return _tipo; }
            set { _tipo = value; OnPropertyChanged("Tipo"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public class DadosDia : INotifyPropertyChanged
    {
        private string _data;
        private int _totalMl;
        private List<RegistroAgua> _registros;

        public string Data
        {
            get { return _data; }
            set { _data = value; OnPropertyChanged("Data"); }
        }

        public int TotalMl
        {
            get { return _totalMl; }
            set { _totalMl = value; OnPropertyChanged("TotalMl"); }
        }

        public List<RegistroAgua> Registros
        {
            get { return _registros; }
            set { _registros = value; OnPropertyChanged("Registros"); }
        }

        public DadosDia()
        {
            _registros = new List<RegistroAgua>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public class HistoricoApp : INotifyPropertyChanged
    {
        private int _metaDiaria;
        private int _intervaloMinutos;
        private bool _somAtivado;
        private bool _notificacoesAtivadas;
        private bool _modoEscuro;
        private List<DadosDia> _dias;

        public int MetaDiaria
        {
            get { return _metaDiaria; }
            set { _metaDiaria = value; OnPropertyChanged("MetaDiaria"); }
        }

        public int IntervaloMinutos
        {
            get { return _intervaloMinutos; }
            set { _intervaloMinutos = value; OnPropertyChanged("IntervaloMinutos"); }
        }

        public bool SomAtivado
        {
            get { return _somAtivado; }
            set { _somAtivado = value; OnPropertyChanged("SomAtivado"); }
        }

        public bool NotificacoesAtivadas
        {
            get { return _notificacoesAtivadas; }
            set { _notificacoesAtivadas = value; OnPropertyChanged("NotificacoesAtivadas"); }
        }

        public bool ModoEscuro
        {
            get { return _modoEscuro; }
            set { _modoEscuro = value; OnPropertyChanged("ModoEscuro"); }
        }

        public List<DadosDia> Dias
        {
            get { return _dias; }
            set { _dias = value; OnPropertyChanged("Dias"); }
        }

        public HistoricoApp()
        {
            _metaDiaria = 2000;
            _intervaloMinutos = 30;
            _somAtivado = true;
            _notificacoesAtivadas = true;
            _modoEscuro = false;
            _dias = new List<DadosDia>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    // ==========================================
    // Sistema Nativo de Notificações do Windows (Action Center / Toasts)
    // ==========================================
    public static class ShellShortcutHelper
    {
        public const string APP_ID = "Gotinha.Agua.App";

        [DllImport("shell32.dll", SetLastError = true)]
        public static extern int SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);

        [ComImport, Guid("00021401-0000-0000-C000-000000000046"), ClassInterface(ClassInterfaceType.None)]
        private class CShellLink { }

        [ComImport, Guid("000214F9-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLinkW
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, uint fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
            void Resolve(IntPtr hwnd, uint fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [ComImport, Guid("0000010b-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPersistFile
        {
            void GetClassID(out Guid pClassID);
            void IsDirty();
            void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
            void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
            void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
            void GetCurFile([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder ppszFileName);
        }

        [ComImport, Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPropertyStore
        {
            void GetCount(out uint cProps);
            void GetAt(uint iProp, out IntPtr pkey);
            void GetValue(IntPtr key, out IntPtr pv);
            void SetValue(ref PropertyKey key, ref PropVariant pv);
            void Commit();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct PropertyKey
        {
            public Guid fmtid;
            public uint pid;
            public PropertyKey(Guid guid, uint id) { fmtid = guid; pid = id; }
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct PropVariant
        {
            [FieldOffset(0)] public ushort vt;
            [FieldOffset(8)] public IntPtr pwszVal;

            public static PropVariant FromString(string val)
            {
                PropVariant pv = new PropVariant();
                pv.vt = 31; // VT_LPWSTR
                pv.pwszVal = Marshal.StringToCoTaskMemUni(val);
                return pv;
            }
        }

        public static void GarantirAtalhoMenuIniciar()
        {
            try
            {
                string programsPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                string shortcutPath = System.IO.Path.Combine(programsPath, "Gotinha.lnk");
                string targetExe = Process.GetCurrentProcess().MainModule.FileName;
                string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");

                CShellLink link = new CShellLink();
                IShellLinkW shellLink = (IShellLinkW)link;
                shellLink.SetPath(targetExe);
                shellLink.SetWorkingDirectory(System.IO.Path.GetDirectoryName(targetExe));
                shellLink.SetDescription("Gotinha - Lembrete de Ãgua");
                if (System.IO.File.Exists(iconPath))
                {
                    shellLink.SetIconLocation(iconPath, 0);
                }

                IPropertyStore propStore = (IPropertyStore)link;
                PropertyKey pkey = new PropertyKey(new Guid("9F4C2855-9F79-48A4-8ED0-4A0831E0527F"), 5);
                PropVariant pv = PropVariant.FromString(APP_ID);
                try
                {
                    propStore.SetValue(ref pkey, ref pv);
                    propStore.Commit();
                }
                finally
                {
                    if (pv.pwszVal != IntPtr.Zero)
                        Marshal.FreeCoTaskMem(pv.pwszVal);
                }

                IPersistFile persistFile = (IPersistFile)link;
                persistFile.Save(shortcutPath, true);
            }
            catch { }
        }
    }

    public static class NotificadorWindows
    {
        private static DateTime _ultimoDisparo = DateTime.MinValue;

        public static void EnviarNotificacao(string titulo, string mensagem, Forms.NotifyIcon trayFallback)
        {
            // Evita disparos duplicados ou spam
            if ((DateTime.Now - _ultimoDisparo).TotalSeconds < 2)
            {
                return;
            }
            _ultimoDisparo = DateTime.Now;

            // Executa em segundo plano para manter fluidez perfeita da UI
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                bool enviadoWinRT = false;
                try
                {
                    string safeTitulo = titulo.Replace("'", "''").Replace("\"", "`\"");
                    string safeMensagem = mensagem.Replace("'", "''").Replace("\"", "`\"");
                    string mascotePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mascote.png");
                    string imgTag = System.IO.File.Exists(mascotePath) ? 
                        string.Format("<image placement=\"appLogoOverride\" hint-crop=\"circle\" src=\"{0}\"/>", mascotePath.Replace("\\", "/")) : "";

                    string psScript = string.Format(
                        "[Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null;" +
                        "[Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime] | Out-Null;" +
                        "$xml = New-Object Windows.Data.Xml.Dom.XmlDocument;" +
                        "$template = '<toast duration=\"short\"><visual><binding template=\"ToastGeneric\"><text>{0}</text><text>{1}</text>{2}</binding></visual><audio src=\"ms-winsoundevent:Notification.Default\"/></toast>';" +
                        "$xml.LoadXml($template);" +
                        "$toast = New-Object Windows.UI.Notifications.ToastNotification $xml;" +
                        "[Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('{3}').Show($toast);",
                        safeTitulo, safeMensagem, imgTag, ShellShortcutHelper.APP_ID);

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"" + psScript + "\"",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                               };

                    using (Process p = Process.Start(psi))
                    {
                        if (p != null)
                        {
                            p.WaitForExit(3500);
                            if (p.ExitCode == 0) enviadoWinRT = true;
                        }
                    }
                }
                catch { }

                // Fallback de garantia com BalloonTip do Windows caso a Central de AÃ§Ãµes esteja silenciada
                if (!enviadoWinRT && trayFallback != null)
                {
                    try
                    {
                        if (Application.Current != null && Application.Current.Dispatcher != null)
                        {
                            Application.Current.Dispatcher.Invoke(new Action(delegate
                            {
                                trayFallback.ShowBalloonTip(5000, titulo, mensagem, Forms.ToolTipIcon.Info);
                            }));
                        }
                    }
                    catch { }
                }
            });
        }
    }
    // ==========================================
    // Gerador de Sons em Memória (WAV PCM)
    // ==========================================
    public static class GeradorDeSom
    {
        public static void TocarGota()
        {
            try
            {
                byte[] wav = CriarWavGota();
                using (MemoryStream ms = new MemoryStream(wav))
                {
                    SoundPlayer sp = new SoundPlayer(ms);
                    sp.Play();
                }
            }
            catch { }
        }

        public static void TocarSinoLembrete()
        {
            try
            {
                byte[] wav = CriarWavSino();
                using (MemoryStream ms = new MemoryStream(wav))
                {
                    SoundPlayer sp = new SoundPlayer(ms);
                    sp.Play();
                }
            }
            catch { }
        }

        public static void TocarCelebracao()
        {
            try
            {
                byte[] wav = CriarWavCelebracao();
                using (MemoryStream ms = new MemoryStream(wav))
                {
                    SoundPlayer sp = new SoundPlayer(ms);
                    sp.Play();
                }
            }
            catch { }
        }

        private static byte[] CriarWavGota()
        {
            int sampleRate = 44100;
            double duration = 0.28;
            int numSamples = (int)(sampleRate * duration);
            short[] samples = new short[numSamples];

            for (int i = 0; i < numSamples; i++)
            {
                double t = (double)i / sampleRate;
                double freq = 500.0 + 700.0 * Math.Sin(Math.PI * t / duration);
                double envelope = Math.Sin(Math.PI * t / duration) * Math.Exp(-5.0 * t);
                double sample = Math.Sin(2.0 * Math.PI * freq * t) * envelope;
                samples[i] = (short)(sample * 16000);
            }

            return MontarArquivoWav(samples, sampleRate);
        }

        private static byte[] CriarWavSino()
        {
            int sampleRate = 44100;
            double noteLen = 0.16;
            double[] freqs = new double[] { 523.25, 659.25, 783.99, 1046.50            };
            int numSamples = (int)(sampleRate * (noteLen * freqs.Length + 0.3));
            short[] samples = new short[numSamples];

            for (int n = 0; n < freqs.Length; n++)
            {
                int start = (int)(n * noteLen * sampleRate);
                double f = freqs[n];
                int len = (int)(0.5 * sampleRate);

                for (int i = 0; i < len && (start + i) < numSamples; i++)
                {
                    double t = (double)i / sampleRate;
                    double env = Math.Exp(-8.0 * t);
                    double s = Math.Sin(2.0 * Math.PI * f * t) * env;
                    int idx = start + i;
                    samples[idx] = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, samples[idx] + (s * 10000)));
                }
            }

            return MontarArquivoWav(samples, sampleRate);
        }

        private static byte[] CriarWavCelebracao()
        {
            int sampleRate = 44100;
            double[] freqs = new double[] { 440, 554.37, 659.25, 880, 1108.73            };
            double noteLen = 0.12;
            int numSamples = (int)(sampleRate * 0.9);
            short[] samples = new short[numSamples];

            for (int n = 0; n < freqs.Length; n++)
            {
                int start = (int)(n * noteLen * sampleRate);
                double f = freqs[n];
                int len = (int)(0.4 * sampleRate);

                for (int i = 0; i < len && (start + i) < numSamples; i++)
                {
                    double t = (double)i / sampleRate;
                    double env = Math.Exp(-6.0 * t);
                    double s = Math.Sin(2.0 * Math.PI * f * t) * env;
                    int idx = start + i;
                    samples[idx] = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, samples[idx] + (s * 8000)));
                }
            }

            return MontarArquivoWav(samples, sampleRate);
        }

        private static byte[] MontarArquivoWav(short[] samples, int sampleRate)
        {
            int subChunk2Size = samples.Length * 2;
            int chunkSize = 36 + subChunk2Size;

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(chunkSize);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(sampleRate);
                writer.Write(sampleRate * 2);
                writer.Write((short)2);
                writer.Write((short)16);
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(subChunk2Size);

                for (int i = 0; i < samples.Length; i++)
                {
                    writer.Write(samples[i]);
                }

                writer.Flush();
                return stream.ToArray();
            }
        }
    }

    // ==========================================
    // Janela Principal do Aplicativo (WPF Moderno)
    // ==========================================
    public class MainWindow : Window
    {
        private HistoricoApp _dados;
        private string _caminhoArquivo;
        private DispatcherTimer _timerLembrete;
        private DispatcherTimer _timerAnimacaoMascote;
        private int _segundosRestantes;
        private bool _timerPausado = false;
        private List<Button> _botoesIntervalo = new List<Button>();

        private Forms.NotifyIcon _trayIcon;

        // Elementos da UI
        private TextBlock _txtMlAtual;
        private TextBlock _txtMetaStatus;
        private TextBlock _txtPorcentagem;
        private TextBlock _txtFaltaMl;
        private TextBlock _txtTimerDisplay;
        private TextBlock _txtMensagemGotinha;
        private Button _btnPausarTimer;
        private Button _btnSom;
        private Button _btnNotif;
        private Button _btnTema;
        private Button _btnConfig;
        private List<Border> _cardsLista = new List<Border>();
        private TextBlock _txtBrandTitle;
        private TextBlock _txtBrandSub;
        private Button _btnMin;
        private Button _btnClose;
        private Border _statusPill;
        private TextBlock _txtStatusPill;
        private TextBlock _txtHeaderTitle;
        private Border _speechBubble;
        private TextBlock _txtMlUnit;
        private Border _barTrack;
        private TextBlock _txtTimerLabel;
        private Button _btnTestarLembrete;
        private TextBlock _txtLembrarACada;
        private TextBlock _txtTituloRegistrar;
        private Button _btnDesfazer;
        private List<Button> _botoesBebida = new List<Button>();
        private TextBox _txtCustomAgua;

        private Border _bordaAguaCopo;
        private Border _copoContainer;
        private Canvas _canvasMascote;
        private System.Windows.Shapes.Path _pathBocaGotinha;
        private System.Windows.Shapes.Polygon _coroaGotinha;
        private Ellipse _olhoEsquerdo;
        private Ellipse _olhoDireito;
        private StackPanel _painelHistorico;
        private StackPanel _painelGraficoSemanal;
        private TabControl _tabHistorico;

        // Elementos de Feedback & Overlay
        private Grid _containerPrincipal;
        private Border _snackBar;
        private TextBlock _txtSnackBar;
        private Border _overlayLoading;
        private ProgressBar _progressBarMeta;
        private Border _janelaBorda;
        private Button _btnMax;
        private Grid _gridConteudo;
        private Border _cardStatusFalta;
        private Border _badgePorcentagem;
        private ColumnDefinition _colProgressoAtivo;
        private ColumnDefinition _colProgressoRestante;

        public MainWindow()
        {
            _caminhoArquivo = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dados_agua.json");
            CarregarDados();

            ConfigurarJanela();
            ConstruirInterface();
            ConfigurarBandejaSistema();

            IniciarTimers();
            AplicarTema();
            AtualizarInterface();
        }

                private void AlternarMaximizar()
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                MaxHeight = SystemParameters.WorkArea.Height;
                MaxWidth = SystemParameters.WorkArea.Width;
                WindowState = WindowState.Maximized;
            }
        }

                        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            SolidColorBrush cor = (_dados != null && _dados.ModoEscuro)
                ? new SolidColorBrush(Color.FromRgb(56, 189, 248))
                : new SolidColorBrush(Color.FromRgb(3, 105, 161));
            if (WindowState == WindowState.Maximized)
            {
                if (_janelaBorda != null)
                {
                    _janelaBorda.CornerRadius = new CornerRadius(0);
                    _janelaBorda.BorderThickness = new Thickness(0);
                }
                if (_btnMax != null)
                {
                    _btnMax.Content = CriarIconeMaximizar(cor, true);
                    _btnMax.ToolTip = "Restaurar";
                }
                if (_gridConteudo != null)
                {
                    _gridConteudo.Margin = new Thickness(24, 8, 24, 20);
                }
            }
            else if (WindowState == WindowState.Normal)
            {
                if (_janelaBorda != null)
                {
                    _janelaBorda.CornerRadius = new CornerRadius(20);
                    _janelaBorda.BorderThickness = new Thickness(1.5);
                }
                if (_btnMax != null)
                {
                    _btnMax.Content = CriarIconeMaximizar(cor, false);
                    _btnMax.ToolTip = "Maximizar";
                }
                if (_gridConteudo != null)
                {
                    _gridConteudo.Margin = new Thickness(12, 0, 12, 12);
                }
            }
        }

        private void ConfigurarJanela()
        {
            Title = "Gotinha - Lembrete de \u00C1gua \uD83D\uDCA7";
            Width = 880;
            Height = 520;
            MinWidth = 760;
            MinHeight = 460;
            MaxHeight = SystemParameters.WorkArea.Height;
            MaxWidth = SystemParameters.WorkArea.Width;
            ResizeMode = ResizeMode.CanResize;
            StateChanged += MainWindow_StateChanged;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;

            // Nitidez e renderização precisa de texto e bordas (evita desfoque no WPF)
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
            RenderOptions.SetClearTypeHint(this, ClearTypeHint.Enabled);

            string icoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
            if (File.Exists(icoPath))
            {
                try
                {
                    Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri(icoPath, UriKind.Absolute));
                }
                catch { }
            }

            Background = Brushes.Transparent;
        }

        private void ConstruirInterface()
        {
            _containerPrincipal = new Grid();

            // Card principal externo com cantos arredondados, borda e sombra suave
            _janelaBorda = new Border
            {
                CornerRadius = new CornerRadius(20),
                Background = new LinearGradientBrush(
                    Color.FromRgb(224, 242, 254),
                    Color.FromRgb(240, 249, 255),
                    135.0
                ),
                BorderBrush = new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                BorderThickness = new Thickness(1.5),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 36,
                    ShadowDepth = 6,
                    Opacity = 0.25,
                    Color = Color.FromRgb(2, 132, 199)
                }
                       };

            // Layout principal: TituloBar (topo) + área de conteúdo (widescreen 2 colunas)
            Grid layoutInterno = new Grid();
            layoutInterno.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Barra de título
            layoutInterno.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Conteúdo

            // 1. Barra de Título Customizada
            Grid titleBar = new Grid
            {
                Height = 42,
                Background = Brushes.Transparent
                       };
            titleBar.MouseDown += delegate(object s, MouseButtonEventArgs e)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    if (e.ClickCount == 2)
                    {
                        AlternarMaximizar();
                    }
                    else
                    {
                        if (WindowState == WindowState.Maximized)
                        {
                            Point p = e.GetPosition(this);
                            WindowState = WindowState.Normal;
                            Top = Math.Max(0, p.Y - 20);
                            Left = Math.Max(0, p.X - (Width / 2));
                        }
                        try { DragMove(); } catch { }
                    }
                }
            };

                        StackPanel appBrand = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(16, 0, 0, 0)
            };

            Border iconBadge = new Border
            {
                Width = 28,
                Height = 28,
                CornerRadius = new CornerRadius(14),
                Background = new LinearGradientBrush(
                    Color.FromRgb(56, 189, 248),
                    Color.FromRgb(2, 132, 199),
                    135.0
                ),
                Margin = new Thickness(0, 0, 8, 0),
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(2, 132, 199),
                    BlurRadius = 8,
                    ShadowDepth = 2,
                    Opacity = 0.455
                },
                Child = new TextBlock
                {
                    Text = "\uD83D\uDCA7",
                    FontSize = 13,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            appBrand.Children.Add(iconBadge);

            _txtBrandTitle = new TextBlock
            {
                Text = "Gotinha",
                FontWeight = FontWeights.ExtraBold,
                FontSize = 13.5,
                Foreground = new SolidColorBrush(Color.FromRgb(3, 105, 161)),
                VerticalAlignment = VerticalAlignment.Center
            };
            appBrand.Children.Add(_txtBrandTitle);

            _txtBrandSub = new TextBlock
            {
                Text = " \u2022 Lembrete de Hidrata\u00E7\u00E3o",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
                VerticalAlignment = VerticalAlignment.Center
            };
            appBrand.Children.Add(_txtBrandSub);

            _txtStatusPill = new TextBlock
            {
                Text = "\u25CF Ativo",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(5, 150, 105))
            };
            _statusPill = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(236, 253, 245)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(167, 243, 208)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(7),
                Padding = new Thickness(8, 2, 8, 2),
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Child = _txtStatusPill
            };
            Border statusPill = _statusPill;
            appBrand.Children.Add(statusPill);
            titleBar.Children.Add(appBrand);

                                    StackPanel windowButtons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 8, 0)
            };

            SolidColorBrush corIconeBarra = new SolidColorBrush(Color.FromRgb(3, 105, 161));

            _btnMin = new Button

            {
                Content = CriarIconeMinimizar(corIconeBarra),
                ToolTip = "Minimizar",
                Width = 44,
                Height = 32,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                FocusVisualStyle = null,
                Cursor = Cursors.Hand
            };
            _btnMin.MouseEnter += delegate { _btnMin.Background = new SolidColorBrush(Color.FromArgb(170, 224, 242, 254)); };
            _btnMin.MouseLeave += delegate { _btnMin.Background = Brushes.Transparent; };
            _btnMin.Click += delegate { WindowState = WindowState.Minimized; };

            _btnMax = new Button
            {
                Content = CriarIconeMaximizar(corIconeBarra, WindowState == WindowState.Maximized),
                ToolTip = (WindowState == WindowState.Maximized ? "Restaurar" : "Maximizar"),
                Width = 44,
                Height = 32,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                FocusVisualStyle = null,
                Cursor = Cursors.Hand
            };
            _btnMax.MouseEnter += delegate { _btnMax.Background = new SolidColorBrush(Color.FromArgb(170, 224, 242, 254)); };
            _btnMax.MouseLeave += delegate { _btnMax.Background = Brushes.Transparent; };
            _btnMax.Click += delegate { AlternarMaximizar(); };

            _btnClose = new Button

            {
                Content = CriarIconeFechar(corIconeBarra),
                ToolTip = "Minimizar para a bandeja",
                Width = 44,
                Height = 32,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                FocusVisualStyle = null,
                Cursor = Cursors.Hand
            };
            _btnClose.MouseEnter += delegate
            {
                _btnClose.Background = new SolidColorBrush(Color.FromRgb(239, 68, 68));
                _btnClose.Content = CriarIconeFechar(Brushes.White);
            };
            _btnClose.MouseLeave += delegate
            {
                _btnClose.Background = Brushes.Transparent;
                _btnClose.Content = CriarIconeFechar(corIconeBarra);
            };
            _btnClose.Click += delegate
            {
                WindowState = WindowState.Minimized;
                if (_dados.NotificacoesAtivadas)
                {
                    NotificadorWindows.EnviarNotificacao(
                        "Gotinha continua rodando \uD83D\uDCA7",
                        "Vou continuar te lembrando de beber \u00E1gua enquanto voc\u00EA usa o computador!",
                        _trayIcon
                    );
                }
            };

            windowButtons.Children.Add(_btnMin);
            windowButtons.Children.Add(_btnMax);
            windowButtons.Children.Add(_btnClose);
            titleBar.Children.Add(windowButtons);

            Grid.SetRow(titleBar, 0);
            layoutInterno.Children.Add(titleBar);

            // 2. Área de Conteúdo Widescreen — dois painéis lado a lado
            // Coluna esquerda (~45%): Header (mascote) + Progresso (copo + stats) + Lembrete/Timer
            // Coluna direita (~55%): Botões de registro + Histórico
                        _gridConteudo = new Grid { Margin = new Thickness(12, 0, 12, 12) };
            _gridConteudo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(46, GridUnitType.Star) });
            _gridConteudo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) }); // espaÃ§o
            _gridConteudo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(54, GridUnitType.Star) });

            // --- Coluna Esquerda: Hero Card Unificado da Gotinha + Temporizador ---
            Grid colunaEsq = new Grid();
            colunaEsq.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            colunaEsq.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            UIElement cardHero = CriarCardHero();
            Grid.SetRow(cardHero, 0);
            colunaEsq.Children.Add(cardHero);

            UIElement cardLembrete = CriarCardLembrete();
            Grid.SetRow(cardLembrete, 1);
            colunaEsq.Children.Add(cardLembrete);

            Grid.SetColumn(colunaEsq, 0);
            _gridConteudo.Children.Add(colunaEsq);

            // --- Coluna Direita ---
            Grid colunaDir = new Grid();
            colunaDir.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            colunaDir.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            UIElement cardBotoes = CriarCardBotoes();
            Grid.SetRow(cardBotoes, 0);
            colunaDir.Children.Add(cardBotoes);

            UIElement cardHistorico = CriarCardHistorico();
            Grid.SetRow(cardHistorico, 1);
            colunaDir.Children.Add(cardHistorico);

            Grid.SetColumn(colunaDir, 2);
            _gridConteudo.Children.Add(colunaDir);

            Grid.SetRow(_gridConteudo, 1);
            layoutInterno.Children.Add(_gridConteudo);

            _janelaBorda.Child = layoutInterno;
            _containerPrincipal.Children.Add(_janelaBorda);

            // Animação suave de entrada da interface (Fade + Slide suave)
            TranslateTransform ttEntrada = new TranslateTransform(0, 14);
            _janelaBorda.RenderTransform = ttEntrada;
            _janelaBorda.Opacity = 0;

            Loaded += delegate
            {
                DoubleAnimation animOp = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(380))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                           };
                DoubleAnimation animTr = new DoubleAnimation(14, 0, TimeSpan.FromMilliseconds(380))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                           };
                _janelaBorda.BeginAnimation(UIElement.OpacityProperty, animOp);
                ttEntrada.BeginAnimation(TranslateTransform.YProperty, animTr);
                       };

            // 3. Feedback Visual: SnackBar Flutuante
            CriarSnackBar();

            // 4. Overlay de Carregamento Seguro
            CriarOverlayLoading();

            Content = _containerPrincipal;
        }

        private void CriarSnackBar()
        {
            _snackBar = new Border
            {
                Background = new LinearGradientBrush(
                    Color.FromRgb(12, 74, 110),
                    Color.FromRgb(3, 105, 161),
                    135.0
                ),
                BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(16, 10, 16, 10),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, 24),
                Opacity = 0,
                IsHitTestVisible = false,
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(2, 132, 199),
                    BlurRadius = 16,
                    ShadowDepth = 4,
                    Opacity = 0.45
                }
                       };

            _txtSnackBar = new TextBlock
            {
                Text = "\u00C1gua registrada com sucesso! \uD83D\uDCA7",
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                FontSize = 12
                       };
            _snackBar.Child = _txtSnackBar;
            _containerPrincipal.Children.Add(_snackBar);
        }

        private void ExibirSnackBar(string texto)
        {
            _txtSnackBar.Text = texto;
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300))
            {
                BeginTime = TimeSpan.FromSeconds(2.0)
                       };

            Storyboard sb = new Storyboard();
            Storyboard.SetTarget(fadeIn, _snackBar);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath(UIElement.OpacityProperty));
            Storyboard.SetTarget(fadeOut, _snackBar);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(UIElement.OpacityProperty));

            sb.Children.Add(fadeIn);
            sb.Children.Add(fadeOut);
            sb.Begin();
        }

        private void CriarOverlayLoading()
        {
            _overlayLoading = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(140, 255, 255, 255)),
                CornerRadius = new CornerRadius(20),
                Visibility = Visibility.Collapsed
                       };

            StackPanel sp = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
                       };

            ProgressBar pbar = new ProgressBar
            {
                IsIndeterminate = true,
                Width = 140,
                Height = 6,
                Foreground = new SolidColorBrush(Color.FromRgb(2, 132, 199)),
                BorderThickness = new Thickness(0)
                       };

            TextBlock txt = new TextBlock
            {
                Text = "Salvando dados...",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 8, 0, 0)
                       };

            sp.Children.Add(pbar);
            sp.Children.Add(txt);
            _overlayLoading.Child = sp;

            _containerPrincipal.Children.Add(_overlayLoading);
        }

        // --- Hero Card: Gotinha Protagonista & Progresso Integrado ---
        private UIElement CriarCardHero()
        {
            Border card = CriarCardBase();
            card.Padding = new Thickness(14, 12, 14, 14);

            StackPanel mainStack = new StackPanel();

            // 1. Barra Superior: Marca Gotinha + Badge de Amiga + Toolbar de Ações
            Grid topBar = new Grid();
            topBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel brandSp = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            _txtHeaderTitle = new TextBlock
            {
                Text = "Gotinha",
                FontSize = 19,
                FontWeight = FontWeights.ExtraBold,
                Foreground = new SolidColorBrush(Color.FromRgb(2, 132, 199)),
                VerticalAlignment = VerticalAlignment.Center
            };
            brandSp.Children.Add(_txtHeaderTitle);

            Border badgeAmiga = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(200, 224, 242, 254)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 2, 8, 2),
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Child = new TextBlock
                {
                    Text = "\u2728 Sua Companheira",
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(2, 132, 199))
                }
            };
            brandSp.Children.Add(badgeAmiga);
            Grid.SetColumn(brandSp, 0);
            topBar.Children.Add(brandSp);

            // Toolbar com botões redondinhos elegantes
            StackPanel toolbarSp = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            _btnSom = CriarBotaoToolbarCard(CriarIconeSom(_dados.SomAtivado), _dados.SomAtivado ? "Som ativado" : "Som silenciado", delegate { AlternarSom(); }, false);
            _btnNotif = CriarBotaoToolbarCard(CriarIconeNotif(_dados.NotificacoesAtivadas), _dados.NotificacoesAtivadas ? "Notifica\u00E7\u00F5es ativadas" : "Notifica\u00E7\u00F5es desativadas", delegate { AlternarNotificacoes(); }, false);
            _btnTema = CriarBotaoToolbarCard(CriarIconeTema(_dados.ModoEscuro), _dados.ModoEscuro ? "Ativar Modo Claro" : "Ativar Modo Escuro", delegate { AlternarModoEscuro(); }, false);
            _btnConfig = CriarBotaoToolbarCard(CriarIconeEngrenagem(), "Configurar Meta e Prefer\u00EAncias", delegate { AbrirJanelaConfiguracoes(); }, true);

            toolbarSp.Children.Add(_btnSom);
            toolbarSp.Children.Add(_btnNotif);
            toolbarSp.Children.Add(_btnTema);
            toolbarSp.Children.Add(_btnConfig);
            Grid.SetColumn(toolbarSp, 1);
            topBar.Children.Add(toolbarSp);

            mainStack.Children.Add(topBar);

            // 2. Área Central: Gotinha Grande e Fofa + Balão de Fala + Volume Atual
            Grid heroBody = new Grid { Margin = new Thickness(0, 10, 0, 10) };
            heroBody.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(104) });
            heroBody.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            _canvasMascote = CriarMascoteCanvas();
            Grid.SetColumn(_canvasMascote, 0);
            heroBody.Children.Add(_canvasMascote);

            StackPanel heroRight = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };

            // Balão de fala moderno e acolhedor
            _speechBubble = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(235, 240, 249, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(10, 7, 10, 7),
                Margin = new Thickness(0, 0, 0, 8),
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(2, 132, 199),
                    BlurRadius = 8,
                    ShadowDepth = 1,
                    Opacity = 0.08
                }
            };
            _txtMensagemGotinha = new TextBlock
            {
                Text = "Oi! Vamos cuidar da sua hidrata\u00E7\u00E3o hoje? \uD83D\uDCA7",
                FontSize = 11.5,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(3, 105, 161)),
                TextWrapping = TextWrapping.Wrap
            };
            _speechBubble.Child = _txtMensagemGotinha;
            heroRight.Children.Add(_speechBubble);

            // Linha do Volume e Porcentagem
            Grid volGrid = new Grid();
            volGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            volGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            StackPanel mlStack = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Bottom };
            _txtMlAtual = new TextBlock
            {
                Text = "0",
                FontSize = 34,
                FontWeight = FontWeights.ExtraBold,
                Foreground = new SolidColorBrush(Color.FromRgb(2, 132, 199))
            };
            _txtMlUnit = new TextBlock
            {
                Text = " ml",
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(2, 0, 0, 5)
            };
            mlStack.Children.Add(_txtMlAtual);
            mlStack.Children.Add(_txtMlUnit);
            Grid.SetColumn(mlStack, 0);
            volGrid.Children.Add(mlStack);

            _badgePorcentagem = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(224, 242, 254)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(8, 3, 8, 3),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            _txtPorcentagem = new TextBlock
            {
                Text = "0% da meta",
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(2, 132, 199))
            };
            _badgePorcentagem.Child = _txtPorcentagem;
            Grid.SetColumn(_badgePorcentagem, 1);
            volGrid.Children.Add(_badgePorcentagem);

            heroRight.Children.Add(volGrid);

            _txtMetaStatus = new TextBlock
            {
                Text = "Meta di\u00E1ria: 2000 ml",
                FontSize = 11.5,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 2, 0, 0)
            };
            heroRight.Children.Add(_txtMetaStatus);

            Grid.SetColumn(heroRight, 1);
            heroBody.Children.Add(heroRight);

            mainStack.Children.Add(heroBody);

            // 3. Barra de Progresso Fluida com Gradiente de Água
            _barTrack = new Border
            {
                Height = 11,
                CornerRadius = new CornerRadius(5.5),
                Background = new SolidColorBrush(Color.FromRgb(224, 242, 254)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                BorderThickness = new Thickness(1),
                ClipToBounds = true,
                Margin = new Thickness(0, 2, 0, 8)
            };
            Grid barGrid = new Grid();
            _colProgressoAtivo = new ColumnDefinition { Width = new GridLength(0, GridUnitType.Star) };
            _colProgressoRestante = new ColumnDefinition { Width = new GridLength(100, GridUnitType.Star) };
            barGrid.ColumnDefinitions.Add(_colProgressoAtivo);
            barGrid.ColumnDefinitions.Add(_colProgressoRestante);

            Border barFill = new Border
            {
                CornerRadius = new CornerRadius(5.5),
                Background = new LinearGradientBrush(
                    Color.FromRgb(56, 189, 248),
                    Color.FromRgb(2, 132, 199),
                    0.0
                )
            };
            Grid.SetColumn(barFill, 0);
            barGrid.Children.Add(barFill);
            _barTrack.Child = barGrid;
            mainStack.Children.Add(_barTrack);

            _progressBarMeta = new ProgressBar
            {
                Height = 0,
                Visibility = Visibility.Collapsed
            };
            mainStack.Children.Add(_progressBarMeta);

            // 4. Pílula de Incentivo da Meta
            _cardStatusFalta = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(220, 240, 249, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10, 6, 10, 6)
            };
            _txtFaltaMl = new TextBlock
            {
                Text = "\uD83D\uDCA7 Faltam 2000 ml para a sua meta di\u00E1ria!",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(3, 105, 161)),
                TextWrapping = TextWrapping.Wrap
            };
            _cardStatusFalta.Child = _txtFaltaMl;
            mainStack.Children.Add(_cardStatusFalta);

            // Copo em background para compatibilidade de animação
            _copoContainer = new Border { Visibility = Visibility.Collapsed };
            _bordaAguaCopo = new Border { Height = 0 };
            _copoContainer.Child = _bordaAguaCopo;
            mainStack.Children.Add(_copoContainer);

            card.Child = mainStack;
            return card;
        }

        private Canvas CriarMascoteCanvas()
        {
            Canvas canvas = new Canvas
            {
                Width = 98,
                Height = 100,
                Cursor = Cursors.Hand
            };

            string mascotPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mascote.png");
            if (File.Exists(mascotPath))
            {
                try
                {
                    Border imgBorder = new Border
                    {
                        Width = 92,
                        Height = 92,
                        CornerRadius = new CornerRadius(24),
                        ClipToBounds = true,
                        BorderBrush = new SolidColorBrush(Color.FromRgb(125, 211, 252)),
                        BorderThickness = new Thickness(2),
                        RenderTransformOrigin = new Point(0.5, 0.5),
                        RenderTransform = new ScaleTransform(1.0, 1.0),
                        Effect = new DropShadowEffect
                        {
                            BlurRadius = 20,
                            ShadowDepth = 3,
                            Opacity = 0.38,
                            Color = Color.FromRgb(2, 132, 199)
                        }
                    };

                    Image img = new Image
                    {
                        Width = 92,
                        Height = 92,
                        Stretch = Stretch.UniformToFill
                    };
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(mascotPath, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    img.Source = bmp;

                    imgBorder.Child = img;
                    Canvas.SetLeft(imgBorder, 3);
                    Canvas.SetTop(imgBorder, 5);
                    canvas.Children.Add(imgBorder);

                    // Efeito suave de hover na Gotinha
                    imgBorder.MouseEnter += delegate
                    {
                        ScaleTransform st = imgBorder.RenderTransform as ScaleTransform;
                        if (st != null)
                        {
                            DoubleAnimation sa = new DoubleAnimation(1.0, 1.06, TimeSpan.FromMilliseconds(160))
                            {
                                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                            };
                            st.BeginAnimation(ScaleTransform.ScaleXProperty, sa);
                            st.BeginAnimation(ScaleTransform.ScaleYProperty, sa);
                        }
                    };
                    imgBorder.MouseLeave += delegate
                    {
                        ScaleTransform st = imgBorder.RenderTransform as ScaleTransform;
                        if (st != null)
                        {
                            DoubleAnimation sa = new DoubleAnimation(1.06, 1.0, TimeSpan.FromMilliseconds(180))
                            {
                                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                            };
                            st.BeginAnimation(ScaleTransform.ScaleXProperty, sa);
                            st.BeginAnimation(ScaleTransform.ScaleYProperty, sa);
                        }
                    };

                    _coroaGotinha = new System.Windows.Shapes.Polygon
                    {
                        Points = new PointCollection(new Point[] {
                            new Point(34, 14), new Point(40, 2), new Point(48, 8), new Point(56, 2), new Point(62, 14)
                        }),
                        Fill = new SolidColorBrush(Color.FromRgb(251, 191, 36)),
                        Stroke = new SolidColorBrush(Color.FromRgb(217, 119, 6)),
                        StrokeThickness = 1.5,
                        Visibility = Visibility.Collapsed
                    };
                    canvas.Children.Add(_coroaGotinha);

                    canvas.MouseLeftButtonDown += delegate { InteragirComGotinha(); };
                    return canvas;
                }
                catch { }
            }

            // Fallback vetorial gracioso caso imagem não exista
            System.Windows.Shapes.Path corpo = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse("M 48 12 C 48 12 14 58 14 74 C 14 94 28 100 48 100 C 68 100 82 94 82 74 C 82 58 48 12 48 12 Z"),
                Fill = new LinearGradientBrush(
                    Color.FromRgb(56, 189, 248),
                    Color.FromRgb(2, 132, 199),
                    90
                ),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 16,
                    ShadowDepth = 3,
                    Opacity = 0.32,
                    Color = Color.FromRgb(2, 132, 199)
                }
            };
            canvas.Children.Add(corpo);

            _olhoEsquerdo = new Ellipse { Width = 7, Height = 9, Fill = new SolidColorBrush(Color.FromRgb(30, 41, 59)) };
            Canvas.SetLeft(_olhoEsquerdo, 34);
            Canvas.SetTop(_olhoEsquerdo, 66);
            canvas.Children.Add(_olhoEsquerdo);

            _olhoDireito = new Ellipse { Width = 7, Height = 9, Fill = new SolidColorBrush(Color.FromRgb(30, 41, 59)) };
            Canvas.SetLeft(_olhoDireito, 55);
            Canvas.SetTop(_olhoDireito, 66);
            canvas.Children.Add(_olhoDireito);

            _pathBocaGotinha = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse("M 42 77 Q 48 84 54 77"),
                Stroke = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                StrokeThickness = 2.5,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            canvas.Children.Add(_pathBocaGotinha);

            _coroaGotinha = new System.Windows.Shapes.Polygon
            {
                Points = new PointCollection(new Point[] {
                    new Point(34, 22), new Point(40, 10), new Point(48, 17), new Point(56, 10), new Point(62, 22)
                }),
                Fill = new SolidColorBrush(Color.FromRgb(251, 191, 36)),
                Stroke = new SolidColorBrush(Color.FromRgb(217, 119, 6)),
                StrokeThickness = 1.5,
                Visibility = Visibility.Collapsed
            };
            canvas.Children.Add(_coroaGotinha);

            canvas.MouseLeftButtonDown += delegate { InteragirComGotinha(); };
            return canvas;
        }

        // --- Card 3: Lembrete e Timer ---
                private UIElement CriarCardLembrete()
        {
            Border card = CriarCardBase();

            StackPanel sp = new StackPanel();

            Grid topoGrid = new Grid();
            topoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            topoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock iconeRelogio = new TextBlock
            {
                Text = "\u23F0",
                FontSize = 24,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };
            Grid.SetColumn(iconeRelogio, 0);
            topoGrid.Children.Add(iconeRelogio);

            StackPanel timerTextos = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            _txtTimerLabel = new TextBlock
            {
                Text = "PR\u00D3XIMO LEMBRETE",
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184))
            };
            timerTextos.Children.Add(_txtTimerLabel);
            _txtTimerDisplay = new TextBlock
            {
                Text = "30:00",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 42))
            };
            timerTextos.Children.Add(_txtTimerDisplay);
            Grid.SetColumn(timerTextos, 1);
            topoGrid.Children.Add(timerTextos);

            // AÃ§Ãµes do Timer
            StackPanel btnRow = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            _btnPausarTimer = CriarBotaoPill("\u23F8 Pausar", false, delegate { AlternarPausaTimer(); });
            _btnTestarLembrete = CriarBotaoPill("\uD83D\uDD14 Testar", true, delegate { DispararLembrete(); });
            Button btnTestar = _btnTestarLembrete;
            btnRow.Children.Add(_btnPausarTimer);
            btnRow.Children.Add(btnTestar);
            Grid.SetColumn(btnRow, 2);
            topoGrid.Children.Add(btnRow);

            sp.Children.Add(topoGrid);

            // Seletor de Intervalos
            StackPanel intervalRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 12, 0, 0)
                       };
            _txtLembrarACada = new TextBlock
            {
                Text = "Lembrar a cada:",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };
            intervalRow.Children.Add(_txtLembrarACada);

            int[] mins = new int[] { 15, 30, 45, 60            };
            foreach (int m in mins)
            {
                Button btnM = new Button
                {
                    Content = m + "m",
                    Tag = m,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Padding = new Thickness(10, 4, 10, 4),
                    Margin = new Thickness(3, 0, 3, 0),
                    Cursor = Cursors.Hand,
                    Background = (m == _dados.IntervaloMinutos) ? new SolidColorBrush(Color.FromRgb(2, 132, 199)) : new SolidColorBrush(Color.FromRgb(240, 249, 255)),
                    Foreground = (m == _dados.IntervaloMinutos) ? Brushes.White : new SolidColorBrush(Color.FromRgb(3, 105, 161)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                    BorderThickness = new Thickness(1)
                           };
                btnM.Click += delegate(object s, RoutedEventArgs e)
                {
                    DefinirIntervalo((int)((Button)s).Tag);
                           };
                _botoesIntervalo.Add(btnM);
                intervalRow.Children.Add(btnM);
            }

            sp.Children.Add(intervalRow);

            card.Child = sp;
            return card;
        }

        // --- Card 4: Botões de Registro Rápido ---
        private UIElement CriarCardBotoes()
        {
            Border card = CriarCardBase();

            StackPanel sp = new StackPanel();

            Grid tituloRow = new Grid();
            tituloRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            tituloRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _txtTituloRegistrar = new TextBlock
            {
                Text = "Registrar \u00C1gua",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 42))
            };
            TextBlock titulo = _txtTituloRegistrar;
            Grid.SetColumn(titulo, 0);
            tituloRow.Children.Add(titulo);

            _btnDesfazer = new Button
            {
                Content = "\u21A9 Desfazer",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            Button btnDesfazer = _btnDesfazer;
            btnDesfazer.Click += delegate { DesfazerUltimo();            };
            Grid.SetColumn(btnDesfazer, 1);
            tituloRow.Children.Add(btnDesfazer);

            sp.Children.Add(tituloRow);

            UniformGrid gridBotoes = new UniformGrid { Columns = 4, Margin = new Thickness(0, 10, 0, 8) };
            _botoesBebida.Clear();
            Button b1 = CriarBotaoBebida("\u2615", "Gole", 150, false);
            Button b2 = CriarBotaoBebida("\uD83E\uDD5B", "Copo", 250, true);
            Button b3 = CriarBotaoBebida("\uD83E\uDD64", "Grande", 350, false);
            Button b4 = CriarBotaoBebida("\uD83C\uDF76", "Garrafa", 500, false);
            _botoesBebida.Add(b1);
            _botoesBebida.Add(b2);
            _botoesBebida.Add(b3);
            _botoesBebida.Add(b4);
            gridBotoes.Children.Add(b1);
            gridBotoes.Children.Add(b2);
            gridBotoes.Children.Add(b3);
            gridBotoes.Children.Add(b4);

            sp.Children.Add(gridBotoes);

            // Entrada personalizada
            Grid customGrid = new Grid();
            customGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            customGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _txtCustomAgua = new TextBox
            {
                FontSize = 12,
                Padding = new Thickness(10, 6, 10, 6),
                VerticalContentAlignment = VerticalAlignment.Center,
                BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                Background = Brushes.White
            };
            TextBox txtCustom = _txtCustomAgua;
            txtCustom.ToolTip = "Digite um valor em ml (ex: 200, 400)";
            Grid.SetColumn(txtCustom, 0);
            customGrid.Children.Add(txtCustom);

            Button btnAddCustom = new Button
            {
                Content = "+ Adicionar ml",
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromRgb(2, 132, 199)),
                Padding = new Thickness(14, 6, 14, 6),
                Margin = new Thickness(8, 0, 0, 0),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
                       };
            btnAddCustom.Click += delegate
            {
                int val;
                if (int.TryParse(txtCustom.Text, out val) && val > 0 && val <= 3000)
                {
                    AdicionarAgua(val, "Personalizado 💧");
                    txtCustom.Text = "";
                }
                else
                {
                    MessageBox.Show("Por favor digite um número válido em ml (ex: 200).", "Valor inválido", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                       };
            Grid.SetColumn(btnAddCustom, 1);
            customGrid.Children.Add(btnAddCustom);

            sp.Children.Add(customGrid);

            card.Child = sp;
            return card;
        }

        private Button CriarBotaoBebida(string icone, string nome, int ml, bool destaque = false)
        {
            Button btn = new Button
            {
                Tag = destaque,
                Margin = new Thickness(3),
                Padding = new Thickness(4, 10, 4, 10),
                Background = destaque ? new SolidColorBrush(Color.FromRgb(240, 249, 255)) : Brushes.White,
                BorderBrush = destaque ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                BorderThickness = new Thickness(destaque ? 1.5 : 1),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1.0, 1.0),
                Cursor = Cursors.Hand
                       };

            StackPanel sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center            };
            sp.Children.Add(new TextBlock { Text = icone, FontSize = 22, HorizontalAlignment = HorizontalAlignment.Center });
            sp.Children.Add(new TextBlock { Text = nome, FontSize = 11, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 42)), HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 2, 0, 0) });
            sp.Children.Add(new TextBlock { Text = "+" + ml + " ml", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Color.FromRgb(2, 132, 199)), HorizontalAlignment = HorizontalAlignment.Center });

            btn.Content = sp;

            btn.MouseEnter += delegate
            {
                if (btn.RenderTransform is ScaleTransform)
                {
                    ScaleTransform st = (ScaleTransform)btn.RenderTransform;
                    DoubleAnimation sa = new DoubleAnimation(1.0, 1.05, TimeSpan.FromMilliseconds(160))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                               };
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, sa);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, sa);
                }
                       };
            btn.MouseLeave += delegate
            {
                if (btn.RenderTransform is ScaleTransform)
                {
                    ScaleTransform st = (ScaleTransform)btn.RenderTransform;
                    DoubleAnimation sa = new DoubleAnimation(1.05, 1.0, TimeSpan.FromMilliseconds(160))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                               };
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, sa);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, sa);
                }
                       };
            btn.PreviewMouseDown += delegate
            {
                if (btn.RenderTransform is ScaleTransform)
                {
                    ScaleTransform st = (ScaleTransform)btn.RenderTransform;
                    DoubleAnimation sa = new DoubleAnimation(1.05, 0.94, TimeSpan.FromMilliseconds(80));
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, sa);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, sa);
                }
                       };
            btn.PreviewMouseUp += delegate
            {
                if (btn.RenderTransform is ScaleTransform)
                {
                    ScaleTransform st = (ScaleTransform)btn.RenderTransform;
                    DoubleAnimation sa = new DoubleAnimation(0.94, 1.05, TimeSpan.FromMilliseconds(100));
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, sa);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, sa);
                }
                       };
            btn.Click += delegate
            {
                AdicionarAgua(ml, nome + " (" + icone + ")");
                       };
            return btn;
        }

        // --- Card 5: Histórico de Hoje e Semana ---
        private UIElement CriarCardHistorico()
        {
            Border card = CriarCardBase();

            _tabHistorico = new TabControl
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0)
                       };

            // Aba 1: Hoje
            TabItem tabHoje = new TabItem { Header = "Hoje"            };
            ScrollViewer scrollHoje = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto            };
            _painelHistorico = new StackPanel { Margin = new Thickness(0, 8, 0, 0)            };
            scrollHoje.Content = _painelHistorico;
            tabHoje.Content = scrollHoje;
            _tabHistorico.Items.Add(tabHoje);

            // Aba 2: Últimos 7 dias
            TabItem tabSemana = new TabItem { Header = "Últimos 7 dias"            };
            _painelGraficoSemanal = new StackPanel { Margin = new Thickness(0, 8, 0, 0)            };
            tabSemana.Content = _painelGraficoSemanal;
            _tabHistorico.Items.Add(tabSemana);

            _tabHistorico.SelectionChanged += delegate
            {
                if (_tabHistorico.SelectedItem is TabItem)
                {
                    TabItem ti = (TabItem)_tabHistorico.SelectedItem;
                    if (ti.Content is UIElement)
                    {
                        UIElement contentElem = (UIElement)ti.Content;
                        DoubleAnimation tabFade = new DoubleAnimation(0.2, 1.0, TimeSpan.FromMilliseconds(220))
                        {
                            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                                   };
                        contentElem.BeginAnimation(UIElement.OpacityProperty, tabFade);
                    }
                }
                if (_tabHistorico.SelectedIndex == 1)
                {
                    AtualizarGraficoSemanal();
                }
                       };

            card.Child = _tabHistorico;
            return card;
        }

        // ==========================================
        // Lógica de Negócio e Estados
        // ==========================================
        private void DispararFeedbackAgua()
        {
            if (_copoContainer != null && _copoContainer.RenderTransform is ScaleTransform)
            {
                ScaleTransform stC = (ScaleTransform)_copoContainer.RenderTransform;
                DoubleAnimation pulseAnim = new DoubleAnimation(1.0, 1.07, TimeSpan.FromMilliseconds(150))
                {
                    AutoReverse = true,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                           };
                stC.BeginAnimation(ScaleTransform.ScaleXProperty, pulseAnim);
                stC.BeginAnimation(ScaleTransform.ScaleYProperty, pulseAnim);
            }

            if (_txtMlAtual != null)
            {
                if (!(_txtMlAtual.RenderTransform is ScaleTransform))
                {
                    _txtMlAtual.RenderTransformOrigin = new Point(0.5, 0.5);
                    _txtMlAtual.RenderTransform = new ScaleTransform(1.0, 1.0);
                }
                ScaleTransform stNum = (ScaleTransform)_txtMlAtual.RenderTransform;
                DoubleAnimation numPop = new DoubleAnimation(1.0, 1.20, TimeSpan.FromMilliseconds(140))
                {
                    AutoReverse = true,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                           };
                stNum.BeginAnimation(ScaleTransform.ScaleXProperty, numPop);
                stNum.BeginAnimation(ScaleTransform.ScaleYProperty, numPop);
            }

            if (_canvasMascote != null)
            {
                if (!(_canvasMascote.RenderTransform is TranslateTransform))
                {
                    _canvasMascote.RenderTransform = new TranslateTransform(0, 0);
                }
                TranslateTransform ttG = (TranslateTransform)_canvasMascote.RenderTransform;
                DoubleAnimation puloAnim = new DoubleAnimation(0, -9, TimeSpan.FromMilliseconds(170))
                {
                    AutoReverse = true,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                           };
                ttG.BeginAnimation(TranslateTransform.YProperty, puloAnim);
            }
        }

        public void AdicionarAgua(int ml, string tipo)
        {
            DadosDia diaAtual = ObterDiaHoje();
            int mlAnterior = diaAtual.TotalMl;

            diaAtual.TotalMl += ml;
            diaAtual.Registros.Insert(0, new RegistroAgua
            {
                Hora = DateTime.Now.ToString("HH:mm"),
                Quantidade = ml,
                Tipo = tipo
            });

            SalvarDados();
            AtualizarInterface();
            DispararFeedbackAgua();
            ExibirSnackBar("+" + ml + " ml adicionados! Muito bem! 💧");

            if (_dados.SomAtivado)
            {
                if (mlAnterior < _dados.MetaDiaria && diaAtual.TotalMl >= _dados.MetaDiaria)
                {
                    GeradorDeSom.TocarCelebracao();
                    if (_dados.NotificacoesAtivadas)
                    {
                        NotificadorWindows.EnviarNotificacao(
                            "Meta Diária Concluída! 🎉💧",
                            "Parabéns! Você bebeu toda a água necessária para hoje!",
                            _trayIcon
                        );
                    }
                    MessageBox.Show("PARABÉNS! 🎉💧\nVocê atingiu sua meta diária de hidratação!", "Meta Batida!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    GeradorDeSom.TocarGota();
                }
            }

            // Reinicia o timer do lembrete
            _segundosRestantes = _dados.IntervaloMinutos * 60;
            AtualizarDisplayTimer();
        }

        private void DesfazerUltimo()
        {
            DadosDia diaAtual = ObterDiaHoje();
            if (diaAtual.Registros.Count > 0)
            {
                RegistroAgua ultimo = diaAtual.Registros[0];
                diaAtual.Registros.RemoveAt(0);
                diaAtual.TotalMl = Math.Max(0, diaAtual.TotalMl - ultimo.Quantidade);

                SalvarDados();
                AtualizarInterface();
                ExibirSnackBar("Registro desfeito com sucesso.");
                if (_dados.SomAtivado) GeradorDeSom.TocarGota();
            }
        }

                        private void AtualizarInterface()
        {
            DadosDia diaAtual = ObterDiaHoje();
            int ml = diaAtual.TotalMl;
            int meta = _dados.MetaDiaria;
            int pct = Math.Min(100, (int)Math.Round(((double)ml / meta) * 100));

            _txtMlAtual.Text = ml.ToString();
            _txtMetaStatus.Text = "Meta di\u00E1ria: " + meta + " ml";
            _txtPorcentagem.Text = pct + "% da meta";
            if (_progressBarMeta != null) _progressBarMeta.Value = pct;

            // Atualiza colunas da barra de progresso moderna
            int pctAtivo = Math.Max(0, Math.Min(100, pct));
            int pctRestante = 100 - pctAtivo;
            if (_colProgressoAtivo != null && _colProgressoRestante != null)
            {
                _colProgressoAtivo.Width = new GridLength(pctAtivo, GridUnitType.Star);
                _colProgressoRestante.Width = new GridLength(pctRestante, GridUnitType.Star);
            }

            int restante = meta - ml;
            if (restante > 0)
            {
                _txtFaltaMl.Text = "\uD83D\uDCA7 Faltam " + restante + " ml para a sua meta di\u00E1ria!";
                _txtFaltaMl.Foreground = new SolidColorBrush(Color.FromRgb(71, 85, 105));
                if (_cardStatusFalta != null)
                {
                    _cardStatusFalta.Background = _dados.ModoEscuro ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromArgb(220, 240, 249, 255));
                    _cardStatusFalta.BorderBrush = _dados.ModoEscuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                }
                if (_badgePorcentagem != null)
                {
                    _badgePorcentagem.Background = _dados.ModoEscuro ? new SolidColorBrush(Color.FromRgb(15, 47, 74)) : new SolidColorBrush(Color.FromRgb(224, 242, 254));
                    _badgePorcentagem.BorderBrush = _dados.ModoEscuro ? new SolidColorBrush(Color.FromRgb(2, 132, 199)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                    _txtPorcentagem.Foreground = _dados.ModoEscuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199));
                }
            }
            else
            {
                _txtFaltaMl.Text = "\uD83C\uDF89 Meta batida com sucesso! Corpo 100% hidratado!";
                _txtFaltaMl.Foreground = new SolidColorBrush(Color.FromRgb(22, 101, 52));
                if (_cardStatusFalta != null)
                {
                    _cardStatusFalta.Background = new SolidColorBrush(Color.FromRgb(240, 253, 244));
                    _cardStatusFalta.BorderBrush = new SolidColorBrush(Color.FromRgb(187, 247, 208));
                }
                if (_badgePorcentagem != null)
                {
                    _badgePorcentagem.Background = new SolidColorBrush(Color.FromRgb(240, 253, 244));
                    _badgePorcentagem.BorderBrush = new SolidColorBrush(Color.FromRgb(187, 247, 208));
                    _txtPorcentagem.Foreground = new SolidColorBrush(Color.FromRgb(22, 101, 52));
                }
            }

            // AnimaÃ§Ã£o suave da altura do copo (copo interno mede ~108px)
            double alturaAgua = (pct / 100.0) * 108.0;
            DoubleAnimation animAltura = new DoubleAnimation(alturaAgua, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            _bordaAguaCopo.BeginAnimation(FrameworkElement.HeightProperty, animAltura);

            // Mascote expressiva
            if (_coroaGotinha != null)
            {
                _coroaGotinha.Visibility = (pct >= 100) ? Visibility.Visible : Visibility.Collapsed;
            }
            if (_pathBocaGotinha != null)
            {
                if (pct >= 100) _pathBocaGotinha.Data = Geometry.Parse("M 32 66 Q 40 78 48 66");
                else if (pct == 0) _pathBocaGotinha.Data = Geometry.Parse("M 36 70 Q 40 68 44 70");
                else if (pct < 50) _pathBocaGotinha.Data = Geometry.Parse("M 35 68 Q 40 73 45 68");
                else _pathBocaGotinha.Data = Geometry.Parse("M 34 67 Q 40 75 46 67");
            }
            if (pct >= 100)
            {
                _txtMensagemGotinha.Text = "PARAB\u00C9NS!! Meta batida com louvor! Voc\u00EA \u00E9 nota 10!";
            }
            else if (pct == 0)
            {
                _txtMensagemGotinha.Text = "Oi! Que tal come\u00E7ar com um belo copo de \u00E1gua?";
            }
            else if (pct < 50)
            {
                _txtMensagemGotinha.Text = "Muito bem! O corpo agradece cada gota!";
            }
            else
            {
                _txtMensagemGotinha.Text = "Mais da metade! Quase l\u00E1 na sua meta di\u00E1ria!";
            }

            AtualizarListaHoje(diaAtual);
        }

        private void AtualizarListaHoje(DadosDia dia)
        {
            _painelHistorico.Children.Clear();
            if (dia.Registros.Count == 0)
            {
                _painelHistorico.Children.Add(new TextBlock
                {
                    Text = "Nenhum gole registrado ainda hoje.\nBeba um copo para come\u00E7ar! \uD83C\uDF31",
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 10, 0, 10)
                });
                return;
            }

            foreach (var r in dia.Registros)
            {
                Border item = new Border
                {
                    Background = _dados.ModoEscuro ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255)),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10, 6, 10, 6),
                    Margin = new Thickness(0, 3, 0, 3),
                    BorderThickness = new Thickness(2, 0, 0, 0),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248))
                };

                Grid g = new Grid();
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                StackPanel left = new StackPanel { Orientation = Orientation.Horizontal };
                left.Children.Add(new TextBlock { Text = "+" + r.Quantidade + " ml", FontWeight = FontWeights.Bold, Foreground = _dados.ModoEscuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199)), FontSize = 12 });
                left.Children.Add(new TextBlock { Text = "  " + r.Tipo, Foreground = _dados.ModoEscuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139)), FontSize = 11 });
                Grid.SetColumn(left, 0);
                g.Children.Add(left);

                TextBlock hora = new TextBlock { Text = r.Hora, Foreground = _dados.ModoEscuro ? new SolidColorBrush(Color.FromRgb(100, 116, 139)) : new SolidColorBrush(Color.FromRgb(148, 163, 184)), FontSize = 11 };
                Grid.SetColumn(hora, 1);
                g.Children.Add(hora);

                item.Child = g;
                _painelHistorico.Children.Add(item);
            }
        }

        private void AtualizarGraficoSemanal()
        {
            _painelGraficoSemanal.Children.Clear();

            UniformGrid colunas = new UniformGrid { Columns = 7, Height = 120, Margin = new Thickness(0, 4, 0, 4)            };
            DateTime hoje = DateTime.Today;
            int maxVal = Math.Max(_dados.MetaDiaria, 2000);

            for (int i = 6; i >= 0; i--)
            {
                DateTime d = hoje.AddDays(-i);
                string dataStr = d.ToString("yyyy-MM-dd");
                DadosDia diaSalvo = _dados.Dias.Find(delegate(DadosDia x) { return x.Data == dataStr; });
                int ml = (diaSalvo != null) ? diaSalvo.TotalMl : 0;

                StackPanel col = new StackPanel { VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Center            };

                col.Children.Add(new TextBlock
                {
                    Text = ml > 0 ? (ml / 1000.0).ToString("0.0") + "L" : "0",
                    FontSize = 9,
                    Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 2)
                });

                double alturaBarra = Math.Min(80.0, (ml / (double)maxVal) * 80.0);
                Border barra = new Border
                {
                    Width = 18,
                    Height = 0,
                    Background = (ml >= _dados.MetaDiaria) ? new SolidColorBrush(Color.FromRgb(16, 185, 129)) : new SolidColorBrush(Color.FromRgb(56, 189, 248)),
                    CornerRadius = new CornerRadius(4, 4, 0, 0)
                           };
                col.Children.Add(barra);

                string nomeDia = (i == 0) ? "Hoje" : d.ToString("ddd");
                col.Children.Add(new TextBlock
                {
                    Text = nomeDia,
                    FontSize = 10,
                    FontWeight = (i == 0) ? FontWeights.Bold : FontWeights.Normal,
                    Foreground = (i == 0) ? new SolidColorBrush(Color.FromRgb(2, 132, 199)) : new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 4, 0, 0)
                });

                DoubleAnimation animBarra = new DoubleAnimation(0, Math.Max(4, alturaBarra), TimeSpan.FromMilliseconds(420))
                {
                    BeginTime = TimeSpan.FromMilliseconds((6 - i) * 45),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                           };
                barra.BeginAnimation(FrameworkElement.HeightProperty, animBarra);
                colunas.Children.Add(col);
            }

            _painelGraficoSemanal.Children.Add(colunas);
        }

        // ==========================================
        // Timer e Lembretes Nativos do Windows
        // ==========================================
        private void IniciarTimers()
        {
            _segundosRestantes = _dados.IntervaloMinutos * 60;

            _timerLembrete = new DispatcherTimer();
            _timerLembrete.Interval = TimeSpan.FromSeconds(1);
            _timerLembrete.Tick += delegate
            {
                if (_timerPausado) return;

                if (_segundosRestantes > 0)
                {
                    _segundosRestantes--;
                    AtualizarDisplayTimer();
                }
                else
                {
                    DispararLembrete();
                    _segundosRestantes = _dados.IntervaloMinutos * 60;
                    AtualizarDisplayTimer();
                }
                       };
            _timerLembrete.Start();

            // Piscar suave dos olhos do mascote a cada 4 segundos
            _timerAnimacaoMascote = new DispatcherTimer();
            _timerAnimacaoMascote.Interval = TimeSpan.FromSeconds(4);
            _timerAnimacaoMascote.Tick += delegate
            {
                if (_olhoEsquerdo != null && _olhoDireito != null)
                {
                    _olhoEsquerdo.Height = 1;
                    _olhoDireito.Height = 1;

                    DispatcherTimer tVolta = new DispatcherTimer();
                    tVolta.Interval = TimeSpan.FromMilliseconds(160);
                    tVolta.Tick += delegate(object sender, EventArgs e)
                    {
                        tVolta.Stop();
                        if (_olhoEsquerdo != null && _olhoDireito != null)
                        {
                            _olhoEsquerdo.Height = 8;
                            _olhoDireito.Height = 8;
                        }
                               };
                    tVolta.Start();
                }
                       };
            _timerAnimacaoMascote.Start();
        }

        private void AtualizarDisplayTimer()
        {
            int min = _segundosRestantes / 60;
            int seg = _segundosRestantes % 60;
            _txtTimerDisplay.Text = string.Format("{0:D2}:{1:D2}", min, seg);
        }

        private void AlternarPausaTimer()
        {
            _timerPausado = !_timerPausado;
            _btnPausarTimer.Content = _timerPausado ? "▶️ Retomar" : "⏸️ Pausar";
            ExibirSnackBar(_timerPausado ? "Lembretes pausados." : "Lembretes retomados.");
        }

        private void DefinirIntervalo(int minutos)
        {
            _dados.IntervaloMinutos = minutos;
            _segundosRestantes = minutos * 60;
            SalvarDados();
            AtualizarDisplayTimer();

            foreach (var b in _botoesIntervalo)
            {
                int m = (int)b.Tag;
                b.Background = (m == _dados.IntervaloMinutos) ? new SolidColorBrush(Color.FromRgb(2, 132, 199)) : new SolidColorBrush(Color.FromRgb(240, 249, 255));
                b.Foreground = (m == _dados.IntervaloMinutos) ? Brushes.White : new SolidColorBrush(Color.FromRgb(3, 105, 161));
            }
            ExibirSnackBar("Intervalo definido para " + minutos + " minutos!");
        }

        public void DispararLembrete()
        {
            if (_dados.SomAtivado)
            {
                GeradorDeSom.TocarSinoLembrete();
            }

            // Exibir Notificação Nativa do Windows (na Central de Ações e na Bandeja)
            if (_dados.NotificacoesAtivadas)
            {
                NotificadorWindows.EnviarNotificacao(
                    "Gotinha - Hora de Beber Água! 💧",
                    "Faça uma pausa rápida para um gole d'água agora mesmo e cuide do seu corpo!",
                    _trayIcon
                );
            }

            // Exibir popup modal interativa se a janela estiver visível
            if (WindowState != WindowState.Minimized && IsVisible)
            {
                LembreteAlertaWindow alerta = new LembreteAlertaWindow(this);
                alerta.Owner = this;
                alerta.ShowDialog();
            }
        }

        // ==========================================
        // Bandeja do Sistema (System Tray)
        // ==========================================
        private void ConfigurarBandejaSistema()
        {
            _trayIcon = new Forms.NotifyIcon();
            _trayIcon.Text = "Gotinha - Lembrete de Água 💧";

            string icoPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
            if (File.Exists(icoPath))
            {
                try
                {
                    _trayIcon.Icon = new Drawing.Icon(icoPath);
                }
                catch
                {
                    _trayIcon.Icon = Drawing.SystemIcons.Application;
                }
            }
            else
            {
                _trayIcon.Icon = Drawing.SystemIcons.Application;
            }

            _trayIcon.Visible = true;

            Forms.ContextMenuStrip menu = new Forms.ContextMenuStrip();
            menu.Items.Add("Abrir Gotinha 💧", null, delegate { Show(); WindowState = WindowState.Normal; Activate(); });
            menu.Items.Add("Beber Copo (+250ml)", null, delegate { AdicionarAgua(250, "Copo (🥛)"); });
            menu.Items.Add("Testar Notificação Nativa", null, delegate { DispararLembrete(); });
            menu.Items.Add("-");
            menu.Items.Add("Sair", null, delegate { _trayIcon.Visible = false; Application.Current.Shutdown(); });

            _trayIcon.ContextMenuStrip = menu;
            _trayIcon.DoubleClick += delegate { Show(); WindowState = WindowState.Normal; Activate();            };
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_timerLembrete != null)
            {
                _timerLembrete.Stop();
                _timerLembrete = null;
            }
            if (_timerAnimacaoMascote != null)
            {
                _timerAnimacaoMascote.Stop();
                _timerAnimacaoMascote = null;
            }

            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayIcon = null;
            }
            base.OnClosed(e);
        }

                private void InteragirComGotinha()
        {
            string[] falas = new string[] {
                "Hihi! N\u00E3o esque\u00E7a de beber mais um golinho! \uD83D\uDCA7",
                "Estou aqui para cuidar da sua sa\u00FAde enquanto voc\u00EA usa o PC! \uD83E\uDD70",
                "Pele bonita, foco afiado e mente leve precisam de \u00E1gua! \u2728",
                "Voc\u00EA \u00E9 incr\u00EDvel! Vamos bater essa meta juntas! \uD83D\uDC96"
            };
            Random rnd = new Random();
            _txtMensagemGotinha.Text = falas[rnd.Next(falas.Length)];
            if (_dados.SomAtivado) GeradorDeSom.TocarGota();
            ExibirSnackBar("Gotinha mandou um abra\u00E7o carinhoso! \uD83D\uDCA7");
        }

                private void AtualizarVisualBtnSom()
        {
            if (_btnSom != null)
            {
                _btnSom.Content = CriarIconeSom(_dados.SomAtivado);
                _btnSom.ToolTip = _dados.SomAtivado ? "Som ativado (clique para silenciar)" : "Som silenciado (clique para ativar)";
            }
        }

                private void AtualizarVisualBtnNotif()
        {
            if (_btnNotif != null)
            {
                _btnNotif.Content = CriarIconeNotif(_dados.NotificacoesAtivadas);
                _btnNotif.ToolTip = _dados.NotificacoesAtivadas ? "Notifica\u00E7\u00F5es ativadas (clique para pausar)" : "Notifica\u00E7\u00F5es desativadas (clique para ativar)";
            }
        }

        private void AlternarSom()
        {
            _dados.SomAtivado = !_dados.SomAtivado;
            AtualizarVisualBtnSom();
            SalvarDados();
            if (_dados.SomAtivado) GeradorDeSom.TocarGota();
            ExibirSnackBar(_dados.SomAtivado ? "Som ativado." : "Som silenciado.");
        }

        private void AlternarNotificacoes()
        {
            _dados.NotificacoesAtivadas = !_dados.NotificacoesAtivadas;
            AtualizarVisualBtnNotif();
            SalvarDados();
            ExibirSnackBar(_dados.NotificacoesAtivadas ? "Notifica\u00E7\u00F5es do Windows ativadas!" : "Notifica\u00E7\u00F5es desativadas.");
        }

        private void AbrirJanelaConfiguracoes()
        {
            ConfigWindow cfg = new ConfigWindow(_dados, delegate
            {
                SalvarDados();
                AtualizarInterface();
                AtualizarVisualBtnSom();
                AtualizarVisualBtnNotif();
            });
            cfg.Owner = this;
            cfg.ShowDialog();
        }

        // ==========================================
        // Persistência em JSON e Estados
        // ==========================================
        private DadosDia ObterDiaHoje()
        {
            string hoje = DateTime.Today.ToString("yyyy-MM-dd");
            DadosDia dia = _dados.Dias.Find(delegate(DadosDia x) { return x.Data == hoje; });
            if (dia == null)
            {
                dia = new DadosDia { Data = hoje, TotalMl = 0            };
                _dados.Dias.Add(dia);
            }
            return dia;
        }

        private void CarregarDados()
        {
            try
            {
                if (File.Exists(_caminhoArquivo))
                {
                    string json = File.ReadAllText(_caminhoArquivo, Encoding.UTF8);
                    _dados = SimpleJson.Deserialize(json);
                }
            }
            catch (Exception ex)
            {
                try { File.AppendAllText("error.log", string.Format("[{0}] Erro ao carregar dados: {1}\r\n", DateTime.Now, ex)); } catch { }
            }

            if (_dados == null)
            {
                _dados = new HistoricoApp();
            }
        }

        private void SalvarDados()
        {
            try
            {
                string json = SimpleJson.Serialize(_dados);
                File.WriteAllText(_caminhoArquivo, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                try { File.AppendAllText("error.log", string.Format("[{0}] Erro ao salvar dados: {1}\r\n", DateTime.Now, ex)); } catch { }
            }
        }

        // ==========================================
        // Helpers de Construção de UI
        // ==========================================
                private Border CriarCardBase()
        {
            LinearGradientBrush bgBrush = new LinearGradientBrush();
            bgBrush.StartPoint = new Point(0, 0);
            bgBrush.EndPoint = new Point(0, 1);
            bgBrush.GradientStops.Add(new GradientStop(Color.FromArgb(250, 255, 255, 255), 0.0));
            bgBrush.GradientStops.Add(new GradientStop(Color.FromArgb(242, 248, 252, 255), 1.0));

            LinearGradientBrush borderBrush = new LinearGradientBrush();
            borderBrush.StartPoint = new Point(0, 0);
            borderBrush.EndPoint = new Point(1, 1);
            borderBrush.GradientStops.Add(new GradientStop(Color.FromArgb(240, 255, 255, 255), 0.0));
            borderBrush.GradientStops.Add(new GradientStop(Color.FromArgb(170, 186, 230, 253), 1.0));

            Border card = new Border
            {
                Background = bgBrush,
                CornerRadius = new CornerRadius(18),
                BorderBrush = borderBrush,
                BorderThickness = new Thickness(1.2),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 0, 10),
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(2, 132, 199),
                    Opacity = 0.08,
                    BlurRadius = 18,
                    ShadowDepth = 3
                }
            };
            _cardsLista.Add(card);
            return card;
        }

                        private static UIElement CriarIconeMinimizar(Brush cor)
        {
            Canvas c = new Canvas { Width = 10, Height = 10 };
            Line l = new Line { X1 = 0, Y1 = 5, X2 = 10, Y2 = 5, Stroke = cor, StrokeThickness = 1.3 };
            c.Children.Add(l);
            return c;
        }

        private static UIElement CriarIconeMaximizar(Brush cor, bool maximizado)
        {
            if (maximizado)
            {
                Canvas c = new Canvas { Width = 10, Height = 10 };
                System.Windows.Shapes.Rectangle rBack = new System.Windows.Shapes.Rectangle
                {
                    Width = 7, Height = 7, Stroke = cor, StrokeThickness = 1.1, Fill = Brushes.Transparent
                };
                Canvas.SetLeft(rBack, 3);
                Canvas.SetTop(rBack, 0);
                c.Children.Add(rBack);

                System.Windows.Shapes.Rectangle rFront = new System.Windows.Shapes.Rectangle
                {
                    Width = 7, Height = 7, Stroke = cor, StrokeThickness = 1.1, Fill = Brushes.White
                };
                Canvas.SetLeft(rFront, 0);
                Canvas.SetTop(rFront, 3);
                c.Children.Add(rFront);
                return c;
            }
            else
            {
                System.Windows.Shapes.Rectangle r = new System.Windows.Shapes.Rectangle
                {
                    Width = 9.5, Height = 9.5, Stroke = cor, StrokeThickness = 1.2, Fill = Brushes.Transparent
                };
                return r;
            }
        }

        private static UIElement CriarIconeFechar(Brush cor)
        {
            Canvas c = new Canvas { Width = 10, Height = 10 };
            Line l1 = new Line { X1 = 0, Y1 = 0, X2 = 10, Y2 = 10, Stroke = cor, StrokeThickness = 1.3 };
            Line l2 = new Line { X1 = 10, Y1 = 0, X2 = 0, Y2 = 10, Stroke = cor, StrokeThickness = 1.3 };
            c.Children.Add(l1);
            c.Children.Add(l2);
            return c;
        }

        private static UIElement CriarIconeEngrenagem()
        {
            Viewbox vb = new Viewbox { Width = 17, Height = 17, Stretch = Stretch.Uniform };
            System.Windows.Shapes.Path p = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse("M 12,15 C 10.34,15 9,13.66 9,12 C 9,10.34 10.34,9 12,9 C 13.66,9 15,10.34 15,12 C 15,13.66 13.66,15 12,15 Z M 19.4,13 C 19.45,12.67 19.5,12.34 19.5,12 C 19.5,11.66 19.45,11.33 19.4,11 L 21.54,9.33 C 21.73,9.18 21.78,8.91 21.66,8.69 L 19.66,5.23 C 19.54,5.01 19.28,4.93 19.05,5.02 L 16.53,6.03 C 16,5.62 15.43,5.28 14.8,5.03 L 14.42,2.35 C 14.39,2.11 14.18,1.93 13.94,1.93 L 9.94,1.93 C 9.7,1.93 9.5,2.11 9.46,2.35 L 9.08,5.03 C 8.45,5.28 7.88,5.62 7.35,6.03 L 4.83,5.02 C 4.6,4.93 4.34,5.01 4.22,5.23 L 2.22,8.69 C 2.1,8.91 2.15,9.18 2.34,9.33 L 4.48,11 C 4.43,11.33 4.38,11.67 4.38,12 C 4.38,12.33 4.43,12.67 4.48,13 L 2.34,14.67 C 2.15,14.82 2.1,15.09 2.22,15.31 L 4.22,18.77 C 4.34,18.99 4.6,19.07 4.83,18.98 L 7.35,17.97 C 7.88,18.38 8.45,18.72 9.08,18.97 L 9.46,21.65 C 9.5,21.89 9.7,22.07 9.94,22.07 L 13.94,22.07 C 14.18,22.07 14.38,21.89 14.42,21.65 L 14.8,18.97 C 15.43,18.72 16,18.38 16.53,17.97 L 19.05,18.98 C 19.28,19.07 19.54,18.99 19.66,18.77 L 21.66,15.31 C 21.78,15.09 21.73,14.82 21.54,14.67 L 19.4,13 Z"),
                Fill = new SolidColorBrush(Color.FromRgb(71, 85, 105))
            };
            vb.Child = p;
            return vb;
        }

        private static UIElement CriarIconeSom(bool ativado)
        {
            Viewbox vb = new Viewbox { Width = 17, Height = 17, Stretch = Stretch.Uniform };
            System.Windows.Shapes.Path p = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse(ativado
                    ? "M 3,9 L 7,9 L 12,4 L 12,20 L 7,15 L 3,15 Z M 16.5,12 C 16.5,10.23 15.48,8.71 14,7.97 L 14,16.02 C 15.48,15.29 16.5,13.77 16.5,12 Z M 14,3.23 L 14,5.29 C 16.89,6.15 19,8.83 19,12 C 19,15.17 16.89,17.85 14,18.71 L 14,20.77 C 18.01,19.86 21,16.28 21,12 C 21,7.72 18.01,4.14 14,3.23 Z"
                    : "M 4.27,3 L 3,4.27 L 7.73,9 L 3,9 L 3,15 L 7,15 L 12,20 L 12,13.27 L 16.25,17.53 C 15.58,18.04 14.83,18.46 14,18.7 L 14,20.77 C 15.38,20.45 16.63,19.82 17.68,18.96 L 19.73,21 L 21,19.73 L 4.27,3 Z M 12,4 L 9.91,6.09 L 12,8.18 L 12,4 Z M 19,12 C 19,12.82 18.84,13.61 18.55,14.33 L 20.08,15.86 C 20.67,14.69 21,13.39 21,12 C 21,7.72 18.01,4.14 14,3.23 L 14,5.29 C 16.89,6.15 19,8.83 19,12 Z M 16.5,12 C 16.5,10.23 15.48,8.71 14,7.97 L 14,10.18 L 16.45,12.63 C 16.48,12.43 16.5,12.22 16.5,12 Z"),
                Fill = ativado ? new SolidColorBrush(Color.FromRgb(2, 132, 199)) : new SolidColorBrush(Color.FromRgb(148, 163, 184))
            };
            vb.Child = p;
            return vb;
        }

        private static UIElement CriarIconeNotif(bool ativado)
        {
            Viewbox vb = new Viewbox { Width = 17, Height = 17, Stretch = Stretch.Uniform };
            System.Windows.Shapes.Path p = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse(ativado
                    ? "M 12,22 C 13.1,22 14,21.1 14,20 L 10,20 C 10,21.1 10.89,22 12,22 Z M 18,16 L 18,11 C 18,7.93 16.36,5.36 13.5,4.68 L 13.5,4 C 13.5,3.17 12.83,2.5 12,2.5 C 11.17,2.5 10.5,3.17 10.5,4 L 10.5,4.68 C 7.63,5.36 6,7.92 6,11 L 6,16 L 4,18 L 4,19 L 20,19 L 20,18 L 18,16 Z"
                    : "M 20,18.69 L 5.31,4 L 4,5.31 L 6.27,7.58 C 6.1,8.65 6,9.8 6,11 L 6,16 L 4,18 L 4,19 L 17.69,19 L 18.69,20 L 20,18.69 Z M 12,22 C 13.1,22 14,21.1 14,20 L 10,20 C 10,21.1 10.89,22 12,22 Z M 18,16 L 18,11 C 18,8.6 16.94,6.54 15,5.35 L 15,4 C 15,2.34 13.66,1 12,1 C 11.39,1 10.83,1.18 10.36,1.49 L 18,9.13 L 18,16 Z"),
                Fill = ativado ? new SolidColorBrush(Color.FromRgb(2, 132, 199)) : new SolidColorBrush(Color.FromRgb(148, 163, 184))
            };
            vb.Child = p;
            return vb;
        }

        
        private static UIElement CriarIconeLua(Brush cor)
        {
            Viewbox vb = new Viewbox { Width = 17, Height = 17, Stretch = Stretch.Uniform };
            System.Windows.Shapes.Path p = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse("M 12.3,2 C 12.2,2 12.1,2 12,2 C 6.5,2 2,6.5 2,12 C 2,17.5 6.5,22 12,22 C 17.5,22 22,17.5 22,12 C 22,11.9 22,11.8 22,11.7 C 21.05,12.53 19.83,13 18.5,13 C 14.91,13 12,10.09 12,6.5 C 12,5.17 12.47,3.95 13.3,3 C 13,2.4 12.6,2 12.3,2 Z"),
                Fill = cor
            };
            vb.Child = p;
            return vb;
        }

        private static UIElement CriarIconeSol(Brush cor)
        {
            Viewbox vb = new Viewbox { Width = 17, Height = 17, Stretch = Stretch.Uniform };
            System.Windows.Shapes.Path p = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse("M 12,7 C 9.24,7 7,9.24 7,12 C 7,14.76 9.24,17 12,17 C 14.76,17 17,14.76 17,12 C 17,9.24 14.76,7 12,7 Z M 12,2 C 11.45,2 11,2.45 11,3 L 11,5 C 11,5.55 11.45,6 12,6 C 12.55,6 13,5.55 13,5 L 13,3 C 13,2.45 12.55,2 12,2 Z M 12,18 C 11.45,18 11,18.45 11,19 L 11,21 C 11,21.55 11.45,22 12,22 C 12.55,22 13,21.55 13,21 L 13,19 C 13,18.45 12.55,18 12,18 Z M 2,12 C 2,11.45 2.45,11 3,11 L 5,11 C 5.55,11 6,11.45 6,12 C 6,12.55 5.55,13 5,13 L 3,13 C 2.45,13 2,12.55 2,12 Z M 18,12 C 18,11.45 18.45,11 19,11 L 21,11 C 21.55,11 22,11.45 22,12 C 22,12.55 21.55,13 21,13 L 19,13 C 18.45,13 18,12.55 18,12 Z M 4.93,4.93 C 4.54,5.32 4.54,5.95 4.93,6.34 L 6.34,7.75 C 6.73,8.14 7.36,8.14 7.75,7.75 C 8.14,7.36 8.14,6.73 7.75,6.34 L 6.34,4.93 C 5.95,4.54 5.32,4.54 4.93,4.93 Z M 16.25,16.25 C 15.86,16.64 15.86,17.27 16.25,17.66 L 17.66,19.07 C 18.05,19.46 18.68,19.46 19.07,19.07 C 19.46,18.68 19.46,18.05 19.07,17.66 L 17.66,16.25 C 17.27,15.86 16.64,15.86 16.25,16.25 Z M 19.07,4.93 C 18.68,4.54 18.05,4.54 17.66,4.93 L 16.25,6.34 C 15.86,6.73 15.86,7.36 16.25,7.75 C 16.64,8.14 17.27,8.14 17.66,7.75 L 19.07,6.34 C 19.46,5.95 19.46,5.32 19.07,4.93 Z M 7.75,16.25 C 7.36,15.86 6.73,15.86 6.34,16.25 L 4.93,17.66 C 4.54,18.05 4.54,18.68 4.93,19.07 C 5.32,19.46 5.95,19.46 6.34,19.07 L 7.75,17.66 C 8.14,17.27 8.14,16.64 7.75,16.25 Z"),
                Fill = cor
            };
            vb.Child = p;
            return vb;
        }

        private static UIElement CriarIconeTema(bool escuro)
        {
            return escuro
                ? CriarIconeSol(new SolidColorBrush(Color.FromRgb(251, 191, 36)))
                : CriarIconeLua(new SolidColorBrush(Color.FromRgb(2, 132, 199)));
        }

        private static UIElement CriarIconeEngrenagem(Brush cor)
        {
            Viewbox vb = new Viewbox { Width = 17, Height = 17, Stretch = Stretch.Uniform };
            System.Windows.Shapes.Path p = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse("M 12,15 C 10.34,15 9,13.66 9,12 C 9,10.34 10.34,9 12,9 C 13.66,9 15,10.34 15,12 C 15,13.66 13.66,15 12,15 Z M 19.4,13 C 19.45,12.67 19.5,12.34 19.5,12 C 19.5,11.66 19.45,11.33 19.4,11 L 21.54,9.33 C 21.73,9.18 21.78,8.91 21.66,8.69 L 19.66,5.23 C 19.54,5.01 19.28,4.93 19.05,5.02 L 16.53,6.03 C 16,5.62 15.43,5.28 14.8,5.03 L 14.42,2.35 C 14.39,2.11 14.18,1.93 13.94,1.93 L 9.94,1.93 C 9.7,1.93 9.5,2.11 9.46,2.35 L 9.08,5.03 C 8.45,5.28 7.88,5.62 7.35,6.03 L 4.83,5.02 C 4.6,4.93 4.34,5.01 4.22,5.23 L 2.22,8.69 C 2.1,8.91 2.15,9.18 2.34,9.33 L 4.48,11 C 4.43,11.33 4.38,11.67 4.38,12 C 4.38,12.33 4.43,12.67 4.48,13 L 2.34,14.67 C 2.15,14.82 2.1,15.09 2.22,15.31 L 4.22,18.77 C 4.34,18.99 4.6,19.07 4.83,18.98 L 7.35,17.97 C 7.88,18.38 8.45,18.72 9.08,18.97 L 9.46,21.65 C 9.5,21.89 9.7,22.07 9.94,22.07 L 13.94,22.07 C 14.18,22.07 14.38,21.89 14.42,21.65 L 14.8,18.97 C 15.43,18.72 16,18.38 16.53,17.97 L 19.05,18.98 C 19.28,19.07 19.54,18.99 19.66,18.77 L 21.66,15.31 C 21.78,15.09 21.73,14.82 21.54,14.67 L 19.4,13 Z"),
                Fill = cor
            };
            vb.Child = p;
            return vb;
        }

        private void AlternarModoEscuro()
        {
            _dados.ModoEscuro = !_dados.ModoEscuro;
            SalvarDados();
            AplicarTema();
            AtualizarInterface();
            ExibirSnackBar(_dados.ModoEscuro ? "Modo Oceano Noturno ativado! \uD83C\uDF19" : "Modo Brisa Aqu\u00E1tica ativado! \u2600\uFE0F");
        }

        private void AtualizarVisualBtnTema()
        {
            if (_btnTema != null)
            {
                bool escuro = _dados.ModoEscuro;
                _btnTema.ToolTip = escuro ? "Ativar Modo Claro (Brisa Aqu\u00E1tica)" : "Ativar Modo Escuro (Oceano Noturno)";
                _btnTema.Content = CriarIconeTema(escuro);
            }
        }

        private void AplicarTema()
        {
            bool escuro = (_dados != null && _dados.ModoEscuro);

            // 1. Fundo e contorno da janela
            if (_janelaBorda != null)
            {
                if (escuro)
                {
                    LinearGradientBrush janelaBg = new LinearGradientBrush();
                    janelaBg.StartPoint = new Point(0, 0);
                    janelaBg.EndPoint = new Point(1, 1);
                    janelaBg.GradientStops.Add(new GradientStop(Color.FromRgb(10, 17, 32), 0.0));
                    janelaBg.GradientStops.Add(new GradientStop(Color.FromRgb(15, 23, 42), 0.5));
                    janelaBg.GradientStops.Add(new GradientStop(Color.FromRgb(11, 19, 43), 1.0));

                    _janelaBorda.Background = janelaBg;
                    _janelaBorda.BorderBrush = new SolidColorBrush(Color.FromArgb(140, 56, 189, 248));
                    _janelaBorda.Effect = new DropShadowEffect
                    {
                        BlurRadius = 40,
                        ShadowDepth = 6,
                        Opacity = 0.35,
                        Color = Color.FromRgb(2, 132, 199)
                    };
                }
                else
                {
                    LinearGradientBrush janelaBg = new LinearGradientBrush();
                    janelaBg.StartPoint = new Point(0, 0);
                    janelaBg.EndPoint = new Point(1, 1);
                    janelaBg.GradientStops.Add(new GradientStop(Color.FromRgb(224, 242, 254), 0.0));
                    janelaBg.GradientStops.Add(new GradientStop(Color.FromRgb(240, 249, 255), 0.45));
                    janelaBg.GradientStops.Add(new GradientStop(Color.FromRgb(229, 246, 254), 1.0));

                    LinearGradientBrush janelaBordaBrush = new LinearGradientBrush();
                    janelaBordaBrush.StartPoint = new Point(0, 0);
                    janelaBordaBrush.EndPoint = new Point(1, 1);
                    janelaBordaBrush.GradientStops.Add(new GradientStop(Color.FromArgb(240, 255, 255, 255), 0.0));
                    janelaBordaBrush.GradientStops.Add(new GradientStop(Color.FromArgb(180, 186, 230, 253), 1.0));

                    _janelaBorda.Background = janelaBg;
                    _janelaBorda.BorderBrush = janelaBordaBrush;
                    _janelaBorda.Effect = new DropShadowEffect
                    {
                        BlurRadius = 36,
                        ShadowDepth = 6,
                        Opacity = 0.25,
                        Color = Color.FromRgb(2, 132, 199)
                    };
                }
            }

            // 2. Os 5 Cards principais
            foreach (Border card in _cardsLista)
            {
                if (escuro)
                {
                    LinearGradientBrush bgCard = new LinearGradientBrush();
                    bgCard.StartPoint = new Point(0, 0);
                    bgCard.EndPoint = new Point(0, 1);
                    bgCard.GradientStops.Add(new GradientStop(Color.FromRgb(23, 32, 54), 0.0));
                    bgCard.GradientStops.Add(new GradientStop(Color.FromRgb(15, 23, 42), 1.0));

                    card.Background = bgCard;
                    card.BorderBrush = new SolidColorBrush(Color.FromArgb(90, 56, 189, 248));
                    card.BorderThickness = new Thickness(1.2);
                    card.Effect = new DropShadowEffect
                    {
                        Color = Color.FromRgb(0, 0, 0),
                        Opacity = 0.35,
                        BlurRadius = 16,
                        ShadowDepth = 4
                    };
                }
                else
                {
                    LinearGradientBrush bgCard = new LinearGradientBrush();
                    bgCard.StartPoint = new Point(0, 0);
                    bgCard.EndPoint = new Point(0, 1);
                    bgCard.GradientStops.Add(new GradientStop(Color.FromArgb(250, 255, 255, 255), 0.0));
                    bgCard.GradientStops.Add(new GradientStop(Color.FromArgb(242, 248, 252, 255), 1.0));

                    LinearGradientBrush borderCard = new LinearGradientBrush();
                    borderCard.StartPoint = new Point(0, 0);
                    borderCard.EndPoint = new Point(1, 1);
                    borderCard.GradientStops.Add(new GradientStop(Color.FromArgb(240, 255, 255, 255), 0.0));
                    borderCard.GradientStops.Add(new GradientStop(Color.FromArgb(170, 186, 230, 253), 1.0));

                    card.Background = bgCard;
                    card.BorderBrush = borderCard;
                    card.BorderThickness = new Thickness(1.2);
                    card.Effect = new DropShadowEffect
                    {
                        Color = Color.FromRgb(2, 132, 199),
                        Opacity = 0.08,
                        BlurRadius = 18,
                        ShadowDepth = 3
                    };
                }
            }

            // 3. Barra de Título
            Brush corTitulo = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(3, 105, 161));
            Brush corSub = escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(71, 85, 105));
            if (_txtBrandTitle != null) _txtBrandTitle.Foreground = corTitulo;
            if (_txtBrandSub != null) _txtBrandSub.Foreground = corSub;
            if (_btnMin != null) _btnMin.Content = CriarIconeMinimizar(corTitulo);
            if (_btnMax != null) _btnMax.Content = CriarIconeMaximizar(corTitulo, WindowState == WindowState.Maximized);
            if (_btnClose != null) _btnClose.Content = CriarIconeFechar(corTitulo);

            // 4. Card Header (Mascote, PRO, Toolbar, Balão)
            if (_txtHeaderTitle != null) _txtHeaderTitle.Foreground = corTitulo;
            if (_speechBubble != null)
            {
                _speechBubble.Background = escuro ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromArgb(235, 240, 249, 255));
                _speechBubble.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
            }
            if (_txtMensagemGotinha != null)
            {
                _txtMensagemGotinha.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(226, 232, 240)) : new SolidColorBrush(Color.FromRgb(3, 105, 161));
            }

            Brush bgBtn = escuro ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255));
            Brush borderBtn = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));

            if (_btnSom != null) { _btnSom.Background = bgBtn; _btnSom.BorderBrush = borderBtn; _btnSom.Content = CriarIconeSom(_dados.SomAtivado); }
            if (_btnNotif != null) { _btnNotif.Background = bgBtn; _btnNotif.BorderBrush = borderBtn; _btnNotif.Content = CriarIconeNotif(_dados.NotificacoesAtivadas); }
            if (_btnConfig != null) { _btnConfig.Background = bgBtn; _btnConfig.BorderBrush = borderBtn; _btnConfig.Content = CriarIconeEngrenagem(escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(71, 85, 105))); }
            AtualizarVisualBtnTema();

            // 5. Card Progresso
            if (_txtMlAtual != null) _txtMlAtual.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199));
            if (_txtMlUnit != null) _txtMlUnit.Foreground = corSub;
            if (_txtMetaStatus != null) _txtMetaStatus.Foreground = corSub;
            if (_barTrack != null)
            {
                _barTrack.Background = escuro ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(224, 242, 254));
                _barTrack.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
            }
            if (_copoContainer != null)
            {
                _copoContainer.Background = escuro ? new SolidColorBrush(Color.FromArgb(100, 10, 17, 32)) : new SolidColorBrush(Color.FromArgb(150, 240, 249, 255));
                _copoContainer.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(56, 189, 248));
            }

            // 6. Card Lembrete
            if (_txtTimerLabel != null) _txtTimerLabel.Foreground = corSub;
            if (_txtTimerDisplay != null) _txtTimerDisplay.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(248, 250, 252)) : new SolidColorBrush(Color.FromRgb(15, 23, 42));
            if (_txtLembrarACada != null) _txtLembrarACada.Foreground = corSub;
            if (_btnTestarLembrete != null)
            {
                _btnTestarLembrete.Background = escuro ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255));
                _btnTestarLembrete.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                _btnTestarLembrete.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(3, 105, 161));
            }
            foreach (var b in _botoesIntervalo)
            {
                int m = (int)b.Tag;
                bool ativo = (m == _dados.IntervaloMinutos);
                if (ativo)
                {
                    b.Background = new SolidColorBrush(Color.FromRgb(2, 132, 199));
                    b.Foreground = Brushes.White;
                    b.BorderBrush = Brushes.Transparent;
                }
                else
                {
                    b.Background = escuro ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255));
                    b.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(3, 105, 161));
                    b.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                }
            }

            // 7. Card Registrar Água
            if (_txtTituloRegistrar != null) _txtTituloRegistrar.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(15, 23, 42));
            if (_btnDesfazer != null) _btnDesfazer.Foreground = corSub;
            foreach (var btnBebida in _botoesBebida)
            {
                bool dest = (btnBebida.Tag is bool) ? (bool)btnBebida.Tag : false;
                if (escuro)
                {
                    btnBebida.Background = dest ? new SolidColorBrush(Color.FromRgb(12, 74, 110)) : new SolidColorBrush(Color.FromRgb(23, 32, 54));
                    btnBebida.BorderBrush = dest ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(51, 65, 85));
                }
                else
                {
                    btnBebida.Background = dest ? new SolidColorBrush(Color.FromRgb(240, 249, 255)) : Brushes.White;
                    btnBebida.BorderBrush = dest ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                }
                if (btnBebida.Content is StackPanel)
                {
                    StackPanel spContent = (StackPanel)btnBebida.Content;
                    if (spContent.Children.Count >= 3)
                    {
                        if (spContent.Children[1] is TextBlock)
                        {
                            ((TextBlock)spContent.Children[1]).Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(15, 23, 42));
                        }
                        if (spContent.Children[2] is TextBlock)
                        {
                            ((TextBlock)spContent.Children[2]).Foreground = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199));
                        }
                    }
                }
            }

            if (_txtCustomAgua != null)
            {
                _txtCustomAgua.Background = escuro ? new SolidColorBrush(Color.FromRgb(15, 23, 42)) : Brushes.White;
                _txtCustomAgua.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(15, 23, 42));
                _txtCustomAgua.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(203, 213, 225));
            }

            // 8. Card Histórico
            if (_tabHistorico != null)
            {
                foreach (var item in _tabHistorico.Items)
                {
                    if (item is TabItem)
                    {
                        ((TabItem)item).Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(15, 23, 42));
                    }
                }
            }
        }

        private Button CriarBotaoToolbarCard(UIElement icone, string tooltip, RoutedEventHandler handler, bool isGear = false)
        {
            Button btn = new Button
            {
                Content = icone,
                ToolTip = tooltip,
                Width = 34,
                Height = 34,
                Margin = new Thickness(3, 0, 3, 0),
                Background = new SolidColorBrush(Color.FromRgb(240, 249, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                BorderThickness = new Thickness(1),
                FocusVisualStyle = null,
                Cursor = Cursors.Hand
            };
            TransformGroup tg = new TransformGroup();
            ScaleTransform st = new ScaleTransform(1.0, 1.0);
            RotateTransform rt = new RotateTransform(0);
            tg.Children.Add(st);
            tg.Children.Add(rt);
            btn.RenderTransformOrigin = new Point(0.5, 0.5);
            btn.RenderTransform = tg;
            btn.MouseEnter += delegate
            {
                btn.Background = new SolidColorBrush(Color.FromRgb(224, 242, 254));
                btn.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                st.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.0, 1.08, TimeSpan.FromMilliseconds(150)));
                st.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.0, 1.08, TimeSpan.FromMilliseconds(150)));
                if (isGear) rt.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(0, 45, TimeSpan.FromMilliseconds(220)));
            };
            btn.MouseLeave += delegate
            {
                btn.Background = new SolidColorBrush(Color.FromRgb(240, 249, 255));
                btn.BorderBrush = new SolidColorBrush(Color.FromRgb(186, 230, 253));
                st.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.08, 1.0, TimeSpan.FromMilliseconds(150)));
                st.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.08, 1.0, TimeSpan.FromMilliseconds(150)));
                if (isGear) rt.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(45, 0, TimeSpan.FromMilliseconds(220)));
            };
            btn.Click += handler;
            return btn;
        }

        private Button CriarBotaoPill(string texto, bool secondary, RoutedEventHandler handler)
        {
            Button btn = new Button
            {
                Content = texto,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = secondary ? new SolidColorBrush(Color.FromRgb(3, 105, 161)) : Brushes.White,
                Background = secondary ? new SolidColorBrush(Color.FromRgb(240, 249, 255)) : new SolidColorBrush(Color.FromRgb(2, 132, 199)),
                Padding = new Thickness(10, 5, 10, 5),
                Margin = new Thickness(4, 0, 0, 0),
                BorderBrush = secondary ? new SolidColorBrush(Color.FromRgb(186, 230, 253)) : Brushes.Transparent,
                BorderThickness = new Thickness(secondary ? 1 : 0),
                Cursor = Cursors.Hand,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1.0, 1.0)
                       };
            btn.MouseEnter += delegate
            {
                if (btn.RenderTransform is ScaleTransform)
                {
                    ScaleTransform st = (ScaleTransform)btn.RenderTransform;
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.0, 1.05, TimeSpan.FromMilliseconds(150)));
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.0, 1.05, TimeSpan.FromMilliseconds(150)));
                }
                       };
            btn.MouseLeave += delegate
            {
                if (btn.RenderTransform is ScaleTransform)
                {
                    ScaleTransform st = (ScaleTransform)btn.RenderTransform;
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.05, 1.0, TimeSpan.FromMilliseconds(150)));
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.05, 1.0, TimeSpan.FromMilliseconds(150)));
                }
                       };
            btn.Click += handler;
            return btn;
        }


    }

    // ==========================================
    // Janela Modal: Alerta de Lembrete
    // ==========================================
    public class LembreteAlertaWindow : Window
    {
        private MainWindow _main;

        public LembreteAlertaWindow(MainWindow main)
        {
            _main = main;
            Title = "Hora do Gole! 💧";
            Width = 340;
            Height = 310;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Background = Brushes.White;
            Opacity = 0;
            Loaded += delegate {
                DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)) {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                           };
                BeginAnimation(UIElement.OpacityProperty, fadeIn);
                       };

            StackPanel sp = new StackPanel { Margin = new Thickness(20), VerticalAlignment = VerticalAlignment.Center            };

            string mascotAlertPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mascote.png");
            if (File.Exists(mascotAlertPath))
            {
                try
                {
                    Border bImg = new Border
                    {
                        Width = 64,
                        Height = 64,
                        CornerRadius = new CornerRadius(16),
                        ClipToBounds = true,
                        BorderBrush = new SolidColorBrush(Color.FromRgb(125, 211, 252)),
                        BorderThickness = new Thickness(1.5),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1.0, 1.0),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 8),
                        Effect = new DropShadowEffect { BlurRadius = 10, ShadowDepth = 2, Opacity = 0.25, Color = Color.FromRgb(2, 132, 199) }
                               };
                    Image imgAlert = new Image { Width = 64, Height = 64, Stretch = Stretch.UniformToFill            };
                    BitmapImage bmpAlert = new BitmapImage();
                    bmpAlert.BeginInit();
                    bmpAlert.UriSource = new Uri(mascotAlertPath, UriKind.Absolute);
                    bmpAlert.CacheOption = BitmapCacheOption.OnLoad;
                    bmpAlert.EndInit();
                    imgAlert.Source = bmpAlert;
                    bImg.Child = imgAlert;
                    sp.Children.Add(bImg);
                }
                catch
                {
                    sp.Children.Add(new TextBlock { Text = "💧", FontSize = 44, HorizontalAlignment = HorizontalAlignment.Center });
                }
            }
            else
            {
                sp.Children.Add(new TextBlock { Text = "💧", FontSize = 44, HorizontalAlignment = HorizontalAlignment.Center });
            }

            TextBlock title = new TextBlock
            {
                Text = "Hora de Beber Água!",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(2, 132, 199)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 8, 0, 4)
                       };
            sp.Children.Add(title);

            TextBlock desc = new TextBlock
            {
                Text = "Seus olhos e sua mente precisam de uma pausa.\nHidrate seu corpo agora!",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 14)
                       };
            sp.Children.Add(desc);

            Button btn250 = new Button
            {
                Content = "🥛 Tomei um Copo (+250 ml)",
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromRgb(2, 132, 199)),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 0, 6),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
                       };
            btn250.Click += delegate
            {
                _main.AdicionarAgua(250, "Copo (🥛)");
                FecharSuave();
                       };
            sp.Children.Add(btn250);

            Button btnAdiar = new Button
            {
                Content = "⏳ Adiar por 5 minutos",
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                Background = new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                Padding = new Thickness(12, 6, 12, 6),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
                       };
            btnAdiar.Click += delegate { FecharSuave();            };
            sp.Children.Add(btnAdiar);

            Content = sp;
        }

        private void FecharSuave()
        {
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(160));
            fadeOut.Completed += delegate { Close();            };
            BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }

    // ==========================================
    // Janela Modal: Configurações
    // ==========================================
    // Componente Moderno: Toggle Switch (BotÃ£o de AlternÃ¢ncia)
    // ==========================================
    // Componente Moderno: Toggle Switch (BotÃ£o de AlternÃ¢ncia)
    // ==========================================
    public class ModernToggleSwitch : ContentControl
    {
        private bool _isChecked;
        private Border _track;
        private Ellipse _thumb;
        private TextBlock _labelStatus;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    AtualizarVisual();
                    if (IsCheckedChanged != null)
                    {
                        IsCheckedChanged(this, _isChecked);
                    }
                }
            }
        }

        public event Action<object, bool> IsCheckedChanged;

        public ModernToggleSwitch(bool initialValue)
        {
            _isChecked = initialValue;
            Cursor = Cursors.Hand;
            Focusable = true;

            Grid container = new Grid { VerticalAlignment = VerticalAlignment.Center            };
            container.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            container.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _track = new Border
            {
                Width = 48,
                Height = 26,
                CornerRadius = new CornerRadius(13),
                Background = _isChecked ? new SolidColorBrush(Color.FromRgb(2, 132, 199)) : new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                Padding = new Thickness(3),
                VerticalAlignment = VerticalAlignment.Center
                       };

            Grid thumbGrid = new Grid();
            _thumb = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.White,
                HorizontalAlignment = _isChecked ? HorizontalAlignment.Right : HorizontalAlignment.Left
                       };
            thumbGrid.Children.Add(_thumb);
            _track.Child = thumbGrid;

            _labelStatus = new TextBlock
            {
                Text = _isChecked ? "Ativado" : "Desativado",
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = _isChecked ? new SolidColorBrush(Color.FromRgb(2, 132, 199)) : new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                MinWidth = 65
                       };

            Grid.SetColumn(_track, 0);
            Grid.SetColumn(_labelStatus, 1);
            container.Children.Add(_track);
            container.Children.Add(_labelStatus);

            Content = container;

            MouseDown += delegate(object s, MouseButtonEventArgs e)
            {
                IsChecked = !IsChecked;
                e.Handled = true;
                       };

            KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.Key == Key.Space || e.Key == Key.Enter)
                {
                    IsChecked = !IsChecked;
                    e.Handled = true;
                }
                       };
        }

        private void AtualizarVisual()
        {
            if (_track == null || _thumb == null || _labelStatus == null) return;

            _track.Background = _isChecked ? new SolidColorBrush(Color.FromRgb(2, 132, 199)) : new SolidColorBrush(Color.FromRgb(203, 213, 225));
            _thumb.HorizontalAlignment = _isChecked ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            _labelStatus.Text = _isChecked ? "Ativado" : "Desativado";
            _labelStatus.Foreground = _isChecked ? new SolidColorBrush(Color.FromRgb(2, 132, 199)) : new SolidColorBrush(Color.FromRgb(148, 163, 184));
        }
    }

    // ==========================================
    // Janela Modal: ConfiguraÃ§Ãµes com Toggle Nativo
    // ==========================================
    public class ConfigWindow : Window
    {
        public ConfigWindow(HistoricoApp dados, Action onSalvar)
        {
            Title = "Configura\u00E7\u00F5es da Gotinha \u2699\uFE0F";
            Width = 400;
            Height = 490;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Background = new SolidColorBrush(Color.FromRgb(248, 250, 252));
            Opacity = 0;
            Loaded += delegate {
                DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)) {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                           };
                BeginAnimation(UIElement.OpacityProperty, fadeIn);
                       };

            ScrollViewer sv = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto            };
            StackPanel sp = new StackPanel { Margin = new Thickness(22)            };

            // CabeÃ§alho da Janela
            StackPanel headerSp = new StackPanel { Margin = new Thickness(0, 0, 0, 16)            };
            headerSp.Children.Add(new TextBlock
            {
                Text = "Configura\u00E7\u00F5es & Lembretes",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 42))
            });
            headerSp.Children.Add(new TextBlock
            {
                Text = "Ajuste sua meta di\u00E1ria e prefer\u00EAncias do Windows",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                Margin = new Thickness(0, 2, 0, 0)
            });
            sp.Children.Add(headerSp);

            // Card 1: Meta DiÃ¡ria
            Border cardMeta = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 0, 0, 12)
                       };
            StackPanel spMeta = new StackPanel();
            spMeta.Children.Add(new TextBlock
            {
                Text = "\uD83D\uDCA7 Meta Di\u00E1ria de \u00C1gua (ml)",
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                Margin = new Thickness(0, 0, 0, 6)
            });

            TextBox txtMeta = new TextBox
            {
                Text = dados.MetaDiaria.ToString(),
                FontSize = 13,
                Padding = new Thickness(8, 6, 8, 6),
                BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                Margin = new Thickness(0, 0, 0, 8)
                       };
            spMeta.Children.Add(txtMeta);

            Grid pesoGrid = new Grid();
            pesoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            pesoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBox txtPeso = new TextBox
            {
                FontSize = 11,
                Padding = new Thickness(8, 6, 8, 6),
                ToolTip = "Digite seu peso em kg (ex: 70)",
                BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225))
                       };
            Grid.SetColumn(txtPeso, 0);
            pesoGrid.Children.Add(txtPeso);

            Button btnCalcular = new Button
            {
                Content = "Calcular p/ Peso",
                FontSize = 11,
                Padding = new Thickness(10, 6, 10, 6),
                Margin = new Thickness(8, 0, 0, 0),
                Background = new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                Cursor = Cursors.Hand
                       };
            btnCalcular.Click += delegate
            {
                double peso;
                if (double.TryParse(txtPeso.Text, out peso) && peso > 20)
                {
                    int recomendacao = (int)Math.Round(peso * 35);
                    txtMeta.Text = recomendacao.ToString();
                }
                else
                {
                    MessageBox.Show("Digite um peso v\u00E1lido em kg (ex: 65).", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                       };
            Grid.SetColumn(btnCalcular, 1);
            pesoGrid.Children.Add(btnCalcular);
            spMeta.Children.Add(pesoGrid);
            cardMeta.Child = spMeta;
            sp.Children.Add(cardMeta);

            // Card 2: NotificaÃ§Ãµes Nativas do Windows com TOGGLE SWITCH
            Border cardNotif = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 0, 0, 12)
                       };
            Grid notifGrid = new Grid();
            notifGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            notifGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel notifTextSp = new StackPanel { VerticalAlignment = VerticalAlignment.Center            };
            notifTextSp.Children.Add(new TextBlock
            {
                Text = "\uD83D\uDD14 Notifica\u00E7\u00F5es do Windows",
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59))
            });
            notifTextSp.Children.Add(new TextBlock
            {
                Text = "Exibe lembretes na Central de A\u00E7\u00F5es no canto inferior direito, mesmo em segundo plano.",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 8, 0)
            });
            Grid.SetColumn(notifTextSp, 0);
            notifGrid.Children.Add(notifTextSp);

            ModernToggleSwitch toggleNotif = new ModernToggleSwitch(dados.NotificacoesAtivadas);
            Grid.SetColumn(toggleNotif, 1);
            notifGrid.Children.Add(toggleNotif);

            cardNotif.Child = notifGrid;
            sp.Children.Add(cardNotif);

            // Card 3: Efeitos Sonoros com TOGGLE SWITCH
            Border cardSom = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 0, 0, 18)
                       };
            Grid somGrid = new Grid();
            somGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            somGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel somTextSp = new StackPanel { VerticalAlignment = VerticalAlignment.Center            };
            somTextSp.Children.Add(new TextBlock
            {
                Text = "\uD83D\uDD0A Sons de Gota e Sino",
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59))
            });
            somTextSp.Children.Add(new TextBlock
            {
                Text = "Tocar efeitos sonoros relaxantes ao registrar \u00E1gua e alertar.",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 8, 0)
            });
            Grid.SetColumn(somTextSp, 0);
            somGrid.Children.Add(somTextSp);

            ModernToggleSwitch toggleSom = new ModernToggleSwitch(dados.SomAtivado);
            Grid.SetColumn(toggleSom, 1);
            somGrid.Children.Add(toggleSom);

            cardSom.Child = somGrid;
            sp.Children.Add(cardSom);

            // Card 4: Modo Escuro (Oceano Noturno)
            Border cardTema = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(10),
                BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 0, 0, 12)
            };
            Grid temaGrid = new Grid();
            temaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            temaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel temaTextSp = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            temaTextSp.Children.Add(new TextBlock
            {
                Text = "🌙 Modo Escuro (Oceano Noturno)",
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 41, 59))
            });
            temaTextSp.Children.Add(new TextBlock
            {
                Text = "Visual confort\u00E1vel para os olhos e uso noturno, com tema azul marinho profundo.",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 8, 0)
            });
            Grid.SetColumn(temaTextSp, 0);
            temaGrid.Children.Add(temaTextSp);

            ModernToggleSwitch toggleTema = new ModernToggleSwitch(dados.ModoEscuro);
            Grid.SetColumn(toggleTema, 1);
            temaGrid.Children.Add(toggleTema);

            cardTema.Child = temaGrid;
            sp.Children.Add(cardTema);

            // BotÃ£o Salvar
            Button btnSalvar = new Button
            {
                Content = "Salvar Prefer\u00EAncias",
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromRgb(2, 132, 199)),
                Padding = new Thickness(16, 10, 16, 10),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
                       };
            btnSalvar.Click += delegate
            {
                int novaMeta;
                if (int.TryParse(txtMeta.Text, out novaMeta) && novaMeta >= 500 && novaMeta <= 8000)
                {
                    dados.MetaDiaria = novaMeta;
                    dados.NotificacoesAtivadas = toggleNotif.IsChecked;
                    dados.SomAtivado = toggleSom.IsChecked;
                    dados.ModoEscuro = toggleTema.IsChecked;
                    onSalvar();
                    DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(160));
                    fadeOut.Completed += delegate { Close();            };
                    BeginAnimation(UIElement.OpacityProperty, fadeOut);
                }
                else
                {
                    MessageBox.Show("Por favor informe uma meta di\u00E1ria v\u00E1lida entre 500 ml e 8000 ml.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                       };
            sp.Children.Add(btnSalvar);

            sv.Content = sp;
            Content = sv;
        }
    }

    // ==========================================
    // Serializador JSON Real
    // ==========================================
    public static class SimpleJson
    {
        private static readonly JavaScriptSerializer _serializer = new JavaScriptSerializer();

        public static string Serialize(HistoricoApp obj)
        {
            if (obj == null) return "{}";
            try
            {
                return _serializer.Serialize(obj);
            }
            catch (Exception ex)
            {
                try { File.AppendAllText("error.log", string.Format("[{0}] Erro ao serializar dados: {1}\r\n", DateTime.Now, ex)); } catch { }
                return "{}";
            }
        }

        public static HistoricoApp Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json)) return new HistoricoApp();
            try
            {
                HistoricoApp app = _serializer.Deserialize<HistoricoApp>(json);
                if (app != null)
                {
                    if (app.Dias == null) app.Dias = new List<DadosDia>();
                    return app;
                }
            }
            catch (Exception ex)
            {
                try { File.AppendAllText("error.log", string.Format("[{0}] Erro ao desserializar dados: {1}\r\n", DateTime.Now, ex)); } catch { }
            }
            return new HistoricoApp();
        }
    }
    public class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [DllImport("shcore.dll")]
        private static extern int SetProcessDpiAwareness(int awareness);

        [STAThread]
        public static void Main()
        {
            try
            {
                // Registrar AUMID do aplicativo e atalho no Menu Iniciar para Central de NotificaÃ§Ãµes
                try
                {
                    ShellShortcutHelper.SetCurrentProcessExplicitAppUserModelID(ShellShortcutHelper.APP_ID);
                    ShellShortcutHelper.GarantirAtalhoMenuIniciar();
                }
                catch { }

                try
                {
                    // Tenta Per-Monitor DPI Awareness (Windows 8.1+) ou fallback para DPI Aware (Windows Vista/7/8)
                    SetProcessDpiAwareness(2); // 2 = Per_Monitor_DPI_Aware
                }
                catch
                {
                    try { SetProcessDPIAware(); } catch { }
                }

                AppDomain.CurrentDomain.UnhandledException += delegate(object s, UnhandledExceptionEventArgs e)
                {
                    try { File.WriteAllText("error.log", e.ExceptionObject.ToString()); } catch { }
                           };

                Application app = new Application();
                app.DispatcherUnhandledException += delegate(object s, DispatcherUnhandledExceptionEventArgs e)
                {
                    try { File.WriteAllText("error.log", e.Exception.ToString()); } catch { }
                    e.Handled = true;
                           };

                app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                SplashWindow splash = new SplashWindow();
                splash.Show();
                app.Run();
            }
            catch (Exception ex)
            {
                try { File.WriteAllText("error.log", ex.ToString()); } catch { }
                MessageBox.Show(ex.ToString(), "Erro ao Iniciar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
