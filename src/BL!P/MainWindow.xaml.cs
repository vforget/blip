using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;

using MBF;
using MBF.IO.Fasta;
using MBF.Web;
using MBF.Web.Blast;
using MBF.IO.GenBank;
using System.Net;
using Microsoft.DeepZoomTools;
using System.Runtime.InteropServices;

using HttpServer;
using HttpServer.Modules;
using HttpServer.Resources;
using HttpServer.Headers;
using HttpListener = HttpServer.HttpListener;
namespace Blip
{
    /// <summary>
    /// Stored parameters that are required across multiple UserControls.
    /// </summary>
    public class UIParameters
    {
        public string ProjectDir { get; set; } // Path to project folder
        public string FastaFile { get; set; } // Path to query fasta file
        public string CxmlFile { get; set; }
        public string CxmlDir { get; set; }
        public int WebServerPort { get; set; }
        public string Log { get; set; } // Program log, used for debugging
        public IList<ISequence> QuerySequences { get; set; }
        // BLAST related information
        public bool BlastUseBrowserProxy { get; set; }
        public double BlastMaxEvalue { get; set; }
        public double BlastMinPercentIdentity { get; set; }
        public double BlastMinPercentQueryCoverage { get; set; }
        public string BlastProgram { get; set; }
        public string BlastDatabase { get; set; }
        public string BlastGeneticCode { get; set; }
        public string BlastAlgorithm { get; set; }
        public int BlastMaxNumHits { get; set; }
        // Pivot related information
        public Collection Collection { get; set; }
        public string CollectionName { get; set; }
        public string CollectionPath { get; set; }
        public string CollectionTitle { get; set; }
        public string CollectionUrl { get; set; }
        public string CollectionImagePath { get; set; }
        public string CollectionDeepzoomPath { get; set; }
        public string CollectionUid { get; set; }
        public FacetCategory BackgroundFacetCategory { get; set; }
        public IList<System.Windows.Media.Color> BackgroundColorScheme { get; set; }
        public IList<FacetCategory> FacetCategories = new List<FacetCategory>();
        public string PivotEXE = @"C:\Program Files (x86)\Microsoft Live Labs Pivot\Pivot.exe";

    }

    
    /// <summary>
    /// I should refactor this to seperate classes.
    /// </summary>
    public partial class MainWindow : Window
    {
        private UserControl CurrentControl; // The current user control loaded in the UI.
        public int CurrentStep { get; set; } // The current step number.
        private UIParameters Up = new UIParameters(); // Stores the parameters collected across the UserControl steps.
        private IList<UserControl> UserControls = new List<UserControl>(); // The Usercontrols at each step.

        /// <summary>
        /// Load the UserControls
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            // Create Facet Categories
            Pivot.CreateFacetCategories(Up);
            // Set the default BLAST parameters.
            Up.WebServerPort = 8085;
            Up.BlastUseBrowserProxy = true;
            Up.BlastMaxNumHits = 5;
            Up.BlastMaxEvalue = 1.0e-20;
            Up.BlastMinPercentIdentity = 80.0;
            Up.BlastMinPercentQueryCoverage = 40.0;
            Up.QuerySequences = new List<ISequence>();
            Up.Log = "";
            CurrentStep = 0; // Start at Step 0.
            // Load the UserControls for each step.
            UserControl0 uc0 = new UserControl0(Up);
            UserControls.Add(uc0);
            UserControl9 uc9 = new UserControl9(Up);
            UserControls.Add(uc9);

            strVersion.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        /// <summary>
        /// Log a message to the text UI text log.
        /// </summary>
        /// <param name="message">A message to log.</param>
        private void LogMessage(string message)
        {
            Up.Log += message;
            LogText.Text = Up.Log;
        }

        /// <summary>
        /// Controls behaviour when user clicks the Previous button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Previous_Click(object sender, RoutedEventArgs e)
        {

            MainGrid.Children.Remove(CurrentControl); // Remove the current UserControl from the UI.
            // Update the current step and bound checking to decide which buttons to enable/disable.
            if (CurrentStep > 0)
            {
                CurrentStep -= 1;
            }
            if (CurrentStep == 0)
            {
                Previous_Button.IsEnabled = false;
            }
            else 
            {
                Previous_Button.IsEnabled = true;
            }
            if (CurrentStep == (UserControls.Count() - 1))
            {
                Next_Button.IsEnabled = false;
                //Finish_Button.IsEnabled = true;
            }
            else
            {
                Next_Button.IsEnabled = true;
                Finish_Button.IsEnabled = false;
            }
            // Load the user control for the updated current step into the UI
            UserControl uc = UserControls[CurrentStep];
            MainGrid.Children.Add(uc);
            uc.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(uc, 0);
            Grid.SetColumn(uc, 1);
            CurrentControl = uc;
        }

