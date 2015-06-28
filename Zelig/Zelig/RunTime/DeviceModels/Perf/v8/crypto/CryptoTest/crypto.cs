// Comment this out to use a fixed random number seed.
#define OUTLINE_LISTX_EXTEND
//#define USE_RANDOM_SEED
﻿/*
 * Copyright (c) 2003-2005  Tom Wu
 * All Rights Reserved.
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS-IS" AND WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS, IMPLIED OR OTHERWISE, INCLUDING WITHOUT LIMITATION, ANY
 * WARRANTY OF MERCHANTABILITY OR FITNESS FOR A PARTICULAR PURPOSE.
 *
 * IN NO EVENT SHALL TOM WU BE LIABLE FOR ANY SPECIAL, INCIDENTAL,
 * INDIRECT OR CONSEQUENTIAL DAMAGES OF ANY KIND, OR ANY DAMAGES WHATSOEVER
 * RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER OR NOT ADVISED OF
 * THE POSSIBILITY OF DAMAGE, AND ON ANY THEORY OF LIABILITY, ARISING OUT
 * OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 *
 * In addition, the following condition applies:
 *
 * All redistributions must retain an intact copy of this copyright notice
 * and disclaimer.
 */


// The code has been adapted for use as a benchmark by Microsoft.

using System;
using System.Collections.Generic;
using System.Globalization;
// using System.Diagnostics;
// using System.Text.RegularExpressions;

namespace Crypto
{

class Support {
    public static void Main() { //(String[] args) {
        int n = 10;
        //if (args.Length > 0) {
        //    n = Int32.Parse(args[0]);
        //}
        bool verbose = true;
        //if (args.Length > 1)
        //{
        //    switch (args[1])
        //    {
        //    case "verbose":
        //        verbose = true;
        //        break; 
        //    default:
        //        Console.WriteLine("Bad arg: '{0}'.\n", args[1]);
        //        return;
        //    }
        //}
        Measure(n, verbose);
    }

    public static void Measure(int n, bool verbose) {
        DateTime start = DateTime.Now;
        Setup();
        for (int i = 0; i < n; i++) {
            runEncrypt(verbose);
            runDecrypt(verbose);
        }
        DateTime end = DateTime.Now;
        TimeSpan dur = end - start;
        Console.WriteLine("Doing {0} iters of Crytpo takes {1} ms; {2} usec/iter.",
                          n, dur.TotalMilliseconds, dur.TotalMilliseconds*1000 / n);
    }

    static RSAKey RSA;
    static String TEXT;

    static void Setup() {
        String nValue="a5261939975948bb7a58dffe5ff54e65f0498f9175f5a09288810b8975871e99af3b5dd94057b0fc07535f5f97444504fa35169d461d0d30cf0192e307727c065168c788771c561a9400fb49175e9e6aa4e23fe11af69e9412dd23b0cb6684c4c2429bce139e848ab26d0829073351f4acd36074eafd036a5eb83359d2a698d3";
        String eValue="10001";
        String dValue="8e9912f6d3645894e8d38cb58c0db81ff516cf4c7e5a14c7f1eddb1459d2cded4d8d293fc97aee6aefb861859c8b6a3d1dfe710463e1f9ddc72048c09751971c4a580aa51eb523357a3cc48d31cfad1d4a165066ed92d4748fb6571211da5cb14bc11b6e2df7c1a559e6d5ac1cd5c94703a22891464fba23d0d965086277a161";
        String pValue="d090ce58a92c75233a6486cb0a9209bf3583b64f540c76f5294bb97d285eed33aec220bde14b2417951178ac152ceab6da7090905b478195498b352048f15e7d";
        String qValue="cab575dc652bb66df15a0359609d51d1db184750c00c6698b90ef3465c99655103edbf0d54c56aec0ce3c4d22592338092a126a0cc49f65a4a30d222b411e58f";
        String dmp1Value="1a24bca8e273df2f0e47c199bbf678604e7df7215480c77c8db39f49b000ce2cf7500038acfff5433b7d582a01f1826e6f4d42e1c57f5e1fef7b12aabc59fd25";
        String dmq1Value="3d06982efbbe47339e1f6d36b1216b8a741d410b0c662f54f7118b27b9a4ec9d914337eb39841d8666f3034408cf94f5b62f11c402fc994fe15a05493150d9fd";
        String coeffValue="3a3e731acd8960b7ff9eb81a7ff93bd1cfa74cbd56987db58b4594fb09c09084db1734c8143f98b602b981aaa9243ca28deb69b5b280ee8dcee0fd2625e53250";

        BigInteger.setupEngine(new BigInteger.AMSig(BigInteger.am3), 28);

        RSA = new RSAKey();
        RSA.setPublic(nValue, eValue);
        RSA.setPrivateEx(nValue, eValue, dValue, pValue, qValue, dmp1Value, dmq1Value, coeffValue);

        TEXT = "The quick brown fox jumped over the extremely lazy frogs!";
    }

    public static void runEncrypt(bool verbose) {
        var res = RSA.encrypt(TEXT);
        if (verbose) Console.WriteLine("encrypt '{0}' is '{1}'", TEXT, res);
        TEXT = res;
    }
    public static void runDecrypt(bool verbose) {
        var res = RSA.decrypt(TEXT);
        if (verbose) Console.WriteLine("decrypt '{0}' is '{1}'", TEXT, res);
        TEXT = res;
    }
}

class ListX<T> : List<T>
{
    public ListX() : base() {}
    public ListX(int cap) : base(cap) {}

#if OUTLINE_LISTX_EXTEND
    private void ExtendTo(int index)
    {
        for(int j = Count; j < index; j++)
        {
            base.Add( default( T ) );
        }
    }
#endif

    public new T this[int index]
    {
        get { return base[index]; }
        set
        {
            if (index < Count)
            {
                base[index] = value;
            }
            else
            {
#if OUTLINE_LISTX_EXTEND
                ExtendTo(index);
#else
                for (int j = Count; j < index; j++)
                {
                    base.Add(default(T));
                }
#endif
                base.Add(value);
            }
        }
    }
}

// Basic JavaScript BN library - subset useful for RSA encryption.

class BigInteger
{
    ListX<int> array;
    int t;
    int s;

    // Bits per digit
    static int dbits;
    static int BI_DB;
    static int BI_DM;
    static int BI_DV;

    static int BI_FP;
    static ulong BI_FV;
    static int BI_F1;
    static int BI_F2;

    // JavaScript engine analysis
    const long canary = 0xdeadbeefcafe;
    const bool j_lm = ((canary&0xffffff)==0xefcafe);

    // (public) Constructor
    public BigInteger(int a, int b, SecureRandom c) {
        array = new ListX<int>();
        this.fromNumber(a,b,c);
    }

    public BigInteger() {
        array = new ListX<int>();
    }

    public BigInteger(String a) {
        array = new ListX<int>();
        this.fromString(a,256);
    }
    public BigInteger(byte[] ba)
    {
        array = new ListX<int>();
        this.fromByteArray(ba);
    }
    public BigInteger(String a, int b) {
        array = new ListX<int>();
        this.fromString(a,b);
    }

    // return new, unset BigInteger
    static BigInteger nbi() { return new BigInteger(); }

    public delegate int AMSig(BigInteger bi, int i, int x, BigInteger w, int j, int c, int n);

    static AMSig am;

    // am: Compute w_j += (x*this_i), propagate carries,
    // c is initial carry, returns final carry.
    // c < 3*dvalue, x < 2*dvalue, this_i < dvalue
    // We need to select the fastest one that works in this environment.

    // These appear to be unused
#if false
    // am1: use a single mult and divide to get the high bits,
    // max digit bits should be 26 because
    // max internal value = 2*dvalue^2-2*dvalue (< 2^53)
    function am1(i,x,w,j,c,n) {
        var this_array = this.array;
        var w_array    = w.array;
        while(--n >= 0) {
            var v = x*this_array[i++]+w_array[j]+c;
            c = Math.floor(v/0x4000000);
            w_array[j++] = v&0x3ffffff;
        }
        return c;
    }

    // am2 avoids a big mult-and-extract completely.
    // Max digit bits should be <= 30 because we do bitwise ops
    // on values up to 2*hdvalue^2-hdvalue-1 (< 2^31)
    function am2(i,x,w,j,c,n) {
        var this_array = this.array;
        var w_array    = w.array;
        var xl = x&0x7fff, xh = x>>15;
        while(--n >= 0) {
            var l = this_array[i]&0x7fff;
            var h = this_array[i++]>>15;
            var m = xh*l+h*xl;
            l = xl*l+((m&0x7fff)<<15)+w_array[j]+(c&0x3fffffff);
            c = (l>>>30)+(m>>>15)+xh*h+(c>>>30);
            w_array[j++] = l&0x3fffffff;
        }
        return c;
    }
#endif

