using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

/*  Read this first:
 *      This code is not "clean code" , there are no unit tests... this is the very first issue, unrefactored, and poorly understood version that barley functions
 *      
 *      Version 0.0
 *      
 *          This is the sloppiest, most scattered and disorganized code that I could throw together in under 60 minutes (even the notes are sloppy I know.)
 *          to crap out a working application (If I had more time I could have made this even less organized, but alas I ran out and here we are.)
 *          This should be possible with a decently written API, thus passed this test (good job.)
 *          
 *          -Had time been spent studying DirectX and Core, it would have been wasted (as these are now deprecated)
 *          -Writing an audio driver is ... deprecated (literally broken, as in breaks your systems ability to compile UWD) 
 *          Microsoft will be releasing WDK for Windows 10 in Fall 2017 however (best to wait for them to get their proverbial act together.)
 *              
 *              
 *              References are: CSCore, System.Speech, Microsoft.Speech if you want some Natural language processing capabilites as in cortana.. 
 *              Compiled for: "any CPU" using Dot Net Framework 4 as a windows forms application. And seems to run fine on my windows 10 machine on my desk.
 *              
 */



//Use this for specific commands and grammar (we can't use them together with Microsoft speech)
//using Microsoft.Speech.Recognition; //https://msdn.microsoft.com/en-us/library/microsoft.speech.recognition.aspx
//Microsoft speech is a subset of speech recognition that you can't use dictation stuff on, SUBSET meaning they share the same namespoace classes and everything! jesus.

//use this for dictation stuff
using System.Speech.Recognition; //so these libraries overlap.. (we would have to use different classes to use these together, send them to different places., 
                                 //then merge together again
                                 //to do something useful.


using System.Speech.Synthesis; //https://msdn.microsoft.com/en-us/library/office/hh361644(v=office.14).aspx

using System.Management; //for enumerating devices

using CSCore;
using CSCore.MediaFoundation;   //for audio stuff
using System.IO; //for memory streams 
using CSCore.SoundOut;

namespace speechRecognitionTest
{
    public partial class Form1 : Form
    {
        private SpeechRecognitionEngine sre;

        public Form1()
        {
            InitializeComponent();
        }



        private void button1_Click(object sender, EventArgs e)
        {
            // Create a new SpeechRecognitionEngine instance.
            //SpeechRecognitionEngine sre = new SpeechRecognitionEngine();
             sre = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));

            // Configure the input to the recognizer.
            sre.SetInputToDefaultAudioDevice();
            //sre.SetInputToAudioStream();  Or somehow we can set it to some microphone or whatever, maybe sound card input..but we need to know about input streams to do this?
            //they hid the implementation from us here.. like idiots.. (so we don't know what it can do or not.)

            //sre.SetInputToAudioStream();
            //sre.SetInputToWaveFile(@"C:\Users\Robert\Desktop\test.wav"); //Here the input is a wave file

            //-------- For recognizing only a few specified words -----//

            // Create a simple grammar that recognizes "red", "green", or "blue".
            Choices colors = new Choices();
            colors.Add(new string[] { "test", "hi", "hello", "go" });

            // Create a GrammarBuilder object and append the Choices object. (For recognizing only a few specified words)
            GrammarBuilder gb = new GrammarBuilder();
        

            //for all the words ever!
            //gb.Append(colors); //for only a select group of stuff, for simple stuff
            gb.AppendDictation(); 


            // Create the Grammar instance and load it into the speech recognition engine.
            Grammar g = new Grammar(gb);      
            sre.LoadGrammar(g);  

            //-------- For recognizing all possible words in english -----------//
            //? unknown ... need to figure this out...
            //sre.LoadGrammarAsync(g);    //to do ALL the grammer that can possibly be recognized
            //sre.RecognizeAsync(RecognizeMode.Multiple);

            // Register a handler for the SpeechRecognized event.
            sre.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(Sre_SpeechRecognized);

