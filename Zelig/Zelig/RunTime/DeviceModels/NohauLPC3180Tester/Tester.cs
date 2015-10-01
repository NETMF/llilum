//
// Copyright (c) Microsoft Corporation.   All rights reserved.
//

#define SEND_TEST_PATTERNS
//#define DEBUG_MEMORYPERFORMANCE

//#define TEST_SERIAL
//#define TEST_FLOATINGPOINT
//#define TEST_FLOATINGPOINT_FROM_IRAM
#define TEST_VECTORHACK
//#define TEST_VECTORHACK_MAC
//#define TEST_VECTORHACK_OLD
//#define TEST_DMA
//#define TEST_QUANTIZED_MLP


#if DEBUG_MEMORYPERFORMANCE
#define TEST_VECTORHACK
#endif


namespace Microsoft.NohauLPC3180Tester
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    using RT = Microsoft.Zelig.Runtime;
    using TS = Microsoft.Zelig.Runtime.TypeSystem;

    using Chipset = Microsoft.DeviceModels.Chipset.LPC3180;
    
    class Tester
    {
        [RT.NoInline]
        private static unsafe uint FlushCache()
        {
            uint* ptr = (uint*)0x80000000;
            uint* end = (uint*)0x81000000;
            uint  sum = 0;

            while(ptr < end)
            {
                sum += *ptr++;
            }

            return sum;
        }

#if TEST_FLOATINGPOINT
        [RT.DisableBoundsChecks]
        [RT.DisableNullChecks]
        private static unsafe double FMACD( double* left  ,
                                            double* right ,
                                            double  sum   )
        {
            for(int i = 0; i < 1024; i += 8)
            {
                sum += left[0] * right[0];
                sum += left[1] * right[1];
                sum += left[2] * right[2];
                sum += left[3] * right[3];
                sum += left[4] * right[4];
                sum += left[5] * right[5];
                sum += left[6] * right[6];
                sum += left[7] * right[7];


                left  += 8;
                right += 8;
            }

            return sum;
        }

        [RT.DisableBoundsChecks]
        [RT.DisableNullChecks]
        private static unsafe float FMACS( float* left  ,
                                           float* right ,
                                           float  sum   )
        {
            for(int i = 0; i < 1024; i += 8)
            {
                sum += left[0] * right[0];
                sum += left[1] * right[1];
                sum += left[2] * right[2];
                sum += left[3] * right[3];
                sum += left[4] * right[4];
                sum += left[5] * right[5];
                sum += left[6] * right[6];
                sum += left[7] * right[7];


                left  += 8;
                right += 8;
            }

            return sum;
        }

        [RT.DisableBoundsChecks]
        [RT.DisableNullChecks]
        private static unsafe float FMACS2( float* lPtr ,
                                            float* rPtr ,
                                            float  sum   )
        {
            for(int i = 0; i < 1024; i += 8)
            {
                float lIdx0 = lPtr[0]; float rIdx0 = rPtr[0];
                float lIdx1 = lPtr[1]; float rIdx1 = rPtr[1];
                float lIdx2 = lPtr[2]; float rIdx2 = rPtr[2];
                float lIdx3 = lPtr[3]; float rIdx3 = rPtr[3];
                float lIdx4 = lPtr[4]; float rIdx4 = rPtr[4];
                float lIdx5 = lPtr[5]; float rIdx5 = rPtr[5];
                float lIdx6 = lPtr[6]; float rIdx6 = rPtr[6];
                float lIdx7 = lPtr[7]; float rIdx7 = rPtr[7];

                float mul0 = lIdx0 * rIdx0;
                float mul1 = lIdx1 * rIdx1;
                float mul2 = lIdx2 * rIdx2;
                float mul3 = lIdx3 * rIdx3;
                float mul4 = lIdx4 * rIdx4;
                float mul5 = lIdx5 * rIdx5;
                float mul6 = lIdx6 * rIdx6;
                float mul7 = lIdx7 * rIdx7;

                sum += mul0;
                sum += mul1;
                sum += mul2;
                sum += mul3;
                sum += mul4;
                sum += mul5;
                sum += mul6;
                sum += mul7;

                lPtr += 8;
                rPtr += 8;
            }

            return sum;
        }

        [RT.DisableBoundsChecks]
        [RT.DisableNullChecks]
        private static unsafe int IMAC( int* left  ,
                                        int* right ,
                                        int  sum   )
        {
            for(int i = 0; i < 1024; i += 8)
            {
                sum += left[0] * right[0];
                sum += left[1] * right[1];
                sum += left[2] * right[2];
                sum += left[3] * right[3];
                sum += left[4] * right[4];
                sum += left[5] * right[5];
                sum += left[6] * right[6];
                sum += left[7] * right[7];


                left  += 8;
                right += 8;
            }

            return sum;
        }

        static long valFMACD;
        static long valFMACS;
        static long valFMACS2;
        static long valIMAC;

        private static unsafe void TestFMAC()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

#if TEST_FLOATINGPOINT_FROM_IRAM
            UIntPtr leftBase  = new UIntPtr( 0x08000000 + 64 * 1024 - 1 * 1024 * 8 );
            UIntPtr rightBase = new UIntPtr( 0x08000000 + 64 * 1024 - 2 * 1024 * 8 );
#else
            UIntPtr leftBase  = new UIntPtr( 0x81000000 + 1 * 1024 * 8 );
            UIntPtr rightBase = new UIntPtr( 0x81000000 + 2 * 1024 * 8 );
#endif

            {
                double* left  = (double*)leftBase .ToPointer();
                double* right = (double*)rightBase.ToPointer();

                for(int loop = 0; loop < 1024; loop++)
                {
                    left [loop] = loop + 1024;
                    right[loop] = loop * 13;
                }

                FlushCache();

                stopwatch.Start();
                for(int i = 0; i < 128; i++)
                {
                    double sum = FMACD( left, right, 0 );
                }
                stopwatch.Stop();
                valFMACD = stopwatch.ElapsedTicks;
                stopwatch.Reset();
            }

            {
                float* left  = (float*)leftBase .ToPointer();
                float* right = (float*)rightBase.ToPointer();

                for(int loop = 0; loop < 1024; loop++)
                {
                    left [loop] = loop + 1024;
                    right[loop] = loop * 13;
                }

                FlushCache();

                stopwatch.Start();
                for(int i = 0; i < 128; i++)
                {
                    float sum = FMACS( left, right, 0 );
                }
                stopwatch.Stop();
                valFMACS = stopwatch.ElapsedTicks;
                stopwatch.Reset();
            }

            {
                float* left  = (float*)leftBase .ToPointer();
                float* right = (float*)rightBase.ToPointer();

                for(int loop = 0; loop < 1024; loop++)
                {
                    left [loop] = loop + 1024;
                    right[loop] = loop * 13;
                }

                FlushCache();

                stopwatch.Start();
                for(int i = 0; i < 128; i++)
                {
                    float sum = FMACS2( left, right, 0 );
                }
                stopwatch.Stop();
                valFMACS2 = stopwatch.ElapsedTicks;
                stopwatch.Reset();
            }

            {
                int* left  = (int*)leftBase .ToPointer();
                int* right = (int*)rightBase.ToPointer();

                for(int loop = 0; loop < 1024; loop++)
                {
                    left [loop] = loop + 1024;
                    right[loop] = loop * 13;
                }

                FlushCache();

                stopwatch.Start();
                for(int i = 0; i < 128; i++)
                {
                    int sum = IMAC( left, right, 0 );
                }
                stopwatch.Stop();
                valIMAC = stopwatch.ElapsedTicks;
                stopwatch.Reset();
            }
        }
