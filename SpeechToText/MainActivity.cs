using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Speech;
using System.Collections.Generic;
using Lego.Ev3.Core;
using Lego.Ev3.Desktop;
using Android.Bluetooth;

namespace SpeechToText
{
    [Activity(Label = "LegoEV3Voice", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private bool isRecording;
        private readonly int VOICE = 10;
        private TextView textBox;
        private TextView textViewSpeed;
        private Button recButton;
        private Button upButton;
        private Button stopButton;
        private Button downButton;
        private Button connectButton;
        private EditText editTextBrickName;
        private SeekBar seekBarSpeed;
        Brick brick;
        int speed = 50;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

           


            // set the isRecording flag to false (not recording)
            isRecording = false;

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // get the resources from the layout
            recButton = FindViewById<Button>(Resource.Id.btnRecord);
            textBox = FindViewById<TextView>(Resource.Id.textYourText);
            textViewSpeed = FindViewById<TextView>(Resource.Id.textViewSpeed);
            upButton = FindViewById<Button>(Resource.Id.btnUP);
            stopButton = FindViewById<Button>(Resource.Id.btnStop);
            downButton = FindViewById<Button>(Resource.Id.btnDown);
            connectButton = FindViewById<Button>(Resource.Id.btnConnect);
            editTextBrickName = FindViewById<EditText>(Resource.Id.editText1);
            seekBarSpeed = FindViewById<SeekBar>(Resource.Id.seekBar1);

            seekBarSpeed.ProgressChanged += SeekBarSpeed_ProgressChanged;

            upButton.Click += UpButton_Click;
            downButton.Click += DownButton_Click;
            stopButton.Click += StopButton_Click;

            connectButton.Click += ConnectButton_Click1;


           


            // check to see if we can actually record - if we can, assign the event to the button
            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
            {
                // no microphone, no recording. Disable the button and output an alert
                var alert = new AlertDialog.Builder(recButton.Context);
                alert.SetTitle("You don't seem to have a microphone to record with");
                alert.SetPositiveButton("OK", (sender, e) =>
                {
                    textBox.Text = "No microphone present";
                    recButton.Enabled = false;
                    return;
                });

                alert.Show();
            }
            else
                recButton.Click += delegate
                {
                   
                    // change the text on the button
                    recButton.Text = "End Recording";
                    isRecording = !isRecording;
                    if (isRecording)
                    {
                        // create the intent and start the activity
                        var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

                        // put a message on the modal dialog
                        voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, Application.Context.GetString(Resource.String.messageSpeakNow));

                        // if there is more then 1.5s of silence, consider the speech over
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
                        voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);

                        // you can specify other languages recognised here, for example
                        // voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.German);
                        // if you wish it to recognise the default Locale language and German
                        // if you do use another locale, regional dialects may not be recognised very well

                        voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.English);
                        StartActivityForResult(voiceIntent, VOICE);
                    }
                };
        }

        private void SeekBarSpeed_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            speed = e.Progress;
            textViewSpeed.Text = string.Format("Speed: " + e.Progress);
        }

        private async void ConnectButton_Click1(object sender, System.EventArgs e)
        {
            brick = new Brick(new BluetoothCommunication(editTextBrickName.Text));
            await brick.ConnectAsync();
            await brick.DirectCommand.PlayToneAsync(10, 1000, 300);
        }


        private void StopButton_Click(object sender, System.EventArgs e)
        {
            brick.DirectCommand.StopMotorAsync(OutputPort.B | OutputPort.C, false);
        }

        private void UpButton_Click(object sender, System.EventArgs e)
        {
            brick.DirectCommand.StopMotorAsync(OutputPort.B | OutputPort.C, false);
            brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.B | OutputPort.C, speed);
        }

        private void DownButton_Click(object sender, System.EventArgs e)
        {
            brick.DirectCommand.StopMotorAsync(OutputPort.B | OutputPort.C, false);
            brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.B | OutputPort.C, -speed);
        }

        protected  override void OnActivityResult(int requestCode, Result resultVal, Intent data)
        {
            if (requestCode == VOICE)
            {
                if (resultVal == Result.Ok)
                {
                    var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                    if (matches.Count != 0)
                    {
                        string textInput = textBox.Text + matches[0];
                        
                        if (matches[0] == "go")
                        {
                             brick.DirectCommand.StopMotorAsync(OutputPort.B | OutputPort.C, false);
                             brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.B | OutputPort.C, speed);
                        }
                        if (matches[0] == "stop")
                        {
                             brick.DirectCommand.StopMotorAsync(OutputPort.B | OutputPort.C, false);
                        }
                        if (matches[0] == "go Left")
                        {
                             brick.DirectCommand.StopMotorAsync(OutputPort.B | OutputPort.C, false);
                             brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.C, speed);
                        }
                        if (matches[0] == "go right")
                        {
                             brick.DirectCommand.StopMotorAsync(OutputPort.B | OutputPort.C, false);
                             brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.B , speed);
                        }
                        if (matches[0] == "Go Back")
                        {
                             brick.DirectCommand.StopMotorAsync(OutputPort.B | OutputPort.C, false);
                             brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.B | OutputPort.C, -speed);
                        }
                        if (matches[0] == "Faster")
                        {
                            speed += 10;
                            brick.DirectCommand.StopMotorAsync(OutputPort.B | OutputPort.C, false);
                            brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.B | OutputPort.C, speed);
                        }
                        if (matches[0] == "slowly")
                        {
                            speed -= 10;
                            brick.DirectCommand.StopMotorAsync(OutputPort.B | OutputPort.C, false);
                            brick.DirectCommand.TurnMotorAtPowerAsync(OutputPort.B | OutputPort.C, -speed);
                        }

                        // limit the output to 500 characters
                        if (textInput.Length > 500)
                            textInput = textInput.Substring(0, 500);
                        textBox.Text = textInput;
                    }
                    else
                        textBox.Text = "No speech was recognised";
                    // change the text back on the button
                    recButton.Text = "Start Recording";
                }
            }

            base.OnActivityResult(requestCode, resultVal, data);
        }
    }
}