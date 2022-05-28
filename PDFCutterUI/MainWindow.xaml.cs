using Syncfusion.Pdf;
using Syncfusion.Windows.PdfViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PDFCutterUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //Implementacja INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _wybranyPlik = string.Empty;

        public string WybranyPlik
        {
            get
            {
                if (_wybranyPlik == string.Empty)
                    return "Wybierz plik PDF...";
                else
                    return _wybranyPlik;
            }
            set
            {
                _wybranyPlik = value;
                OnPropertyChanged(nameof(WybranyPlik));
            }
        }


        private List<string> OutputFileNames;

        public List<string> _outputFileNames
        {
            get { return OutputFileNames; }
            set { OutputFileNames = value; OnPropertyChanged(nameof(OutputFileNames)); }
        }



        public string CurrentOutputFileName
        {
            get
            {
                if (OutputFileNames is not null && OutputFileNames.Count > 0 && pdfViewer.CurrentPageIndex > 0)
                    return OutputFileNames[pdfViewer.CurrentPageIndex - 1];
                else
                    return string.Empty;
            }
            set
            {
                if (OutputFileNames is not null && OutputFileNames.Count > 0 && pdfViewer.CurrentPageIndex > 0)
                    OutputFileNames[pdfViewer.CurrentPageIndex - 1] = value;
            }
        }


        private int _progress;

        public int Progress
        {
            get { return _progress; }
            set { _progress = value; OnPropertyChanged(nameof(Progress)); }
        }


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            OutputFileNames = new();

            //Ustawienia okienka podglądu
            pdfViewer.ShowScrollbar = false;
            pdfViewer.ZoomMode = ZoomMode.FitPage;
            pdfViewer.IsEnabled = false;
            Progress = 0;
        }

        private void WybierzPlikBtn_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog wybierzPlikDialog = new();
            wybierzPlikDialog.Filter = "Plik PDF|*.pdf";
            wybierzPlikDialog.RestoreDirectory = true;
            if (wybierzPlikDialog.ShowDialog() == true)
            {
                WybranyPlik = String.Empty;
                WybranyPlik = wybierzPlikDialog.FileName;
                pdfViewer.Load(WybranyPlik);
                OutputFileNames = new();
                for (int i = 0; i < pdfViewer.PageCount; i++)
                {
                    OutputFileNames.Add(string.Empty);
                }
            }
            OnPropertyChanged(nameof(CurrentOutputFileName));
        }

        private void pdfViewer_CurrentPageChanged(object sender, EventArgs args)
        {
            PageNumber.Text = string.Format("Strona {0} z {1}", pdfViewer.CurrentPageIndex, pdfViewer.PageCount);
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                if (pdfViewer.CurrentPageIndex < pdfViewer.PageCount)
                    pdfViewer.GoToNextPage();
            }
            else
            {
                pdfViewer.GoToPreviousPage();
            }
            OnPropertyChanged(nameof(CurrentOutputFileName));
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Home)
                pdfViewer.GoToFirstPage();

            if (e.Key == Key.End)
                pdfViewer.GoToLastPage();

            if (e.Key == Key.PageDown)
                pdfViewer.GoToNextPage();

            if (e.Key == Key.PageUp)
                pdfViewer.GoToPreviousPage();

            OnPropertyChanged(nameof(CurrentOutputFileName));
        }

        private void OutputFileNameTB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                LastBtn.Focus();
            }
            
        }

        private void FirstBtn_Click(object sender, RoutedEventArgs e)
        {
            pdfViewer.GoToFirstPage();
            OnPropertyChanged(nameof(CurrentOutputFileName));
        }

        private void PreviousBtn_Click(object sender, RoutedEventArgs e)
        {
            pdfViewer.GoToPreviousPage();
            OnPropertyChanged(nameof(CurrentOutputFileName));
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (pdfViewer.CurrentPageIndex < pdfViewer.PageCount)
                pdfViewer.GoToNextPage();
            OnPropertyChanged(nameof(CurrentOutputFileName));
        }

        private void LastBtn_Click(object sender, RoutedEventArgs e)
        {
            pdfViewer.GoToLastPage();
            OnPropertyChanged(nameof(CurrentOutputFileName));
        }

        private async void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            Progress = 0;
            string? path = System.IO.Path.GetDirectoryName(WybranyPlik);

            decimal total = OutputFileNames.Where(x => string.IsNullOrWhiteSpace(x) == false).Count();
            decimal completed = 0;
            PdfDocument currentPdfDocument = new();
            string currentOutputFileName = string.Empty;
            bool fileInProgress = false;
            if (total > 0)
            {
                await Task.Run(() =>
                {
                    for (int i = 0; i < (OutputFileNames.Count); i++)
                    {                        
                        if (string.IsNullOrWhiteSpace(OutputFileNames[i]) == false)
                        {
                            if (fileInProgress)
                            {
                                try
                                {
                                    currentPdfDocument.Save(path + @"\cut\" + currentOutputFileName + ".pdf");
                                    fileInProgress = false;
                                    completed++;
                                    Progress = (int)(completed * 100 / total);
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.Message);
                                }
                            }
                            currentOutputFileName = OutputFileNames[i];
                            currentPdfDocument = new();
                            fileInProgress = true;
                            currentPdfDocument.ImportPage(pdfViewer.LoadedDocument, i);
                        }
                        else if (fileInProgress)
                        {
                            currentPdfDocument.ImportPage(pdfViewer.LoadedDocument, i);
                        }
                        
                        if (fileInProgress && i == OutputFileNames.Count - 1)
                        {
                            try
                            {
                                currentPdfDocument.Save(path + @"\cut\" + currentOutputFileName + ".pdf");
                                fileInProgress = false;
                                completed++;
                                Progress = (int)(completed * 100 / total);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.Message);
                            }
                        }
                    }
                }
                );
            }
        }
    }
}
