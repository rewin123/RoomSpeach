using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Speech.Recognition.SrgsGrammar;
using System.IO;
using System.Net;
using NAudio;
using NAudio.Wave;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Threading;


namespace RoomSpeach
{
    public partial class Form1 : Form
    {
       
        string key = "cf807d0e-9aae-445c-88df-570c995c3e91";


        WaveIn recognizeSample;
        bool voice = false;
        bool recoding = false;
        BufferedWaveProvider voiceStore;
        DateTime lastVoiceCaptured = new DateTime(0);
        Grammar grammar;
        SpeechRecognitionEngine sre;
        WaveFileWriter writer;
        HttpWebRequest postRequest;
        MemoryStream memoryBuffer;

        List<Skill> skills;

        string alex_hear_loc = "alex_hear.mp3";
        Stream alex_hear_stream;

        Thread recognizeThread;

        string name = "pen";
        public Form1()
        {
            InitializeComponent();


            Voice.Init();

            skills = new List<Skill>();
            skills.Add(new WeatherSkill());
            skills.Add(new AnimeSkill());
            skills.Add(new DateSkill());

            skills[0].MakeCommand("погода на сегодня");
            //WeatherNow();
            //Weather();

            StreamWriter wr = new StreamWriter(alex_hear_loc);
            GetPhraseStream(name + " слушает").CopyTo(wr.BaseStream);
            wr.Close();
            alex_hear_stream = (new StreamReader(alex_hear_loc)).BaseStream;
            
            recognizeSample = new WaveIn();
            recognizeSample.DataAvailable += RecognizeSample_DataAvailable;

            voiceStore = new BufferedWaveProvider(recognizeSample.WaveFormat);
            voiceStore.BufferDuration = TimeSpan.FromSeconds(60);

            sre = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-us"));
            sre.SetInputToDefaultAudioDevice();
            sre.SpeechRecognized += Sre_SpeechRecognized1;

            Choices chs = new Choices();
            chs.Add(name);
            GrammarBuilder grbuilder = new GrammarBuilder();
            grbuilder.Culture = new System.Globalization.CultureInfo("en-us");
            grbuilder.Append(chs);

            grammar = new Grammar(grbuilder);
            

            sre.LoadGrammar(grammar);

            sre.RecognizeAsync(RecognizeMode.Multiple);

        }

        private void Sre_SpeechRecognized1(object sender, SpeechRecognizedEventArgs e)
        {
            button1.BackColor = Color.Green;
            micro = new WaveIn();
            micro.WaveFormat = new WaveFormat(44100, 1);

            postRequest = (HttpWebRequest)HttpWebRequest.Create("http://asr.yandex.net/asr_xml?key=" + key + "&uuid=" + RandUID() + "&topic=queries&lang=ru-RU");
            postRequest.Method = "POST";
            postRequest.ContentType = "audio/x-wav";
            writer = new WaveFileWriter("Test001.wav", micro.WaveFormat);


            microBuffer = new BufferedWaveProvider(micro.WaveFormat);
            micro.DataAvailable += Micro_DataAvailable;
            micro.RecordingStopped += Micro_RecordingStopped;

            PlayMp3FromStream(alex_hear_stream);
            alex_hear_stream.Position = 0;

            startRecord = DateTime.Now;
            micro.StartRecording();
        }
        

        private void button1_Click(object sender, EventArgs e)
        {

            PlayMp3FromUrl("https://tts.voicetech.yandex.net/generate?text=" + textBox1.Text + "&format=mp3&lang=ru-RU&speaker=zahar&emotion=good&key=" + key);

            //HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://tts.voicetech.yandex.net/generate?text=Наш%20текст%20гот+ов&format=mp3&lang=ru-RU&speaker=zahar&emotion=good&key=" + key);

            //HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            
            ;

            

        }

