using System.IO;

namespace Achievement.Exporter.Plugin
{
    public class IOUtils
    {
        public static void CreateFolder(string dir)
        {
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }
        }

        /// <summary>
        /// 清空文件夹
        /// </summary>
        public static void DeleteFolder(string dir)
        {
            foreach (string d in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(d))
                {
                    FileInfo fi = new(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;
                    File.Delete(d); // 直接删除其中的文件 
                }
                else
                {
                    DirectoryInfo d1 = new(d);
                    if (d1.GetFiles().Length != 0)
                    {
                        DeleteFolder(d1.FullName); // 递归删除子文件夹
                    }
                    Directory.Delete(d);
                }
            }
        }
    }
}
