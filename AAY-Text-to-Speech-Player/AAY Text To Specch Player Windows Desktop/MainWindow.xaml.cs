using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Speech.Synthesis;
using System.Runtime.InteropServices;
using NAudio.Wave;
using NAudio.Lame;
using System.Windows.Automation;
using System.Windows.Interop;
using System.ComponentModel;
using System.Threading;

namespace AAY_Text_To_Specch_Player_Windows_Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();
        SpeechSynthesizer readerspeak = new SpeechSynthesizer();
        System.Windows.Forms.TextBox Speaktxt = new System.Windows.Forms.TextBox();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Volumebar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                int temp = (int)e.NewValue;
                VolumeState.Content = temp;
                readerspeak.Volume = temp;
            }
            catch
            {

            }
        }

        private void RateBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                int temp = (int)e.NewValue;
                RateState.Content = temp;
                readerspeak.Rate = temp;
            }
            catch
            {

            }
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        #region Minumize
        bool minumizing = false;
        double savedx = 0;
        double savedy = 0;
        private void Minumizing_Click(object sender, RoutedEventArgs e)
        {
            minumizing = !minumizing;
            double width = System.Windows.SystemParameters.PrimaryScreenWidth;
            double height = System.Windows.SystemParameters.PrimaryScreenHeight;
            if (minumizing)
            {
                savedx = this.Top;
                savedy = this.Left;
                this.Height = 50;
                this.Width = 100;
                this.Top = height - 70;
                this.Left = 5;
            }
            else
            {
                this.Height = 230;
                this.Width = 294;
                this.Top = savedx;
                this.Left = savedy;
            }
        }
        #endregion

        private void PlayAction_Click(object sender, RoutedEventArgs e)
        {
            voice();
        }

#region mousefinder
        Mousefind Findmouse = new Mousefind();

        #region MouseXY
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }
        #endregion

        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);
