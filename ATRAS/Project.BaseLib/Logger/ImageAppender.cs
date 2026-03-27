using log4net.Appender;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Media.Imaging;

namespace Project.BaseLib.Logger
{
    public class ImageAppender : AppenderSkeleton
    {
        private String path;
        public String Path
        {
            get { return path; }
            set
            {
                path = value;
                Directory.CreateDirectory(path);
            }
        }

        private TiffCompressOption compression = TiffCompressOption.None;
        public TiffCompressOption Compression
        {
            get { return compression; }
            set { compression = value; }
        }

        private float resizeFactor = 1.0f;
        public float ResizeFactor
        {
            get { return resizeFactor; }
            set { resizeFactor = value; }
        }


        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            //SaveImageData saveData = (loggingEvent.MessageObject as MessageLog).MessageObject as SaveImageData;
            //if (saveData != null)
            //{
            //    saveData.Save(Path, Compression, ResizeFactor);
            //}
        }
    }
}
