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
        int sWidth;
        int sHeight;

        private char[] screenBuffer;
        private CharInfo[] consoleBuffer;

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
        }
        #endregion

        SafeFileHandle h;
        public void Setup()
        {
            Console.CursorVisible = false;
            sWidth = Console.WindowWidth;
            sHeight = Console.WindowHeight;
            GameBorder = new RectangleF(0, 0, sWidth, sHeight);
            screenBuffer = new char[sWidth * sHeight];
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

            h = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
        }

        public void PreFrame()
        {
        }

        public void DrawStart(FrameData data)
        {
            for (int i = 0; i < screenBuffer.Length; i++) screenBuffer[i] = ' ';
        }
        public void StartFrame(FrameData data, DrawLayers layer) { }
        public void EndFrame(FrameData data, DrawLayers layer) { }
        public void DrawFinish(FrameData data)
        {
            for (int i = 0; i < screenBuffer.Length; i++)
            {
                consoleBuffer[i].Char.UnicodeChar = screenBuffer[i];
            }

            SmallRect rect = new SmallRect()
            {
                Left = 0,
                Top = 0,
                Right = (short)sWidth,
                Bottom = (short)sHeight
            };

            bool b = WriteConsoleOutputW(h, consoleBuffer,
              new Coord() { X = rect.Right, Y = rect.Bottom },
              new Coord() { X = 0, Y = 0 },
              ref rect);
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
