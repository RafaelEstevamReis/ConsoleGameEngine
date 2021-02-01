using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using Simple.CGE.Interfaces;

namespace Simple.CGE.DrawEngines
{
    public class FastDraw : IDrawEngine
    {
        public RectangleF GameBorder { get; private set; }
        readonly short sWidth;
        readonly short sHeight;
        readonly short sFontH;
        readonly short sFontW;

        private char[] screenBuffer;
        private char[] emptyScreenBuffer;
        private CharInfo[] consoleBuffer;

        private SmallRect cachedScreenRect;
        private Coord cachedScreenCoord;

        #region dll imports
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteConsoleOutputW(
            SafeFileHandle hConsoleOutput,
            CharInfo[] lpBuffer,
            Coord dwBufferSize,
            Coord dwBufferCoord,
            ref SmallRect lpWriteRegion);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleWindowInfo(
            SafeFileHandle hConsoleOutput,
            bool bAbsolute,
            ref SmallRect lpConsoleWindow);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleScreenBufferSize(
            SafeFileHandle hConsoleOutput,
            Coord dwSize);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleActiveScreenBuffer(SafeFileHandle hConsoleOutput);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetCurrentConsoleFontEx(
            SafeFileHandle ConsoleOutput,
            bool MaximumWindow,
            ref ConsoleFontInfoEx ConsoleCurrentFontEx);

        [StructLayout(LayoutKind.Sequential)]
        private struct Coord
        {
            public short X;
            public short Y;

            public Coord(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }

