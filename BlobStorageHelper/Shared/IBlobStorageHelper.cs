using System;
using System.IO;
using System.Threading.Tasks;

namespace LeoJHarris.XForms.Plugin.BlobStorageHelper
{
    public interface IBlobStorageHelper
    {
        // <summary>
        /// The save file.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <param name="stream">
        /// The stream.
        /// </param>
        string SaveFile(string filename, Stream stream);

        /// <summary>
        /// The load file.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        byte[] LoadFile(string filename);

        /// <summary>
        /// The upload blob.
        /// </summary>
        /// <param name="containerName">
        /// The container name.
        /// </param>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<Uri> UploadBlob(Guid containerName, Guid fileName, string connectionString, Stream stream);

        /// <summary>
        /// The delete files.
        /// </summary>
        /// <param name="fileNamesList">
        /// The file names list.
        /// </param>
        void DeleteFile(string filename);

        Task<string> RetrieveBlob(string linkToFile, string fileName, string connectionString);

        string FileExists(string filename);
    }
}