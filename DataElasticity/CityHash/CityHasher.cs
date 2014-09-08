#region usings

using System.Text;

#endregion

namespace Microsoft.AzureCat.Patterns.CityHash
{
    /// <summary>
    /// Logic inspired by combination of C code from http://code.google.com/p/cityhash/source/checkout
    /// and java code from https://github.com/tamtam180/CityHash-For-Java/blob/master/src/main/java/at/orz/hash/CityHash.java
    /// </summary>
    public class CityHasher
    {
        #region fields

        private const ulong k0 = 0xc3a5c85c97cb3127L;
        private const ulong k1 = 0xb492b66fbe98f273L;
        private const ulong k2 = 0x9ae16a3b2f90404fL;
        private const ulong k3 = 0xc949d7c7509e6557L;
        private const ulong kMul = 0x9ddfea08eb382d69L;

        #endregion

        #region methods

        public static ulong CityHash64(byte[] s, int pos, int len)
        {
            if (len <= 32)
            {
                if (len <= 16)
                {
                    return HashLen0to16(s, pos, len);
                }
                return HashLen17to32(s, pos, len);
            }
            if (len <= 64)
            {
                return HashLen33to64(s, pos, len);
            }

            var x = Fetch64(s, pos + len - 40);
            var y = Fetch64(s, pos + len - 16) + Fetch64(s, pos + len - 56);
            var z = HashLen16(Fetch64(s, pos + len - 48) + (ulong) len, Fetch64(s, pos + len - 24));

            var v = WeakHashLen32WithSeeds(s, pos + len - 64, (ulong) len, z);
            var w = WeakHashLen32WithSeeds(s, pos + len - 32, y + k1, x);
            x = x*k1 + Fetch64(s, pos + 0);

            len = (len - 1) & (~63);
            do
            {
                x = Rotate(x + y + v[0] + Fetch64(s, pos + 8), 37)*k1;
                y = Rotate(y + v[1] + Fetch64(s, pos + 48), 42)*k1;
                x ^= w[1];
                y += v[0] + Fetch64(s, pos + 40);
                z = Rotate(z + w[0], 33)*k1;
                v = WeakHashLen32WithSeeds(s, pos + 0, v[1]*k1, x + w[0]);
                w = WeakHashLen32WithSeeds(s, pos + 32, z + w[1], y + Fetch64(s, pos + 16));
                {
                    var swap = z;
                    z = x;
                    x = swap;
                }
                pos += 64;
                len -= 64;
            } while (len != 0);

            return HashLen16(
                HashLen16(v[0], w[0]) + ShiftMix(y)*k1 + z,
                HashLen16(v[1], w[1]) + x
                );
        }

        public static ulong CityHash64String(string s)
        {
            var encoding = new UTF8Encoding();
            var bytes = encoding.GetBytes(s);
            return CityHash64(bytes, 0, bytes.Length);
        }

        public static long CityHash64StringGetLong(string s)
        {
            return (long) CityHash64String(s);
        }

        public static ulong CityHash64WithSeed(byte[] s, int pos, int len, ulong seed)
        {
            return CityHash64WithSeeds(s, pos, len, k2, seed);
        }

        public static ulong CityHash64WithSeeds(byte[] s, int pos, int len, ulong seed0, ulong seed1)
        {
            return HashLen16(CityHash64(s, pos, len) - seed0, seed1);
        }

        private static ulong Fetch32(byte[] b, int i)
        {
            return (ulong) ((((long) b[i + 3] & 255) << 24) +
                            ((b[i + 2] & 255) << 16) +
                            ((b[i + 1] & 255) << 8) +
                            ((b[i + 0] & 255) << 0));
        }

        private static ulong Fetch64(byte[] b, int i)
        {
            return (ulong)
                (((long) b[i + 7] << 56) +
                 ((long) (b[i + 6] & 255) << 48) +
                 ((long) (b[i + 5] & 255) << 40) +
                 ((long) (b[i + 4] & 255) << 32) +
                 ((long) (b[i + 3] & 255) << 24) +
                 ((b[i + 2] & 255) << 16) +
                 ((b[i + 1] & 255) << 8) +
                 ((b[i + 0] & 255) << 0));
        }

