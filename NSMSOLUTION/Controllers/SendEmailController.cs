//using EASendMail;
using MVC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Utilities;

namespace NewInventorySystem.Controllers
{
    public class SendEmailController : Controller
    {
        // GET: SendEmail
        [ValidateInput(false)]
        public ActionResult Index(object[] PrimaryFieldValue)
        {
            ViewBag.Data = JsonConvert.SerializeObject(PrimaryFieldValue[0].ToString());
            return View();
        }

        [ValidateInput(false)]
        //public JsonResult Send(string EmailTo, string CC = "", string BCC = "", string Subject = "", string MessageBody = "", string imgAttachmentFile="")

        public JsonResult Send(object[] Data)
        {
            SqlResult result = new SqlResult();
            try
            {
                JavaScriptSerializer jsJson = new JavaScriptSerializer
                {
                    MaxJsonLength = int.MaxValue
                };
                dynamic Datainfo = jsJson.DeserializeObject(Data[0].ToString());
                string CustomerName = Datainfo["CustomerName"];
                string InvoiceFileName = Datainfo["InvoiceFileName"];
                string CustomerCode = Datainfo["CustomerCode"];
                string InvoiceNo = Datainfo["InvoiceNo"];
                string EmailTo = Datainfo["EmailTo"];
                string CC = Datainfo["CC"];
                string BCC = Datainfo["BCC"];
                string Subject = Datainfo["Subject"];
                string MessageBody = Datainfo["MessageBody"];
                string PDF_FileName = Datainfo["PDF_FileName"];
                string PNG_FileName = Datainfo["PNG_FileName"];
                //string imgAttachmentFile = Datainfo["imgAttachmentFile"];
                //string pdfData = Datainfo["pdfData"];
                //string NSM_LOGO = Datainfo["NSM_LOGO"];
                //string NSM_ISP_LOGO = Datainfo["NSM_ISP_LOGO"];

                //SmtpMail oMail = new SmtpMail("TryIt");
                //oMail.From.Address = "sothea8989@hotmail.com";
                //oMail.From.Name = "NSM ISP";
                //if (CC != "")
                //    oMail.Cc = CC;
                //if (BCC != "")
                //    oMail.Bcc = BCC;
                //oMail.To = EmailTo;
                //oMail.Subject = Subject;
                //oMail.HtmlBody = MessageBody;
                //SmtpServer oServer = new SmtpServer("smtp.office365.com");
                //oServer.User = "sothea8989@hotmail.com"; oServer.Password = "Thea8989";
                //oServer.Port = 587;
                //oServer.ConnectType = SmtpConnectType.ConnectSSLAuto;
                //SmtpClient oSmtp = new SmtpClient();
                //oSmtp.SendMail(oServer, oMail);

                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                EmailProperty emailProperty = new EmailProperty();

                //message.From = new MailAddress(emailProperty.EmailFrom, "NSM SOLUTION CO.,LTD");
                message.From = new MailAddress("sothea@royaldragonenergy.com", "NSM SOLUTION");
                message.To.Add(new MailAddress(EmailTo));
                message.To.Add(new MailAddress(emailProperty.EmailFrom));
                if (CC != "")
                    message.CC.Add(new MailAddress(CC));
                if (BCC != "")
                    message.Bcc.Add(new MailAddress(BCC));
                message.Subject = Subject;
                message.SubjectEncoding = Encoding.UTF8;
                message.IsBodyHtml = true;

                smtp.Port = emailProperty.Port;
                //smtp.Host = emailProperty.Host;
                smtp.Host = "mail.royaldragonenergy.com";
                //smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                //smtp.Credentials = new NetworkCredential(emailProperty.CredentialsEmail, emailProperty.CredentialsPassword);
                smtp.Credentials = new NetworkCredential("sothea@royaldragonenergy.com", "Thea!8989");
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                //Regex regex = new Regex(@"^[\w/\:.-]+;base64,");
                //string base64 = regex.Replace(imgAttachmentFile, string.Empty);
                //byte[] bitmapData = Convert.FromBase64String(FixBase64ForImage(base64));
                ////string fileName = UploadToServer(bitmapData, CustomerName);

                //base64 = regex.Replace(pdfData, string.Empty);
                //bitmapData = Convert.FromBase64String(FixBase64ForImage(base64));
                //string PDFfileName = UploadToServer(bitmapData, CustomerName, true);

                //MemoryStream streamBitmap = new MemoryStream(bitmapData);
                StringBuilder sb = new StringBuilder();
                HTML html = new HTML();
                var localPath = html.ServerRoot; // HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
                sb.AppendLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
                sb.AppendLine("<html xmln=\"http://www.w3.org/1999/xhtml\">");
                sb.AppendLine("<head>");
                sb.AppendLine("<meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\" />");
                //sb.Append("<link rel=\"stylesheet\" type=\"text/css\" href=\"" + localPath + "/Content/AdminLTE-3.2.0/dist/css/adminlte.min.css\" />");
                sb.AppendLine("</head><body>");
                sb.AppendLine(MessageBody);
                //sb.Append("<script src=\"" + localPath + "/Scripts/466ac529e3.js\"></script>");
                sb.AppendLine("</body></html>");

                //string[] rows = Regex.Split(sb.ToString(), "\r\n");                

                //string NSMLOGO = "~/Content/CompanyLogo/NSM Logo_1784101994.png";
                //string NSMISPLOGO = "~/Content/CompanyLogo/Logo NSM-ISP_519751247.png";
                //NSMLOGO = HttpContext.Server.MapPath(NSMLOGO);
                //NSMISPLOGO= HttpContext.Server.MapPath(NSMISPLOGO);
                //sb.Clear();
                //for (int i = 0; i < rows.Length; i++)
                //{
                //    if (rows[i].Contains("imgnsmlogo"))
                //    {
                //        rows[i] = rows[i].Replace(NSM_LOGO, "cid:NSMLOGO");
                //    }
                //    if (rows[i].Contains("imgnsmisplogo"))
                //    {
                //        rows[i] = rows[i].Replace(NSM_ISP_LOGO, "cid:NSMISPLOGO");
                //    }
                //    sb.AppendLine(rows[i]);
                //}

                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(
                                                          sb.ToString(),
                                                          Encoding.UTF8,
                                                          MediaTypeNames.Text.Html);
                //string FilePath = HttpContext.Server.MapPath(@"\\" + InvoiceFileName);
                string FilePath = InvoiceFileName;

                Attachment attachment = new Attachment(FilePath);
                attachment.ContentDisposition.Inline = true;
                attachment.TransferEncoding = TransferEncoding.Base64;
                message.Attachments.Add(attachment);

                message.AlternateViews.Add(htmlView);

                //attachment = new Attachment(NSMLOGO);
                //attachment.ContentId = "cid:NSMLOGO";
                //message.Attachments.Add(attachment);

                //attachment = new Attachment(NSMISPLOGO);
                //attachment.ContentId = "cid:NSMISPLOGO";
                //message.Attachments.Add(attachment);

                //string mediaType = MediaTypeNames.Image.Jpeg;
                //LinkedResource img = new LinkedResource(NSMLOGO);
                //img.ContentId = "cid:NSMLOGO";
                //img.ContentType.MediaType = mediaType;
                //img.TransferEncoding = TransferEncoding.Base64;
                //img.ContentType.Name = img.ContentId;
                //img.ContentLink = new Uri("cid:" + img.ContentId);
                //htmlView.LinkedResources.Add(img);
                //message.AlternateViews.Add(htmlView);

                //img = new LinkedResource(NSMISPLOGO);
                //img.ContentId = "cid:NSMISPLOGO";
                //img.ContentType.MediaType = mediaType;
                //img.TransferEncoding = TransferEncoding.Base64;
                //img.ContentType.Name = img.ContentId;
                //img.ContentLink = new Uri("cid:" + img.ContentId);
                //htmlView.LinkedResources.Add(img);
                //message.AlternateViews.Add(htmlView);

                //message.BodyEncoding = Encoding.UTF8;
                //message.BodyTransferEncoding = TransferEncoding.Base64;
                //message.AlternateViews.Add(htmlView);
                //message.Body = sb.ToString();

                //string body = string.Empty;
                //using (StreamReader reader = new StreamReader(Server.MapPath("~/Templates/EmailTemplate.html")))
                //{
                //    body = reader.ReadToEnd();
                //}
                //body = body.Replace("{Title}", "Customer Invoice");
                //body = body.Replace("{MessageBody}", MessageBody);
                //message.Body = body;

                //Attachment attachment = new Attachment(fileName);
                //attachment.ContentDisposition.Inline = true;
                //ContentDisposition disposition = attachment.ContentDisposition;
                //disposition.CreationDate = System.IO.File.GetCreationTime(fileName);
                //disposition.ModificationDate = System.IO.File.GetLastWriteTime(fileName);
                //disposition.ReadDate = System.IO.File.GetLastAccessTime(fileName);
                //disposition.FileName = Path.GetFileName(fileName);
                //disposition.Size = new FileInfo(fileName).Length;
                //disposition.DispositionType = DispositionTypeNames.Attachment;

                //attachment.TransferEncoding = TransferEncoding.Base64;
                //message.Attachments.Add(attachment);
                //attachment = new Attachment(PDFfileName);
                //attachment.TransferEncoding = TransferEncoding.Base64;
                //message.Attachments.Add(attachment);
                //message.Priority = MailPriority.High;
                smtp.Send(message);

                result.Result = ResultType.Successful;
                result.Message = "Sent successful.";

            }
            catch (Exception ex)
            {
                result.Result = ResultType.Failed;
                result.Message = ex.Message;
            }
            return Json(new { Result = result.Result, Message = result.Message }, JsonRequestBehavior.AllowGet);
        }

