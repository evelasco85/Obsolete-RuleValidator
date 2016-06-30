using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RuleValidator_Proto.Integration.Utility
{
    public interface IFileConverter
    {
        byte[] GetFileBytes(Stream file, int contentLength);
        StringBuilder GetFileTextContent(byte[] fileContentBytes);

        Stream GetFileStream(byte[] fileContentBytes);
    }

    //Singleton implementation
    public class FileConverter : IFileConverter
    {
        static IFileConverter s_singletonInstance;

        private FileConverter()
        {
        }

        public Stream GetFileStream(byte[] fileContentBytes)
        {
            Stream file = FileStream.Null;

            file = new MemoryStream(fileContentBytes, true);

            return file;
        }

        public static IFileConverter GetInstance()
        {
            if (s_singletonInstance == null)
                s_singletonInstance = new FileConverter();

            return s_singletonInstance;
        }
        public byte[] GetFileBytes(Stream file, int contentLength)
        {
            byte[] fileBytes = null;

            if((file == null) || (contentLength < 1))
                return fileBytes;

            fileBytes = new byte[contentLength];
            file.Read(fileBytes, 0, contentLength);

            file.Close();

            return fileBytes;
        }

        public StringBuilder GetFileTextContent(byte[] fileContentBytes)
        {
            StringBuilder textContent = new StringBuilder(500);

            if (fileContentBytes == null)
                return textContent;

            using (MemoryStream memoryStream = new MemoryStream(fileContentBytes))
            {
                StreamReader textReader = new StreamReader(memoryStream);

                textContent.Append(textReader.ReadToEnd());
            }

            return textContent;
        }
    }
}