        public static void PlayMp3FromUrl(string url)
        {
            
            using (Stream ms = new MemoryStream())
            {
                using (Stream stream = WebRequest.Create(url)
                    .GetResponse().GetResponseStream())
                {
                    byte[] buffer = new byte[32768];
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                }

                ms.Position = 0;
                using (WaveStream blockAlignedStream =
                    new BlockAlignReductionStream(
                        WaveFormatConversionStream.CreatePcmStream(
                            new Mp3FileReader(ms))))
                {
                    using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                    {
                        waveOut.Init(blockAlignedStream);
                        waveOut.Play();
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
            }
        }

        public static void PlayMp3FromStreamAsync(object stream)
        {
            PlayMp3FromStream((Stream)stream);
        }
        public static void PlayMp3FromStream(Stream stream)
        {
            using (Stream ms = new MemoryStream())
            {

                byte[] buffer = new byte[32768];
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }


                ms.Position = 0;
                using (WaveStream blockAlignedStream =
                    new BlockAlignReductionStream(
                        WaveFormatConversionStream.CreatePcmStream(
                            new Mp3FileReader(ms))))
                {
                    using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                    {
                        waveOut.Init(blockAlignedStream);
                        waveOut.Play();
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
            }
        }

        public static void PlayMp3FromFile(string url)
        {

            using (Stream ms = new MemoryStream())
            {
                StreamReader readStr = new StreamReader(url);
                using (Stream stream = readStr.BaseStream)
                {
                    byte[] buffer = new byte[32768];
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                }

                ms.Position = 0;
                using (WaveStream blockAlignedStream =
                    new BlockAlignReductionStream(
                        WaveFormatConversionStream.CreatePcmStream(
                            new Mp3FileReader(ms))))
                {
                    using (WaveOut waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback()))
                    {
                        waveOut.Init(blockAlignedStream);
                        waveOut.Play();
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
                readStr.Close();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Stream media = WebRequest.Create("https://tts.voicetech.yandex.net/generate?text=" + textBox1.Text + "&format=wav&lang=ru-RU&speaker=zahar&emotion=good&key=" + key).GetResponse().GetResponseStream();

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://asr.yandex.net/asr_xml?key=" + key + "&uuid=" + RandUID() + "&topic=queries&lang=ru-RU");
            request.Method = "POST";
            request.ContentType = "audio/x-wav";
            Stream str = request.GetRequestStream();
            media.CopyTo(str);
            HttpWebResponse resp = (HttpWebResponse)request.GetResponse();


            string result = new StreamReader(resp.GetResponseStream(),Encoding.UTF8).ReadToEnd();
            label1.Text = result;
            
        }

        string GetBestRecognize(string input)
        {
            return "";
        }

        int SkipIt(string data, int pos, string find)
        {
            return 0;
        }

        string RandUID()
        {
            Random r = new Random();
            string result = "";
            for(int i = 0;i < 32;i++)
            {
                result += r.Next(9).ToString();
            }
            return result;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        BufferedWaveProvider microBuffer;
        DateTime startRecord = DateTime.Now;
        WaveIn micro;
        private void button3_Click(object sender, EventArgs e)
        {
            micro = new WaveIn();
           
            microBuffer = new BufferedWaveProvider(micro.WaveFormat);
            micro.DataAvailable += Micro_DataAvailable;
            micro.RecordingStopped += Micro_RecordingStopped;
            startRecord = DateTime.Now;
            micro.StartRecording();
        }

        private void Micro_RecordingStopped(object sender, StoppedEventArgs e)
        {
            PlayMicroBuffer();
        }

        private void Micro_DataAvailable(object sender, WaveInEventArgs e)
        {
            try
            {
                microBuffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
                writer.Write(e.Buffer, 0, e.BytesRecorded);
                writer.Flush();

                if ((DateTime.Now - startRecord).TotalSeconds > 10)
                {
                    micro.StopRecording();
                }
                byte[] buffer = e.Buffer;

                float avr = 0;
                for (int index = 0; index < e.BytesRecorded; index += 2)
                {
                    short sample = (short)((buffer[index + 1] << 8) |
                                            buffer[index]);
                    avr += Math.Abs(sample / 32768f);
                }
                avr /= e.BytesRecorded / 2;
                if (avr < 0.05f)
                {
                    if((DateTime.Now - startRecord).TotalSeconds > 1.5)
                        micro.StopRecording();
                }
            }
            catch
            {

            }
        }

        void PlayMicroBuffer()
        {
            //WaveOut dinamic = new WaveOut();
            //dinamic.Init(microBuffer);
            //dinamic.Play();

            button1.BackColor = Color.Wheat;

            writer.Close();
            StreamReader str = new StreamReader("Test001.wav");
            str.BaseStream.CopyTo(postRequest.GetRequestStream());
            str.Close();
            
            
            HttpWebResponse resp = (HttpWebResponse)postRequest.GetResponse();


            string result = new StreamReader(resp.GetResponseStream(), Encoding.UTF8).ReadToEnd();

            File.WriteAllText("Test.txt", result);

            Regex regex = new Regex(">([йцукенгшщзхъфывапролджэячсмитьбю ]+)<\\/variant>");
            
            MatchCollection matchs = regex.Matches(result);
            try
            {
                label1.Text = matchs[0].Groups[1].Value;
                bool ok = false;

                for (int i = 0; i < matchs.Count; i++)
                {
                    if (CommandRecognize(matchs[i].Groups[1].Value))
                    {
                        label1.Text = matchs[i].Groups[1].Value;
                        ok = true;
                        break;
                    }
                }
                

                if(ok == false)
                {
                    PlayPhrase("Команда не распознана");
                }
            }
            catch
            {
                label1.Text = "No text";
            }
            

        }
        
        private void button4_Click(object sender, EventArgs e)
        {
            if(recoding)
            {
                recognizeSample.StopRecording();
            }
            else
            {
                recognizeSample.StartRecording();
            }
            recoding = !recoding;
        }

        private void RecognizeSample_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;

            float sample32 = 0;
            float maxVal = 0;

            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = (short)((buffer[index + 1] << 8) |
                                        buffer[index]);
                
                sample32 = sample / 32768f;
                maxVal = Math.Max(Math.Abs(sample32), maxVal);
            }
            
            if(maxVal > 0.02f)
            {
                voice = true;
                if((DateTime.Now - lastVoiceCaptured).TotalSeconds > 3)
                {
                    voiceStore.ClearBuffer();
                }
                lastVoiceCaptured = DateTime.Now;
            }
            else
            {
                if ((DateTime.Now - lastVoiceCaptured).TotalSeconds > 3)
                {
                    voice = false;
                }
            }

            if(voice)
            {
                voiceStore.AddSamples(e.Buffer, 0, e.BytesRecorded);
                label2.Text = "Recording";
            }
            else
            {
                label2.Text = "Waiting";
                
            }
        }

       bool CommandRecognize(string text)
        {
            bool commandRecognized = false;
            for(int i = 0;i < skills.Count;i++)
            {
                if(skills[i].MakeCommand(text))
                {
                    commandRecognized = true;
                    break;
                }
            }

            while (Voice.EmptyFactory() == false)
                Thread.Sleep(100);


            return commandRecognized;
        }

        bool Include(string data,string val)
        {
            Regex r = new Regex(val);
            if (r.Match(data).Value != "")
                return true;
            else return false;
        }
        
        

        string GetPage(string uri)
        {
            return (new StreamReader(WebRequest.Create(uri).GetResponse().GetResponseStream())).ReadToEnd();
        }

        

        
        
       
        

        void PlayPhraseAsync(object phrase)
        {
            PlayPhrase((string)phrase);
        }

        void PlayPhrase(string phrase)
        {
            PlayMp3FromUrl("https://tts.voicetech.yandex.net/generate?text=" + phrase + "&format=mp3&lang=ru-RU&speaker=jane&emotion=neutral&speed=0.8&key=" + key);
        }

        Stream GetPhraseStream(string phrase)
        {
            return WebRequest.Create("https://tts.voicetech.yandex.net/generate?text=" + phrase + "&format=mp3&lang=ru-RU&speaker=jane&emotion=neutral&speed=0.8&key=" + key)
                    .GetResponse().GetResponseStream();
        }

        ~Form1()
        {
            alex_hear_stream.Close();
        }
    }
}
