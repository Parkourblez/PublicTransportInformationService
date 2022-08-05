using Microsoft.Win32;
using PublicTransportInformationService.Algorithms;
using PublicTransportInformationService.Algorithms.BaseClasses;
using PublicTransportInformationService.DataStructures;
using PublicTransportInformationService.DataStructures.RouteInfoFactory;
using PublicTransportInformationService.DataStructures.RouteInfoFactory.BaseClasses;
using PublicTransportInformationService.Tools.Parser;
using PublicTransportInformationService.Tools.Parser.BaseClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace PublicTransportInformationService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<RouteInfo> routesInfoList;
        private int startPoint;
        private int finishPoint;
        private TimeSpan startTime;
        private RoutePathAlgorithmBase fastestRouteAlgo;
        private RoutePathAlgorithmBase cheapestRouteAlgo;
        private CancellationTokenSource cancellationTokenSource;

        private object loadFileLockObject = new object();

        public MainWindow()
        {
            Dispatcher.UnhandledException += OnUnhandledException;
            Application.Current.Exit += OnApplicationExit;

            InitializeComponent();
        }

        private void OnApplicationExit(object sender, ExitEventArgs args)
        {
            Dispatcher.UnhandledException -= OnUnhandledException;
            Application.Current.Exit -= OnApplicationExit;
        }

        private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string firstStackCall = e.Exception.StackTrace.Split("   at ").Skip(1).FirstOrDefault();
            MessageBox.Show("An unhandled exception was thrown.\n\n" + e.Exception.Message + "\n\n"+ firstStackCall, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void Load_Data_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt";
            openFileDialog.FileOk += OpenFileDialog_FileOk;
            openFileDialog.ShowDialog();
        }

        private async void OpenFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var dialog = sender as OpenFileDialog;
            if (dialog != null && dialog.CheckFileExists)
            {
                chkBox.IsChecked = true;
                infoOutput.Text = "Loading data...";

                List<RouteInfo> routesInfoList = null;
                await Task.Run(() =>
                {
                    string sourceDataString = string.Empty;
                    lock (loadFileLockObject)
                    {
                        using FileStream stream = File.OpenRead(dialog.FileName);
                        using StreamReader reader = new StreamReader(stream);
                        sourceDataString = reader.ReadToEnd();
                    }
                    if (string.IsNullOrEmpty(sourceDataString) || string.IsNullOrWhiteSpace(sourceDataString))
                    {
                        return;
                    }

                    RouteInfoParserBase parser = new TextRouteInfoParser();
                    RouteInfoFactoryBase routeInfoFactory = new TextRouteInfoFactory(parser);
                    routesInfoList = routeInfoFactory.GenerateRoutesInfoBasedOn(sourceDataString);
                    fastestRouteAlgo = new ShortestRoutePathAlgorithm(routesInfoList);
                    cheapestRouteAlgo = new CheapestRoutePathAlgorithm(routesInfoList);
                });

                if (routesInfoList == null || !routesInfoList.Any())
                {
                    infoOutput.Text = "No data loaded. Old data will be used if loaded before.";
                }
                else
                {
                    this.routesInfoList = routesInfoList;
                    infoOutput.Text = "Data loaded.";
                }
            }
        }

        private async void Compute_Button_Click(object sender, RoutedEventArgs e)
        {
            infoOutput.Text = "Calculating...";
            if (fastestRouteAlgo == null ||
                !routesInfoList.Any(routeInfo => routeInfo.RoutePartsTripDuration.Keys.Contains(startPoint)) ||
                !routesInfoList.Any(routeInfo => routeInfo.RoutePartsTripDuration.Keys.Contains(finishPoint)) ||
                string.IsNullOrWhiteSpace(tripStartTime.Text))
            {
                infoOutput.Text = "No input provided or values are incorrect. ";
                return;
            }

            ClearOutput();

            CancellationTokenSource ts = new CancellationTokenSource();

            bool isCanceled = false;
            chkBox.IsChecked = false;

            var fastestRouteAlgoTask = Task.Run(() =>
            {
                try
                {
                    fastestRouteAlgo.Initialize(startPoint, finishPoint, startTime);
                    fastestRouteAlgo.Compute(ts.Token);
                }
                catch(OperationCanceledException e) { isCanceled = true; }
            });

            var cheapestRouteAlgoTask = Task.Run(() =>
            {
                try
                {
                    cheapestRouteAlgo.Initialize(startPoint, finishPoint, startTime);
                    cheapestRouteAlgo.Compute(ts.Token);
                }
                catch (OperationCanceledException e) { isCanceled = true; }
            });

            cancellationTokenSource = ts;

            Task[] tasks = new Task[] { fastestRouteAlgoTask, cheapestRouteAlgoTask };

            await Task.Run(() => { Task.WaitAll(tasks); });

            ts.Dispose();
            cancellationTokenSource = null;

            string completionText = "Calculation finished.";
            if (!isCanceled)
            {
                GenerateOutputRouteAlgo(fastestRouteAlgo, "Fastest route takes: ", "minutes",fastestPathOutput);
                GenerateOutputRouteAlgo(cheapestRouteAlgo, "Cheapest route costs: ", "rubles", cheapestPathOutput);
            }
            else
            {
                completionText = "Canceled.";
            }

            infoOutput.Text = completionText;
            chkBox.IsChecked = true;
        }

        private void GenerateOutputRouteAlgo(RoutePathAlgorithmBase algo, string description, string mesureName, TextBlock outputContainer)
        {
            double distance = 0;
            algo?.TryGetDistanceToFinish(out distance);

            StringBuilder outputString = new StringBuilder();
            outputString.Append(description).Append(distance).Append($" {mesureName}.\n");

            List<Tuple<int, int>> path = new List<Tuple<int, int>>();
            algo.TryGetPathToFinish(out path);

            outputString.Append("Route path (stop_point[routeId]): ");
            foreach (var pathPart in path)
            {
                outputString.Append(pathPart.Item1).Append("[").Append(pathPart.Item2 + 1).Append("]");
                if (path.IndexOf(pathPart) != path.Count - 1)
                {
                    outputString.Append(" -> ");
                }
            }

            outputContainer.Text = outputString.ToString();
        }

        private void ClearOutput()
        {
            fastestPathOutput.Text = string.Empty;
        }

        private void tripStartPoint_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandleTextChanged(sender, "Incorrect start point. ",
                (dataString) => { startPoint = int.Parse(dataString); },
                () => { startPoint = 0; });
        }
        private void tripFinishPoint_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandleTextChanged(sender, "Incorrect finish point. ",
                (dataString) => { finishPoint = int.Parse(dataString); },
                () => { finishPoint = 0; });
        }

        private void tripStartTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            HandleTextChanged(sender, "Incorrect start time. ",
                (dataString) =>
                {
                    startTime = TimeSpan.Parse(dataString);
                    if (startTime > TimeSpan.FromDays(1))
                    {
                        throw new Exception();
                    }
                }, () => { startTime = TimeSpan.Zero; });
        }

        private void HandleTextChanged(object sender, string errorMessageString, Action<string> initAction, Action defaultSetter)
        {
            var textBox = sender as TextBox;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                defaultSetter?.Invoke();
                return;
            }
            try
            {
                initAction?.Invoke(textBox.Text);
                
                if (infoOutput.Text.Contains(errorMessageString))
                {
                    infoOutput.Text = infoOutput.Text.Remove(infoOutput.Text.IndexOf(errorMessageString), errorMessageString.Length);
                }
            }
            catch (Exception)
            {
                startTime = TimeSpan.Zero;
                if (!infoOutput.Text.Contains(errorMessageString))
                    infoOutput.Text += errorMessageString;
            }
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;
        }

        private void tripStartTime_LostFocus(object sender, RoutedEventArgs e)
        {
            tripStartTime.Text = startTime.ToString();
        }
    }
}
