using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Diagnostics;
using System.Threading;

namespace SNSHelper.AutoUpdate
{
    class Program
    {
        static string workingFolder = Directory.GetCurrentDirectory();
        static string versionFolder = Path.Combine(workingFolder, "Version");
        static string latestFolder = Path.Combine(versionFolder, "Latest");
        static string backupFolder = Path.Combine(versionFolder, "BackUp");

        static void Main(string[] args)
        {
            Console.WriteLine("《开心网争车位助手》更新程序 -- By Jailu(jailu@163.com)");

            try
            {
                if (Directory.Exists(latestFolder))
                {
                    Directory.Delete(latestFolder, true);
                }

                Directory.CreateDirectory(latestFolder);

                string latestFileName = GetLatestFileName();

                if (string.IsNullOrEmpty(latestFileName))
                {
                    Console.WriteLine("未找到可用更新！");
                    Console.Write("请单击任意键，退出更新程序");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine();
                Console.WriteLine("开始解压文件...");
                Console.WriteLine();

                latestFileName = Path.Combine(versionFolder, latestFileName);

                UnZip(latestFileName);

                Console.WriteLine();
                Console.Write("正在备份原文件...");
                BackUpOldFile();
                Console.WriteLine("完成！");

                Console.WriteLine();
                Console.Write("开始进行版本升级...");
                UpdateFile();
                Console.WriteLine("完成！");
                Console.WriteLine();
                Console.Write("请单击任意键，退出更新程序");
                File.Delete(Path.Combine(versionFolder, "LatestVersion.txt"));
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("更新出错！");
                Console.Write("请单击任意键，退出更新程序");
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
        }

        static string GetLatestFileName()
        {
            string latestFilePath = Path.Combine(versionFolder, "LatestVersion.txt");
            if (File.Exists(latestFilePath))
            {
                return File.ReadAllText(latestFilePath);
            }
            else
            {
                return null;
            }
        }

        static void UnZip(string path)
        {
            ZipInputStream s = new ZipInputStream(File.OpenRead(path));

            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string fileName = Path.GetFileName(theEntry.Name);

                if (!Directory.Exists(latestFolder))
                {
                    Directory.CreateDirectory(latestFolder);
                }

                if (fileName != String.Empty)
                {
                    string curPath = Path.Combine(latestFolder, theEntry.Name);
                    Console.Write("正在解压：" + curPath);
                    string curDir = Path.GetDirectoryName(curPath);
                    if (!Directory.Exists(curDir))
                    {
                        Directory.CreateDirectory(curDir);
                    }

                    FileStream streamWriter = File.Create(curPath);

                    int size = 2048;
                    byte[] data = new byte[2048];
                    while (true)
                    {
                        size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            streamWriter.Write(data, 0, size);
                        }
                        else
                        {
                            break;
                        }
                    }

                    streamWriter.Close();
                    Console.WriteLine(" 完成！");
                }
            }
            s.Close();
        }

        static void BackUpOldFile()
        {
            if (Directory.Exists(backupFolder))
            {
                Directory.Delete(backupFolder, true);
            }
            Directory.CreateDirectory(backupFolder);

            foreach (string filePath in Directory.GetFiles(workingFolder))
            {
                File.Copy(filePath, Path.Combine(backupFolder, Path.GetFileName(filePath)), true);
            }
        }

        static void UpdateFile()
        {
            foreach (string filePath in Directory.GetFiles(latestFolder))
            {
                if (Path.GetFileName(filePath).Equals("AutoUpdate.exe"))
                {
                    continue;
                }

                Thread.Sleep(1000);
                File.Copy(filePath, Path.Combine(workingFolder, Path.GetFileName(filePath)), true);
            }
        }
    }
}
