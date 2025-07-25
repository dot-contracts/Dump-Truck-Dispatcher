using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Abp.IO;

namespace DispatcherWeb.IO
{
    public static class AppFileHelper
    {
        public static async IAsyncEnumerable<string> ReadLinesAsync(string path)
        {
            await using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0x1000, FileOptions.SequentialScan))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            {
                string line;
                while ((line = await sr.ReadLineAsync()) != null)
                {
                    yield return line;
                }
            }
        }

        public static async IAsyncEnumerable<string> ReadLinesReverseAsync(string path)
        {
            var encoding = Encoding.UTF8;
            var charBuffer = new char[1];
            var lineBuffer = new StringBuilder();
            var byteBuffer = new byte[encoding.GetMaxByteCount(1)];

            await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            long position = fs.Length;

            while (position > 0)
            {
                fs.Seek(--position, SeekOrigin.Begin);
                int bytesRead = await fs.ReadAsync(byteBuffer.AsMemory(0, 1));
                if (bytesRead == 0)
                {
                    break; // End of stream
                }

                char[] chars = encoding.GetChars(byteBuffer, 0, bytesRead);
                if (chars.Length > 0)
                {
                    charBuffer[0] = chars[0];
                    if (charBuffer[0] == '\n' || position == 0)
                    {
                        // If we're at the start of the file, we add the character to the line buffer
                        // before yielding the line since there's no preceding newline character.
                        if (position == 0)
                        {
                            lineBuffer.Insert(0, charBuffer[0]);
                        }

                        // Skip empty lines and the last newline character at the end of the file
                        if (lineBuffer.Length > 0)
                        {
                            string line = lineBuffer.ToString();
                            lineBuffer.Clear();
                            yield return new string(line.ToArray());
                        }
                    }
                    else
                    {
                        lineBuffer.Insert(0, charBuffer[0]);
                    }
                }
            }
        }

        public static void DeleteFilesInFolderIfExists(string folderPath, string fileNameWithoutExtension)
        {
            var directory = new DirectoryInfo(folderPath);
            var tempUserProfileImages = directory.GetFiles(fileNameWithoutExtension + ".*", SearchOption.AllDirectories).ToList();
            foreach (var tempUserProfileImage in tempUserProfileImages)
            {
                FileHelper.DeleteIfExists(tempUserProfileImage.FullName);
            }
        }
    }
}
