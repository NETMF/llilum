/*---------------------------------------------------------------------
* Master.cs - file description
* Main class, responsible for running all of the other *Tests.cs files
* Version: 1.0
* Author: REDMOND\a-grchat
* Created: 9/4/2007 10:20:51 AM 
* ---------------------------------------------------------------------*/
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

namespace FileSystemTest
{
    public class SetUp : Attribute
    {
    }

    public class TearDown : Attribute
    {
    }

    public class TestMethod : Attribute
    {
    }

    public delegate MFTestResults MFTestDelegate();

    public class MFTestMethod
    {
        public MFTestDelegate Test;
        public string TestName;

        public MFTestMethod( MFTestDelegate test, string testName )
        {
            Test = test;
            TestName = testName;
        }
    }

    public interface IMFTestInterface
    {
        InitializeResult Initialize();
        void CleanUp();
        MFTestMethod[] Tests { get; }
    }

    public enum InitializeResult
    {
        Skip,
        ReadyToGo,
    }

    public enum MFTestResults
    {
        Pass,
        Fail,
        Skip,
        KnownFailure
    }

    public class Log
    {
        public static void Comment( string str )
        {
            Console.WriteLine( "\t\t" + str );
        }
        public static void Exception( string str )
        {
            Console.WriteLine( "\t\tException: " + str );
        }
        public static void Exception( string str, Exception ex )
        {
            Console.WriteLine( "\t\tException: " + str + "\n\t\t" + ex.ToString() );
        }
        public static void FilteredComment( string str )
        {
            Console.WriteLine( "\t\t" + str );
        }
    }

    public class FormatParameters
    {
        public String VolumeName = "";
        public uint Parameter = 0;
        public String Comment = "";
    }

    public class MFUtilities
    {
        public static byte[] GetRandomBytes( int bytes )
        {
            byte[] ret = new byte[bytes];

            Random rand = new Random();

            rand.NextBytes(ret);

            return ret;
        }
    }

    public class IOTests
    {
        static VolumeInfo _volumeInfo = null;
        static FormatParameters[] _volumes = null;
        static int _currentVolume = -1;

        public static void RunTests( IMFTestInterface tst )
        {
            Console.WriteLine( "\n~~~~~ " + tst.GetType().Name + " ~~~~~" );
            tst.Initialize();
            foreach(MFTestMethod tm in tst.Tests)
            {
                try
                {
                    MFTestResults res = tm.Test();

                    string resVal = "";

                    switch(res)
                    {
                        case MFTestResults.Pass:
                            resVal = "\n>>>PASS      : ";
                            break;

                        case MFTestResults.Fail:
                            resVal = "\n>>>FAIL      : ";
                            break;
                        case MFTestResults.Skip:
                            resVal = "\n>>>SKIP      : ";
                            break;
                        case MFTestResults.KnownFailure:
                            resVal = "\n>>>KNOWN FAIL: ";
                            break;

                        default:
                            resVal = "\n>>>BADRESULT: ";
                            break;
                    }

                    Console.WriteLine( resVal + tm.TestName );
                }
                catch
                {
                    Console.WriteLine( "!!!!!Exception while running test " + tm.TestName + "!!!!!" );
                }
            }
            tst.CleanUp();

            GC.Collect();
        }

        public static void Initialize()
        {
            List<FormatParameters> deviceVolumes = new List<FormatParameters>();

            try
            {
                // Get Volumes from device
                foreach (VolumeInfo volume in VolumeInfo.GetVolumes())
                {
                    if (volume.Name == "WINFS")
                    {
                        deviceVolumes.Add(new FormatParameters { VolumeName = "WINFS", Parameter = 0, Comment = "Emulator" });
                    }
                    else
                    {
                        // Do one pass formating FAT16 and one pass formating FAT32
                        deviceVolumes.Add(new FormatParameters { VolumeName = volume.Name, Parameter = 1, Comment = "FAT16" });
                        deviceVolumes.Add(new FormatParameters { VolumeName = volume.Name, Parameter = 2, Comment = "FAT32" });
                    }
                }
            }
            catch
            {
            }

            _volumes = deviceVolumes.ToArray();
            NextVolume();
        }

        public static VolumeInfo Volume
        {
            get
            {
                return _volumeInfo;
            }
        }

        public static VolumeInfo NextVolume()
        {
            _currentVolume++;

            try
            {
                _volumeInfo = new VolumeInfo(_volumes[_currentVolume].VolumeName);
                Log.Comment("The following tests are running on volume " + _volumeInfo.Name + " [" + _volumes[_currentVolume].Comment + "]");
            }
            catch
            {
                _volumeInfo = null;
            }

            return _volumeInfo;
        }
         

        public static void IntializeVolume()
        {
            Log.Comment("Formatting " + Volume.Name + " in " + Volume.FileSystem + " [" + _volumes[_currentVolume].Comment + "]");
            Volume.Format(Volume.FileSystem, _volumes[_currentVolume].Parameter, "TEST_VOL", true);
            Directory.SetCurrentDirectory(Volume.RootDirectory);

            Log.Comment("TestVolumeLabel: " + Volume.VolumeLabel);
        }
    }
}