#endif

#if TEST_VECTORHACK

        static float   valFMACS_sum1;
        static float   valFMACS_sum2;
        static double  valFMACS_time1;
        static long    valFMACS_time2;
        static double  valFMACS_timePerIter;
        static float[] s_A = new float[512];
        static float[] s_B = new float[1024];

#if TEST_VECTORHACK_OLD
        [RT.DisableNullChecks()]
        private static unsafe float MultiplyAndAccumulate( float* Abase      ,
                                                           float* Bbase      ,
                                                           int    N          ,
                                                           int    vectorSize )
        {
            float c = 0.0f;

            float* Aptr =  Abase;
            float* Bptr =  Bbase;
            float* Aend = &Aptr[N & ~7];

            float C1 = 0;
            float C2 = 0;
            float C3 = 0;
            float C4 = 0;
            float C5 = 0;
            float C6 = 0;
            float C7 = 0;
            float C8 = 0;

            while(Aptr < Aend)
            {
                float A1 = Aptr[0];
                float A2 = Aptr[1];
                float A3 = Aptr[2];
                float A4 = Aptr[3];
                float A5 = Aptr[4];
                float A6 = Aptr[5];
                float A7 = Aptr[6];
                float A8 = Aptr[7];

                float B1 = Bptr[0];
                float B2 = Bptr[1];
                float B3 = Bptr[2];
                float B4 = Bptr[3];
                float B5 = Bptr[4];
                float B6 = Bptr[5];
                float B7 = Bptr[6];
                float B8 = Bptr[7];

                float M1 = A1 * B1;
                float M2 = A2 * B2;
                float M3 = A3 * B3;
                float M4 = A4 * B4;
                float M5 = A5 * B5;
                float M6 = A6 * B6;
                float M7 = A7 * B7;
                float M8 = A8 * B8;

                C1 += M1;
                C2 += M2;
                C3 += M3;
                C4 += M4;
                C5 += M5;
                C6 += M6;
                C7 += M7;
                C8 += M8;

                Aptr += 8;
                Bptr += 8;
            }

            C1 += C5;
            C2 += C6;
            C3 += C7;
            C4 += C8;

            C1 += C3;
            C2 += C4;

            c  = C1 + C2;

            return c;
        }
