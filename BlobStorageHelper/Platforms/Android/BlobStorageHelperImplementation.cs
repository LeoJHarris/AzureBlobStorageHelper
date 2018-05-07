using Android.Webkit;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LeoJHarris.XForms.Plugin.BlobStorageHelper
{
    /// <summary>
    /// Interface for $safeprojectgroupname$
    /// </summary>
    public class BlobStorageHelperImplementation : IBlobStorageHelper
    {
        /// <summary>
        /// The save file.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <param name="stream">
        /// The stream.
        /// </param>
        public string SaveFile(string filename, Stream stream)
        {
            Java.IO.File dir = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryPictures);

            string filePath = Path.Combine(dir.AbsolutePath, filename);
            CopyStream(stream, filePath);

            return filePath;
        }

        /// <summary>
        /// The load file.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        public byte[] LoadFile(string filename)
        {
            Java.IO.File path = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryPictures);

            string filePath = Path.Combine(path.AbsolutePath, filename);

            return File.ReadAllBytes(filePath);
        }

        public string FileExists(string filename)
        {
            string directory = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);

            return File.Exists(directory + "/" + filename)
                ? directory + "/" + filename
                : string.Empty;
        }

        public async Task<string> RetrieveBlob(string linkToFile, string fileName, string connectionString)
        {
            CloudBlockBlob blob = string.IsNullOrEmpty(connectionString) ? new CloudBlockBlob(new Uri(linkToFile)) :
                new CloudBlockBlob(new Uri(linkToFile), new StorageCredentials(connectionString));

            if (await blob.ExistsAsync().ConfigureAwait(false))
            {
                string directory = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

                // Checks Directory exists
                if (File.Exists(directory + "/" + blob.Name) == false)
                {
                    Stream fileStream = new FileStream(
                        directory + "/" + blob.Name,
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.None);

                    await blob.DownloadToStreamAsync(fileStream).ConfigureAwait(false);

                    fileStream.Close();
                }

                return directory + "/" + blob.Name;
            }

            return string.Empty;
        }

        /// <summary>
        /// The delete files.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        public void DeleteFile(string filename)
        {
            Java.IO.File path = Android.App.Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryPictures);

            string filePath = Path.Combine(path.AbsolutePath, filename);

            File.Delete(filePath);
        }

        /// <summary>
        /// The copy stream.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="destPath">
        /// The dest path.
        /// </param>
        public void CopyStream(Stream stream, string destPath)
        {
            using (FileStream fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }

        public async Task<Uri> UploadBlob(Guid containerName, Guid blobName, string storageConnectionString, Stream stream)
        {
            string filePath = SaveFile(blobName.ToString(), stream);

            if (!string.IsNullOrEmpty(filePath))
            {
                // Get the file
                byte[] fileInfo = LoadFile(blobName.ToString("D"));
                if (fileInfo.Length > 0)
                {
                    string newFileName = blobName.ToString("D");

                    // Get the container
                    CloudBlobContainer container = await GetContainer(containerName, storageConnectionString).ConfigureAwait(false);

                    CloudBlockBlob blob = container.GetBlockBlobReference(newFileName);

                    BlobRequestOptions options = new BlobRequestOptions { ServerTimeout = new TimeSpan(12, 0, 0) };

                    try
                    {
                        using (Stream fileStream = new MemoryStream(fileInfo))
                        {
                            // If file is larger than 4MB, split file into 250kb chunks and upload
                            int maxSize = 1 * 1024 * 1024; // 4 MB
                            if (fileStream.Length > maxSize)
                            {
                                byte[] data = ReadToEnd(fileStream);
                                int id = 0;
                                int byteslength = data.Length;
                                int bytesread;
                                int index = 0;
                                List<string> blocklist = new List<string>();
                                const int numBytesPerChunk = 250 * 1024;

                                do
                                {
                                    byte[] buffer = new byte[numBytesPerChunk];
                                    int limit = index + numBytesPerChunk;
                                    for (int loops = 0; index < limit; index++)
                                    {
                                        buffer[loops] = data[index];
                                        loops++;
                                    }

                                    bytesread = index;
                                    string blockIdBase64 = Convert.ToBase64String(BitConverter.GetBytes(id));

                                    await blob.PutBlockAsync(blockIdBase64, new MemoryStream(buffer, true), null).ConfigureAwait(false);
                                    blocklist.Add(blockIdBase64);
                                    id++;
                                }
                                while (byteslength - bytesread > numBytesPerChunk);

                                int final = byteslength - bytesread;
                                byte[] finalbuffer = new byte[final];
                                for (int loops = 0; index < byteslength; index++)
                                {
                                    finalbuffer[loops] = data[index];
                                    loops++;
                                }

                                string blockId = Convert.ToBase64String(BitConverter.GetBytes(id));
                                await blob.PutBlockAsync(blockId, new MemoryStream(finalbuffer, true), null).ConfigureAwait(false);
                                blocklist.Add(blockId);

                                await blob.PutBlockListAsync(blocklist, null, options, null).ConfigureAwait(false);
                            }
                            else
                            {
                                await blob.UploadFromStreamAsync(fileStream).ConfigureAwait(false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is ArgumentException
                            || ex is UnauthorizedAccessException
                            || ex is FileNotFoundException
                            || ex is NotSupportedException
                            || ex is StorageException)
                        {
                            return default(Uri);
                        }

                        throw;
                    }

                    // Update the mime type
                    await blob.FetchAttributesAsync().ConfigureAwait(false);

                    string type = null;
                    var extension = MimeTypeMap.GetFileExtensionFromUrl(blobName.ToString().ToLower());

                    if (extension != null)
                    {
                        blob.Properties.ContentType = MimeTypeMap.Singleton.GetMimeTypeFromExtension(extension);
                        await blob.SetPropertiesAsync().ConfigureAwait(false);

                        // Success!
                        return GenerateUrl(containerName, blobName, storageConnectionString);
                    }
                }
            }

            return default(Uri);
        }

        /// <summary>
        /// The get container.
        /// </summary>
        /// <param name="containerName">
        /// The container name.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async static Task<CloudBlobContainer> GetContainer(Guid containerName, string storageConnectionString)
        {
            // Retrieve storage account from connection-string
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // Create the blob client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to container
            CloudBlobContainer container = blobClient.GetContainerReference(containerName.ToString("D"));

            // Create container if it doesn't exist and set permissions to public
            if (!await container.ExistsAsync().ConfigureAwait(false))
            {
                await container.CreateAsync().ConfigureAwait(false);
                await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob }).ConfigureAwait(false);
            }

            // Return container
            return container;
        }

        /// <summary>
        /// The read to end.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <returns>
        /// The <see cref="byte[]"/>.
        /// </returns>
        public static byte[] ReadToEnd(Stream stream)
        {
            long originalPosition = 0;

            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Position = 0;
            }

            try
            {
                byte[] readBuffer = new byte[4096];

                int totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
                {
                    totalBytesRead += bytesRead;

                    if (totalBytesRead == readBuffer.Length)
                    {
                        int nextByte = stream.ReadByte();
                        if (nextByte != -1)
                        {
                            byte[] temp = new byte[readBuffer.Length * 2];
                            Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                            Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                            readBuffer = temp;
                            totalBytesRead++;
                        }
                    }
                }

                byte[] buffer = readBuffer;
                if (readBuffer.Length != totalBytesRead)
                {
                    buffer = new byte[totalBytesRead];
                    Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
                }

                return buffer;
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Position = originalPosition;
                }
            }
        }

        /// <summary>
        /// The generate url.
        /// </summary>
        /// <param name="containerName">
        /// The container name.
        /// </param>
        /// <param name="blob">
        /// The blob.
        /// </param>
        /// <param name="storageConnectionString">
        /// The storage Connection String.
        /// </param>
        /// <returns>
        /// The <see cref="Uri"/>.
        /// </returns>
        public static Uri GenerateUrl(Guid containerName, Guid blob, string storageConnectionString)
        {
            // Retrieve storage account from the connection string
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // Get the URL inside the container
            StringBuilder sb = new StringBuilder();
            sb.Append(containerName.ToString("D"));
            sb.Append("/");
            sb.Append(blob.ToString("D"));

            // Generate the absolute url
            return new Uri(storageAccount.BlobEndpoint, new Uri(sb.ToString(), UriKind.Relative));
        }
    }
}
