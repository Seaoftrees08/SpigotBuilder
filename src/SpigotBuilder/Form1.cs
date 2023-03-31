using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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
                dc.Headers.Add("method", "GET");
                dc.Headers.Add("authority", "hub.spigotmc.org");
                dc.Headers.Add("scheme", "https");
                dc.Headers.Add("pragma", "no-cache");
                dc.Headers.Add("cache-control", "no-cache");
                dc.Headers.Add("upgrade-insecure-requests", "1");
                dc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36");
                dc.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                dc.Headers.Add("sec-fetch-site", "none");
                dc.Headers.Add("sec-fetch-mode", "navigate");
                dc.Headers.Add("sec-fetch-user", "?1");
                dc.Headers.Add("sec-fetch-dest", "document");
                dc.Headers.Add("accept-language", "en,ja;q=0.9,ko;q=0.8,zh-TW;q=0.7,zh;q=0.6");
                Uri uri = new Uri("https://hub.spigotmc.org/jenkins/job/BuildTools/lastSuccessfulBuild/artifact/target/BuildTools.jar");
                dc.DownloadFileAsync(uri, directoryName + "\\BuildTools.jar");
            }
            catch(Exception ex)
            {
                textBox1.AppendText("ERROR! Download Failded.");
                textBox1.AppendText(ex.Message);
                button2.Enabled = true;
                button1.Enabled = true;
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
        void Cout(object sender, System.Diagnostics.DataReceivedEventArgs e)
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
            var lst = new List<McVersion>();

            try
            {
                //get spigot json list
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                WebClient wc = new WebClient();
                wc.Headers.Add("method", "GET");
                wc.Headers.Add("authority", "hub.spigotmc.org");
                wc.Headers.Add("scheme", "https");
                wc.Headers.Add("path", "/versions/");
                wc.Headers.Add("pragma", "no-cache");
                wc.Headers.Add("cache-control", "no-cache");
                wc.Headers.Add("upgrade-insecure-requests", "1");
                wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36");
                wc.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                wc.Headers.Add("sec-fetch-site", "none");
                wc.Headers.Add("sec-fetch-mode", "navigate");
                wc.Headers.Add("sec-fetch-user", "?1");
                wc.Headers.Add("sec-fetch-dest", "document");
                wc.Headers.Add("accept-language", "en,ja;q=0.9,ko;q=0.8,zh-TW;q=0.7,zh;q=0.6");

                vl = await wc.DownloadStringTaskAsync("https://hub.spigotmc.org/versions/");

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

                    var mv = new McVersion(v);
                    if(!lst.Contains(mv))
                        lst.Add(mv);

                    m = m.NextMatch();
                }

                //sort
                var orderd = lst.OrderBy(a => a.EvalValue()).Reverse();


                foreach (McVersion v in orderd)
                {
                    if(!comboBox1.Items.Contains(v.ToVerison()))
                        comboBox1.Items.Add(v.ToVerison());
                }

                textBox1.AppendText("Update Success!\r\n");

            }
            catch(Exception e)
            {
                textBox1.AppendText("Update ERROR!");
                textBox1.AppendText(e.ToString());
            }

            button2.Enabled = true;
        }
    }

    class McVersion
    {
        private int First { get; set; }
        private int Middle { get; set; }
        private int Last { get; set; }

        public McVersion(string version)
        {
            string[] lst = version.Split('.');
            First = int.Parse(lst[0]);
            Middle = int.Parse(lst[1]);
            if (lst.Length > 2)
            {
                Last = int.Parse(lst[2]);
            }
            else
            {
                Last = 0;
            }
                
        }

        public string ToVerison()
        {
            return First + "." + Middle + "." + Last;
        }

        public int EvalValue()
        {
            return First * 10000 + Middle * 100 + Last;
        }
    }

}