#else
        [TS.WellKnownMethod( "Solo_DSP_MatrixMultiply__MultiplyAndAccumulate" )]
        [MethodImpl( MethodImplOptions.InternalCall )]
        private static unsafe extern float MultiplyAndAccumulate( float* Abase      ,
                                                                  float* Bbase      ,
                                                                  int    N          ,
                                                                  int    vectorSize );
#endif

        private static unsafe void TestVectorHack()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

#if TEST_FLOATINGPOINT_FROM_IRAM
            UIntPtr leftBase  = new UIntPtr( 0x08000000 + 64 * 1024 - 1 * 1024 * 8 );
            UIntPtr rightBase = new UIntPtr( 0x08000000 + 64 * 1024 - 2 * 1024 * 8 );
#else
            UIntPtr leftBase  = new UIntPtr( 0x81000000 + 1 * 1024 * 8 );
            UIntPtr rightBase = new UIntPtr( 0x81000000 + 2 * 1024 * 8 );
#endif

            {
                float* left  = (float*)leftBase .ToPointer();
                float* right = (float*)rightBase.ToPointer();

                for(int loop = 0; loop < 1024; loop++)
                {
                    left [loop] = loop + 1024;
                    right[loop] = loop * 13;
                }

                FlushCache();

                stopwatch.Start();
                for(int i = 0; i < 128; i++)
                {
                    valFMACS_sum1 = MultiplyAndAccumulate( left, right, 1024, 8 );
                }
                stopwatch.Stop();
                valFMACS_time1 = stopwatch.ElapsedTicks;
                stopwatch.Reset();
            }

            {
                float* left  = (float*)leftBase .ToPointer();
                float* right = (float*)rightBase.ToPointer();

                for(int loop = 0; loop < 1024; loop++)
                {
                    left [loop] = loop + 1024;
                    right[loop] = loop * 13;
                }

                FlushCache();

                stopwatch.Start();
                valFMACS_sum2 = MultiplyAndAccumulate( left, right, 1024, 8 );
                stopwatch.Stop();
                valFMACS_time2 = stopwatch.ElapsedTicks;
                stopwatch.Reset();
            }
        }

        static public unsafe float AccumulateRowByColumn( float[] A       ,
                                                          int     Aoffset ,
                                                          float[] B       ,
                                                          int     Boffset ,
                                                          int     N       )
        {
            fixed(float* Abase = &A[Aoffset])
            {
                fixed(float* Bbase = &B[Boffset])
                {
                    const int VectorStep = 8;

                    int Nrounded = N & ~(VectorStep-1);

////                Chipset.GPIO gpio = Chipset.GPIO.Instance;
////
////                gpio.SetGPO( 13 );
                    float res = MultiplyAndAccumulate( Abase, Bbase, Nrounded, VectorStep );
////                gpio.ResetGPO( 13 );

                    if(Nrounded < N)
                    {
                        float* Aptr = &Abase[Nrounded];
                        float* Bptr = &Bbase[Nrounded];

                        while(Nrounded < N)
                        {
                            res += *Aptr++ * *Bptr++;

                            Nrounded++;
                        }
                    }

                    return res;
                }
            }
        }

        private static unsafe void TestVectorHack2( double[] results ,
                                                    int      runs    ,
                                                    int      K       ,
                                                    int      N       )
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            const int M       = 1;
#if TEST_ROUND_SIZE
            const int K       = 256;
            const int N       = 512;
#elif TEST_CLASSIC_MLP
            const int K       = 226;
            const int N       = 500;
