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

        Microsoft.Win32.OpenFileDialog wybierzPlikDialog = new Microsoft.Win32.OpenFileDialog();
        private string _wybranyPlik = string.Empty;

        public string WybranyPlik
        {
            get
            {
                if (_wybranyPlik == string.Empty)
                    return "Nie wybrano pliku.";
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
                if (OutputFileNames is not null)
                    return OutputFileNames[pdfViewer.CurrentPageIndex - 1];
                else
                    return string.Empty;
            }
            set
            {
                if (OutputFileNames is not null)
                    OutputFileNames[pdfViewer.CurrentPageIndex - 1] = value;
            }
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
        }

        private void WybierzPlikBtn_Click(object sender, RoutedEventArgs e)
        {
            WybranyPlik = String.Empty;
            wybierzPlikDialog.Filter = "Plik PDF|*.pdf";
            wybierzPlikDialog.RestoreDirectory = true;
            wybierzPlikDialog.ShowDialog();
            WybranyPlik = wybierzPlikDialog.FileName;
            pdfViewer.Load(WybranyPlik);
            OutputFileNames = new();
            for (int i = 0; i < pdfViewer.PageCount; i++)
            {
                OutputFileNames.Add(string.Empty);
            }
            OnPropertyChanged(nameof(CurrentOutputFileName));
        }

        private void pdfViewer_CurrentPageChanged(object sender, EventArgs args)
        {
            PageNumber.Text = pdfViewer.CurrentPageIndex.ToString();
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                pdfViewer.GoToNextPage();
            }
            else
            {
                pdfViewer.GoToPreviousPage();
            }
            OnPropertyChanged(nameof(CurrentOutputFileName));
        }
    }
}
