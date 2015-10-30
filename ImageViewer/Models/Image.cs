﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Threading.Tasks;
using System.IO;
using Livet;
using System.Drawing.Imaging;
using ImageViewer.Infrastructures;

namespace ImageViewer.Models
{
    public class Image : NotificationObject
    {
        public byte[] OriginalData { get; private set; }
        public string FileExtension { get; private set; }

        ~Image()
        {
            OriginalData = null;
            Bitmap = null;
        }

        #region Bitmap変更通知プロパティ
        private BitmapSource _Bitmap;

        public BitmapSource Bitmap
        {
            get
            { return _Bitmap; }
            set
            { 
                if (_Bitmap == value)
                    return;
                _Bitmap = value;
                RaisePropertyChanged();
            }
        }
        #endregion


        public async Task<BitmapImage> DownloadDataAsync(string uri, string referer = null)
        {
            var wc = new WebClient();
            wc.Referer = referer?.ToString();
            OriginalData = await wc.DownloadDataTaskAsync(uri);

            var bi = new BitmapImage();
            using (var ms = new MemoryStream(OriginalData))
            {
                using (var b = new Bitmap(ms))
                {
                    var decoders = ImageCodecInfo.GetImageDecoders();
                    foreach (var ici in decoders)
                    {
                        if (ici.FormatID == b.RawFormat.Guid)
                        {
                            FileExtension = ici.FilenameExtension.Split(';')[0].ToLower();
                        }
                    }
                }
                ms.Seek(0, SeekOrigin.Begin);

                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();
            }
            Bitmap = bi;

            return bi;
        }

        public BitmapImage SetData(byte[] data)
        {
            OriginalData = data;

            var bi = new BitmapImage();
            using (var ms = new MemoryStream(data))
            {
                using (var b = new Bitmap(ms))
                {
                    var decoders = ImageCodecInfo.GetImageDecoders();
                    foreach (var ici in decoders)
                    {
                        if (ici.FormatID == b.RawFormat.Guid)
                        {
                            FileExtension = ici.FilenameExtension.Split(';')[0].ToLower();
                        }
                    }
                }
                ms.Seek(0, SeekOrigin.Begin);

                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();
            }
            Bitmap = bi;

            return bi;
        }

        public void Save(string targetPath)
        {
            using (var fs = new FileStream(targetPath, FileMode.Create))
            {
                fs.Write(OriginalData, 0, OriginalData.Length);
            }
        }
    }
}