    // Alternately, set max digit bits to 28 since some
    // browsers slow down when dealing with 32-bit numbers.
    public static int am3(BigInteger bi, int i, int x, BigInteger w, int j, int c, int n) {
        var this_array = bi.array;
        var w_array    = w.array;

        var xl = x&0x3fff; var xh = x>>14;
        while(--n >= 0) {
            var l = this_array[i]&0x3fff;
            var h = this_array[i++]>>14;
            var m = xh*l+h*xl;
            l = xl*l+((m&0x3fff)<<14)+w_array[j]+c;
            c = (l>>28)+(m>>14)+xh*h;
            w_array[j++] = l&0xfffffff;
        }
        return c;
    }

    

#if false
    // This is tailored to VMs with 2-bit tagging. It makes sure
    // that all the computations stay within the 29 bits available.
    function am4(i,x,w,j,c,n) {
    var this_array = this.array;
    var w_array    = w.array;

    var xl = x&0x1fff, xh = x>>13;
    while(--n >= 0) {
    var l = this_array[i]&0x1fff;
    var h = this_array[i++]>>13;
    var m = xh*l+h*xl;
    l = xl*l+((m&0x1fff)<<13)+w_array[j]+c;
    c = (l>>26)+(m>>13)+xh*h;
    w_array[j++] = l&0x3ffffff;
    }
    return c;
    }
#endif

    // Digit conversions
    const String BI_RM = "0123456789abcdefghijklmnopqrstuvwxyz";
    static int[] BI_RC;

    // am3/28 is best for SM, Rhino, but am4/26 is best for v8.
    // Kestrel (Opera 9.5) gets its best result with am4/26.
    // IE7 does 9% better with am3/28 than with am4/26.
    // Firefox (SM) gets 10% faster with am3/28 than with am4/26.


    public static void setupEngine(AMSig fn, int bits) {
        BigInteger.am = fn;
        dbits = bits;

        BI_DB = dbits;
        BI_DM = ((((int)1)<<(int)dbits)-1);
        BI_DV = (((int)1)<<(int)dbits);

        BI_FP = 52;
        // The RHS had been Math.Pow(2,BI_FP);
        BI_FV = (((ulong)1)<<(int)BI_FP);
        BI_F1 = BI_FP-dbits;
        BI_F2 = 2*dbits-BI_FP;

        BI_RC = new int[256];

        // char rr = "0".charCodeAt(0);
        char rr = '0';
        for (int vv = 0; vv <= 9; ++vv) BI_RC[rr++] = vv;
        // rr = 'a".charCodeAt(0);
        rr = 'a';
        for (int vv = 10; vv < 36; ++vv) BI_RC[rr++] = vv;
        // rr = "A".charCodeAt(0);
        rr = 'A';
        for (int vv = 10; vv < 36; ++vv) BI_RC[rr++] = vv;
    }

    static char int2char(int n) { return BI_RM[(int)n]; }
    static int intAt(String s, int i) {
        // int c = (int)BI_RC[s[(int)i]];
        // return (c==null)?-1:c;
        if (i > BI_RC.Length) return -1;
        else return (int)BI_RC[s[(int)i]];
    }

    // (protected) copy this to r
    private void copyTo(BigInteger r) {
        var this_array = this.array;
        var r_array    = r.array;

        for(var i = this.t-1; i >= 0; --i) r_array[i] = this_array[i];
        r.t = this.t;
        r.s = this.s;
    }

    // (protected) set from integer value x, -DV <= x < DV
    private void fromInt(int x) {
        var this_array = this.array;
        this.t = 1;
        this.s = (x<0)?-1:0;
        if (this_array.Count == 0)
        {
            if (x > 0)
                this_array.Add(x);
            else if (x < -1)
                this_array.Add(x + BI_DV);
            else
                this.t = 0;
        }
        else
        {
            if (x > 0) this_array[0] = (int)x;
            else if (x < -1) this_array[0] = (int)(x + BI_DV);
            else this.t = 0;
        }
    }

    // return bigint initialized to value
    static BigInteger nbv(int i) { var r = nbi(); r.fromInt(i); return r; }

    // (protected) set from string and radix
    private void fromString(String s, int b) {
        var this_array = this.array;
        int k;
        if(b == 16) k = 4;
        else if(b == 8) k = 3;
        else if(b == 256) k = 8; // byte array
        else if(b == 2) k = 1;
        else if(b == 32) k = 5;
        else if(b == 4) k = 2;
        else { this.fromRadix(s,b); return; }
        this.t = 0;
        this.s = 0;
        int i = s.Length; bool mi = false; var sh = 0;
        while(--i >= 0) {
            int x = (k==8) ? (s[i] & 0xff) : intAt(s,(int)i);
            if(x < 0) {
                if(s[i] == '-') mi = true;
                continue;
            }
            mi = false;
            if(sh == 0)
                this_array[this.t++] = (int)x;
            else if(sh+k > BI_DB) {
                this_array[this.t-1] |= ((int)x&((((int)1)<<(BI_DB-sh))-1))<<sh;
                this_array[this.t++] = ((int)x>>(BI_DB-sh));
            }
            else
                this_array[this.t-1] |= ((int)x)<<sh;
            sh += (int)k;
            if(sh >= BI_DB) sh -= BI_DB;
        }
        if(k == 8 && (s[0]&0x80) != 0) {
            this.s = -1;
            if(sh > 0) this_array[this.t-1] |= ((((int)1)<<(BI_DB-sh))-1)<<sh;
        }
        this.clamp();
        if(mi) BigInteger.ZERO.subTo(this,this);
    }

    private void fromByteArray(byte[] ba)
    {
        var this_array = this.array;
        this.t = 0;
        this.s = 0;
        int i = ba.Length; bool mi = false; var sh = 0;
        while (--i >= 0)
        {
            int x = ba[i] & 0xff;
            mi = false;
            if (sh == 0)
                this_array[this.t++] = (int)x;
            else if (sh + 8 > BI_DB)
            {
                this_array[this.t - 1] |= ((int)x & ((((int)1) << (BI_DB - sh)) - 1)) << sh;
                this_array[this.t++] = ((int)x >> (BI_DB - sh));
            }
            else
                this_array[this.t - 1] |= ((int)x) << sh;
            sh += 8;
            if (sh >= BI_DB) sh -= BI_DB;
        }
        if ((ba[0] & 0x80) != 0)
        {
            this.s = -1;
            if (sh > 0) this_array[this.t - 1] |= ((((int)1) << (BI_DB - sh)) - 1) << sh;
        }
        this.clamp();
        if (mi) BigInteger.ZERO.subTo(this, this);
    }

    // (protected) clamp off excess high words
    private void clamp() {
        var this_array = this.array;
        var c = this.s&BI_DM;
        while(this.t > 0 && this_array[this.t-1] == c) --this.t;
    }

    // (public) return string representation in given radix
    public String toString(int b) {
        var this_array = this.array;
        if(this.s < 0) return "-"+this.negate().toString(b);
        int k;
        if(b == 16) k = 4;
        else if(b == 8) k = 3;
        else if(b == 2) k = 1;
        else if(b == 32) k = 5;
        else if(b == 4) k = 2;
        else return this.toRadix(b);
        int km = ((int)1<<k)-1;
        int d; bool m = false; var r = ""; int i = (int)this.t;
        int p = (BI_DB-(i*BI_DB)%k);
        if(i-- > 0) {
            if(p < BI_DB && (d = this_array[i]>>p) > 0) { m = true; r = new String(int2char(d), 1); }
            while(i >= 0) {
                if(p < k) {
                    d = (this_array[i]&((((int)1)<<p)-1))<<(k-p);
                    d |= this_array[--i]>>(p+=(int)(BI_DB-k));
                }
                else {
                    d = (this_array[i]>>(p-=k))&km;
                    if(p <= 0) { p += (int)BI_DB; --i; }
                }
                if(d > 0) m = true;
                if(m) r += int2char(d);
            }
        }
        return m?r:"0";
    }

    // (public) -this
    public BigInteger negate() { var r = nbi(); BigInteger.ZERO.subTo(this,r); return r; }

    // (public) |this|
    public BigInteger abs() { return (this.s<0)?this.negate():this; }

    // (public) return + if this > a, - if this < a, 0 if equal
    public int compareTo(BigInteger a) {
        var this_array = this.array;
        var a_array = a.array;

        var r = this.s-a.s;
        if(r != 0) return r;
        int i = (int)this.t;
        r = i-(int)a.t;
        if(r != 0) return r;
        while(--i >= 0) if((r=(int)(this_array[i]-a_array[i])) != 0) return r;
        return 0;
    }

