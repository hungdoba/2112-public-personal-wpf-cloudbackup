using System;
using System.IO;
using LibrarySqlite;
using System.Security;
using Microsoft.Graph;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using System.Collections.Generic;

namespace LibraryCloud
{
    public class CloudHandle
    {
        private static string _userName;
        private static string _clientId;
        private static string _tenantId;
        private static string _cloudDirectory;
        private static SecureString _password;

        public static void Init(string userName, string password, string clientId, string tenantId, string cloudDirectory)
        {
            _userName = userName;
            _clientId = clientId;
            _tenantId = tenantId;
            _cloudDirectory = cloudDirectory;

            _password = new SecureString();
            foreach (char c in password)
            {
                _password.AppendChar(c);
            }
        }

        public static async Task UploadFileAsync(string fileFullName)
        {
            long length = new FileInfo(fileFullName).Length;
            _ = length < 4000000 ? await uploadSmallFileAsync(fileFullName) : await uploadBigFileAsync(fileFullName);
        }

        private static async Task<bool> uploadBigFileAsync(string fileFullName)
        {
            try
            {
                using (var fileStream = new FileStream(fileFullName, FileMode.Open))
                {
                    var cloudFileFullName = Path.Combine(_cloudDirectory, modifyCloudFileFullName(fileFullName));

                    GraphServiceClient graphClient = getAuthenticatedGraphClient();
                    var uploadSession = await graphClient.Me.Drive.Root
                                                    .ItemWithPath(cloudFileFullName)
                                                    .CreateUploadSession()
                                                    .Request()
                                                    .PostAsync();
                    // create upload task
                    var maxChunkSize = 320 * 1024;
                    var largeUploadTask = new LargeFileUploadTask<DriveItem>(uploadSession, fileStream, maxChunkSize);

                    // upload file
                    UploadResult<DriveItem> uploadResult = await largeUploadTask.UploadAsync();

                    if (uploadResult.UploadSucceeded)
                    {
                        DatabaseHandle.BackupLog.Insert(fileFullName, "Success");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                DatabaseHandle.BackupLog.Insert(fileFullName, ex.Message);
                return false;
            }
        }

        private static async Task<bool> uploadSmallFileAsync(string fileFullName)
        {
            try
            {
                using (var fileStream = new FileStream(fileFullName, FileMode.Open))
                {
                    string cloudFileFullName = Path.Combine(_cloudDirectory, modifyCloudFileFullName(fileFullName));

                    GraphServiceClient graphClient = getAuthenticatedGraphClient();

                    await graphClient.Me.Drive.Root .ItemWithPath(cloudFileFullName)
                                                    .Content
                                                    .Request()
                                                    .PutAsync<DriveItem>(fileStream);
                    DatabaseHandle.BackupLog.Insert(fileFullName, "Success");
                }
                return true;
            }
            catch (Exception ex)
            {
                _ = DatabaseHandle.BackupLog.Insert(fileFullName, ex.Message);
                return false;
            }
        }

        private static GraphServiceClient getAuthenticatedGraphClient()
        {
            try
            {
                return new GraphServiceClient(createAuthorizationProvider());
            }
            catch (Exception ex)
            {
                _ = DatabaseHandle.BackupLog.Insert("getAuthenticatedGraphClient", ex.Message);
                return null;
            }
        }

        private static IAuthenticationProvider createAuthorizationProvider()
        {
            string authority = "https://login.microsoftonline.com/" + _tenantId + "/v2.0";
            List<string> scopes = new List<string>
            {
                "User.Read",
                "Files.Read",
                "Files.ReadWrite"
            };

            IPublicClientApplication publicClient = PublicClientApplicationBuilder.Create(_clientId)
                                                    .WithAuthority(authority)
                                                    .Build();
            return MsalAuthenticationProvider.GetInstance(publicClient, scopes.ToArray(), _userName, _password);
        }

        private static string modifyCloudFileFullName(string fileFullName)
        {
            while(fileFullName.Contains(@"\\"))
            {
                fileFullName = fileFullName.Replace(@"\\", @"\");
            }

            if (fileFullName.IndexOf("\\") == 0)
            {
                fileFullName = fileFullName.Remove(0, 1);
            }

            return fileFullName.Replace(":", "");
        }
    }
}