        /// <summary>
        /// Controls behaviour when user clicks the Next button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Next_Click(object sender, RoutedEventArgs e)
        {


            Previous_Button.IsEnabled = true; // Previous button should always be enabled after clicking Next.
            RunTask(CurrentControl.Name); // Run the task assocaited with the current control step
            MainGrid.Children.Remove(CurrentControl); // Remove the current control from the UI. 
            
            // Update the current step and bound checking to decide which buttons to enable/disable.
            if (CurrentStep < (UserControls.Count() - 1))
            {
                CurrentStep += 1;
            }
            if (CurrentStep == (UserControls.Count() - 1))
            {
                //Next_Button.IsEnabled = false;
                //Finish_Button.IsEnabled = true;
            }
            else
            {
                Next_Button.IsEnabled = true;
                Finish_Button.IsEnabled = false;
            }

            // Load the user control for the updated current step into the UI
            UserControl uc = UserControls[CurrentStep];
            uc.VerticalAlignment = VerticalAlignment.Top;
            MainGrid.Children.Add(uc);
            Grid.SetRow(uc, 0);
            Grid.SetColumn(uc, 1);
            CurrentControl = uc;
            LogText.Text = Up.Log; // Update Log
            
            // SPECIAL CASE: If uploading file, disable Next button until user selects a file from RunkTask or the file already exists.

            if (uc.Name == "UserControl2")
            {

                UserControl2 uc2 = CurrentControl as UserControl2; 
                Next_Button.IsEnabled = false;
                //System.Windows.MessageBox.Show(String.Format("CHECK: {0}", Up.ProjectDir));
                
                if (File.Exists(Up.ProjectDir + "\\genes.fasta"))
                {
                    //System.Windows.MessageBox.Show("TRUE");
                    (CurrentControl as UserControl2).LoadFastaFileGrid.Children.Clear();
                    TextBlock txt1 = new TextBlock();
                    FastaParser parser = new FastaParser();
                    try
                    {
                        Up.QuerySequences = parser.Parse(Up.ProjectDir + "\\genes.fasta").ToList();
                    }
                    catch
                    {
                        FatalErrorDialog("Error parsing FASTA file. Please confirm that the input file is in FASTA format. If the problem persists file a bug report at blip.codeplex.com.  The application will now be closed.");
                    }
                    txt1.Text = ("A file with " + Up.QuerySequences.Count() + " gene sequences exists in this folder.\n\nIf you want to load a new file select an empty project folder instead.\r\n");
                    txt1.Margin = new Thickness(55);
                    (CurrentControl as UserControl2).LoadFastaFileGrid.Children.Add(txt1);
                    Next_Button.IsEnabled = true;
                }
                else
                {
                    //System.Windows.MessageBox.Show("FALSE");
                    MainGrid.Children.Remove(CurrentControl);
                    UserControls[CurrentStep] = new UserControl2(Up);
                    uc = UserControls[CurrentStep];
                    uc.VerticalAlignment = VerticalAlignment.Top;
                    MainGrid.Children.Add(uc);
                    Grid.SetRow(uc, 0);
                    Grid.SetColumn(uc, 1);
                    CurrentControl = uc; 
                }
                (uc as UserControl2).RunCompleted += delegate(object sender1, RoutedEventArgs arg)
                {
                    Next_Button.IsEnabled = true;
                    Previous_Button.IsEnabled = true;
                };
            }

            
            
            if (uc.Name == "UserControl7")
            {
                (CurrentControl as UserControl7).CollectionUrlBox.Text = Up.CollectionUrl;
            }
            
        }

        /// <summary>
        /// Load first User Control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            UserControl uc = UserControls[CurrentStep];
            MainGrid.Children.Add(uc);
            uc.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetRow(uc, 0);
            Grid.SetColumn(uc, 1);
            CurrentControl = uc;
            Next_Button.IsEnabled = true;
            LogMessage("Program started on " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + "\r\n");
        }