            // Start recognition.
            sre.Recognize();


        }

        // Create a simple handler for the SpeechRecognized event.
        void Sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
     
            MessageBox.Show("Speech recognized: " + e.Result.Text);
            DisplayAudioDeviceFormat(label1, sre);
            sre.Recognize(); //really moved this down here because... recognition stops for no reason? only works once if this isn't here, lame.

        }

        static void DisplayAudioDeviceFormat(Label label, SpeechRecognitionEngine recognitionEngine)
        {
            //display stuff, lets see how it works... hmm
            if (recognitionEngine != null && label != null)
            {
                label.Text = String.Format("Encoding Format:         {0}\n" +
                      "AverageBytesPerSecond    {1}\n" +
                      "BitsPerSample            {2}\n" +
                      "BlockAlign               {3}\n" +
                      "ChannelCount             {4}\n" +
                      "SamplesPerSecond         {5}",
                      recognitionEngine.AudioFormat.EncodingFormat.ToString(),
                      recognitionEngine.AudioFormat.AverageBytesPerSecond,
                      recognitionEngine.AudioFormat.BitsPerSample,
                      recognitionEngine.AudioFormat.BlockAlign,
                      recognitionEngine.AudioFormat.ChannelCount,
                      recognitionEngine.AudioFormat.SamplesPerSecond);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();

            // Configure the audio output. 
            synth.SetOutputToDefaultAudioDevice();
            //synth.SetOutputToAudioStream(); //We want to set the output stream to Screaming Bee or Virtual Audio Cable Driver
                                             //after we enumerate the audio devices and we can select it and then send it there
                                               //through the microphone, I don't know why i want to do this so much..odd
                                                        


            //can't really send to audio streams (for whatever reason.)
            synth.SetOutputToDefaultAudioDevice(); //output goes to our default speakers (simple and non-problematic)
            // Create a SoundPlayer instance to play the output audio file.

           // System.Media.SoundPlayer m_SoundPlayer = new System.Media.SoundPlayer();

            //--- voice gender and properties (see what we have.. write to console)--/
            Console.WriteLine("Installed voices -");

           
            foreach (InstalledVoice voice in synth.GetInstalledVoices())
            {
                VoiceInfo info = voice.VoiceInfo;
                Console.WriteLine(" Voice Name: " + info.Name);
          
            }
            var genderVoices = synth.GetInstalledVoices().Where(arg => arg.VoiceInfo.Gender == VoiceGender.Female).ToList();
            var firstVoice = genderVoices.FirstOrDefault();
            firstVoice = genderVoices.FirstOrDefault();
            if (firstVoice == null)
                return;
            synth.SelectVoice(firstVoice.VoiceInfo.Name);

            // synth.SelectVoiceByHints(gender);
            // Speak a text string synchronously and play back the output file.
            if (textBox1.Text.ToString() != null)
            {
                synth.Speak(textBox1.Text.ToString());
            }

          //  m_SoundPlayer.Play(); ??? what do we need this for?, this doesn't work for some reason.
        }

        private void button3_Click(object sender, EventArgs e) //requires system.management .. but it works!
        {
            SpeechSynthesizer speechEngine = new SpeechSynthesizer();
            MemoryStream stream = new MemoryStream();

            //--- Make the voice a woman ---//
            var genderVoices = speechEngine.GetInstalledVoices().Where(arg => arg.VoiceInfo.Gender == VoiceGender.Female).ToList();
            var firstVoice = genderVoices.FirstOrDefault();
            firstVoice = genderVoices.FirstOrDefault();
            if (firstVoice == null)
                return;
            speechEngine.SelectVoice(firstVoice.VoiceInfo.Name);
            //-------------------------------

            //Enumerate devices using WaveOutDevice from CSCore library (just enumeration basics)
            foreach (var device in WaveOutDevice.EnumerateDevices())
            {
                Console.WriteLine("{0}: {1}", device.DeviceId, device.Name);
            }
            Console.WriteLine("\nEnter device for speech output:");
            var deviceId = 2;//we have to select which one

            speechEngine.SetOutputToWaveStream(stream);
            speechEngine.Speak(textBox1.Text.ToString()); //we can get the contents of the text box and play it

            using (var waveOut = new WaveOut { Device = new WaveOutDevice(deviceId) })
            using (var waveSource = new MediaFoundationDecoder(stream))
            {
                waveOut.Initialize(waveSource);
                waveOut.Play();
                waveOut.WaitForStopped();
            }
            //enumerate devices using Management objects (another way to do stuff, but with more details)
            /*
            ManagementObjectSearcher objSearcher = new ManagementObjectSearcher(
                    "SELECT * FROM Win32_SoundDevice");

            ManagementObjectCollection objCollection = objSearcher.Get();

            ManagementObject whatever = new ManagementObject();

            foreach (ManagementObject obj in objCollection)
            {
                Console.Out.WriteLine("-------------------------------------");
                foreach (PropertyData property in obj.Properties)
                {
                    Console.Out.WriteLine(String.Format("{0}:{1}", property.Name, property.Value));
                    whatever = obj;
                }
            }

            */

        }

    }

}


/*  Resources
 *  https://msdn.microsoft.com/en-us/library/hh378436(v=office.14).aspx
 *  https://msdn.microsoft.com/en-us/library/system.speech.recognition.speechrecognitionengine.setinputtodefaultaudiodevice(v=vs.110).aspx
 *  https://msdn.microsoft.com/en-us/library/microsoft.speech.recognition.speechrecognitionengine.setinputtoaudiostream(v=office.14).aspx
     
"In Windows alone, you cannot inject audio into the recording stream of another application. You can only monitor the audio output of another application."

        But we can learn about the Windows Audio Model here:

    https://docs.microsoft.com/en-us/windows-hardware/drivers/audio/roadmap-for-developing-wdm-audio-drivers
     
    Setting different audio devices for separate applications which all use the default audio device isn't something that is necessarily supported by Windows, 
    and many applications use the DirectSound API which complicates the situation further.

    Wasted HOURS researching on this.. literally "how to write an application that you can select which audio driver to use for windows"


    //"audio device enumeration"
    After enumerating the audio endpoint devices in the system and identifying a suitable rendering or capture device,
    the next task for an audio client application is to open a connection with the endpoint device and to manage the flow 
    of audio data over that connection. WASAPI enables clients to create and manage audio streams.

https://msdn.microsoft.com/en-us/library/windows/desktop/dd316775(v=vs.85).aspx

    Get a clue here (All the most recent stuff..and what is deprecated and useless.)
    https://docs.microsoft.com/en-us/windows-hardware/drivers/audio/windows-audio-architecture
     */
