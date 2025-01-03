using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WebAccess;

namespace WebAccessDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Url
        {
            get
            {
                return this._url;
            }
            set
            {
                if (this._url != value)
                {
                    this._url = value;
                    OnPropertyChanged(nameof(Url));
                }
            }
        }

        public string TargetDirectory
        {
            get
            {
                return _targetDirectory;
            }
            set
            {
                if (this._targetDirectory != value)
                {
                    this._targetDirectory = value;
                    OnPropertyChanged(nameof(TargetDirectory));
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Url = "https://neteti.de/Vishnu.doc.de/Vishnu_doc.de.chm";
            this.TargetDirectory = Path.Combine(Path.GetTempPath(), "Vishnu_doc.de.chm");
            this._isDownloadPossible = true;
        }

        public bool IsDownloadPossible
        {
            get { return this._isDownloadPossible; }
            set
            {
                this._isDownloadPossible = value;
            }
        }

        private bool _isDownloadPossible;
        private string _url = "";
        private string _targetDirectory = "";

        private void CmdBtnDownload_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.IsDownloadPossible;
        }

        private async void CmdBtnDownload_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.lblProgress.Content = "Operation gestartet.";
            this.pbProgress.Value = 0;
            this.IsDownloadPossible = false;
            await Task.Delay(500);

            using (WebOperator webOperator = new WebOperator())
            {
                var progress = new Progress<int>(wert =>
                {
                    this.pbProgress.Value = wert;
                    {
                        this.percentageText.Text = $"{wert}%";
                    }
                    if (wert >= 100)
                    {
                        this.lblProgress.Content = "Operation abgeschlossen.";
                    }
                    // CommandManager.InvalidateRequerySuggested(); // erzwingt die Neuberechnung der CanExecute-Methoden
                });
                await webOperator.DownloadFileAsync(this.Url, this.TargetDirectory, progress); // blockiert den UI-Thread nicht
                await Task.Delay(1000);
                this.lblProgress.Content = "Web-Datei-Download";
                this.IsDownloadPossible = true;
                CommandManager.InvalidateRequerySuggested(); // erzwingt die Neuberechnung der CanExecute-Methoden
                Exception? ex = webOperator.GetAndResetTaskException();
                if (ex != null)
                {
                    MessageBox.Show(ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CmdBtnSearch_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true; // immer möglich, da ohnehin modal
        }

        private void CmdBtnSearch_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                TargetDirectory = dialog.FolderName;
            }
        }
    }
}