﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Desktop_Sharing_Shared
{
    public class ScreenCapture
    {
        // P/Invoke declarations
        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);
        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr ptr);

        private ImageCodecInfo _jgpEncoder;
        private EncoderParameters _myEncoderParameters;

        public ImageCodecInfo jgpEncoder { get { return _jgpEncoder; } }
        public EncoderParameters EncoderParameters { get { return _myEncoderParameters; } }

        public ScreenCapture(long jpgquality=60L)
        {
            _jgpEncoder = GetEncoder(ImageFormat.Jpeg);
            var myEncoder = System.Drawing.Imaging.Encoder.Quality;
            var myEncoderParameter = new EncoderParameter(myEncoder, jpgquality);
            _myEncoderParameters = new EncoderParameters(1);
            _myEncoderParameters.Param[0] = myEncoderParameter;
        }
        public Bitmap GetScreen(Size sz)
        {

            //Bitmap bmpScreenCapture = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
            //                                  Screen.PrimaryScreen.Bounds.Height);

            //    using(Graphics g = Graphics.FromImage(bmpScreenCapture))
            //    {
            //        g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X,
            //                         Screen.PrimaryScreen.Bounds.Y,
            //                         0, 0,
            //                         bmpScreenCapture.Size,
            //                         CopyPixelOperation.SourceCopy);
            //    }
            //    return bmpScreenCapture;


            IntPtr hDesk, hSrce, hDest, hBmp, hOldBmp;
            hDesk = hSrce = hDest = hBmp = hOldBmp = IntPtr.Zero;
            try
            {
                hDesk = GetDesktopWindow();
                hSrce = GetWindowDC(hDesk);
                hDest = CreateCompatibleDC(hSrce);
                hBmp = CreateCompatibleBitmap(hSrce, sz.Width, sz.Height);
                hOldBmp = SelectObject(hDest, hBmp);
                bool b = BitBlt(hDest, 0, 0, sz.Width, sz.Height, hSrce, 0, 0, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
                Bitmap bmp = Bitmap.FromHbitmap(hBmp);
                SelectObject(hDest, hOldBmp);
                return bmp;
            } catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            } finally
            {
                if(hBmp != IntPtr.Zero)
                    DeleteObject(hBmp);
                if(hDest != IntPtr.Zero)
                    DeleteDC(hDest);
                ReleaseDC(hDesk, hSrce);
            }
            return new Bitmap(sz.Height, sz.Width);
        }
        private ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach(ImageCodecInfo codec in codecs)
            {
                if(codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

    }
}
