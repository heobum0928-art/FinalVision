using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using log4net.Core;

namespace log4net.Appender
{
  public class CompresssionRollingFileAppender : RollingFileAppender
  {
    private readonly static Type declaringType = typeof(CompresssionRollingFileAppender);

    private List<FileInfo> alCompressedFiles = new List<FileInfo>();

    private int m_maxSizeCompressedBackups = 0;

    public int MaxSizeCompressedBackups
    {
      get { return m_maxSizeCompressedBackups; }
      set { m_maxSizeCompressedBackups = value; }
    }

    private int m_keepCompressedDays = 0;

    public int KeepCompressedDays
    {
      get { return m_keepCompressedDays; }
      set { m_keepCompressedDays = value; }
    }

    private Timer deleteBackupsTimer;

    public override void ActivateOptions()
    {
      base.ActivateOptions();

      log4net.Util.LogLog.Error(declaringType, "CompresssionRollingFileAppender");

      string directory = null;

      using (SecurityContext.Impersonate(this))
      {
        string fullPath = Path.GetFullPath(File);

        directory = Path.Combine(Path.GetDirectoryName(fullPath), "Backup");
        if (!Directory.Exists(directory))
        {
          Directory.CreateDirectory(directory);
        }

        if (Directory.Exists(directory))
        {
          string baseFileName = Path.GetFileNameWithoutExtension(fullPath);

          var files = new DirectoryInfo(directory).GetFiles(baseFileName + "*.zip", SearchOption.TopDirectoryOnly).OrderByDescending(f => f.LastWriteTime).ToList();

          while (files.Count() > m_maxSizeCompressedBackups)
          {
            DeleteFile(files[0].FullName);
            files.RemoveAt(0);
          }

          alCompressedFiles = files;
          if (m_keepCompressedDays > 0)
          {
            TimerCallback cb = new TimerCallback(DeleteOldCompressedFilesCallBack);
            deleteBackupsTimer = new Timer(cb, null, new TimeSpan(0, 0, 0), TimeSpan.FromHours(1));
          }
        }
      }
    }
    protected override void OnClose()
    {
      base.OnClose();

      if (deleteBackupsTimer != null)
      {
        deleteBackupsTimer.Dispose();
        deleteBackupsTimer = null;
      }
    }

    private void DeleteOldCompressedFilesCallBack(object state)
    {
      if (deleteBackupsTimer != null)
      {
        if (m_keepCompressedDays > 0)
        {
          string directory = Path.Combine(Path.GetDirectoryName(File), "Backup");
          string baseFileName = Path.GetFileNameWithoutExtension(File);

          var files = new DirectoryInfo(directory).GetFiles(baseFileName + "*.zip", SearchOption.TopDirectoryOnly).OrderByDescending(f => f.LastWriteTime).ToList();

          foreach (var file in files)
          {
            if ((DateTime.Now - file.LastWriteTime).Days > m_keepCompressedDays)
            {
              DeleteFile(file.FullName);
            }
          }
        }
      }
    }
    protected override void Append(LoggingEvent loggingEvent)
    {
      if (m_maxSizeCompressedBackups > 0 && IfRollOverSize())
      {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        if (IfDeleteLastFile())
        {
          string lastFile = CombinePath(File, "." + MaxSizeRollBackups);

          if (FileExists(lastFile))
          {
            string zipFile = Path.Combine(Path.GetDirectoryName(File), "Backup", Path.GetFileNameWithoutExtension(File) + "_" + System.IO.File.GetLastWriteTime(lastFile).ToString("dd-MM-yyyy_hh-mm-ss") + ".zip");

            int i = 0;
            while (FileExists(zipFile))
            {
              zipFile = Path.Combine(Path.GetDirectoryName(File), "Backup", Path.GetFileNameWithoutExtension(File) + "_" + System.IO.File.GetLastWriteTime(lastFile).ToString("dd-MM-yyyy_hh-mm-ss") + String.Format(".{0}.zip", ++i));
            }

            RollFile(lastFile, zipFile + ".tmp");

            string filePathAndName = Path.GetFileName(File);

            new Thread(() =>
            {
              try
              {
                using (ZipArchive zip = ZipFile.Open(zipFile, ZipArchiveMode.Create))
                {
                  if (FileExists(zipFile + ".tmp"))
                  {
                    zip.CreateEntryFromFile(zipFile + ".tmp", filePathAndName, CompressionLevel.Optimal);
                  }
                  else
                  {
                    log4net.Util.LogLog.Warn(declaringType, "CAnnot Backup File [" + zipFile + ".tmp" + "]. It does not exist");
                  }
                }

                DeleteFile(zipFile + ".tmp");

                alCompressedFiles.Add(new FileInfo(zipFile));
                if (alCompressedFiles.Count > m_maxSizeCompressedBackups)
                {
                  DeleteFile(alCompressedFiles[0].FullName);
                  alCompressedFiles.RemoveAt(0);
                }
              }
              catch (Exception ex)
              {
                log4net.Util.LogLog.Error(declaringType, "Exception in CompresssionRollingFileAppender ", ex);
                log4net.Util.LogLog.Error(declaringType, "Exception in CompresssionRolingFileAppender " + ex.Message);
                log4net.Util.LogLog.Error(declaringType, String.Format("Exception in CompresssionRollingFileAppender zipfile {0} filePathAndName {1}", zipFile, filePathAndName));

              }
            }) { IsBackground = true }.Start();
          }
          else
          {
            log4net.Util.LogLog.Warn(declaringType, "Cannot RollFile [" + lastFile + "]. It does not exist");
          }
        }
        log4net.Util.LogLog.Debug(declaringType, "time" + sw.ElapsedMilliseconds + " thread " + Thread.CurrentThread.ManagedThreadId);
      }
      base.Append(loggingEvent);
    }
  }
}