#endregion

        private void AAYTextToSpeechPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            readerspeak.SetOutputToDefaultAudioDevice();
            readerspeak.SpeakProgress += new EventHandler<SpeakProgressEventArgs>(synth_SpeakProgress);
            readerspeak.SpeakCompleted += new EventHandler<SpeakCompletedEventArgs>(synth_Speakcomplete);
            readerspeak.Rate = (int)RateBar.Value;
            RateState.Content = readerspeak.Rate;
            VolumeState.Content = readerspeak.Volume;
            RateBar.Value = readerspeak.Rate;
            System.Timers.Timer t = new System.Timers.Timer();
            t.Interval = 10; //In milliseconds here
            t.AutoReset = true; //Stops it from repeating
            t.Elapsed += new System.Timers.ElapsedEventHandler(TimerElapsed);
            t.Start();
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            int keyState = GetAsyncKeyState(162);
            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (keyState == -32767)
                {
                    Findmouse.Show();
                    Findmouse.Left = GetMousePosition().X + 5 - (Findmouse.Width / 2);
                    Findmouse.Top = GetMousePosition().Y - (Findmouse.Height / 2);
                }
                else if (keyState == 0)
                {
                    Findmouse.Hide();
                }
            }));
        }

        private void synth_Speakcomplete(object sender, SpeakCompletedEventArgs e)
        {
            startplay = false;
            PlayAction.Content = "Play";
            playpause = true;
            Stopbtn.IsEnabled = false;
        }

        private void synth_SpeakProgress(object sender, SpeakProgressEventArgs e)
        {
            SpokeWord.Content = "Word: " + e.Text;
        }

        #region Speaking Voice
        void SpeakText(bool switching, bool Speak)
        {
            if (!GetText())
                Speaktxt.Text = Clipboard.GetText();

            new Thread(() => 
            {
                if (switching)
                {
                    this.Dispatcher.BeginInvoke((Action)(() => {
                        Wordpro.Visibility = System.Windows.Visibility.Visible;
                        Wordpro.Maximum = Speaktxt.Text.Length;
                        Wordpro.Value = 0;
                        foreach (string textchar in Speaktxt.Lines)
                        {
                            FixTextTxt.Text += textchar + " ";
                            /*if (textchar >= '0' && textchar <= '9' || textchar >= 'A' && textchar <= 'Z'
                                || textchar >= 'a' && textchar <= 'z' || textchar == '/' || textchar == '?'
                                || textchar == '=' || textchar == '-' || textchar == '+' || textchar == '<'
                                || textchar == '>' || textchar == '(' || textchar == ')' || textchar == '\''
                                || textchar == ' ' || textchar == ',' || textchar == '.' || textchar == ';'
                                || textchar == ':' || textchar == '"' || textchar == ' ' || textchar == '\\')
                            {
                                FixTextTxt.Text += textchar;
                            }
                            else
                            {
                                FixTextTxt.Text += " ";
                            }*/
                            Wordpro.Value++;
                        }
                        Wordpro.Visibility = System.Windows.Visibility.Hidden;
                        if (Speak)
                        {
                            p = new Prompt(FixTextTxt.Text);

                            readerspeak.SpeakAsync(p);
                        }
                    }));
                }
                else
                {
                    this.Dispatcher.BeginInvoke((Action)(() => 
                    {
                        FixTextTxt.Text = Speaktxt.Text;
                        if (Speak)
                        {
                            p = new Prompt(FixTextTxt.Text);
                            readerspeak.SpeakAsync(p);
                        }
                    }));
                }
            }).Start();
        }

        bool playpause = true;
        bool startplay = false;
        Prompt p;
        public void voice()
        {
            FixTextTxt.Text = "";
            if (startplay)
            {
                if (playpause)
                {
                    playpause = false;
                    readerspeak.Resume();
                    PlayAction.Content = "Pause";
                }
                else
                {
                    playpause = true;
                    readerspeak.Pause();
                    PlayAction.Content = "Resume";
                }
            }
            else if (!startplay)
            {
                SpeakText(ErrorFix.IsChecked.Value, true);
                // Speak a string asynchronously.
                startplay = true;
                playpause = false;
                PlayAction.Content = "Pause";
                Stopbtn.IsEnabled = true;
            }
        }
        #endregion

        private void Stopbtn_Click(object sender, RoutedEventArgs e)
        {
            readerspeak.Resume();
            readerspeak.SpeakAsyncCancelAll();
            Stopbtn.IsEnabled = false;
        }

        #region Text To Wave
        private void Converttowave_Click(object sender, RoutedEventArgs e)
        {
            SpeakText(ErrorFix.IsChecked.Value, false);
            Microsoft.Win32.SaveFileDialog save = new Microsoft.Win32.SaveFileDialog();
            save.Filter = "Wave file (.wav)|*.wav";
            save.ShowDialog();
            readerspeak.SpeakAsync("beginning conversion");
            if (!(save.FileName == null))
            {
                try
                {
                    using (SpeechSynthesizer synth = new SpeechSynthesizer())
                    {
                        // Configure the audio output. 
                        synth.SetOutputToWaveFile(save.FileName,
                          new System.Speech.AudioFormat.SpeechAudioFormatInfo(32000, System.Speech.AudioFormat.AudioBitsPerSample.Sixteen, System.Speech.AudioFormat.AudioChannel.Mono));

                        // Create a SoundPlayer instance to play output audio file.
                        synth.Rate = (int)RateState.Content;
                        synth.Volume = (int)VolumeState.Content;
                        // Build a prompt.
                        PromptBuilder builder = new PromptBuilder();
                        builder.AppendText(FixTextTxt.Text);

                        // Speak the prompt.

                        synth.Speak(builder);
                    }
                    readerspeak.SpeakAsync("conversion complete");
                }
                catch (Exception g)
                {
                    readerspeak.SpeakAsync("Could not convert do to an error: " + g);
                }
            }
        }
        #endregion

        #region Text to MP3
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SpeakText(ErrorFix.IsChecked.Value, false);
            SpeechSynthesizer readerspeakmp3 = new SpeechSynthesizer();
            readerspeakmp3.Rate = (int)RateState.Content;
            readerspeakmp3.Volume = (int)VolumeState.Content;
            Microsoft.Win32.SaveFileDialog save = new Microsoft.Win32.SaveFileDialog();
            save.Filter = "Mp3 file (.mp3)|*.mp3";
            save.ShowDialog();
            if (!(save.FileName == null))
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                readerspeakmp3.SetOutputToWaveStream(ms);
                readerspeakmp3.Speak(FixTextTxt.Text);

                //do speaking
                readerspeak.SpeakAsync("beginning conversion");

                //now convert to mp3 using LameEncoder or shell out to audiograbber
                try
                {
                    ConvertWavStreamToMp3File(ref ms, save.FileName);
                    readerspeak.SpeakAsync("conversion complete");
                }
                catch (Exception g)
                {
                    readerspeak.SpeakAsync("Could not convert do to an error: " + g);
                }

            }
        }
        public static void ConvertWavStreamToMp3File(ref System.IO.MemoryStream ms, string savetofilename)
        {
            //rewind to beginning of stream
            ms.Seek(0, System.IO.SeekOrigin.Begin);

            using (var retMs = new System.IO.MemoryStream())
            using (var rdr = new WaveFileReader(ms))
            using (var wtr = new LameMP3FileWriter(savetofilename, rdr.WaveFormat, LAMEPreset.VBR_90))
            {
                rdr.CopyTo(wtr);
            }
        }
        #endregion

        bool Grabtext()
        {
            try
            {
                var element = AutomationElement.FocusedElement;
                if (element != null)
                {
                    object pattern = null;
                    if (element.TryGetCurrentPattern(TextPattern.Pattern, out pattern))
                    {
                        var tp = (TextPattern)pattern;
                        var sb = new StringBuilder();

                        foreach (var r in tp.GetSelection())
                        {
                            sb.AppendLine(r.GetText(-1));
                        }
                        Speaktxt.Text = sb.ToString();
                        return true;
                    }
                }
            }
            catch
            {

            }
            return false;
        }

        bool GetText()
        {
            if(!Grabtext())
            {
                
                PressKey(System.Windows.Forms.Keys.ControlKey, false);
                PressKey(System.Windows.Forms.Keys.C, false);
                PressKey(System.Windows.Forms.Keys.C, true);
                PressKey(System.Windows.Forms.Keys.ControlKey, true);
                System.Threading.Thread.Sleep(1000);
                return false;
            }
            return true;
        }

        #region SendKeys
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
        public static void PressKey(System.Windows.Forms.Keys key, bool up)
        {
            const int KEYEVENTF_EXTENDEDKEY = 0x1;
            const int KEYEVENTF_KEYUP = 0x2;
            if (up)
            {
                keybd_event((byte)key, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, (UIntPtr)0);
            }
            else
            {
                keybd_event((byte)key, 0x45, KEYEVENTF_EXTENDEDKEY, (UIntPtr)0);
            }
        }
        #endregion

        #region never become focused
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            //Set the window style to noactivate.
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SetWindowLong(helper.Handle, GWL_EXSTYLE,
                GetWindowLong(helper.Handle, GWL_EXSTYLE) | WS_EX_NOACTIVATE);
        }

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        #endregion
    }
}