        private static ulong ForceRotate(ulong val, int shift)
        {
            return ((val >> shift) | (val << (64 - shift)));
        }

        private static ulong Hash128to64(ulong u, ulong v)
        {
            var a = (u ^ v)*kMul;
            a ^= (a >> 47);
            var b = (v ^ a)*kMul;
            b ^= (b >> 47);
            b *= kMul;
            return b;
        }

        private static ulong HashLen0to16(byte[] s, int pos, int len)
        {
            if (len > 8)
            {
                var a = Fetch64(s, pos + 0);
                var b = Fetch64(s, pos + len - 8);
                return HashLen16(a, ForceRotate((b + (ulong) len), len)) ^ b;
            }
            if (len >= 4)
            {
                var a = 0xffffffffL & Fetch32(s, pos + 0);
                return HashLen16(((a << 3) + (ulong) len), 0xffffffffL & Fetch32(s, pos + len - 4));
            }
            if (len > 0)
            {
                var a = s[0];
                var b = s[len >> 1];
                var c = s[len - 1];
                var y = a + (((uint) (b)) << 8);
                var z = (uint) len + ((uint) (c) << 2);
                return ShiftMix(y*k2 ^ z*k0)*k2;
            }
            return k2;
        }

        private static ulong HashLen16(ulong u, ulong v)
        {
            return Hash128to64(u, v);
        }

        private static ulong HashLen17to32(byte[] s, int pos, int len)
        {
            var a = Fetch64(s, pos + 0)*k1;
            var b = Fetch64(s, pos + 8);
            var c = Fetch64(s, pos + len - 8)*k2;
            var d = Fetch64(s, pos + len - 16)*k0;
            return HashLen16(
                Rotate(a - b, 43) + Rotate(c, 30) + d,
                a + Rotate(b ^ k3, 20) - c + (ulong) len
                );
        }

        private static ulong HashLen33to64(byte[] s, int pos, int len)
        {
            var z = Fetch64(s, pos + 24);
            var a = Fetch64(s, pos + 0) + (Fetch64(s, pos + len - 16) + (ulong) len)*k0;
            var b = Rotate(a + z, 52);
            var c = Rotate(a, 37);

            a += Fetch64(s, pos + 8);
            c += Rotate(a, 7);
            a += Fetch64(s, pos + 16);

            var vf = a + z;
            var vs = b + Rotate(a, 31) + c;

            a = Fetch64(s, pos + 16) + Fetch64(s, pos + len - 32);
            z = Fetch64(s, pos + len - 8);
            b = Rotate(a + z, 52);
            c = Rotate(a, 37);
            a += Fetch64(s, pos + len - 24);
            c += Rotate(a, 7);
            a += Fetch64(s, pos + len - 16);

            var wf = a + z;
            var ws = b + Rotate(a, 31) + c;
            var r = ShiftMix((vf + ws)*k2 + (wf + vs)*k0);

            return ShiftMix(r*k0 + vs)*k2;
        }

        private static ulong Rotate(ulong val, int shift)
        {
            return shift == 0 ? val : ((val >> shift) | (val << (64 - shift)));
        }

        private static ulong ShiftMix(ulong val)
        {
            return val ^ (val >> 47);
        }

        private static ulong[] WeakHashLen32WithSeeds(
            ulong w, ulong x, ulong y, ulong z,
            ulong a, ulong b)
        {
            a += w;
            b = Rotate(b + a + z, 21);
            var c = a;
            a += x;
            a += y;
            b += Rotate(a, 44);
            return new[] {a + z, b + c};
        }

        private static ulong[] WeakHashLen32WithSeeds(byte[] s, int pos, ulong a, ulong b)
        {
            return WeakHashLen32WithSeeds(
                Fetch64(s, pos + 0),
                Fetch64(s, pos + 8),
                Fetch64(s, pos + 16),
                Fetch64(s, pos + 24),
                a,
                b
                );
        }

        #endregion
    }
}