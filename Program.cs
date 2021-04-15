using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Runtime.InteropServices;
using System.Drawing;


namespace FrameBuffer
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FrameBufferInfo 
    {
        public int fd;
        public IntPtr data;
        public uint w;
        public uint h;
        public uint bpp;
        public uint line_length;
    }

    class Program
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct fb_fix_screeninfo {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] id;
            [MarshalAs(UnmanagedType.U4)] public uint smem_start;
            [MarshalAs(UnmanagedType.U4)] public uint smem_len;
            [MarshalAs(UnmanagedType.U4)] public uint type;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)] public byte[] stuff;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct fb_var_screeninfo {
            public int xres;
            public int yres;
            public int xres_virtual;
            public int yres_virtual;
            public int xoffset;
            public int yoffset;
            public int bits_per_pixel;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 132)] public byte[] stuff;
        };


        [DllImport("libc", EntryPoint = "close", SetLastError = true)]
        static extern int Close(int handle);

        [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
        static extern int Ioctl(int handle, uint request, ref fb_fix_screeninfo capability);


        [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
        static extern int Ioctl(int handle, uint request, ref fb_var_screeninfo capability);

        [DllImport("libc", EntryPoint = "open", SetLastError = true)]
        static extern int Open(string path, uint flag);

        [DllImport("libc", EntryPoint = "mmap", SetLastError = true)]
        public static extern int Mmap(
            [MarshalAs(UnmanagedType.U4)] uint addr,
            [MarshalAs(UnmanagedType.U4)] uint length,
            [MarshalAs(UnmanagedType.I4)] int prot,
            [MarshalAs(UnmanagedType.I4)] int flags,
            [MarshalAs(UnmanagedType.I4)] int fdes,
            [MarshalAs(UnmanagedType.I4)] int offset
        );

        [DllImport("libc", EntryPoint = "munmap", SetLastError = true)]
        public static extern int Munmap(
            [MarshalAs(UnmanagedType.I4)] int addr,
            [MarshalAs(UnmanagedType.U4)] uint length
        );

        static T ByteArrayToStructure<T>(byte[] bytes) where T: struct 
        {
            T stuff;
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
            return stuff;
        }

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("missing device path");
                return;
            }

            // var handle = Open(args[0], 2);
            // if(handle == -1)
            // {
            //     Console.WriteLine($"Open {args[0]} failed.");
            //     return;
            // }

            // // #define FBIOGET_VSCREENINFO	0x4600
            // // #define FBIOPUT_VSCREENINFO	0x4601
            // // #define FBIOGET_FSCREENINFO	0x4602
            // // #define FBIOGETCMAP		0x4604
            // // #define FBIOPUTCMAP		0x4605
            // // #define FBIOPAN_DISPLAY		0x4606
            // fb_fix_screeninfo fix = new fb_fix_screeninfo();
            // fb_var_screeninfo v = new fb_var_screeninfo();
            // if(Ioctl(handle, 0x4602, ref fix) < 0 || Ioctl(handle,0x4600, ref v)<0)
            // {
            //     Console.WriteLine("Ioctl error");
            // }
            // Console.WriteLine($"mem start:{fix.smem_start} mem len:{fix.smem_len}");
            // Console.WriteLine($"x:{v.xres} y:{v.yres} pix:{v.bits_per_pixel}");

            // //#define PROT_READ       0x1                /* Page can be read.  */
            // //#define PROT_WRITE      0x2                /* Page can be written.  */
            // //#define MAP_SHARED      0x01               /* Share changes.  */
            // var fbs = Mmap(0, fix.smem_len,0x3, 0x01,handle,0);
            // if(fbs == -1)
            // {
            //     Console.WriteLine("Map failed");
            // }

            // Random rand = new Random();
            // while(true)
            // {
            //     var pos = rand.Next(v.xres * v.yres);
            //     Marshal.WriteInt32(new IntPtr(fbs + pos * 4), rand.Next());
            //     //Thread.Sleep(1);
            // }

            // Munmap(fbs, fix.smem_len);
            // Close(handle);

            var mmap = MemoryMappedFile.CreateFromFile(args[0], FileMode.Open, null, 800 * 600 * 4);
            var stream = mmap.CreateViewStream();

            fb_fix_screeninfo fix = new fb_fix_screeninfo();
            fb_var_screeninfo v = new fb_var_screeninfo();
            if (Ioctl(stream.SafeMemoryMappedViewHandle.DangerousGetHandle().ToInt32(), 0x4602, ref fix) < 0 || Ioctl(stream.SafeMemoryMappedViewHandle.DangerousGetHandle().ToInt32(), 0x4600, ref v) < 0)
            {
                Console.WriteLine($"handle:{stream.SafeMemoryMappedViewHandle.DangerousGetHandle().ToInt32()} Ioctl error");
            }
            Console.WriteLine($"mem start:{fix.smem_start} mem len:{fix.smem_len}");
            Console.WriteLine($"x:{v.xres} y:{v.yres} pix:{v.bits_per_pixel}");

            Random rand = new Random();
            Console.WriteLine($"handle:{mmap} length:{stream.Length}");
            //var color = new byte[4];
            var pngStream = File.OpenRead("sample.bmp");
            var buffer = new byte[800 * 600 * 3];
            pngStream.Seek(56, SeekOrigin.Begin);
            pngStream.Read(buffer, 0, buffer.Length);
            for (int i = 0; i < 800 * 600; i += 3)
            {
                stream.Write(buffer, i, 3);
                stream.Seek(1, SeekOrigin.Current);
            }
            stream.Close();

        }
    }
}