#else
////        const int K       = 159;
////        const int N       = 50;
#endif
            int Astride = K;
            int Bstride = K;
            int Cstride = N;

            float[] A = new float[K  ];
            float[] B = new float[K*N];
            float[] C = new float[  N];

            //--//

            for(int i = 0; i < K; i++)
            {
                A[i] = 1;
            }

            for(int i = 0; i < N; i++)
            {
                for(int j = 0; j < K; j++)
                {
                    B[i * K + j] = i;
                }
            }

            //--//

            for(int run = 1; run < runs; run++)
            {
                FlushCache();

                stopwatch.Start();

                for(int run2 = 0; run2 < run; run2++)
                {
                    for (int i = 0; i < M; i++)
                    {
                        for (int j = 0; j < N; j++)
                        {
                            C[i * Cstride + j] = AccumulateRowByColumn( A, i * Astride, B, j * Bstride, K );
                        }
                    }
                }

                stopwatch.Stop();

                results[run] = (stopwatch.ElapsedTicks / (double)System.Diagnostics.Stopwatch.Frequency * 1E9) / (N * K) / run;

                stopwatch.Reset();
            }
        }

        public static float Exp( float x )
        {
            const float Exp           = (float)Math.E;
            const float ExpHalfFactor = 1.648721f;
            const float ExpStep1      = 0.00138888f;
            const float ExpStep2      = 0.00833333f;
            const float ExpStep3      = 0.04166666f;
            const float ExpStep4      = 0.16666666f;
            const float ExpStep5      = 0.5f;
            const float ExpStep6      = 1.0f;

            int sign;

            // Reduce range to [0.0,1.0] 
            if (x < 0)
            {
                x    = -x;
                sign = -1;
            }
            else
            {
                sign = 1;
            }

            float result = 1.0f;

            while(x > 1.0f)
            {
                x      -= 1.0f;
                result *= Exp;
            }

            // Reduce range to [0.0,0.5] 
            if (x > 0.5f)
            {
                x      -= 0.5f;
                result *= ExpHalfFactor;
            }

            float temp;

            temp =         ExpStep1  * x;
            temp = (temp + ExpStep2) * x;
            temp = (temp + ExpStep3) * x;
            temp = (temp + ExpStep4) * x;
            temp = (temp + ExpStep5) * x;
            temp = (temp + ExpStep6) * x;

            result *= (temp + 1.0f);

            if (sign == -1)
            {
                result = 1.0f / result;
            }

            return result;
        }

        private static unsafe double[] TestVectorHack2( int runs )
        {
            double[] results = new double[runs];

////        TestVectorHack2( results, runs, 159, 30  );
////        TestVectorHack2( results, runs, 159, 50  );
////        TestVectorHack2( results, runs, 226, 500 );

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            stopwatch.Start();

            for(int i = 0; i < 1000; i++)
            {
                Math.Exp( 0.5 );
            }

            stopwatch.Stop();

            results[0] = (stopwatch.ElapsedTicks / (double)System.Diagnostics.Stopwatch.Frequency * 1E9) / 1000;

            stopwatch.Reset();
            stopwatch.Start();

            for(int i = 0; i < 1000; i++)
            {
                Exp( 0.5f );
            }

            stopwatch.Stop();

            results[1] = (stopwatch.ElapsedTicks / (double)System.Diagnostics.Stopwatch.Frequency * 1E9) / 1000;


            TestVectorHack2( results, runs, 160, 30  );
            TestVectorHack2( results, runs, 160, 50  );
            TestVectorHack2( results, runs, 232, 500 );
            TestVectorHack2( results, runs, 160, 30  );

            return results;
        }

        [RT.DisableNullChecks()]
        private static unsafe float TestVectorHack3()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            float[] B = s_B;//new float[8192];

            while(true)
            {
                stopwatch.Start();

////            for(int i = 0; i < 128; i++)
////            {
////                fixed(float* Bbase = &B[0])
////                {
////                    float* Bptr = Bbase;
////                    float* Bend = Bbase + B.Length;
////                    float  res1 = 0;
////                    float  res2 = 0;
////                    float  res3 = 0;
////                    float  res4 = 0;
////                    float  res5 = 0;
////                    float  res6 = 0;
////                    float  res7 = 0;
////                    float  res8 = 0;
////
////                    while(Bptr < Bend)
////                    {
////                        res1 = Bptr[0];
////                        res2 = Bptr[1];
////                        res3 = Bptr[2];
////                        res4 = Bptr[3];
////                        res5 = Bptr[4];
////                        res6 = Bptr[5];
////                        res7 = Bptr[6];
////                        res8 = Bptr[7];
////
////                        Bptr += 8;
////                    }
////
////                    valFMACS_sum1 = res1 + res2 + res3 + res4 + res5 + res6 + res7 + res8;
////                }
////            }
    
                int res1 = 1;
////            int add  = 2;

                int* ptr = (int*)0x80000000;

                for(int i = 0; i < 128*1024; i++)
                {
                    res1 += *ptr;

                    //ptr++;
                }
    
                stopwatch.Stop();

                valFMACS_time1 = stopwatch.ElapsedTicks;
                stopwatch.Reset();
            }
        }

        [RT.DisableNullChecks]
        private static unsafe void TestVectorHack4( float* Abase ,
                                                    float* Bbase )
        {
////        const int VectorStep = 8;

            for(int count = 0; count < 16; count++)
            {
                float* Bptr = Bbase;
                for(int i = 0; i < 512; i++)
                {
////                MultiplyAndAccumulate( Abase, Bptr, 512, VectorStep );

                    Chipset.GPIO gpio = Chipset.GPIO.Instance;

                    float res = 0;
////                float add = 1;

                    gpio.SetGPO( 13 );
                    for(int j = 0; j < 32; j++)
                    {
////                    res += add;
////                    gpio.SetGPO( 9 );
                        res = *Bptr;
////                    gpio.ResetGPO( 9 );
                        Bptr += 8;
                    }
                    gpio.ResetGPO( 13 );
                    
                    Abase[i] = res;

////                Bptr += 32;
                }
            }
        }

        [RT.DisableNullChecks]
        private static unsafe void TestVectorHack4b( float* Abase ,
                                                     float* Bbase )
        {
            const int VectorStep = 8;

            for(int count = 0; count < 16; count++)
            {
                const int stepReduction = 1;

                float* Bptr = Bbase;

                for(int i = 0; i < 512 * stepReduction; i++)
                {
                    Chipset.GPIO gpio = Chipset.GPIO.Instance;

                    gpio.SetGPO( 13 );
                    MultiplyAndAccumulate( Abase, Bptr, 512 / stepReduction, VectorStep );
                    gpio.ResetGPO( 13 );

                    Bptr += 512 / stepReduction;
                }
            }
        }

        [RT.DisableNullChecks]
        private static unsafe void TestVectorHack4c( float* Abase ,
                                                     float* Bbase )
        {
            for(int count = 0; count < 16; count++)
            {
                uint* Bptr = (uint*)Bbase;

                for(int i = 0; i < 512; i++)
                {
                    Chipset.GPIO gpio = Chipset.GPIO.Instance;

                    uint res = 0;

                    gpio.SetGPO( 13 );
                    for(int j = 0; j < 512/64; j++)
                    {
                        gpio.SetGPO( 9 );
                        gpio.ResetGPO( 9 );
                        gpio.SetGPO( 9 );
                        gpio.ResetGPO( 9 );
                        gpio.SetGPO( 9 );

                        res += *Bptr;
                        Bptr += 8;
                        res += *Bptr;
                        Bptr += 8;
                        res += *Bptr;
                        Bptr += 8;
                        res += *Bptr;
                        Bptr += 8;
                        res += *Bptr;
                        Bptr += 8;
                        res += *Bptr;
                        Bptr += 8;
                        res += *Bptr;
                        Bptr += 8;
                        res += *Bptr;
                        Bptr += 8;

                        gpio.ResetGPO( 9 );
                    }
                    gpio.ResetGPO( 13 );
                    
                    Abase[i] = (res + 1);
                }
            }
        }

        private static unsafe void TestVectorHack4()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            float[] A = s_A;//new float[512    ];
            float[] B = new float[512*512];
            float[] C = new float[    512];

            FlushCache();

