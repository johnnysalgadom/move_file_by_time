using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Timers;


namespace MoveFileByTime
{
    public partial class MoveFileByTimeService : ServiceBase
    {
        Timer timer = new Timer();
        string folderA = "";
        string folderB = "";
        string folderC = "";
        int controlTime1 = 0;
        int controlTime2 = 0;
        int controlTime3 = 0;
        bool configurationIsOk = true;

        public MoveFileByTimeService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteToLog("Service is started at " + DateTime.Now);
            int timerInterval = 10; //minutes

            if (ConfigurationManager.AppSettings["TimerInterval"] != null) {
                timerInterval = Convert.ToInt32(ConfigurationManager.AppSettings["TimerInterval"]);
            }

            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = timerInterval * 60 * 1000; //number in milisecinds - timerInterval is expressed in minutes
            timer.Enabled = true;

            //Get the folders and the control times
            if (ConfigurationManager.AppSettings["FolderA"] != null)
            {
                folderA = ConfigurationManager.AppSettings["FolderA"];
            }
            else
            {
                WriteToLog("No tiene configurado la ruta para el folder A");
                configurationIsOk = false;
            }

            if (ConfigurationManager.AppSettings["FolderB"] != null)
            {
                folderB = ConfigurationManager.AppSettings["FolderB"];
            }
            else
            {
                WriteToLog("No tiene configurado la ruta para el folder B");
                configurationIsOk = false;
            }

            if (ConfigurationManager.AppSettings["FolderC"] != null)
            {
                folderC = ConfigurationManager.AppSettings["FolderC"];
            }
            else
            {
                WriteToLog("No tiene configurado la ruta para el folder C");
                configurationIsOk = false;
            }

            if (ConfigurationManager.AppSettings["ControlTime1"] != null)
            {
                controlTime1 = Convert.ToInt32(ConfigurationManager.AppSettings["ControlTime1"]);
            }
            else
            {
                WriteToLog("No tiene configurado el timepo de control 1");
                configurationIsOk = false;
            }

            if (ConfigurationManager.AppSettings["ControlTime2"] != null)
            {
                controlTime2 = Convert.ToInt32(ConfigurationManager.AppSettings["ControlTime2"]);
            }
            else
            {
                WriteToLog("No tiene configurado el timepo de control 2");
                configurationIsOk = false;
            }

            if (ConfigurationManager.AppSettings["ControlTime3"] != null)
            {
                controlTime3 = Convert.ToInt32(ConfigurationManager.AppSettings["ControlTime3"]);
            }
            else
            {
                WriteToLog("No tiene configurado el timepo de control 3");
                configurationIsOk = false;
            }

            WriteToLog($"folder A : {folderA}");
            WriteToLog($"folder B : {folderB}");
            WriteToLog($"folder C : {folderC}");
            WriteToLog($"control time 1: {controlTime1} minutos");
            WriteToLog($"control time 2: {controlTime2} minutos");
            WriteToLog($"control time 3: {controlTime3} minutos");

        }

        protected override void OnStop()
        {
            WriteToLog("Service is stopped at " + DateTime.Now);
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            WriteToLog("Service is recall at " + DateTime.Now);
            if (configurationIsOk)
            {
                CheckAndMoveFile(folderA, folderB, folderC, controlTime1, controlTime2, controlTime3);
            }
        }

        private void CheckAndMoveFile(string folderA, string folderB, string folderC, int controlTime1, int controlTime2, int controlTime3)
        {

            //Create folder B 
            if (!Directory.Exists(folderB))
            {
                Directory.CreateDirectory(folderB);
                WriteToLog($"{folderB} creado exitosamente.");
            }

            //Create folder C
            if (!Directory.Exists(folderC))
            {
                Directory.CreateDirectory(folderC);
                WriteToLog($"{folderC} creado exitosamente.");
            }

            string[] files;

            //Check folder A > control time 1 => move to folder B
            //if is > control time 3 => move to folder C
            if (Directory.Exists(folderA))
            {
                files = Directory.GetFiles(folderA);

                foreach (string file in files)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.Exists)
                    {
                        DateTime currentTime = DateTime.Now;
                        TimeSpan timeInFolder = currentTime - fileInfo.LastWriteTime;
                        if (fileInfo.LastWriteTime.AddMinutes(1) == fileInfo.CreationTime) //It is a marked when the file return to folderA
                        {
                            if (timeInFolder.TotalMinutes > controlTime3)
                            {
                                MoveFile(file, folderC);
                            }
                        } else { 
                            if (timeInFolder.TotalMinutes > controlTime1)
                            {
                                MoveFile(file, folderB);
                            }
                        }
                    }
                    fileInfo = null;
                }
            }
            else
            {
                WriteToLog($"El folder {folderA} no existe.");
            }

            //Check folder B > control time 2 => move to folder A
            files = Directory.GetFiles(folderB);
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Exists)
                {
                    DateTime currentTime = DateTime.Now;
                    TimeSpan timeInFolder = currentTime - fileInfo.LastWriteTime;
                    if (timeInFolder.TotalMinutes > controlTime2)
                    {
                        MoveFile(file, folderA, true);
                    }
                }
                fileInfo = null;
            }

        }

        private void MoveFile (string sourceFilePath, string targetFolderPath, bool returnToFolder = false)
        {
            string fileName = Path.GetFileName(sourceFilePath);
            string targetFilePath = Path.Combine(targetFolderPath, fileName);
            File.Move(sourceFilePath, targetFilePath);
            DateTime currentTime = DateTime.Now;
            File.SetLastWriteTime(targetFilePath, currentTime);
            if (returnToFolder)
            {
                File.SetCreationTime(targetFilePath, currentTime.AddMinutes(1));
            }
            WriteToLog($"El archivo {sourceFilePath} se mueve al {targetFolderPath}");
        }


        private void WriteToLog(string message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            message = DateTime.Now + " - " + message;
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(message);
                }
            }
        }

    }
}