        public static string FixBase64ForImage(string Image)
        {
            StringBuilder sbText = new StringBuilder(Image, Image.Length);
            sbText.Replace("\r\n", string.Empty); sbText.Replace(" ", string.Empty);
            return sbText.ToString();
        }
        public Image LoadBase64(string base64)
        {
            //base64 = base64.Replace("data:image/png;base64,", String.Empty);
            //base64 = base64.Replace("data:image/jpg;base64,", String.Empty);
            Regex regex = new Regex(@"^[\w/\:.-]+;base64,");
            base64 = regex.Replace(base64, string.Empty);

            byte[] bytes = Convert.FromBase64String(base64);
            Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
            }
            return image;
        }
        private static AlternateView ContentToAlternateView(string content)
        {
            var imgCount = 0;
            List<LinkedResource> resourceCollection = new List<LinkedResource>();
            foreach (Match m in Regex.Matches(content, "<img(?<value>.*?)>"))
            {
                imgCount++;
                var imgContent = m.Groups["value"].Value;
                string type = Regex.Match(imgContent, ":(?<type>.*?);base64,").Groups["type"].Value;
                string base64 = Regex.Match(imgContent, "base64,(?<base64>.*?)\"").Groups["base64"].Value;
                if (String.IsNullOrEmpty(type) || String.IsNullOrEmpty(base64))
                {
                    //ignore replacement when match normal <img> tag
                    continue;
                }
                var replacement = " src=\"cid:" + imgCount + "\"";
                content = content.Replace(imgContent, replacement);
                var tempResource = new LinkedResource(Base64ToImageStream(base64), new ContentType(type))
                {
                    ContentId = imgCount.ToString()
                };
                resourceCollection.Add(tempResource);
            }

            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(content, null, MediaTypeNames.Text.Html);
            foreach (var item in resourceCollection)
            {
                alternateView.LinkedResources.Add(item);
            }

            return alternateView;
        }
        public static Stream Base64ToImageStream(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            return ms;
        }