////        //
////        // Invalidate ICache.
////        //
////        Microsoft.Zelig.Runtime.TargetPlatform.ARMv4.ProcessorARMv4.MoveToCoprocessor( 15, 0, 7, 5, 0, 0 );

            while(true)
            {
                stopwatch.Start();

////            TestVectorHack4( (float*)0x80000000, (float*)(0x80000000 + 512 * 4) );

                fixed(float* Abase = &A[0])
                {
                    fixed(float* Bbase = &B[0])
                    {
                        TestVectorHack4b( Abase, Bbase );
////                    TestVectorHack4c( Abase, (float*)0x80000000 );
                    }
                }

                stopwatch.Stop();

                valFMACS_timePerIter  = stopwatch.ElapsedTicks * 16.0;
                valFMACS_timePerIter /= (16.0 * 512.0 * 512.0);
                stopwatch.Reset();
            }
        }
#endif

#if TEST_DMA

        static double valDMA_time1;

        [RT.AlignmentRequirements( 32, sizeof(uint) )] static uint[] s_IRAM_src = new uint[512 + 32];
        [RT.AlignmentRequirements( 32, sizeof(uint) )] static uint[] s_IRAM_dst = new uint[512 + 32];

        private static unsafe void TestDMA( uint* SrcBase    ,
                                            uint* DstBase    ,
                                            bool  fIncrement )
        {
            Chipset.GPDMA dma = Chipset.GPDMA.Instance;

            dma.Enable();

            Chipset.GPDMA.Channel chn = dma.Channels[0];

            uint* SrcPtr = SrcBase;

            System.Diagnostics.Stopwatch stopwatch1 = new System.Diagnostics.Stopwatch();
            System.Diagnostics.Stopwatch stopwatch2 = new System.Diagnostics.Stopwatch();

            for(int i = 0; i < 512; i++)
            {
////            Chipset.GPIO gpio = Chipset.GPIO.Instance;

                stopwatch1.Start();
                stopwatch1.Stop();

                stopwatch2.Start();
////            gpio.SetGPO( 13 );

                chn.CopyMemory( SrcPtr, DstBase, 512, 32, true, true, false );

////            gpio.SetGPO( 9 );
////            chn.WaitForCompletion();
////            gpio.ResetGPO( 9 );
////
////            gpio.ResetGPO( 13 );
                stopwatch2.Stop();

                valDMA_time1  = (stopwatch2.ElapsedTicks - stopwatch1.ElapsedTicks) * 16.0;
                valDMA_time1 /= 208.0;

                stopwatch1.Reset();
                stopwatch2.Reset();
             
                if(fIncrement)
                {
                    SrcPtr += 512;
                }
            }

            chn.WaitForCompletion();

            dma.Disable();
        }

        enum Tests
        {
            IRAM_to_IRAM  ,
            SDRAM_to_IRAM ,
            SDRAM_to_SDRAM,
            IRAM_to_SDRAM ,
        }

        private static unsafe void TestDMA()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            Tests test = Tests.IRAM_to_IRAM;

            uint[] Src;
            uint[] Dst;
            bool   fIncrementSrc;

            switch(test)
            {
                default:
                case Tests.IRAM_to_IRAM:
                    Src           = s_IRAM_src;
                    Dst           = s_IRAM_dst;
                    fIncrementSrc = false;
                    break;

                case Tests.IRAM_to_SDRAM:
                    Src           = s_IRAM_src;
                    Dst           = new uint[512];
                    fIncrementSrc = false;
                    break;

                case Tests.SDRAM_to_SDRAM:
                    Src           = new uint[512*512];
                    Dst           = new uint[512];
                    fIncrementSrc = true;
                    break;

                case Tests.SDRAM_to_IRAM:
                    Src           = new uint[512*512];
                    Dst           = s_IRAM_dst;
                    fIncrementSrc = true;
                    break;
            }

            for(int i = 0; i < Src.Length; i++)
            {
                Src[i] = 0xDEADBEEF;
            }

            for(int i = 0; i < Dst.Length; i++)
            {
                Dst[i] = 0xBAD00000u + (uint)i;
            }

            FlushCache();

