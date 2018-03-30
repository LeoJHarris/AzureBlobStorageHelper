using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.BlobStorageHelper
{
    /// <summary>
    /// Interface for $safeprojectgroupname$
    /// </summary>
    public class BlobStorageHelperImplementation : IBlobStorageHelper
    {
        public void DeleteFile(string filename)
        {
            throw new NotImplementedException();
        }

        public string FileExists(string filename)
        {
            throw new NotImplementedException();
        }

        public byte[] LoadFile(string filename)
        {
            throw new NotImplementedException();
        }

        public Task<string> RetrieveBlob(string linkToFile, string fileName, string connectionString)
        {
            throw new NotImplementedException();
        }

        public string SaveFile(string filename, Stream stream)
        {
            throw new NotImplementedException();
        }

        public Task<Uri> UploadBlob(Guid containerName, Guid fileName, string connectionString, Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
