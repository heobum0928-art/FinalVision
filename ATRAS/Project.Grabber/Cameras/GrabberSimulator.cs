using Project.BaseLib.DataStructures;
using Project.BaseLib.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Grabber
{
    public class GrabberSimulator : GrabberBase
    {
        #region fields

        protected object obj_lock = new object();

        protected int [] sim_grab_index;

        protected int[] sim_grab_image_total_count;
        #endregion

        #region propertise


        #endregion

        #region methods

        protected void InitializeByteImage()
        {
            sim_grab_index = new int[CameraCount];
            sim_grab_image_total_count = new int[CameraCount];
            for (int i = 0; i < CameraCount; i++)
            {
                sim_grab_index[i] = 0;
                string root_path = string.Format("{0}\\{1}",
                    CameraInfos[i].VirtualGrabPath, i);

                if (!Directory.Exists(root_path))
                    Directory.CreateDirectory(root_path);

                var folders = Directory.GetDirectories(root_path);
                sim_grab_image_total_count[i] = folders.Length;
            }
        }

        public override bool Close()
        {
            AppLogger.Info()("[{0}] grabber closed", Name);
            return true;
        }
        public override bool GrabOnce(int camera_idx)
        {


            int frame_count  = CameraInfos[camera_idx].FrameBufferCount;

            OnPreGrabed(camera_idx, GrabTypes.OnceGrab);

            string root_path = string.Format("{0}\\{1}", 
                CameraInfos[camera_idx].VirtualGrabPath, camera_idx);

            int img_idx = sim_grab_index[camera_idx] % sim_grab_image_total_count[camera_idx];

            root_path = string.Format("{0}\\{1}", root_path, img_idx);

            AppLogger.Info()("{0} root image loaded.", root_path);

            int buffer_index;

            for (buffer_index = 0; buffer_index < frame_count; buffer_index++)
            {
                string path = string.Format("{0}\\{1}.bmp", root_path, buffer_index);

                ByteImage image = new ByteImage();  //CameraInfos[camera_idx].Image.Load(path);
                var loaded = image.Load(path);



                OnBufferGrabDone(camera_idx, buffer_index, image);                
            }

            OnGrabComplete(camera_idx);

            lock(obj_lock)
            {
                sim_grab_index[camera_idx]++;
            }
            return true;
  
        }
        public override bool GrabStart(int camera_idx)
        {


            int frame_count = CameraInfos[camera_idx].FrameBufferCount;

            OnPreGrabed(camera_idx, GrabTypes.OnceGrab);

            string root_path = string.Format("{0}\\{1}",
                CameraInfos[camera_idx].VirtualGrabPath, camera_idx);

            int img_idx = sim_grab_index[camera_idx] % sim_grab_image_total_count[camera_idx];

            root_path = string.Format("{0}\\{1}", root_path, img_idx);

            AppLogger.Info()("{0} root image loaded.", root_path);

            int buffer_index;

            for (buffer_index = 0; buffer_index < frame_count; buffer_index++)
            {
                string path = string.Format("{0}\\{1}.bmp", root_path, buffer_index);

                ByteImage image = new ByteImage();  //CameraInfos[camera_idx].Image.Load(path);
                var loaded = image.Load(path);



                OnBufferGrabDone(camera_idx, buffer_index, image);
            }

            OnGrabComplete(camera_idx);

            lock (obj_lock)
            {
                sim_grab_index[camera_idx]++;
            }
            return true;
        }
        public override bool GrabStop(int camera_idx)
        {
            AppLogger.Info()("GrabStop [{0}] is empty.", camera_idx);
            return true;
        }
        public override bool Open()
        {
            InitializeByteImage();
            return true;
        }
        public override bool Reset()
        {
            AppLogger.Info()("Reset() is empty.");
            return true;
        }
        #endregion

        #region constructors
        public GrabberSimulator()
            : base(GrabberTypes.Simulator)
        {

        }
        #endregion


    }
}