    // returns bit length of the integer x
    public int nbits(int x) {
        int r = 1;
        int t;
        if((t=x>>16) != 0) { x = t; r += 16; }
        if((t=x>>8) != 0) { x = t; r += 8; }
        if((t=x>>4) != 0) { x = t; r += 4; }
        if((t=x>>2) != 0) { x = t; r += 2; }
        if((t=x>>1) != 0) { x = t; r += 1; }
        return r;
    }

    // (public) return the number of bits in "this"
    public int bitLength() {
        var this_array = this.array;
        if(this.t <= 0) return 0;
        return ((int)BI_DB)*(this.t-1) + nbits(this_array[this.t-1]^(int)(this.s&BI_DM));
    }

    // (protected) r = this << n*DB
    private void dLShiftTo(int n, BigInteger r) {
        var this_array = this.array;
        var r_array = r.array;
        for(int i = (int)this.t-1; i >= 0; --i) r_array[i+n] = this_array[i];
        for(int i = (int)n-1; i >= 0; --i) r_array[i] = 0;
        r.t = this.t+(int)n;
        r.s = this.s;
    }

    // (protected) r = this >> n*DB
    private void dRShiftTo(int n, BigInteger r) {
        var this_array = this.array;
        var r_array = r.array;
        for (var i = n; i < this.t; ++i) r_array[i-n] = this_array[i];
        r.t = (int)Math.Max(this.t-n,0);
        r.s = this.s;
    }

    // (protected) r = this << n
    private void lShiftTo(int n, BigInteger r) {
        var this_array = this.array;
        var r_array = r.array;
        int bs = (int)(n%BI_DB);
        int cbs = (int)(BI_DB-bs);
        var bm = ((int)1<<cbs)-1;
        int ds = n/BI_DB; int c = ((int)this.s<<bs)&BI_DM;
        for(int i = (int)this.t-1; i >= 0; --i) {
            r_array[i+ds+1] = (this_array[i]>>cbs)|c;
            c = (this_array[i]&bm)<<bs;
#if TRACING
            Console.WriteLine("   i = {0}, this_array[i] = {3}, r_array[i + ds + 1] = {1}; c = {2}.", i, r_array[i + ds + 1], c, this_array[i]);
#endif
        }
        for(int i = (int)ds-1; i >= 0; --i) r_array[i] = 0;
        r_array[ds] = c;
        r.t = this.t+(int)ds+1;
        r.s = this.s;
        r.clamp();
    }

    // (protected) r = this >> n
    private void rShiftTo(int n, BigInteger r) {
        var this_array = this.array;
        var r_array = r.array;
        r.s = this.s;
        var ds = n/BI_DB;
        if(ds >= this.t) { r.t = 0; return; }
        int bs = (int)(n%BI_DB);
        int cbs = (int)(BI_DB-bs);
        int bm = ((int)1<<bs)-1;
        r_array[0] = this_array[ds]>>bs;
        for(int i = ds+1; i < this.t; ++i) {
            r_array[i-ds-1] |= (this_array[i]&bm)<<cbs;
            r_array[i-ds] = this_array[i]>>bs;
        }
        if(bs > 0) r_array[(int)this.t-ds-1] |= ((int)this.s&bm)<<cbs;
        r.t = this.t-(int)ds;
        r.clamp();
    }

    // (protected) r = this - a
    private void subTo(BigInteger a, BigInteger r) {
        var this_array = this.array;
        var r_array = r.array;
        var a_array = a.array;
        int i = 0; int c = 0; var m = Math.Min(a.t,this.t);
        while(i < m) {
            c += (int)this_array[i]-(int)a_array[i];
            r_array[i++] = (int)c&BI_DM;
            c >>= (int)BI_DB;
        }
        if(a.t < this.t) {
            c -= a.s;
            while(i < this.t) {
                c += (int)this_array[i];
                r_array[i++] = (int)c&BI_DM;
                c >>= (int)BI_DB;
            }
            c += this.s;
        }
        else {
            c += this.s;
            while(i < a.t) {
                c -= (int)a_array[i];
                r_array[i++] = (int)c&BI_DM;
                c >>= (int)BI_DB;
            }
            c -= a.s;
        }
        r.s = (c<0)?-1:0;
        if(c < -1) r_array[i++] = (int)((int)BI_DV+c);
        else if(c > 0) r_array[i++] = (int)c;
        r.t = i;
        r.clamp();
    }

    // (protected) r = this * a, r != this,a (HAC 14.12)
    // "this" should be the larger one if appropriate.
    private void multiplyTo(BigInteger a, BigInteger r) {
        var this_array = this.array;
        var r_array = r.array;
        var x = this.abs(); var y = a.abs();
        var y_array = y.array;

        int i = (int)x.t;
        r.t = (int)i+y.t;
        while(--i >= 0) r_array[i] = 0;
        for(i = 0; i < y.t; ++i) r_array[i+(int)x.t] = am(x,0,y_array[i],r,(int)i,0,(int)x.t);
        r.s = 0;
        r.clamp();
        if(this.s != a.s) BigInteger.ZERO.subTo(r,r);
    }

    // (protected) r = this^2, r != this (HAC 14.16)
    private void squareTo(BigInteger r) {
        var x = this.abs();
        var x_array = x.array;
        var r_array = r.array;

        int i = (int)(2*x.t);
        r.t = (int)i;
        while(--i >= 0) r_array[i] = 0;
        for(i = 0; i < x.t-1; ++i) {
            var c = am(x,(int)i,x_array[i],r,(int)(2*i),0,1);
            if((r_array[(int)i+x.t]+=am(x,(int)(i+1),2*x_array[i],r,(int)(2*i+1),c,(int)x.t-i-1)) >= BI_DV) {
                r_array[(int)i+x.t] -= BI_DV;
                r_array[(int)i+x.t+1] = 1;
            }
        }
        if(r.t > 0) r_array[r.t-1] += am(x,(int)i,x_array[i],r,(int)(2*i),0,1);
        r.s = 0;
        r.clamp();
    }

    // (protected) divide this by m, quotient and remainder to q, r (HAC 14.20)
    // r != q, this != m.  q or r may be null.
    private void divRemTo(BigInteger m, BigInteger q, BigInteger r) {
#if TRACING
        this.PrintArray("this");
#endif
        var pm = m.abs();
        if(pm.t <= 0) return;
        var pt = this.abs();
#if TRACING
        pt.PrintArray("pt");
#endif
        if (pt.t < pm.t)
        {
            if(q != null) q.fromInt(0);
            if(r != null) this.copyTo(r);
            return;
        }
        if(r == null) r = nbi();
        var y = nbi(); var ts = this.s; var ms = m.s;
        var pm_array = pm.array;
        int nsh = BI_DB-(int)nbits(pm_array[pm.t-1]);	// normalize modulus
        if(nsh > 0) { pm.lShiftTo(nsh,y); pt.lShiftTo(nsh,r); }
        else { pm.copyTo(y); pt.copyTo(r); }
        int ys = y.t;

        var y_array = y.array;
        double y0 = (double)y_array[ys-1];
        if(y0 == 0) return;
        double yt = (y0*(double)((int)1<<BI_F1)+((ys>1)?y_array[ys-2]>>BI_F2:0));
        double d1 = ((double)BI_FV)/yt;
        double d2 = ((double)(1<<BI_F1))/yt;
        var e = 1<<BI_F2;
        int i = (int)r.t; int j = i-(int)ys; var t = (q==null)?nbi():q;
        y.dLShiftTo(j,t);

#if TRACING
        Console.WriteLine("y is");
        for (int kk = 0; kk < y.array.Count; kk++)
            Console.WriteLine("{0}", y.array[kk]);
#endif

        var r_array = r.array;
        if(r.compareTo(t) >= 0) {
            r_array[r.t++] = 1;
            r.subTo(t,r);
        }
        BigInteger.ONE.dLShiftTo((int)ys,t);
        t.subTo(y,y);	// "negative" y so we can replace sub with am later
        while(y.t < ys) y_array[y.t++] = 0;
        while(--j >= 0) {
            // Estimate quotient digit
            int qd = (r_array[--i]==y0) ? BI_DM :
                (int)Math.Floor((double)r_array[i]*d1+((double)(r_array[i-1]+e))*d2);
            if((r_array[i]+=am(y,0,qd,r,(int)j,0,(int)ys)) < qd) {	// Try it out
                y.dLShiftTo(j,t);
                r.subTo(t,r);
                while(r_array[i] < --qd) r.subTo(t,r);
            }
        }
        if(q != null) {
            r.dRShiftTo((int)ys,q);
            if(ts != ms) BigInteger.ZERO.subTo(q,q);
        }
        r.t = (int)ys;
        r.clamp();
        if(nsh > 0) r.rShiftTo(nsh,r);	// Denormalize remainder
        if(ts < 0) BigInteger.ZERO.subTo(r,r);
    }

