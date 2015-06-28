/*---------------------------------------------------------------------
* FileTests.cs - file description
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 9/12/2007 4:51:10 PM 
* 
* Tests the basic functionality of Socket objects 
* ---------------------------------------------------------------------*/

using System;
using System.Text;
using System.Reflection;


using System.IO;


namespace FileSystemTest
{

    public class FileStreamTests : IMFTestInterface
    {
        private const String randomString =
@"This is an unusual paragraph. I'm curious how quickly you can find out 
what is so unusual about it. It looks so plain you would think nothing was
wrong with it. In fact, nothing is wrong with it! It is unusual though. 
Study it, and think about it, but you still may not find anything odd. But
if you work at it a bit, you might find out! Try to do so without any 
coaching! You probably won't, at first, find anything particularly odd or 
unusual or in any way dissimilar to any ordinary composition. That is not 
at all surprising, for it is no strain to accomplish in so short a paragraph 
a stunt similar to that which an author did throughout all of his book, 
without spoiling a good writing job, and it was no small book at that. By 
studying this paragraph assiduously, you will shortly, I trust, know what is
its distinguishing oddity. Upon locating that 'mark of distinction,' you 
will probably doubt my story of this author and his book of similar 
unusuality throughout. It is commonly known among book-conscious folk and 
proof of it is still around. If you must know, this sort of writing is known 
as a lipogram, but don't look up that word in any dictionary until you find 
out what this is all about.";

        static byte[] randomBytes = System.Text.Encoding.UTF8.GetBytes(randomString);

        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("The following tests are located in FileStreamTests.cs");

            // delete the directory DOTNETMF_FS_EMULATION
            try 
            {
                IOTests.IntializeVolume();
            }
            catch 
            { 
                Log.Comment("Unable to format media, skipping class tests."); 

                return InitializeResult.Skip;
            }

            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
        }

        private void CreateFile(String path)
        {
            FileStream fs = new FileStream(path, FileMode.Create);

            fs.Write(randomBytes, 0, randomBytes.Length);

            fs.Close();
        }



