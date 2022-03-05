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
            private static double _inputSencetivity = 0.10;

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
            public static double InputSencetivity
            {
                get { return _inputSencetivity; }
                set { _inputSencetivity = value; }
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
        public Form1()
        {
            InitializeComponent();
            InitialSettings();
            SetCursor();

            Thread audioThread = new Thread(new ThreadStart(AudioInput));
            audioThread.IsBackground = true; //Will close thread when form is closed
            audioThread.Start();

            //Thread keyInputThread 

        }

        public void InitialSettings()
        {
            Global.IdlePose = @"images\spriteClosedd.png";
            Global.TalkPose = @"images\spriteOpen.png";
            Global.Placeholder = @"images\placeholder.jpg";

            Global.CursorName = "test.ani";


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
                    BufferMilliseconds = 100
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
                this.pictureBox1.Image = Image.FromFile(Global.Placeholder);
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