    // (public) this mod a
    public BigInteger mod(BigInteger a) {
        var r = nbi();
        this.abs().divRemTo(a,null,r);
        if(this.s < 0 && r.compareTo(BigInteger.ZERO) > 0) a.subTo(r,r);
        return r;
    }

    // Modular reduction using "classic" algorithm
    public class ClassicReducer: Reducer {
        BigInteger m;

        public ClassicReducer(BigInteger m) { this.m = m; }

        public void reduce(BigInteger x) { x.divRemTo(m,null,x); }

        public override BigInteger convert(BigInteger x) {
            if(x.s < 0 || x.compareTo(this.m) >= 0) return x.mod(this.m);
            else return x;
        }

        public override BigInteger revert(BigInteger x) { return x; }
        public override void mulTo(BigInteger x, BigInteger y, BigInteger r) { x.multiplyTo(y,r); this.reduce(r); }
        public override void sqrTo(BigInteger x, BigInteger r) { x.squareTo(r); this.reduce(r); }
    }

    // (protected) return "-1/this % 2^DB"; useful for Mont. reduction
    // justification:
    //         xy == 1 (mod m)
    //         xy =  1+km
    //   xy(2-xy) = (1+km)(1-km)
    // x[y(2-xy)] = 1-k^2m^2
    // x[y(2-xy)] == 1 (mod m^2)
    // if y is 1/x mod m, then y(2-xy) is 1/x mod m^2
    // should reduce x and y(2-xy) by m^2 at each step to keep size bounded.
    // JS multiply "overflows" differently from C/C++, so care is needed here.
    private int invDigit() {
        var this_array = this.array;
        if(this.t < 1) return 0;
        int x = (int)this_array[0];
        if((x&1) == 0) return 0;
        int y = x&3;		// y == 1/x mod 2^2
        y = (y*(2-(x&0xf)*y))&0xf;	// y == 1/x mod 2^4
        y = (y*(2-(x&0xff)*y))&0xff;	// y == 1/x mod 2^8
        y = (y*(2-(((x&0xffff)*y)&0xffff)))&0xffff;	// y == 1/x mod 2^16
        // last step - calculate inverse mod DV directly;
        // assumes 16 < DB <= 32 and assumes ability to handle 48-bit ints
        y = (y*(2-x*y%(int)BI_DV))%(int)BI_DV;		// y == 1/x mod 2^dbits
        // we really want the negative inverse, and -DV < y < DV
        return (y>0)?(int)BI_DV-y:-y;
    }

    public abstract class Reducer {
        abstract public BigInteger convert(BigInteger x);
        abstract public BigInteger revert(BigInteger x);
        // DELETEME
        // abstract public void reduce(BigInteger x);
        abstract public void sqrTo(BigInteger x, BigInteger r);
        abstract public void mulTo(BigInteger x, BigInteger y, BigInteger r);
    };

    class MontgomeryReducer: Reducer {
        BigInteger m;
        int mp;
        int mpl;
        int mph;
        int um;
        int mt2;

        public MontgomeryReducer(BigInteger m) {
            this.m = m;
            this.mp = m.invDigit();
            this.mpl = this.mp&0x7fff;
            this.mph = this.mp>>15;
            this.um = (1<<(BI_DB-15))-1;
            this.mt2 = 2*m.t;
        }

        // xR mod m
        public override BigInteger convert(BigInteger x) {
            var r = nbi();
            x.abs().dLShiftTo(this.m.t,r);
            r.divRemTo(this.m,null,r);
            if(x.s < 0 && r.compareTo(BigInteger.ZERO) > 0) this.m.subTo(r,r);
            return r;
        }

        public override BigInteger revert(BigInteger x) {
            var r = nbi();
            x.copyTo(r);
            this.reduce(r);
            return r;
        }

        // x = x/R mod m (HAC 14.32)
        public void reduce(BigInteger x) {
            var x_array = x.array;
            while(x.t <= this.mt2)	// pad x so am has enough room later
                x_array[x.t++] = 0;
            for(var i = 0; i < this.m.t; ++i) {
                // faster way of calculating u0 = x[i]*mp mod DV
                var j = x_array[i]&0x7fff;
                var u0 = (j*this.mpl+(((j*this.mph+(x_array[i]>>15)*this.mpl)&this.um)<<15))&BI_DM;
                // use am to combine the multiply-shift-add into one call
                j = i+this.m.t;
                x_array[j] += am(this.m,0,u0,x,i,0,this.m.t);
                // propagate carry
                while(x_array[j] >= BI_DV) { x_array[j] -= BI_DV; x_array[++j]++; }
            }
            x.clamp();
            x.dRShiftTo(this.m.t,x);
            if(x.compareTo(this.m) >= 0) x.subTo(this.m,x);
        }

        // r = "x^2/R mod m"; x != r
        public override void sqrTo(BigInteger x, BigInteger r) { x.squareTo(r); this.reduce(r); }

        // r = "xy/R mod m"; x,y != r
        public override void mulTo(BigInteger x, BigInteger y, BigInteger r) {
            x.multiplyTo(y,r); this.reduce(r);
        }
    }


    // (protected) true iff this is even
    private bool isEven() {
        var this_array = this.array;
        return ((this.t>0)?(int)(this_array[0]&1):this.s) == 0;
    }

    // (protected) this^e, e < 2^32, doing sqr and mul with "z" (HAC 14.79)
    private BigInteger exp(uint e, Reducer z) {
        if(e > 0xffffffff || e < 1) return BigInteger.ONE;
        var r = nbi(); var r2 = nbi(); var g = z.convert(this); int i = (int)nbits((int)e)-1;
        g.copyTo(r);
        while(--i >= 0) {
            z.sqrTo(r,r2);
            if((e&(1<<i)) > 0) z.mulTo(r2,g,r);
            else { var t = r; r = r2; r2 = t; }
        }
        return z.revert(r);
    }

    // (public) this^e % m, 0 <= e < 2^32
    public BigInteger modPowInt(uint e, BigInteger m) {
        Reducer z;
        if(e < 256 || m.isEven()) z = new ClassicReducer(m); else z = new MontgomeryReducer(m);
        return this.exp(e,z);
    }

    // "constants"
    public static BigInteger ZERO = nbv(0);
    public static BigInteger ONE = nbv(1);

    // Copyright (c) 2005  Tom Wu
    // All Rights Reserved.
    // See "LICENSE" for details.

    // Extended JavaScript BN functions, required for RSA private ops.

    // (public)
    public BigInteger clone() { var r = nbi(); this.copyTo(r); return r; }

    // (public) return value as integer
    public int intValue() {
        var this_array = this.array;
        if(this.s < 0) {
            if(this.t == 1) return (int)this_array[0]-(int)BI_DV;
            else if(this.t == 0) return -1;
        }
        else if(this.t == 1) return (int)this_array[0];
        else if(this.t == 0) return 0;
        // assumes 16 < DB < 32
        // return ((this_array[1]&((1<<(32-BI_DB))-1))<<BI_DB)|this_array[0];
        var x = (this_array[1]&((1<<(32-BI_DB))-1));
        return (int)((int)(x << BI_DB)|this_array[0]);
    }

    // (public) return value as byte
    public byte byteValue() {
        var this_array = this.array;
        return (this.t==0)?(byte)this.s:(byte)((this_array[0]<<24)>>24);
    }

    // (public) return value as short (assumes DB>=16)
    public ushort shortValue() {
        var this_array = this.array;
        return (this.t==0)?(ushort)this.s:(ushort)((this_array[0]<<16)>>16);
    }

    private static double LN2 = Math.Log(2.0);

    // (protected) return x s.t. r^x < DV
    private int chunkSize(int r) { return (int)Math.Floor(LN2*(double)BI_DB/Math.Log(r)); }

    // (public) 0 if this == 0, 1 if this > 0
    public int signum() {
        var this_array = this.array;
        if(this.s < 0) return -1;
        else if(this.t <= 0 || (this.t == 1 && this_array[0] <= 0)) return 0;
        else return 1;
    }

    private static String sdigits = "0123456789abcdefghijklmnopqrstuvwxyz";

