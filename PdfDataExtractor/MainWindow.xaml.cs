using CefSharp.Internals;
using PdfDataExtractor.PdfUtils;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Logging;

namespace PdfDataExtractor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //constants
        int BORDERMARGIN = 30;


        //Localvariables.
        string PDFFileName = string.Empty;
        double scaleX = 1;
        double scaleY = 1;
        bool drawMode = true;


        public MainWindow()
        {
            InitializeComponent();

        }

        private void UpdateUI()
        {
            double FullWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double FullHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            this.Width = FullWidth;
            this.Height = FullHeight;

            //Command and Control GroupBox Position Layout. Group box line has the GRP Margins.
            //Compensate the same while adjusting the Controls.MK
            grp_commandAndControl.Width = (this.Width / 2) - BORDERMARGIN;
            grp_commandAndControl.Height = this.Height - (BORDERMARGIN * 4);

            Canvas.SetTop(grp_commandAndControl, BORDERMARGIN);
            Canvas.SetLeft(grp_commandAndControl, BORDERMARGIN);


            //Pdf Viwer GroupBox Position Layout. Group box line has the GRP Margins.
            //Compensate the same while adjusting the Controls.MK
            grp_PdfViewer.Width = (this.Width / 2) - BORDERMARGIN;
            grp_PdfViewer.Height = this.Height - (BORDERMARGIN * 4);

            Canvas.SetTop(grp_PdfViewer, BORDERMARGIN);
            Canvas.SetLeft(grp_PdfViewer, this.Width / 2);


            //Adjust Controls for PDfViewer.
            this.pdf_canvas.Width = grp_PdfViewer.Width;
            this.pdf_canvas.Height = grp_PdfViewer.Height;

            this.pdfviewer.Width = pdf_canvas.Width;
            this.pdfviewer.Height = pdf_canvas.Height;


        }

        private void can_CommandAndControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            UpdateUI();
        }

        private void btn_LoadPdf_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Pdf |*.pdf";

            if (dialog.ShowDialog() == true)
            {
                pdfviewer.Address = dialog.FileName;
                PDFFileName = dialog.FileName;
                PdfWrapper pdfWrapper = new PdfWrapper();
                scaleX = pdfWrapper.GetXdimensions(PDFFileName) / pdf_canvas.Width;
                scaleY = pdfWrapper.GetYdimensions(PDFFileName) / (pdf_canvas.Height - 120);

                AdjustLayoutForPdf(PDFFileName);


            }
        }

        private void AdjustLayoutForPdf(string PDFFileName)
        {

            PdfWrapper pdfWrapper = new PdfWrapper();
            double PDFWidth = pdfWrapper.GetXdimensions(PDFFileName);
            double PDFHeight = pdfWrapper.GetYdimensions(PDFFileName);

            double PDFAspectRatioWidth = pdfviewer.Width / PDFWidth;
            double PDFAspectRatioHeight = pdfviewer.Height / PDFHeight;

            double ScaleFactor = 1;
            if (PDFAspectRatioWidth < PDFAspectRatioHeight)
            {
                //Set the Height : KMSR
                pdfviewer.Height = pdfviewer.Width * PDFHeight / PDFWidth;
                pdfdraw_canvas.Height = pdfviewer.Height;


            }
            else
            {
                //Set the Width  : KMSR
                pdfviewer.Width = pdfviewer.Height * PDFWidth / PDFHeight;
                pdfdraw_canvas.Width = pdfviewer.Width;
            }


            //Set the dimensions of the PDF Container as per the PDF Size  : KMSR
            pdfdraw_canvas.Width = pdfviewer.Width;
            pdfdraw_canvas.Height = pdfviewer.Height;
            pdfdraw_canvas.Margin = pdfviewer.Margin;


            //Scale Factor to convert from control locatin to PDf Location : KMSR
            scaleX = PDFWidth / pdfdraw_canvas.Width;
            scaleY = PDFHeight / (pdfdraw_canvas.Height - 60);



        }

        private void btn_enabledraw_Click(object sender, RoutedEventArgs e)
        {
            if (drawMode)
            {
                drawMode = false;
                btn_enabledraw.Content = "Enable Draw";
                pdfdraw_canvas.Visibility = Visibility.Hidden;

            }
            else
            {
                drawMode = true;
                btn_enabledraw.Content = "Disable Draw";
                pdfdraw_canvas.Visibility = Visibility.Visible;
            }
        }

        private System.Windows.Point mouseLeftDownPoint;
        private void pdfdraw_canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!pdfdraw_canvas.IsMouseCaptured && drawMode)
            {
                mouseLeftDownPoint = e.GetPosition(pdfdraw_canvas);
                pdfdraw_canvas.CaptureMouse();
            }
        }

        private System.Windows.Shapes.Rectangle rubberBand = null;
        private void pdfdraw_canvas_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point currentPoint = e.GetPosition(pdfdraw_canvas);

            currentPoint = new System.Windows.Point(currentPoint.X, currentPoint.Y);

            if (pdfdraw_canvas.IsMouseCaptured)
            {


                if (rubberBand == null)
                {
                    rubberBand = new System.Windows.Shapes.Rectangle();
                    rubberBand.Stroke = new SolidColorBrush(Colors.Red);
                    pdfdraw_canvas.Children.Add(rubberBand);
                }

                double width = Math.Abs(mouseLeftDownPoint.X - currentPoint.X);
                double height = Math.Abs(mouseLeftDownPoint.Y - currentPoint.Y);
                double left = Math.Min(mouseLeftDownPoint.X, currentPoint.X);
                double top = Math.Min(mouseLeftDownPoint.Y, currentPoint.Y);

                rubberBand.Width = width;
                rubberBand.Height = height;

                rectangle.Width = (int)rubberBand.Width;
                rectangle.Height = (int)rubberBand.Height;
                rectangle.X = (int)left;
                rectangle.Y = (int)top;

                Canvas.SetLeft(rubberBand, left);
                Canvas.SetTop(rubberBand, top);
            }
        }

        System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle();
        private void pdfdraw_canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (pdfdraw_canvas.IsMouseCaptured && rubberBand != null)
            {
                pdfdraw_canvas.ReleaseMouseCapture();

                pdfdraw_canvas.Children.Clear();

                if (rubberBand != null)
                {
                    rectangle.Width = (int)rubberBand.Width;
                    rectangle.Height = (int)rubberBand.Height;
                    rubberBand = null;
                }


            }

            rectangle.Y = (int)(pdfdraw_canvas.Height - rectangle.Y);


            rectangle.X = (int)((double)rectangle.X * scaleX);
            rectangle.Width = (int)((double)rectangle.Width * scaleX);
            rectangle.Y = (int)((double)rectangle.Y * scaleY);
            rectangle.Height = (int)((double)rectangle.Height * scaleY);




            PdfWrapper pdfWrapper = new PdfWrapper();

            //Locations are availble. Get the text from PDF from this location.

            if (rb_textselected.IsChecked.Value)
            {
                List<string> textsfound = pdfWrapper.GetTextsFromLocation(PDFFileName, new PdfRectangle(rectangle.X, rectangle.Y, rectangle.X + rectangle.Width, rectangle.Y - rectangle.Height));
                string concatstring = string.Empty;

                foreach (string text in textsfound)
                {

                    concatstring += text;
                }

                MessageBox.Show(concatstring);
            }
            else
            {

                img_selectedfromPDF.Source = pdfWrapper.GetAllImageLocationsFromLocation(PDFFileName, new PdfRectangle(rectangle.X, rectangle.Y, rectangle.X + rectangle.Width, rectangle.Y - rectangle.Height));

                //if (Imagefound != string.Empty)
                //{
                //    img_selectedfromPDF.Source = new BitmapImage(new Uri(Imagefound));

                //}

                //pdfWrapper.ExtractAllImages(PDFFileName);
            }
        }
    }
}