////        //
////        // Invalidate ICache.
////        //
////        Microsoft.Zelig.Runtime.TargetPlatform.ARMv4.ProcessorARMv4.MoveToCoprocessor( 15, 0, 7, 5, 0, 0 );

            while(true)
            {
                stopwatch.Start();

                fixed(uint* SrcBase = &Src[0])
                {
                    fixed(uint* DstBbase = &Dst[0])
                    {
                        TestDMA( SrcBase, DstBbase, fIncrementSrc );
                    }
                }

                stopwatch.Stop();

                FlushCache();

                //valDMA_time1 = stopwatch.ElapsedTicks;
                stopwatch.Reset();
            }
        }
#endif

#if TEST_QUANTIZED_MLP

        static double valMLP_time1;

        const int M       = 1;
        const int K       = 256;
        const int N       = 512;
        const int Astride = K;
        const int Bstride = K;
        const int Cstride = N;

        [RT.DisableNullChecks()]
        [RT.DisableBoundsChecks()]
        static unsafe float AccumulateRowByColumn( float[] A       ,
                                                   int     Aoffset ,
                                                   uint[]  B       ,
                                                   int     Boffset ,
                                                   int     N       ,
                                                   float[] Lookup  )
        {
            fixed(float* Abase = &A[Aoffset])
            {
                fixed(uint* Bbase = &B[Boffset])
                {
                    fixed(float* Lptr = Lookup)
                    {
                        float* Aptr    =  Abase;
                        float* AptrEnd = &Abase[N];
                        uint*  Bptr    =  Bbase;
                        float  res1    =  0;
                        float  res2    =  0;
                        float  res3    =  0;
                        float  res4    =  0;
                        float  res5    =  0;
                        float  res6    =  0;
                        float  res7    =  0;
                        float  res8    =  0;

                        while(Aptr < AptrEnd)
                        {
                            uint  val = Bptr[0];
                            float V1  = *(float*)((uint)Lptr + ((val <<     2 ) & 0x3FC));
                            float V2  = *(float*)((uint)Lptr + ((val >> ( 8-2)) & 0x3FC));
                            float V3  = *(float*)((uint)Lptr + ((val >> (16-2)) & 0x3FC));
                            float V4  = *(float*)((uint)Lptr + ((val >> (24-2)) & 0x3FC));
                            
                            val = Bptr[1];
                            float V5  = *(float*)((uint)Lptr + ((val <<     2 ) & 0x3FC));
                            float V6  = *(float*)((uint)Lptr + ((val >> ( 8-2)) & 0x3FC));
                            float V7  = *(float*)((uint)Lptr + ((val >> (16-2)) & 0x3FC));
                            float V8  = *(float*)((uint)Lptr + ((val >> (24-2)) & 0x3FC));

                            V1 *= Aptr[0];
                            V2 *= Aptr[1];
                            V3 *= Aptr[2];
                            V4 *= Aptr[3];
                            V5 *= Aptr[4];
                            V6 *= Aptr[5];
                            V7 *= Aptr[6];
                            V8 *= Aptr[7];

                            res1 += V1;
                            res2 += V2;
                            res3 += V3;
                            res4 += V4;
                            res5 += V5;
                            res6 += V6;
                            res7 += V7;
                            res8 += V8;

                            Bptr += 2;
                            Aptr += 8;
                        }

                        return res1 + res2 + res3 + res4 + res5 + res6 + res7 + res8;
                    }
                }
            }
        }

        private static unsafe void TestQuantizedMLP()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            float[] Lookup = new float[256];
            float[] A      = new float[K];
            uint[]  B      = new uint [K * N / sizeof(uint)];
            float[] C      = new float[N];

            for(int i = 0; i < Lookup.Length; i++)
            {
                Lookup[i] = i;
            }

            for(int i = 0; i < K; i++)
            {
                A[i] = 1;
            }

            for(int i = 0; i < B.Length; i++)
            {
                uint weight = 0;
                
                for(int j = 0; j < 4; j++)
                {
                    int val = 33477 * (i * 4 + j) % 12397;

                    weight |= (uint)(val & 0xFF) << (j * 8);
                }

                B[i] = weight;
            }

            FlushCache();

            //--//

            while(true)
            {
                stopwatch.Start();

                for(int count = 0; count < 16; count++)
                {
                    for (int i = 0; i < M; i++)
                    {
                        for (int j = 0; j < N; j++)
                        {
                            C[i * Cstride + j] = AccumulateRowByColumn( A, i * Astride, B, j * Bstride, K, Lookup );
                        }
                    }
                }

                stopwatch.Stop();

                valMLP_time1 = (stopwatch.ElapsedTicks * 16.0) / (16.0 * K * N);
                stopwatch.Reset();
            }
        }
