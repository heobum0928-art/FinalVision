using Project.BaseLib.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Grabber
{
    public delegate void GrabberOpened();
    public delegate void GrabberClosed();
    public delegate void PreGrabEvent(int camera_index, GrabTypes grab_type);

    public delegate void GrabDoneEvent(int camera_index, ByteImage image);    
    public delegate void GrabAllDoneEvent();

    public delegate void BufferGrabDoneEvent(int camera_index, int buffer_index, ByteImage image);
    public delegate void GrabCompleteEvent(int camera_index);
    //public delegate void BufferMergeCompleteEvent(int camera_index, ByteImage merged_image);

}