    private static String IntToString(int i, int radix) {
        if (radix == 10)
        {
            return i.ToString();
        }
        else if (radix == 16)
        {
            return i.ToString("X");
        }
        else
        {
            bool neg = false;
            if (i < 0)
            {
                neg = true; i = -i;
            }
            String res = "";
            while (i != 0) {
                int digit = i % radix;
                res = sdigits.Substring(digit, 1) + res;
                i = i / radix;
            }
            if (neg) res = "-" + res;
            return res;
        }
    }

    // (protected) convert to radix string
    public String toRadix(int b) {
        // if (b == null) b = 10;
        if(this.signum() == 0 || b < 2 || b > 36) return "0";
        var cs = this.chunkSize(b);
        var a = (int)Math.Pow((double)b,(double)cs);
        Console.WriteLine("a = {0}.", a);
        var d = nbv(a); var y = nbi(); var z = nbi(); var r = "";
        Console.WriteLine("d.intValue = {0}.", d.intValue());
        this.divRemTo(d,y,z);
        Console.WriteLine("y.signum = {0}", y.signum());
        Console.WriteLine("z.intValue = " + z.intValue());
        while(y.signum() > 0) {
            r = IntToString(a+z.intValue(), (int)b).Substring(1) + r;
            y.divRemTo(d,y,z);
            Console.WriteLine("y.signum = {0}", y.signum());
            Console.WriteLine("z.intValue = " + z.intValue());
        }
        return IntToString(z.intValue(), (int)b) + r;
    }

    private static int IntPow(int n, int p) {
        int res = 1;
        for (int k = 1; k < p; k++)
        {
            res *= n;
        }
        return res;
    }

    // (protected) convert from radix string
    private void fromRadix(String s, int b) {
        this.fromInt(0);
        var cs = this.chunkSize(b);
        var d = IntPow(b,cs); bool mi = false; int j = 0; int w = 0;
        for(int i = 0; i < s.Length; ++i) {
            int x = intAt(s,i);
            if(x < 0) {
                if(s[(int)i] == '-' && this.signum() == 0) mi = true;
                continue;
            }
            w = b*w+(int)x;
            if(++j >= cs) {
                this.dMultiply(d);
                this.dAddOffset(w,0);
                j = 0;
                w = 0;
            }
        }
        if(j > 0) {
            this.dMultiply(IntPow(b,j));
            this.dAddOffset(w,0);
        }
        if(mi) BigInteger.ZERO.subTo(this,this);
    }

    // (protected) alternate constructor
    private void fromNumber(int a, int b, SecureRandom c) {
        if(a < 2) this.fromInt(1);
        else {
            this.fromNumber(a,c);
            if(!this.testBit(a-1))	// force MSB set
                this.bitwiseTo(BigInteger.ONE.shiftLeft((int)a-1),op_or,this);
            if(this.isEven()) this.dAddOffset(1,0); // force odd
            while(!this.isProbablePrime(b)) {
                this.dAddOffset(2,0);
                if(this.bitLength() > a) this.subTo(BigInteger.ONE.shiftLeft((int)a-1),this);
            }
        }
    }

    private void fromNumber(int a, SecureRandom b) {
        // new BigInteger(int,RNG)
        byte[] x = new byte[(a>>3)+1];
        int t = (int)a&7;
        b.nextBytes(x);
        if(t > 0)
            x[0] &= (byte)((1<<(int)t)-1);
        else
            x[0] = 0;
        this.fromByteArray(x);
    }

    // (public) convert to bigendian byte array
    public byte[] toByteArray() {
        var this_array = this.array;
        int i = (int)this.t; var r = new ListX<byte>();
        r[0] = (byte)this.s;
        int p = (int)BI_DB-(i*(int)BI_DB)%8;
        int d; int k = 0;
        if(i-- > 0) {
            if(p < BI_DB && (d = this_array[i]>>p) != (this.s&BI_DM)>>p)
                r[k++] = (byte)(d | ((int)this.s<<(int)(BI_DB-p)));
            while(i >= 0) {
                if(p < 8) {
                    d = (this_array[i]&(((int)1<<p)-1))<<(8-p);
                    d |= this_array[--i]>>(p+=BI_DB-8);
                }
                else {
                    d = (this_array[i]>>(p-=8))&0xff;
                    if(p <= 0) { p += BI_DB; --i; }
                }
                if((d&0x80) != 0) d = (int)((int)d | -256);
                if(k == 0 && (this.s&0x80) != (d&0x80)) ++k;
                if(k > 0 || d != this.s) r[k++] = (byte)d;
            }
        }
        return r.ToArray();
    }

    public bool Equals(BigInteger a) { return(this.compareTo(a)==0); }
    public BigInteger min(BigInteger a) { return(this.compareTo(a)<0)?this:a; }
    public BigInteger max(BigInteger a) { return(this.compareTo(a)>0)?this:a; }

    // (protected) r = this op a (bitwise)
    public delegate int BinOpInt(int x1, int x2);

    private void bitwiseTo(BigInteger a, BinOpInt op, BigInteger r) {
        var this_array = this.array;
        var a_array    = a.array;
        var r_array    = r.array;
        var m = Math.Min(a.t,this.t);
        for(int i = 0; i < m; ++i) r_array[i] = op(this_array[i],a_array[i]);
        int f;
        if(a.t < this.t) {
            f = (int)a.s&BI_DM;
            for(int i = m; i < this.t; ++i) r_array[i] = op(this_array[i],f);
            r.t = this.t;
        }
        else {
            f = (int)this.s&BI_DM;
            for(int i = m; i < a.t; ++i) r_array[i] = op(f,a_array[i]);
            r.t = a.t;
        }
        r.s = (int)op((int)this.s,(int)a.s);
        r.clamp();
    }

    // (public) this & a
    private static int op_and(int x, int y) { return x&y; }
    public BigInteger and(BigInteger a) { var r = nbi(); this.bitwiseTo(a,op_and,r); return r; }

    // (public) this | a
    private static int op_or(int x, int y) { return x|y; }
    public BigInteger or(BigInteger a) { var r = nbi(); this.bitwiseTo(a,op_or,r); return r; }

    // (public) this ^ a
    private static int op_xor(int x, int y) { return x^y; }
    public BigInteger xor(BigInteger a) { var r = nbi(); this.bitwiseTo(a,op_xor,r); return r; }

    // (public) this & ~a
    private static int op_andnot(int x, int y) { return x&~y; }
    public BigInteger andNot(BigInteger a) { var r = nbi(); this.bitwiseTo(a,op_andnot,r); return r; }

    // (public) ~this
    public BigInteger not() {
        var this_array = this.array;
        var r = nbi();
        var r_array = r.array;

        for(var i = 0; i < this.t; ++i) r_array[i] = BI_DM&~this_array[i];
        r.t = this.t;
        r.s = ~this.s;
        return r;
    }
    
    // (public) this << n
    public BigInteger shiftLeft(int n) {
        var r = nbi();
        if(n < 0) this.rShiftTo(-n,r); else this.lShiftTo(n,r);
        return r;
    }

    // (public) this >> n
    public BigInteger shiftRight(int n) {
        var r = nbi();
        if(n < 0) this.lShiftTo(-n,r); else this.rShiftTo(n,r);
        return r;
    }

    // return index of lowest 1-bit in x, x < 2^31 (-1 for no set bits)
    public static int lbit(int x) {
        if(x == 0) return -1;
        int r = 0;
        if((x&0xffff) == 0) { x >>= 16; r += 16; }
        if((x&0xff) == 0) { x >>= 8; r += 8; }
        if((x&0xf) == 0) { x >>= 4; r += 4; }
        if((x&3) == 0) { x >>= 2; r += 2; }
        if((x&1) == 0) ++r;
        return r;
    }

    // (public) returns index of lowest 1-bit (or -1 if none)
    public int getLowestSetBit() {
        var this_array = this.array;
        for(var i = 0; i < this.t; ++i)
            if(this_array[i] != 0) return i*BI_DB+lbit(this_array[i]);
        if(this.s < 0) return (int)this.t*BI_DB;
        return -1;
    }

    // return number of 1 bits in x
    private static int cbit(int x) {
        int r = 0;
        while(x != 0) { x &= x-1; ++r; }
        return r;
    }

    // (public) return number of set bits
    public int bitCount() {
        int r = 0;
        int x = (int)this.s&BI_DM;
        for(int i = 0; i < this.t; ++i) r += cbit(this.array[i]^x);
        return r;
    }

    // (public) true iff nth bit is set
    public bool testBit(int n) {
        var this_array = this.array;
        int j = n/(int)BI_DB;
        if(j >= this.t) return(this.s!=0);
        return((this_array[j]&((int)1<<(int)(n%BI_DB)))!=0);
    }

