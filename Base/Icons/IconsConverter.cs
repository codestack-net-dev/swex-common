﻿//**********************
//SwEx - development tools for SOLIDWORKS
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/swex-common/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex
//**********************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeStack.SwEx.Common.Icons
{
    public class IconsConverter : IDisposable
    {
        private class IconData
        {
            internal Image SourceIcon { get; set; }
            internal Size TargetSize { get; set; }
            internal string TargetIconPath { get; private set; }

            internal IconData(string iconsDir, Image sourceIcon, Size targetSize, string name)
            {
                SourceIcon = sourceIcon;
                TargetSize = targetSize;
                TargetIconPath = Path.Combine(iconsDir, name);
            }
        }

        private readonly string m_IconsDir;
        private readonly Color m_TransparencyKey;
        private readonly bool m_DisposeIcons;

        public IconsConverter(Color transparencyKey)
            : this(transparencyKey,
                  Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), true)
        {
        }

        public IconsConverter(Color transparencyKey, string iconsDir,
            bool disposeIcons = true)
        {
            m_IconsDir = iconsDir;
            m_TransparencyKey = transparencyKey;
            m_DisposeIcons = disposeIcons;

            if (!Directory.Exists(m_IconsDir))
            {
                Directory.CreateDirectory(m_IconsDir);
            }
        }
        
        public string[] ConvertIconsGroup(IIcon[] icons, bool highRes)
        {
            IconData[,] iconsDataGroup = null;

            for (int i = 0; i < icons.Length; i++)
            {
                var data = CreateIconData(icons[i], highRes);

                if (iconsDataGroup == null)
                {
                    iconsDataGroup = new IconData[data.Length, icons.Length];
                }

                for (int j = 0; j < data.Length; j++)
                {
                    iconsDataGroup[j, i] = data[j];
                }
            }

            var iconsPaths = new string[iconsDataGroup.GetLength(0)];

            for (int i = 0; i < iconsDataGroup.GetLength(0); i++)
            {
                var imgs = new Image[iconsDataGroup.GetLength(1)];
                for (int j = 0; j < iconsDataGroup.GetLength(1); j++)
                {
                    imgs[j] = iconsDataGroup[i, j].SourceIcon;
                }

                iconsPaths[i] = iconsDataGroup[i, 0].TargetIconPath;
                CreateBitmap(imgs, iconsPaths[i],
                    iconsDataGroup[i, 0].TargetSize, m_TransparencyKey);
            }

            return iconsPaths;
        }

        public string[] ConvertIcon(IIcon icon, bool highRes)
        {
            var iconsData = CreateIconData(icon, highRes);

            foreach (var iconData in iconsData)
            {
                CreateBitmap(new Image[] { iconData.SourceIcon },
                    iconData.TargetIconPath,
                    iconData.TargetSize, m_TransparencyKey);
            }

            return iconsData.Select(i => i.TargetIconPath).ToArray();
        }

        private IconData[] CreateIconData(IIcon icon, bool highRes)
        {
            if (icon == null)
            {
                throw new ArgumentException("Icons are not specified");
            }
            
            IEnumerable<IconSizeInfo> sizes = null;

            if (highRes)
            {
                sizes = icon.GetHighResolutionIconSizes();
            }
            else
            {
                sizes = icon.GetIconSizes();
            }

            if (sizes == null || !sizes.Any())
            {
                throw new NullReferenceException($"Specified icon '{icon.GetType().FullName}' doesn't provide any sizes");
            }

            var iconsData = sizes.Select(s => new IconData(m_IconsDir, s.SourceImage, s.TargetSize, s.Name)).ToArray();

            //if (icon is HighResIcon)
            //{
            //    var highResIcons = icon as HighResIcon;

            //    if (highRes)
            //    {
            //        iconsData = new IconData[]
            //        {
            //            new IconData(m_IconsDir, highResIcons.Size20x20, new Size(20,20)),
            //            new IconData(m_IconsDir, highResIcons.Size32x32, new Size(32,32)),
            //            new IconData(m_IconsDir, highResIcons.Size40x40, new Size(40,40)),
            //            new IconData(m_IconsDir, highResIcons.Size64x64, new Size(64,64)),
            //            new IconData(m_IconsDir, highResIcons.Size96x96, new Size(96,96)),
            //            new IconData(m_IconsDir, highResIcons.Size128x128, new Size(128,128))
            //        };
            //    }
            //    else
            //    {
            //        iconsData = new IconData[]
            //        {
            //            new IconData(m_IconsDir, highResIcons.Size20x20, new Size(16,16)),
            //            new IconData(m_IconsDir, highResIcons.Size32x32, new Size(24,24)),
            //        };
            //    }
            //}
            //else if (icon is BasicIcon)
            //{
            //    var basicIcons = icon as BasicIcon;

            //    if (highRes)
            //    {
            //        iconsData = new IconData[]
            //        {
            //            new IconData(m_IconsDir, basicIcons.Size16x16, new Size(20,20)),
            //            new IconData(m_IconsDir, basicIcons.Size24x24, new Size(32,32)),
            //            new IconData(m_IconsDir, basicIcons.Size24x24, new Size(40,40)),
            //            new IconData(m_IconsDir, basicIcons.Size24x24, new Size(64,64)),
            //            new IconData(m_IconsDir, basicIcons.Size24x24, new Size(96,96)),
            //            new IconData(m_IconsDir, basicIcons.Size24x24, new Size(128,128))
            //        };
            //    }
            //    else
            //    {
            //        iconsData = new IconData[]
            //        {
            //            new IconData(m_IconsDir, basicIcons.Size16x16, new Size(16,16)),
            //            new IconData(m_IconsDir, basicIcons.Size24x24, new Size(24,24)),
            //        };
            //    }
            //}
            //else if (icon is MasterIcon)
            //{
            //    var masterIcons = icon as MasterIcon;

            //    if (highRes)
            //    {
            //        iconsData = new IconData[]
            //        {
            //            new IconData(m_IconsDir, masterIcons.Icon, new Size(20,20)),
            //            new IconData(m_IconsDir, masterIcons.Icon, new Size(32,32)),
            //            new IconData(m_IconsDir, masterIcons.Icon, new Size(40,40)),
            //            new IconData(m_IconsDir, masterIcons.Icon, new Size(64,64)),
            //            new IconData(m_IconsDir, masterIcons.Icon, new Size(96,96)),
            //            new IconData(m_IconsDir, masterIcons.Icon, new Size(128,128))
            //        };
            //    }
            //    else
            //    {
            //        iconsData = new IconData[]
            //        {
            //            new IconData(m_IconsDir, masterIcons.Icon, new Size(16,16)),
            //            new IconData(m_IconsDir, masterIcons.Icon, new Size(24,24)),
            //        };
            //    }
            //}
            //else
            //{
            //    throw new NotSupportedException($"Specified icon '{icon.GetType().FullName}' is not supported");
            //}

            return iconsData;
        }

        private void CreateBitmap(Image[] sourceIcons,
            string targetIcon, Size size, Color background)
        {
            var width = size.Width * sourceIcons.Length;
            var height = size.Height;

            using (var bmp = new Bitmap(width,
                                    height, PixelFormat.Format24bppRgb))
            {
                using (var graph = Graphics.FromImage(bmp))
                {
                    graph.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graph.SmoothingMode = SmoothingMode.HighQuality;
                    graph.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    using (var brush = new SolidBrush(background))
                    {
                        graph.FillRectangle(brush, 0, 0, bmp.Width, bmp.Height);
                    }

                    for (int i = 0; i < sourceIcons.Length; i++)
                    {
                        var sourceIcon = sourceIcons[i];

                        if (bmp.HorizontalResolution != sourceIcon.HorizontalResolution
                            || bmp.VerticalResolution != sourceIcon.VerticalResolution)
                        {
                            bmp.SetResolution(
                                sourceIcon.HorizontalResolution,
                                sourceIcon.VerticalResolution);
                        }

                        var widthScale = (double)size.Width / (double)sourceIcon.Width;
                        var heightScale = (double)size.Height / (double)sourceIcon.Height;
                        var scale = Math.Min(widthScale, heightScale);

                        int destX = 0;
                        int destY = 0;

                        if (heightScale < widthScale)
                        {
                            destX = (int)(size.Width - sourceIcon.Width * scale) / 2;
                        }
                        else
                        {
                            destY = (int)(size.Height - sourceIcon.Height * scale) / 2;
                        }

                        int destWidth = (int)(sourceIcon.Width * scale);
                        int destHeight = (int)(sourceIcon.Height * scale);

                        destX += i * size.Width;

                        graph.DrawImage(sourceIcon,
                            new Rectangle(destX, destY, destWidth, destHeight),
                            new Rectangle(0, 0, sourceIcon.Width, sourceIcon.Height),
                            GraphicsUnit.Pixel);
                    }
                }

                var dir = Path.GetDirectoryName(targetIcon);

                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                bmp.Save(targetIcon);
            }
        }

        public void Dispose()
        {
            if (m_DisposeIcons)
            {
                try
                {
                    Directory.Delete(m_IconsDir, true);
                }
                catch
                {
                }
            }
        }
    }
}