        public string UploadToServer(byte[] image, string fileName, bool isPDF = false)
        {
            HTML html = new HTML();
            var localPath = HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);
            fileName = $"{html.ToUrlSlug($"{fileName}")}";
            var root = HttpContext.Server.MapPath(@"/Uploads/CustomerAttFiles/" + fileName + "/");
            fileName = "SentMail_" + fileName;
            string filePath;
            try
            {
                if (!Directory.Exists(root))
                    Directory.CreateDirectory(root);

                if (!isPDF)
                {
                    var bm = ByteArrayToImage(image);
                    //filePath = $"{ToUrlSlug($"IMG-{GetUnixEpochMethod(DateTime.Now)}-{fileName}")}.{ImageFormat.Jpeg}";
                    filePath = fileName + $"{html.ToUrlSlug($"-{html.GetUnixEpochMethod(DateTime.Now)}")}.{ImageFormat.Jpeg}";
                    var filePathUpdated = Path.Combine(root, filePath);
                    bm.Save(filePathUpdated, ImageFormat.Jpeg);
                    bm.Dispose();
                }
                else
                {
                    filePath = $"{html.ToUrlSlug($"{fileName}-{html.GetUnixEpochMethod(DateTime.Now)}")}.pdf";
                    var filePathUpdated = Path.Combine(root, filePath);
                    //System.IO.File(filePathUpdated, "application/pdf");
                    System.IO.File.WriteAllBytes(filePathUpdated, image);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //return localPath + "/Uploads/" + filePath;
            return root + filePath;
        }

        protected Image ByteArrayToImage(byte[] byteArrayIn)
        {
            var converter = new ImageConverter();
            var img = (Image)converter.ConvertFrom(byteArrayIn);
            return img;
        }




    }
}