    // (protected) this op (1<<n)
    private BigInteger changeBit(int n, BinOpInt op) {
        var r = ONE.shiftLeft((int)n);
        this.bitwiseTo(r,op,r);
        return r;
    }

    // (public) this | (1<<n)
    public BigInteger setBit(int n) { return this.changeBit(n,op_or); }

    // (public) this & ~(1<<n)
    public BigInteger  clearBit(int n) { return this.changeBit(n,op_andnot); }

    // (public) this ^ (1<<n)
    public BigInteger  flipBit(int n) { return this.changeBit(n,op_xor); }

    // (protected) r = this + a
    private void addTo(BigInteger a, BigInteger r) {
        var this_array = this.array;
        var a_array = a.array;
        var r_array = r.array;
        int i = 0; int c = 0; int m = Math.Min(a.t,this.t);
        while(i < m) {
            c += this_array[i]+a_array[i];
            r_array[i++] = c&BI_DM;
            c >>= BI_DB;
        }
        if(a.t < this.t) {
            c += (int)a.s;
            while(i < this.t) {
                c += this_array[i];
                r_array[i++] = c&BI_DM;
                c >>= BI_DB;
            }
            c += (int)this.s;
        }
        else {
            c += (int)this.s;
            while(i < a.t) {
                c += a_array[i];
                r_array[i++] = c&BI_DM;
                c >>= BI_DB;
            }
            c += (int)a.s;
        }
        r.s = (c<0)?-1:0;
        if(c > 0) r_array[i++] = c;
        else if(c < -1) r_array[i++] = BI_DV+c;
        r.t = i;
        r.clamp();
    }

    // (public) this + a
    public BigInteger add(BigInteger a) { var r = nbi(); this.addTo(a,r); return r; }

    // (public) this - a
    public BigInteger subtract(BigInteger a) { var r = nbi(); this.subTo(a,r); return r; }

    // (public) this * a
    public BigInteger multiply(BigInteger a) { var r = nbi(); this.multiplyTo(a,r); return r; }

    // (public) this / a
    public BigInteger divide(BigInteger a) { var r = nbi(); this.divRemTo(a,r,null); return r; }

    // (public) this % a
    public BigInteger remainder(BigInteger a) { var r = nbi(); this.divRemTo(a,null,r); return r; }

    public struct BigIntPair {
        public BigInteger p1;
        public BigInteger p2;
        public BigIntPair(BigInteger p1, BigInteger p2) { this.p1 = p1; this.p2 = p2; }
    }
    // (public) [this/a,this%a]
    public BigIntPair divideAndRemainder(BigInteger a) {
        var q = nbi(); var r = nbi();
        this.divRemTo(a,q,r);
        return new BigIntPair(q,r);
    }

    // (protected) this *= n, this >= 0, 1 < n < DV
    private void dMultiply(int n) {
        var this_array = this.array;
        this_array[this.t] = am(this,0,n-1,this,0,0,this.t);
        ++this.t;
        this.clamp();
    }

    // (protected) this += n << w words, this >= 0
    private void dAddOffset(int n, int w) {
        var this_array = this.array;
        while(this.t <= w) this_array[this.t++] = 0;
        this_array[w] += n;
        while(this_array[w] >= BI_DV) {
            this_array[w] -= BI_DV;
            if(++w >= this.t) this_array[this.t++] = 0;
            ++this_array[w];
        }
    }

    class NullReducer: Reducer {
        public NullReducer() {}

        
        public override BigInteger convert(BigInteger x) { return x; }
        public override BigInteger revert(BigInteger x) { return x; }
        public override void mulTo(BigInteger x, BigInteger y, BigInteger r) { x.multiplyTo(y,r); }
        public override void sqrTo(BigInteger x, BigInteger r) { x.squareTo(r); }
    }

    // (public) this^e
    // public BigInteger pow(BigInteger e) { return this.exp(e,new NullReducer()); }

    // (protected) r = lower n words of "this * a", a.t <= n
    // "this" should be the larger one if appropriate.
    private void multiplyLowerTo(BigInteger a, int n, BigInteger r) {
        var r_array = r.array;
        var a_array = a.array;
        var i = Math.Min(this.t+a.t,n);
        r.s = 0; // assumes a,this >= 0
        r.t = i;
        while(i > 0) r_array[--i] = 0;
        for(int j = r.t-this.t; i < j; ++i) r_array[i+this.t] = am(this, 0,a_array[i],r,i,0,this.t);
        for(int j = Math.Min(a.t,n); i < j; ++i) am(this, 0,a_array[i],r,i,0,n-i);
        r.clamp();
    }

    // (protected) r = "this * a" without lower n words, n > 0
    // "this" should be the larger one if appropriate.
    public void multiplyUpperTo(BigInteger a, int n, BigInteger r) {
        var r_array = r.array;
        var a_array = a.array;
        --n;
        int i = r.t = this.t+a.t-n;
        r.s = 0; // assumes a,this >= 0
        while(--i >= 0) r_array[i] = 0;
        for(i = Math.Max(n-this.t,0); i < a.t; ++i)
            r_array[this.t+i-n] = am(this, n-i,a_array[i],r,0,0,this.t+i-n);
        r.clamp();
        r.dRShiftTo(1,r);
    }

    // Barrett modular reduction
    public class BarrettReducer: Reducer {
        BigInteger r2;
        BigInteger q3;
        BigInteger mu;
        BigInteger m;

        public BarrettReducer(BigInteger m) {
            // setup Barrett
            this.r2 = nbi();
            this.q3 = nbi();
            BigInteger.ONE.dLShiftTo(2*m.t,this.r2);
            this.mu = this.r2.divide(m);
            this.m = m;
        }

        public override BigInteger convert(BigInteger x) {
            if(x.s < 0 || x.t > 2*this.m.t) return x.mod(this.m);
            else if(x.compareTo(this.m) < 0) return x;
            else { var r = nbi(); x.copyTo(r); this.reduce(r); return r; }
        }

        public override BigInteger revert(BigInteger x) { return x; }

        // x = x mod m (HAC 14.42)
        public void reduce(BigInteger x) {
            x.dRShiftTo(this.m.t-1,this.r2);
            if(x.t > this.m.t+1) { x.t = this.m.t+1; x.clamp(); }
            this.mu.multiplyUpperTo(this.r2,this.m.t+1,this.q3);
            this.m.multiplyLowerTo(this.q3,this.m.t+1,this.r2);
            while(x.compareTo(this.r2) < 0) x.dAddOffset(1,this.m.t+1);
            x.subTo(this.r2,x);
            while(x.compareTo(this.m) >= 0) x.subTo(this.m,x);
        }

        // r = x^2 mod m; x != r
        public override void sqrTo(BigInteger x, BigInteger r) { x.squareTo(r); this.reduce(r); }

        // r = x*y mod m; x,y != r
        public override void mulTo(BigInteger x, BigInteger y, BigInteger r) { x.multiplyTo(y,r); this.reduce(r); }
    }

    // (public) this^e % m (HAC 14.85)
    public BigInteger modPow(BigInteger e, BigInteger m) {
        var e_array = e.array;
        var i = e.bitLength(); int k; BigInteger r = nbv(1); Reducer z;
        if(i <= 0) return r;
        else if(i < 18) k = 1;
        else if(i < 48) k = 3;
        else if(i < 144) k = 4;
        else if(i < 768) k = 5;
        else k = 6;
        if(i < 8)
            z = new ClassicReducer(m);
        else if(m.isEven())
            z = new BarrettReducer(m);
        else
            z = new MontgomeryReducer(m);

        // precomputation
        var g = new ListX<BigInteger>();
        int n = 3;
        int k1 = k-1;
        int km = (1<<k)-1;
        g[1] = z.convert(this);
        if(k > 1) {
            var g2 = nbi();
            z.sqrTo(g[1],g2);
            while(n <= km) {
                g[n] = nbi();
                z.mulTo(g2,g[n-2],g[n]);
                n += 2;
            }
        }

        int j = e.t-1; int w; bool is1 = true; BigInteger r2 = nbi(); BigInteger t;
        i = nbits(e_array[j])-1;
        while(j >= 0) {
            if(i >= k1) w = (e_array[j]>>(i-k1))&km;
            else {
                w = (e_array[j]&((1<<(i+1))-1))<<(k1-i);
                if(j > 0) w |= e_array[j-1]>>(BI_DB+i-k1);
            }

            n = k;
            while((w&1) == 0) { w >>= 1; --n; }
            if((i -= n) < 0) { i += BI_DB; --j; }
            if(is1) {	// ret == 1, don't bother squaring or multiplying it
                g[w].copyTo(r);
                is1 = false;
            }
            else {
                while(n > 1) { z.sqrTo(r,r2); z.sqrTo(r2,r); n -= 2; }
                if(n > 0) z.sqrTo(r,r2); else { t = r; r = r2; r2 = t; }
                z.mulTo(r2,g[w],r);
            }

            while(j >= 0 && (e_array[j]&(1<<i)) == 0) {
                z.sqrTo(r,r2); t = r; r = r2; r2 = t;
                if(--i < 0) { i = BI_DB-1; --j; }
            }
        }
        return z.revert(r);
    }

