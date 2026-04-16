using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace FinalVisionProject.Utility {
    //260409 hbk -- 크래시 덤프 + 예외 로그 저장 유틸리티
    public static class CrashDumpHelper {
        private static readonly string DumpFolder =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CrashDump");

        #region MiniDumpWriteDump P/Invoke

        [Flags]
        private enum MiniDumpType : uint {
            MiniDumpNormal = 0x00000000,
            MiniDumpWithDataSegs = 0x00000001,
            MiniDumpWithFullMemory = 0x00000002,
            MiniDumpWithHandleData = 0x00000004,
            MiniDumpWithThreadInfo = 0x00001000,
        }

        [DllImport("dbghelp.dll", SetLastError = true)]
        private static extern bool MiniDumpWriteDump(
            IntPtr hProcess, uint processId, IntPtr hFile,
            MiniDumpType dumpType, IntPtr exceptionParam,
            IntPtr userStreamParam, IntPtr callbackParam);

        #endregion

        /// <summary>
        /// .dmp 파일 + .log 텍스트를 CrashDump 폴더에 저장한다.
        /// </summary>
        public static string WriteDump(Exception ex, string source) {
            try {
                if (!Directory.Exists(DumpFolder))
                    Directory.CreateDirectory(DumpFolder);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                string baseName = $"Crash_{timestamp}";

                // 1) MiniDump (.dmp)
                string dmpPath = Path.Combine(DumpFolder, baseName + ".dmp");
                WriteMiniDump(dmpPath);

                // 2) 예외 텍스트 로그 (.log)
                string logPath = Path.Combine(DumpFolder, baseName + ".log");
                WriteCrashLog(logPath, ex, source);

                return dmpPath;
            }
            catch {
                // 덤프 저장 자체가 실패해도 앱 종료 흐름을 방해하지 않는다
                return null;
            }
        }

        private static void WriteMiniDump(string path) {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None)) {
                var process = Process.GetCurrentProcess();
                MiniDumpWriteDump(
                    process.Handle,
                    (uint)process.Id,
                    fs.SafeFileHandle.DangerousGetHandle(),
                    MiniDumpType.MiniDumpNormal | MiniDumpType.MiniDumpWithThreadInfo,
                    IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            }
        }

        private static void WriteCrashLog(string path, Exception ex, string source) {
            var sb = new StringBuilder();
            sb.AppendLine("========== FinalVision Crash Report ==========");
            sb.AppendLine($"Time    : {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            sb.AppendLine($"Source  : {source}");
            sb.AppendLine($"Machine : {Environment.MachineName}");
            sb.AppendLine($"OS      : {Environment.OSVersion}");
            sb.AppendLine($"CLR     : {Environment.Version}");
            sb.AppendLine($"64bit   : {Environment.Is64BitProcess}");
            sb.AppendLine();

            var cur = ex;
            int depth = 0;
            while (cur != null) {
                sb.AppendLine($"--- Exception [{depth}] ---");
                sb.AppendLine($"Type    : {cur.GetType().FullName}");
                sb.AppendLine($"Message : {cur.Message}");
                sb.AppendLine($"Stack   :");
                sb.AppendLine(cur.StackTrace ?? "(no stack trace)");
                sb.AppendLine();
                cur = cur.InnerException;
                depth++;
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 오래된 덤프 파일 삭제 (일수 기준)
        /// </summary>
        public static void CleanOldDumps(int keepDays = 30) {
            try {
                if (!Directory.Exists(DumpFolder)) return;
                var cutoff = DateTime.Now.AddDays(-keepDays);
                foreach (var file in Directory.GetFiles(DumpFolder)) {
                    if (File.GetCreationTime(file) < cutoff)
                        File.Delete(file);
                }
            }
            catch { }
        }
    }
}