#endif

#if TEST_QUANTIZED_MLP2

        static double valMLP_time1;

        const int M       = 1;
        const int K       = 256;
        const int N       = 512;
        const int Astride = K;
        const int Bstride = K;
        const int Cstride = N;

        [RT.DisableNullChecks()]
        [RT.DisableBoundsChecks()]
        static unsafe float AccumulateRowByColumn( float[] A       ,
                                                   int     Aoffset ,
                                                   byte[]  B       ,
                                                   int     Boffset ,
                                                   int     N       ,
                                                   float[] Lookup  )
        {
            fixed(float* Abase = &A[Aoffset])
            {
                fixed(byte* Bbase = &B[Boffset])
                {
                    fixed(float* Lptr = Lookup)
                    {
                        float* Aptr    =  Abase;
                        float* AptrEnd = &Abase[N];
                        byte*  Bptr    =  Bbase;
                        float  res1    =  0;
                        float  res2    =  0;
                        float  res3    =  0;
                        float  res4    =  0;
                        float  res5    =  0;
                        float  res6    =  0;
                        float  res7    =  0;
                        float  res8    =  0;

                        while(Aptr < AptrEnd)
                        {
                            float V1 = Lptr[Bptr[0]];
                            float V2 = Lptr[Bptr[1]];
                            float V3 = Lptr[Bptr[2]];
                            float V4 = Lptr[Bptr[3]];
                            float V5 = Lptr[Bptr[4]];
                            float V6 = Lptr[Bptr[5]];
                            float V7 = Lptr[Bptr[6]];
                            float V8 = Lptr[Bptr[7]];

                            V1 *= Aptr[0];
                            V2 *= Aptr[1];
                            V3 *= Aptr[2];
                            V4 *= Aptr[3];
                            V5 *= Aptr[4];
                            V6 *= Aptr[5];
                            V7 *= Aptr[6];
                            V8 *= Aptr[7];

                            res1 += V1;
                            res2 += V2;
                            res3 += V3;
                            res4 += V4;
                            res5 += V5;
                            res6 += V6;
                            res7 += V7;
                            res8 += V8;

                            Bptr += 8;
                            Aptr += 8;
                        }

                        return res1 + res2 + res3 + res4 + res5 + res6 + res7 + res8;
                    }
                }
            }
        }

        private static unsafe void TestQuantizedMLP()
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

            float[] Lookup = new float[256];
            float[] A      = new float[K];
            byte[]  B      = new byte[K * N];
            float[] C      = new float[N];

            for(int i = 0; i < Lookup.Length; i++)
            {
                Lookup[i] = i;
            }

            for(int i = 0; i < K; i++)
            {
                A[i] = 1;
            }

            for(int i = 0; i < B.Length; i++)
            {
                B[i] = (byte)(33477 * (i) % 12397);
            }

            FlushCache();

            //--//

            while(true)
            {
                stopwatch.Start();

                for(int count = 0; count < 16; count++)
                {
                    for (int i = 0; i < M; i++)
                    {
                        for (int j = 0; j < N; j++)
                        {
                            C[i * Cstride + j] = AccumulateRowByColumn( A, i * Astride, B, j * Bstride, K, Lookup );
                        }
                    }
                }

                stopwatch.Stop();

                valMLP_time1 = (stopwatch.ElapsedTicks * 16) / (16 * K * N);
                stopwatch.Reset();
            }
        }
