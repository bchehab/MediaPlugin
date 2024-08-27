//using System.Runtime.InteropServices;

//using Foundation;

//namespace MPowerKit.MediaPlugin;

//public class NSDataStream : Stream
//{
//    protected NSData? Data;
//    protected uint Pos;

//    public NSDataStream(NSData data) => Data = data;

//    protected override void Dispose(bool disposing)
//    {
//        Data?.Dispose();
//        Data = null;
//    }

//    public override void Flush()
//    {
//    }

//    public override int Read(byte[] buffer, int offset, int count)
//    {
//        if (Pos >= Data!.Length) return 0;

//        var len = (int)Math.Min(count, (double)(Data.Length - Pos));

//        Marshal.Copy(new IntPtr(Convert.ToInt64(Data.Bytes) + Pos), buffer, offset, len);

//        Pos += (uint)len;

//        return len;
//    }

//    public override long Seek(long offset, SeekOrigin origin) =>
//        throw new NotSupportedException();

//    public override void SetLength(long value) =>
//        throw new NotSupportedException();

//    public override void Write(byte[] buffer, int offset, int count) =>
//        throw new NotSupportedException();

//    public override bool CanRead => true;

//    public override bool CanSeek => false;

//    public override bool CanWrite => false;

//    public override long Length => (long)Data!.Length;

//    public override long Position
//    {
//        get => Pos;
//        set
//        {
//        }
//    }
//}