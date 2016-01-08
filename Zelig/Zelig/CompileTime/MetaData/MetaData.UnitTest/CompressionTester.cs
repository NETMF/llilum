//
// Copyright (c) Microsoft Corporation.    All rights reserved.
//

namespace Microsoft.Zelig.MetaData.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Zelig.MetaData;
    using Microsoft.Zelig.Test;

    class CompressionTester : TestBase
    {
        override public TestResult Run( string[] args )
        {
            string currentDir = System.Environment.CurrentDirectory;

            Compress( Expand( currentDir + @"\Test\TestPayload__CLR1_1__VanillaSingleClass.dll" ) );

            foreach(string file in System.IO.Directory.GetFiles( Expand( @"%WINDIR%\Microsoft.NET\Framework\v2.0.50727" ), "*.dll" ))
            {
                try
                {
                    Compress( file );
                }
                catch(Importer.PELoader.IllegalPEFormatException)
                {
                }
                catch(Exception e)
                {
                    Console.WriteLine( "Error while loading {0}: {1}", file, e );
                }
            }

            return TestResult.Pass;
        }

        static string Expand( string file )
        {
            return Environment.ExpandEnvironmentVariables( file );
        }

        static void Compress( string file )
        {
            byte[] image = System.IO.File.ReadAllBytes( file );

            file = System.IO.Path.GetFileName( file );

            byte[] spacedImage = new byte[image.Length * 2];

            for(int i = 0; i < image.Length; i++)
            {
                spacedImage[i*2] = image[i];
            }

            byte[] imageC       = Compress( image       );
            byte[] spacedImageC = Compress( spacedImage );

            Console.WriteLine( "{0,-48} O:{1,7}  C:{2,7}/{3,7} Loss:{4,3}", file, image.Length, imageC.Length, spacedImageC.Length, Math.Round( ((double)spacedImageC.Length / (double)imageC.Length - 1) * 100 ) );
        }

        static byte[] Compress( byte[] buf )
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            System.IO.Compression.GZipStream gzip = new System.IO.Compression.GZipStream( ms, System.IO.Compression.CompressionMode.Compress );

            gzip.Write( buf, 0, buf.Length );
            gzip.Flush();

            return ms.ToArray();
        }
    }
}
