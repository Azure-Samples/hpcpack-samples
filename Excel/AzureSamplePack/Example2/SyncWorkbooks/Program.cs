//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Sample demonstrating how to copy a workbook from Azure storage onto
//      an Azure VM Node
// </summary>
//------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace SyncWorkbooks
{
    class Program
    {
        static void Main(string[] args)
        {
            string workingDirectory = Environment.GetEnvironmentVariable( "WORKING_DIRECTORY" );
            string cloudAccount = Environment.GetEnvironmentVariable("CLOUD_ACCOUNT");
            string cloudKey = Environment.GetEnvironmentVariable("CLOUD_KEY");
            string blobURI = string.Format("https://{0}.blob.core.windows.net", cloudAccount);
            string workbook = Environment.GetEnvironmentVariable("MICROSOFT.HPC.EXCEL.WORKBOOKPATH");

            if (null == workbook || workbook.Equals("")) return;

            string directory = Path.GetDirectoryName(workbook);
            string lastDirectory = Path.GetFileName(directory);
            string localDir = Path.Combine(workingDirectory, lastDirectory);

            if (null == workingDirectory || workingDirectory.Equals("")) return;
            if (null == cloudAccount || cloudAccount.Equals("")) return;
            if (null == cloudKey || cloudKey.Equals("")) return;

            // install: download workbook package, unzip and store

            if (args[0].ToLower().Equals("install"))
            {

                try
                {
                    Directory.CreateDirectory(localDir);
                }
                catch { }

                string localFile = downloadFile(blobURI, cloudAccount, cloudKey, lastDirectory + ".zip");

                // if the file was not found, that's not necessarily an
                // error; the service might be trying to use a static workbook.
                // in that case, just exit

                if (null == localFile || localFile.Equals(""))
                {
                    Console.WriteLine("Package not found, skipping workbook sync");
                    return;
                }

                // unzip, move.  use the shell to unzip

                Shell32.Folder SrcFlder = GetShell32NameSpace(localFile);
                Shell32.Folder DestFlder = GetShell32NameSpace(localDir);
                Shell32.FolderItems items = SrcFlder.Items();
                DestFlder.CopyHere(items, 20);

                // clean up

                try
                {
                    File.Delete(localFile);
                }
                catch { }

            }

            // cleanup: remove the directory

            else if (args[0].ToLower().Equals("cleanup"))
            {
                try
                {
                    Directory.Delete(localDir, true);
                }
                catch { }
            }

        }

        /**
         * get the file and store it in the temp directory, return a
         * path to the file (or null if not found)
         */
        public static string downloadFile( string blobURI, string accountName, string accountKey, string fileName)
        {
            CloudBlobClient blobClient = new CloudBlobClient(new Uri(blobURI), new StorageCredentials(accountName, accountKey));
            fileName = fileName.ToLower();

            foreach (CloudBlobContainer container in blobClient.ListContainers())
            {
                foreach ( IListBlobItem blobItem in container.ListBlobs())
                {
                    string file = blobItem.Uri.ToString();
                    file = file.Substring(file.LastIndexOf('/') + 1);

                    if (file.ToLower().Equals(fileName))
                    {
                        using (FileStream fs = new FileStream(Path.Combine(Path.GetTempPath(), fileName), FileMode.Create))
                        {
                            ((ICloudBlob)blobItem).DownloadToStream(fs);
                        }
                        return Path.Combine(Path.GetTempPath(), fileName);
                    }

                }
            }

            return null;
        }

        static private Shell32.Folder GetShell32NameSpace(Object folder)
        {
            Type shellAppType = Type.GetTypeFromProgID("Shell.Application");
            Object shell = Activator.CreateInstance(shellAppType);
            return (Shell32.Folder)shellAppType.InvokeMember("NameSpace", System.Reflection.BindingFlags.InvokeMethod, null, shell, new object[] { folder });
        }

    }
}
