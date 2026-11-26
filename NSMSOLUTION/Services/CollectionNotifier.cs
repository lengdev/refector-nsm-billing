using MVC;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace NSMSOLUTION.Services
{
    public class SlipFile
    {
        public string OriginalName { get; set; }
        public string Extension { get; set; }
        public byte[] Bytes { get; set; }
    }

    public class StoredSlip
    {
        public string DestinationFolder { get; set; }
        public string FileName { get; set; }
        public string OriginalName { get; set; }
        public int Size { get; set; }
        public string FullPath { get; set; }
    }

    public class CollectionNotifier
    {
        private const int MaxFiles = 6;
        private const int MaxBytes = 5 * 1024 * 1024;
        private const string FolderRoot = "~/Uploads/CollectionSlips/";

        private readonly HTML html = new HTML();

        private readonly Func<IList<string>, string, long, string, Task<string>> sendPhotos;
        private readonly int systemId;

        public CollectionNotifier(Func<IList<string>, string, long, string, Task<string>> sendPhotos)
        {
            this.sendPhotos = sendPhotos;
            this.systemId = int.TryParse(html.SystemID, out var id) ? id : 0;
        }

        public static List<SlipFile> ReadSlips(HttpFileCollectionBase files)
        {
            var slips = new List<SlipFile>();
            if (files == null) return slips;

            for (int i = 0; i < files.Count; i++)
            {
                var f = files[i];
                if (f == null || f.ContentLength == 0) continue;

                if (slips.Count >= MaxFiles)
                    throw new CollectionException("You can attach up to " + MaxFiles + " slips.");
                if (f.ContentLength > MaxBytes)
                    throw new CollectionException(Path.GetFileName(f.FileName) + " is larger than 5 MB.");

                var bytes = new byte[f.ContentLength];
                var read = 0;
                while (read < bytes.Length)
                {
                    var n = f.InputStream.Read(bytes, read, bytes.Length - read);
                    if (n <= 0) break;
                    read += n;
                }

                var ext = DetectImage(bytes);
                if (ext == null)
                    throw new CollectionException(Path.GetFileName(f.FileName) + " is not a JPG, PNG or WEBP image.");

                slips.Add(new SlipFile { OriginalName = Path.GetFileName(f.FileName), Extension = ext, Bytes = bytes });
            }

            return slips;
        }

        private static string DetectImage(byte[] b)
        {
            if (b.Length > 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF) return ".jpg";
            if (b.Length > 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47) return ".png";
            if (b.Length > 12 && b[0] == 0x52 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x46 &&
                b[8] == 0x57 && b[9] == 0x45 && b[10] == 0x42 && b[11] == 0x50) return ".webp";
            return null;
        }

        public static List<StoredSlip> StoreSlips(int collectionId, List<SlipFile> slips)
        {
            var relative = FolderRoot.TrimStart('~', '/') + collectionId;
            var folder = HostingEnvironment.MapPath("~/" + relative);
            Directory.CreateDirectory(folder);

            var stored = new List<StoredSlip>();
            foreach (var s in slips)
            {
                var name = Guid.NewGuid().ToString("N") + s.Extension;
                var path = Path.Combine(folder, name);
                File.WriteAllBytes(path, s.Bytes);

                stored.Add(new StoredSlip
                {
                    DestinationFolder = relative,
                    FileName = name,
                    OriginalName = s.OriginalName,
                    Size = s.Bytes.Length,
                    FullPath = path
                });
            }
            return stored;
        }

        public async Task<string> SendAsync(string caption, IList<string> paths)
        {
            long chatId = -1003702719002;
            var bot = html.GetBotTokenBySystemID(systemId);

            var error = await sendPhotos(paths, caption, chatId, bot);
            return string.IsNullOrEmpty(error) ? null : error;
        }
    }
}