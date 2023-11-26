﻿// BLAKE2 reference source code package - C# implementation

// Written in 2012 by Christian Winnerlein  <codesinchaos@gmail.com>

// To the extent possible under law, the author(s) have dedicated all copyright
// and related and neighboring rights to this software to the public domain
// worldwide. This software is distributed without any warranty.

// You should have received a copy of the CC0 Public Domain Dedication along with
// this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
//
/*
  Based on BlakeSharp
  by Dominik Reichl <dominik.reichl@t-online.de>
  Web: http://www.dominik-reichl.de/
  If you're using this class, it would be nice if you'd mention
  me somewhere in the documentation of your program, but it's
  not required.

  BLAKE was designed by Jean-Philippe Aumasson, Luca Henzen,
  Willi Meier and Raphael C.-W. Phan.
  BlakeSharp was derived from the reference C implementation.
*/

namespace Blake2Sharp
{
    public sealed partial class Blake2BCore
    {
        private bool _isInitialized = false;

        private int _bufferFilled;
        private byte[] _buf = new byte[128];

        private ulong[] _m = new ulong[16];
        private ulong[] _h = new ulong[8];
        private ulong _counter0;
        private ulong _counter1;
        private ulong _finalizationFlag0;
        private ulong _finalizationFlag1;

        private const int NumberOfRounds = 12;
        private const int BlockSizeInBytes = 128;

        private const ulong IV0 = 0x6A09E667F3BCC908UL;
        private const ulong IV1 = 0xBB67AE8584CAA73BUL;
        private const ulong IV2 = 0x3C6EF372FE94F82BUL;
        private const ulong IV3 = 0xA54FF53A5F1D36F1UL;
        private const ulong IV4 = 0x510E527FADE682D1UL;
        private const ulong IV5 = 0x9B05688C2B3E6C1FUL;
        private const ulong IV6 = 0x1F83D9ABFB41BD6BUL;
        private const ulong IV7 = 0x5BE0CD19137E2179UL;

        internal static ulong BytesToUInt64(byte[] buf, int offset)
        {
            return
                ((ulong)buf[offset + 7] << 7 * 8 |
                ((ulong)buf[offset + 6] << 6 * 8) |
                ((ulong)buf[offset + 5] << 5 * 8) |
                ((ulong)buf[offset + 4] << 4 * 8) |
                ((ulong)buf[offset + 3] << 3 * 8) |
                ((ulong)buf[offset + 2] << 2 * 8) |
                ((ulong)buf[offset + 1] << 1 * 8) |
                ((ulong)buf[offset]));
        }

        private static void UInt64ToBytes(ulong value, byte[] buf, int offset)
        {
            buf[offset + 7] = (byte)(value >> 7 * 8);
            buf[offset + 6] = (byte)(value >> 6 * 8);
            buf[offset + 5] = (byte)(value >> 5 * 8);
            buf[offset + 4] = (byte)(value >> 4 * 8);
            buf[offset + 3] = (byte)(value >> 3 * 8);
            buf[offset + 2] = (byte)(value >> 2 * 8);
            buf[offset + 1] = (byte)(value >> 1 * 8);
            buf[offset] = (byte)value;
        }

        private partial void Compress(byte[] block, int start);

        public void Initialize(ulong[] config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (config.Length != 8)
                throw new ArgumentException("config length must be 8 words");
            _isInitialized = true;

            _h[0] = IV0;
            _h[1] = IV1;
            _h[2] = IV2;
            _h[3] = IV3;
            _h[4] = IV4;
            _h[5] = IV5;
            _h[6] = IV6;
            _h[7] = IV7;

            _counter0 = 0;
            _counter1 = 0;
            _finalizationFlag0 = 0;
            _finalizationFlag1 = 0;

            _bufferFilled = 0;

            Array.Clear(_buf, 0, _buf.Length);

            for (int i = 0; i < 8; i++)
                _h[i] ^= config[i];
        }

        public void HashCore(byte[] array, int start, int count)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Not initialized");
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            if ((long)start + (long)count > array.Length)
                throw new ArgumentOutOfRangeException("start+count");
            int offset = start;
            int bufferRemaining = BlockSizeInBytes - _bufferFilled;

            if ((_bufferFilled > 0) && (count > bufferRemaining))
            {
                Array.Copy(array, offset, _buf, _bufferFilled, bufferRemaining);
                _counter0 += BlockSizeInBytes;
                if (_counter0 == 0)
                    _counter1++;
                Compress(_buf, 0);
                offset += bufferRemaining;
                count -= bufferRemaining;
                _bufferFilled = 0;
            }

            while (count > BlockSizeInBytes)
            {
                _counter0 += BlockSizeInBytes;
                if (_counter0 == 0)
                    _counter1++;
                Compress(array, offset);
                offset += BlockSizeInBytes;
                count -= BlockSizeInBytes;
            }

            if (count > 0)
            {
                Array.Copy(array, offset, _buf, _bufferFilled, count);
                _bufferFilled += count;
            }
        }

        public byte[] HashFinal()
        {
            return HashFinal(false);
        }

        public byte[] HashFinal(bool isEndOfLayer)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Not initialized");
            _isInitialized = false;

            //Last compression
            _counter0 += (uint)_bufferFilled;
            _finalizationFlag0 = ulong.MaxValue;
            if (isEndOfLayer)
                _finalizationFlag1 = ulong.MaxValue;
            for (int i = _bufferFilled; i < _buf.Length; i++)
                _buf[i] = 0;
            Compress(_buf, 0);

            //Output
            byte[] hash = new byte[64];
            for (int i = 0; i < 8; ++i)
                UInt64ToBytes(_h[i], hash, i << 3);
            return hash;
        }
    }
}