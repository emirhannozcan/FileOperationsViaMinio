namespace FileOperationsViaMinio.Utilities
{
    public static class MinioUtil
    {
        public static string Join(string path1, string path2)
        {
            return $"{path1}/{path2}";
        }
        public static string Join(string path1, string path2, string path3)
        {
            return $"{path1}/{path2}/{path3}";
        }
        public static string Join(string path1, string path2, string path3, string path4)
        {
            return $"{path1}/{path2}/{path3}/{path4}";
        }
        public static string Join(params string[] paths)
        {
            string fullPath = "";
            for (int i = 0; i < paths.Length; i++)
            {
                if (i == 0)
                    fullPath = paths[i];

                else
                    fullPath = $"{fullPath}/{paths[i]}";
            }
            return fullPath;
        }

        public static byte[] ToByteArray(this Stream stream)
        {
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            for (int totalBytesCopied = 0; totalBytesCopied < stream.Length;)
                totalBytesCopied += stream.Read(buffer, totalBytesCopied, Convert.ToInt32(stream.Length) - totalBytesCopied);
            return buffer;
        }
    }
}