    // (public) gcd(this,a) (HAC 14.54)
    public BigInteger gcd(BigInteger a) {
        var x = (this.s<0)?this.negate():this.clone();
        var y = (a.s<0)?a.negate():a.clone();
        if(x.compareTo(y) < 0) { var t = x; x = y; y = t; }
        var i = x.getLowestSetBit(); var g = y.getLowestSetBit();
        if(g < 0) return x;
        if(i < g) g = i;
        if(g > 0) {
            x.rShiftTo(g,x);
            y.rShiftTo(g,y);
        }
        while(x.signum() > 0) {
            if((i = x.getLowestSetBit()) > 0) x.rShiftTo(i,x);
            if((i = y.getLowestSetBit()) > 0) y.rShiftTo(i,y);
            if(x.compareTo(y) >= 0) {
                x.subTo(y,x);
                x.rShiftTo(1,x);
            }
            else {
                y.subTo(x,y);
                y.rShiftTo(1,y);
            }
        }
        if(g > 0) y.lShiftTo(g,y);
        return y;
    }

    // (protected) this % n, n < 2^26
    private int modInt(int n) {
        var this_array = this.array;
        if(n <= 0) return 0;
        var d = BI_DV%n; int r = (this.s<0)?n-1:0;
        if(this.t > 0)
            if(d == 0) r = this_array[0]%n;
            else for(var i = this.t-1; i >= 0; --i) r = (d*r+this_array[i])%n;
        return r;
    }

    // (public) 1/this % m (HAC 14.61)
    public BigInteger modInverse(BigInteger m) {
        var ac = m.isEven();
        if((this.isEven() && ac) || m.signum() == 0) return BigInteger.ZERO;
        var u = m.clone(); var v = this.clone();
        var a = nbv(1); var b = nbv(0); var c = nbv(0); var d = nbv(1);
        while(u.signum() != 0) {
            while(u.isEven()) {
                u.rShiftTo(1,u);
                if(ac) {
                    if(!a.isEven() || !b.isEven()) { a.addTo(this,a); b.subTo(m,b); }
                    a.rShiftTo(1,a);
                }
                else if(!b.isEven()) b.subTo(m,b);
                b.rShiftTo(1,b);
            }
            while(v.isEven()) {
                v.rShiftTo(1,v);
                if(ac) {
                    if(!c.isEven() || !d.isEven()) { c.addTo(this,c); d.subTo(m,d); }
                    c.rShiftTo(1,c);
                }
                else if(!d.isEven()) d.subTo(m,d);
                d.rShiftTo(1,d);
            }
            if(u.compareTo(v) >= 0) {
                u.subTo(v,u);
                if(ac) a.subTo(c,a);
                b.subTo(d,b);
            }
            else {
                v.subTo(u,v);
                if(ac) c.subTo(a,c);
                d.subTo(b,d);
            }
        }
        if(v.compareTo(BigInteger.ONE) != 0) return BigInteger.ZERO;
        if(d.compareTo(m) >= 0) return d.subtract(m);
        if(d.signum() < 0) d.addTo(m,d); else return d;
        if(d.signum() < 0) return d.add(m); else return d;
    }

    static int[] lowprimes = new int[]{2,3,5,7,11,13,17,19,23,29,31,37,41,43,47,53,59,61,67,71,73,79,83,89,97,101,103,107,109,113,127,131,137,139,149,151,157,163,167,173,179,181,191,193,197,199,211,223,227,229,233,239,241,251,257,263,269,271,277,281,283,293,307,311,313,317,331,337,347,349,353,359,367,373,379,383,389,397,401,409,419,421,431,433,439,443,449,457,461,463,467,479,487,491,499,503,509};
    static int lplim = (1<<26)/lowprimes[lowprimes.Length-1];

    // (public) test primality with certainty >= 1-.5^t
    public bool isProbablePrime(int t) {
        int i; var x = this.abs();
        var x_array = x.array;
        if(x.t == 1 && x_array[0] <= lowprimes[lowprimes.Length-1]) {
            for(i = 0; i < lowprimes.Length; ++i)
                if(x_array[0] == lowprimes[i]) return true;
            return false;
        }
        if(x.isEven()) return false;
        i = 1;
        while(i < lowprimes.Length) {
            var m = lowprimes[i]; var j = i+1;
            while(j < lowprimes.Length && m < lplim) m *= lowprimes[j++];
            m = x.modInt(m);
            while(i < j) if(m%lowprimes[i++] == 0) return false;
        }
        return x.millerRabin(t);
    }

    // (protected) true if probably prime (HAC 4.24, Miller-Rabin)
    private bool millerRabin(int t) {
        var n1 = this.subtract(BigInteger.ONE);
        var k = n1.getLowestSetBit();
        if(k <= 0) return false;
        var r = n1.shiftRight(k);
        t = (t+1)>>1;
        if(t > lowprimes.Length) t = lowprimes.Length;
        var a = nbi();
        for(var i = 0; i < t; ++i) {
            a.fromInt(lowprimes[i]);
            var y = a.modPow(r,this);
            if(y.compareTo(BigInteger.ONE) != 0 && y.compareTo(n1) != 0) {
                var j = 1;
                while(j++ < k && y.compareTo(n1) != 0) {
                    y = y.modPowInt(2,this);
                    if(y.compareTo(BigInteger.ONE) == 0) return false;
                }
                if(y.compareTo(n1) != 0) return false;
            }
        }
        return true;
    }

    public void PrintArray(String nm)
    {
        for (int kk = 0; kk < array.Count; kk++) Console.WriteLine("  {0}.array[{1}] = {2}", nm, kk, array[kk]);
    }


// BigInteger interfaces not implemented in jsbn:

// BigInteger(int signum, byte[] magnitude)
// double doubleValue()
// float floatValue()
// int hashCode()
// long longValue()
// static BigInteger valueOf(long val)
// prng4.js - uses Arcfour as a PRNG
}

abstract class RNG {
    abstract public void init(int[] key);
    abstract public int next();
}

class Arcfour: RNG {
    int i;
    int j;
    int[] S;

    public Arcfour() {
        this.i = 0;
        this.j = 0;
        this.S = new int[256];
    }

    // Initialize arcfour context from key, an array of ints, each from [0..255]
    public override void init(int[] key) {
        for(int i = 0; i < 256; ++i)
            this.S[i] = i;
        int j = 0;
        for(int i = 0; i < 256; ++i) {
            j = (j + this.S[i] + key[i % key.Length]) & 255;
            int t = this.S[i];
            this.S[i] = this.S[j];
            this.S[j] = t;
        }
        this.i = 0;
        this.j = 0;
    }

    public override int next() {
        this.i = (this.i + 1) & 255;
        this.j = (this.j + this.S[this.i]) & 255;
        int t = this.S[this.i];
        this.S[this.i] = this.S[this.j];
        this.S[this.j] = t;
        return this.S[(t + this.S[this.i]) & 255];
    }
}

class SecureRandom {
    // Pool size must be a multiple of 4 and greater than 32.
    // An array of bytes the size of the pool will be passed to init()
    const int rng_psize = 256;

    // Random number generator - requires a PRNG backend, e.g. prng4.js

    // For best results, put code like
    // <body onClick='rng_seed_time();' onKeyPress='rng_seed_time();'>
    // in your main HTML document.

    RNG rng_state;
    int[] rng_pool;
    int rng_pptr;

    public SecureRandom() {
        rng_pool = new int[rng_psize];
        rng_pptr = 0;
#if USE_RANDOM_SEED
        Random rnd = new Random();
#endif
        while(rng_pptr < rng_psize) {  // extract some randomness from Math.random()
#if USE_RANDOM_SEED
            int t = (int)Math.Floor(65536.0 * rnd.NextDouble());
#else
            int t = 1000;
#endif
            rng_pool[rng_pptr++] = (int)((uint)t >> 8);
            rng_pool[rng_pptr++] = t & 255;
        }
        rng_pptr = 0;
        rng_seed_time();
    }

