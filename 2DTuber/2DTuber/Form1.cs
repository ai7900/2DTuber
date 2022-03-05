using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2DTuber
{

    public partial class Form1 : Form
    {
        static class Global
        {
            private static bool _isTalking = false;
            private static float _inputSencetivity = 0.10f;
            private static int _audioBufferMilliseconds = 100;

            private static string _cursorName = "";

            private static string _idlePose = "";
            private static string _talkPose = "";
            private static string _background = "";
            private static string _placeholder = "";

            public static bool IsTalking
            {
                get { return _isTalking; }
                set { _isTalking = value; }
            }
            public static float InputSencetivity
            {
                get { return _inputSencetivity; }
                set { _inputSencetivity = value; }
            }
            public static int AudioBufferMilliseconds
            {
                get { return _audioBufferMilliseconds; }
                set { _audioBufferMilliseconds = value; }
            }
            public static string CursorName
            {
                get { return _cursorName; }
                set { _cursorName = value; }
            }

            // Images
            public static string IdlePose
            {
                get { return _idlePose; }
                set { _idlePose = value; }
            }
            public static string TalkPose
            {
                get { return _talkPose; }
                set { _talkPose = value; }
            }
            public static string Background
            {
                get { return _background; }
                set { _background = value; }
            }
            public static string Placeholder
            {
                get { return _placeholder; }
                set { _placeholder = value; }
            }

        }

        /*
        static class GlobalHotkeys 
        {
            private static char _toggleStatsHUD = ' ';
            private static char _toggleAlwaysOnTop = Convert.ToChar(Keys.T);
            private static char _decreaseInputSensetivity = ' ';
            private static char _increaseInputSensetivity = ' ';

            public static char ToggleAlwaysOnTop
            {
                get { return _toggleAlwaysOnTop; }
                set { _toggleAlwaysOnTop = value; }
            }
        }
        */
        public Form1()
        {
            InitializeComponent();
            InitialSettings();
            SetCursor();

            // Allows files to be dropped into the form
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);

            // Sets the form to listen for keyboard inputs
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);

            // This handles the microphone input levels
            Thread audioThread = new Thread(new ThreadStart(AudioInput));
            audioThread.IsBackground = true; //Will close thread when form is closed
            audioThread.Start();

        }

        public void InitialSettings()
        {
            Global.IdlePose = @"images\spriteClosed.png";
            Global.TalkPose = @"images\spriteOpen.png";
            Global.Placeholder = @"images\placeholder.jpg";

            Global.CursorName = "cirnoCursor.ani";


            setImage(Global.IdlePose);
        }

        public void SetCursor()
        {
            try
            {
                this.Cursor = AdvancedCursors.Create(Path.Combine(Application.StartupPath, Global.CursorName));
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
                this.Cursor = System.Windows.Forms.Cursor.Current;
            }
        }

        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
                Console.WriteLine(file);
        }

        void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //MessageBox.Show("KeyPressed");

            switch (e.KeyCode)
            {
                // Toggle always on top
                case Keys.T:
                    if (this.TopMost == true)
                        this.TopMost = false;
                    else
                        this.TopMost = true;
                    break;

                // Increase input sensitivity
                case Keys.X:
                    Global.InputSencetivity += 0.01f;
                    break;

                // Decrease input sencetivity
                case Keys.Z:
                    Global.InputSencetivity -= 0.01f;
                    break;

                // Toggle stats
                case Keys.P:
                    if (richTextBox1.Visible == true)
                        richTextBox1.Visible = false;
                    else
                        richTextBox1.Visible = true;
                    break;

                case Keys.Q:
                    if (this.TransparencyKey == Color.Empty)
                        this.TransparencyKey = Color.Green;
                    else
                        this.TransparencyKey = Color.Empty;
                    break;

                default:
                    break;
            }

            richTextBox1.Clear();
            richTextBox1.Text =
                "Input-level transition " + Math.Floor(Global.InputSencetivity * 100) + "%\n\n"
                + "Is top most: " + this.TopMost + "\n\n"
                + "Transparency Key: " + this.TransparencyKey + "\n\n";

        }

        private void AudioInput()
        {
            bool isRecording = false;


            while (true)
            {
                bool lastState = Global.IsTalking;

                var waveIn = new NAudio.Wave.WaveInEvent
                {
                    DeviceNumber = 0,
                    WaveFormat = new NAudio.Wave.WaveFormat(rate: 44100, bits: 16, channels: 1),
                    BufferMilliseconds = Global.AudioBufferMilliseconds
                };


                waveIn.DataAvailable += ShowPeakMono;
                if (!isRecording)
                {
                    waveIn.StartRecording();
                    isRecording = true;
                }

                if (Global.IsTalking != lastState)
                {
                    if (lastState == true)
                        setImage(Global.IdlePose);
                    else
                        setImage(Global.TalkPose);
                }
            }

        }

        public static void ShowPeakMono(object sender, NAudio.Wave.WaveInEventArgs args)
        {
            float maxValue = 32767;
            int peakValue = 0;
            int bytesPerSample = 2;
            for (int index = 0; index < args.BytesRecorded; index += bytesPerSample)
            {
                int value = BitConverter.ToInt16(args.Buffer, index);
                peakValue = Math.Max(peakValue, value);
            }



            if ((peakValue / maxValue) >= Global.InputSencetivity)
            {
                Global.IsTalking = true;
            }
            else
            {
                Global.IsTalking = false;
            }

        }

        public void setImage(string imagePath)
        {
            try
            {
                this.pictureBox1.Image = Image.FromFile(imagePath);
            }

            catch(Exception err)
            {
                //this.pictureBox1.Image = Properties.Resources.placeholder;
                setImage(Global.Placeholder);
                //MessageBox.Show(err.Message);
            }

        }

        private void pictureBox1_Click(object sender, EventArgs e) { }
    }
    public class AdvancedCursors
    {
        [DllImport("User32.dll")]
        private static extern IntPtr LoadCursorFromFile(String str);

        public static Cursor Create(String filename)
        {
            IntPtr hCursor = LoadCursorFromFile(filename);

            if (!IntPtr.Zero.Equals(hCursor))
            {
                return new Cursor(hCursor);
            }
            else
            {
                throw new ApplicationException("Could not create cursor from file" + filename);
            }
        }
    }

}
