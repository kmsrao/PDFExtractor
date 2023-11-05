using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.XObjects;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Geometry;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace PdfDataExtractor.PdfUtils
{
    public class PdfWrapper
    {
        public void ExtractAllImages(string PdfFile)
        {

            using (var doc = PdfDocument.Open(PdfFile))
            {
                ImageIndex = 0;
                foreach (var page in doc.GetPages())
                {
                    ExtractAllImagesFromPage(page);


                }
            }

        }

        int ImageIndex = 0;
        private void ExtractAllImagesFromPage(UglyToad.PdfPig.Content.Page page)
        {
            
            foreach (XObjectImage image in page.GetImages())
            {
                var type = string.Empty;

                switch (image)
                {
                    case XObjectImage ximg:
                        type = "XObject";
                        break;
                    //case InlineImage inline:
                    //    type = "Inline";
                    //    break;
                }

                byte[] pngImage;

                bool extractionSuccessful = image.TryGetPng(out pngImage);

                if (extractionSuccessful)
                {
                    File.WriteAllBytes("D:\\Sample" + ImageIndex.ToString() + ".png", pngImage);
                    ImageIndex++;
                }

            }
        }


        public List<PdfRectangle> GetAllImagesLocations(string PdfFile)
        {

            List<PdfRectangle> imagelocations = new List<PdfRectangle>();

            using (var doc = PdfDocument.Open(PdfFile))
            {
                foreach (var page in doc.GetPages())
                {
                    imagelocations.AddRange(GetAllImageLocationsFromPage(page));
                }
            }

            return imagelocations;
        }

        public BitmapSource GetAllImageLocationsFromLocation(string PdfFile,PdfRectangle location)
        {

           
            using (var doc = PdfDocument.Open(PdfFile))
            {
                foreach (var page in doc.GetPages())
                {
                    return GetAllImageLocationsFromLocation(page, location);
                }
            }

            return null;
        }
        
        private List<PdfRectangle> GetAllImageLocationsFromPage(UglyToad.PdfPig.Content.Page page)
        {

            List<PdfRectangle> imagelocations = new List<PdfRectangle>();

            foreach (var image in page.GetImages())
            {
                PdfRectangle rectangle = image.Bounds;

            }

            return imagelocations;
        }


        private BitmapSource  GetAllImageLocationsFromLocation(UglyToad.PdfPig.Content.Page page,PdfRectangle pdfRectangle)
        {
            string SamplePNGFile = "D:\\Sample.png";

         
            if (File.Exists(SamplePNGFile))
            {
                File.Delete(SamplePNGFile);
            }

            List<PdfRectangle> imagelocations = new List<PdfRectangle>();


            foreach (var image in page.GetImages())
            {
                PdfRectangle rectangle = image.Bounds;

                if(pdfRectangle.Contains(rectangle.Centroid))
                {
                    return TryGetImage(image);

                    //byte[] pngImage;

                    
                    //bool extractionSuccessful = image.TryGetPng(out pngImage);

                    //if (extractionSuccessful)
                    //{
                    //    File.WriteAllBytes(SamplePNGFile, pngImage);



                    //    return SamplePNGFile;
                    //}
                }

            }
            return null;
            
        }

        public List<string> GetTextsFromLocation(string PdfFile, PdfRectangle rectangle)
        {

            List<string> wordsinRect = new List<string>();

            using (var doc = PdfDocument.Open(PdfFile))
            {
                foreach (var page in doc.GetPages())
                {

                    wordsinRect.AddRange(GetTextFromLocationPage(page, rectangle));
                    break;

                }
            }

            return wordsinRect;
        }



        public List<string> GetTextFromLocationPage(UglyToad.PdfPig.Content.Page page, PdfRectangle rectangle)
        {

            List<string> TextinLocation = new List<string>();

            List<Word> wordsinPage = page.GetWords().ToList();


            foreach (var Word in wordsinPage)
            {
                if (rectangle.Contains(Word.BoundingBox.Centroid))
                {
                    TextinLocation.Add(Word.Text);
                }

            }
            return TextinLocation;
        }

        public List<string> GetTextFromLocationPage(string PdfFile, int pageNumber, PdfRectangle rectangle)
        {
            List<string> TextinLocation = new List<string>();

            using (var doc = PdfDocument.Open(PdfFile))
            {
                var page = doc.GetPages().ToList()[pageNumber];

                List<Word> wordsinPage = page.GetWords().ToList();

                foreach (var Word in wordsinPage)
                {
                    if (rectangle.Contains(Word.BoundingBox.Centroid))
                    {
                        TextinLocation.Add(Word.Text);
                    }


                }
            }
            return TextinLocation;
        }


        public List<string> GetAllTexts(string PdfFile)
        {

            List<string> wordsinRect = new List<string>();

            using (var doc = PdfDocument.Open(PdfFile))
            {
                foreach (var page in doc.GetPages())
                {

                    wordsinRect.AddRange(GetTextFromPage(page));
                    break;

                }
            }

            return wordsinRect;
        }
        private List<string> GetTextFromPage(UglyToad.PdfPig.Content.Page page)
        {

            List<string> TextinLocation = new List<string>();

            List<Word> wordsinPage = page.GetWords().ToList();

            foreach (var Word in wordsinPage)
            {
                TextinLocation.Add(Word.Text);
            }

            return TextinLocation;
        }

        public List<Word> GetInteractiveAllTexts(string PdfFile)
        {

            List<Word> wordsinRect = new List<Word>();

            using (var doc = PdfDocument.Open(PdfFile))
            {
                foreach (var page in doc.GetPages())
                {

                    wordsinRect.AddRange(GetInteractiveTextFromPage(page));
                    break;

                }
            }

            return wordsinRect;
        }

        public double GetXdimensions(string PdfFile)
        {

            using (var doc = PdfDocument.Open(PdfFile))
            {
                foreach (var page in doc.GetPages())
                {

                    return page.Width;

                }
            }

            return 0;
        }

        public double GetYdimensions(string PdfFile)
        {

            using (var doc = PdfDocument.Open(PdfFile))
            {
                foreach (var page in doc.GetPages())
                {

                    return page.Height;

                }
            }

            return 0;
        }
        private List<Word> GetInteractiveTextFromPage(UglyToad.PdfPig.Content.Page page)
        {

            StreamWriter sw = new StreamWriter("D:\\position.txt");


            List<Word> wordsinPage = page.GetWords().ToList();



            foreach (var Word in wordsinPage)
            {

                sw.WriteLine(Word.Text + ":: X - " + Word.BoundingBox.TopLeft.X.ToString() + " :: Y- " + Word.BoundingBox.TopLeft.Y.ToString());

                //if (Word.BoundingBox.Contains(new PdfPoint(location.X+location.Width/2 ,location.Y+ location.Height/2)))
                //{
                //    TextinLocation.Add(Word.Text);
                //}

            }
            sw.Close();


            return page.GetWords().ToList();

        }

        private static BitmapSource TryGetImage(IPdfImage image)
        {
            BitmapSource bmp;
            byte[] bytes;
            if (image.TryGetPng(out bytes))
            {
                bmp = (BitmapSource)new ImageSourceConverter().ConvertFrom(bytes);

            }
            else
            {
                IReadOnlyList<byte> iroBytes;
                if (image.TryGetBytes(out iroBytes))
                {
                    bmp = (BitmapSource)new ImageSourceConverter().ConvertFrom(bytes);
                }
                else
                {
                    var rawB = image.RawBytes.ToArray<Byte>();

                    
                    using (var ms = new MemoryStream(rawB))
                    {
                       var nbmp = new BitmapImage();
                        nbmp.BeginInit();
                        nbmp.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        nbmp.CacheOption = BitmapCacheOption.OnLoad;
                        nbmp.StreamSource = ms;
                        nbmp.EndInit();

                        return nbmp;


                    }
                   
                   
                }
            }
            return bmp;
        }

        //public static BitmapSource ConvertBmpToBmpSource(BitmapImage bitmap)
        //{
        //    var bitmapData = bitmap.LockBits(
        //        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
        //        System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

        //    var bitmapSource = BitmapSource.Create(
        //        bitmapData.Width, bitmapData.Height,
        //        bitmap.HorizontalResolution, bitmap.VerticalResolution,
        //        PixelFormats.Bgr24, null,
        //        bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

        //    bitmap.UnlockBits(bitmapData);

        //    return bitmapSource;
        //}



    }

}
