using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpigotBuilder
{
    //
    //    This software is released under MIT License.
    //    https://opensource.org/licenses/MIT
    //

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_ShownAsync(object sender, EventArgs e)
        {
            //run update
            VersionUpdate();
            button2.Enabled = true;
        }

        string file;

        //build
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            textBox1.AppendText("Starting Spigotbuild...\r\n");

            string path = Application.ExecutablePath;
            path = path.Remove((path.LastIndexOf("\\") + 1)) + "Spigot";

            //folder check
            textBox1.AppendText("checking directory...\r\n");

            string directoryName = path;
            file = "Spigot";

            if (Directory.Exists(directoryName))
            {
                int i = 1;
                directoryName = path + i;
                file = "Spigot" + i;

                while (Directory.Exists(directoryName))
                {
                    i++;
                    directoryName = path + i;
                    file = "Spigot" + i;
                }
            }

            Directory.CreateDirectory(directoryName);

            textBox1.AppendText("directory decision!\r\nDownloading BuildTools.jar...\r\n"
                + "Location: " + directoryName + "\r\n");

            try
            {
                WebClient dc = new WebClient();
                dc.DownloadFileCompleted += DownloadCompleted;
                Uri uri = new Uri("https://hub.spigotmc.org/jenkins/job/BuildTools/lastSuccessfulBuild/artifact/target/BuildTools.jar");
                dc.DownloadFileAsync(uri, directoryName + "\\BuildTools.jar");
            }
            catch
            {
                textBox1.AppendText("ERROR! spigotmc.org is invalid.");
                textBox1.AppendText("URL: https://hub.spigotmc.org/jenkins/job/BuildTools/lastSuccessfulBuild/artifact/target/BuildTools.jar");
            }

        }

        //Update
        private void button2_Click(object sender, EventArgs e)
        {
            VersionUpdate();
        }

        //Clear Log
        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = null;
        }

        //download completed
        private async void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if(e.Error == null)
            {
                textBox1.AppendText("download completed!\r\n");

                //make process
                System.Diagnostics.Process p = new System.Diagnostics.Process();

                //enable cin
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;

                //enable cout
                p.StartInfo.RedirectStandardOutput = true;
                p.OutputDataReceived += Cout;

                //set cmd.exe path
                p.StartInfo.FileName = Environment.GetEnvironmentVariable("ComSpec");

                //dont create window
                p.StartInfo.CreateNoWindow = true;

                textBox1.AppendText("Spigot Build Start!\r\n \r\n");

                //make command
                string cd_command = @"cd " + file;
                string build_command = @"java -jar BuildTools.jar"
                    + " --rev "
                    + comboBox1.Text;

                //run with another thread
                Task task = Task.Run(new Action(() =>
                {
                    //run
                    p.Start();

                    //enable cout
                    p.BeginOutputReadLine();

                    //Command stream
                    StreamWriter sw = p.StandardInput;
                    if (sw.BaseStream.CanWrite)
                    {
                        //command
                        sw.WriteLine(cd_command);
                        sw.WriteLine(build_command);
                        sw.WriteLine(@"exit");

                        //finish
                        sw.Close();
                        p.WaitForExit();
                        p.Close();
                    }
                }));

                await task;

                textBox1.AppendText("Spigot Build Finish.\r\n");
                button1.Enabled = true;
                button2.Enabled = true;
            }
            else
            {
                textBox1.AppendText("download Error!\r\n" + e.Error.Message
                    + "\r\nBuild Canceled.\r\n");

            }
        }

        //OutputDataReceivedEventHandler
        void Cout(object sender
            , System.Diagnostics.DataReceivedEventArgs e)
        {

            Task.Run(() =>
            {
                Invoke((MethodInvoker)(() =>
                {
                    textBox1.AppendText(e.Data + "\r\n");
                }));
            });
        }

        //Update method
        private async void VersionUpdate()
        {
            //get version list
            button2.Enabled = false;

            textBox1.AppendText("getting new version spigot list...\r\n");
            string vl;

            try
            {
                //get spigot json list
                HttpClient hc = new HttpClient();
                Task<string> task = hc.GetStringAsync("https://hub.spigotmc.org/versions/");

                vl = await task;

                //extraction version list
                System.Text.RegularExpressions.Regex r =
                    new System.Text.RegularExpressions.Regex(
                        @"\d{1,2}[.]\d{1,2}[.]\d{0,1}",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                System.Text.RegularExpressions.Match m = r.Match(vl);

                while (m.Success)
                {
                    string v = m.Value;
                    int l = v.Length - 1;

                    if (v.Substring(l)==".")
                    {
                        v = v.Remove(l);
                    }

                    if(!comboBox1.Items.Contains(v))
                        comboBox1.Items.Add(v);

                    m = m.NextMatch();
                }

                textBox1.AppendText("Update Success!\r\n");

            }
            catch
            {
                textBox1.AppendText("ERROR! spigotmc.org is invalid.");
                textBox1.AppendText("URL: https://hub.spigotmc.org/versions/");
            }

            button2.Enabled = true;
        }
    }
}
