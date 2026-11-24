using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Elements.Core.FileUtil;

namespace Elements.Core
{
    public static class FileUtil
    {
        public enum FileHash
        {
            MD5,
            SHA1,
            SHA256,
        }

        public static byte[] ComputeHash(string filename, FileHash algorithm)
        {
            using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read,
                FileShare.Read))
            {
                return ComputeHash(stream, algorithm);
            }
        }

        public static byte[] ComputeHash(Stream stream, FileHash algorithm)
        {
            HashAlgorithm hasher = null;

            switch(algorithm)
            {
                case FileHash.MD5:
                    hasher = MD5.Create(); break;
                case FileHash.SHA256:
                    hasher = SHA256.Create(); break;
                case FileHash.SHA1:
                    hasher = SHA1.Create(); break;
            }

            var hash = hasher.ComputeHash(stream);

            return hash;
        }

        public static string GenerateFileSignature(string filename, FileHash method = FileHash.SHA1)
        {
            var hash = ComputeHash(filename, method);
            return StringHelper.ToURLBase64(hash);
        }

        public static async Task<string> GenerateFileSignatureAsync(string filename, FileHash method = FileHash.SHA1, int attempts = 10)
        {
            Exception exception = null;

            int delay = 50;

            do
            {
                try
                {
                    return GenerateFileSignature(filename, method);
                }
                catch (Exception ex)
                {
                    await Task.Delay(delay);

                    delay *= 2;

                    attempts--;

                    exception = ex;
                }
            } while (attempts > 0);

            throw new Exception("Exception when generating file signature " + filename + "\n" + exception);
        }

        public static string GenerateFileSignature(Stream stream, FileHash method = FileHash.SHA1)
        {
            var hash = ComputeHash(stream, method);
            return StringHelper.ToURLBase64(hash);
        }

        public static string FindFile(string filename, string directory, int maxDepth = -1)
        {
            string path = Path.Combine(directory, filename);

            if (File.Exists(path))
                return path;

            // try the subdirectories
            try
            {
                if (maxDepth != 0)
                    foreach (var subdir in Directory.GetDirectories(directory))
                    {
                        path = FindFile(filename, subdir, maxDepth == -1 ? -1 : maxDepth - 1);

                        if (path != null)
                            return path;
                    }
            }
            catch(ArgumentException ex)
            {
                // Some paths seem to throw: The specified path is not of a legal form (empty).
                if (!ex.Message.Contains("not of a legal form"))
                    throw;
            }
            catch(UnauthorizedAccessException ex)
            {
                
            }

            // didn't find anything
            return null;
        }

        public static void Move(string source, string destination, int attempts = 10)
        {
            int backOff = 10;

            IOException exception;

            do
            {
                try
                {
                    File.Move(source, destination);
                    return;
                }
                catch (IOException ex)
                {
                    exception = ex;

                    if (ex.Message.Contains("Sharing violation"))
                    {
                        System.Threading.Thread.Sleep(backOff);
                        backOff = (int)(backOff * 1.5);
                        attempts--;
                    }
                    else
                        throw;
                }
            } while (attempts > 0);

            throw new Exception("Unable to move file after several attempts", exception);
        }

        public static void Delete(string path, int attempts = 10)
        {
            int backOff = 10;

            IOException exception;

            do
            {
                try
                {
                    if (File.Exists(path))
                        File.Delete(path);

                    return;
                }
                catch (IOException ex)
                {
                    exception = ex;

                    if (ex.Message.Contains("Sharing violation") || ex.Message.Contains("used by another process"))
                    {
                        System.Threading.Thread.Sleep(backOff);
                        backOff = (int)(backOff * 1.5);
                        attempts--;
                    }
                    else
                        throw;
                }
            } while (attempts > 0);

            throw new Exception("Unable to move file after several attempts", exception);
        }

        public static async Task DeleteAsync(string filePath, int attempts = 10)
        {
            Exception exception = null;

            int delay = 50;

            do
            {
                try
                {
                    File.Delete(filePath);
                    return;
                }
                catch (Exception ex)
                {
                    await Task.Delay(delay);

                    delay *= 2;

                    attempts--;

                    exception = ex;
                }
            } while (attempts > 0);

            throw new Exception("Exception when deleting file " + filePath + "\n" + exception);
        }

        public static List<string> GetPathDirectories(string path)
        {
            var list = new List<string>();

            string root = null;

            if(!string.IsNullOrEmpty(path))
                root = Path.GetPathRoot(path);

            while (!string.IsNullOrEmpty(path))
            {
                var entry = Path.GetFileName(path);
                if(!string.IsNullOrEmpty(entry))
                    list.Add(entry);
                path = Path.GetDirectoryName(path);
            }

            if(!string.IsNullOrEmpty(root))
            {
                // remove the slash in the drive root (if present)
                root = root.Replace("\\", "").Replace("/", "");
                list.Add(root);
            }

            list.Reverse();

            return list;
        }

        // From: https://stackoverflow.com/questions/58744/copy-the-entire-contents-of-a-directory-in-c-sharp
        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }
    }
}
