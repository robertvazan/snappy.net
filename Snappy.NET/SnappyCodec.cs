using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snappy
{
    public static class SnappyCodec
    {
        public static unsafe int Compress(byte[] input, int offset, int length, byte[] output, int outOffset)
        {
            if (input == null || output == null)
                throw new ArgumentNullException();
            if (offset < 0 || length < 0 || offset + length > input.Length)
                throw new ArgumentOutOfRangeException("Selected range is outside the bounds of the input array");
            if (outOffset < 0 || outOffset >= output.Length)
                throw new ArgumentOutOfRangeException("Output offset is outside the bounds of the output array");
            int outLength = output.Length - outOffset;
            if (offset == input.Length)
            {
                input = new byte[1];
                offset = 0;
            }
            fixed (byte *inputPtr = &input[offset])
            fixed (byte* outputPtr = &output[outOffset])
            {
                var status = NativeProxy.Instance.Compress(inputPtr, length, outputPtr, ref outLength);
                if (status == SnappyStatus.Ok)
                    return outLength;
                else if (status == SnappyStatus.BufferTooSmall)
                    throw new ArgumentOutOfRangeException("Output array is too small");
                else
                    throw new ArgumentException("Invalid input");
            }
        }

        public static byte[] Compress(byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException();
            var max = GetMaxCompressedLength(input.Length);
            var output = new byte[max];
            int outLength = Compress(input, 0, input.Length, output, 0);
            if (outLength == max)
                return output;
            var truncated = new byte[outLength];
            Array.Copy(output, truncated, outLength);
            return truncated;
        }

        public static unsafe int Uncompress(byte[] input, int offset, int length, byte[] output, int outOffset)
        {
            if (input == null || output == null)
                throw new ArgumentNullException();
            if (offset < 0 || length < 0 || offset + length > input.Length)
                throw new ArgumentOutOfRangeException("Selected range is outside the bounds of the input array");
            if (length == 0)
                throw new ArgumentOutOfRangeException("Compressed block cannot be empty");
            if (outOffset < 0 || outOffset > output.Length)
                throw new ArgumentOutOfRangeException("Output offset is outside the bounds of the output array");
            int outLength = output.Length - outOffset;
            if (outOffset == output.Length)
            {
                output = new byte[1];
                outOffset = 0;
            }
            fixed (byte* inputPtr = &input[offset])
            fixed (byte* outputPtr = &output[outOffset])
            {
                var status = NativeProxy.Instance.Uncompress(inputPtr, length, outputPtr, ref outLength);
                if (status == SnappyStatus.Ok)
                    return outLength;
                else if (status == SnappyStatus.BufferTooSmall)
                    throw new ArgumentOutOfRangeException("Output array is too small");
                else
                    throw new ArgumentException("Input is not a valid snappy-compressed block");
            }
        }

        public static byte[] Uncompress(byte[] input)
        {
            var max = GetUncompressedLength(input);
            var output = new byte[max];
            int outLength = Uncompress(input, 0, input.Length, output, 0);
            if (outLength == max)
                return output;
            var truncated = new byte[outLength];
            Array.Copy(output, truncated, outLength);
            return truncated;
        }

        public static int GetMaxCompressedLength(int inLength)
        {
            return NativeProxy.Instance.GetMaxCompressedLength(inLength);
        }

        public static unsafe int GetUncompressedLength(byte[] input, int offset, int length)
        {
            if (input == null)
                throw new ArgumentNullException();
            if (offset < 0 || length < 0 || offset + length > input.Length)
                throw new ArgumentOutOfRangeException("Selected range is outside the bounds of the input array");
            if (length == 0)
                throw new ArgumentOutOfRangeException("Compressed block cannot be empty");
            fixed (byte* inputPtr = &input[offset])
            {
                int outLength;
                var status = NativeProxy.Instance.GetUncompressedLength(inputPtr, length, out outLength);
                if (status == SnappyStatus.Ok)
                    return outLength;
                else
                    throw new ArgumentException("Input is not a valid snappy-compressed block");
            }
        }

        public static int GetUncompressedLength(byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException();
            return GetUncompressedLength(input, 0, input.Length);
        }

        public static unsafe bool Validate(byte[] input, int offset, int length)
        {
            if (input == null)
                throw new ArgumentNullException();
            if (offset < 0 || length < 0 || offset + length > input.Length)
                throw new ArgumentOutOfRangeException("Selected range is outside the bounds of the input array");
            if (length == 0)
                throw new ArgumentOutOfRangeException("Compressed block cannot be empty");
            fixed (byte* inputPtr = &input[offset])
                return NativeProxy.Instance.ValidateCompressedBuffer(inputPtr, length) == SnappyStatus.Ok;
        }

        public static bool Validate(byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException();
            return Validate(input, 0, input.Length);
        }
    }
}
