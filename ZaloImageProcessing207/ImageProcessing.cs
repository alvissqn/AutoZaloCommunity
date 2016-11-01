using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Tesseract;
using ZaloCommunityDev.Shared;
using ZaloCommunityDev.Shared.Structures;

namespace ZaloCommunityDev.ImageProcessing
{
    public class ZaloImageProcessing : IZaloImageProcessing
    {
        const string TesseracOcrDataFolder = @"C:\Users\ngan\documents\visual studio 2015\Projects\ZaloCommunityDev\ZaloImageProcessing207\Tesseract-OCR\tessdata";

        public static void Main(string[] arrgs)
        {
            var imageProcessing = new ZaloImageProcessing();

            imageProcessing.GetFriendProfileList(@"C:\Users\ngan\Desktop\Screenshot_20161030-181646.png", new ScreenInfo());
        }

        public FriendPositionMessage[] GetListFriendName(string fileImage, ScreenInfo screen)
        {
            var rects = DetectTemplate(fileImage, new[] { $@".\ImageData\{screen.Name}\template\male_sign_pattern_template.png", $@".\ImageData\{screen.Name}\template\female_sign_pattern_template.png" });

            var bitmap = new Bitmap(fileImage);
            var names = new List<FriendPositionMessage>();

            for (var i = 1; i <= rects.Length; i++)
            {
                var rect = rects[i - 1];
                using (var bitmap2 = bitmap.Clone(new Rectangle(rect.Left, rect.Top - 70, screen.WorkingRect.Right - rect.Left - 20, 70), bitmap.PixelFormat))
                {
                    var username = GetVietnameseText(bitmap2).Split("\r\n".ToArray()).FirstOrDefault();
                    names.Add(new FriendPositionMessage { Name = Regex.Replace(username ?? string.Empty, "/[!@#$%^&*.?}{,`~]/g", ""), Point = new ScreenPoint(rect.X, rect.Y) });
                }
            }

            return names.ToArray();
        }

        public bool IsShowDialogWaitAddedFriendConfirmWhenRequestAdd(string fileImage, ScreenInfo info)
        {
            var infoRect = DetectTemplate(fileImage, $@".\ImageData\{info.Name}\template\dialog_wait_added_friend_confirm_pattern_template.png");

            return infoRect.Any();
        }