            public static readonly Coord Zero = new Coord(0, 0);
        };

        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
        public struct CharUnion
        {
            [FieldOffset(0)] public char UnicodeChar;
            [FieldOffset(0)] public byte AsciiChar;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct CharInfo
        {
            [FieldOffset(0)] public CharUnion Char;
            [FieldOffset(2)] public short Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SmallRect
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;

            public SmallRect(short Left, short Top, short Right, short Bottom)
            {
                this.Left = Left;
                this.Top = Top;
                this.Right = Right;
                this.Bottom = Bottom;
            }

            public static readonly SmallRect Zero = new SmallRect(0, 0, 0, 0);
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct ConsoleFontInfoEx
        {
            public uint cbSize;
            public uint nFont;
            public Coord dwFontSize;
            public int FontFamily;
            public int FontWeight;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string FaceName;
        }
        #endregion

        SafeFileHandle h;

        public FastDraw(Size WindowSizeInCharacters, Size FontSizeInPixels)
        {
            sWidth = (short)WindowSizeInCharacters.Width;
            sHeight = (short)WindowSizeInCharacters.Height;

            sFontW = (short)FontSizeInPixels.Width;
            sFontH = (short)FontSizeInPixels.Height;
        }

        public void Setup()
        {
            h = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            if (h.IsInvalid) throw new ExternalException("Could not open console");

            Console.CursorVisible = false;
            createConsole();

            GameBorder = new RectangleF(0, 0, sWidth, sHeight);
            screenBuffer = new char[sWidth * sHeight];
            emptyScreenBuffer = new char[sWidth * sHeight];
            Array.Fill(emptyScreenBuffer, ' ');
            cachedScreenRect = new SmallRect(0,0,sWidth,sHeight);
            cachedScreenCoord = new Coord(sWidth, sHeight);

            consoleBuffer = new CharInfo[sWidth * sHeight];
            for (int i = 0; i < screenBuffer.Length; i++)
            {
                consoleBuffer[i] = new CharInfo()
                {
                    Attributes = 7,
                    Char = new CharUnion()
                    {
                        UnicodeChar = ' ',
                    }
                };
            }

        }

        private int createConsole()
        {
            // Set window to a very low value to allow buffer change
            SmallRect rect = new SmallRect(0, 0, 1, 1);
            if (!SetConsoleWindowInfo(h, true, ref rect)) throw new InvalidOperationException("Failed to SetConsoleWindowInfo [1]");
            // Set new screen buffer
            Coord coord = new Coord(sWidth, sHeight);
            if (!SetConsoleScreenBufferSize(h, coord)) throw new InvalidOperationException("Failed to SetConsoleScreenBufferSize");
            // Set current buffer as Active
            if(!SetConsoleActiveScreenBuffer(h)) throw new InvalidOperationException("Failed to SetConsoleActiveScreenBuffer");
            // set font size
            var font = new ConsoleFontInfoEx()
            {
                nFont = 0,
                dwFontSize = new Coord()
                {
                    X = sFontW,
                    Y = sFontH,
                },
                FontFamily = 0, // DONTCARE
                FontWeight = 400, // FW_NORMAL
                FaceName = "Consolas",
            };
            font.cbSize = (uint)Marshal.SizeOf(font);
            if (!SetCurrentConsoleFontEx(h, false, ref font)) throw new InvalidOperationException("Failed to SetCurrentConsoleFontEx");

            // get maximum buffer size
            if (sWidth > Console.LargestWindowWidth) throw new InvalidOperationException("Width is too big for current console");
            if (sHeight > Console.LargestWindowHeight) throw new InvalidOperationException("Height is too big for current console");

            // return console window size
            rect = new SmallRect(0, 0, (short)(sWidth - 1), (short)(sHeight - 1));
            if (!SetConsoleWindowInfo(h, true, ref rect)) throw new InvalidOperationException("Failed to SetConsoleWindowInfo [2]");

            return -1;
        }

        public void PreFrame()
        {
        }

        public void DrawStart(FrameData data)
        {
            //for (int i = 0; i < screenBuffer.Length; i++) screenBuffer[i] = ' ';
            //Array.Fill(screenBuffer, ' ');
            Array.Copy(emptyScreenBuffer, screenBuffer, screenBuffer.Length);
        }
        public void StartFrame(FrameData data, DrawLayers layer) { }
        public void EndFrame(FrameData data, DrawLayers layer) { }
        public void DrawFinish(FrameData data)
        {
            for (int i = 0; i < screenBuffer.Length; i++)
            {
                consoleBuffer[i].Char.UnicodeChar = screenBuffer[i];
            }

            bool b = WriteConsoleOutputW(h, consoleBuffer,
              cachedScreenCoord,
              Coord.Zero,
              ref cachedScreenRect);
        }

        public void PosFrame()
        {
        }

        public void DrawLine(int left, int top, string text)
        {
            if (top < 0) return;
            if (top >= GameBorder.Height) return;
            if (text.Length <= -left) return;
            if (left >= GameBorder.Width) return;

            if (left < 0)
            {
                int skip = -left;
                left = 0;
                text = text[skip..];
            }
            if (left + text.Length > GameBorder.Width)
            {
                text = text[..((int)GameBorder.Width - left)];
            }
            int offset = top * (int)GameBorder.Width + left;
            for (int i = 0; i < text.Length; i++)
            {
                screenBuffer[i + offset] = text[i];
            }
        }
        public void DrawRectangle(RectangleF rectangle, char[] data)
        {
            if (!rectangle.IntersectsWith(GameBorder)) return;

            var linesToPrint = getLines(data, (int)rectangle.Width).ToArray();

            for (int i = 0; i < linesToPrint.Length; i++)
            {
                int top = i + (int)rectangle.Top;
                int left = 0 + (int)rectangle.Left;

                DrawLine(left, top, linesToPrint[i]);
            }
        }
        IEnumerable<string> getLines(char[] data, int width)
        {
            var sb = new StringBuilder();

            foreach (var d in data)
            {
                sb.Append(d);

                if (sb.Length == width)
                {
                    yield return sb.ToString();
                    sb.Clear();
                }
            }
            if (sb.Length > 0) yield return sb.ToString();
        }

    }
}