#endif

#if TEST_SERIAL
        static void TestSerial()
        {
            var port = new System.IO.Ports.SerialPort( "UART5" );

            port.BaudRate        = 230400 * 2;
            port.ReadBufferSize  = 256;
            port.WriteBufferSize = 256;

            port.Open();

            port.Write( "Hello World!" );

            byte[] writeBuf = new byte[1];

            while(true)
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    
                stopwatch.Start();
    
                const int total = 32 * 1024;
    
                int ch = 0;
    
                for(int j = 0; j < total; j++)
                {
                    writeBuf[0] = (byte)(48 + ch);

                    port.Write( writeBuf, 0, 1 );
    
                    ch = (ch + 1) % 32;
                }
    
                stopwatch.Stop();
    
                port.Write( "\r\n" );
                port.Write( "\r\n" );
    
                port.Write( string.Format( "Took {0} to send {1} bytes", stopwatch.ElapsedMilliseconds, total ) );
    
////            byte[] buf = new byte[64];
////
////            while(true)
////            {
////                for(int i = 0; i < 64; i++)
////                {
////                    buf[i] = port.Read();
////                }
////
////                //port.Write( (byte)'>' );
////                for(int i = 0; i < 64; i++)
////                {
////                    port.Write( buf[i] );
////                }
////                //port.Write( (byte)'<' );
////            }
    
                GC.Collect();
            }
        }
#endif

        static void Main()
        {
#if TEST_SERIAL

            TestSerial();

#elif TEST_FLOATINGPOINT

            TestFMAC();

#elif TEST_VECTORHACK

            TestVectorHack2( 30 );
////        TestVectorHack4();

#elif TEST_DMA

            TestDMA();

#elif TEST_QUANTIZED_MLP

            TestQuantizedMLP();

#else

            var bc = new Microsoft.NohauLPC3180.Drivers.BelaChannel();

            short[] samples = Microsoft.NohauLPC3180.Drivers.BelaChannel.AllocateSamplesArray();

#if SEND_TEST_PATTERNS
////        short[]  testPatterns = new short[]
////        { 0,2047,4095,6143,8191,10239,12287,14335,16383,18431,20479,22527,24575,26623,28671,30719,
////        30719,28671,26623,24575,22527,20479,18431,16383,14335,12287,10239,8191,6143,4095,2047,0,
////        0,-2047,-4095,-6143,-8191,-10239,-12287,-14335,-16383,-18431,-20479,-22527,-24575,-26623,-28671,-30719,
////        -30719,-28671,-26623,-24575,-22527,-20479,-18431,-16383,-14335,-12287,-10239,-8191,-6143,-4095,-2047,0,
////        16384,16384,-16384,-16384,16384,16384,-16384,-16384,16384,16384,-16384,-16384,16384,16384,-16384,-16384 };

            short[]  testPatterns = new short[80];

            for(int i = 0; i < 80; i++)
            {
                testPatterns[i] = (short)((2*i+1) * 256 + (2*i));
            }
#endif

            bc.RequestAttention();

            bc.DebugSuccess( "Hello World!" );

            int frames = 0;

#if SEND_TEST_PATTERNS
            bc.QueueAudio( testPatterns );
#endif

            while(true)
            {
                short[] samplesN  = samples;

#if SEND_TEST_PATTERNS
                bc.QueueAudio( testPatterns );
#endif

                bc.FetchSamples( samplesN );

                frames++;

                if((frames % 100) == 0)
                {
                    bc.DebugInformational( string.Format( "Got Frames: {0}", frames ) );
                }

                bc.RequestSamples = true;
            }

////        Bluetooth bt = new Bluetooth();
////
////        bt.Connection();
////
////        while(true);
#endif
        }
    }
}
