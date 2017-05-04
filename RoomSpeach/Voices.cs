using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using NAudio;
using NAudio.Wave;
using System.Threading;

namespace RoomSpeach
{
    static class Voice
    {
        static string key = "cf807d0e-9aae-445c-88df-570c995c3e91";
        static List<Task> phrases = new List<Task>();
        static Thread taskWatcher = null;

        public static void Init()
        {
            if(taskWatcher == null)
            {
                taskWatcher = new Thread(new ThreadStart(TaskThread));
                taskWatcher.Start();
            }
        }
        static void PlayMp3FromUrl(string url)
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

        static void PlayPhraseTask(object phrase)
        {
            PlayMp3FromStream((Stream)phrase);
        }

        public static void PlayPhrase(string phrase)
        {
            PlayMp3FromUrl("https://tts.voicetech.yandex.net/generate?text=" + phrase + "&format=mp3&lang=ru-RU&speaker=omazh&emotion=neutral&speed=1&key=" + key);
        }

        public static Task PlayPhraseAsync(string phrase)
        {
            Task t = new Task(PlayPhraseTask,GetPhraseStream(phrase));
            TaskFactory(t);
            return t;
        }

        static void TaskFactory(Task t)
        {
            Monitor.Enter(phrases);
            phrases.Add(t);
            Monitor.Exit(phrases);
        }

        static void TaskThread()
        {
            while(true)
            {
                Task now = null;
                Monitor.Enter(phrases);
                if(phrases.Count > 0)
                {
                    now = phrases[0];
                }
                Monitor.Exit(phrases);
                if (now != null)
                {
                    now.Start();
                    now.Wait();
                    Monitor.Enter(phrases);
                    phrases.RemoveAt(0);
                    Monitor.Exit(phrases);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        public static bool EmptyFactory()
        {
            bool result = false;
            Monitor.Enter(phrases);
            if (phrases.Count == 0)
                result = true;
            Monitor.Exit(phrases);
            return result;
        }
        
        static void PlayMp3FromStream(Stream stream)
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

        static Stream GetPhraseStream(string phrase)
        {
            return WebRequest.Create("https://tts.voicetech.yandex.net/generate?text=" + phrase + "&format=mp3&lang=ru-RU&speaker=jane&emotion=neutral&speed=0.8&key=" + key)
                    .GetResponse().GetResponseStream();
        }
    }
}