        /// <summary>
        /// Exit program 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            LogMessage("Program Terminated.\r\n");
            string messageBoxText = "Are you sure you want to exit the program?";
            string caption = "Confirm Exit";
            MessageBoxButton button = MessageBoxButton.YesNoCancel;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    Application.Current.Shutdown();
                    break;
                case MessageBoxResult.No:
                    break;
                case MessageBoxResult.Cancel:
                    break;
            }
        }

        private void FatalErrorDialog(string messageBoxText)
        {
            string caption = "Program Error";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Warning;
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, button, icon);
            Application.Current.Shutdown();
        }


        /// <summary>
        /// Run task for a particular step. 
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        private void RunTask(string task)
        {
            bool res = false;
            switch (task)
            {
                case "UserControl0":
                    break;
                case "UserControl1":
                    Util.SetupDirectories(Up.ProjectDir);
                    break;
                case "UserControl2":
                    if ((!File.Exists(Up.ProjectDir + "\\genes.fasta")) ||
                        (File.Exists(Up.ProjectDir + "\\genes.fasta") && (Up.FastaFile != "")))
                    {
                        FastaParser parser = new FastaParser();
                        try
                        {
                            Up.QuerySequences = parser.Parse(Up.FastaFile).ToList();
                        }
                        catch
                        {
                            FatalErrorDialog("Error parsing FASTA file. Please confirm that the input file is in FASTA format. If the problem persists file a bug report at blip.codeplex.com.  The application will now be closed.");
                        }
                        File.Copy(Up.FastaFile, Up.ProjectDir + "\\genes.fasta", true);
                    }
                    break;
                case "UserControl3":
                    break;
                case "UserControl4":
                    UserControl4 uc4 = (CurrentControl as UserControl4);
                    BlastUtil.RecordBlastThresholds(Up, uc4, ((ComboBoxItem)uc4.BlastProgram.SelectedItem).Content.ToString(), uc4.BlastDatabase.SelectedItem.ToString(), uc4.BlastAlgorithm.SelectedItem.ToString());
                    /*
                    MessageBox.Show(String.Format("Recorded Parameters:\n{0}\n{1}\n{2}\n{3}\n{4}\n{5}\n{6}\n{7}",
                            Up.BlastProgram,
                            Up.BlastDatabase,
                            Up.BlastGeneticCode, 
                            Up.BlastAlgorithm, 
                            Up.BlastMaxEvalue, 
                            Up.BlastMaxNumHits, 
                            Up.BlastMinPercentIdentity, 
                            Up.BlastMinPercentQueryCoverage
                            ));
                    */
                    LaunchBlastPipeline(Up.ProjectDir + "\\genes.fasta");
                    break;
                case "UserControl5":
                    LogMessage("Set Pivot parameters.\r\n");
                    break;
                case "UserControl6":
                    UserControl6 uc6 = (CurrentControl as UserControl6);
                    //Pivot.RecordPivotParameters(Up, uc6.CollectionNameBox.Text, uc6.CollectionTitleBox.Text);
                    Pivot.RecordPivotParameters(Up, "blip", uc6.CollectionTitleBox.Text);
                    break;
                case "UserControl7":
                    break;
                case "UserControl8":
                    UserControl8 uc8 = (CurrentControl as UserControl8);
                    // progressBar1.Maximum = Up.QuerySequences.Count();
                    // progressBar1.Minimum = 0;
                    // progressBar1.Value = 0;
                    uc8.SaveImagePreviewState();
                    res = WriteCollection();
                    break;
                case "UserControl9":
                    UserControl9 uc9 = (CurrentControl as UserControl9);
                    if (uc9.createProject.IsChecked == true)
                    {
                        Debug.WriteLine("NEW");
                        UserControls.Clear();
                        UserControl0 uc0 = new UserControl0(Up);
                        UserControls.Add(uc0);
                        UserControl9 c9 = new UserControl9(Up);
                        UserControls.Add(c9);
                        UserControl1 uc1 = new UserControl1(Up);
                        UserControls.Add(uc1);
                        UserControl2 uc2 = new UserControl2(Up);
                        UserControls.Add(uc2);
                        //UserControl3 uc3 = new UserControl3(Up);
                        //UserControls.Add(uc3);
                        UserControl6 c6 = new UserControl6(Up);
                        UserControls.Add(c6);
                        UserControl4 c4 = new UserControl4(Up);
                        UserControls.Add(c4);
                        UserControl5 uc5 = new UserControl5(Up);
                        UserControls.Add(uc5);
                        UserControl8 c8 = new UserControl8(Up);
                        UserControls.Add(c8);
                        UserControl7 uc7 = new UserControl7(Up);
                        UserControls.Add(uc7);
                    }
                    if (uc9.loadProject.IsChecked == true)
                    {
                        UserControls.Clear();
                        UserControl0 uc0 = new UserControl0(Up);
                        UserControls.Add(uc0);
                        UserControl9 c9 = new UserControl9(Up);
                        UserControls.Add(c9);
                        UserControl10 c10 = new UserControl10(Up);
                        UserControls.Add(c10);
                        UserControl11 c11 = new UserControl11(Up);
                        UserControls.Add(c11);
                    }
                    break;
                case "UserControl10":
                    UserControl10 uc10 = (CurrentControl as UserControl10);
                    Debug.WriteLine("UC10");
                    
                    
                    Action<object> action = (object obj) =>
                    {
                        StartWebServer("/", Up.CxmlDir, Up.WebServerPort);
                    };
                    Task t1 = new Task(action, "BLiP_WS");
                    t1.Start();
                    Previous_Button.IsEnabled = false;
                    Next_Button.IsEnabled = false;
                    Finish_Button.IsEnabled = true;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// NCBIBlastService may fail due to proxy setting, toggle proxy to try and recover.
        /// </summary>
        public void ToggleProxy()
        {
            if (Up.BlastUseBrowserProxy == true)
            {
                Up.BlastUseBrowserProxy = false;
            }
            else
            {
                Up.BlastUseBrowserProxy = true;
            }
        }

        /// <summary>
        /// A helper method for the BLAST task. Submits a job to BLAST.
        /// </summary>
        /// <param name="qp"></param>
        /// <returns></returns>
        private BlastJob submit(QueueSequence qp)
        {
            // Configure BLAST service
            ConfigParameters configParams = new ConfigParameters();
            configParams.UseBrowserProxy = Up.BlastUseBrowserProxy;
            BlastParameters searchParams = new BlastParameters();

            string query = ">" + qp.Sequence.DisplayID + "\n" + qp.Sequence.ToString();
            
            searchParams.Add("Program", Up.BlastProgram);
            searchParams.Add("Database", Up.BlastDatabase);
            searchParams.Add("GeneticCode", Up.BlastGeneticCode);
            searchParams.Add("Expect", "10");
            searchParams.Add("Query", query);


            // Create BLAST service
            NCBIBlastHandler blastService = new NCBIBlastHandler();
            
            blastService.Configuration = configParams;

            // Submit BLAST Job
            string jobId;
            
            try
            {
                jobId = blastService.SubmitRequest(qp.Sequence, searchParams);
            }
            catch(Exception eee)
            {
                if (eee.Message.Contains("WebException"))
                {
                    ToggleProxy();
                }
                BlastJob blastJob2 = new BlastJob();
                blastJob2.JobStatus = BlastJob.FAILED;
                blastJob2.Query = qp.Sequence;
                blastJob2.Position = qp.Position;
                return blastJob2;
            }
            // Make sure job was submitted successfully
            ServiceRequestInformation info = blastService.GetRequestStatus(jobId);

            if (info.Status != ServiceRequestStatus.Waiting
                && info.Status != ServiceRequestStatus.Ready
                && info.Status != ServiceRequestStatus.Queued)
            {
                //Console.WriteLine("\tError Submitting Job: " + info.Status);
                BlastJob blastJob2 = new BlastJob();
                blastJob2.JobStatus = BlastJob.FAILED;
                blastJob2.Query = qp.Sequence;
                blastJob2.Position = qp.Position;
                return blastJob2;
            }

            Thread.Sleep(BlastQueue.SubmitDelay);
            //Console.WriteLine("\tSuccessfully submitted jobId: " + jobId);
            // Return a BlastJob, set jobStatus to BUSY
            BlastJob blastJob = new BlastJob()
            {
                JobId = jobId,
                SearchParams = searchParams,
                ConfigParams = configParams,
                BlastService = blastService,
                JobStatus = BlastJob.BUSY,
                Query = qp.Sequence,
                Position = qp.Position

            };
            return blastJob;
        }

        /// <sumamry>
        /// This is the task for UserControl4. Runs BLAST for all the sequences in a thread. The real work is done by Blastp property.
        /// </summary>
        private void LaunchBlastPipeline(string filename)
        {
            Action<object> action = (object obj) =>
            {
                DoBlastPipeline(filename);
            };
            Task t1 = new Task(action, "translate");
            t1.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private void DoBlastPipeline(string filename)
        {

            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {
                UserControl5 uc5 = CurrentControl as UserControl5;
                progressBar1.Visibility = System.Windows.Visibility.Visible;
                progressText1.Visibility = System.Windows.Visibility.Visible;
                progressBar1.Value = 0;
                Next_Button.IsEnabled = false;
                Previous_Button.IsEnabled = false;
                uc5.UserControl5TextBlock2.Visibility = System.Windows.Visibility.Hidden;
                uc5.UserControl5Step1.Foreground = System.Windows.Media.Brushes.Gray;
                uc5.UserControl5Step2.Foreground = System.Windows.Media.Brushes.Gray;
                uc5.UserControl5Step3.Foreground = System.Windows.Media.Brushes.Gray;
                uc5.UserControl5Step4.Foreground = System.Windows.Media.Brushes.Gray;
            }));
            
            DoBLAST(filename);
            DoGenBank();
            
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {
                (CurrentControl as UserControl5).UserControl5Step3.Foreground = System.Windows.Media.Brushes.Black;
            }));
            
            Collection collection = CreateCollection();
            Up.Collection = collection;
            if (Up.Collection.Items.Count() == 0)
            {
                MessageBox.Show("The BLAST process or filtering of the results returned no hits. Please revise the BLAST parameters in the previous step.");
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                {
                    Next_Button.IsEnabled = false;
                    Previous_Button.IsEnabled = true;
                }));
            }
            else
            {

                SetItemBgIndex(Up.Collection);

                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                {
                    UserControl5 uc5 = CurrentControl as UserControl5;
                    uc5.UserControl5Step4.Foreground = System.Windows.Media.Brushes.Black;
                    uc5.UserControl5TextBlock2.Visibility = System.Windows.Visibility.Visible;
                    uc5.UserControl5TextBlock1.Text += "\n\nClick Next to proceed.";
                    Next_Button.IsEnabled = true;
                    Previous_Button.IsEnabled = true;
                    progressBar1.Visibility = System.Windows.Visibility.Hidden;
                    progressText1.Visibility = System.Windows.Visibility.Hidden;
                }));
            }
        }

        private void DoBLAST(string filename)
        {
            // Update progress bar, set content of user control to initial state
            

            // Load protein sequences
            FastaParser parser = new FastaParser();
            IList<ISequence> queryList = parser.Parse(filename).ToList();

            // Initialize and populate queue of query sequences
            Queue<QueueSequence> queryQueue = new Queue<QueueSequence>();
            int j = 0; // For debuging
            int progValue = 0;
            int currentProgress = 0;
            foreach (ISequence protein in queryList)
            {
                QueueSequence qp = new QueueSequence();
                qp.Sequence = protein;
                qp.Position = j;
                string name = j.ToString();
                j++;
                if (File.Exists(Up.ProjectDir + "\\xml\\" + name + ".xml"))
                {
                    IList<BlastResult> blastResults;
                    BlastXmlParser parser2 = new BlastXmlParser();
                    try
                    {
                        blastResults = parser2.Parse(Up.ProjectDir + "\\xml\\" + name + ".xml");
                        progValue = Convert.ToInt32(Math.Round((double)currentProgress / queryList.Count() * 100, 0));
                        UpdateProgressBar(progValue, "Validating BLAST results.");
                        currentProgress++;
                    }
                    catch
                    {
                        queryQueue.Enqueue(qp);
                    }
                }
                else
                {
                    queryQueue.Enqueue(qp);
                }
            }

            // Initialize BLAST queue positions to having no jobs (EMPTY)
            BlastQueue blastQueue = new BlastQueue();
            // While there are proteins left to submit to BLAST, or there are 
            // busy jobs still on the queue
            UpdateProgressBar(progValue, "Starting up BLAST service, please wait.");
            while (queryQueue.Count > 0 || blastQueue.isBlastQueueBusy())
            {
                // Iterate over blastQueue
                for (int i = 0; i < BlastQueue.Length; i++)
                {
                    // Get blastJob from array and update status
                    BlastJob blastJob = blastQueue[i];
                    QueueSequence qp = new QueueSequence();
                    qp.Sequence = blastJob.Query;
                    qp.Position = blastJob.Position;
                    // if queue position is AVAILABLE
                    if (blastJob.JobStatus == BlastJob.AVAILABLE)
                    {
                        if (queryQueue.Count > 0)
                        {
                            QueueSequence qp2 = queryQueue.Dequeue();
                            // try to submit job, enqueue back the protein if submission failed.
                            try
                            {
                                blastQueue[i] = submit(qp2);
                                if (blastQueue[i].JobStatus == BlastJob.FAILED)
                                {
                                    blastQueue[i].JobStatus = BlastJob.AVAILABLE;
                                    queryQueue.Enqueue(qp2);
                                }
                                else
                                {
                                    UpdateProgressBar(progValue, "Submitting sequences to NCBI BLAST");
                                }
                            }
                            catch (Exception eee)
                            {
                                MessageBox.Show(eee.Message);
                            }
                        }
                    }
                    else
                    {
                        string jobId = blastJob.JobId;
                        NCBIBlastHandler blastService = blastJob.BlastService;
                        ServiceRequestInformation info = blastService.GetRequestStatus(jobId);
                        Thread.Sleep(BlastQueue.RequestDelay);
                        switch (info.Status)
                        {
                            case ServiceRequestStatus.Error:
                                blastQueue[i].JobStatus = BlastJob.AVAILABLE;
                                queryQueue.Enqueue(qp);
                                break;
                            case ServiceRequestStatus.Canceled:
                                blastQueue[i].JobStatus = BlastJob.AVAILABLE;
                                queryQueue.Enqueue(qp);
                                break;
                            case ServiceRequestStatus.Ready:
                                string result = blastService.GetResult(jobId, blastJob.SearchParams);
                                string name = blastJob.Position.ToString();
                                TextWriter tw = new StreamWriter(Up.ProjectDir + "\\xml\\" + name + ".xml");
                                tw.Write(result);
                                tw.Close();
                                Debug.WriteLine("BLAST JOB: " + jobId + " , " + name + " , " + info.StatusInformation);
                                
                                // Added by VF on Jan, 22, 2013. Catches invalid BLAST records
                                IList<BlastResult> blastResults;
                                BlastXmlParser parser2 = new BlastXmlParser();
                                bool parsePassed = false;
                                int fetchAttempts = 0;
                                while (!parsePassed && fetchAttempts < 3)
                                {
                                    try
                                    {
                                        
                                        blastResults = parser2.Parse(Up.ProjectDir + "\\xml\\" + name + ".xml");
                                        parsePassed = true;
                                        Debug.WriteLine("FETCH OK  JobId: " + jobId + " InputOrder: " + name + ". This is attempt:" + fetchAttempts.ToString());

                                    }   
                                    catch (Exception eee)
                                    {
                                        Debug.WriteLine("Trying to fetch JobId: " + jobId + " InputOrder: " + name + ". This is attempt: " + fetchAttempts.ToString());
                                        parsePassed = false;
                                        result = blastService.GetResult(jobId, blastJob.SearchParams);
                                        TextWriter tw2 = new StreamWriter(Up.ProjectDir + "\\xml\\" + name + ".xml");
                                        tw2.Write(result);
                                        tw2.Close();
                                        fetchAttempts += 1;
                                    }
                                    Thread.Sleep(1000);                                
                                }
                                
                                try
                                {
                                    blastResults = parser2.Parse(Up.ProjectDir + "\\xml\\" + name + ".xml");
                                }
                                catch (Exception eee)
                                {
                                    blastQueue[i].JobStatus = BlastJob.AVAILABLE;
                                    queryQueue.Enqueue(qp);
                                    Debug.WriteLine("REQUEUE of JobId: " + " " + jobId + " InputOrder: " + name + " because max fetch is " + fetchAttempts.ToString());
                                    break;
                                }
                                
                                currentProgress += 1;
                                progValue = Convert.ToInt32(Math.Round((double)currentProgress / queryList.Count() * 100, 0));
                                UpdateProgressBar(progValue, "Saving");
                                blastQueue[i].JobStatus = BlastJob.AVAILABLE;
                                if (queryQueue.Count > 0)
                                {
                                    QueueSequence qp3 = queryQueue.Dequeue();
                                    try
                                    {
                                        blastQueue[i] = submit(qp3);

                                        if (blastQueue[i].JobStatus == BlastJob.FAILED)
                                        {
                                            blastQueue[i].JobStatus = BlastJob.AVAILABLE;
                                            queryQueue.Enqueue(qp3);

                                        }
                                        else
                                        {
                                            UpdateProgressBar(progValue, "Submitting sequences to NCBI BLAST");
                                        }
                                    }
                                    catch (Exception eee)
                                    {
                                        MessageBox.Show(eee.Message);
                                        MessageBox.Show("Error creating a jobId for sequence " + qp3.Position);
                                        throw new Exception("Error creating a jobId for sequence" + qp3.Position);
                                    }
                                }
                                break;
                            case ServiceRequestStatus.Queued:
                                break;
                            case ServiceRequestStatus.Waiting:
                                break;
                            default:
                                MessageBox.Show("BLAST error " + info.Status + " " + blastJob.JobStatus + " for " + qp.Position);
                                break;
                        }
                    }
                }
            }
        }

        
        private string GetGenbankUrl(GenBankItem gitem){
            string url = "";
            
            if ((Up.BlastProgram == "blastn") || (Up.BlastProgram == "tblastn") || (Up.BlastProgram == "tblastx"))
            {
                url = "http://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi?tool=FetchGenBank&email=blip@gmail.com&db=nucleotide&retmode=text&rettype=gb&id=";
                url += gitem.Id.ToString();
                url += "&seq_start=" + gitem.HitStart;
                url += "&seq_stop=" + gitem.HitEnd;
                //System.Windows.Forms.MessageBox.Show(url);
            }
            else
            {
                url = "http://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi?tool=FetchGenBank&email=blip@gmail.com&db=protein&retmode=text&rettype=gp&id=";
                url += gitem.Id.ToString();
                //MessageBox.Show("Protein!");
            }
            return url;
        }
        
        private void DoGenBank()
        {
            int progValue = 0;
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {
                (CurrentControl as UserControl5).UserControl5Step1.Foreground = System.Windows.Media.Brushes.Black;
                progressBar1.Value = progValue;
            }));

            string inputDir = Up.ProjectDir + "\\xml";
            if (!Directory.Exists(inputDir))
            {
                throw new Exception("Directory " + inputDir + " does not exist.");
            }
            string[] blastXmlFiles = Directory.GetFiles(inputDir, "*.xml");
            int c = 1;
            Stack<GenBankItem> giList = new Stack<GenBankItem>();
            foreach (string blastFile in blastXmlFiles)
            {
                BlastXmlParser blastParser = new BlastXmlParser();
                progValue = Convert.ToInt32(Math.Round((double)c / blastXmlFiles.Count() * 100, 0));
                UpdateProgressBar(progValue, "Filtering results");
                try
                {
                    IList<BlastResult> blastResults = blastParser.Parse(blastFile);

                    List<GenBankItem> recordGiList = BlastUtil.filter(blastResults, Up.BlastMaxNumHits, Up.BlastMaxEvalue, Up.BlastMinPercentIdentity, Up.BlastMinPercentQueryCoverage);
                    foreach (GenBankItem gi in recordGiList)
                    {
                        giList.Push(gi);
                        Debug.WriteLine(gi.HitStart.ToString() + " " + gi.HitEnd.ToString());
                    }
                }
                catch
                {
                    FatalErrorDialog("Cannot parse " + blastFile);
                    Debug.WriteLine("Cannot parse " + blastFile);
                }
                c += 1;
            }
            progValue = 0;
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {
                (CurrentControl as UserControl5).UserControl5Step2.Foreground = System.Windows.Media.Brushes.Black;
                progressBar1.Value = progValue;
            }));

            int totalGi = giList.Count();
            GenBankParser genkBankParser = new GenBankParser();
            int unParsableCount = 0;
            int notDownloadedCount = 0;
            string unParsableGIs = "";
            string notDownloadedGIs = "";
            bool isConnected = true;
            if (!IsConnectedToInternet())
            {
                isConnected = false;
                MessageBox.Show("Your internet connection appears to be down. As a result, missing GenBank records will not be downloaded.");
            }
            
            while (giList.Count > 0)
            {
                GenBankItem gitem = giList.Pop();
                progValue = Convert.ToInt32(Math.Round(((totalGi - giList.Count()) / (double)totalGi) * 100, 0));
                UpdateProgressBar(progValue, "Downloading GenBank records");
                string outFilename = Up.ProjectDir + "\\gb\\" + gitem.Id;
                outFilename += "_" + gitem.HitStart.ToString();
                outFilename += "_" + gitem.HitEnd.ToString();
                outFilename += ".gb";

                WebClient wc = new WebClient();

                if (File.Exists(outFilename))
                {
                    try
                    {
                        ISequence gpitem = genkBankParser.ParseOne(outFilename);
                    }
                    catch
                    {
                        if (isConnected)
                        {

                            string url = GetGenbankUrl(gitem);
                            try
                            {
                                wc.DownloadFile(url, outFilename);
                                Thread.Sleep(1000);
                            }
                            catch
                            {
                                wc.Proxy = null;
                                giList.Push(gitem);
                            }
                            try
                            {
                                ISequence gpitem = genkBankParser.ParseOne(outFilename);
                            }
                            catch
                            {
                                unParsableCount += 1;
                                unParsableGIs += gitem.Id + ",";
                            }
                        }
                        else
                        {
                            notDownloadedCount += 1;
                            notDownloadedGIs += gitem.Id + ",";
                        }
                    }

                }
                else
                {
                    if (isConnected)
                    {
                        string url = GetGenbankUrl(gitem);
                        try
                        {
                            wc.DownloadFile(url, outFilename);
                            Thread.Sleep(1000);
                        }
                        catch
                        {
                            wc.Proxy = null;
                            giList.Push(gitem);
                        }
                        try
                        {
                            ISequence gpitem = genkBankParser.ParseOne(outFilename);
                        }
                        catch
                        {
                            unParsableCount += 1;
                            unParsableGIs += gitem.Id + ",";
                        }
                    }
                    else
                    {
                        notDownloadedCount += 1;
                        notDownloadedGIs += gitem.Id + ",";
                    }
                }
            }

            if (notDownloadedCount > 0)
            {
                MessageBox.Show("Error downloading GenBank records: " + notDownloadedGIs + ".\r\nThis is likely caused by an interruption in the internet connection. Re-attempt the download by repeating this step.\r\n");
            }
            if (unParsableCount > 0)
            {
                MessageBox.Show("Error parsing GenBank records: " + unParsableGIs + ".\r\nThis is likely due to an unsupported field in the GenBank record. Contact the MBF development team at http://mbf.codeplex.com, and include one of the GI numbers in the bug report.\r\nYou can copy this message to the clipboard using Ctrl-C.\r\n");
            }
        }


        private void IncrementProgressBar(string message)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {
                progressBar1.Value += 1;
                int progValue = Convert.ToInt32(Math.Round((progressBar1.Value / (double)progressBar1.Maximum), 2) * 100);
                progressText1.Text = progValue + "% " + message;
            }));
        }
        
        private void UpdateProgressBar(int value, string message)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {
                progressBar1.Value = value;
                progressText1.Text = value + "% " + message;
            }));
        }

        /// <summary>
        /// This is the task for UserControl7
        /// Creates a Pivot Collection
        /// </summary>
        public bool WriteCollection()
        {
            Action<object> action = (object obj) =>
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                {
                    progressBar1.Visibility = System.Windows.Visibility.Visible;
                    progressText1.Visibility = System.Windows.Visibility.Visible;
                    progressBar1.Value = 0;
                    Next_Button.IsEnabled = false;
                    Previous_Button.IsEnabled = false;
                    (CurrentControl as UserControl7).UserControl7Step1.Foreground = System.Windows.Media.Brushes.Gray;
                    (CurrentControl as UserControl7).UserControl7Step2.Foreground = System.Windows.Media.Brushes.Gray;
                    (CurrentControl as UserControl7).PivotGrid.Visibility = System.Windows.Visibility.Hidden;
                }));
                
                TextWriter tw2 = new StreamWriter(Up.CollectionPath + "\\" + Up.CollectionName + ".cxml");
                tw2.Write(Up.Collection.ToXml());
                tw2.Close();
                
                DrawImages(Up.Collection);

                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                {
                    (CurrentControl as UserControl7).UserControl7Step1.Foreground = System.Windows.Media.Brushes.Black;
                }));

                UpdateProgressBar(0, "Collecting results");
                TileImages();

                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                {
                   (CurrentControl as UserControl7).UserControl7Step2.Foreground = System.Windows.Media.Brushes.Black;
                }));

                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                {
                    Next_Button.IsEnabled = false;
                    Previous_Button.IsEnabled = true;
                    (CurrentControl as UserControl7).PivotGrid.Visibility = System.Windows.Visibility.Visible;
                    if (!File.Exists(Up.PivotEXE))
                    {
                        //(CurrentControl as UserControl7).UserControl7TextBlock2.Text = "Pivot is not installed in the default location:";
                        //(CurrentControl as UserControl7).LauchPivot.Content = "Get Pivot";
                    }
                    progressBar1.Visibility = System.Windows.Visibility.Hidden;
                    progressText1.Visibility = System.Windows.Visibility.Hidden;
                    Up.CxmlDir = Up.CollectionPath;
                    Debug.WriteLine(Up.CollectionPath);
                    string processPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    Debug.WriteLine(processPath);
                    try
                    {
                        System.IO.File.Copy(processPath + "\\blip.html", Up.CxmlDir + "\\blip.html", true);
                        System.IO.File.Copy(processPath + "\\BL!PPivotPiewer.xap", Up.CxmlDir + "\\BL!PPivotPiewer.xap", true);
                        System.IO.File.Copy(processPath + "\\Silverlight.js", Up.CxmlDir + "\\Silverlight.js", true);
                    }
                    catch
                    {
                        MessageBox.Show("There was an error copying required files. To solve this try any of the following:\n- UNCHECK the read-only flag for the following files in the project directory: *.html, *.xap, *.js.\n- DELETE the following files in the project directory: *.html, *.xap, *.js.\n- Create a new project.");
                    }
                    StartWebServer("/", Up.CxmlDir, Up.WebServerPort);
                    Previous_Button.IsEnabled = false;
                    Finish_Button.IsEnabled = true;
                }));
            };
            
            Task t1 = new Task(action, "CreateCollection");
            t1.Start();
            return true;
        }
        
        public void DrawImages(Collection collection)
        {
            Dictionary<string, int> facetValues = new Dictionary<string, int>();
            int ic = 0;

            foreach (Item item in collection.Items)
            {
                ic++;
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
                {
                    int p = calcProgress(ic, collection.Items.Count());
                    progressBar1.Value = p;
                    progressText1.Text = "Drawing images (" + p + "%)";
                }));
                Image image = new Image(item, Up.FacetCategories);
                image.DrawAndSaveImage(Up.CollectionImagePath + "\\" + item.Id + ".png", Up.BackgroundFacetCategory, Up.BackgroundColorScheme);
            }
        }

        public List<string> GetCollectionImages()
        {
            DirectoryInfo di = new DirectoryInfo(Up.CollectionImagePath);
            List<string> images = new List<string>();
            foreach (var filename in di.GetFiles("*.png"))
            {
                images.Add(filename.Name);
            }
            return images;
        }

        
        
        /// <summary>
        /// Create the Pivot collection.
        /// Uses global parameters from UIParameters.
        /// </summary>
        /// <returns>Object representing a Pivot collection.</returns>
        public Collection CreateCollection()
        {
            
            // Create an empty collection
            string imgBase = Up.CollectionUid + "_deepzoom\\" + Up.CollectionUid + "_deepzoom.xml";
            Collection collection = new Collection(Up.CollectionTitle, imgBase);
            collection.FacetCategories = Up.FacetCategories;
            // Create output directories
            if (!Directory.Exists(Up.CollectionPath)) { Directory.CreateDirectory(Up.CollectionPath); }
            if (!Directory.Exists(Up.CollectionImagePath)) { Directory.CreateDirectory(Up.CollectionImagePath); }
            
            // Init current sequence position and item number.
            int seqPos = 0;
            int itemId = 0;
            
            // For each query sequence (parsed from FASTA file). 
            // I this version, the only property used from the query sequence is the displayID i.e. can be factored out.
            int progValue = 0;
            foreach (ISequence rec in Up.QuerySequences)
            {
                //GeneSequence gene = Collection.ParseGene(rec);
                // Update  the progress bar in the WPF app
                progValue = Convert.ToInt32(Math.Round((seqPos / (double)Up.QuerySequences.Count()), 2) * 100);
                UpdateProgressBar(progValue, "Generating Output");
                itemId = Pivot.CreateItems(Up, rec, itemId, seqPos, collection);
                // parse name from the query seq displayID property
                seqPos += 1;
            }
            //MessageBox.Show(collection.Items.Count().ToString());
            return collection;
            
        }
        
        public void TileImages()
        {
            if (!Directory.Exists(Up.CollectionDeepzoomPath))
            {
                Directory.CreateDirectory(Up.CollectionDeepzoomPath);
            }

            if (!Directory.Exists(Up.CollectionDeepzoomPath + "_tmp"))
            {
                Directory.CreateDirectory(Up.CollectionDeepzoomPath + "_tmp");
            }


            List<string> images = GetCollectionImages();
            List<string> data = new List<string>();

            //sort by filename
            int offset = 4;
            var sortedFiles = from imgF in images
                              orderby Int32.Parse(imgF.Substring(0, imgF.Length - offset)) ascending
                              select imgF;

            IList<string> sortedFileList = sortedFiles.ToList();
            UpdateProgressBar(0, "Generating Images");

            
            
            int numImages = sortedFiles.Count();
            int updateInterval =  Convert.ToInt32(Math.Ceiling(numImages / 100.0));
            //MessageBox.Show(sortedFiles.Count().ToString());
            
            string timeInterval = DateTime.Now.ToString();
            int count = 0;
            ImageCreator ic = new ImageCreator();
            ic.TileSize = 512;
            ic.TileFormat = ImageFormat.Png;
            ic.ImageQuality = 0.92;
            ic.TileOverlap = 0;
            ic.MaxLevel = Convert.ToInt32(Math.Log(ic.TileSize, 2));
            //Parallel.For(0, numImages, i =>
            for (int i = 0; i < numImages; i++)
            {
                string image = sortedFileList[i];
                if ((count % updateInterval) == 0)
                {
                    int progValue = Convert.ToInt32(Math.Round((double)count / sortedFiles.Count() * 100, 0));
                    UpdateProgressBar(progValue, "Generating Images");
                }
                count++;

                string target = Up.CollectionDeepzoomPath + "\\" + image.Substring(0, (image.Length - 4)) + ".xml";
                data.Add(target);
                ic.Create(Up.CollectionImagePath + "\\" + image, target);
                //TileSingleImage(image, target);
            //});
            }
            timeInterval += "\n" + DateTime.Now.ToString();
            //MessageBox.Show(timeInterval);

            /*
            foreach (string image in sortedFiles)
            {
                string target = Up.CollectionDeepzoomPath + "\\" + System.IO.Path.GetFileNameWithoutExtension(image);
                data.Add(System.IO.Path.ChangeExtension(target, ".xml"));
            }

            
            Parallel.ForEach(sortedFiles, (image, state, count) =>
            {
                int progValue = Convert.ToInt32(Math.Round((double)count / sortedFiles.Count() * 100, 0));
                UpdateProgressBar(progValue, "Generating Images");
                TileSingleImage(image);
            });
            */

            CollectionCreator cc = new CollectionCreator();
            cc.TileSize = 512;
            cc.TileFormat = ImageFormat.Png;
            cc.ImageQuality = 0.92;
            cc.TileOverlap = 0;
            cc.MaxLevel = Convert.ToInt32(Math.Log(cc.TileSize, 2));
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {
                progressBar1.Visibility = System.Windows.Visibility.Hidden;
                progressText1.Text = "Processing Images";
            }));
            //UpdateProgressBar(0, "Processing Images");
            /*
            Directory.CreateDirectory(Up.CollectionDeepzoomPath + "\\" + Up.CollectionName + "_deepzoom_files");
            Bitmap img1px = new Bitmap(1, 1);
            for (int i = 0; i <= cc.MaxLevel; i++)
            {
                string levelDir = Up.CollectionDeepzoomPath + "\\" + Up.CollectionName + "_deepzoom_files" + "\\" + i.ToString();
                Directory.CreateDirectory(levelDir);
                img1px.Save(levelDir + "\\0_0.png");
            }

            StreamWriter sw = new StreamWriter(Up.CollectionDeepzoomPath + "\\" + Up.CollectionName + "_deepzoom.xml");
            sw.Write("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Collection MaxLevel=\"{0}\" TileSize=\"{1}\" Format=\"{2}\" NextItemId=\"{3}\" ServerFormat=\"{4}\" xmlns=\"http://schemas.microsoft.com/deepzoom/2009\">\n<Items>",
                    cc.MaxLevel,
                    cc.TileSize,
                    cc.TileFormat,
                    data.Count,
                    cc.ServerFormat);
            
            for (int i = 0; i < numImages; i++)
            {
                string image = sortedFileList[i];
                string name = image.Substring(0, (image.Length - 4));
                string xml = name + ".xml";
                sw.WriteLine(
                    "<I Id=\"{0}\" N=\"{0}\" Source=\"{0}.xml\"><Size Width=\"{1}\" Height=\"{1}\" /></I>",
                    name,
                    cc.TileSize
                    );
            }
            sw.WriteLine("</Items>\n</Collection>");
            sw.Close();
            */
            cc.Create(data, Up.CollectionDeepzoomPath + "\\" + Up.CollectionUid + "_deepzoom");
        }

        private void TileSingleImage(string image, string target)
        {
            ImageCreator ic = new ImageCreator();
            ic.TileSize = 512;
            ic.TileFormat = ImageFormat.Png;
            ic.ImageQuality = 0.92;
            ic.TileOverlap = 0;
            ic.MaxLevel = Convert.ToInt32(Math.Log(ic.TileSize, 2));
            ic.Create(image, target);
        }

        public void SetItemBgIndex(Collection collection)
        {
            foreach (FacetCategory fc in Up.FacetCategories)
            {
                if ((fc.Type == "String") && fc.IsFilterVisible)
                {
                    Dictionary<string, int> facetValues = new Dictionary<string, int>();
                    foreach (Item item in collection.Items)
                    {
                        foreach (Facet f in item.Facets)
                        {
                            if (f.Name == fc.Name)
                            {
                                if (!facetValues.ContainsKey(f[0].Value))
                                {
                                    facetValues[f[0].Value] = 1;
                                }
                                else
                                {
                                    facetValues[f[0].Value] += 1;
                                }
                            }
                        }
                    }

                    var sortedFacetValues = (from entry in facetValues orderby entry.Value descending select entry);
                    int rank = 0;
                    int lastCount = 0;
                    Dictionary<string, int> facetRank = new Dictionary<string, int>();
                    foreach (KeyValuePair<string, int> pair in sortedFacetValues)
                    {
                        if (pair.Value != lastCount)
                        {
                            rank += 1;
                        }
                        facetRank[pair.Key] = rank - 1;
                        lastCount = pair.Value;
                    }
                    foreach (Item item in collection.Items)
                    {
                        foreach (Facet f in item.Facets)
                        {
                            if (f.Name == fc.Name)
                            {
                                f.Rank = facetRank[f[0].Value];
                            }
                        }
                    }
                }
            }
        }

        public int calcProgress(int current, int max)
        {
            int p = Convert.ToInt32(Math.Round((current / (double)max), 2) * 100);
            return p;
        }

        // From http://dotnetslackers.com/VB_NET/re-225627_How_to_check_your_network_connection_state.aspx
        [DllImport("wininet.dll", SetLastError = true)]
        extern static bool InternetGetConnectedState(
           out InternetGetConnectedStateFlags Description, int ReservedValue);

        [Flags]
        enum InternetGetConnectedStateFlags
        {
            INTERNET_CONNECTION_MODEM = 0x01,
            INTERNET_CONNECTION_LAN = 0x02,
            INTERNET_CONNECTION_PROXY = 0x04,
            INTERNET_CONNECTION_RAS_INSTALLED = 0x10,
            INTERNET_CONNECTION_OFFLINE = 0x20,
            INTERNET_CONNECTION_CONFIGURED = 0x40
        }

        public static bool IsConnectedToInternet()
        {
            InternetGetConnectedStateFlags flags;
            var ret = InternetGetConnectedState(out flags, 0);
            Console.WriteLine(flags.ToString());
            return ret;
        }

        private static void StartWebServer(string uri, string absolutePath, int port)
        {
            // create a server.
            var server = new Server();
            // add module to serve files i.e. act like a web server
            var module = new FileModule();
            
            // Add mime types for Silverlight PivotViewer
            module.ContentTypes.Add("xaml", new ContentTypeHeader("application/xaml+xml"));
            module.ContentTypes.Add("xap", new ContentTypeHeader("application/x-silverlight-2"));
            module.ContentTypes.Add("cxml", new ContentTypeHeader("text/xml"));
            module.ContentTypes.Add("xml", new ContentTypeHeader("text/xml"));
            module.ContentTypes.Add("dzi", new ContentTypeHeader("text/xml"));
            module.ContentTypes.Add("dzc", new ContentTypeHeader("text/xml"));
            
            module.Resources.Add(new FileResources("/", absolutePath));
            server.Add(module);

            // use one http listener.
            server.Add(HttpListener.Create(IPAddress.Any, port));
            // start server, can have max 5 pending accepts.
            server.Start(5);
        }
        
    }
}