        [TestMethod]
        public MFTestResults Basic_Behavior()
        {
            const string file = "foo.txt";
            MFTestResults result = MFTestResults.Pass;
            try
            {
                const string content = "Hello, World!";

                StreamWriter sw = new StreamWriter(file);

                sw.Write(content);

                sw.Close();

                try
                {
                    sw.Write("cannot write this");
                    return MFTestResults.Fail;
                }
                catch (ObjectDisposedException)
                {
                    result = MFTestResults.Pass;
                    Log.Exception("Disposed Stream cannot be written");
                }

                //--//

                StreamReader sr = new StreamReader(file);

                char[] buffer = new char[13];

                int read = sr.Read(buffer, 0, buffer.Length);

                sr.Close();

                try
                {
                    int readAfter = 0;

                    readAfter = sr.Read(buffer, buffer.Length, 1);

                    if (readAfter != 0)
                    {
                        throw new Exception("cannot read after the StreamReader is closed");
                    }

                }
                catch (ObjectDisposedException)
                {
                    result = MFTestResults.Pass;
                    Log.Exception("Disposed Stream cannot be read");
                }

                string readContent = new string(buffer);

                if (readContent != content)
                {
                    result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }

            return result;
        }

        //--//

        [TestMethod]
        public MFTestResults Basic_Read_Int()
        {
            const string file = "foo.txt";
            MFTestResults result = MFTestResults.Pass;
            try
            {
                const string content = "Hello, World!";

                StreamWriter sw = new StreamWriter(file);

                sw.Write(content);

                sw.Close();

                //--//

                StreamReader sr = new StreamReader(file);

                char[] buffer = new char[13];

                int read = sr.Read(buffer, 0, buffer.Length);

                sr.Close();

                result = MFTestResults.Pass;

                string readContent = new string(buffer);

                if (readContent != content)
                {
                    result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }

            return result;
        }

        [TestMethod]
        public MFTestResults Basic_ReadBlock()
        {
            const string file = "foo.txt";
            MFTestResults result = MFTestResults.Pass;
            try
            {
                const string content = "Hello, World!";

                StreamWriter sw = new StreamWriter(file);

                sw.Write(content);

                sw.Close();

                //--//

                StreamReader sr = new StreamReader(file);

                char[] buffer = new char[13];

                int read = sr.ReadBlock(buffer, 0, buffer.Length);

                sr.Close();

                string readContent = new string(buffer);

                result = MFTestResults.Pass;

                if (readContent != content)
                {
                    result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }

            return result;
        }

        //--//
        
        [TestMethod]
        public MFTestResults Basic_ReadToEnd()
        {
            const string file = "foo.txt";
            MFTestResults result = MFTestResults.Pass;
            try
            {
                const string content = "Hello, World!";

                StreamWriter sw = new StreamWriter(file);

                sw.Write(content);

                sw.Close();

                //--//

                StreamReader sr = new StreamReader(file);

                string readContent = sr.ReadToEnd();

                sr.Close();

                result = MFTestResults.Pass;

                if (readContent != content)
                {
                    result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }

            return result;
        }

        [TestMethod]
        public MFTestResults Basic_ReadToEnd_Long()
        {
            const string file = "foo.txt";
            MFTestResults result = MFTestResults.Pass;

            try
            {
                string veryShortString = "Hello, World!";
                string fourKBString = "This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!, This is 4KB long!";
                string sixKBStringString = "Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!, Instead this one is 6KB long!";
        
                string all_string_original = veryShortString + fourKBString + sixKBStringString + veryShortString + fourKBString + sixKBStringString + veryShortString + fourKBString + sixKBStringString;

                FileStream fs = new FileStream(file, FileMode.CreateNew);

                byte[] veryShortStringBytes = Encoding.UTF8.GetBytes(veryShortString);
                byte[] fourKBStringBytes = Encoding.UTF8.GetBytes(fourKBString);
                byte[] sixKBStringBytes = Encoding.UTF8.GetBytes(sixKBStringString);

                fs.Write(veryShortStringBytes, 0, veryShortStringBytes.Length); fs.Flush();
                fs.Write(fourKBStringBytes, 0, fourKBStringBytes.Length); fs.Flush();
                fs.Write(sixKBStringBytes, 0, sixKBStringBytes.Length); fs.Flush();
                //--//
                fs.Write(veryShortStringBytes, 0, veryShortStringBytes.Length); fs.Flush();
                fs.Write(fourKBStringBytes, 0, fourKBStringBytes.Length); fs.Flush();
                fs.Write(sixKBStringBytes, 0, sixKBStringBytes.Length); fs.Flush();
                //--//
                fs.Write(veryShortStringBytes, 0, veryShortStringBytes.Length); fs.Flush();
                fs.Write(fourKBStringBytes, 0, fourKBStringBytes.Length); fs.Flush();
                fs.Write(sixKBStringBytes, 0, sixKBStringBytes.Length); fs.Flush();

                fs.Dispose();

                StreamReader sr = new StreamReader(file);

                string all_text = sr.ReadToEnd();

                Log.Comment("all_text length           : " + all_text.Length);
                Log.Comment("all_string_original length: " + all_string_original.Length);

                result = MFTestResults.Pass;

                if (all_text != all_string_original)
                {
                    result = MFTestResults.Fail;
                }

                sr.Close();

                return result;
            }
            catch (OutOfMemoryException ex)
            {
                Log.Exception("OutOfMemoryException exception, this device is too small", ex);
                result = MFTestResults.Pass;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                result = MFTestResults.Fail;
            }
            finally
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }

            return result;
        }

        [TestMethod]
        public MFTestResults Basic_ReadLine()
        {
            const string file = "foo.txt";
            MFTestResults result = MFTestResults.Pass;
            try
            {
                const string content = "Hello, World!";

                StreamWriter sw = new StreamWriter(file);

                sw.WriteLine(content);

                sw.Close();

                //--//

                StreamReader sr = new StreamReader(file);

                string readContent = sr.ReadLine();

                sr.Close();

                result = MFTestResults.Pass;

                if (readContent != content)
                {
                    result = MFTestResults.Fail;
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
            }
            finally
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }

            return result;
        }

        [TestMethod]
        public MFTestResults FileStreamTest_ReadWrite0a()
        {
            Directory.SetCurrentDirectory(IOTests.Volume.RootDirectory);
            return ReadWrite0();
        }

        private MFTestResults ReadWrite0()
        {
            try
            {
                FileStream fs = new FileStream("ReadWrite0.txt", FileMode.CreateNew);

                fs.Write(randomBytes, 0, randomBytes.Length);

                fs.Close();

                fs = new FileStream("ReadWrite0.txt", FileMode.Open);

                byte[] readBack = new byte[randomBytes.Length];

                fs.Read(readBack, 0, randomBytes.Length);

                fs.Close();

                for (int i = 0; i < 50; i++)
                {
                    if(randomBytes[i] != readBack[i])
                    {
                        throw new Exception();
                    }
                }

                return MFTestResults.Pass;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                return MFTestResults.Fail;
            }
        }

        [TestMethod]
        public MFTestResults FileStreamTest_Seek0()
        {
            try
            {
                CreateFile("Seek0.txt");

                FileStream fs = new FileStream("Seek0.txt", FileMode.Open, FileAccess.Read);

                byte[] readBack = new byte[10];

                int pos = (int)fs.Seek(-20, SeekOrigin.End);

                if(randomBytes.Length - 20 != pos) throw new Exception();

                fs.Read(readBack, 0, 10);

                for (int i = 0; i < 10; i++)
                {
                    if(randomBytes[randomBytes.Length - 20 + i] != readBack[i]) throw new Exception();
                }

                pos = (int)fs.Seek(5, SeekOrigin.Current);
                if(randomBytes.Length - 5 != pos) throw new Exception();

                pos = (int)fs.Seek(10, SeekOrigin.Begin);
                if(10 != pos) throw new Exception();

                fs.Read(readBack, 0, 10);
                for (int i = 0; i < 10; i++)
                {
                    if(randomBytes[10 + i] != readBack[i]) throw new Exception();
                }

                fs.Close();

                return MFTestResults.Pass;
            }
            catch (Exception ex)
            {
                Log.Exception("Unexpected Exception", ex);
                return MFTestResults.Fail;
            }
        }

        private class FileAccessAndFileShare
        {
            public FileShare File1Share;
            public FileAccess File2Access;
            public FileShare File2Share;
            public bool ShouldPass;

            public FileAccessAndFileShare(FileShare file1Share, FileAccess file2Access, FileShare file2Share, bool shouldPass)
            {
                File1Share = file1Share;
                File2Access = file2Access;
                File2Share = file2Share;
                ShouldPass = shouldPass;
            }

            public override string ToString()
            {
                return "File1: " + FileShareToString(File1Share) + "  File2: " + FileAccessToString(File2Access) + " " + FileShareToString(File2Share);
            }

            private String FileShareToString(FileShare fs)
            {
                switch (fs)
                {
                    case FileShare.None:
                        return "FileShare.None";
                    case FileShare.Read:
                        return "FileShare.Read";
                    case FileShare.Write:
                        return "FileShare.Write";
                    case FileShare.ReadWrite:
                        return "FileShare.ReadWrite";
                }
                return "";
            }

            private String FileAccessToString(FileAccess fa)
            {
                switch (fa)
                {
                    case FileAccess.Read:
                        return "FileAccess.Read";
                    case FileAccess.ReadWrite:
                        return "FileAccess.ReadWrite";
                    case FileAccess.Write:
                        return "FileAccess.Write";
                }
                return "";
            }
        }

        [TestMethod]
        public MFTestResults FileStreamTest_FileAccessAndFileShare()
        {
            FileAccessAndFileShare[] fafs =
            {
                new FileAccessAndFileShare(FileShare.None, FileAccess.Read, FileShare.None, false),
                new FileAccessAndFileShare(FileShare.None, FileAccess.Read, FileShare.Read, false),
                new FileAccessAndFileShare(FileShare.None, FileAccess.Read, FileShare.Write, false),
                new FileAccessAndFileShare(FileShare.None, FileAccess.Read, FileShare.ReadWrite, false),
                new FileAccessAndFileShare(FileShare.None, FileAccess.Write, FileShare.None, false),
                new FileAccessAndFileShare(FileShare.None, FileAccess.Write, FileShare.Read, false),
                new FileAccessAndFileShare(FileShare.None, FileAccess.Write, FileShare.Write, false),
                new FileAccessAndFileShare(FileShare.None, FileAccess.Write, FileShare.ReadWrite, false),
                new FileAccessAndFileShare(FileShare.None, FileAccess.ReadWrite, FileShare.None, false),
                new FileAccessAndFileShare(FileShare.None, FileAccess.ReadWrite, FileShare.Read, false),
                new FileAccessAndFileShare(FileShare.None, FileAccess.ReadWrite, FileShare.Write, false),
                new FileAccessAndFileShare(FileShare.None, FileAccess.ReadWrite, FileShare.ReadWrite, false),
                new FileAccessAndFileShare(FileShare.Read, FileAccess.Read, FileShare.None, false),
                new FileAccessAndFileShare(FileShare.Read, FileAccess.Read, FileShare.Read, false),
                new FileAccessAndFileShare(FileShare.Read, FileAccess.Read, FileShare.Write, false),
                new FileAccessAndFileShare(FileShare.Read, FileAccess.Read, FileShare.ReadWrite, true),
                new FileAccessAndFileShare(FileShare.Read, FileAccess.Write, FileShare.None, false),
                new FileAccessAndFileShare(FileShare.Read, FileAccess.Write, FileShare.Read, false),
                new FileAccessAndFileShare(FileShare.Read, FileAccess.Write, FileShare.Write, false),
                new FileAccessAndFileShare(FileShare.Read, FileAccess.Write, FileShare.ReadWrite, false),
                new FileAccessAndFileShare(FileShare.Read, FileAccess.ReadWrite, FileShare.None, false),
                new FileAccessAndFileShare(FileShare.Read, FileAccess.ReadWrite, FileShare.Read, false),
                new FileAccessAndFileShare(FileShare.Read, FileAccess.ReadWrite, FileShare.Write, false),
                new FileAccessAndFileShare(FileShare.Read, FileAccess.ReadWrite, FileShare.ReadWrite, false),
                new FileAccessAndFileShare(FileShare.Write, FileAccess.Read, FileShare.None, false),
                new FileAccessAndFileShare(FileShare.Write, FileAccess.Read, FileShare.Read, false),
                new FileAccessAndFileShare(FileShare.Write, FileAccess.Read, FileShare.Write, false),
                new FileAccessAndFileShare(FileShare.Write, FileAccess.Read, FileShare.ReadWrite, false),
                new FileAccessAndFileShare(FileShare.Write, FileAccess.Write, FileShare.None, false),
                new FileAccessAndFileShare(FileShare.Write, FileAccess.Write, FileShare.Read, false),
                new FileAccessAndFileShare(FileShare.Write, FileAccess.Write, FileShare.Write, false),
                new FileAccessAndFileShare(FileShare.Write, FileAccess.Write, FileShare.ReadWrite, true),
                new FileAccessAndFileShare(FileShare.Write, FileAccess.ReadWrite, FileShare.None, false),
                new FileAccessAndFileShare(FileShare.Write, FileAccess.ReadWrite, FileShare.Read, false),
                new FileAccessAndFileShare(FileShare.Write, FileAccess.ReadWrite, FileShare.Write, false),
                new FileAccessAndFileShare(FileShare.Write, FileAccess.ReadWrite, FileShare.ReadWrite, false),
                new FileAccessAndFileShare(FileShare.ReadWrite, FileAccess.Read, FileShare.None, false),
                new FileAccessAndFileShare(FileShare.ReadWrite, FileAccess.Read, FileShare.Read, false),
                new FileAccessAndFileShare(FileShare.ReadWrite, FileAccess.Read, FileShare.Write, false),
                new FileAccessAndFileShare(FileShare.ReadWrite, FileAccess.Read, FileShare.ReadWrite, true),
                new FileAccessAndFileShare(FileShare.ReadWrite, FileAccess.Write, FileShare.None, false),
                new FileAccessAndFileShare(FileShare.ReadWrite, FileAccess.Write, FileShare.Read, false),
                new FileAccessAndFileShare(FileShare.ReadWrite, FileAccess.Write, FileShare.Write, false),
                new FileAccessAndFileShare(FileShare.ReadWrite, FileAccess.Write, FileShare.ReadWrite, true),
                new FileAccessAndFileShare(FileShare.ReadWrite, FileAccess.ReadWrite, FileShare.None, false),
                new FileAccessAndFileShare(FileShare.ReadWrite, FileAccess.ReadWrite, FileShare.Read, false),
                new FileAccessAndFileShare(FileShare.ReadWrite, FileAccess.ReadWrite, FileShare.Write, false),
                new FileAccessAndFileShare(FileShare.ReadWrite, FileAccess.ReadWrite, FileShare.ReadWrite, true),
            };

            bool pass = true;

            for (int i = 0; i < fafs.Length; i++)
            {
                FileStream fs1 = null, fs2 = null;
                try
                {
                    Log.Comment("Trying " + fafs[i].ToString());

                    fs1 = new FileStream(IOTests.Volume.RootDirectory + "\\abc.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, fafs[i].File1Share);
                    fs2 = new FileStream(IOTests.Volume.RootDirectory + "\\abc.txt", FileMode.OpenOrCreate, fafs[i].File2Access, fafs[i].File2Share);

                    if (fafs[i].ShouldPass == false)
                    {
                        Log.Comment("Should fail, but didn't");
                        pass = false;
                    }
                }
                catch (Exception ex)
                {
                    if (fafs[i].ShouldPass == true)
                    {
                        Log.Exception("Should pass, but didn't", ex);
                        pass = false;
                    }
                }
                finally
                {
                    if (fs1 != null) fs1.Close();
                    if (fs2 != null) fs2.Close();
                }
            }

            return (pass)? MFTestResults.Pass : MFTestResults.Fail;
        }

        public MFTestMethod[] Tests
        {
            get
            {
                return new MFTestMethod[] 
                {
                    new MFTestMethod( Basic_Behavior, "Basic_Behavior" ),
                    new MFTestMethod( Basic_Read_Int, "Basic_Read_Int" ),
                    new MFTestMethod( Basic_ReadBlock, "Basic_ReadBlock" ),
                    new MFTestMethod( Basic_ReadToEnd, "Basic_ReadToEnd" ),
                    new MFTestMethod( Basic_ReadToEnd_Long, "Basic_ReadToEnd_Long" ),
                    new MFTestMethod( Basic_ReadLine, "Basic_ReadLine" ),
                    new MFTestMethod( FileStreamTest_ReadWrite0a, "FileStreamTest_ReadWrite0a" ),
                    new MFTestMethod( FileStreamTest_Seek0, "FileStreamTest_Seek0" ),
                    new MFTestMethod( FileStreamTest_FileAccessAndFileShare, "FileStreamTest_FileAccessAndFileShare" ),
                };
             }
        }
    }
}
