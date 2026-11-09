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
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Interop;
using System.Windows.Markup;
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
        public const string APP_ID = "Lumi.Agua.App";

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
                string shortcutPath = System.IO.Path.Combine(programsPath, "Lumi.lnk");
                string targetExe = Process.GetCurrentProcess().MainModule.FileName;
                string iconPath = IconeHelper.ObterCaminhoIcone();
                if (string.IsNullOrEmpty(iconPath)) iconPath = targetExe;

                CShellLink link = new CShellLink();
                IShellLinkW shellLink = (IShellLinkW)link;
                shellLink.SetPath(targetExe);
                shellLink.SetWorkingDirectory(System.IO.Path.GetDirectoryName(targetExe));
                shellLink.SetDescription("Lumi - Lembrete de Ãgua");
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
    // Gerenciador centralizado do Ícone do App
    // ==========================================
    public static class IconeHelper
    {
        public static string ObterCaminhoIcone()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string iconePath = System.IO.Path.Combine(baseDir, "icone.ico");
                if (System.IO.File.Exists(iconePath)) return iconePath;
                string appIco = System.IO.Path.Combine(baseDir, "app.ico");
                if (System.IO.File.Exists(appIco)) return appIco;
            }
            catch { }
            return null;
        }

        public static System.Windows.Media.ImageSource ObterIconeJanela()
        {
            try
            {
                string icoPath = ObterCaminhoIcone();
                if (!string.IsNullOrEmpty(icoPath) && System.IO.File.Exists(icoPath))
                {
                    return System.Windows.Media.Imaging.BitmapFrame.Create(new Uri(icoPath, UriKind.Absolute));
                }
            }
            catch { }
            return null;
        }
    }

    // ==========================================
    // Janela Principal do Aplicativo (WPF Moderno)
    // ==========================================
    public class MainWindow : Window
    {
        public HistoricoApp _dados;
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
        private Border _badgeIconeTimer;
        private TextBlock _txtIconeTimer;
        private Border _trackTimerProgress;
        private Border _fillTimerProgress;
        private ColumnDefinition _colTimerAtivo;
        private ColumnDefinition _colTimerRestante;
        private Button _btnTestarLembrete;
        private TextBlock _txtLembrarACada;
        private TextBlock _txtTituloRegistrar;
        private Button _btnDesfazer;
        private List<Button> _botoesBebida = new List<Button>();
        private TextBox _txtCustomAgua;

        private Border _bordaAguaCopo;
        private Border _copoContainer;
        private Canvas _canvasMascote;
        private Border _imgBorderMascote;
        private ScaleTransform _stMascote;
        private TranslateTransform _ttMascote;
        private RotateTransform _rtMascote;
        private System.Windows.Shapes.Path _pathBocaGotinha;
        private System.Windows.Shapes.Polygon _coroaGotinha;
        private Ellipse _olhoEsquerdo;
        private Ellipse _olhoDireito;
        private StackPanel _painelHistorico;
        private StackPanel _painelGraficoSemanal;
        private Button _btnTabHoje;
        private Button _btnTabSemana;
        private UIElement _containerTabHoje;
        private UIElement _containerTabSemana;
        private Border _pillsSegmentedContainer;
        private TextBlock _txtTituloHistorico;
        private TextBlock _txtSubtituloHistorico;
        private Button _btnVoltarHistorico;
        private Button _btnExportarRelatorio;
        private TextBlock _txtStatsHojeVal;
        private TextBlock _txtStatsHojeSub;
        private TextBlock _txtStatsMediaVal;
        private TextBlock _txtStatsMetasVal;
        private Border _cardStatsHoje;
        private Border _cardStatsMedia;
        private Border _cardStatsMetas;
        private int _abaHistoricoAtiva = 0;

        // Elementos de Feedback & Overlay
        private Grid _containerPrincipal;
        private Border _snackBar;
        private TextBlock _txtSnackBar;
        private Border _overlayLoading;
        private ProgressBar _progressBarMeta;
        private Border _janelaBorda;
        private Button _btnMax;
        private Grid _gridConteudo;
        private UIElement _viewGotinha;
        private UIElement _viewHistorico;
        private Button _btnToggleHistorico;
        private Button _btnModoWidget;
        private WidgetWindow _widgetWindow;
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

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            if (source != null)
            {
                source.AddHook(WndProc);
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_NCHITTEST = 0x0084;
            if (msg == WM_NCHITTEST && WindowState == WindowState.Normal)
            {
                try
                {
                    short sx = unchecked((short)(lParam.ToInt32() & 0xFFFF));
                    short sy = unchecked((short)((lParam.ToInt32() >> 16) & 0xFFFF));

                    Point ptClient = PointFromScreen(new Point(sx, sy));

                    const int b = 8; // espessura da margem de redimensionamento em pixels

                    if (ptClient.X >= -5 && ptClient.X <= ActualWidth + 5 &&
                        ptClient.Y >= -5 && ptClient.Y <= ActualHeight + 5)
                    {
                        bool left = ptClient.X <= b;
                        bool right = ptClient.X >= (ActualWidth - b);
                        bool top = ptClient.Y <= b;
                        bool bottom = ptClient.Y >= (ActualHeight - b);

                        const int HTLEFT = 10;
                        const int HTRIGHT = 11;
                        const int HTTOP = 12;
                        const int HTTOPLEFT = 13;
                        const int HTTOPRIGHT = 14;
                        const int HTBOTTOM = 15;
                        const int HTBOTTOMLEFT = 16;
                        const int HTBOTTOMRIGHT = 17;

                        if (top && left) { handled = true; return new IntPtr(HTTOPLEFT); }
                        if (top && right) { handled = true; return new IntPtr(HTTOPRIGHT); }
                        if (bottom && left) { handled = true; return new IntPtr(HTBOTTOMLEFT); }
                        if (bottom && right) { handled = true; return new IntPtr(HTBOTTOMRIGHT); }
                        if (left) { handled = true; return new IntPtr(HTLEFT); }
                        if (right) { handled = true; return new IntPtr(HTRIGHT); }
                        if (top) { handled = true; return new IntPtr(HTTOP); }
                        if (bottom) { handled = true; return new IntPtr(HTBOTTOM); }
                    }
                }
                catch { }
            }
            return IntPtr.Zero;
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
            bool isDark = (_dados != null && _dados.ModoEscuro);
            Brush cor = isDark
                ? Brushes.White
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
                    _btnMax.Content = CriarIconeMaximizar(cor, true, isDark);
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
                    _btnMax.Content = CriarIconeMaximizar(cor, false, isDark);
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
            Title = "Lumi - Lembrete de \u00C1gua \uD83D\uDCA7";

            double workH = SystemParameters.WorkArea.Height;
            double workW = SystemParameters.WorkArea.Width;

            // Dimensões responsivas para qualquer resolução (ex: 1366x768 ou escala de 125%/150%)
            Width = Math.Min(490, Math.Max(380, workW - 40));
            Height = Math.Min(740, Math.Max(520, workH - 50));
            MinWidth = 360;
            MinHeight = 440;
            MaxHeight = workH;
            MaxWidth = workW;
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

            Icon = IconeHelper.ObterIconeJanela();

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
            layoutInterno.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 0: TitleBar (36px)
            layoutInterno.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 1: ActionToolbar (44px espaçosa)
            layoutInterno.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Row 2: Conteúdo

            // 1. Barra de Título Customizada (Apenas Marca na esquerda + Janela na direita)
            Grid titleBar = new Grid
            {
                Height = 36,
                Background = Brushes.Transparent
            };
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

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
                Margin = new Thickness(14, 0, 0, 0)
            };

            Image imgBrandIcon = new Image
            {
                Width = 26,
                Height = 26,
                Margin = new Thickness(0, 0, 6, 0),
                Stretch = Stretch.Uniform
            };
            string brandMascotPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mascote.png");
            if (File.Exists(brandMascotPath))
            {
                try
                {
                    BitmapImage bmpBrand = new BitmapImage();
                    bmpBrand.BeginInit();
                    bmpBrand.UriSource = new Uri(brandMascotPath, UriKind.Absolute);
                    bmpBrand.CacheOption = BitmapCacheOption.OnLoad;
                    bmpBrand.EndInit();
                    imgBrandIcon.Source = bmpBrand;
                }
                catch { }
            }
            appBrand.Children.Add(imgBrandIcon);

            _txtBrandTitle = new TextBlock
            {
                Text = "Lumi",
                FontWeight = FontWeights.ExtraBold,
                FontSize = 13.5,
                Foreground = new SolidColorBrush(Color.FromRgb(3, 105, 161)),
                VerticalAlignment = VerticalAlignment.Center
            };
            appBrand.Children.Add(_txtBrandTitle);

            _txtBrandSub = new TextBlock
            {
                Text = " \u2022 Hidrata\u00E7\u00E3o",
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                VerticalAlignment = VerticalAlignment.Center
            };
            appBrand.Children.Add(_txtBrandSub);

            _txtStatusPill = new TextBlock
            {
                Text = "\u25CF Ativo",
                FontSize = 9.5,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(5, 150, 105))
            };
            _statusPill = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(236, 253, 245)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(167, 243, 208)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6, 1, 6, 1),
                Margin = new Thickness(8, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Child = _txtStatusPill
            };
            appBrand.Children.Add(_statusPill);
            Grid.SetColumn(appBrand, 0);
            titleBar.Children.Add(appBrand);

            // Botões de Janela na extrema direita (Minimizar, Maximizar, Fechar)
            StackPanel windowButtons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 6, 0)
            };

            SolidColorBrush corIconeBarra = new SolidColorBrush(Color.FromRgb(3, 105, 161));

            _btnMin = new Button
            {
                Content = CriarIconeMinimizar(corIconeBarra),
                ToolTip = "Minimizar",
                Width = 38,
                Height = 30,
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
                Width = 38,
                Height = 30,
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
                Width = 38,
                Height = 30,
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
                bool isDark = (_dados != null && _dados.ModoEscuro);
                _btnClose.Content = CriarIconeFechar(isDark ? Brushes.White : corIconeBarra);
            };
            _btnClose.Click += delegate
            {
                WindowState = WindowState.Minimized;
                if (_dados.NotificacoesAtivadas)
                {
                    NotificadorWindows.EnviarNotificacao(
                        "Lumi continua rodando \uD83D\uDCA7",
                        "Vou continuar te lembrando de beber \u00E1gua enquanto voc\u00EA usa o computador!",
                        _trayIcon
                    );
                }
            };

            windowButtons.Children.Add(_btnMin);
            windowButtons.Children.Add(_btnMax);
            windowButtons.Children.Add(_btnClose);
            Grid.SetColumn(windowButtons, 1);
            titleBar.Children.Add(windowButtons);

            Grid.SetRow(titleBar, 0);
            layoutInterno.Children.Add(titleBar);

            // 1.5 Nova Barra de Ações Dedicada e Espaçosa (Sem aperto!)
            Grid actionToolbar = new Grid
            {
                Margin = new Thickness(14, 2, 14, 6)
            };
            actionToolbar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            actionToolbar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel toolsLeft = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            _btnToggleHistorico = new Button
            {
                Content = "\uD83D\uDCCA Ver Hist\u00F3rico",
                ToolTip = "Alternar para o Hist\u00F3rico e Estat\u00EDsticas",
                FontSize = 11.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(2, 132, 199)),
                Background = new SolidColorBrush(Color.FromArgb(220, 224, 242, 254)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                BorderThickness = new Thickness(1.2),
                Padding = new Thickness(12, 6, 12, 6),
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center
            };
            AplicarEstiloPill(_btnToggleHistorico, 12);
            _btnToggleHistorico.MouseEnter += delegate
            {
                bool isDark = (_dados != null && _dados.ModoEscuro);
                _btnToggleHistorico.Background = isDark ? new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                _btnToggleHistorico.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            };
            _btnToggleHistorico.MouseLeave += delegate
            {
                bool isDark = (_dados != null && _dados.ModoEscuro);
                _btnToggleHistorico.Background = isDark ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromArgb(220, 224, 242, 254));
                _btnToggleHistorico.BorderBrush = isDark ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
            };
            _btnToggleHistorico.Click += delegate { AlternarVisualizacaoHistorico(); };
            toolsLeft.Children.Add(_btnToggleHistorico);

            _btnModoWidget = new Button
            {
                Content = "\uD83D\uDCCC Modo Widget",
                ToolTip = "Colocar no canto da tela como um mini widget flutuante",
                FontSize = 11.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(2, 132, 199)),
                Background = new SolidColorBrush(Color.FromArgb(220, 224, 242, 254)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                BorderThickness = new Thickness(1.2),
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(6, 0, 0, 0),
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center
            };
            AplicarEstiloPill(_btnModoWidget, 12);
            _btnModoWidget.MouseEnter += delegate
            {
                bool isDark = (_dados != null && _dados.ModoEscuro);
                _btnModoWidget.Background = isDark ? new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                _btnModoWidget.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
            };
            _btnModoWidget.MouseLeave += delegate
            {
                bool isDark = (_dados != null && _dados.ModoEscuro);
                _btnModoWidget.Background = isDark ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromArgb(220, 224, 242, 254));
                _btnModoWidget.BorderBrush = isDark ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
            };
            _btnModoWidget.Click += delegate { AtivarModoWidget(); };
            toolsLeft.Children.Add(_btnModoWidget);

            Grid.SetColumn(toolsLeft, 0);
            actionToolbar.Children.Add(toolsLeft);

            StackPanel toolsRight = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            _txtHeaderTitle = _txtBrandTitle;
            _btnSom = CriarBotaoToolbarCard(CriarIconeSom(_dados.SomAtivado), _dados.SomAtivado ? "Som ativado" : "Som silenciado", delegate { AlternarSom(); }, false);
            _btnNotif = CriarBotaoToolbarCard(CriarIconeNotif(_dados.NotificacoesAtivadas), _dados.NotificacoesAtivadas ? "Notifica\u00E7\u00F5es ativadas" : "Notifica\u00E7\u00F5es desativadas", delegate { AlternarNotificacoes(); }, false);
            _btnTema = CriarBotaoToolbarCard(CriarIconeTema(_dados.ModoEscuro), _dados.ModoEscuro ? "Ativar Modo Claro" : "Ativar Modo Escuro", delegate { AlternarModoEscuro(); }, false);
            _btnConfig = CriarBotaoToolbarCard(CriarIconeEngrenagem(), "Configurar Meta e Prefer\u00EAncias", delegate { AbrirJanelaConfiguracoes(); }, true);

            _btnSom.Margin = new Thickness(4, 0, 0, 0);
            _btnNotif.Margin = new Thickness(4, 0, 0, 0);
            _btnTema.Margin = new Thickness(4, 0, 0, 0);
            _btnConfig.Margin = new Thickness(4, 0, 0, 0);

            toolsRight.Children.Add(_btnSom);
            toolsRight.Children.Add(_btnNotif);
            toolsRight.Children.Add(_btnTema);
            toolsRight.Children.Add(_btnConfig);
            Grid.SetColumn(toolsRight, 1);
            actionToolbar.Children.Add(toolsRight);

            Grid.SetRow(actionToolbar, 1);
            layoutInterno.Children.Add(actionToolbar);

            // 2. Área de Conteúdo Centralizada e Unificada (Row 2)
            _gridConteudo = new Grid { Margin = new Thickness(14, 0, 14, 14) };

            _viewGotinha = CriarViewGotinha();
            _gridConteudo.Children.Add(_viewGotinha);

            _viewHistorico = CriarViewHistorico();
            _gridConteudo.Children.Add(_viewHistorico);

            Grid.SetRow(_gridConteudo, 2);
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

        // --- Visão Central da Gotinha Companheira ---
        private UIElement CriarViewGotinha()
        {
            ScrollViewer scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                FocusVisualStyle = null
            };

            scroll.PreviewMouseWheel += delegate(object s, MouseWheelEventArgs e)
            {
                ScrollViewer scv = s as ScrollViewer;
                if (scv != null && scv.ScrollableHeight > 0)
                {
                    scv.ScrollToVerticalOffset(scv.VerticalOffset - (e.Delta > 0 ? 45 : -45));
                    e.Handled = true;
                }
            };

            StackPanel sp = new StackPanel { Margin = new Thickness(4, 0, 4, 8) };

            // 1. Mascote Gotinha GRANDE no CENTRO!
            Border mascoteBox = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 2, 0, 6)
            };
            _canvasMascote = CriarMascoteCanvas();
            mascoteBox.Child = _canvasMascote;
            sp.Children.Add(mascoteBox);

            // 2. Balão de fala carinhoso centralizado
            _speechBubble = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                MaxWidth = 410,
                Background = new SolidColorBrush(Color.FromArgb(235, 240, 249, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(16, 8, 16, 8),
                Margin = new Thickness(0, 0, 0, 8),
                Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(2, 132, 199),
                    BlurRadius = 10,
                    ShadowDepth = 1,
                    Opacity = 0.08
                }
            };
            _txtMensagemGotinha = new TextBlock
            {
                Text = "Oi! Voc\u00EA est\u00E1 indo muito bem! \u2728",
                FontSize = 12.5,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Color.FromRgb(3, 105, 161)),
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            _speechBubble.Child = _txtMensagemGotinha;
            sp.Children.Add(_speechBubble);

            // 3. Card de Progresso
            UIElement cardProgresso = CriarCardProgressoCentral();
            sp.Children.Add(cardProgresso);

            // 4. Card do Temporizador Zen
            UIElement cardLembrete = CriarCardLembrete();
            sp.Children.Add(cardLembrete);

            // 5. Card de Registrar Bebidas
            UIElement cardBotoes = CriarCardBotoes();
            sp.Children.Add(cardBotoes);

            scroll.Content = sp;
            return scroll;
        }

        private UIElement CriarViewHistorico()
        {
            Grid gridHist = new Grid { Margin = new Thickness(4, 0, 4, 8) };
            gridHist.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            gridHist.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Header do Historico: Titulo amigavel + Botao Voltar em pilula
            Grid topHist = new Grid { Margin = new Thickness(2, 0, 2, 10) };
            topHist.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topHist.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel titleStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            _txtTituloHistorico = new TextBlock
            {
                Text = "\uD83D\uDCCA Hist\u00F3rico de Hidrata\u00E7\u00E3o",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = _dados.ModoEscuro ? (Brush)new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199))
            };
            titleStack.Children.Add(_txtTituloHistorico);

            _txtSubtituloHistorico = new TextBlock
            {
                Text = "Acompanhe seus goles e conquistas di\u00E1rias \u2728",
                FontSize = 11,
                Foreground = _dados.ModoEscuro ? (Brush)new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                Margin = new Thickness(0, 2, 0, 0)
            };
            titleStack.Children.Add(_txtSubtituloHistorico);
            Grid.SetColumn(titleStack, 0);
            topHist.Children.Add(titleStack);

            StackPanel topActions = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

            _btnExportarRelatorio = CriarBotaoPill("\uD83D\uDCCB Relat\u00F3rio de Sa\u00FAde", true, delegate { AbrirJanelaRelatorioSaude(); });
            _btnExportarRelatorio.Padding = new Thickness(10, 6, 10, 6);
            _btnExportarRelatorio.ToolTip = "Exportar Relat\u00F3rio Completo de Sa\u00FAde e Hidrata\u00E7\u00E3o (TXT / CSV)";
            _btnExportarRelatorio.Margin = new Thickness(0, 0, 6, 0);
            topActions.Children.Add(_btnExportarRelatorio);

            _btnVoltarHistorico = CriarBotaoPill("\u2190 Voltar \u00E0 Lumi", true, delegate { AlternarVisualizacaoHistorico(); });
            _btnVoltarHistorico.Padding = new Thickness(12, 6, 12, 6);
            topActions.Children.Add(_btnVoltarHistorico);

            Grid.SetColumn(topActions, 1);
            topHist.Children.Add(topActions);

            Grid.SetRow(topHist, 0);
            gridHist.Children.Add(topHist);

            UIElement cardHistorico = CriarCardHistorico();
            Grid.SetRow(cardHistorico, 1);
            gridHist.Children.Add(cardHistorico);

            gridHist.Visibility = Visibility.Collapsed;
            return gridHist;
        }

        private void AlternarVisualizacaoHistorico()
        {
            if (_viewGotinha == null || _viewHistorico == null) return;

            bool mostrandoHistorico = (_viewHistorico.Visibility == Visibility.Visible);

            UIElement sair = mostrandoHistorico ? _viewHistorico : _viewGotinha;
            UIElement entrar = mostrandoHistorico ? _viewGotinha : _viewHistorico;

            if (_btnToggleHistorico != null)
            {
                _btnToggleHistorico.Content = mostrandoHistorico ? "\uD83D\uDCCA Ver Hist\u00F3rico" : "\uD83D\uDCA7 Ver Lumi";
                _btnToggleHistorico.ToolTip = mostrandoHistorico ? "Ver Hist\u00F3rico e Estat\u00EDsticas" : "Voltar para a Lumi";
            }

            if (!mostrandoHistorico)
            {
                AtualizarEstatisticasHistorico();
                AtualizarGraficoSemanal();
                AtualizarListaHoje(ObterDiaHoje());
            }

            // Animacao de transicao suave (Fade + Slide)
            DoubleAnimation fadeOut = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(160))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };
            fadeOut.Completed += delegate
            {
                sair.Visibility = Visibility.Collapsed;
                entrar.Opacity = 0.0;
                entrar.Visibility = Visibility.Visible;

                if (entrar.RenderTransform == null || !(entrar.RenderTransform is TranslateTransform))
                {
                    entrar.RenderTransform = new TranslateTransform(0, 10);
                }
                TranslateTransform tt = (TranslateTransform)entrar.RenderTransform;
                tt.Y = 10;

                DoubleAnimation fadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(220))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                DoubleAnimation slideIn = new DoubleAnimation(10, 0, TimeSpan.FromMilliseconds(220))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                entrar.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                tt.BeginAnimation(TranslateTransform.YProperty, slideIn);
            };
            sair.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private UIElement CriarCardProgressoCentral()
        {
            Border card = CriarCardBase();
            card.Padding = new Thickness(14, 10, 14, 10);
            card.Margin = new Thickness(0, 0, 0, 8);

            StackPanel sp = new StackPanel();

            // Linha de Volume Atual + Meta + Porcentagem
            Grid rowVol = new Grid();
            rowVol.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            rowVol.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            StackPanel mlStack = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Bottom };
            _txtMlAtual = new TextBlock
            {
                Text = "0",
                FontSize = 36,
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
                Margin = new Thickness(2, 0, 0, 6)
            };
            mlStack.Children.Add(_txtMlAtual);
            mlStack.Children.Add(_txtMlUnit);
            Grid.SetColumn(mlStack, 0);
            rowVol.Children.Add(mlStack);

            _badgePorcentagem = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(224, 242, 254)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10, 3, 10, 3),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            _txtPorcentagem = new TextBlock
            {
                Text = "0% da meta",
                FontSize = 11.5,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(2, 132, 199))
            };
            _badgePorcentagem.Child = _txtPorcentagem;
            Grid.SetColumn(_badgePorcentagem, 1);
            rowVol.Children.Add(_badgePorcentagem);

            sp.Children.Add(rowVol);

            _txtMetaStatus = new TextBlock
            {
                Text = "Meta di\u00E1ria: 2000 ml",
                FontSize = 11.5,
                Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                FontWeight = FontWeights.Medium,
                Margin = new Thickness(0, 2, 0, 6)
            };
            sp.Children.Add(_txtMetaStatus);

            // Barra de onda suave
            _barTrack = new Border
            {
                Height = 11,
                CornerRadius = new CornerRadius(5.5),
                Background = new SolidColorBrush(Color.FromRgb(224, 242, 254)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                BorderThickness = new Thickness(1),
                ClipToBounds = true,
                Margin = new Thickness(0, 0, 0, 6)
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
            sp.Children.Add(_barTrack);

            _progressBarMeta = new ProgressBar { Height = 0, Visibility = Visibility.Collapsed };
            sp.Children.Add(_progressBarMeta);

            // Pílula de incentivo da meta
            _cardStatusFalta = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(220, 240, 249, 255)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10, 5, 10, 5)
            };
            _txtFaltaMl = new TextBlock
            {
                Text = "\uD83D\uDCA7 Faltam 2000 ml para a sua meta di\u00E1ria!",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(3, 105, 161)),
                TextAlignment = TextAlignment.Center
            };
            _cardStatusFalta.Child = _txtFaltaMl;
            sp.Children.Add(_cardStatusFalta);

            _copoContainer = new Border { Visibility = Visibility.Collapsed };
            _bordaAguaCopo = new Border { Height = 0 };
            _copoContainer.Child = _bordaAguaCopo;
            sp.Children.Add(_copoContainer);

            card.Child = sp;
            return card;
        }

        private Canvas CriarMascoteCanvas()
        {
            Canvas canvas = new Canvas
            {
                Width = 140,
                Height = 142,
                Cursor = Cursors.Hand
            };

            string mascotPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mascote.png");
            if (File.Exists(mascotPath))
            {
                try
                {
                    _imgBorderMascote = new Border
                    {
                        Width = 136,
                        Height = 136,
                        CornerRadius = new CornerRadius(0),
                        ClipToBounds = false,
                        Background = Brushes.Transparent,
                        BorderThickness = new Thickness(0),
                        Effect = new DropShadowEffect
                        {
                            BlurRadius = 18,
                            ShadowDepth = 3,
                            Opacity = 0.22,
                            Color = Color.FromRgb(2, 132, 199)
                        }
                    };

                    Image img = new Image
                    {
                        Width = 136,
                        Height = 136,
                        Stretch = Stretch.Uniform
                    };
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(mascotPath, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    img.Source = bmp;

                    _imgBorderMascote.Child = img;

                    // Grupo de transformações para respiração, flutuação e pulo da Gotinha
                    TransformGroup tg = new TransformGroup();
                    _stMascote = new ScaleTransform(1.0, 1.0);
                    _rtMascote = new RotateTransform(0.0);
                    _ttMascote = new TranslateTransform(0.0, 0.0);
                    tg.Children.Add(_stMascote);
                    tg.Children.Add(_rtMascote);
                    tg.Children.Add(_ttMascote);

                    _imgBorderMascote.RenderTransformOrigin = new Point(0.5, 0.5);
                    _imgBorderMascote.RenderTransform = tg;

                    Canvas.SetLeft(_imgBorderMascote, 5);
                    Canvas.SetTop(_imgBorderMascote, 6);
                    canvas.Children.Add(_imgBorderMascote);

                    // Efeito suave de hover na Gotinha
                    _imgBorderMascote.MouseEnter += delegate
                    {
                        DoubleAnimation sa = new DoubleAnimation(1.0, 1.05, TimeSpan.FromMilliseconds(160))
                        {
                            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                        };
                        _stMascote.BeginAnimation(ScaleTransform.ScaleXProperty, sa);
                        _stMascote.BeginAnimation(ScaleTransform.ScaleYProperty, sa);
                    };
                    _imgBorderMascote.MouseLeave += delegate
                    {
                        DoubleAnimation sa = new DoubleAnimation(1.05, 1.0, TimeSpan.FromMilliseconds(180))
                        {
                            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                        };
                        _stMascote.BeginAnimation(ScaleTransform.ScaleXProperty, sa);
                        _stMascote.BeginAnimation(ScaleTransform.ScaleYProperty, sa);
                    };

                    _coroaGotinha = new System.Windows.Shapes.Polygon
                    {
                        Points = new PointCollection(new Point[] {
                            new Point(60, 18), new Point(68, 4), new Point(78, 11), new Point(88, 4), new Point(96, 18)
                        }),
                        Fill = new SolidColorBrush(Color.FromRgb(251, 191, 36)),
                        Stroke = new SolidColorBrush(Color.FromRgb(217, 119, 6)),
                        StrokeThickness = 1.5,
                        Visibility = Visibility.Collapsed
                    };
                    canvas.Children.Add(_coroaGotinha);

                    canvas.MouseLeftButtonDown += delegate { InteragirComGotinha(); };

                    // Inicia animação fluida e suave de flutuar na água
                    IniciarAnimacaoFlutuarGotinha();

                    return canvas;
                }
                catch { }
            }

            // Fallback vetorial
            System.Windows.Shapes.Path corpo = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse("M 60 14 C 60 14 18 72 18 92 C 18 116 36 124 60 124 C 84 124 102 116 102 92 C 102 72 60 14 60 14 Z"),
                Fill = new LinearGradientBrush(
                    Color.FromRgb(56, 189, 248),
                    Color.FromRgb(2, 132, 199),
                    90
                ),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 18,
                    ShadowDepth = 3,
                    Opacity = 0.35,
                    Color = Color.FromRgb(2, 132, 199)
                }
            };
            canvas.Children.Add(corpo);

            _olhoEsquerdo = new Ellipse { Width = 8, Height = 10, Fill = new SolidColorBrush(Color.FromRgb(30, 41, 59)) };
            Canvas.SetLeft(_olhoEsquerdo, 42);
            Canvas.SetTop(_olhoEsquerdo, 82);
            canvas.Children.Add(_olhoEsquerdo);

            _olhoDireito = new Ellipse { Width = 8, Height = 10, Fill = new SolidColorBrush(Color.FromRgb(30, 41, 59)) };
            Canvas.SetLeft(_olhoDireito, 70);
            Canvas.SetTop(_olhoDireito, 82);
            canvas.Children.Add(_olhoDireito);

            _pathBocaGotinha = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse("M 52 96 Q 60 105 68 96"),
                Stroke = new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                StrokeThickness = 3.0,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };
            canvas.Children.Add(_pathBocaGotinha);

            _coroaGotinha = new System.Windows.Shapes.Polygon
            {
                Points = new PointCollection(new Point[] {
                    new Point(42, 24), new Point(50, 10), new Point(60, 18), new Point(70, 10), new Point(78, 24)
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

        private void IniciarAnimacaoFlutuarGotinha()
        {
            if (_ttMascote == null || _stMascote == null) return;

            DoubleAnimation animFloat = new DoubleAnimation(0.0, -5.0, TimeSpan.FromMilliseconds(1900))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            _ttMascote.BeginAnimation(TranslateTransform.YProperty, animFloat);

            DoubleAnimation animBreath = new DoubleAnimation(1.0, 1.025, TimeSpan.FromMilliseconds(1900))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            _stMascote.BeginAnimation(ScaleTransform.ScaleYProperty, animBreath);
        }

        private void ExecutarAnimacaoPuloGotinha()
        {
            if (_stMascote == null || _ttMascote == null) return;

            // Animação elástica estilo geléia / pulo comemorativo
            DoubleAnimationUsingKeyFrames animScaleX = new DoubleAnimationUsingKeyFrames();
            animScaleX.KeyFrames.Add(new SplineDoubleKeyFrame(1.16, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(85))));
            animScaleX.KeyFrames.Add(new SplineDoubleKeyFrame(0.88, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(220))));
            animScaleX.KeyFrames.Add(new SplineDoubleKeyFrame(1.04, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(340))));
            animScaleX.KeyFrames.Add(new SplineDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(460))));
            _stMascote.BeginAnimation(ScaleTransform.ScaleXProperty, animScaleX);

            DoubleAnimationUsingKeyFrames animScaleY = new DoubleAnimationUsingKeyFrames();
            animScaleY.KeyFrames.Add(new SplineDoubleKeyFrame(0.84, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(85))));
            animScaleY.KeyFrames.Add(new SplineDoubleKeyFrame(1.18, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(220))));
            animScaleY.KeyFrames.Add(new SplineDoubleKeyFrame(0.96, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(340))));
            animScaleY.KeyFrames.Add(new SplineDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(460))));
            _stMascote.BeginAnimation(ScaleTransform.ScaleYProperty, animScaleY);

            DoubleAnimationUsingKeyFrames animY = new DoubleAnimationUsingKeyFrames();
            animY.KeyFrames.Add(new SplineDoubleKeyFrame(4.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(85))));
            animY.KeyFrames.Add(new SplineDoubleKeyFrame(-16.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(220))));
            animY.KeyFrames.Add(new SplineDoubleKeyFrame(-2.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(340))));
            animY.KeyFrames.Add(new SplineDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(460))));
            animY.Completed += delegate { IniciarAnimacaoFlutuarGotinha(); };
            _ttMascote.BeginAnimation(TranslateTransform.YProperty, animY);

            if (_rtMascote != null)
            {
                DoubleAnimationUsingKeyFrames animRot = new DoubleAnimationUsingKeyFrames();
                animRot.KeyFrames.Add(new SplineDoubleKeyFrame(-4.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(120))));
                animRot.KeyFrames.Add(new SplineDoubleKeyFrame(4.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(260))));
                animRot.KeyFrames.Add(new SplineDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(400))));
                _rtMascote.BeginAnimation(RotateTransform.AngleProperty, animRot);
            }

            CriarParticulaMascote("\u2728", 25, 30);
            CriarParticulaMascote("\uD83D\uDCA7", 95, 25);
            CriarParticulaMascote("\uD83D\uDC96", 60, 15);
        }

        private void CriarParticulaMascote(string emoji, double x, double y)
        {
            if (_canvasMascote == null) return;
            TextBlock p = new TextBlock
            {
                Text = emoji,
                FontSize = 14,
                IsHitTestVisible = false,
                Opacity = 1.0
            };
            Canvas.SetLeft(p, x);
            Canvas.SetTop(p, y);
            _canvasMascote.Children.Add(p);

            DoubleAnimation animSubir = new DoubleAnimation(y, y - 30, TimeSpan.FromMilliseconds(650))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation animFade = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(650));
            animFade.Completed += delegate
            {
                try { _canvasMascote.Children.Remove(p); } catch { }
            };

            p.BeginAnimation(Canvas.TopProperty, animSubir);
            p.BeginAnimation(UIElement.OpacityProperty, animFade);
        }

        // --- Card 3: Lembrete e Timer ---
                private UIElement CriarCardLembrete()
        {
            Border card = CriarCardBase();

            StackPanel sp = new StackPanel();

            // Topo do Timer Zen: Badge Icon + Infos do Timer + Botoes de Acao
            Grid topoGrid = new Grid();
            topoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            topoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Badge Zen com icone suave e respiracao
            _badgeIconeTimer = new Border
            {
                Width = 44,
                Height = 44,
                CornerRadius = new CornerRadius(14),
                Background = _dados.ModoEscuro ? (Brush)new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new LinearGradientBrush(Color.FromRgb(224, 242, 254), Color.FromRgb(186, 230, 253), 45),
                BorderBrush = _dados.ModoEscuro ? new SolidColorBrush(Color.FromArgb(80, 56, 189, 248)) : new SolidColorBrush(Color.FromRgb(125, 211, 252)),
                BorderThickness = new Thickness(1.2),
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1.0, 1.0)
            };

            DoubleAnimation animRespiro = new DoubleAnimation
            {
                From = 0.96,
                To = 1.04,
                Duration = TimeSpan.FromSeconds(2.5),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            _badgeIconeTimer.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animRespiro);
            _badgeIconeTimer.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animRespiro);

            _txtIconeTimer = new TextBlock
            {
                Text = "\u23F3",
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _badgeIconeTimer.Child = _txtIconeTimer;
            Grid.SetColumn(_badgeIconeTimer, 0);
            topoGrid.Children.Add(_badgeIconeTimer);

            // Textos: Rotulo e Display Numerico Zen
            StackPanel timerTextos = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            _txtTimerLabel = new TextBlock
            {
                Text = "Pr\u00F3ximo gole de \u00E1gua em",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = _dados.ModoEscuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139))
            };
            timerTextos.Children.Add(_txtTimerLabel);

            _txtTimerDisplay = new TextBlock
            {
                Text = "30:00",
                FontSize = 26,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 1, 0, 0),
                Foreground = _dados.ModoEscuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199))
            };
            timerTextos.Children.Add(_txtTimerDisplay);
            Grid.SetColumn(timerTextos, 1);
            topoGrid.Children.Add(timerTextos);

            // Acoes do Timer: Pausar e Testar em pilulas harmonicas
            StackPanel btnRow = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            _btnPausarTimer = CriarBotaoPill("\u23F8 Pausar", false, delegate { AlternarPausaTimer(); });
            _btnTestarLembrete = CriarBotaoPill("\uD83D\uDD14 Testar", true, delegate { DispararLembrete(); });
            btnRow.Children.Add(_btnPausarTimer);
            btnRow.Children.Add(_btnTestarLembrete);
            Grid.SetColumn(btnRow, 2);
            topoGrid.Children.Add(btnRow);

            sp.Children.Add(topoGrid);

            // Barra / Capsula Fluida de Progresso Zen do Lembrete
            _trackTimerProgress = new Border
            {
                Height = 6,
                CornerRadius = new CornerRadius(3),
                Background = _dados.ModoEscuro ? new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(224, 242, 254)),
                BorderBrush = _dados.ModoEscuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                BorderThickness = new Thickness(1),
                ClipToBounds = true,
                Margin = new Thickness(0, 10, 0, 10)
            };
            Grid barGrid = new Grid();
            _colTimerAtivo = new ColumnDefinition { Width = new GridLength(0.001, GridUnitType.Star) };
            _colTimerRestante = new ColumnDefinition { Width = new GridLength(100, GridUnitType.Star) };
            barGrid.ColumnDefinitions.Add(_colTimerAtivo);
            barGrid.ColumnDefinitions.Add(_colTimerRestante);

            _fillTimerProgress = new Border
            {
                CornerRadius = new CornerRadius(3),
                Background = new LinearGradientBrush(
                    Color.FromRgb(56, 189, 248),
                    Color.FromRgb(2, 132, 199),
                    0.0
                )
            };
            Grid.SetColumn(_fillTimerProgress, 0);
            barGrid.Children.Add(_fillTimerProgress);
            _trackTimerProgress.Child = barGrid;
            sp.Children.Add(_trackTimerProgress);

            // Seletor de Intervalos Zen (Pilulas arredondadas com microinteracao)
            StackPanel intervalRow = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 2, 0, 0)
            };
            _txtLembrarACada = new TextBlock
            {
                Text = "Lembrar a cada:",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = _dados.ModoEscuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };
            intervalRow.Children.Add(_txtLembrarACada);

            _botoesIntervalo.Clear();
            int[] mins = new int[] { 15, 30, 45, 60 };
            foreach (int m in mins)
            {
                bool ativo = (m == _dados.IntervaloMinutos);
                Button btnM = new Button
                {
                    Content = m + "m",
                    Tag = m,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Padding = new Thickness(12, 5, 12, 5),
                    Margin = new Thickness(3, 0, 3, 0),
                    Cursor = Cursors.Hand,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new ScaleTransform(1.0, 1.0)
                };
                AplicarEstiloPill(btnM, 13);

                if (ativo)
                {
                    btnM.Background = new LinearGradientBrush(Color.FromRgb(56, 189, 248), Color.FromRgb(2, 132, 199), 90);
                    btnM.Foreground = Brushes.White;
                    btnM.BorderBrush = Brushes.Transparent;
                    btnM.Effect = new DropShadowEffect
                    {
                        Color = Color.FromRgb(2, 132, 199),
                        BlurRadius = 8,
                        Opacity = 0.35,
                        ShadowDepth = 2
                    };
                }
                else
                {
                    btnM.Background = _dados.ModoEscuro ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255));
                    btnM.Foreground = _dados.ModoEscuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(3, 105, 161));
                    btnM.BorderBrush = _dados.ModoEscuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                    btnM.BorderThickness = new Thickness(1);
                    btnM.Effect = null;
                }

                btnM.MouseEnter += delegate(object s, MouseEventArgs e)
                {
                    Button b = (Button)s;
                    if (b.RenderTransform is ScaleTransform)
                    {
                        ScaleTransform st = (ScaleTransform)b.RenderTransform;
                        st.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.0, 1.06, TimeSpan.FromMilliseconds(120)));
                        st.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.0, 1.06, TimeSpan.FromMilliseconds(120)));
                    }
                };
                btnM.MouseLeave += delegate(object s, MouseEventArgs e)
                {
                    Button b = (Button)s;
                    if (b.RenderTransform is ScaleTransform)
                    {
                        ScaleTransform st = (ScaleTransform)b.RenderTransform;
                        st.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.06, 1.0, TimeSpan.FromMilliseconds(120)));
                        st.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.06, 1.0, TimeSpan.FromMilliseconds(120)));
                    }
                };

                btnM.Click += delegate(object s, RoutedEventArgs e)
                {
                    Button b = (Button)s;
                    if (b.RenderTransform is ScaleTransform)
                    {
                        ScaleTransform st = (ScaleTransform)b.RenderTransform;
                        DoubleAnimation bounceX = new DoubleAnimation(0.92, 1.0, TimeSpan.FromMilliseconds(200));
                        DoubleAnimation bounceY = new DoubleAnimation(0.92, 1.0, TimeSpan.FromMilliseconds(200));
                        bounceX.EasingFunction = new BounceEase { Bounces = 1, Bounciness = 2 };
                        bounceY.EasingFunction = new BounceEase { Bounces = 1, Bounciness = 2 };
                        st.BeginAnimation(ScaleTransform.ScaleXProperty, bounceX);
                        st.BeginAnimation(ScaleTransform.ScaleYProperty, bounceY);
                    }
                    DefinirIntervalo((int)b.Tag);
                };

                _botoesIntervalo.Add(btnM);
                intervalRow.Children.Add(btnM);
            }

            sp.Children.Add(intervalRow);

            card.Child = sp;
            return card;
        }

        // --- Card 4: Botões de Registro Rápido ---
        public class BebidaInfo
        {
            public int Index;
            public string Icone;
            public string Nome;
            public int Ml;
            public bool Destaque;
            public Border BadgeIcone;
            public TextBlock TxtIcone;
            public TextBlock TxtNome;
            public TextBlock TxtMl;
        }

        // --- Card 4: Botões de Registro Rápido Vivos & Fofos ---
        private UIElement CriarCardBotoes()
        {
            Border card = CriarCardBase();
            card.Padding = new Thickness(14, 12, 14, 12);
            card.Margin = new Thickness(0, 0, 0, 8);

            StackPanel sp = new StackPanel();

            // Header do Card: Título com gota + Botão Desfazer em pílula
            Grid tituloRow = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            tituloRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            tituloRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _txtTituloRegistrar = new TextBlock
            {
                Text = "Registrar \u00C1gua \uD83D\uDCA7",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = _dados.ModoEscuro ? (Brush)Brushes.White : new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(_txtTituloRegistrar, 0);
            tituloRow.Children.Add(_txtTituloRegistrar);

            _btnDesfazer = CriarBotaoPill("\u21A9 Desfazer", true, delegate
            {
                DesfazerUltimo();
            });
            _btnDesfazer.Padding = new Thickness(10, 4, 10, 4);
            _btnDesfazer.FontSize = 11;
            _btnDesfazer.ToolTip = "Desfazer o \u00FAltimo gole registrado";
            Grid.SetColumn(_btnDesfazer, 1);
            tituloRow.Children.Add(_btnDesfazer);

            sp.Children.Add(tituloRow);

            // 4 Botões de Bebidas Vivos em Grid 4 colunas
            UniformGrid gridBotoes = new UniformGrid { Columns = 4, Margin = new Thickness(0, 0, 0, 10) };
            _botoesBebida.Clear();

            Button b1 = CriarBotaoBebida(0, "\u2615", "Gole", 75, false);
            Button b2 = CriarBotaoBebida(1, "\uD83E\uDD5B", "Copo", 200, true);
            Button b3 = CriarBotaoBebida(2, "\uD83E\uDD64", "Grande", 350, false);
            Button b4 = CriarBotaoBebida(3, "\uD83C\uDF76", "Garrafa", 500, false);

            _botoesBebida.Add(b1);
            _botoesBebida.Add(b2);
            _botoesBebida.Add(b3);
            _botoesBebida.Add(b4);

            gridBotoes.Children.Add(b1);
            gridBotoes.Children.Add(b2);
            gridBotoes.Children.Add(b3);
            gridBotoes.Children.Add(b4);

            sp.Children.Add(gridBotoes);

            // Entrada personalizada de ml com container arredondado e botão pill
            Grid customGrid = new Grid();
            customGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            customGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Border customBoxBorder = new Border
            {
                Background = _dados.ModoEscuro ? (Brush)new SolidColorBrush(Color.FromRgb(30, 41, 59)) : Brushes.White,
                BorderBrush = _dados.ModoEscuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12, 2, 12, 2),
                Height = 36
            };

            _txtCustomAgua = new TextBox
            {
                FontSize = 12,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                Foreground = _dados.ModoEscuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                VerticalContentAlignment = VerticalAlignment.Center,
                ToolTip = "Digite um valor em ml (ex: 200, 350, 600)"
            };
            customBoxBorder.Child = _txtCustomAgua;
            Grid.SetColumn(customBoxBorder, 0);
            customGrid.Children.Add(customBoxBorder);

            Button btnAddCustom = CriarBotaoPill("+ Adicionar ml", false, delegate
            {
                if (_txtCustomAgua == null) return;
                int val;
                if (int.TryParse(_txtCustomAgua.Text.Trim(), out val) && val > 0 && val <= 3000)
                {
                    AdicionarAgua(val, "Personalizado (" + val + " ml) \uD83D\uDCA7");
                    AtualizarFalaGotinha("Mais " + val + " ml registrados! Cada gota te faz brilhar! \u2728");
                    ExecutarAnimacaoPuloGotinha();
                    _txtCustomAgua.Text = "";
                }
                else
                {
                    ExibirSnackBar("Digite um n\u00FAmero v\u00E1lido em ml (ex: 200, 350).");
                }
            });
            _txtCustomAgua.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.Key == Key.Enter)
                {
                    btnAddCustom.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    e.Handled = true;
                }
            };
            btnAddCustom.Padding = new Thickness(14, 8, 14, 8);
            btnAddCustom.Margin = new Thickness(8, 0, 0, 0);
            Grid.SetColumn(btnAddCustom, 1);
            customGrid.Children.Add(btnAddCustom);

            sp.Children.Add(customGrid);

            // Aplica os estilos individuais de tema em cada botão
            AtualizarVisualBotoesBebida();

            card.Child = sp;
            return card;
        }

        private Button CriarBotaoBebida(int index, string icone, string nome, int ml, bool destaque = false)
        {
            BebidaInfo info = new BebidaInfo
            {
                Index = index,
                Icone = icone,
                Nome = nome,
                Ml = ml,
                Destaque = destaque
            };

            Button btn = new Button
            {
                Tag = info,
                Margin = new Thickness(3, 0, 3, 0),
                Padding = new Thickness(4, 8, 4, 8),
                BorderThickness = new Thickness(destaque ? 2 : 1.2),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1.0, 1.0),
                Cursor = Cursors.Hand
            };
            AplicarEstiloPill(btn, 14);

            StackPanel sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

            // Badge com ícone
            info.BadgeIcone = new Border
            {
                Width = 38,
                Height = 38,
                CornerRadius = new CornerRadius(12),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4)
            };
            info.TxtIcone = new TextBlock
            {
                Text = icone,
                FontSize = 19,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            info.BadgeIcone.Child = info.TxtIcone;
            sp.Children.Add(info.BadgeIcone);

            // Nome da bebida
            info.TxtNome = new TextBlock
            {
                Text = nome,
                FontSize = 11.5,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 1, 0, 0)
            };
            sp.Children.Add(info.TxtNome);

            // Quantidade em ml
            info.TxtMl = new TextBlock
            {
                Text = "+" + ml + " ml",
                FontSize = 10.5,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 1, 0, 0)
            };
            sp.Children.Add(info.TxtMl);

            btn.Content = sp;

            // Animações de Hover
            btn.MouseEnter += delegate
            {
                if (btn.RenderTransform is ScaleTransform)
                {
                    ScaleTransform st = (ScaleTransform)btn.RenderTransform;
                    DoubleAnimation sa = new DoubleAnimation(1.0, 1.05, TimeSpan.FromMilliseconds(130))
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
                    DoubleAnimation sa = new DoubleAnimation(1.05, 1.0, TimeSpan.FromMilliseconds(130))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, sa);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, sa);
                }
            };

            // Clique com efeito de mola gostoso (Bounce) + feedback fofo da Gotinha
            btn.PreviewMouseDown += delegate
            {
                if (btn.RenderTransform is ScaleTransform)
                {
                    ScaleTransform st = (ScaleTransform)btn.RenderTransform;
                    DoubleAnimation sa = new DoubleAnimation(1.05, 0.93, TimeSpan.FromMilliseconds(70));
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, sa);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, sa);
                }
            };
            btn.PreviewMouseUp += delegate
            {
                if (btn.RenderTransform is ScaleTransform)
                {
                    ScaleTransform st = (ScaleTransform)btn.RenderTransform;
                    DoubleAnimation sa = new DoubleAnimation(0.93, 1.05, TimeSpan.FromMilliseconds(160))
                    {
                        EasingFunction = new BounceEase { Bounces = 1, Bounciness = 2 }
                    };
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, sa);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, sa);
                }
            };

            btn.Click += delegate
            {
                AdicionarAgua(ml, nome + " (" + icone + ")");

                // Fala fofa e personalizada para cada bebida
                string fala = "";
                switch (index)
                {
                    case 0: fala = "Um golinho gostoso para refrescar! Cada gota conta! \u2728"; break;
                    case 1: fala = "Muito bem! Um copo d'\u00E1gua fresquinha faz maravilhas! \uD83E\uDD5B\uD83D\uDCA7"; break;
                    case 2: fala = "Uau, super hidrata\u00E7\u00E3o! Seu corpo est\u00E1 amando! \uD83E\uDD64\uD83C\uDF0A"; break;
                    case 3: fala = "Uma garrafa inteira! Voc\u00EA est\u00E1 de parab\u00E9ns! \uD83C\uDFC6\uD83C\uDF76"; break;
                    default: fala = "Mais " + ml + " ml de \u00E1gua para voc\u00EA! Muito bem! \uD83D\uDCA7"; break;
                }
                AtualizarFalaGotinha(fala);
                ExecutarAnimacaoPuloGotinha();
            };

            return btn;
        }

        private void AtualizarVisualBotoesBebida()
        {
            bool escuro = _dados.ModoEscuro;
            if (_txtTituloRegistrar != null)
                _txtTituloRegistrar.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(15, 23, 42));

            if (_btnDesfazer != null)
            {
                _btnDesfazer.Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(241, 245, 249));
                _btnDesfazer.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(226, 232, 240));
                _btnDesfazer.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(100, 116, 139));
            }

            if (_txtCustomAgua != null && _txtCustomAgua.Parent is Border)
            {
                Border b = (Border)_txtCustomAgua.Parent;
                b.Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(30, 41, 59)) : Brushes.White;
                b.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(203, 213, 225));
                _txtCustomAgua.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(15, 23, 42));
            }

            foreach (var btn in _botoesBebida)
            {
                if (!(btn.Tag is BebidaInfo)) continue;
                BebidaInfo info = (BebidaInfo)btn.Tag;

                if (info.TxtIcone != null)
                {
                    info.TxtIcone.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(15, 23, 42));
                }

                switch (info.Index)
                {
                    case 0: // Gole (Âmbar/Pêssego)
                        btn.Background = escuro ? (Brush)new SolidColorBrush(Color.FromArgb(200, 45, 24, 7)) : new SolidColorBrush(Color.FromRgb(255, 251, 235));
                        btn.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(251, 191, 36)) : new SolidColorBrush(Color.FromRgb(253, 230, 138));
                        if (info.BadgeIcone != null)
                        {
                            info.BadgeIcone.Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(120, 53, 15)) : new SolidColorBrush(Color.FromRgb(254, 243, 199));
                            info.BadgeIcone.BorderBrush = escuro ? new SolidColorBrush(Color.FromArgb(160, 255, 255, 255)) : Brushes.Transparent;
                            info.BadgeIcone.BorderThickness = escuro ? new Thickness(1) : new Thickness(0);
                        }
                        if (info.TxtNome != null)
                            info.TxtNome.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(146, 64, 14));
                        if (info.TxtMl != null)
                            info.TxtMl.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(253, 230, 138)) : new SolidColorBrush(Color.FromRgb(217, 119, 6));
                        btn.Effect = null;
                        break;

                    case 1: // Copo (Azul Celeste - Destaque com glow suave)
                        btn.Background = escuro ? (Brush)new SolidColorBrush(Color.FromArgb(220, 8, 47, 73)) : new SolidColorBrush(Color.FromRgb(240, 249, 255));
                        btn.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                        if (info.BadgeIcone != null)
                        {
                            info.BadgeIcone.Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(12, 74, 110)) : new SolidColorBrush(Color.FromRgb(224, 242, 254));
                            info.BadgeIcone.BorderBrush = escuro ? new SolidColorBrush(Color.FromArgb(160, 255, 255, 255)) : Brushes.Transparent;
                            info.BadgeIcone.BorderThickness = escuro ? new Thickness(1) : new Thickness(0);
                        }
                        if (info.TxtNome != null)
                            info.TxtNome.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(3, 105, 161));
                        if (info.TxtMl != null)
                            info.TxtMl.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(186, 230, 253)) : new SolidColorBrush(Color.FromRgb(2, 132, 199));
                        btn.Effect = new DropShadowEffect
                        {
                            Color = Color.FromRgb(2, 132, 199),
                            BlurRadius = 8,
                            Opacity = escuro ? 0.35 : 0.22,
                            ShadowDepth = 2
                        };
                        break;

                    case 2: // Grande (Turquesa / Aqua)
                        btn.Background = escuro ? (Brush)new SolidColorBrush(Color.FromArgb(200, 4, 47, 46)) : new SolidColorBrush(Color.FromRgb(240, 253, 250));
                        btn.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(45, 212, 191)) : new SolidColorBrush(Color.FromRgb(153, 246, 228));
                        if (info.BadgeIcone != null)
                        {
                            info.BadgeIcone.Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(19, 78, 74)) : new SolidColorBrush(Color.FromRgb(204, 251, 241));
                            info.BadgeIcone.BorderBrush = escuro ? new SolidColorBrush(Color.FromArgb(160, 255, 255, 255)) : Brushes.Transparent;
                            info.BadgeIcone.BorderThickness = escuro ? new Thickness(1) : new Thickness(0);
                        }
                        if (info.TxtNome != null)
                            info.TxtNome.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(15, 118, 110));
                        if (info.TxtMl != null)
                            info.TxtMl.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(153, 246, 228)) : new SolidColorBrush(Color.FromRgb(13, 148, 136));
                        btn.Effect = null;
                        break;

                    case 3: // Garrafa (Menta / Esmeralda)
                        btn.Background = escuro ? (Brush)new SolidColorBrush(Color.FromArgb(200, 5, 46, 22)) : new SolidColorBrush(Color.FromRgb(240, 253, 244));
                        btn.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(52, 211, 153)) : new SolidColorBrush(Color.FromRgb(167, 243, 208));
                        if (info.BadgeIcone != null)
                        {
                            info.BadgeIcone.Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(6, 78, 59)) : new SolidColorBrush(Color.FromRgb(209, 250, 229));
                            info.BadgeIcone.BorderBrush = escuro ? new SolidColorBrush(Color.FromArgb(160, 255, 255, 255)) : Brushes.Transparent;
                            info.BadgeIcone.BorderThickness = escuro ? new Thickness(1) : new Thickness(0);
                        }
                        if (info.TxtNome != null)
                            info.TxtNome.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(4, 120, 87));
                        if (info.TxtMl != null)
                            info.TxtMl.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(167, 243, 208)) : new SolidColorBrush(Color.FromRgb(5, 150, 105));
                        btn.Effect = null;
                        break;
                }
            }
        }

        private Border CriarCardMiniStat(string titulo, out TextBlock txtValor, out TextBlock txtSub, string icone)
        {
            bool escuro = _dados.ModoEscuro;
            Border b = new Border
            {
                Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255)),
                BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(8, 7, 8, 7),
                Margin = new Thickness(3, 0, 3, 0)
            };

            StackPanel sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

            StackPanel topRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            topRow.Children.Add(new TextBlock
            {
                Text = icone + " ",
                FontSize = 10,
                Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                VerticalAlignment = VerticalAlignment.Center
            });
            topRow.Children.Add(new TextBlock
            {
                Text = titulo,
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                VerticalAlignment = VerticalAlignment.Center
            });
            sp.Children.Add(topRow);

            txtValor = new TextBlock
            {
                Text = "--",
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 1, 0, 0)
            };
            sp.Children.Add(txtValor);

            txtSub = new TextBlock
            {
                Text = "",
                FontSize = 9.5,
                Foreground = escuro ? new SolidColorBrush(Color.FromRgb(100, 116, 139)) : new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            sp.Children.Add(txtSub);

            b.Child = sp;
            return b;
        }

        private void SelecionarAbaHistorico(int aba)
        {
            _abaHistoricoAtiva = aba;
            bool escuro = _dados.ModoEscuro;

            if (_btnTabHoje == null || _btnTabSemana == null) return;

            if (aba == 0)
            {
                // Aba Hoje Ativa
                _btnTabHoje.Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(2, 132, 199)) : Brushes.White;
                _btnTabHoje.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(2, 132, 199));
                _btnTabHoje.BorderThickness = new Thickness(0);
                _btnTabHoje.Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(2, 132, 199),
                    BlurRadius = 8,
                    Opacity = 0.18,
                    ShadowDepth = 1.5
                };

                _btnTabSemana.Background = Brushes.Transparent;
                _btnTabSemana.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139));
                _btnTabSemana.BorderThickness = new Thickness(0);
                _btnTabSemana.Effect = null;

                if (_containerTabSemana != null) _containerTabSemana.Visibility = Visibility.Collapsed;
                if (_containerTabHoje != null)
                {
                    _containerTabHoje.Visibility = Visibility.Visible;
                    DoubleAnimation fadeIn = new DoubleAnimation(0.2, 1.0, TimeSpan.FromMilliseconds(200))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    _containerTabHoje.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                }
                AtualizarListaHoje(ObterDiaHoje());
            }
            else
            {
                // Aba Semana Ativa
                _btnTabSemana.Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(2, 132, 199)) : Brushes.White;
                _btnTabSemana.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(2, 132, 199));
                _btnTabSemana.BorderThickness = new Thickness(0);
                _btnTabSemana.Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(2, 132, 199),
                    BlurRadius = 8,
                    Opacity = 0.18,
                    ShadowDepth = 1.5
                };

                _btnTabHoje.Background = Brushes.Transparent;
                _btnTabHoje.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139));
                _btnTabHoje.BorderThickness = new Thickness(0);
                _btnTabHoje.Effect = null;

                if (_containerTabHoje != null) _containerTabHoje.Visibility = Visibility.Collapsed;
                if (_containerTabSemana != null)
                {
                    _containerTabSemana.Visibility = Visibility.Visible;
                    DoubleAnimation fadeIn = new DoubleAnimation(0.2, 1.0, TimeSpan.FromMilliseconds(200))
                    {
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };
                    _containerTabSemana.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                }
                AtualizarGraficoSemanal();
            }

            AtualizarEstatisticasHistorico();
        }

        private void AtualizarEstatisticasHistorico()
        {
            DadosDia diaHoje = ObterDiaHoje();
            int totalHoje = diaHoje.TotalMl;
            int meta = Math.Max(1, _dados.MetaDiaria);
            int pctHoje = (totalHoje * 100) / meta;

            if (_txtStatsHojeVal != null) _txtStatsHojeVal.Text = totalHoje + " ml";
            if (_txtStatsHojeSub != null) _txtStatsHojeSub.Text = pctHoje + "% da meta";

            int soma7 = 0;
            int metasBatidas = 0;
            DateTime hj = DateTime.Today;
            for (int i = 0; i < 7; i++)
            {
                DateTime d = hj.AddDays(-i);
                string dStr = d.ToString("yyyy-MM-dd");
                DadosDia diaSalvo = _dados.Dias.Find(delegate(DadosDia x) { return x.Data == dStr; });
                if (diaSalvo != null)
                {
                    soma7 += diaSalvo.TotalMl;
                    if (diaSalvo.TotalMl >= _dados.MetaDiaria) metasBatidas++;
                }
            }

            double media = soma7 / 7.0;
            if (_txtStatsMediaVal != null)
            {
                _txtStatsMediaVal.Text = (media >= 1000) ? (media / 1000.0).ToString("0.1") + " L" : Math.Round(media) + " ml";
            }
            if (_txtStatsMetasVal != null)
            {
                _txtStatsMetasVal.Text = metasBatidas + " de 7";
            }
        }

        private UIElement CriarCardHistorico()
        {
            Border card = CriarCardBase();
            card.Padding = new Thickness(14, 12, 14, 12);

            Grid mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 0: Stats Resumo
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row 1: Segmented Pill Switch
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Row 2: Conteudo das Abas

            // Row 0: 3 Cards de Resumo Rapido
            Grid statsGrid = new Grid { Margin = new Thickness(0, 0, 0, 10) };
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            _cardStatsHoje = CriarCardMiniStat("Total Hoje", out _txtStatsHojeVal, out _txtStatsHojeSub, "\uD83D\uDCA7");
            Grid.SetColumn(_cardStatsHoje, 0);
            statsGrid.Children.Add(_cardStatsHoje);

            TextBlock subMedia;
            _cardStatsMedia = CriarCardMiniStat("M\u00E9dia 7d", out _txtStatsMediaVal, out subMedia, "\uD83D\uDCC8");
            subMedia.Text = "por dia";
            Grid.SetColumn(_cardStatsMedia, 1);
            statsGrid.Children.Add(_cardStatsMedia);

            TextBlock subMetas;
            _cardStatsMetas = CriarCardMiniStat("Dias na Meta", out _txtStatsMetasVal, out subMetas, "\u2B50");
            subMetas.Text = "de 7 dias";
            Grid.SetColumn(_cardStatsMetas, 2);
            statsGrid.Children.Add(_cardStatsMetas);

            Grid.SetRow(statsGrid, 0);
            mainGrid.Children.Add(statsGrid);

            // Row 1: Segmented Pill Switch
            _pillsSegmentedContainer = new Border
            {
                Height = 36,
                CornerRadius = new CornerRadius(14),
                Background = _dados.ModoEscuro ? (Brush)new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(224, 242, 254)),
                Padding = new Thickness(3),
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid pillsGrid = new Grid();
            pillsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            pillsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            _btnTabHoje = new Button
            {
                Content = "\uD83D\uDCA7 Registros de Hoje",
                FontSize = 11.5,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand
            };
            AplicarEstiloPill(_btnTabHoje, 11);
            _btnTabHoje.Click += delegate { SelecionarAbaHistorico(0); };
            Grid.SetColumn(_btnTabHoje, 0);
            pillsGrid.Children.Add(_btnTabHoje);

            _btnTabSemana = new Button
            {
                Content = "\uD83D\uDCCA Gr\u00E1fico 7 Dias",
                FontSize = 11.5,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand
            };
            AplicarEstiloPill(_btnTabSemana, 11);
            _btnTabSemana.Click += delegate { SelecionarAbaHistorico(1); };
            Grid.SetColumn(_btnTabSemana, 1);
            pillsGrid.Children.Add(_btnTabSemana);

            _pillsSegmentedContainer.Child = pillsGrid;
            Grid.SetRow(_pillsSegmentedContainer, 1);
            mainGrid.Children.Add(_pillsSegmentedContainer);

            // Row 2: Conteudo das Abas
            Grid tabContentGrid = new Grid();

            // Painel Hoje: ScrollViewer com lista de registros
            ScrollViewer scrollHoje = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };
            scrollHoje.PreviewMouseWheel += delegate(object s, MouseWheelEventArgs e)
            {
                ScrollViewer scv = s as ScrollViewer;
                if (scv != null && scv.ScrollableHeight > 0)
                {
                    scv.ScrollToVerticalOffset(scv.VerticalOffset - (e.Delta > 0 ? 45 : -45));
                    e.Handled = true;
                }
            };
            _painelHistorico = new StackPanel { Margin = new Thickness(0, 2, 0, 4) };
            scrollHoje.Content = _painelHistorico;
            _containerTabHoje = scrollHoje;
            tabContentGrid.Children.Add(_containerTabHoje);

            // Painel Semana: Grafico semanal + Legenda
            ScrollViewer scrollSemana = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };
            scrollSemana.PreviewMouseWheel += delegate(object s, MouseWheelEventArgs e)
            {
                ScrollViewer scv = s as ScrollViewer;
                if (scv != null && scv.ScrollableHeight > 0)
                {
                    scv.ScrollToVerticalOffset(scv.VerticalOffset - (e.Delta > 0 ? 45 : -45));
                    e.Handled = true;
                }
            };
            _painelGraficoSemanal = new StackPanel { Margin = new Thickness(0, 2, 0, 4) };
            scrollSemana.Content = _painelGraficoSemanal;
            _containerTabSemana = scrollSemana;
            tabContentGrid.Children.Add(_containerTabSemana);

            Grid.SetRow(tabContentGrid, 2);
            mainGrid.Children.Add(tabContentGrid);

            // Inicializa na aba 0 (Hoje)
            SelecionarAbaHistorico(0);

            card.Child = mainGrid;
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
                    LumiDialog.MostrarSucesso(this, "PARAB\u00C9NS! \uD83C\uDF89\uD83D\uDCA7\nVoc\u00EA atingiu sua meta di\u00E1ria de hidrata\u00E7\u00E3o!", "Meta Batida! \uD83C\uDFC6", _dados != null && _dados.ModoEscuro);
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
                AtualizarFalaGotinha("Tudo bem, último gole desfeito com sucesso! ✨");
                if (_dados.SomAtivado) GeradorDeSom.TocarGota();
                ExecutarAnimacaoPuloGotinha();
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
                if (_dados.ModoEscuro)
                {
                    _txtFaltaMl.Foreground = new SolidColorBrush(Color.FromRgb(52, 211, 153));
                    if (_cardStatusFalta != null)
                    {
                        _cardStatusFalta.Background = new SolidColorBrush(Color.FromRgb(6, 46, 26));
                        _cardStatusFalta.BorderBrush = new SolidColorBrush(Color.FromRgb(5, 150, 105));
                    }
                    if (_badgePorcentagem != null)
                    {
                        _badgePorcentagem.Background = new SolidColorBrush(Color.FromRgb(6, 46, 26));
                        _badgePorcentagem.BorderBrush = new SolidColorBrush(Color.FromRgb(5, 150, 105));
                        _txtPorcentagem.Foreground = new SolidColorBrush(Color.FromRgb(52, 211, 153));
                    }
                }
                else
                {
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

            if (_widgetWindow != null)
            {
                _widgetWindow.AtualizarDadosWidget();
            }
        }

        private void AtualizarListaHoje(DadosDia dia)
        {
            _painelHistorico.Children.Clear();
            bool escuro = _dados.ModoEscuro;

            if (dia.Registros.Count == 0)
            {
                Border emptyCard = new Border
                {
                    Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255)),
                    BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(14),
                    Padding = new Thickness(16, 20, 16, 20),
                    Margin = new Thickness(0, 8, 0, 8)
                };

                StackPanel spEmpty = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };

                Border iconCircle = new Border
                {
                    Width = 46,
                    Height = 46,
                    CornerRadius = new CornerRadius(23),
                    Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new LinearGradientBrush(Color.FromRgb(224, 242, 254), Color.FromRgb(186, 230, 253), 45),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 10)
                };
                iconCircle.Child = new TextBlock
                {
                    Text = "\uD83C\uDF31",
                    FontSize = 22,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                spEmpty.Children.Add(iconCircle);

                spEmpty.Children.Add(new TextBlock
                {
                    Text = "Nenhum gole registrado ainda hoje",
                    FontSize = 13,
                    FontWeight = FontWeights.Bold,
                    Foreground = escuro ? new SolidColorBrush(Color.FromRgb(248, 250, 252)) : new SolidColorBrush(Color.FromRgb(15, 23, 42)),
                    HorizontalAlignment = HorizontalAlignment.Center
                });

                spEmpty.Children.Add(new TextBlock
                {
                    Text = "Que tal tomar seu primeiro copo de \u00E1gua agora mesmo?\nSeu corpo e a Lumi agradecem! \u2728",
                    FontSize = 11,
                    Foreground = escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 4, 0, 12)
                });

                Button btnBeberAgora = CriarBotaoPill("\uD83E\uDD64 Beber Copo (200 ml)", false, delegate
                {
                    AdicionarAgua(200, "Copo");
                });
                btnBeberAgora.HorizontalAlignment = HorizontalAlignment.Center;
                spEmpty.Children.Add(btnBeberAgora);

                emptyCard.Child = spEmpty;
                _painelHistorico.Children.Add(emptyCard);
                return;
            }

            // Cabecalho dos registros
            Grid headerRow = new Grid { Margin = new Thickness(2, 4, 2, 8) };
            headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock txtContador = new TextBlock
            {
                Text = "Goles registrados hoje (" + dia.Registros.Count + ")",
                FontSize = 11.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139))
            };
            Grid.SetColumn(txtContador, 0);
            headerRow.Children.Add(txtContador);

            TextBlock txtTotalHoje = new TextBlock
            {
                Text = dia.TotalMl + " ml no total",
                FontSize = 11.5,
                FontWeight = FontWeights.Bold,
                Foreground = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199))
            };
            Grid.SetColumn(txtTotalHoje, 1);
            headerRow.Children.Add(txtTotalHoje);

            _painelHistorico.Children.Add(headerRow);

            int index = 0;
            foreach (var r in dia.Registros)
            {
                Border item = new Border
                {
                    Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255)),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(10, 8, 12, 8),
                    Margin = new Thickness(0, 3, 0, 3),
                    BorderThickness = new Thickness(1),
                    BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                    Cursor = Cursors.Hand,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new TranslateTransform(0, 0)
                };

                Grid g = new Grid();
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // Icone e Badge Colorida
                string iconeStr = "\uD83D\uDCA7";
                Brush badgeBg = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(224, 242, 254));
                if (r.Quantidade <= 100)
                {
                    iconeStr = "\u2615";
                    badgeBg = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(69, 26, 3)) : new SolidColorBrush(Color.FromRgb(254, 243, 199));
                }
                else if (r.Quantidade <= 220)
                {
                    iconeStr = "\uD83E\uDD64";
                    badgeBg = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(12, 74, 110)) : new SolidColorBrush(Color.FromRgb(224, 242, 254));
                }
                else if (r.Quantidade <= 350)
                {
                    iconeStr = "\uD83E\uDD63";
                    badgeBg = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(19, 78, 74)) : new SolidColorBrush(Color.FromRgb(224, 247, 250));
                }
                else if (r.Quantidade <= 500)
                {
                    iconeStr = "\uD83C\uDF76";
                    badgeBg = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(6, 78, 59)) : new SolidColorBrush(Color.FromRgb(209, 250, 229));
                }

                Border badgeIcon = new Border
                {
                    Width = 34,
                    Height = 34,
                    CornerRadius = new CornerRadius(10),
                    Background = badgeBg,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                badgeIcon.Child = new TextBlock
                {
                    Text = iconeStr,
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(badgeIcon, 0);
                g.Children.Add(badgeIcon);

                // Informacoes do Gole (Quantidade e Tipo)
                StackPanel infoStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                infoStack.Children.Add(new TextBlock
                {
                    Text = "+" + r.Quantidade + " ml",
                    FontWeight = FontWeights.Bold,
                    FontSize = 13,
                    Foreground = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199))
                });
                infoStack.Children.Add(new TextBlock
                {
                    Text = r.Tipo,
                    FontSize = 10.5,
                    Foreground = escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139))
                });
                Grid.SetColumn(infoStack, 1);
                g.Children.Add(infoStack);

                // Horario em pilula delicada
                Border pillHora = new Border
                {
                    Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(7, 3, 7, 3),
                    VerticalAlignment = VerticalAlignment.Center
                };
                pillHora.Child = new TextBlock
                {
                    Text = "\u23F0 " + r.Hora,
                    FontSize = 10.5,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139))
                };
                Grid.SetColumn(pillHora, 2);
                g.Children.Add(pillHora);

                // Hover sutil
                item.MouseEnter += delegate
                {
                    item.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199));
                };
                item.MouseLeave += delegate
                {
                    item.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                };

                // Entrada suave escalonada
                TranslateTransform ttItem = (TranslateTransform)item.RenderTransform;
                ttItem.Y = 6;
                item.Opacity = 0.0;

                DoubleAnimation animOp = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(220))
                {
                    BeginTime = TimeSpan.FromMilliseconds(Math.Min(index * 35, 350)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                DoubleAnimation animY = new DoubleAnimation(6, 0, TimeSpan.FromMilliseconds(220))
                {
                    BeginTime = TimeSpan.FromMilliseconds(Math.Min(index * 35, 350)),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                item.BeginAnimation(UIElement.OpacityProperty, animOp);
                ttItem.BeginAnimation(TranslateTransform.YProperty, animY);

                item.Child = g;
                _painelHistorico.Children.Add(item);
                index++;
            }
        }

        private void AtualizarGraficoSemanal()
        {
            _painelGraficoSemanal.Children.Clear();
            bool escuro = _dados.ModoEscuro;

            // Grid das 7 Colunas
            UniformGrid colunas = new UniformGrid
            {
                Columns = 7,
                Height = 175,
                Margin = new Thickness(0, 6, 0, 8)
            };

            DateTime hoje = DateTime.Today;
            int maxVal = Math.Max(_dados.MetaDiaria, 2000);

            for (int i = 6; i >= 0; i--)
            {
                DateTime d = hoje.AddDays(-i);
                string dataStr = d.ToString("yyyy-MM-dd");
                DadosDia diaSalvo = _dados.Dias.Find(delegate(DadosDia x) { return x.Data == dataStr; });
                int ml = (diaSalvo != null) ? diaSalvo.TotalMl : 0;
                bool metaAtingida = (ml >= _dados.MetaDiaria);

                StackPanel col = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                // Tooltip amigavel e completo
                col.ToolTip = string.Format(
                    "{0:dddd, dd/MM}\nVolume: {1} ml ({2}% da meta){3}",
                    d, ml, (ml * 100) / Math.Max(1, _dados.MetaDiaria),
                    metaAtingida ? "\n\u2B50 Parab\u00E9ns! Meta Batida!" : ""
                );

                // Estrela se bateu a meta
                TextBlock starIcon = new TextBlock
                {
                    Text = metaAtingida ? "\u2B50" : " ",
                    FontSize = 10,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 2)
                };
                col.Children.Add(starIcon);

                // Volume em cima da barra
                string volTexto;
                if (ml <= 0) volTexto = "0";
                else if (ml >= 1000) volTexto = (ml / 1000.0).ToString("0.1") + "L";
                else volTexto = ml.ToString();

                TextBlock txtVol = new TextBlock
                {
                    Text = volTexto,
                    FontSize = 9.5,
                    FontWeight = metaAtingida ? FontWeights.Bold : FontWeights.Normal,
                    Foreground = metaAtingida ? new SolidColorBrush(Color.FromRgb(16, 185, 129)) : (escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139))),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 3)
                };
                col.Children.Add(txtVol);

                // Capsula Track (Fundo da Barra)
                Border trackBarra = new Border
                {
                    Width = 26,
                    Height = 110,
                    CornerRadius = new CornerRadius(13),
                    Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                    BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                    BorderThickness = new Thickness(1),
                    ClipToBounds = true
                };

                Grid trackGrid = new Grid { VerticalAlignment = VerticalAlignment.Bottom };

                double alturaCalculada = Math.Min(110.0, (ml / (double)maxVal) * 110.0);

                Border barra = new Border
                {
                    Width = 26,
                    Height = 0,
                    CornerRadius = new CornerRadius(13),
                    VerticalAlignment = VerticalAlignment.Bottom
                };

                if (metaAtingida)
                {
                    barra.Background = new LinearGradientBrush(
                        Color.FromRgb(52, 211, 153),
                        Color.FromRgb(16, 185, 129),
                        90
                    );
                }
                else if (ml > 0)
                {
                    barra.Background = new LinearGradientBrush(
                        Color.FromRgb(56, 189, 248),
                        Color.FromRgb(2, 132, 199),
                        90
                    );
                }
                else
                {
                    barra.Background = Brushes.Transparent;
                }

                trackGrid.Children.Add(barra);
                trackBarra.Child = trackGrid;
                col.Children.Add(trackBarra);

                // Label do Dia
                string[] diasAbrev = new string[] { "Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "S\u00E1b" };
                string nomeDia = (i == 0) ? "Hoje" : diasAbrev[(int)d.DayOfWeek];

                StackPanel diaStack = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                TextBlock txtDia = new TextBlock
                {
                    Text = nomeDia,
                    FontSize = 10.5,
                    FontWeight = (i == 0) ? FontWeights.Bold : FontWeights.Normal,
                    Foreground = (i == 0) ? (escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199))) : (escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139))),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                diaStack.Children.Add(txtDia);

                if (i == 0)
                {
                    // Pontinho indicador de hoje
                    diaStack.Children.Add(new Ellipse
                    {
                        Width = 4,
                        Height = 4,
                        Fill = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199)),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 1, 0, 0)
                    });
                }
                col.Children.Add(diaStack);

                // Animacao de crescimento suave
                DoubleAnimation animBarra = new DoubleAnimation(0, Math.Max(ml > 0 ? 8 : 0, alturaCalculada), TimeSpan.FromMilliseconds(520))
                {
                    BeginTime = TimeSpan.FromMilliseconds((6 - i) * 55),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                barra.BeginAnimation(FrameworkElement.HeightProperty, animBarra);

                colunas.Children.Add(col);
            }

            _painelGraficoSemanal.Children.Add(colunas);

            // Legenda do Grafico
            Border cardLegenda = new Border
            {
                Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255)),
                BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(12, 7, 12, 7),
                Margin = new Thickness(4, 6, 4, 2)
            };

            WrapPanel wpLegenda = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Center };

            // Item 1: Meta batida
            wpLegenda.Children.Add(new Ellipse { Width = 8, Height = 8, Fill = new SolidColorBrush(Color.FromRgb(16, 185, 129)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 4, 0) });
            wpLegenda.Children.Add(new TextBlock { Text = "Meta Batida", FontSize = 10, FontWeight = FontWeights.SemiBold, Foreground = escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0) });

            // Item 2: Em progresso
            wpLegenda.Children.Add(new Ellipse { Width = 8, Height = 8, Fill = new SolidColorBrush(Color.FromRgb(2, 132, 199)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 4, 0) });
            wpLegenda.Children.Add(new TextBlock { Text = "Em Progresso", FontSize = 10, FontWeight = FontWeights.SemiBold, Foreground = escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139)), VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 0) });

            // Item 3: Meta configurada
            wpLegenda.Children.Add(new TextBlock { Text = "\uD83C\uDFAF Meta: " + _dados.MetaDiaria + " ml/dia", FontSize = 10, FontWeight = FontWeights.Bold, Foreground = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199)), VerticalAlignment = VerticalAlignment.Center });

            cardLegenda.Child = wpLegenda;
            _painelGraficoSemanal.Children.Add(cardLegenda);
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
            if (_txtTimerDisplay != null)
            {
                _txtTimerDisplay.Text = string.Format("{0:D2}:{1:D2}", min, seg);
            }

            if (_colTimerAtivo != null && _colTimerRestante != null)
            {
                int total = Math.Max(1, _dados.IntervaloMinutos * 60);
                int decorrido = Math.Max(0, total - _segundosRestantes);
                double frac = Math.Max(0.001, Math.Min(1.0, (double)decorrido / total));
                _colTimerAtivo.Width = new GridLength(frac, GridUnitType.Star);
                _colTimerRestante.Width = new GridLength(Math.Max(0.001, 1.0 - frac), GridUnitType.Star);
            }

            if (_widgetWindow != null)
            {
                _widgetWindow.AtualizarTimerWidget(string.Format("{0:D2}:{1:D2}", min, seg), _timerPausado);
            }
        }

        private void AtualizarEstiloTimer()
        {
            bool escuro = _dados.ModoEscuro;
            if (_timerPausado)
            {
                if (_btnPausarTimer != null)
                {
                    _btnPausarTimer.Content = "\u25B6 Continuar";
                    _btnPausarTimer.Background = escuro ? new SolidColorBrush(Color.FromRgb(120, 53, 15)) : new SolidColorBrush(Color.FromRgb(254, 243, 199));
                    _btnPausarTimer.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(217, 119, 6)) : new SolidColorBrush(Color.FromRgb(253, 230, 138));
                    _btnPausarTimer.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(180, 83, 9));
                }
                if (_badgeIconeTimer != null)
                {
                    _badgeIconeTimer.Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new LinearGradientBrush(Color.FromRgb(254, 243, 199), Color.FromRgb(253, 230, 138), 45);
                    _badgeIconeTimer.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(100, 116, 139)) : new SolidColorBrush(Color.FromRgb(245, 158, 11));
                }
                if (_txtIconeTimer != null)
                {
                    _txtIconeTimer.Text = "\uD83D\uDCA4";
                    _txtIconeTimer.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(180, 83, 9));
                }
                if (_txtTimerLabel != null)
                {
                    _txtTimerLabel.Text = "Lembretes em pausa para descanso \uD83D\uDCA4";
                    _txtTimerLabel.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(251, 191, 36)) : new SolidColorBrush(Color.FromRgb(217, 119, 6));
                }
                if (_fillTimerProgress != null)
                {
                    _fillTimerProgress.Background = new SolidColorBrush(Color.FromRgb(245, 158, 11));
                }
            }
            else
            {
                if (_btnPausarTimer != null)
                {
                    _btnPausarTimer.Content = "\u23F8 Pausar";
                    _btnPausarTimer.Background = escuro ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255));
                    _btnPausarTimer.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                    _btnPausarTimer.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(3, 105, 161));
                }
                if (_badgeIconeTimer != null)
                {
                    _badgeIconeTimer.Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new LinearGradientBrush(Color.FromRgb(224, 242, 254), Color.FromRgb(186, 230, 253), 45);
                    _badgeIconeTimer.BorderBrush = escuro ? new SolidColorBrush(Color.FromArgb(80, 56, 189, 248)) : new SolidColorBrush(Color.FromRgb(125, 211, 252));
                }
                if (_txtIconeTimer != null)
                {
                    _txtIconeTimer.Text = "\u23F3";
                    _txtIconeTimer.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(3, 105, 161));
                }
                if (_txtTimerLabel != null)
                {
                    _txtTimerLabel.Text = "Pr\u00F3ximo gole de \u00E1gua em";
                    _txtTimerLabel.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139));
                }
                if (_fillTimerProgress != null)
                {
                    _fillTimerProgress.Background = new LinearGradientBrush(Color.FromRgb(56, 189, 248), Color.FromRgb(2, 132, 199), 0.0);
                }
            }
        }

        private void AtualizarBotoesIntervalo()
        {
            bool escuro = _dados.ModoEscuro;
            foreach (var b in _botoesIntervalo)
            {
                int m = (int)b.Tag;
                bool ativo = (m == _dados.IntervaloMinutos);
                if (ativo)
                {
                    b.Background = new LinearGradientBrush(Color.FromRgb(56, 189, 248), Color.FromRgb(2, 132, 199), 90);
                    b.Foreground = Brushes.White;
                    b.BorderBrush = Brushes.Transparent;
                    b.Effect = new DropShadowEffect
                    {
                        Color = Color.FromRgb(2, 132, 199),
                        BlurRadius = 8,
                        Opacity = 0.35,
                        ShadowDepth = 2
                    };
                }
                else
                {
                    b.Background = escuro ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255));
                    b.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(3, 105, 161));
                    b.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                    b.Effect = null;
                }
            }
        }

        private void AtualizarFalaGotinha(string msg)
        {
            if (_txtMensagemGotinha != null)
            {
                _txtMensagemGotinha.Text = msg;
            }
        }

        private void AlternarPausaTimer()
        {
            _timerPausado = !_timerPausado;
            AtualizarEstiloTimer();
            if (_timerPausado)
            {
                ExibirSnackBar("Lembretes pausados.");
                AtualizarFalaGotinha("Pode descansar! Eu guardo seu tempo aqui \uD83D\uDCA4");
            }
            else
            {
                ExibirSnackBar("Lembretes retomados.");
                AtualizarFalaGotinha("De volta ao ritmo! Pronta para mais um gole? \u2728");
                ExecutarAnimacaoPuloGotinha();
                if (_dados.SomAtivado)
                {
                    GeradorDeSom.TocarGota();
                }
            }
        }

        private void DefinirIntervalo(int minutos)
        {
            _dados.IntervaloMinutos = minutos;
            _segundosRestantes = minutos * 60;
            SalvarDados();
            AtualizarDisplayTimer();
            AtualizarBotoesIntervalo();

            string fala = "";
            switch (minutos)
            {
                case 15: fala = "Intervalo de 15 minutinhos ativado! Super hidratada hoje! \uD83C\uDF0A"; break;
                case 30: fala = "Perfeito! Vou te avisar a cada 30 minutos com carinho! \uD83D\uDCA7"; break;
                case 45: fala = "45 minutos configurado! Ritmo zen e produtivo! \uD83E\uDDD8"; break;
                case 60: fala = "Lembrete a cada 1 hora ativado! Te chamo certinho! \u2728"; break;
                default: fala = "Intervalo de " + minutos + " minutos configurado! \uD83D\uDCA7"; break;
            }
            AtualizarFalaGotinha(fala);
            ExecutarAnimacaoPuloGotinha();
            if (_dados.SomAtivado)
            {
                GeradorDeSom.TocarGota();
            }
            ExibirSnackBar("Intervalo definido para " + minutos + " minutos!");
        }

        public void DispararLembrete()
        {
            if (_dados.SomAtivado)
            {
                GeradorDeSom.TocarSinoLembrete();
            }

            // Exibir Notificacao Nativa do Windows (na Central de Acoes e na Bandeja)
            if (_dados.NotificacoesAtivadas)
            {
                NotificadorWindows.EnviarNotificacao(
                    "Lumi - Hora de Beber \u00C1gua! \uD83D\uDCA7",
                    "Fa\u00E7a uma pausa r\u00E1pida para um gole d'\u00E1gua agora mesmo e cuide do seu corpo!",
                    _trayIcon
                );
            }

            AtualizarFalaGotinha("Ding-dong! Hora de tomar um belo gole d'\u00E1gua! \uD83D\uDCA7\u2728");
            ExecutarAnimacaoPuloGotinha();

            if (_widgetWindow != null && _widgetWindow.IsVisible)
            {
                _widgetWindow.AlertarLembreteWidget();
                return;
            }

            // Exibir popup modal interativa se a janela estiver visível
            if (WindowState != WindowState.Minimized && IsVisible)
            {
                LembreteAlertaWindow alerta = new LembreteAlertaWindow(this);
                alerta.Owner = this;
                alerta.ShowDialog();
            }
        }

        public void AtivarModoWidget()
        {
            if (_widgetWindow == null)
            {
                _widgetWindow = new WidgetWindow(this);
            }
            _widgetWindow.AplicarTemaWidget();
            _widgetWindow.AtualizarDadosWidget();
            AtualizarDisplayTimer();

            Hide();
            _widgetWindow.MostrarNoCanto();
        }

        public void SairModoWidget()
        {
            if (_widgetWindow != null && _widgetWindow.IsVisible)
            {
                _widgetWindow.Hide();
            }
            Show();
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            Activate();
            AtualizarInterface();
        }

        // ==========================================
        // Bandeja do Sistema (System Tray)
        // ==========================================
        private void ConfigurarBandejaSistema()
        {
            _trayIcon = new Forms.NotifyIcon();
            _trayIcon.Text = "Lumi - Lembrete de \u00C1gua \uD83D\uDCA7";

            string icoPath = IconeHelper.ObterCaminhoIcone();
            if (!string.IsNullOrEmpty(icoPath) && File.Exists(icoPath))
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
                try
                {
                    _trayIcon.Icon = Drawing.Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule.FileName);
                }
                catch
                {
                    _trayIcon.Icon = Drawing.SystemIcons.Application;
                }
            }

            _trayIcon.Visible = true;

            Forms.ContextMenuStrip menu = new Forms.ContextMenuStrip();
            menu.Items.Add("Abrir Lumi \uD83D\uDCA7", null, delegate { SairModoWidget(); });
            menu.Items.Add("Modo Widget (Canto da Tela) 📌", null, delegate { AtivarModoWidget(); });
            menu.Items.Add("Beber Copo (+200ml)", null, delegate { AdicionarAgua(200, "Copo (🥛)"); });
            menu.Items.Add("Testar Notificação Nativa", null, delegate { DispararLembrete(); });
            menu.Items.Add("-");
            menu.Items.Add("Sair", null, delegate { _trayIcon.Visible = false; Application.Current.Shutdown(); });

            _trayIcon.ContextMenuStrip = menu;
            _trayIcon.DoubleClick += delegate { SairModoWidget(); };
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_widgetWindow != null)
            {
                try { _widgetWindow.Close(); } catch { }
                _widgetWindow = null;
            }
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
                "Voc\u00EA \u00E9 incr\u00EDvel! Vamos bater essa meta juntas! \uD83D\uDC96",
                "Glup glup! Cada gole deixa seu corpo mais feliz! \uD83D\uDCA6",
                "Cuidar de voc\u00EA \u00E9 a minha miss\u00E3o favorita! \u2728\uD83D\uDC99"
            };
            Random rnd = new Random();
            if (_txtMensagemGotinha != null)
            {
                _txtMensagemGotinha.Text = falas[rnd.Next(falas.Length)];
                if (_speechBubble != null)
                {
                    DoubleAnimation popAnim = new DoubleAnimation(0.4, 1.0, TimeSpan.FromMilliseconds(220));
                    _speechBubble.BeginAnimation(UIElement.OpacityProperty, popAnim);
                }
            }
            if (_dados.SomAtivado) GeradorDeSom.TocarGota(); ExecutarAnimacaoPuloGotinha();
            ExecutarAnimacaoPuloGotinha();
            ExibirSnackBar("Lumi ficou muito feliz com o seu carinho! \uD83D\uDCA7\u2728");
        }

        private void AtualizarVisualBtnSom()
        {
            if (_btnSom != null)
            {
                bool escuro = (_dados != null && _dados.ModoEscuro);
                _btnSom.Content = CriarIconeSom(_dados.SomAtivado, escuro);
                _btnSom.ToolTip = _dados.SomAtivado ? "Som ativado (clique para silenciar)" : "Som silenciado (clique para ativar)";
            }
        }

        private void AtualizarVisualBtnNotif()
        {
            if (_btnNotif != null)
            {
                bool escuro = (_dados != null && _dados.ModoEscuro);
                _btnNotif.Content = CriarIconeNotif(_dados.NotificacoesAtivadas, escuro);
                _btnNotif.ToolTip = _dados.NotificacoesAtivadas ? "Notifica\u00E7\u00F5es ativadas (clique para pausar)" : "Notifica\u00E7\u00F5es desativadas (clique para ativar)";
            }
        }

        private void AlternarSom()
        {
            _dados.SomAtivado = !_dados.SomAtivado;
            AtualizarVisualBtnSom();
            SalvarDados();
            if (_dados.SomAtivado) GeradorDeSom.TocarGota(); ExecutarAnimacaoPuloGotinha();
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
                AplicarTema();
                AtualizarInterface();
                AtualizarVisualBtnSom();
                AtualizarVisualBtnNotif();
            });
            cfg.Owner = this;
            cfg.ShowDialog();
        }

        private void AbrirJanelaRelatorioSaude()
        {
            RelatorioSaudeWindow rw = new RelatorioSaudeWindow(_dados);
            rw.Owner = this;
            rw.ShowDialog();
        }

        // ==========================================
        // Persistência em JSON e Estados
        // ==========================================
        public DadosDia ObterDiaHoje()
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

        private static UIElement CriarIconeMaximizar(Brush cor, bool maximizado, bool escuro = false)
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
                    Width = 7, Height = 7, Stroke = cor, StrokeThickness = 1.1, Fill = escuro ? new SolidColorBrush(Color.FromRgb(15, 23, 42)) : Brushes.White
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

        private static UIElement CriarIconeEngrenagem(Brush cor = null)
        {
            if (cor == null) cor = new SolidColorBrush(Color.FromRgb(71, 85, 105));
            Viewbox vb = new Viewbox { Width = 17, Height = 17, Stretch = Stretch.Uniform };
            System.Windows.Shapes.Path p = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse("M 12,15 C 10.34,15 9,13.66 9,12 C 9,10.34 10.34,9 12,9 C 13.66,9 15,10.34 15,12 C 15,13.66 13.66,15 12,15 Z M 19.4,13 C 19.45,12.67 19.5,12.34 19.5,12 C 19.5,11.66 19.45,11.33 19.4,11 L 21.54,9.33 C 21.73,9.18 21.78,8.91 21.66,8.69 L 19.66,5.23 C 19.54,5.01 19.28,4.93 19.05,5.02 L 16.53,6.03 C 16,5.62 15.43,5.28 14.8,5.03 L 14.42,2.35 C 14.39,2.11 14.18,1.93 13.94,1.93 L 9.94,1.93 C 9.7,1.93 9.5,2.11 9.46,2.35 L 9.08,5.03 C 8.45,5.28 7.88,5.62 7.35,6.03 L 4.83,5.02 C 4.6,4.93 4.34,5.01 4.22,5.23 L 2.22,8.69 C 2.1,8.91 2.15,9.18 2.34,9.33 L 4.48,11 C 4.43,11.33 4.38,11.67 4.38,12 C 4.38,12.33 4.43,12.67 4.48,13 L 2.34,14.67 C 2.15,14.82 2.1,15.09 2.22,15.31 L 4.22,18.77 C 4.34,18.99 4.6,19.07 4.83,18.98 L 7.35,17.97 C 7.88,18.38 8.45,18.72 9.08,18.97 L 9.46,21.65 C 9.5,21.89 9.7,22.07 9.94,22.07 L 13.94,22.07 C 14.18,22.07 14.38,21.89 14.42,21.65 L 14.8,18.97 C 15.43,18.72 16,18.38 16.53,17.97 L 19.05,18.98 C 19.28,19.07 19.54,18.99 19.66,18.77 L 21.66,15.31 C 21.78,15.09 21.73,14.82 21.54,14.67 L 19.4,13 Z"),
                Fill = cor
            };
            vb.Child = p;
            return vb;
        }

        private static UIElement CriarIconeSom(bool ativado, bool escuro = false)
        {
            Viewbox vb = new Viewbox { Width = 17, Height = 17, Stretch = Stretch.Uniform };
            Brush cor;
            if (escuro)
            {
                cor = ativado ? Brushes.White : new SolidColorBrush(Color.FromArgb(140, 255, 255, 255));
            }
            else
            {
                cor = ativado ? new SolidColorBrush(Color.FromRgb(2, 132, 199)) : new SolidColorBrush(Color.FromRgb(148, 163, 184));
            }
            System.Windows.Shapes.Path p = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse(ativado
                    ? "M 3,9 L 7,9 L 12,4 L 12,20 L 7,15 L 3,15 Z M 16.5,12 C 16.5,10.23 15.48,8.71 14,7.97 L 14,16.02 C 15.48,15.29 16.5,13.77 16.5,12 Z M 14,3.23 L 14,5.29 C 16.89,6.15 19,8.83 19,12 C 19,15.17 16.89,17.85 14,18.71 L 14,20.77 C 18.01,19.86 21,16.28 21,12 C 21,7.72 18.01,4.14 14,3.23 Z"
                    : "M 4.27,3 L 3,4.27 L 7.73,9 L 3,9 L 3,15 L 7,15 L 12,20 L 12,13.27 L 16.25,17.53 C 15.58,18.04 14.83,18.46 14,18.7 L 14,20.77 C 15.38,20.45 16.63,19.82 17.68,18.96 L 19.73,21 L 21,19.73 L 4.27,3 Z M 12,4 L 9.91,6.09 L 12,8.18 L 12,4 Z M 19,12 C 19,12.82 18.84,13.61 18.55,14.33 L 20.08,15.86 C 20.67,14.69 21,13.39 21,12 C 21,7.72 18.01,4.14 14,3.23 L 14,5.29 C 16.89,6.15 19,8.83 19,12 Z M 16.5,12 C 16.5,10.23 15.48,8.71 14,7.97 L 14,10.18 L 16.45,12.63 C 16.48,12.43 16.5,12.22 16.5,12 Z"),
                Fill = cor
            };
            vb.Child = p;
            return vb;
        }

        private static UIElement CriarIconeNotif(bool ativado, bool escuro = false)
        {
            Viewbox vb = new Viewbox { Width = 17, Height = 17, Stretch = Stretch.Uniform };
            Brush cor;
            if (escuro)
            {
                cor = ativado ? Brushes.White : new SolidColorBrush(Color.FromArgb(140, 255, 255, 255));
            }
            else
            {
                cor = ativado ? new SolidColorBrush(Color.FromRgb(2, 132, 199)) : new SolidColorBrush(Color.FromRgb(148, 163, 184));
            }
            System.Windows.Shapes.Path p = new System.Windows.Shapes.Path
            {
                Data = Geometry.Parse(ativado
                    ? "M 12,22 C 13.1,22 14,21.1 14,20 L 10,20 C 10,21.1 10.89,22 12,22 Z M 18,16 L 18,11 C 18,7.93 16.36,5.36 13.5,4.68 L 13.5,4 C 13.5,3.17 12.83,2.5 12,2.5 C 11.17,2.5 10.5,3.17 10.5,4 L 10.5,4.68 C 7.63,5.36 6,7.92 6,11 L 6,16 L 4,18 L 4,19 L 20,19 L 20,18 L 18,16 Z"
                    : "M 20,18.69 L 5.31,4 L 4,5.31 L 6.27,7.58 C 6.1,8.65 6,9.8 6,11 L 6,16 L 4,18 L 4,19 L 17.69,19 L 18.69,20 L 20,18.69 Z M 12,22 C 13.1,22 14,21.1 14,20 L 10,20 C 10,21.1 10.89,22 12,22 Z M 18,16 L 18,11 C 18,8.6 16.94,6.54 15,5.35 L 15,4 C 15,2.34 13.66,1 12,1 C 11.39,1 10.83,1.18 10.36,1.49 L 18,9.13 L 18,16 Z"),
                Fill = cor
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
                ? CriarIconeSol(Brushes.White)
                : CriarIconeLua(new SolidColorBrush(Color.FromRgb(2, 132, 199)));
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
                _btnTema.Background = escuro ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255));
                _btnTema.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
            }
        }

        private void AplicarTema()
        {
            bool escuro = (_dados != null && _dados.ModoEscuro);

            // Estilização moderna e refinada do ScrollBar (substitui o padrão quadrado do Windows)
            AplicarEstiloScrollBar(this, escuro);

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
            if (_statusPill != null && _txtStatusPill != null)
            {
                _statusPill.Background = escuro ? new SolidColorBrush(Color.FromRgb(6, 78, 59)) : new SolidColorBrush(Color.FromRgb(236, 253, 245));
                _statusPill.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(5, 150, 105)) : new SolidColorBrush(Color.FromRgb(167, 243, 208));
                _txtStatusPill.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(52, 211, 153)) : new SolidColorBrush(Color.FromRgb(5, 150, 105));
            }
            Brush corIconesJanela = escuro ? Brushes.White : corTitulo;
            if (_btnMin != null) _btnMin.Content = CriarIconeMinimizar(corIconesJanela);
            if (_btnMax != null) _btnMax.Content = CriarIconeMaximizar(corIconesJanela, WindowState == WindowState.Maximized, escuro);
            if (_btnClose != null) _btnClose.Content = CriarIconeFechar(corIconesJanela);

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

            if (_btnSom != null) { _btnSom.Background = bgBtn; _btnSom.BorderBrush = borderBtn; _btnSom.Content = CriarIconeSom(_dados.SomAtivado, escuro); }
            if (_btnNotif != null) { _btnNotif.Background = bgBtn; _btnNotif.BorderBrush = borderBtn; _btnNotif.Content = CriarIconeNotif(_dados.NotificacoesAtivadas, escuro); }
            if (_btnConfig != null) { _btnConfig.Background = bgBtn; _btnConfig.BorderBrush = borderBtn; _btnConfig.Content = CriarIconeEngrenagem(escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(71, 85, 105))); }
            AtualizarVisualBtnTema();

            if (_btnToggleHistorico != null)
            {
                _btnToggleHistorico.Background = escuro ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromArgb(220, 224, 242, 254));
                _btnToggleHistorico.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                _btnToggleHistorico.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(2, 132, 199));
            }
            if (_btnModoWidget != null)
            {
                _btnModoWidget.Background = escuro ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromArgb(220, 224, 242, 254));
                _btnModoWidget.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                _btnModoWidget.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(2, 132, 199));
            }
            if (_widgetWindow != null)
            {
                _widgetWindow.AplicarTemaWidget();
            }

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

            // 6. Card Lembrete Zen
            AtualizarEstiloTimer();
            AtualizarBotoesIntervalo();
            if (_btnTestarLembrete != null)
            {
                _btnTestarLembrete.Background = escuro ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(248, 250, 252));
                _btnTestarLembrete.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(226, 232, 240));
                _btnTestarLembrete.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(71, 85, 105));
            }
            if (_trackTimerProgress != null)
            {
                _trackTimerProgress.Background = escuro ? new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(224, 242, 254));
                _trackTimerProgress.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
            }
            if (_txtLembrarACada != null)
            {
                _txtLembrarACada.Foreground = corSub;
            }

            // 7. Card Registrar Água Vivo & Fofo
            AtualizarVisualBotoesBebida();

            // 8. View Histórico Zen
            if (_txtTituloHistorico != null)
                _txtTituloHistorico.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199));
            if (_txtSubtituloHistorico != null)
                _txtSubtituloHistorico.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139));
            if (_btnVoltarHistorico != null)
            {
                _btnVoltarHistorico.Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255));
                _btnVoltarHistorico.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                _btnVoltarHistorico.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(2, 132, 199));
            }
            if (_btnExportarRelatorio != null)
            {
                _btnExportarRelatorio.Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255));
                _btnExportarRelatorio.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                _btnExportarRelatorio.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(2, 132, 199));
            }
            if (_pillsSegmentedContainer != null)
            {
                _pillsSegmentedContainer.Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(224, 242, 254));
            }
            if (_cardStatsHoje != null)
            {
                _cardStatsHoje.Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255));
                _cardStatsHoje.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
            }
            if (_cardStatsMedia != null)
            {
                _cardStatsMedia.Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255));
                _cardStatsMedia.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
            }
            if (_cardStatsMetas != null)
            {
                _cardStatsMetas.Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255));
                _cardStatsMetas.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
            }

            if (_btnTabHoje != null && _btnTabSemana != null)
            {
                SelecionarAbaHistorico(_abaHistoricoAtiva);
            }
        }

        private Button CriarBotaoToolbarCard(UIElement icone, string tooltip, RoutedEventHandler handler, bool isGear = false)
        {
            bool escuro = (_dados != null && _dados.ModoEscuro);
            Button btn = new Button
            {
                Content = icone,
                ToolTip = tooltip,
                Width = 34,
                Height = 34,
                Margin = new Thickness(3, 0, 3, 0),
                Background = escuro ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255)),
                BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                BorderThickness = new Thickness(1),
                FocusVisualStyle = null,
                Cursor = Cursors.Hand
            };
            AplicarEstiloPill(btn, 10);
            TransformGroup tg = new TransformGroup();
            ScaleTransform st = new ScaleTransform(1.0, 1.0);
            RotateTransform rt = new RotateTransform(0);
            tg.Children.Add(st);
            tg.Children.Add(rt);
            btn.RenderTransformOrigin = new Point(0.5, 0.5);
            btn.RenderTransform = tg;
            btn.MouseEnter += delegate
            {
                bool isDark = (_dados != null && _dados.ModoEscuro);
                btn.Background = isDark ? new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(224, 242, 254));
                btn.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                st.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.0, 1.08, TimeSpan.FromMilliseconds(150)));
                st.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.0, 1.08, TimeSpan.FromMilliseconds(150)));
                if (isGear) rt.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(0, 45, TimeSpan.FromMilliseconds(220)));
            };
            btn.MouseLeave += delegate
            {
                bool isDark = (_dados != null && _dados.ModoEscuro);
                btn.Background = isDark ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(240, 249, 255));
                btn.BorderBrush = isDark ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                st.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1.08, 1.0, TimeSpan.FromMilliseconds(150)));
                st.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1.08, 1.0, TimeSpan.FromMilliseconds(150)));
                if (isGear) rt.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation(45, 0, TimeSpan.FromMilliseconds(220)));
            };
            btn.Click += handler;
            return btn;
        }

        private void AplicarEstiloPill(Button btn, double cornerRadius)
        {
            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.Name = "pillBorder";
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(cornerRadius));
            borderFactory.SetBinding(Border.BackgroundProperty, new Binding("Background") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.BorderBrushProperty, new Binding("BorderBrush") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.BorderThicknessProperty, new Binding("BorderThickness") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.PaddingProperty, new Binding("Padding") { RelativeSource = RelativeSource.TemplatedParent });

            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            presenter.SetValue(ContentPresenter.RecognizesAccessKeyProperty, true);
            borderFactory.AppendChild(presenter);

            template.VisualTree = borderFactory;
            btn.Template = template;
        }

        public static void AplicarEstiloScrollBar(FrameworkElement element, bool isDark)
        {
            if (element == null) return;
            string thumbColor = isDark ? "#38BDF8" : "#0284C7";
            string thumbHover = isDark ? "#7DD3FC" : "#0369A1";
            string trackHover = isDark ? "#15FFFFFF" : "#100284C7";
            string xaml = string.Format(@"
<Style xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
       xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
       TargetType='{{x:Type ScrollBar}}'>
    <Setter Property='SnapsToDevicePixels' Value='True'/>
    <Setter Property='OverridesDefaultStyle' Value='True'/>
    <Style.Triggers>
        <Trigger Property='Orientation' Value='Vertical'>
            <Setter Property='Width' Value='8'/>
            <Setter Property='MinWidth' Value='8'/>
            <Setter Property='Background' Value='Transparent'/>
            <Setter Property='Template'>
                <Setter.Value>
                    <ControlTemplate TargetType='{{x:Type ScrollBar}}'>
                        <Grid Background='Transparent' Margin='0,2,2,2'>
                            <Border x:Name='trackBg' CornerRadius='4' Background='Transparent'/>
                            <Track x:Name='PART_Track' IsDirectionReversed='true'>
                                <Track.Thumb>
                                    <Thumb FocusVisualStyle='{{x:Null}}'>
                                        <Thumb.Template>
                                            <ControlTemplate TargetType='{{x:Type Thumb}}'>
                                                <Border x:Name='thumbBorder' CornerRadius='3.5' Margin='1,0,1,0' Background='{0}' Opacity='0.38'/>
                                                <ControlTemplate.Triggers>
                                                    <Trigger Property='IsMouseOver' Value='True'>
                                                        <Setter TargetName='thumbBorder' Property='Opacity' Value='0.85'/>
                                                        <Setter TargetName='thumbBorder' Property='Background' Value='{1}'/>
                                                    </Trigger>
                                                    <Trigger Property='IsDragging' Value='True'>
                                                        <Setter TargetName='thumbBorder' Property='Opacity' Value='1.0'/>
                                                        <Setter TargetName='thumbBorder' Property='Background' Value='{1}'/>
                                                    </Trigger>
                                                </ControlTemplate.Triggers>
                                            </ControlTemplate>
                                        </Thumb.Template>
                                    </Thumb>
                                </Track.Thumb>
                            </Track>
                        </Grid>
                        <ControlTemplate.Triggers>
                            <Trigger Property='IsMouseOver' Value='True'>
                                <Setter TargetName='trackBg' Property='Background' Value='{2}'/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Trigger>
        <Trigger Property='Orientation' Value='Horizontal'>
            <Setter Property='Height' Value='8'/>
            <Setter Property='MinHeight' Value='8'/>
            <Setter Property='Background' Value='Transparent'/>
            <Setter Property='Template'>
                <Setter.Value>
                    <ControlTemplate TargetType='{{x:Type ScrollBar}}'>
                        <Grid Background='Transparent' Margin='2,0,2,2'>
                            <Border x:Name='trackBg' CornerRadius='4' Background='Transparent'/>
                            <Track x:Name='PART_Track' IsDirectionReversed='false'>
                                <Track.Thumb>
                                    <Thumb FocusVisualStyle='{{x:Null}}'>
                                        <Thumb.Template>
                                            <ControlTemplate TargetType='{{x:Type Thumb}}'>
                                                <Border x:Name='thumbBorder' CornerRadius='3.5' Margin='0,1,0,1' Background='{0}' Opacity='0.38'/>
                                                <ControlTemplate.Triggers>
                                                    <Trigger Property='IsMouseOver' Value='True'>
                                                        <Setter TargetName='thumbBorder' Property='Opacity' Value='0.85'/>
                                                        <Setter TargetName='thumbBorder' Property='Background' Value='{1}'/>
                                                    </Trigger>
                                                    <Trigger Property='IsDragging' Value='True'>
                                                        <Setter TargetName='thumbBorder' Property='Opacity' Value='1.0'/>
                                                        <Setter TargetName='thumbBorder' Property='Background' Value='{1}'/>
                                                    </Trigger>
                                                </ControlTemplate.Triggers>
                                            </ControlTemplate>
                                        </Thumb.Template>
                                    </Thumb>
                                </Track.Thumb>
                            </Track>
                        </Grid>
                        <ControlTemplate.Triggers>
                            <Trigger Property='IsMouseOver' Value='True'>
                                <Setter TargetName='trackBg' Property='Background' Value='{2}'/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Trigger>
    </Style.Triggers>
</Style>", thumbColor, thumbHover, trackHover);

            try
            {
                Style s = (Style)System.Windows.Markup.XamlReader.Parse(xaml);
                element.Resources[typeof(ScrollBar)] = s;
            }
            catch { }
        }

        private Button CriarBotaoPill(string texto, bool secondary, RoutedEventHandler handler)
        {
            bool escuro = (_dados != null && _dados.ModoEscuro);
            Brush fg = escuro
                ? Brushes.White
                : (secondary ? new SolidColorBrush(Color.FromRgb(71, 85, 105)) : new SolidColorBrush(Color.FromRgb(3, 105, 161)));
            Brush bg = escuro
                ? new SolidColorBrush(Color.FromRgb(23, 32, 54))
                : (secondary ? new SolidColorBrush(Color.FromRgb(248, 250, 252)) : new SolidColorBrush(Color.FromRgb(240, 249, 255)));
            Brush border = escuro
                ? new SolidColorBrush(Color.FromRgb(51, 65, 85))
                : (secondary ? new SolidColorBrush(Color.FromRgb(226, 232, 240)) : new SolidColorBrush(Color.FromRgb(186, 230, 253)));

            Button btn = new Button
            {
                Content = texto,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = fg,
                Background = bg,
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(4, 0, 0, 0),
                BorderBrush = border,
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1.0, 1.0)
            };
            AplicarEstiloPill(btn, 12);

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
            btn.Click += delegate(object s, RoutedEventArgs e)
            {
                if (btn.RenderTransform is ScaleTransform)
                {
                    ScaleTransform st = (ScaleTransform)btn.RenderTransform;
                    DoubleAnimation bounce = new DoubleAnimation(0.94, 1.0, TimeSpan.FromMilliseconds(150));
                    bounce.EasingFunction = new BounceEase { Bounces = 1, Bounciness = 2 };
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, bounce);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, bounce);
                }
                handler(s, e);
            };
            return btn;
        }

    }

    // ==========================================
    // Modo Widget Flutuante (Opção A - Card Compacto no Canto da Tela)
    // ==========================================
    public class WidgetWindow : Window
    {
        private MainWindow _main;
        private Border _cardExterno;
        private Border _pillTimer;
        private TextBlock _txtTimer;
        private Button _btnPin;
        private Button _btnExpandir;
        private Canvas _canvasMascote;
        private Border _imgBorderMascote;
        private ScaleTransform _stMascote;
        private TranslateTransform _ttMascote;
        private RotateTransform _rtMascote;
        private System.Windows.Shapes.Polygon _coroaGotinha;
        private TextBlock _txtStatusGotinha;
        private TextBlock _txtMlProgresso;
        private Border _badgePct;
        private TextBlock _txtPct;
        private Border _barTrack;
        private Border _barFill;
        private ColumnDefinition _colAtivo;
        private ColumnDefinition _colRestante;
        private Border _boxInputMl;
        private TextBox _txtInputMl;
        private TextBlock _txtPlaceholderMl;
        private Button _btnBeber;
        private bool _posicaoInicialDefinida = false;

        public WidgetWindow(MainWindow main)
        {
            _main = main;

            Title = "Lumi Widget \uD83D\uDCA7";
            Width = 396;
            Height = 142;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;
            ShowInTaskbar = true;

            SnapsToDevicePixels = true;
            UseLayoutRounding = true;
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);
            TextOptions.SetTextRenderingMode(this, TextRenderingMode.ClearType);
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);

            Icon = IconeHelper.ObterIconeJanela();

            ConstruirWidgetUI();
            AplicarTemaWidget();
            AtualizarDadosWidget();
        }

        private void ConstruirWidgetUI()
        {
            Grid root = new Grid();

            _cardExterno = new Border
            {
                Margin = new Thickness(10),
                CornerRadius = new CornerRadius(20),
                BorderThickness = new Thickness(1.5),
                Padding = new Thickness(10, 10, 14, 10),
                Opacity = 0.92,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new TranslateTransform(0, 0),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 22,
                    ShadowDepth = 4,
                    Opacity = 0.28,
                    Color = Color.FromRgb(2, 132, 199)
                }
            };

            // Opacidade inteligente ao passar o mouse
            _cardExterno.MouseEnter += delegate
            {
                DoubleAnimation animOp = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(160));
                _cardExterno.BeginAnimation(UIElement.OpacityProperty, animOp);
            };
            _cardExterno.MouseLeave += delegate
            {
                if (_txtInputMl != null && _txtInputMl.IsKeyboardFocused) return;
                DoubleAnimation animOp = new DoubleAnimation(0.90, TimeSpan.FromMilliseconds(220));
                _cardExterno.BeginAnimation(UIElement.OpacityProperty, animOp);
            };

            // Arrastar para qualquer canto + Snap magnético nas bordas + Duplo clique para expandir
            _cardExterno.MouseLeftButtonDown += delegate(object s, MouseButtonEventArgs e)
            {
                if (e.ClickCount == 2)
                {
                    _main.SairModoWidget();
                    e.Handled = true;
                    return;
                }
                try
                {
                    DragMove();
                    AjustarSnapCanto();
                }
                catch { }
            };

            // Layout Horizontal: Coluna 0 = Mascote Gotinha | Coluna 1 = Infos + Progresso + Input ml
            Grid layoutHorizontal = new Grid();
            layoutHorizontal.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            layoutHorizontal.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // --- COLUNA 0: Mini Gotinha à esquerda ---
            _canvasMascote = CriarMiniMascoteCanvas();
            Border mascoteHolder = new Border
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                Child = _canvasMascote
            };
            Grid.SetColumn(mascoteHolder, 0);
            layoutHorizontal.Children.Add(mascoteHolder);

            // --- COLUNA 1: Painel direito (Header + Barra + Status + Input ml) ---
            Grid rightPanel = new Grid { VerticalAlignment = VerticalAlignment.Center };
            rightPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 0: Volume + % + Timer + Pin/Expandir
            rightPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 1: Barra de Progresso
            rightPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 2: Frase de status
            rightPanel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 3: Input de ml + Botão Beber

            // Row 0: Topo (Volume + Badge % + Timer na esquerda | Pin + Expandir na direita)
            Grid header = new Grid { Margin = new Thickness(0, 0, 0, 4) };
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel infoLeft = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            _txtMlProgresso = new TextBlock
            {
                Text = "0 / 2000 ml",
                FontSize = 13,
                FontWeight = FontWeights.ExtraBold,
                VerticalAlignment = VerticalAlignment.Center
            };
            infoLeft.Children.Add(_txtMlProgresso);

            _badgePct = new Border
            {
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6, 1, 6, 1),
                Margin = new Thickness(6, 0, 5, 0),
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Center
            };
            _txtPct = new TextBlock
            {
                Text = "0%",
                FontSize = 9.5,
                FontWeight = FontWeights.Bold
            };
            _badgePct.Child = _txtPct;
            infoLeft.Children.Add(_badgePct);

            _pillTimer = new Border
            {
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6, 1, 6, 1),
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = "Tempo para o pr\u00F3ximo lembrete (arraste o card para mover)"
            };
            _txtTimer = new TextBlock
            {
                Text = "\u23F3 30:00",
                FontSize = 9.5,
                FontWeight = FontWeights.Bold
            };
            _pillTimer.Child = _txtTimer;
            infoLeft.Children.Add(_pillTimer);

            Grid.SetColumn(infoLeft, 0);
            header.Children.Add(infoLeft);

            StackPanel topBtns = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

            _btnPin = new Button
            {
                Content = "\uD83D\uDCCC",
                FontSize = 10,
                Width = 24,
                Height = 22,
                Margin = new Thickness(0, 0, 4, 0),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                FocusVisualStyle = null,
                ToolTip = "Sempre vis\u00EDvel no topo (Ativado)"
            };
            AplicarPillWidget(_btnPin, 7);
            _btnPin.Click += delegate
            {
                Topmost = !Topmost;
                _btnPin.ToolTip = Topmost ? "Sempre vis\u00EDvel no topo (Ativado)" : "Sempre vis\u00EDvel no topo (Desativado)";
                AplicarTemaWidget();
            };
            topBtns.Children.Add(_btnPin);

            _btnExpandir = new Button
            {
                Content = "\u2922",
                FontSize = 11.5,
                FontWeight = FontWeights.Bold,
                Width = 24,
                Height = 22,
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                FocusVisualStyle = null,
                ToolTip = "Voltar para a janela completa"
            };
            AplicarPillWidget(_btnExpandir, 7);
            _btnExpandir.Click += delegate { _main.SairModoWidget(); };
            topBtns.Children.Add(_btnExpandir);

            Grid.SetColumn(topBtns, 1);
            header.Children.Add(topBtns);

            Grid.SetRow(header, 0);
            rightPanel.Children.Add(header);

            // Row 1: Barra de Progresso Horizontal
            _barTrack = new Border
            {
                Height = 7,
                CornerRadius = new CornerRadius(3.5),
                BorderThickness = new Thickness(1),
                ClipToBounds = true,
                Margin = new Thickness(0, 1, 0, 3)
            };
            Grid barGrid = new Grid();
            _colAtivo = new ColumnDefinition { Width = new GridLength(0, GridUnitType.Star) };
            _colRestante = new ColumnDefinition { Width = new GridLength(100, GridUnitType.Star) };
            barGrid.ColumnDefinitions.Add(_colAtivo);
            barGrid.ColumnDefinitions.Add(_colRestante);

            _barFill = new Border
            {
                CornerRadius = new CornerRadius(3.5),
                Background = new LinearGradientBrush(Color.FromRgb(56, 189, 248), Color.FromRgb(2, 132, 199), 0.0)
            };
            Grid.SetColumn(_barFill, 0);
            barGrid.Children.Add(_barFill);
            _barTrack.Child = barGrid;

            Grid.SetRow(_barTrack, 1);
            rightPanel.Children.Add(_barTrack);

            // Row 2: Texto de status/incentivo da Gotinha
            _txtStatusGotinha = new TextBlock
            {
                Text = "Hidrate-se com carinho! \u2728",
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(1, 0, 0, 5)
            };
            Grid.SetRow(_txtStatusGotinha, 2);
            rightPanel.Children.Add(_txtStatusGotinha);

            // Row 3: Entrada escrita de ml + Botão "+ Beber"
            Grid inputGrid = new Grid();
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            inputGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _boxInputMl = new Border
            {
                CornerRadius = new CornerRadius(9),
                BorderThickness = new Thickness(1.2),
                Padding = new Thickness(8, 1, 8, 1),
                Height = 28
            };

            Grid overlayInput = new Grid();
            _txtPlaceholderMl = new TextBlock
            {
                Text = "Quantos ml tomou? (ex: 250)",
                FontSize = 10.5,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false,
                Margin = new Thickness(2, 0, 0, 0)
            };
            overlayInput.Children.Add(_txtPlaceholderMl);

            _txtInputMl = new TextBox
            {
                FontSize = 11.5,
                FontWeight = FontWeights.SemiBold,
                BorderThickness = new Thickness(0),
                Background = Brushes.Transparent,
                VerticalContentAlignment = VerticalAlignment.Center,
                MaxLength = 4,
                ToolTip = "Digite quantos ml voc\u00EA bebeu e pressione Enter"
            };
            _txtInputMl.TextChanged += delegate
            {
                _txtPlaceholderMl.Visibility = string.IsNullOrEmpty(_txtInputMl.Text) ? Visibility.Visible : Visibility.Collapsed;
            };
            _txtInputMl.PreviewTextInput += delegate(object s, TextCompositionEventArgs e)
            {
                for (int i = 0; i < e.Text.Length; i++)
                {
                    if (!char.IsDigit(e.Text[i]))
                    {
                        e.Handled = true;
                        return;
                    }
                }
            };
            _txtInputMl.GotFocus += delegate
            {
                _cardExterno.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(120)));
            };
            _txtInputMl.KeyDown += delegate(object s, KeyEventArgs e)
            {
                if (e.Key == Key.Enter)
                {
                    RegistrarMlDigitado();
                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    _txtInputMl.Text = "";
                    Keyboard.ClearFocus();
                    e.Handled = true;
                }
            };
            overlayInput.Children.Add(_txtInputMl);
            _boxInputMl.Child = overlayInput;

            Grid.SetColumn(_boxInputMl, 0);
            inputGrid.Children.Add(_boxInputMl);

            _btnBeber = new Button
            {
                Content = "+ Beber",
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Background = new LinearGradientBrush(Color.FromRgb(56, 189, 248), Color.FromRgb(2, 132, 199), 90.0),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(12, 0, 12, 0),
                Height = 28,
                Margin = new Thickness(6, 0, 0, 0),
                Cursor = Cursors.Hand,
                FocusVisualStyle = null,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1.0, 1.0),
                ToolTip = "Registrar quantidade digitada"
            };
            AplicarPillWidget(_btnBeber, 9);
            _btnBeber.Click += delegate
            {
                if (_btnBeber.RenderTransform is ScaleTransform)
                {
                    ScaleTransform st = (ScaleTransform)_btnBeber.RenderTransform;
                    DoubleAnimation bounce = new DoubleAnimation(0.92, 1.0, TimeSpan.FromMilliseconds(160))
                    {
                        EasingFunction = new BounceEase { Bounces = 1, Bounciness = 2 }
                    };
                    st.BeginAnimation(ScaleTransform.ScaleXProperty, bounce);
                    st.BeginAnimation(ScaleTransform.ScaleYProperty, bounce);
                }
                RegistrarMlDigitado();
            };
            Grid.SetColumn(_btnBeber, 1);
            inputGrid.Children.Add(_btnBeber);

            Grid.SetRow(inputGrid, 3);
            rightPanel.Children.Add(inputGrid);

            Grid.SetColumn(rightPanel, 1);
            layoutHorizontal.Children.Add(rightPanel);

            _cardExterno.Child = layoutHorizontal;
            root.Children.Add(_cardExterno);
            Content = root;
        }

        private Canvas CriarMiniMascoteCanvas()
        {
            Canvas canvas = new Canvas
            {
                Width = 84,
                Height = 82,
                Cursor = Cursors.Hand,
                ToolTip = "Clique na Lumi para receber um carinho (ou duplo clique no card para expandir)"
            };

            string mascotPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mascote.png");
            if (File.Exists(mascotPath))
            {
                try
                {
                    _imgBorderMascote = new Border
                    {
                        Width = 76,
                        Height = 76,
                        Background = Brushes.Transparent,
                        Effect = new DropShadowEffect
                        {
                            BlurRadius = 12,
                            ShadowDepth = 2,
                            Opacity = 0.24,
                            Color = Color.FromRgb(2, 132, 199)
                        }
                    };

                    Image img = new Image
                    {
                        Width = 76,
                        Height = 76,
                        Stretch = Stretch.Uniform
                    };
                    BitmapImage bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(mascotPath, UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    img.Source = bmp;
                    _imgBorderMascote.Child = img;

                    TransformGroup tg = new TransformGroup();
                    _stMascote = new ScaleTransform(1.0, 1.0);
                    _rtMascote = new RotateTransform(0.0);
                    _ttMascote = new TranslateTransform(0.0, 0.0);
                    tg.Children.Add(_stMascote);
                    tg.Children.Add(_rtMascote);
                    tg.Children.Add(_ttMascote);

                    _imgBorderMascote.RenderTransformOrigin = new Point(0.5, 0.5);
                    _imgBorderMascote.RenderTransform = tg;

                    Canvas.SetLeft(_imgBorderMascote, 4);
                    Canvas.SetTop(_imgBorderMascote, 4);
                    canvas.Children.Add(_imgBorderMascote);

                    _coroaGotinha = new System.Windows.Shapes.Polygon
                    {
                        Points = new PointCollection(new Point[] {
                            new Point(32, 11), new Point(37, 2), new Point(42, 7), new Point(47, 2), new Point(52, 11)
                        }),
                        Fill = new SolidColorBrush(Color.FromRgb(251, 191, 36)),
                        Stroke = new SolidColorBrush(Color.FromRgb(217, 119, 6)),
                        StrokeThickness = 1.2,
                        Visibility = Visibility.Collapsed
                    };
                    canvas.Children.Add(_coroaGotinha);

                    IniciarFlutuarMiniMascote();
                }
                catch { }
            }

            canvas.MouseLeftButtonDown += delegate(object s, MouseButtonEventArgs e)
            {
                if (e.ClickCount == 1)
                {
                    string[] mimos = new string[] {
                        "Cada gole conta! \uD83D\uDCA7\u2728",
                        "Estou cuidando de voc\u00EA! \uD83D\uDC99",
                        "Beba \u00E1gua e brilhe! \uD83C\uDF1F",
                        "Hora de um golinho? \uD83E\uDD64"
                    };
                    Random rnd = new Random();
                    _txtStatusGotinha.Text = mimos[rnd.Next(mimos.Length)];
                    ExecutarPuloMascoteWidget();
                    if (_main._dados != null && _main._dados.SomAtivado)
                    {
                        GeradorDeSom.TocarGota();
                    }
                }
            };

            return canvas;
        }

        private void IniciarFlutuarMiniMascote()
        {
            if (_ttMascote == null || _stMascote == null) return;

            DoubleAnimation animFloat = new DoubleAnimation(0.0, -4.0, TimeSpan.FromMilliseconds(1800))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            _ttMascote.BeginAnimation(TranslateTransform.YProperty, animFloat);

            DoubleAnimation animBreath = new DoubleAnimation(1.0, 1.03, TimeSpan.FromMilliseconds(1800))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            _stMascote.BeginAnimation(ScaleTransform.ScaleYProperty, animBreath);
        }

        public void ExecutarPuloMascoteWidget()
        {
            if (_stMascote == null || _ttMascote == null) return;

            DoubleAnimationUsingKeyFrames animScaleX = new DoubleAnimationUsingKeyFrames();
            animScaleX.KeyFrames.Add(new SplineDoubleKeyFrame(1.16, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(80))));
            animScaleX.KeyFrames.Add(new SplineDoubleKeyFrame(0.88, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))));
            animScaleX.KeyFrames.Add(new SplineDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(380))));
            _stMascote.BeginAnimation(ScaleTransform.ScaleXProperty, animScaleX);

            DoubleAnimationUsingKeyFrames animScaleY = new DoubleAnimationUsingKeyFrames();
            animScaleY.KeyFrames.Add(new SplineDoubleKeyFrame(0.84, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(80))));
            animScaleY.KeyFrames.Add(new SplineDoubleKeyFrame(1.18, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))));
            animScaleY.KeyFrames.Add(new SplineDoubleKeyFrame(1.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(380))));
            _stMascote.BeginAnimation(ScaleTransform.ScaleYProperty, animScaleY);

            DoubleAnimationUsingKeyFrames animY = new DoubleAnimationUsingKeyFrames();
            animY.KeyFrames.Add(new SplineDoubleKeyFrame(3.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(80))));
            animY.KeyFrames.Add(new SplineDoubleKeyFrame(-12.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(200))));
            animY.KeyFrames.Add(new SplineDoubleKeyFrame(0.0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(380))));
            animY.Completed += delegate { IniciarFlutuarMiniMascote(); };
            _ttMascote.BeginAnimation(TranslateTransform.YProperty, animY);

            CriarParticulaWidget("\u2728", 10, 18);
            CriarParticulaWidget("\uD83D\uDCA7", 62, 14);
        }

        private void CriarParticulaWidget(string emoji, double x, double y)
        {
            if (_canvasMascote == null) return;
            TextBlock p = new TextBlock
            {
                Text = emoji,
                FontSize = 11,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(p, x);
            Canvas.SetTop(p, y);
            _canvasMascote.Children.Add(p);

            DoubleAnimation animSubir = new DoubleAnimation(y, y - 22, TimeSpan.FromMilliseconds(550))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            DoubleAnimation animFade = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(550));
            animFade.Completed += delegate
            {
                try { _canvasMascote.Children.Remove(p); } catch { }
            };
            p.BeginAnimation(Canvas.TopProperty, animSubir);
            p.BeginAnimation(UIElement.OpacityProperty, animFade);
        }

        private void RegistrarMlDigitado()
        {
            if (_txtInputMl == null) return;
            int val;
            if (int.TryParse(_txtInputMl.Text.Trim(), out val) && val > 0 && val <= 3000)
            {
                _txtInputMl.Text = "";
                _main.AdicionarAgua(val, "Widget (" + val + " ml) \uD83D\uDCA7");
                _txtStatusGotinha.Text = "+" + val + " ml registrados! \uD83D\uDCA7\u2728";
                ExecutarPuloMascoteWidget();
            }
            else
            {
                _txtStatusGotinha.Text = "Digite os ml (ex: 250) \uD83D\uDCA7";
                _txtInputMl.Focus();
            }
        }

        public void MostrarNoCanto()
        {
            if (!_posicaoInicialDefinida)
            {
                Rect area = SystemParameters.WorkArea;
                Left = area.Right - Width - 12;
                Top = area.Bottom - Height - 12;
                _posicaoInicialDefinida = true;
            }

            Show();
            Activate();

            if (_cardExterno != null && _cardExterno.RenderTransform is TranslateTransform)
            {
                TranslateTransform tt = (TranslateTransform)_cardExterno.RenderTransform;
                DoubleAnimation slide = new DoubleAnimation(12, 0, TimeSpan.FromMilliseconds(260))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                tt.BeginAnimation(TranslateTransform.YProperty, slide);
            }
        }

        private void AjustarSnapCanto()
        {
            Rect area = SystemParameters.WorkArea;
            double snapDist = 24.0;
            double margem = 10.0;

            if (Math.Abs(Left - area.Left) < snapDist) Left = area.Left + margem;
            else if (Math.Abs((Left + Width) - area.Right) < snapDist) Left = area.Right - Width - margem;

            if (Math.Abs(Top - area.Top) < snapDist) Top = area.Top + margem;
            else if (Math.Abs((Top + Height) - area.Bottom) < snapDist) Top = area.Bottom - Height - margem;
        }

        public void AtualizarDadosWidget()
        {
            if (_main == null || _main._dados == null) return;
            DadosDia dia = _main.ObterDiaHoje();
            int ml = dia.TotalMl;
            int meta = Math.Max(1, _main._dados.MetaDiaria);
            int pct = Math.Min(100, (int)Math.Round(((double)ml / meta) * 100));
            bool escuro = _main._dados.ModoEscuro;

            if (_txtMlProgresso != null)
            {
                _txtMlProgresso.Text = ml + " / " + meta + " ml";
            }
            if (_txtPct != null)
            {
                _txtPct.Text = (pct >= 100) ? "\uD83C\uDF89 " + pct + "%" : pct + "%";
            }

            int pctAtivo = Math.Max(0, Math.Min(100, pct));
            if (_colAtivo != null && _colRestante != null)
            {
                _colAtivo.Width = new GridLength(pctAtivo, GridUnitType.Star);
                _colRestante.Width = new GridLength(100 - pctAtivo, GridUnitType.Star);
            }

            if (_barFill != null)
            {
                if (pct >= 100)
                {
                    _barFill.Background = new LinearGradientBrush(Color.FromRgb(52, 211, 153), Color.FromRgb(16, 185, 129), 0.0);
                }
                else
                {
                    _barFill.Background = new LinearGradientBrush(Color.FromRgb(56, 189, 248), Color.FromRgb(2, 132, 199), 0.0);
                }
            }

            if (_badgePct != null && _txtPct != null)
            {
                if (pct >= 100)
                {
                    _badgePct.Background = escuro ? new SolidColorBrush(Color.FromRgb(6, 46, 26)) : new SolidColorBrush(Color.FromRgb(240, 253, 244));
                    _badgePct.BorderBrush = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                    _txtPct.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(52, 211, 153)) : new SolidColorBrush(Color.FromRgb(22, 101, 52));
                }
                else
                {
                    _badgePct.Background = escuro ? new SolidColorBrush(Color.FromRgb(15, 47, 74)) : new SolidColorBrush(Color.FromRgb(224, 242, 254));
                    _badgePct.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(2, 132, 199)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                    _txtPct.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199));
                }
            }

            if (_coroaGotinha != null)
            {
                _coroaGotinha.Visibility = (pct >= 100) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (_txtStatusGotinha != null)
            {
                if (pct >= 100) _txtStatusGotinha.Text = "Meta batida! Parab\u00E9ns! \uD83D\uDC51";
                else if (ml == 0) _txtStatusGotinha.Text = "Qual o primeiro gole de hoje? \uD83D\uDCA7";
                else _txtStatusGotinha.Text = "Faltam " + Math.Max(0, meta - ml) + " ml p/ a meta!";
            }
        }

        public void AtualizarTimerWidget(string tempoStr, bool pausado)
        {
            if (_txtTimer == null) return;
            if (pausado)
            {
                _txtTimer.Text = "\uD83D\uDCA4 Pausa";
            }
            else
            {
                _txtTimer.Text = "\u23F3 " + tempoStr;
            }
        }

        public void AlertarLembreteWidget()
        {
            if (_txtStatusGotinha != null)
            {
                _txtStatusGotinha.Text = "Hora de beber \u00E1gua! \uD83D\uDCA7\uD83D\uDD14";
            }
            ExecutarPuloMascoteWidget();
        }

        public void AplicarTemaWidget()
        {
            bool escuro = (_main != null && _main._dados != null && _main._dados.ModoEscuro);

            if (_cardExterno != null)
            {
                if (escuro)
                {
                    LinearGradientBrush bg = new LinearGradientBrush(
                        Color.FromRgb(15, 23, 42),
                        Color.FromRgb(23, 32, 54),
                        135.0
                    );
                    _cardExterno.Background = bg;
                    _cardExterno.BorderBrush = new SolidColorBrush(Color.FromArgb(150, 56, 189, 248));
                }
                else
                {
                    LinearGradientBrush bg = new LinearGradientBrush(
                        Color.FromRgb(240, 249, 255),
                        Color.FromRgb(255, 255, 255),
                        135.0
                    );
                    _cardExterno.Background = bg;
                    _cardExterno.BorderBrush = new SolidColorBrush(Color.FromRgb(186, 230, 253));
                }
            }

            if (_pillTimer != null && _txtTimer != null)
            {
                _pillTimer.Background = escuro ? new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(224, 242, 254));
                _pillTimer.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                _txtTimer.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(3, 105, 161));
            }

            if (_btnPin != null)
            {
                if (Topmost)
                {
                    _btnPin.Background = escuro ? new SolidColorBrush(Color.FromRgb(12, 74, 110)) : new SolidColorBrush(Color.FromRgb(224, 242, 254));
                    _btnPin.BorderBrush = new SolidColorBrush(Color.FromRgb(56, 189, 248));
                    _btnPin.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(2, 132, 199));
                    _btnPin.Opacity = 1.0;
                }
                else
                {
                    _btnPin.Background = escuro ? new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(241, 245, 249));
                    _btnPin.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(203, 213, 225));
                    _btnPin.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(100, 116, 139));
                    _btnPin.Opacity = 0.65;
                }
            }

            if (_btnExpandir != null)
            {
                _btnExpandir.Background = escuro ? new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(240, 249, 255));
                _btnExpandir.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                _btnExpandir.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(2, 132, 199));
            }

            if (_txtStatusGotinha != null)
            {
                _txtStatusGotinha.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(71, 85, 105));
            }

            if (_txtMlProgresso != null)
            {
                _txtMlProgresso.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(248, 250, 252)) : new SolidColorBrush(Color.FromRgb(3, 105, 161));
            }

            if (_barTrack != null)
            {
                _barTrack.Background = escuro ? new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(224, 242, 254));
                _barTrack.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
            }

            if (_boxInputMl != null)
            {
                _boxInputMl.Background = escuro ? new SolidColorBrush(Color.FromRgb(30, 41, 59)) : Brushes.White;
                _boxInputMl.BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(71, 85, 105)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
            }
            if (_txtInputMl != null)
            {
                _txtInputMl.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(15, 23, 42));
                _txtInputMl.CaretBrush = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199));
            }
            if (_txtPlaceholderMl != null)
            {
                _txtPlaceholderMl.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(100, 116, 139)) : new SolidColorBrush(Color.FromRgb(148, 163, 184));
            }
        }

        private static void AplicarPillWidget(Button btn, double cornerRadius)
        {
            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(cornerRadius));
            borderFactory.SetBinding(Border.BackgroundProperty, new Binding("Background") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.BorderBrushProperty, new Binding("BorderBrush") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.BorderThicknessProperty, new Binding("BorderThickness") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.PaddingProperty, new Binding("Padding") { RelativeSource = RelativeSource.TemplatedParent });

            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFactory.AppendChild(presenter);

            template.VisualTree = borderFactory;
            btn.Template = template;
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
            bool escuro = (_main != null && _main._dados != null && _main._dados.ModoEscuro);

            Title = "Hora do Gole! \uD83D\uDCA7";
            Width = 350;
            Height = 320;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Background = escuro ? new SolidColorBrush(Color.FromRgb(15, 23, 42)) : new SolidColorBrush(Color.FromRgb(248, 250, 252));
            Opacity = 0;

            Icon = IconeHelper.ObterIconeJanela();

            Loaded += delegate {
                DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)) {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            StackPanel sp = new StackPanel { Margin = new Thickness(22), VerticalAlignment = VerticalAlignment.Center };

            string mascotAlertPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mascote.png");
            if (File.Exists(mascotAlertPath))
            {
                try
                {
                    Image imgAlert = new Image
                    {
                        Width = 72,
                        Height = 72,
                        Stretch = Stretch.Uniform,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 10),
                        Effect = new DropShadowEffect
                        {
                            BlurRadius = 16,
                            ShadowDepth = 3,
                            Opacity = 0.35,
                            Color = Color.FromRgb(56, 189, 248)
                        }
                    };
                    BitmapImage bmpAlert = new BitmapImage();
                    bmpAlert.BeginInit();
                    bmpAlert.UriSource = new Uri(mascotAlertPath, UriKind.Absolute);
                    bmpAlert.CacheOption = BitmapCacheOption.OnLoad;
                    bmpAlert.EndInit();
                    imgAlert.Source = bmpAlert;
                    sp.Children.Add(imgAlert);
                }
                catch
                {
                    sp.Children.Add(new TextBlock { Text = "\uD83D\uDCA7", FontSize = 44, HorizontalAlignment = HorizontalAlignment.Center });
                }
            }
            else
            {
                sp.Children.Add(new TextBlock { Text = "\uD83D\uDCA7", FontSize = 44, HorizontalAlignment = HorizontalAlignment.Center });
            }

            TextBlock title = new TextBlock
            {
                Text = "Hora de Beber \u00C1gua!",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 4, 0, 4)
            };
            sp.Children.Add(title);

            TextBlock desc = new TextBlock
            {
                Text = "Seus olhos e sua mente precisam de uma pausa.\nHidrate seu corpo agora!",
                FontSize = 11.5,
                Foreground = escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 16)
            };
            sp.Children.Add(desc);

            Button btn200 = new Button
            {
                Content = "\uD83E\uDD5B Tomei um Copo (+200 ml)",
                FontWeight = FontWeights.Bold,
                FontSize = 12.5,
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromRgb(2, 132, 199)),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, 0, 0, 8),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            AplicarPillModal(btn200, 12);
            btn200.Click += delegate
            {
                _main.AdicionarAgua(200, "Copo (\uD83E\uDD5B)");
                FecharSuave();
            };
            sp.Children.Add(btn200);

            Button btnAdiar = new Button
            {
                Content = "\u23F3 Adiar por 5 minutos",
                FontSize = 11.5,
                FontWeight = FontWeights.Medium,
                Foreground = escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                Background = escuro ? new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(12, 7, 12, 7),
                Cursor = Cursors.Hand
            };
            AplicarPillModal(btnAdiar, 12);
            btnAdiar.Click += delegate { FecharSuave(); };
            sp.Children.Add(btnAdiar);

            Content = sp;
        }

        private static void AplicarPillModal(Button btn, double cornerRadius)
        {
            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(cornerRadius));
            borderFactory.SetBinding(Border.BackgroundProperty, new Binding("Background") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.BorderBrushProperty, new Binding("BorderBrush") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.BorderThicknessProperty, new Binding("BorderThickness") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.PaddingProperty, new Binding("Padding") { RelativeSource = RelativeSource.TemplatedParent });

            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFactory.AppendChild(presenter);

            template.VisualTree = borderFactory;
            btn.Template = template;
        }

        private void FecharSuave()
        {
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(160));
            fadeOut.Completed += delegate { Close(); };
            BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }

    // ==========================================
    // Janela Modal: Configurações
    // ==========================================
    public class ModernToggleSwitch : ContentControl
    {
        private bool _isChecked;
        private bool _isDark;
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

        public ModernToggleSwitch(bool initialValue, bool isDark = false)
        {
            _isChecked = initialValue;
            _isDark = isDark;
            Cursor = Cursors.Hand;
            Focusable = true;

            Grid container = new Grid { VerticalAlignment = VerticalAlignment.Center };
            container.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            container.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _track = new Border
            {
                Width = 48,
                Height = 26,
                CornerRadius = new CornerRadius(13),
                Background = _isChecked 
                    ? new SolidColorBrush(Color.FromRgb(2, 132, 199)) 
                    : (_isDark ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(203, 213, 225))),
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
                Foreground = _isChecked 
                    ? (_isDark ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199)))
                    : (_isDark ? new SolidColorBrush(Color.FromRgb(100, 116, 139)) : new SolidColorBrush(Color.FromRgb(148, 163, 184))),
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

            _track.Background = _isChecked 
                ? new SolidColorBrush(Color.FromRgb(2, 132, 199)) 
                : (_isDark ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(203, 213, 225)));
            _thumb.HorizontalAlignment = _isChecked ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            _labelStatus.Text = _isChecked ? "Ativado" : "Desativado";
            _labelStatus.Foreground = _isChecked 
                ? (_isDark ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199)))
                : (_isDark ? new SolidColorBrush(Color.FromRgb(100, 116, 139)) : new SolidColorBrush(Color.FromRgb(148, 163, 184)));
        }
    }

    // ==========================================
    // Diálogo Modal Moderno: Avisos, Mensagens e Notificações no Padrão Lumi
    // ==========================================
    public enum LumiDialogTipo
    {
        Aviso,
        Informacao,
        Sucesso,
        Erro
    }

    public class LumiDialog : Window
    {
        public LumiDialog(string mensagem, string titulo, LumiDialogTipo tipo, bool isDark)
        {
            Title = string.IsNullOrEmpty(titulo) ? "Lumi - Aviso" : titulo;
            Width = 390;
            SizeToContent = SizeToContent.Height;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;
            ShowInTaskbar = false;

            Grid root = new Grid { Margin = new Thickness(14) };

            Border card = new Border
            {
                CornerRadius = new CornerRadius(20),
                BorderThickness = new Thickness(1.5),
                BorderBrush = isDark 
                    ? (Brush)new SolidColorBrush(Color.FromArgb(140, 56, 189, 248)) 
                    : new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                Background = isDark
                    ? (Brush)new LinearGradientBrush(Color.FromRgb(15, 23, 42), Color.FromRgb(23, 32, 54), 135.0)
                    : new LinearGradientBrush(Color.FromRgb(240, 249, 255), Color.FromRgb(255, 255, 255), 135.0),
                Effect = new DropShadowEffect
                {
                    Color = isDark ? Colors.Black : Color.FromRgb(2, 132, 199),
                    BlurRadius = 26,
                    ShadowDepth = 4,
                    Opacity = isDark ? 0.6 : 0.22
                }
            };

            StackPanel sp = new StackPanel();

            // 1. Barra de Título do Diálogo
            Grid titleBar = new Grid
            {
                Height = 36,
                Margin = new Thickness(16, 8, 10, 0),
                Background = Brushes.Transparent
            };
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel titleLeft = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            titleLeft.Children.Add(new TextBlock
            {
                Text = "\uD83D\uDCA7 ",
                FontSize = 13
            });
            titleLeft.Children.Add(new TextBlock
            {
                Text = string.IsNullOrEmpty(titulo) ? "Aviso" : titulo,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = isDark ? Brushes.White : new SolidColorBrush(Color.FromRgb(3, 105, 161)),
                VerticalAlignment = VerticalAlignment.Center
            });
            Grid.SetColumn(titleLeft, 0);
            titleBar.Children.Add(titleLeft);

            Button btnFechar = new Button
            {
                Content = "\u2715",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Width = 28,
                Height = 28,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = isDark ? Brushes.White : new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                Cursor = System.Windows.Input.Cursors.Hand,
                FocusVisualStyle = null
            };
            btnFechar.MouseEnter += delegate
            {
                btnFechar.Foreground = isDark ? new SolidColorBrush(Color.FromRgb(248, 113, 113)) : new SolidColorBrush(Color.FromRgb(225, 29, 72));
            };
            btnFechar.MouseLeave += delegate
            {
                btnFechar.Foreground = isDark ? Brushes.White : new SolidColorBrush(Color.FromRgb(100, 116, 139));
            };
            btnFechar.Click += delegate { FecharComAnimacao(); };
            Grid.SetColumn(btnFechar, 1);
            titleBar.Children.Add(btnFechar);

            titleBar.MouseDown += delegate(object s, System.Windows.Input.MouseButtonEventArgs e)
            {
                if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                {
                    try { DragMove(); } catch { }
                }
            };
            sp.Children.Add(titleBar);

            // 2. Área Central (Badge com Ícone + Texto da Mensagem)
            Grid corpo = new Grid { Margin = new Thickness(18, 10, 18, 16) };
            corpo.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            corpo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            string iconEmoji;
            Brush badgeBg;
            Brush badgeBorder;

            switch (tipo)
            {
                case LumiDialogTipo.Aviso:
                    iconEmoji = "\u26A0\uFE0F"; // ⚠️
                    badgeBg = isDark ? (Brush)new SolidColorBrush(Color.FromRgb(69, 26, 3)) : new SolidColorBrush(Color.FromRgb(254, 243, 199));
                    badgeBorder = isDark ? (Brush)new SolidColorBrush(Color.FromRgb(120, 53, 15)) : new SolidColorBrush(Color.FromRgb(253, 230, 138));
                    break;
                case LumiDialogTipo.Sucesso:
                    iconEmoji = "\uD83C\uDF89"; // 🎉
                    badgeBg = isDark ? (Brush)new SolidColorBrush(Color.FromRgb(6, 78, 59)) : new SolidColorBrush(Color.FromRgb(220, 252, 231));
                    badgeBorder = isDark ? (Brush)new SolidColorBrush(Color.FromRgb(5, 150, 105)) : new SolidColorBrush(Color.FromRgb(134, 239, 172));
                    break;
                default:
                    iconEmoji = "\uD83D\uDCA7"; // 💧
                    badgeBg = isDark ? (Brush)new SolidColorBrush(Color.FromRgb(23, 37, 84)) : new SolidColorBrush(Color.FromRgb(224, 242, 254));
                    badgeBorder = isDark ? (Brush)new SolidColorBrush(Color.FromRgb(30, 64, 175)) : new SolidColorBrush(Color.FromRgb(186, 230, 253));
                    break;
            }

            Border badge = new Border
            {
                Width = 44,
                Height = 44,
                CornerRadius = new CornerRadius(22),
                Background = badgeBg,
                BorderBrush = badgeBorder,
                BorderThickness = new Thickness(1.5),
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 2, 14, 0),
                Child = new TextBlock
                {
                    Text = iconEmoji,
                    FontSize = 21,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = isDark ? Brushes.White : new SolidColorBrush(Color.FromRgb(15, 23, 42))
                }
            };
            Grid.SetColumn(badge, 0);
            corpo.Children.Add(badge);

            TextBlock txtMsg = new TextBlock
            {
                Text = mensagem,
                FontSize = 13,
                FontWeight = FontWeights.Medium,
                Foreground = isDark ? new SolidColorBrush(Color.FromRgb(241, 245, 249)) : new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                LineHeight = 19
            };
            Grid.SetColumn(txtMsg, 1);
            corpo.Children.Add(txtMsg);
            sp.Children.Add(corpo);

            // 3. Rodapé com Botão "Entendido" estilo Pill
            Border footer = new Border { Margin = new Thickness(18, 0, 18, 16) };
            Button btnOk = new Button
            {
                Content = "Entendido",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Background = new LinearGradientBrush(Color.FromRgb(2, 132, 199), Color.FromRgb(56, 189, 248), 0.0),
                BorderThickness = new Thickness(0),
                Height = 36,
                MinWidth = 110,
                HorizontalAlignment = HorizontalAlignment.Right,
                Cursor = System.Windows.Input.Cursors.Hand,
                FocusVisualStyle = null,
                IsDefault = true,
                IsCancel = true
            };
            AplicarPillDialog(btnOk, 10);
            btnOk.Click += delegate { FecharComAnimacao(); };
            footer.Child = btnOk;
            sp.Children.Add(footer);

            card.Child = sp;
            root.Children.Add(card);
            Content = root;

            Opacity = 0.0;
            card.RenderTransformOrigin = new Point(0.5, 0.5);
            ScaleTransform st = new ScaleTransform(0.93, 0.93);
            card.RenderTransform = st;

            Loaded += delegate
            {
                DoubleAnimation fadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(160));
                DoubleAnimation scaleAnim = new DoubleAnimation(0.93, 1.0, TimeSpan.FromMilliseconds(180))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                BeginAnimation(UIElement.OpacityProperty, fadeIn);
                st.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
                st.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
            };
        }

        private void FecharComAnimacao()
        {
            DoubleAnimation fadeOut = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(120));
            fadeOut.Completed += delegate { Close(); };
            BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private static void AplicarPillDialog(Button btn, double cornerRadius)
        {
            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(cornerRadius));
            borderFactory.SetBinding(Border.BackgroundProperty, new Binding("Background") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.BorderBrushProperty, new Binding("BorderBrush") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.BorderThicknessProperty, new Binding("BorderThickness") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.PaddingProperty, new Binding("Padding") { RelativeSource = RelativeSource.TemplatedParent });

            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFactory.AppendChild(presenter);

            template.VisualTree = borderFactory;
            btn.Template = template;
        }

        public static void MostrarAviso(Window owner, string mensagem, string titulo, bool? modoEscuro)
        {
            Mostrar(owner, mensagem, titulo, LumiDialogTipo.Aviso, modoEscuro);
        }

        public static void MostrarSucesso(Window owner, string mensagem, string titulo, bool? modoEscuro)
        {
            Mostrar(owner, mensagem, titulo, LumiDialogTipo.Sucesso, modoEscuro);
        }

        public static void Mostrar(Window owner, string mensagem, string titulo, LumiDialogTipo tipo, bool? modoEscuro)
        {
            bool isDark = false;
            if (modoEscuro.HasValue)
            {
                isDark = modoEscuro.Value;
            }
            else if (owner is MainWindow)
            {
                MainWindow mw = (MainWindow)owner;
                isDark = (mw._dados != null && mw._dados.ModoEscuro);
            }

            LumiDialog dlg = new LumiDialog(mensagem, titulo, tipo, isDark);
            if (owner != null && owner.IsVisible)
            {
                dlg.Owner = owner;
                dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            else
            {
                dlg.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            try
            {
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                try { File.WriteAllText("error.log", ex.ToString()); } catch { }
            }
        }
    }

    // ==========================================
    // Janela Modal: Relatório de Saúde & Exportação
    // ==========================================
    public class RelatorioSaudeWindow : Window
    {
        public RelatorioSaudeWindow(HistoricoApp dados)
        {
            bool escuro = (dados != null && dados.ModoEscuro);
            MainWindow.AplicarEstiloScrollBar(this, escuro);

            Title = "Relat\u00F3rio de Sa\u00FAde & Hidrata\u00E7\u00E3o \uD83D\uDCCA";
            Width = 600;
            Height = Math.Min(660, SystemParameters.WorkArea.Height - 30);
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;
            Opacity = 0;

            Icon = IconeHelper.ObterIconeJanela();

            Loaded += delegate
            {
                DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            Brush cardBg = escuro
                ? (Brush)new SolidColorBrush(Color.FromRgb(15, 23, 42))
                : new SolidColorBrush(Color.FromRgb(248, 250, 252));
            Brush cardBorder = escuro
                ? (Brush)new SolidColorBrush(Color.FromArgb(140, 56, 189, 248))
                : new SolidColorBrush(Color.FromRgb(186, 230, 253));
            Brush miniCardBg = escuro
                ? (Brush)new SolidColorBrush(Color.FromRgb(30, 41, 59))
                : Brushes.White;
            Brush miniCardBorder = escuro
                ? (Brush)new SolidColorBrush(Color.FromRgb(51, 65, 85))
                : new SolidColorBrush(Color.FromRgb(226, 232, 240));
            Brush txtPreto = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(15, 23, 42));
            Brush txtMuted = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139));
            Brush txtDestaque = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199));

            Grid root = new Grid { Margin = new Thickness(14) };

            Border cardPrincipal = new Border
            {
                CornerRadius = new CornerRadius(20),
                BorderThickness = new Thickness(1.5),
                BorderBrush = cardBorder,
                Background = cardBg,
                Effect = new DropShadowEffect
                {
                    Color = escuro ? Colors.Black : Color.FromRgb(2, 132, 199),
                    BlurRadius = 28,
                    ShadowDepth = 4,
                    Opacity = escuro ? 0.6 : 0.22
                }
            };

            Grid layoutInterno = new Grid();
            layoutInterno.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 0: Title Bar
            layoutInterno.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 1: Mini stats cards
            layoutInterno.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // 2: Text preview
            layoutInterno.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 3: Action Buttons bar

            // 0: Title Bar
            Grid titleBar = new Grid
            {
                Height = 40,
                Margin = new Thickness(18, 10, 12, 0),
                Background = Brushes.Transparent
            };
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel titleLeft = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            titleLeft.Children.Add(new TextBlock
            {
                Text = "\uD83D\uDCCA ",
                FontSize = 14
            });
            titleLeft.Children.Add(new TextBlock
            {
                Text = "Relat\u00F3rio de Sa\u00FAde & Hidrata\u00E7\u00E3o",
                FontSize = 13.5,
                FontWeight = FontWeights.Bold,
                Foreground = escuro ? Brushes.White : txtDestaque,
                VerticalAlignment = VerticalAlignment.Center
            });
            Grid.SetColumn(titleLeft, 0);
            titleBar.Children.Add(titleLeft);

            Button btnFecharTopo = new Button
            {
                Content = "\u2715",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Width = 28,
                Height = 28,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = escuro ? Brushes.White : txtMuted,
                Cursor = Cursors.Hand,
                FocusVisualStyle = null
            };
            btnFecharTopo.MouseEnter += delegate
            {
                btnFecharTopo.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(248, 113, 113)) : new SolidColorBrush(Color.FromRgb(225, 29, 72));
            };
            btnFecharTopo.MouseLeave += delegate
            {
                btnFecharTopo.Foreground = escuro ? Brushes.White : txtMuted;
            };
            btnFecharTopo.Click += delegate
            {
                DoubleAnimation fo = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(160));
                fo.Completed += delegate { Close(); };
                BeginAnimation(UIElement.OpacityProperty, fo);
            };
            Grid.SetColumn(btnFecharTopo, 1);
            titleBar.Children.Add(btnFecharTopo);

            titleBar.MouseDown += delegate(object s, MouseButtonEventArgs e)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    try { DragMove(); } catch { }
                }
            };
            Grid.SetRow(titleBar, 0);
            layoutInterno.Children.Add(titleBar);

            // 1: Mini stats cards row
            int totalDias = (dados != null && dados.Dias != null && dados.Dias.Count > 0) ? dados.Dias.Count : 1;
            int somaTotal = 0;
            int metasBatidas = 0;
            if (dados != null && dados.Dias != null)
            {
                foreach (DadosDia d in dados.Dias)
                {
                    somaTotal += d.TotalMl;
                    if (d.TotalMl >= dados.MetaDiaria) metasBatidas++;
                }
            }
            double mediaDiaria = (double)somaTotal / totalDias;
            int pctMetas = (int)Math.Round(((double)metasBatidas / totalDias) * 100.0);
            int streak = CalcularStreakAtual(dados);

            Grid miniStatsGrid = new Grid { Margin = new Thickness(18, 6, 18, 10) };
            miniStatsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            miniStatsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            miniStatsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            miniStatsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            miniStatsGrid.Children.Add(CriarMiniStatCard(miniCardBg, miniCardBorder, txtPreto, txtMuted, "\uD83D\uDCA7 Total", (somaTotal / 1000.0).ToString("0.1") + " L", totalDias + " dias", 0));
            miniStatsGrid.Children.Add(CriarMiniStatCard(miniCardBg, miniCardBorder, txtPreto, txtMuted, "\uD83D\uDCC8 M\u00E9dia", (mediaDiaria >= 1000 ? (mediaDiaria / 1000.0).ToString("0.1") + " L" : Math.Round(mediaDiaria) + " ml"), "por dia", 1));
            miniStatsGrid.Children.Add(CriarMiniStatCard(miniCardBg, miniCardBorder, txtPreto, txtMuted, "\uD83C\uDFC6 Metas", pctMetas + "%", metasBatidas + " de " + totalDias, 2));
            miniStatsGrid.Children.Add(CriarMiniStatCard(miniCardBg, miniCardBorder, txtPreto, txtMuted, "\uD83D\uDD25 Sequ\u00EAncia", streak + " d", "consecutivos", 3));

            Grid.SetRow(miniStatsGrid, 1);
            layoutInterno.Children.Add(miniStatsGrid);

            // 2: Área Visual do Documento / Relatório de Saúde (estilo editorial limpo, humanizado)
            string relatorioTxt = GerarTextoRelatorio(dados);
            string relatorioCsv = GerarCsvRelatorio(dados);

            ScrollViewer scroll = new ScrollViewer
            {
                Margin = new Thickness(18, 0, 18, 12),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                FocusVisualStyle = null
            };

            StackPanel docPanel = new StackPanel();

            DadosDia melhorDia = null;
            if (dados != null && dados.Dias != null)
            {
                foreach (DadosDia d in dados.Dias)
                {
                    if (melhorDia == null || d.TotalMl > melhorDia.TotalMl) melhorDia = d;
                }
            }

            // --- Card 1: Diagnóstico de Bem-Estar da Lumi ---
            Border cardDiag = new Border
            {
                Background = escuro
                    ? (mediaDiaria >= dados.MetaDiaria
                        ? (Brush)new SolidColorBrush(Color.FromArgb(50, 16, 185, 129))
                        : (mediaDiaria >= dados.MetaDiaria * 0.75
                            ? (Brush)new SolidColorBrush(Color.FromArgb(50, 56, 189, 248))
                            : (Brush)new SolidColorBrush(Color.FromArgb(50, 245, 158, 11))))
                    : (mediaDiaria >= dados.MetaDiaria
                        ? (Brush)new SolidColorBrush(Color.FromRgb(240, 253, 244))
                        : (mediaDiaria >= dados.MetaDiaria * 0.75
                            ? (Brush)new SolidColorBrush(Color.FromRgb(240, 249, 255))
                            : (Brush)new SolidColorBrush(Color.FromRgb(254, 252, 232)))),
                BorderBrush = escuro
                    ? (mediaDiaria >= dados.MetaDiaria
                        ? (Brush)new SolidColorBrush(Color.FromArgb(140, 52, 211, 153))
                        : (mediaDiaria >= dados.MetaDiaria * 0.75
                            ? (Brush)new SolidColorBrush(Color.FromArgb(140, 56, 189, 248))
                            : (Brush)new SolidColorBrush(Color.FromArgb(140, 251, 191, 36))))
                    : (mediaDiaria >= dados.MetaDiaria
                        ? (Brush)new SolidColorBrush(Color.FromRgb(187, 247, 208))
                        : (mediaDiaria >= dados.MetaDiaria * 0.75
                            ? (Brush)new SolidColorBrush(Color.FromRgb(186, 230, 253))
                            : (Brush)new SolidColorBrush(Color.FromRgb(254, 240, 138)))),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(16, 13, 16, 13),
                Margin = new Thickness(0, 0, 0, 12)
            };

            StackPanel diagStack = new StackPanel();

            Grid diagHeader = new Grid();
            diagHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            diagHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel diagTitleLeft = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            diagTitleLeft.Children.Add(new TextBlock
            {
                Text = "\u2728 ",
                FontSize = 13.5
            });
            diagTitleLeft.Children.Add(new TextBlock
            {
                Text = "Diagn\u00F3stico de Bem-Estar da Lumi",
                FontSize = 12.5,
                FontWeight = FontWeights.Bold,
                Foreground = escuro ? Brushes.White : txtPreto,
                VerticalAlignment = VerticalAlignment.Center
            });
            Grid.SetColumn(diagTitleLeft, 0);
            diagHeader.Children.Add(diagTitleLeft);

            string badgeTexto;
            Brush badgeBg;
            Brush badgeBorder;
            Brush badgeFg;

            if (mediaDiaria >= dados.MetaDiaria)
            {
                badgeTexto = "\u2713 Excelente";
                badgeBg = escuro ? (Brush)new SolidColorBrush(Color.FromArgb(90, 6, 95, 70)) : new SolidColorBrush(Color.FromRgb(220, 252, 231));
                badgeBorder = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(52, 211, 153)) : new SolidColorBrush(Color.FromRgb(134, 239, 172));
                badgeFg = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(21, 128, 61));
            }
            else if (mediaDiaria >= dados.MetaDiaria * 0.75)
            {
                badgeTexto = "\uD83D\uDCA7 Muito Bom";
                badgeBg = escuro ? (Brush)new SolidColorBrush(Color.FromArgb(90, 12, 74, 110)) : new SolidColorBrush(Color.FromRgb(224, 242, 254));
                badgeBorder = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(125, 211, 252));
                badgeFg = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(3, 105, 161));
            }
            else
            {
                badgeTexto = "\u26A0\uFE0F Aten\u00E7\u00E3o \u00E0 Hidrata\u00E7\u00E3o";
                badgeBg = escuro ? (Brush)new SolidColorBrush(Color.FromArgb(90, 120, 53, 15)) : new SolidColorBrush(Color.FromRgb(254, 243, 199));
                badgeBorder = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(251, 191, 36)) : new SolidColorBrush(Color.FromRgb(252, 211, 77));
                badgeFg = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(180, 83, 9));
            }

            Border diagBadge = new Border
            {
                Background = badgeBg,
                BorderBrush = badgeBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10, 3, 10, 3),
                VerticalAlignment = VerticalAlignment.Center
            };
            diagBadge.Child = new TextBlock
            {
                Text = badgeTexto,
                FontSize = 10.5,
                FontWeight = FontWeights.Bold,
                Foreground = badgeFg
            };
            Grid.SetColumn(diagBadge, 1);
            diagHeader.Children.Add(diagBadge);
            diagStack.Children.Add(diagHeader);

            string mensagemDiag;
            if (mediaDiaria >= dados.MetaDiaria)
            {
                mensagemDiag = "Parab\u00E9ns! Sua rotina de hidrata\u00E7\u00E3o est\u00E1 impec\u00E1vel. Manter esse n\u00EDvel protege seus rins, melhora o foco cognitivo no computador e revigora sua disposi\u00E7\u00E3o f\u00EDsica e mental. Continue com essa const\u00E2ncia maravilhosa!";
            }
            else if (mediaDiaria >= dados.MetaDiaria * 0.75)
            {
                mensagemDiag = "Voc\u00EA est\u00E1 no caminho certo e muito perto da sua meta ideal! Seu h\u00E1bito est\u00E1 bem estabelecido. Uma dica da Lumi: experimente tomar um copo de \u00E1gua logo pela manh\u00E3 ou no meio da tarde para alcan\u00E7ar 100% de forma leve e natural.";
            }
            else
            {
                mensagemDiag = "Seu consumo m\u00E9dio est\u00E1 abaixo do recomendado para o seu bem-estar. Dica da Lumi: mantenha uma garrafinha sempre vis\u00EDvel na sua mesa e aproveite os lembretes do app para dar pequenos goles constantes ao longo do dia.";
            }

            diagStack.Children.Add(new TextBlock
            {
                Text = mensagemDiag,
                FontSize = 11.5,
                Foreground = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(241, 245, 249)) : new SolidColorBrush(Color.FromRgb(51, 65, 85)),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18,
                Margin = new Thickness(0, 8, 0, 0)
            });

            if (melhorDia != null && melhorDia.TotalMl > 0)
            {
                Border recordeBox = new Border
                {
                    Background = escuro ? (Brush)new SolidColorBrush(Color.FromArgb(60, 30, 41, 59)) : new SolidColorBrush(Color.FromArgb(120, 241, 245, 249)),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10, 5, 10, 5),
                    Margin = new Thickness(0, 9, 0, 0)
                };
                StackPanel recordeSp = new StackPanel { Orientation = Orientation.Horizontal };
                recordeSp.Children.Add(new TextBlock
                {
                    Text = "\uD83C\uDFC6 Recorde Pessoal: ",
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Foreground = escuro ? Brushes.White : txtPreto
                });
                recordeSp.Children.Add(new TextBlock
                {
                    Text = string.Format("{0:N0} ml em {1}", melhorDia.TotalMl, FormatarDataAmigavel(melhorDia.Data)),
                    FontSize = 11,
                    Foreground = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(56, 189, 248)) : txtDestaque,
                    FontWeight = FontWeights.SemiBold
                });
                recordeBox.Child = recordeSp;
                diagStack.Children.Add(recordeBox);
            }

            cardDiag.Child = diagStack;
            docPanel.Children.Add(cardDiag);

            // --- Card 2: Histórico Diário de Consumo ---
            Border cardHistorico = new Border
            {
                Background = miniCardBg,
                BorderBrush = miniCardBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(16, 13, 16, 13),
                Margin = new Thickness(0, 0, 0, 12)
            };

            StackPanel histStack = new StackPanel();

            Grid histHeader = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            histHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            histHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel histTitleSp = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            histTitleSp.Children.Add(new TextBlock
            {
                Text = "\uD83D\uDCC5 ",
                FontSize = 13
            });
            histTitleSp.Children.Add(new TextBlock
            {
                Text = "Hist\u00F3rico Di\u00E1rio de Consumo",
                FontSize = 12.5,
                FontWeight = FontWeights.Bold,
                Foreground = escuro ? Brushes.White : txtPreto,
                VerticalAlignment = VerticalAlignment.Center
            });
            Grid.SetColumn(histTitleSp, 0);
            histHeader.Children.Add(histTitleSp);

            TextBlock txtMetaRef = new TextBlock
            {
                Text = "Meta: " + dados.MetaDiaria + " ml",
                FontSize = 11,
                Foreground = txtMuted,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtMetaRef, 1);
            histHeader.Children.Add(txtMetaRef);

            histStack.Children.Add(histHeader);

            List<DadosDia> listaOrdenada = new List<DadosDia>(dados != null && dados.Dias != null ? dados.Dias : new List<DadosDia>());
            listaOrdenada.Sort(delegate(DadosDia a, DadosDia b) { return string.Compare(b.Data, a.Data); });

            if (listaOrdenada.Count == 0)
            {
                histStack.Children.Add(new TextBlock
                {
                    Text = "Nenhum dia registrado ainda. Comece a beber \u00E1gua para acompanhar seu progresso! \uD83D\uDCA7",
                    FontSize = 11.5,
                    Foreground = txtMuted,
                    Margin = new Thickness(0, 8, 0, 8)
                });
            }
            else
            {
                int maxExibir = Math.Min(30, listaOrdenada.Count);
                for (int i = 0; i < maxExibir; i++)
                {
                    DadosDia d = listaOrdenada[i];
                    histStack.Children.Add(CriarLinhaDiaVisual(d, dados.MetaDiaria, escuro, txtPreto, txtMuted));
                }

                if (listaOrdenada.Count > maxExibir)
                {
                    histStack.Children.Add(new TextBlock
                    {
                        Text = "+ " + (listaOrdenada.Count - maxExibir) + " dias anteriores dispon\u00EDveis nos arquivos TXT e CSV.",
                        FontSize = 10.5,
                        Foreground = txtMuted,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 8, 0, 0)
                    });
                }
            }

            cardHistorico.Child = histStack;
            docPanel.Children.Add(cardHistorico);

            // --- Card 3: Goles Registrados Hoje ---
            string hojeStr = DateTime.Today.ToString("yyyy-MM-dd");
            DadosDia diaHoje = listaOrdenada.Find(delegate(DadosDia x) { return x.Data == hojeStr; });

            Border cardHoje = new Border
            {
                Background = miniCardBg,
                BorderBrush = miniCardBorder,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(14),
                Padding = new Thickness(16, 13, 16, 13),
                Margin = new Thickness(0, 0, 0, 10)
            };

            StackPanel hojeStack = new StackPanel();

            Grid hojeHeader = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            hojeHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            hojeHeader.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel hojeTitleSp = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            hojeTitleSp.Children.Add(new TextBlock
            {
                Text = "\uD83D\uDCA7 ",
                FontSize = 13
            });
            hojeTitleSp.Children.Add(new TextBlock
            {
                Text = "Goles Registrados Hoje",
                FontSize = 12.5,
                FontWeight = FontWeights.Bold,
                Foreground = escuro ? Brushes.White : txtPreto,
                VerticalAlignment = VerticalAlignment.Center
            });
            Grid.SetColumn(hojeTitleSp, 0);
            hojeHeader.Children.Add(hojeTitleSp);

            int qtdGolesHoje = (diaHoje != null && diaHoje.Registros != null) ? diaHoje.Registros.Count : 0;
            int totalHoje = (diaHoje != null) ? diaHoje.TotalMl : 0;

            TextBlock txtResumoHoje = new TextBlock
            {
                Text = qtdGolesHoje + " gole(s) \u2022 Total: " + totalHoje + " ml",
                FontSize = 11,
                Foreground = txtMuted,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtResumoHoje, 1);
            hojeHeader.Children.Add(txtResumoHoje);
            hojeStack.Children.Add(hojeHeader);

            if (diaHoje != null && diaHoje.Registros != null && diaHoje.Registros.Count > 0)
            {
                WrapPanel wpGoles = new WrapPanel { Margin = new Thickness(0, 4, 0, 0) };
                foreach (RegistroAgua r in diaHoje.Registros)
                {
                    string iconeTipo = "\uD83E\uDD64";
                    string tipoLower = (r.Tipo ?? "").ToLower();
                    if (tipoLower.Contains("garrafa")) iconeTipo = "\uD83C\uDF7C";
                    else if (tipoLower.Contains("gole")) iconeTipo = "\uD83D\uDCA7";

                    Border chip = new Border
                    {
                        Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(15, 23, 42)) : new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                        BorderBrush = miniCardBorder,
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(8, 4, 8, 4),
                        Margin = new Thickness(0, 0, 6, 6)
                    };
                    StackPanel spChip = new StackPanel { Orientation = Orientation.Horizontal };
                    spChip.Children.Add(new TextBlock
                    {
                        Text = iconeTipo + " ",
                        FontSize = 10.5,
                        VerticalAlignment = VerticalAlignment.Center
                    });
                    spChip.Children.Add(new TextBlock
                    {
                        Text = r.Hora + " \u2022 ",
                        FontSize = 10.5,
                        Foreground = txtMuted,
                        VerticalAlignment = VerticalAlignment.Center
                    });
                    spChip.Children.Add(new TextBlock
                    {
                        Text = "+" + r.Quantidade + " ml",
                        FontSize = 10.5,
                        FontWeight = FontWeights.Bold,
                        Foreground = escuro ? Brushes.White : txtPreto,
                        VerticalAlignment = VerticalAlignment.Center
                    });
                    chip.Child = spChip;
                    wpGoles.Children.Add(chip);
                }
                hojeStack.Children.Add(wpGoles);
            }
            else
            {
                hojeStack.Children.Add(new TextBlock
                {
                    Text = "Nenhum gole registrado hoje ainda. Que tal beber um copinho de \u00E1gua agora? \uD83D\uDCA7",
                    FontSize = 11,
                    Foreground = txtMuted,
                    Margin = new Thickness(0, 4, 0, 2)
                });
            }

            cardHoje.Child = hojeStack;
            docPanel.Children.Add(cardHoje);

            // --- Rodapé de Metadados ---
            docPanel.Children.Add(new TextBlock
            {
                Text = "Emitido em " + DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm") + " \u2022 Aplicativo Lumi \u2728",
                FontSize = 10,
                Foreground = txtMuted,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 4, 0, 8)
            });

            scroll.Content = docPanel;
            Grid.SetRow(scroll, 2);
            layoutInterno.Children.Add(scroll);

            // 3: Botões de Ação no Rodapé
            Border footer = new Border
            {
                Margin = new Thickness(18, 0, 18, 16),
                Background = Brushes.Transparent
            };

            Grid footerGrid = new Grid();
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel leftButtons = new StackPanel { Orientation = Orientation.Horizontal };

            Button btnCopiar = new Button
            {
                Content = "\uD83D\uDCCB Copiar Texto",
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 8, 0),
                Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                BorderBrush = miniCardBorder,
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand
            };
            ConfigWindow.AplicarPillModal(btnCopiar, 10);
            btnCopiar.Click += delegate
            {
                try
                {
                    Clipboard.SetText(relatorioTxt);
                    GeradorDeSom.TocarGota();
                    btnCopiar.Content = "\u2713 Copiado!";
                    DispatcherTimer dt = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2.5) };
                    dt.Tick += delegate
                    {
                        btnCopiar.Content = "\uD83D\uDCCB Copiar Texto";
                        dt.Stop();
                    };
                    dt.Start();
                }
                catch (Exception ex)
                {
                    LumiDialog.MostrarAviso(this, "N\u00E3o foi poss\u00EDvel copiar para a \u00E1rea de transfer\u00EAncia: " + ex.Message, "Aviso", escuro);
                }
            };
            leftButtons.Children.Add(btnCopiar);

            Button btnSalvarTxt = new Button
            {
                Content = "\uD83D\uDCBE Salvar TXT",
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 8, 0),
                Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                BorderBrush = miniCardBorder,
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand
            };
            ConfigWindow.AplicarPillModal(btnSalvarTxt, 10);
            btnSalvarTxt.Click += delegate
            {
                try
                {
                    Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                    sfd.Title = "Salvar Relat\u00F3rio de Sa\u00FAde Lumi";
                    sfd.Filter = "Relat\u00F3rio de Texto (*.txt)|*.txt|Todos os arquivos (*.*)|*.*";
                    sfd.FileName = "Lumi_Relatorio_Saude_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                    if (sfd.ShowDialog(this) == true)
                    {
                        File.WriteAllText(sfd.FileName, relatorioTxt, Encoding.UTF8);
                        GeradorDeSom.TocarGota();
                        LumiDialog.MostrarSucesso(this, "Relat\u00F3rio de Sa\u00FAde salvo com sucesso!\n\n" + sfd.FileName, "Relat\u00F3rio Exportado! \uD83D\uDCC4", escuro);
                    }
                }
                catch (Exception ex)
                {
                    LumiDialog.MostrarAviso(this, "Erro ao salvar arquivo: " + ex.Message, "Erro", escuro);
                }
            };
            leftButtons.Children.Add(btnSalvarTxt);

            Button btnSalvarCsv = new Button
            {
                Content = "\uD83D\uDCCA Planilha CSV (Excel)",
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(12, 8, 12, 8),
                Background = new SolidColorBrush(Color.FromRgb(2, 132, 199)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };
            ConfigWindow.AplicarPillModal(btnSalvarCsv, 10);
            btnSalvarCsv.Click += delegate
            {
                try
                {
                    Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                    sfd.Title = "Salvar Planilha de Hidrata\u00E7\u00E3o Lumi (Excel)";
                    sfd.Filter = "Planilha CSV (*.csv)|*.csv|Todos os arquivos (*.*)|*.*";
                    sfd.FileName = "Lumi_Dados_Hidratacao_" + DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
                    if (sfd.ShowDialog(this) == true)
                    {
                        File.WriteAllText(sfd.FileName, relatorioCsv, Encoding.UTF8);
                        GeradorDeSom.TocarGota();
                        LumiDialog.MostrarSucesso(this, "Planilha CSV salva com sucesso!\nCompat\u00EDvel com Excel e Google Sheets.\n\n" + sfd.FileName, "Planilha Exportada! \uD83D\uDCCA", escuro);
                    }
                }
                catch (Exception ex)
                {
                    LumiDialog.MostrarAviso(this, "Erro ao salvar planilha: " + ex.Message, "Erro", escuro);
                }
            };
            leftButtons.Children.Add(btnSalvarCsv);

            Grid.SetColumn(leftButtons, 0);
            footerGrid.Children.Add(leftButtons);

            Button btnFechar = new Button
            {
                Content = "Fechar",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Padding = new Thickness(16, 8, 16, 8),
                Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(30, 41, 59)) : new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                Foreground = txtPreto,
                BorderBrush = miniCardBorder,
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand
            };
            ConfigWindow.AplicarPillModal(btnFechar, 10);
            btnFechar.Click += delegate
            {
                DoubleAnimation fo = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(160));
                fo.Completed += delegate { Close(); };
                BeginAnimation(UIElement.OpacityProperty, fo);
            };
            Grid.SetColumn(btnFechar, 1);
            footerGrid.Children.Add(btnFechar);

            footer.Child = footerGrid;
            Grid.SetRow(footer, 3);
            layoutInterno.Children.Add(footer);

            cardPrincipal.Child = layoutInterno;
            root.Children.Add(cardPrincipal);
            Content = root;
        }

        private static Border CriarMiniStatCard(Brush bg, Brush border, Brush txtTitle, Brush txtSub, string titulo, string valor, string sub, int col)
        {
            Border b = new Border
            {
                Background = bg,
                BorderBrush = border,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(8, 6, 8, 6),
                Margin = new Thickness(col == 0 ? 0 : 4, 0, col == 3 ? 0 : 4, 0)
            };
            StackPanel sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            sp.Children.Add(new TextBlock
            {
                Text = titulo,
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = txtSub,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            sp.Children.Add(new TextBlock
            {
                Text = valor,
                FontSize = 13.5,
                FontWeight = FontWeights.Bold,
                Foreground = txtTitle,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 1, 0, 0)
            });
            sp.Children.Add(new TextBlock
            {
                Text = sub,
                FontSize = 9.5,
                Foreground = txtSub,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            b.Child = sp;
            Grid.SetColumn(b, col);
            return b;
        }

        public static int CalcularStreakAtual(HistoricoApp dados)
        {
            if (dados == null || dados.Dias == null || dados.Dias.Count == 0) return 0;
            int streak = 0;
            DateTime dataAtual = DateTime.Today;

            string hojeStr = dataAtual.ToString("yyyy-MM-dd");
            DadosDia diaHoje = dados.Dias.Find(delegate(DadosDia x) { return x.Data == hojeStr; });
            if (diaHoje != null && diaHoje.TotalMl >= dados.MetaDiaria)
            {
                streak++;
                dataAtual = dataAtual.AddDays(-1);
            }
            else
            {
                dataAtual = dataAtual.AddDays(-1);
            }

            while (true)
            {
                string dStr = dataAtual.ToString("yyyy-MM-dd");
                DadosDia diaSalvo = dados.Dias.Find(delegate(DadosDia x) { return x.Data == dStr; });
                if (diaSalvo != null && diaSalvo.TotalMl >= dados.MetaDiaria)
                {
                    streak++;
                    dataAtual = dataAtual.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        private static FrameworkElement CriarLinhaDiaVisual(DadosDia d, int metaDiaria, bool escuro, Brush txtPreto, Brush txtMuted)
        {
            Border rowBorder = new Border
            {
                Padding = new Thickness(10, 7, 10, 7),
                Margin = new Thickness(0, 2, 0, 2),
                CornerRadius = new CornerRadius(8),
                Background = escuro ? (Brush)new SolidColorBrush(Color.FromArgb(40, 15, 23, 42)) : new SolidColorBrush(Color.FromArgb(60, 248, 250, 252))
            };

            Grid g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(145) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(125) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Col 0: Data
            string dataFormatada = FormatarDataAmigavel(d.Data);
            bool ehHoje = (d.Data == DateTime.Today.ToString("yyyy-MM-dd"));
            TextBlock txtData = new TextBlock
            {
                Text = dataFormatada,
                FontSize = 11.5,
                FontWeight = ehHoje ? FontWeights.Bold : FontWeights.Normal,
                Foreground = ehHoje
                    ? (escuro ? (Brush)new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199)))
                    : (escuro ? Brushes.White : txtPreto),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(txtData, 0);
            g.Children.Add(txtData);

            // Col 1: Barra de progresso
            int pct = (d.TotalMl * 100) / Math.Max(1, metaDiaria);
            double larguraTotal = 110;
            double preenchimento = Math.Min(larguraTotal, Math.Max(0, ((double)d.TotalMl / Math.Max(1, metaDiaria)) * larguraTotal));
            bool metaBatida = (d.TotalMl >= metaDiaria);

            Border progressTrack = new Border
            {
                Width = larguraTotal,
                Height = 8,
                CornerRadius = new CornerRadius(4),
                Background = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(226, 232, 240)),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };

            Border progressFill = new Border
            {
                Width = preenchimento,
                Height = 8,
                CornerRadius = new CornerRadius(4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = metaBatida
                    ? (escuro ? (Brush)new SolidColorBrush(Color.FromRgb(52, 211, 153)) : new SolidColorBrush(Color.FromRgb(16, 185, 129)))
                    : (escuro ? (Brush)new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(2, 132, 199)))
            };
            progressTrack.Child = progressFill;
            Grid.SetColumn(progressTrack, 1);
            g.Children.Add(progressTrack);

            // Col 2: Volume ml
            TextBlock txtVol = new TextBlock
            {
                Text = d.TotalMl.ToString("N0") + " ml",
                FontSize = 11.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = escuro ? Brushes.White : txtPreto,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 0)
            };
            Grid.SetColumn(txtVol, 2);
            g.Children.Add(txtVol);

            // Col 3: Badge
            Border badge = new Border
            {
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8, 2, 8, 2),
                VerticalAlignment = VerticalAlignment.Center
            };
            if (metaBatida)
            {
                badge.Background = escuro ? (Brush)new SolidColorBrush(Color.FromArgb(90, 6, 95, 70)) : new SolidColorBrush(Color.FromRgb(220, 252, 231));
                badge.BorderBrush = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(52, 211, 153)) : new SolidColorBrush(Color.FromRgb(134, 239, 172));
                badge.BorderThickness = new Thickness(1);
                badge.Child = new TextBlock
                {
                    Text = "\u2713 Batida",
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(21, 128, 61))
                };
            }
            else
            {
                badge.Background = escuro ? (Brush)new SolidColorBrush(Color.FromArgb(60, 51, 65, 85)) : new SolidColorBrush(Color.FromRgb(241, 245, 249));
                badge.BorderBrush = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(71, 85, 105)) : new SolidColorBrush(Color.FromRgb(226, 232, 240));
                badge.BorderThickness = new Thickness(1);
                badge.Child = new TextBlock
                {
                    Text = pct + "%",
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(226, 232, 240)) : txtMuted
                };
            }
            Grid.SetColumn(badge, 3);
            g.Children.Add(badge);

            rowBorder.Child = g;
            return rowBorder;
        }

        private static string FormatarDataAmigavel(string yyyyMmDd)
        {
            DateTime dt;
            if (DateTime.TryParse(yyyyMmDd, out dt))
            {
                if (dt.Date == DateTime.Today)
                {
                    return "Hoje (" + dt.ToString("dd/MM") + ")";
                }
                if (dt.Date == DateTime.Today.AddDays(-1))
                {
                    return "Ontem (" + dt.ToString("dd/MM") + ")";
                }
                try
                {
                    System.Globalization.CultureInfo ptBr = System.Globalization.CultureInfo.GetCultureInfo("pt-BR");
                    return dt.ToString("dd 'de' MMM (ddd)", ptBr);
                }
                catch
                {
                    return dt.ToString("dd/MM/yyyy");
                }
            }
            return yyyyMmDd;
        }

        private static string FormatarData(string yyyyMmDd)
        {
            DateTime dt;
            if (DateTime.TryParse(yyyyMmDd, out dt))
            {
                return dt.ToString("dd/MM/yyyy");
            }
            return yyyyMmDd;
        }

        public static string GerarTextoRelatorio(HistoricoApp dados)
        {
            if (dados == null) return "Nenhum dado dispon\u00EDvel.";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=================================================================");
            sb.AppendLine("           LUMI - RELAT\u00D3RIO DE SA\u00DADE & HIDRATA\u00C7\u00C3O \uD83D\uDCA7            ");
            sb.AppendLine("=================================================================");
            sb.AppendLine("Emitido em: " + DateTime.Now.ToString("dd/MM/yyyy 'às' HH:mm"));
            sb.AppendLine("Meta Di\u00E1ria Configurada: " + dados.MetaDiaria + " ml");
            sb.AppendLine();

            int totalDias = Math.Max(1, dados.Dias != null && dados.Dias.Count > 0 ? dados.Dias.Count : 1);
            int somaTotal = 0;
            int metasBatidas = 0;
            DadosDia melhorDia = null;

            if (dados.Dias != null)
            {
                foreach (DadosDia d in dados.Dias)
                {
                    somaTotal += d.TotalMl;
                    if (d.TotalMl >= dados.MetaDiaria) metasBatidas++;
                    if (melhorDia == null || d.TotalMl > melhorDia.TotalMl) melhorDia = d;
                }
            }

            double mediaDiaria = (double)somaTotal / totalDias;
            double taxaSucesso = ((double)metasBatidas / totalDias) * 100.0;
            int streak = CalcularStreakAtual(dados);

            sb.AppendLine("RESUMO EXECUTIVO");
            sb.AppendLine("-----------------------------------------------------------------");
            sb.AppendLine(string.Format("\u2022 Total de Dias Monitorados : {0} dia(s)", totalDias));
            sb.AppendLine(string.Format("\u2022 Volume Total Consumido    : {0:N0} ml ({1:0.0} Litros)", somaTotal, somaTotal / 1000.0));
            sb.AppendLine(string.Format("\u2022 M\u00E9dia Di\u00E1ria              : {0:N0} ml/dia", Math.Round(mediaDiaria)));
            sb.AppendLine(string.Format("\u2022 Metas Di\u00E1rias Batidas     : {0} de {1} ({2:0}%)", metasBatidas, totalDias, taxaSucesso));
            sb.AppendLine(string.Format("\u2022 Sequ\u00EAncia Atual de Metas  : {0} dia(s) consecutivo(s)", streak));
            if (melhorDia != null)
            {
                sb.AppendLine(string.Format("\u2022 Recorde em um \u00DAnico Dia   : {0:N0} ml em {1}", melhorDia.TotalMl, FormatarData(melhorDia.Data)));
            }

            sb.AppendLine();
            sb.AppendLine("AVALIA\u00C7\u00C3O DE HIDRATA\u00C7\u00C3O");
            sb.AppendLine("-----------------------------------------------------------------");
            if (mediaDiaria >= dados.MetaDiaria)
            {
                sb.AppendLine("Status: EXCELENTE \uD83C\uDF1F");
                sb.AppendLine("Parab\u00E9ns! Sua rotina de hidrata\u00E7\u00E3o est\u00E1 impec\u00E1vel. Manter esse n\u00EDvel");
                sb.AppendLine("protege a fun\u00E7\u00E3o renal, melhora o foco cognitivo no computador");
                sb.AppendLine("e revigora a sa\u00FAde da pele e energia do corpo.");
            }
            else if (mediaDiaria >= dados.MetaDiaria * 0.75)
            {
                sb.AppendLine("Status: MUITO BOM (Perto da meta) \uD83D\uDCA7");
                sb.AppendLine("Voc\u00EA est\u00E1 no caminho certo! Tente adicionar apenas um copo extra");
                sb.AppendLine("pela manh\u00E3 ou pela tarde para atingir 100% da sua meta todos os dias.");
            }
            else
            {
                sb.AppendLine("Status: ATEN\u00C7\u00C3O \u00C0 HIDRATA\u00C7\u00C3O \u26A0\uFE0F");
                sb.AppendLine("Seu consumo m\u00E9dio est\u00E1 abaixo do recomendado. Dica da Lumi:");
                sb.AppendLine("Mantenha uma garrafa de \u00E1gua por perto e aproveite os lembretes");
                sb.AppendLine("autom\u00E1ticos para dar pequenos goles constantes.");
            }

            sb.AppendLine();
            sb.AppendLine("HIST\u00D3RICO DI\u00C1RIO DETALHADO");
            sb.AppendLine("-----------------------------------------------------------------");
            sb.AppendLine(string.Format("{0,-12} | {1,-10} | {2,-8} | {3,-7} | {4}", "DATA", "CONSUMO", "META", "% META", "SITUA\u00C7\u00C3O"));
            sb.AppendLine("-----------------------------------------------------------------");

            List<DadosDia> listaOrdenada = new List<DadosDia>(dados.Dias != null ? dados.Dias : new List<DadosDia>());
            listaOrdenada.Sort(delegate(DadosDia a, DadosDia b) { return string.Compare(b.Data, a.Data); });

            foreach (DadosDia d in listaOrdenada)
            {
                int pct = (d.TotalMl * 100) / Math.Max(1, dados.MetaDiaria);
                string situacao = (d.TotalMl >= dados.MetaDiaria) ? "Meta Batida! \uD83C\uDFC6" : (pct >= 75 ? "Bom \uD83D\uDCA7" : "Abaixo \u26A0\uFE0F");
                sb.AppendLine(string.Format("{0,-12} | {1,7} ml | {2,5} ml | {3,5}% | {4}", FormatarData(d.Data), d.TotalMl, dados.MetaDiaria, pct, situacao));
            }

            string hojeStr = DateTime.Today.ToString("yyyy-MM-dd");
            DadosDia diaHoje = listaOrdenada.Find(delegate(DadosDia x) { return x.Data == hojeStr; });
            if (diaHoje != null && diaHoje.Registros != null && diaHoje.Registros.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine(string.Format("REGISTROS INDIVIDUAIS DE HOJE ({0}):", FormatarData(hojeStr)));
                sb.AppendLine("-----------------------------------------------------------------");
                for (int i = 0; i < diaHoje.Registros.Count; i++)
                {
                    RegistroAgua r = diaHoje.Registros[i];
                    sb.AppendLine(string.Format("  {0}. [{1}] +{2} ml ({3})", i + 1, r.Hora, r.Quantidade, r.Tipo));
                }
            }

            sb.AppendLine();
            sb.AppendLine("=================================================================");
            sb.AppendLine("Gerado automaticamente pelo aplicativo Lumi - Hidrata\u00E7\u00E3o Di\u00E1ria");
            sb.AppendLine("=================================================================");

            return sb.ToString();
        }

        public static string GerarCsvRelatorio(HistoricoApp dados)
        {
            if (dados == null) return "";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Data;Total_ml;Meta_ml;Percentual;Situacao;Qtd_Goles");

            List<DadosDia> listaOrdenada = new List<DadosDia>(dados.Dias != null ? dados.Dias : new List<DadosDia>());
            listaOrdenada.Sort(delegate(DadosDia a, DadosDia b) { return string.Compare(b.Data, a.Data); });

            foreach (DadosDia d in listaOrdenada)
            {
                int pct = (d.TotalMl * 100) / Math.Max(1, dados.MetaDiaria);
                string situacao = (d.TotalMl >= dados.MetaDiaria) ? "Meta Batida" : (pct >= 75 ? "Bom" : "Abaixo da Meta");
                int qtdGoles = (d.Registros != null ? d.Registros.Count : 0);
                sb.AppendLine(string.Format("{0};{1};{2};{3}%;{4};{5}", d.Data, d.TotalMl, dados.MetaDiaria, pct, situacao, qtdGoles));
            }

            sb.AppendLine();
            sb.AppendLine("DETALHAMENTO DE CADA GOLE");
            sb.AppendLine("Data;Hora;Quantidade_ml;Tipo");
            foreach (DadosDia d in listaOrdenada)
            {
                if (d.Registros != null)
                {
                    foreach (RegistroAgua r in d.Registros)
                    {
                        sb.AppendLine(string.Format("{0};{1};{2};{3}", d.Data, r.Hora, r.Quantidade, (r.Tipo ?? "").Replace(';', ' ')));
                    }
                }
            }

            return sb.ToString();
        }
    }

    // ==========================================
    // Janela Modal: Configurações com Toggle Nativo & Suporte a Modo Escuro
    // ==========================================
    public class ConfigWindow : Window
    {
        public ConfigWindow(HistoricoApp dados, Action onSalvar)
        {
            bool escuro = (dados != null && dados.ModoEscuro);
            MainWindow.AplicarEstiloScrollBar(this, escuro);

            Title = "Configura\u00E7\u00F5es da Lumi \u2699\uFE0F";
            Width = 440;
            Height = Math.Min(560, SystemParameters.WorkArea.Height - 40);
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;
            Opacity = 0;

            Icon = IconeHelper.ObterIconeJanela();

            Loaded += delegate {
                DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220)) {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                BeginAnimation(UIElement.OpacityProperty, fadeIn);
            };

            ScrollViewer sv = new ScrollViewer {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0)
            };
            sv.PreviewMouseWheel += delegate(object s, MouseWheelEventArgs e)
            {
                ScrollViewer scv = s as ScrollViewer;
                if (scv != null && scv.ScrollableHeight > 0)
                {
                    scv.ScrollToVerticalOffset(scv.VerticalOffset - (e.Delta > 0 ? 45 : -45));
                    e.Handled = true;
                }
            };
            StackPanel sp = new StackPanel { Margin = new Thickness(22) };

            // Cabeçalho da Janela
            StackPanel headerSp = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
            headerSp.Children.Add(new TextBlock
            {
                Text = "Configura\u00E7\u00F5es & Lembretes",
                FontSize = 17,
                FontWeight = FontWeights.Bold,
                Foreground = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : new SolidColorBrush(Color.FromRgb(15, 23, 42))
            });
            headerSp.Children.Add(new TextBlock
            {
                Text = "Ajuste sua meta di\u00E1ria e prefer\u00EAncias do Windows",
                FontSize = 11,
                Foreground = escuro ? new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                Margin = new Thickness(0, 3, 0, 0)
            });
            sp.Children.Add(headerSp);

            Brush cardBg = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(30, 41, 59)) : Brushes.White;
            Brush cardBorder = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(226, 232, 240));
            Brush textTitle = escuro ? (Brush)Brushes.White : new SolidColorBrush(Color.FromRgb(30, 41, 59));
            Brush textDesc = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(148, 163, 184)) : new SolidColorBrush(Color.FromRgb(100, 116, 139));
            Brush inputBg = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(15, 23, 42)) : Brushes.White;
            Brush inputBorder = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(71, 85, 105)) : new SolidColorBrush(Color.FromRgb(203, 213, 225));
            Brush inputFg = escuro ? (Brush)new SolidColorBrush(Color.FromRgb(248, 250, 252)) : new SolidColorBrush(Color.FromRgb(15, 23, 42));

            // Card 1: Meta Diária
            Border cardMeta = new Border
            {
                Background = cardBg,
                CornerRadius = new CornerRadius(12),
                BorderBrush = cardBorder,
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
                Foreground = textTitle,
                Margin = new Thickness(0, 0, 0, 6)
            });

            TextBox txtMeta = new TextBox
            {
                Text = dados.MetaDiaria.ToString(),
                FontSize = 13,
                Padding = new Thickness(8, 6, 8, 6),
                Background = inputBg,
                Foreground = inputFg,
                BorderBrush = inputBorder,
                CaretBrush = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : null,
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
                Background = inputBg,
                Foreground = inputFg,
                BorderBrush = inputBorder,
                CaretBrush = escuro ? new SolidColorBrush(Color.FromRgb(56, 189, 248)) : null
            };
            Grid.SetColumn(txtPeso, 0);
            pesoGrid.Children.Add(txtPeso);

            Button btnCalcular = new Button
            {
                Content = "Calcular p/ Peso",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Padding = new Thickness(12, 6, 12, 6),
                Margin = new Thickness(8, 0, 0, 0),
                Background = escuro ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand
            };
            AplicarPillModal(btnCalcular, 8);
            btnCalcular.Click += delegate
            {
                double peso;
                string pesoStr = txtPeso.Text != null ? txtPeso.Text.Trim().Replace(',', '.') : "";
                if (double.TryParse(pesoStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out peso) && peso >= 20 && peso <= 300)
                {
                    int recomendacao = (int)Math.Round(peso * 35);
                    txtMeta.Text = recomendacao.ToString();
                }
                else
                {
                    LumiDialog.MostrarAviso(this, "Por favor, digite um peso v\u00E1lido em kg (ex: 65) para calcular a sua meta recomendada.", "Aviso de Peso \u2696\uFE0F", escuro);
                }
            };
            Grid.SetColumn(btnCalcular, 1);
            pesoGrid.Children.Add(btnCalcular);
            spMeta.Children.Add(pesoGrid);
            cardMeta.Child = spMeta;
            sp.Children.Add(cardMeta);

            // Card 2: Notificações Nativas
            Border cardNotif = new Border
            {
                Background = cardBg,
                CornerRadius = new CornerRadius(12),
                BorderBrush = cardBorder,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 0, 0, 12)
            };
            Grid notifGrid = new Grid();
            notifGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            notifGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel notifTextSp = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            notifTextSp.Children.Add(new TextBlock
            {
                Text = "\uD83D\uDD14 Notifica\u00E7\u00F5es do Windows",
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Foreground = textTitle
            });
            notifTextSp.Children.Add(new TextBlock
            {
                Text = "Exibe lembretes na Central de A\u00E7\u00F5es no canto inferior direito, mesmo em segundo plano.",
                FontSize = 10,
                Foreground = textDesc,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 8, 0)
            });
            Grid.SetColumn(notifTextSp, 0);
            notifGrid.Children.Add(notifTextSp);

            ModernToggleSwitch toggleNotif = new ModernToggleSwitch(dados.NotificacoesAtivadas, escuro);
            Grid.SetColumn(toggleNotif, 1);
            notifGrid.Children.Add(toggleNotif);

            cardNotif.Child = notifGrid;
            sp.Children.Add(cardNotif);

            // Card 3: Efeitos Sonoros
            Border cardSom = new Border
            {
                Background = cardBg,
                CornerRadius = new CornerRadius(12),
                BorderBrush = cardBorder,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 0, 0, 12)
            };
            Grid somGrid = new Grid();
            somGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            somGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel somTextSp = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            somTextSp.Children.Add(new TextBlock
            {
                Text = "\uD83D\uDD0A Sons de Gota e Sino",
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Foreground = textTitle
            });
            somTextSp.Children.Add(new TextBlock
            {
                Text = "Tocar efeitos sonoros relaxantes ao registrar \u00E1gua e alertar.",
                FontSize = 10,
                Foreground = textDesc,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 8, 0)
            });
            Grid.SetColumn(somTextSp, 0);
            somGrid.Children.Add(somTextSp);

            ModernToggleSwitch toggleSom = new ModernToggleSwitch(dados.SomAtivado, escuro);
            Grid.SetColumn(toggleSom, 1);
            somGrid.Children.Add(toggleSom);

            cardSom.Child = somGrid;
            sp.Children.Add(cardSom);

            // Card 4: Modo Escuro
            Border cardTema = new Border
            {
                Background = cardBg,
                CornerRadius = new CornerRadius(12),
                BorderBrush = cardBorder,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 0, 0, 16)
            };
            Grid temaGrid = new Grid();
            temaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            temaGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel temaTextSp = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            temaTextSp.Children.Add(new TextBlock
            {
                Text = "\uD83C\uDF19 Modo Escuro (Oceano Noturno)",
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Foreground = textTitle
            });
            temaTextSp.Children.Add(new TextBlock
            {
                Text = "Visual confort\u00E1vel para os olhos e uso noturno, com tema azul marinho profundo.",
                FontSize = 10,
                Foreground = textDesc,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 8, 0)
            });
            Grid.SetColumn(temaTextSp, 0);
            temaGrid.Children.Add(temaTextSp);

            ModernToggleSwitch toggleTema = new ModernToggleSwitch(dados.ModoEscuro, escuro);
            Grid.SetColumn(toggleTema, 1);
            temaGrid.Children.Add(toggleTema);

            cardTema.Child = temaGrid;
            sp.Children.Add(cardTema);

            // Card 5: Relatório de Saúde
            Border cardExport = new Border
            {
                Background = cardBg,
                CornerRadius = new CornerRadius(12),
                BorderBrush = cardBorder,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 0, 0, 16)
            };
            Grid exportGrid = new Grid();
            exportGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            exportGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel exportTextSp = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            exportTextSp.Children.Add(new TextBlock
            {
                Text = "\uD83D\uDCCA Relat\u00F3rio de Sa\u00FAde & Hidrata\u00E7\u00E3o",
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                Foreground = textTitle
            });
            exportTextSp.Children.Add(new TextBlock
            {
                Text = "Exporte seus dados em relat\u00F3rio TXT ou planilha CSV para nutricionistas ou controle pessoal.",
                FontSize = 10,
                Foreground = textDesc,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 8, 0)
            });
            Grid.SetColumn(exportTextSp, 0);
            exportGrid.Children.Add(exportTextSp);

            Button btnAbrirRelatorio = new Button
            {
                Content = "Ver Relat\u00F3rio \uD83D\uDCCB",
                FontSize = 11,
                FontWeight = FontWeights.SemiBold,
                Padding = new Thickness(12, 6, 12, 6),
                Background = escuro ? new SolidColorBrush(Color.FromRgb(23, 32, 54)) : new SolidColorBrush(Color.FromRgb(241, 245, 249)),
                Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(30, 41, 59)),
                BorderBrush = escuro ? new SolidColorBrush(Color.FromRgb(51, 65, 85)) : new SolidColorBrush(Color.FromRgb(203, 213, 225)),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand
            };
            AplicarPillModal(btnAbrirRelatorio, 8);
            btnAbrirRelatorio.Click += delegate
            {
                RelatorioSaudeWindow rw = new RelatorioSaudeWindow(dados);
                rw.Owner = this;
                rw.ShowDialog();
            };
            Grid.SetColumn(btnAbrirRelatorio, 1);
            exportGrid.Children.Add(btnAbrirRelatorio);

            cardExport.Child = exportGrid;
            sp.Children.Add(cardExport);

            // Botão Salvar
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
            AplicarPillModal(btnSalvar, 12);
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
                    fadeOut.Completed += delegate { Close(); };
                    BeginAnimation(UIElement.OpacityProperty, fadeOut);
                }
                else
                {
                    LumiDialog.MostrarAviso(this, "Por favor, informe uma meta di\u00E1ria v\u00E1lida entre 500 ml e 8000 ml.", "Meta Inv\u00E1lida \uD83D\uDCA7", escuro);
                }
            };
            sp.Children.Add(btnSalvar);

            sv.Content = sp;

            Grid root = new Grid { Margin = new Thickness(14) };

            Border cardPrincipal = new Border
            {
                CornerRadius = new CornerRadius(20),
                BorderThickness = new Thickness(1.5),
                BorderBrush = escuro
                    ? (Brush)new SolidColorBrush(Color.FromArgb(140, 56, 189, 248))
                    : new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                Background = escuro
                    ? (Brush)new SolidColorBrush(Color.FromRgb(15, 23, 42))
                    : new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                Effect = new DropShadowEffect
                {
                    Color = escuro ? Colors.Black : Color.FromRgb(2, 132, 199),
                    BlurRadius = 28,
                    ShadowDepth = 4,
                    Opacity = escuro ? 0.6 : 0.22
                }
            };

            Grid layoutInterno = new Grid();
            layoutInterno.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layoutInterno.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            Grid titleBar = new Grid
            {
                Height = 38,
                Margin = new Thickness(18, 10, 12, 0),
                Background = Brushes.Transparent
            };
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            titleBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel titleLeft = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            titleLeft.Children.Add(new TextBlock
            {
                Text = "\u2699\uFE0F ",
                FontSize = 13
            });
            titleLeft.Children.Add(new TextBlock
            {
                Text = "Configura\u00E7\u00F5es da Lumi",
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(3, 105, 161)),
                VerticalAlignment = VerticalAlignment.Center
            });
            Grid.SetColumn(titleLeft, 0);
            titleBar.Children.Add(titleLeft);

            Button btnFecharTopo = new Button
            {
                Content = "\u2715",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Width = 28,
                Height = 28,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(100, 116, 139)),
                Cursor = System.Windows.Input.Cursors.Hand,
                FocusVisualStyle = null
            };
            btnFecharTopo.MouseEnter += delegate
            {
                btnFecharTopo.Foreground = escuro ? new SolidColorBrush(Color.FromRgb(248, 113, 113)) : new SolidColorBrush(Color.FromRgb(225, 29, 72));
            };
            btnFecharTopo.MouseLeave += delegate
            {
                btnFecharTopo.Foreground = escuro ? Brushes.White : new SolidColorBrush(Color.FromRgb(100, 116, 139));
            };
            btnFecharTopo.Click += delegate
            {
                DoubleAnimation fo = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(160));
                fo.Completed += delegate { Close(); };
                BeginAnimation(UIElement.OpacityProperty, fo);
            };
            Grid.SetColumn(btnFecharTopo, 1);
            titleBar.Children.Add(btnFecharTopo);

            titleBar.MouseDown += delegate(object s, System.Windows.Input.MouseButtonEventArgs e)
            {
                if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                {
                    try { DragMove(); } catch { }
                }
            };
            Grid.SetRow(titleBar, 0);
            layoutInterno.Children.Add(titleBar);

            Grid.SetRow(sv, 1);
            layoutInterno.Children.Add(sv);

            cardPrincipal.Child = layoutInterno;
            root.Children.Add(cardPrincipal);
            Content = root;
        }

        public static void AplicarPillModal(Button btn, double cornerRadius)
        {
            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(cornerRadius));
            borderFactory.SetBinding(Border.BackgroundProperty, new Binding("Background") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.BorderBrushProperty, new Binding("BorderBrush") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.BorderThicknessProperty, new Binding("BorderThickness") { RelativeSource = RelativeSource.TemplatedParent });
            borderFactory.SetBinding(Border.PaddingProperty, new Binding("Padding") { RelativeSource = RelativeSource.TemplatedParent });

            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFactory.AppendChild(presenter);

            template.VisualTree = borderFactory;
            btn.Template = template;
        }
    }

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