        public ProfileMessage GetProfile(string fileImage, ScreenInfo screenSize)
        {
            try
            {
                var infoRect = DetectTemplate(fileImage, $@".\ImageData\{screenSize.Name}\template\profile_info_pattern_template.png").First();
                var iconAddFriendPositions = DetectTemplate(fileImage, $@".\ImageData\{screenSize.Name}\template\add_friend_pattern_template.png");

                var bitmap = new Bitmap(fileImage);

                var rowHeight = screenSize.ProfileInfoRowHeight;

                var startX = infoRect.Right;
                var sizeWidth = screenSize.WorkingRect.Right - startX - 20;

                var profile = new ProfileMessage() { IsAddedToFriend = !iconAddFriendPositions.Any() };

                var textProfile = GetVietnameseText(bitmap.Clone(new Rectangle(startX, infoRect.Y, sizeWidth, rowHeight * 4), bitmap.PixelFormat));
                var infoTexts = textProfile.Split("\r\n".ToArray()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                profile.Name = infoTexts.ElementAtOrDefault(0)?.Trim();
                profile.BirthdayText = infoTexts.ElementAtOrDefault(1)?.Trim();
                profile.Gender = infoTexts.ElementAtOrDefault(2)?.Trim();
                profile.PhoneNumber = infoTexts.ElementAtOrDefault(3)?.Trim();

                return profile;
            }
            catch (Exception ex)
            {
                return new ProfileMessage();
            }
        }

        public FriendPositionMessage[] GetFriendProfileList(string fileName, ScreenInfo screen)
        {
            var points = DetectFriendProfileLocation(fileName, screen);
            var friendPositionMessageList = new List<FriendPositionMessage>();
            using (var bitmapData = new Bitmap(fileName))
            {
                foreach (var point in points)
                {
                    var startX = point.X + screen.FriendTabCircleRadiusAvartarUserTo + 3;
                    var startY = point.Y - screen.FriendRowHeight / 2;

                    using (var subBitmap = bitmapData.Clone(new Rectangle(startX, startY, screen.WorkingRect.Right - startX, screen.FriendRowHeight), bitmapData.PixelFormat))
                    {
                        var text = GetVietnameseText(subBitmap);
                        friendPositionMessageList.Add(new FriendPositionMessage { Name = text.Trim(), Point = point });
                    }
                }
            }

            return friendPositionMessageList.ToArray();
        }

        private ScreenPoint[] DetectFriendProfileLocation(string fileName, ScreenInfo screen)
        {
            var img = new Image<Bgr, byte>(fileName);

            var uimage = new UMat();

            CvInvoke.CvtColor(img, uimage, ColorConversion.Bgr2Gray);

            const double cannyThreshold = 30;
            const double circleAccumulatorThreshold = 120;

            var iconGroupTop = 0;
            using (var imgtemplate = new Image<Bgr, byte>($@".\ImageData\{screen.Name}\template\more_friend_pattern_template.png"))
            {
                var points = DetectTemplate(img, imgtemplate);
                if (points.Any())
                {
                    iconGroupTop = points.First().Bottom;
                }
            }

            var circles = CvInvoke.HoughCircles(uimage, HoughType.Gradient, 2.0, 20.0, cannyThreshold, circleAccumulatorThreshold, screen.FriendTabCircleRadiusAvartarUserFrom, screen.FriendTabCircleRadiusAvartarUserTo);
            var circles2 = circles.Where(x => x.Center.Y > iconGroupTop).OrderBy(x => x.Center.Y).ToArray();

            return circles2.Select(x => new ScreenPoint((int)x.Center.X, (int)x.Center.Y)).ToArray();
        }

        public Point[] GetLikedPoints(string sourceFile) => DetectCenterPoints(sourceFile, @".\ImageData\template\liked_paterrn_template.png");

        public Point[] GetNolikedPoints(string sourceFile) => DetectCenterPoints(sourceFile, @".\ImageData\template\nolike_pattern_template.png");

        #region Functions

        private string GetVietnameseText(Bitmap imgsource)
        {
            //imgsource.Save($"d:\\{DateTime.Now.Ticks}.bmp");
            string ocrtext;
            using (var engine = new TesseractEngine(TesseracOcrDataFolder, "vie", EngineMode.TesseractOnly))
            {
                using (var img = PixConverter.ToPix(imgsource))
                {
                    using (var page = engine.Process(img))
                    {
                        ocrtext = page.GetText();
                    }
                }
            }

            return ocrtext;
        }

        private Rectangle[] DetectTemplate(string sourceFile, string templateFile, double matchPercent = 0.98d)
        {
            using (var source = new Image<Bgr, byte>(sourceFile))
            {
                using (var template = new Image<Bgr, byte>(templateFile))
                {
                    return DetectTemplate(source, template, matchPercent);
                }
            }
        }

        private Rectangle[] DetectTemplate(Image<Bgr, byte> sourceFile, Image<Bgr, byte> template, double matchPercent = 0.98d)
        {
            var centerPoints = new List<Rectangle>();

            using (var imgSrc = sourceFile.Copy())
            {
                using (var templateCopy = template.Copy())
                {
                    var tried = 0;
                    while (tried++ < 100)
                    {
                        using (var result = imgSrc.MatchTemplate(templateCopy, TemplateMatchingType.CcoeffNormed))
                        {
                            double[] minValues, maxValues;
                            Point[] minLocations, maxLocations;
                            result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                            if (maxValues[0] > matchPercent)
                            {
                                var match = new Rectangle(maxLocations[0], template.Size);
                                centerPoints.Add(match);
                                imgSrc.Draw(match, new Bgr(Color.White), -1);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }

            template.Dispose();

            return centerPoints.ToArray();
        }

        private Rectangle[] DetectTemplate(string sourceFile, string[] templateFiles, double matchPercent = 0.98d)
        {
            var centerPoints = new List<Rectangle>();

            using (var imgSrc = new Image<Bgr, byte>(sourceFile))
            {
                foreach (var templateFile in templateFiles)
                {
                    using (var template = new Image<Bgr, byte>(templateFile))
                    {
                        centerPoints.AddRange(DetectTemplate(imgSrc, template, matchPercent));
                    }
                }
            }

            return centerPoints.ToArray();
        }

        private bool ContainsTemplate(string sourceFile, string templateFile, double matchPercent = 0.98d)
        {
            return DetectTemplate(sourceFile, templateFile, matchPercent).Any();
        }

        private Point[] DetectCenterPoints(string sourceFile, string templateFile, double matchPercent = 0.98d)
        {
            using (var source = new Image<Bgr, byte>(sourceFile))
            using (var template = new Image<Bgr, byte>(templateFile))
            {
                return DetectTemplate(source, template, matchPercent)?.Select(x => new Point(x.X + x.Width / 2, x.Y + x.Height / 2)).ToArray() ?? new Point[0];
            }
        }

        public bool HasFindButton(string fileName, ScreenInfo screen)
        {
            return ContainsTemplate(fileName, $@".\ImageData\{screen.Name}\template\more_friend_pattern_template.png");
        }

        #endregion Functions
    }
}