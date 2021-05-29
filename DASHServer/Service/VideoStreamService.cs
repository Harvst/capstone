using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.InteropServices;
using DexterLib;
using System.Drawing.Imaging;
/*
  Copyright 2006 shahin@themorningoutline.com
  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License. 
  You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0 
  Unless required by applicable law or agreed to in writing, software 
  distributed under the License is distributed on an "AS IS" BASIS, 
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
  See the License for the specific language governing permissions and limitations 
  under the License.
  
 */

namespace DASHServer.Service
{
    public class VideoStreamService
    {

        /// <summary>
        /// This method converts a portion of a video file to a string array of Base64 encoded images
        /// starting at startPosition and edning at endPosition 
        /// </summary>
        /// <param name="videoFileName"> path to the video file e.g. c:\test.avi</param>
        /// <param name="startPosition">  start position in seconds  (</param>
        /// <param name="endPosition"> end position in seconds</param>
        /// <param name="outStreamLength">  length of the entire video clip</param>
        /// <param name="outFrameRate">frame rate</param>
        /// <param name="outClipSize"> height and width of the video</param>
        /// <param name="strLogo">if is not left blank, writes strLogo on each frame </param>
        /// <returns></returns>
        public static string[] getBase64FrameArray(string videoFileName, double startPosition, double endPosition,
            out double outStreamLength, out double outFrameRate, out Size outClipSize, string strLogo)
        {
            try
            {
                MediaDet mediaClass = new MediaDet();
                _AMMediaType mediaType;
                outClipSize = Size.Empty;
                mediaClass.Filename = videoFileName;
                outFrameRate = 0.0;
                outStreamLength = mediaClass.StreamLength;
                int outputStreams = mediaClass.OutputStreams;
                for (int i = 0; i < outputStreams; i++)
                {
                    mediaClass.CurrentStream = i;
                    try
                    {
                        //If it can the get the framerate, it's enough, we accept the video file otherwise it throws an exception here
                        outFrameRate = mediaClass.FrameRate;
                        mediaType = mediaClass.StreamMediaType;
                        VIDEOINFOHEADER videoInfo = (VIDEOINFOHEADER)Marshal.PtrToStructure(mediaType.pbFormat, typeof(VIDEOINFOHEADER));
                        outClipSize = new Size(videoInfo.bmiHeader.biWidth, videoInfo.bmiHeader.biHeight);
                        outStreamLength = mediaClass.StreamLength;
                        mediaType = mediaClass.StreamMediaType;
                        break;
                    }
                    catch
                    {
                        // Not a valid meddia type? Go to the next outputstream
                    }

                }
                // No framerate?
                if (outFrameRate == 0.0) throw new NotSupportedException(" The program is unable to read the video file.");

                // We have a framerate? move on...
                string[] strFrameArray;
                double currentStreamPos = startPosition;
                if (endPosition < 0) //get the whole video file. It is a convention, if you want the whole video, pass in a negative end
                {
                    currentStreamPos = 0.0;
                    endPosition = outStreamLength;
                }
                Int64 frameCount = Convert.ToInt64((endPosition - startPosition) * outFrameRate);
                if (frameCount == 0) frameCount++; //Rounding issue
                double frameDuration = ((endPosition - startPosition) / frameCount);
                //if (frameDuration < 0.1) frameDuration = 0.1; //Don't get more that 10 frames per sec
                strFrameArray = new string[frameCount];
                currentStreamPos += 0.001;  //Rounding issue, aggain!
                Bitmap vdeoBitmaps;
                Graphics graphicImage;
                Font myFont = new Font("Arial", 8);
                System.IO.MemoryStream memory = new System.IO.MemoryStream();
                IntPtr ptrRefFrameBufSize;
                int bufferSize = 0; //Strores the size of our frame
                unsafe
                {
                    // No fixed keyword here because we don't manipulate managed objects
                    byte* ptrRefFramesBuffer = null;
                    // Get the size of a frame in order to dedicate enough memory to store the Bitmaps in our pointer
                    mediaClass.GetBitmapBits(0.0, ref bufferSize, ref *ptrRefFramesBuffer, outClipSize.Width, outClipSize.Height);
                    // Dedicate memory to the pointer
                    ptrRefFrameBufSize = System.Runtime.InteropServices.Marshal.AllocHGlobal(bufferSize);
                    ptrRefFramesBuffer = (byte*)ptrRefFrameBufSize.ToPointer();
                    //Do this until we reach at the end of requested stream
                    while (currentStreamPos < endPosition)
                    {
                        //Get the Bitmaps
                        mediaClass.GetBitmapBits(currentStreamPos, ref bufferSize, ref *ptrRefFramesBuffer, outClipSize.Width, outClipSize.Height);
                        vdeoBitmaps = new Bitmap(outClipSize.Width, outClipSize.Height, outClipSize.Width * 3, System.Drawing.Imaging.PixelFormat.Format24bppRgb, new IntPtr(ptrRefFramesBuffer + 40));
                        vdeoBitmaps.RotateFlip(RotateFlipType.Rotate180FlipX);
                        // Print the logo if any
                        if (strLogo != "")
                        {
                            graphicImage = Graphics.FromImage(vdeoBitmaps);
                            graphicImage.DrawString(strLogo, myFont, SystemBrushes.WindowText, 5, 5);
                        }
                        // Save the Bitmpas somewhere in the (managed) memory 
                        vdeoBitmaps.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);
                        // We are going backwards! Otherwise the frames would be put in the wrong order in the array
                        frameCount--;
                        //Convert it to Base64
                        strFrameArray[frameCount] = System.Convert.ToBase64String(memory.ToArray());
                        //Get ready for next
                        memory.Seek(0, System.IO.SeekOrigin.Begin);
                        currentStreamPos += frameDuration;

                    }
                    // Get rid of our poniter before we leave the unmanaged scope
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(ptrRefFrameBufSize);

                }
                //Clean up the memory              
                memory.Close();
                memory.Dispose();
                return strFrameArray;

            }
            catch (Exception exception)
            {
                //need some improvement here...
                throw new Exception(String.Format("Error! at getBase64FrameArray: {0}", exception.Message));
            }

        }
        /// <summary>This structure describes the bitmap and color information for a video image.</summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct VIDEOINFOHEADER
        {
            /// <summary>RECT structure that specifies the source video window.</summary>
            public RECT rcSource;
            /// <summary>RECT structure that specifies the destination video window.</summary>
            public RECT rcTarget;
            /// <summary>DWORD value that specifies the video stream's approximate data rate, in bits per second.</summary>
            public uint dwBitRate;
            /// <summary>DWORD value that specifies the video stream's data error rate, in bit errors per second.</summary>
            public uint dwBitErrorRate;
            /// <summary>REFERENCE_TIME value that specifies the video frame's average display time, in 100-nanosecond units.</summary>
            public ulong AvgTimePerFrame;
            /// <summary>Win32 BITMAPINFOHEADER structure that contains color and dimension information for the video image bitmap.</summary>
            public BITMAPINFOHEADER bmiHeader;
        }
        /// <summary>This structure defines the coordinates of the upper-left and lower-right corners of a rectangle.</summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            /// <summary>Specifies the x-coordinate of the upper-left corner of the rectangle.</summary>
            public int left;
            /// <summary>Specifies the y-coordinate of the upper-left corner of the rectangle.</summary>
            public int top;
            /// <summary>Specifies the x-coordinate of the lower-right corner of the rectangle.</summary>
            public int right;
            /// <summary>Specifies the y-coordinate of the lower-right corner of the rectangle.</summary>
            public int bottom;
        }

        /// <summary>This structure contains information about the dimensions and color format of a device-independent bitmap (DIB).</summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct BITMAPINFOHEADER
        {
            /// <summary>Specifies the size of the structure, in bytes.</summary>
            public uint biSize;
            /// <summary>Specifies the width of the bitmap, in pixels.</summary>
            public int biWidth;
            /// <summary>Specifies the height of the bitmap, in pixels.</summary>
            public int biHeight;
            /// <summary>Specifies the number of planes for the target device.</summary>
            public short biPlanes;
            /// <summary>Specifies the number of bits-per-pixel.</summary>
            public short biBitCount;
            /// <summary>Specifies the type of compression for a compressed bottom-up bitmap.</summary>
            public uint biCompression;
            /// <summary>Specifies the size, in bytes, of the image.</summary>
            public uint biSizeImage;
            /// <summary>Specifies the horizontal resolution, in pixels-per-meter, of the target device for the bitmap.</summary>
            public int biXPelsPerMeter;
            /// <summary>Specifies the vertical resolution, in pixels-per-meter, of the target device for the bitmap.</summary>
            public int biYPelsPerMeter;
            /// <summary>Specifies the number of color indexes in the color table that are actually used by the bitmap.</summary>
            public uint biClrUsed;
            /// <summary>Specifies the number of color indexes required for displaying the bitmap.</summary>
            public uint biClrImportant;
        }



    }
}
