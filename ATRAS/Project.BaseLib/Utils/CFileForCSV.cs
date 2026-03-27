using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.BaseLib.Utils
{
    public class CFileForCSV
    {
        #region fields
        protected string fPath;
        
        protected string [] headers;

        #endregion

        #region propertise


        #endregion

        #region methods
        public bool SaveData(string [] datas)
        {

            return true;
        }

        public async Task<bool> SaveDataAsync(string [] datas)
        {
            return await Task.Run(() =>
            {
                return SaveData(datas);
            });
        }

        #endregion

        #region constructors
        public CFileForCSV()
            : this(null, null)
        {

        }

        public CFileForCSV(string fPath, string [] headers)
        {
            this.fPath = fPath;
            this.headers = headers;
        }
        #endregion


    }
}