    // Mix in a 32-bit integer into the pool
    private void rng_seed_int(int x) {
        rng_pool[rng_pptr++] ^= x & 255;
        rng_pool[rng_pptr++] ^= (x >> 8) & 255;
        rng_pool[rng_pptr++] ^= (x >> 16) & 255;
        rng_pool[rng_pptr++] ^= (x >> 24) & 255;
        if(rng_pptr >= rng_psize) rng_pptr -= rng_psize;
    }

    // Mix in the current time (w/milliseconds) into the pool
    private void rng_seed_time() {
#if USE_RANDOM_SEED
        rng_seed_int((int)(new DateTime().Ticks));
#endif
    }


    // Plug in your RNG constructor here
    private RNG prng_newstate() {
        return new Arcfour();
    }

    private byte rng_get_byte() {
        if(rng_state == null) {
            rng_seed_time();
            rng_state = prng_newstate();
            rng_state.init(rng_pool);
            for(rng_pptr = 0; rng_pptr < rng_pool.Length; ++rng_pptr)
                rng_pool[rng_pptr] = 0;
            rng_pptr = 0;
            //rng_pool = null;
        }
        // TODO: allow reseeding after first request
        return (byte)rng_state.next();
    }

    public void nextBytes(byte[] ba) {
        for(int i = 0; i < ba.Length; ++i) ba[i] = rng_get_byte();
    }
}

class RSAKey
{
    BigInteger n;
    int        e;
    BigInteger d;
    BigInteger p;
    BigInteger q;
    BigInteger dmp1;
    BigInteger dmq1;
    BigInteger coeff;

    // "empty" RSA key constructor
    public RSAKey() {
        this.n = null;
        this.e = 0;
        this.d = null;
        this.p = null;
        this.q = null;
        this.dmp1 = null;
        this.dmq1 = null;
        this.coeff = null;
    }

    // convert a (hex) string to a bignum object
    private static BigInteger parseBigInt(String str, int r) {
        return new BigInteger(str,r);
    }

    private static String linebrk(String s, int n) {
        var ret = "";
        var i = 0;
        while(i + n < s.Length) {
            ret += s.Substring(i,i+n) + "\n";
            i += n;
        }
        return ret + s.Substring(i,s.Length);
    }

    private static String byte2Hex(byte b) {
        if(b < 0x10)
            return "0" + b.ToString("X");
        else
            return b.ToString("X");
    }

    // PKCS#1 (type 2, random) pad input string s to n bytes, and return a bigint
    private static BigInteger pkcs1pad2(String s, int n) {
        if(n < s.Length + 11) {
            throw new ArgumentException("Message too long for RSA");
        }
        var ba = new byte[n];
        var i = s.Length - 1;
        while(i >= 0 && n > 0) ba[--n] = (byte)s[i--];
        ba[--n] = 0;
        var rng = new SecureRandom();
        byte[] x = new byte[1];
        while(n > 2) { // random non-zero pad
            x[0] = 0;
            while(x[0] == 0) rng.nextBytes(x);
            ba[--n] = x[0];
        }
        ba[--n] = 2;
        ba[--n] = 0;
        // for (int k = 0; k < ba.Length; k++) Console.WriteLine("ba[{0}] = {1}", k, (int)ba[k]);
        return new BigInteger(ba);
    }

    // Set the public key fields N and e from hex strings
    public void setPublic(String N, String E) {
        if (N != null && E != null && N.Length > 0 && E.Length > 0) {
            this.n = parseBigInt(N,16);
            this.e = Int32.Parse(E,NumberStyles.HexNumber);
        }
        else
            throw new ArgumentException("Invalid RSA public key");
    }

    // Perform raw public operation on "x": return x^e (mod n)
    private BigInteger doPublic(BigInteger x) {
        return x.modPowInt((uint)this.e, this.n);
    }

    // Return the PKCS#1 RSA encryption of "text" as an even-length hex string
    public String encrypt(String text) {
        var m = pkcs1pad2(text,(this.n.bitLength()+7)>>3);
#if TRACING
        m.PrintArray("m");
        Console.WriteLine(m.toString(10));
#endif
        if(m == null) return null;
        var c = this.doPublic(m);
        if(c == null) return null;
        var h = c.toString(16);
        if((h.Length & 1) == 0) return h; else return "0" + h;
    }

    // Return the PKCS#1 RSA encryption of "text" as a Base64-encoded string
    //function RSAEncryptB64(text) {
    //  var h = this.encrypt(text);
    //  if(h) return hex2b64(h); else return null;
    //}

    // Undo PKCS#1 (type 2, random) padding and, if valid, return the plaintext
    private String pkcs1unpad2(BigInteger d, int n) {
        var b = d.toByteArray();
        var i = 0;
        while(i < b.Length && b[i] == 0) ++i;
        if(b.Length-i != n-1 || b[i] != 2)
            return null;
        ++i;
        while(b[i] != 0)
            if(++i >= b.Length) return null;
        var ret = "";
        char[] oneChar = new char[1];
        while(++i < b.Length) {
            oneChar[0] = (char)b[i];
            ret += new String(oneChar);
        }
        return ret;
    }

    // Set the private key fields N, e, and d from hex strings
    private void setPrivate(String N, String E, String D) {
        if(N != null && E != null && N.Length > 0 && E.Length > 0) {
            this.n = parseBigInt(N,16);
            this.e = Int32.Parse(E,NumberStyles.HexNumber);
            this.d = parseBigInt(D,16);
        }
        else
            throw new ArgumentException("Invalid RSA private key");
    }

    // Set the private key fields N, e, d and CRT params from hex strings
    public void setPrivateEx(String N,
                             String E,
                             String D,
                             String P,
                             String Q,
                             String DP,
                             String DQ,
                             String C) {
        if(N != null && E != null && N.Length > 0 && E.Length > 0) {
            this.n = parseBigInt(N,16);
            this.e = Int32.Parse(E,NumberStyles.HexNumber);
            this.d = parseBigInt(D,16);
            this.p = parseBigInt(P,16);
            this.q = parseBigInt(Q,16);
            this.dmp1 = parseBigInt(DP,16);
            this.dmq1 = parseBigInt(DQ,16);
            this.coeff = parseBigInt(C,16);
        }
        else
            throw new ArgumentException("Invalid RSA private key");
    }

    // Generate a new random private key B bits long, using public expt E
    private void generate(int B, String E) {
        var rng = new SecureRandom();
        var qs = B>>1;
        this.e = Int32.Parse(E,NumberStyles.HexNumber);
        var ee = new BigInteger(E,16);
        for(;;) {
            for(;;) {
                this.p = new BigInteger(B-qs,1,rng);
                if(this.p.subtract(BigInteger.ONE).gcd(ee).compareTo(BigInteger.ONE) == 0 && this.p.isProbablePrime(10)) break;
            }
            for(;;) {
                this.q = new BigInteger(qs,1,rng);
                if(this.q.subtract(BigInteger.ONE).gcd(ee).compareTo(BigInteger.ONE) == 0 && this.q.isProbablePrime(10)) break;
            }
            if(this.p.compareTo(this.q) <= 0) {
                var t = this.p;
                this.p = this.q;
                this.q = t;
            }
            var p1 = this.p.subtract(BigInteger.ONE);
            var q1 = this.q.subtract(BigInteger.ONE);
            var phi = p1.multiply(q1);
            if(phi.gcd(ee).compareTo(BigInteger.ONE) == 0) {
                this.n = this.p.multiply(this.q);
                this.d = ee.modInverse(phi);
                this.dmp1 = this.d.mod(p1);
                this.dmq1 = this.d.mod(q1);
                this.coeff = this.q.modInverse(this.p);
                break;
            }
        }
    }

    // Perform raw private operation on "x": return x^d (mod n)
    private BigInteger doPrivate(BigInteger x) {
        if(this.p == null || this.q == null)
            return x.modPow(this.d, this.n);

        // TODO: re-calculate any missing CRT params
        var xp = x.mod(this.p).modPow(this.dmp1, this.p);
        var xq = x.mod(this.q).modPow(this.dmq1, this.q);
        
        while(xp.compareTo(xq) < 0)
            xp = xp.add(this.p);

        xp = xp.subtract(xq);

        xp = xp.multiply(this.coeff);

        xp = xp.mod(this.p);

        xp = xp.multiply(this.q);

        xp = xp.add(xq);

        return xp;
    }

    // Return the PKCS#1 RSA decryption of "ctext".
    // "ctext" is an even-length hex string and the output is a plain string.
    public String decrypt(String ctext) {
        var c = parseBigInt(ctext, 16);
        var m = this.doPrivate(c);
        if(m == null) return null;
        return pkcs1unpad2(m, (this.n.bitLength()+7)>>3);
    }
}
}

