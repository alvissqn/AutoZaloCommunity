using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Tesseract;
using Emgu.CV.CvEnum;
using ZaloImageProcessing207.Structures;
using System.Text.RegularExpressions;
using System.IO;
using ZaloCommunityDev.Models;
using System.Text;

namespace ZaloImageProcessing207
{
    public class ImageProcessing : IZaloImageProcessing
    {

        public static void Main(string[] arrgs)
        {
            var imageProcessing = new ImageProcessing();

            imageProcessing.GetFriendProfileList(@"C:\Users\ngan\Desktop\Screenshot_20161030-181646.png", new ScreenInfo());
        }

        public FriendPositionMessage[] GetListFriendName(string fileImage, ScreenInfo screen)
        {
            var rects = DetectTemplate(fileImage, new string[] { $@".\ImageData\{screen.Name}\template\male_sign_pattern_template.png", $@".\ImageData\{screen.Name}\template\female_sign_pattern_template.png" });

            var bitmap = new Bitmap(fileImage);
            List<FriendPositionMessage> names = new List<FriendPositionMessage>();

            for (int i = 1; i <= rects.Length; i++)
            {
                var rect = rects[i - 1];
                using (var bitmap2 = bitmap.Clone(new Rectangle(rect.Left, rect.Top - 70, screen.WorkingRect.Right - rect.Left - 20, 70), bitmap.PixelFormat))
                {
                    var username = GetVietnameseText(bitmap2).Split("\r\n".ToArray()).FirstOrDefault();
                    names.Add(new FriendPositionMessage { Name = Regex.Replace(username, "/[!@#$%^&*.?}{,`~]/g", ""), Point = new ScreenPoint(rect.X, rect.Y) });
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
                var requestAddfriendPoint = DetectTemplate(fileImage, $@".\ImageData\{screenSize.Name}\template\add_friend_pattern_template.png").FirstOrDefault();

                var bitmap = new Bitmap(fileImage);

                int rowHeight = screenSize.ProfileInfoRowHeight;

                int start_x = infoRect.Right;
                int size_width = screenSize.WorkingRect.Right - start_x - 20;

                var profile = new ProfileMessage() { IsAddedToFriend = requestAddfriendPoint == null };

                var textProfile = GetVietnameseText(bitmap.Clone(new Rectangle(start_x, infoRect.Y, size_width, rowHeight * 4), bitmap.PixelFormat));
                var infoTexts = textProfile.Split("\r\n".ToArray()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

                profile.Name = infoTexts.ElementAtOrDefault(0).Trim();
                profile.BirthdayText = infoTexts.ElementAtOrDefault(1).Trim();
                profile.Gender = infoTexts.ElementAtOrDefault(2).Trim();
                profile.PhoneNumber = infoTexts.ElementAtOrDefault(3).Trim();

                return profile;
            }
            catch (Exception ex)
            {

                //parse profile error

                return new ProfileMessage();
            }
        }

        TesseractEngine tesseractEngine;
        TesseractEngine TesseractEngine => tesseractEngine ?? (tesseractEngine = new TesseractEngine(@"C:\Users\ngan\documents\visual studio 2015\Projects\ZaloCommunityDev\ZaloImageProcessing207\tessdata", "vie", EngineMode.Default));

        private ScreenPoint[] DetectFriendProfileLocation(string fileName, ScreenInfo screen)
        {
            Image<Bgr, Byte> img = new Image<Bgr, byte>(fileName);

            UMat uimage = new UMat();


            CvInvoke.CvtColor(img, uimage, ColorConversion.Bgr2Gray);


            double cannyThreshold = 30;
            double circleAccumulatorThreshold = 120;
            int ListGroupValuePosition = 0;
            using (var imgtemplate = new Image<Bgr, byte>($@".\ImageData\{screen.Name}\template\more_friend_pattern_template.png"))
            {
                var points = DetectTemplate(img, imgtemplate);
                if (points.Any())
                {
                    ListGroupValuePosition = points.First().Bottom;
                }
            }

            CircleF[] circles = CvInvoke.HoughCircles(uimage, HoughType.Gradient, 2.0, 20.0, cannyThreshold, circleAccumulatorThreshold, screen.FriendTabCircleRadiusAvartarUserFrom, screen.FriendTabCircleRadiusAvartarUserTo);
            var circles2 = circles.Where(x => x.Center.Y > ListGroupValuePosition).OrderBy(x => x.Center.Y).ToArray();

            return circles2.Select(x => new ScreenPoint((int)x.Center.X, (int)x.Center.Y)).ToArray();
        }

        public FriendPositionMessage[] GetFriendProfileList(string fileName, ScreenInfo screen)
        {
            var points = DetectFriendProfileLocation(fileName, screen);
            var FriendPositionMessageList = new List<FriendPositionMessage>();
            using (var bitmapData = new Bitmap(fileName)) {
                foreach (var point in points)
                {
                    int startX = point.X + screen.FriendTabCircleRadiusAvartarUserTo + 3;
                    int startY = point.Y - screen.FriendRowHeight / 2;
                    using (var subBitmap = bitmapData.Clone(new Rectangle(startX, startY, screen.WorkingRect.Right - startX, screen.FriendRowHeight), bitmapData.PixelFormat))
                    {
                        var text = GetVietnameseText(subBitmap);
                        FriendPositionMessageList.Add(new FriendPositionMessage { Name = text.Trim(), Point = point });
                    }
                }
            }

            return FriendPositionMessageList.ToArray();
        }

        private string GetVietnameseText(Bitmap imgsource)
        {
            imgsource.Save($"d:\\{DateTime.Now.Ticks}.bmp");

            var ocrtext = string.Empty;
            using (var engine = new TesseractEngine(@"C:\Users\ngan\documents\visual studio 2015\Projects\ZaloCommunityDev\ZaloImageProcessing207\Tesseract-OCR\tessdata", "vie", EngineMode.TesseractOnly))
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


        public Point[] GetLikedPoints(string sourceFile)
        {
            return DetectCenterPoints(sourceFile, @".\ImageData\template\liked_paterrn_template.png");
        }

        public Point[] GetNolikedPoints(string sourceFile)
        {
            return DetectCenterPoints(sourceFile, @".\ImageData\template\nolike_pattern_template.png");
        }



        private Rectangle[] DetectTemplate(string sourceFile, string templateFile, double matchPercent = 0.98d)
        {

            Image<Bgr, byte> template = new Image<Bgr, byte>(templateFile);

            var centerPoints = new List<Rectangle>();
            int tried = 0;

            using (var imgSrc = new Image<Bgr, byte>(sourceFile))
            {
                while (tried++ < 100)
                {
                    using (var result = imgSrc.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
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

            template.Dispose();

            return centerPoints.ToArray();
        }

        private Rectangle[] DetectTemplate(Image<Bgr, byte> sourceFile, Image<Bgr, byte> template, double matchPercent = 0.98d)
        {

            var centerPoints = new List<Rectangle>();
            int tried = 0;

            using (var imgSrc = sourceFile.Copy())
            {
                using (var templateCopy = template.Copy())
                {
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
                        int tried = 0;
                        while (tried++ < 100)
                        {
                            using (var result = imgSrc.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
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
            }

            return centerPoints.ToArray();
        }

        private Point[] DetectCenterPoints(string sourceFile, string templateFile)
        {
            Image<Bgr, byte> source = new Image<Bgr, byte>(sourceFile);
            Image<Bgr, byte> template = new Image<Bgr, byte>(templateFile);

            var centerPoints = new List<Point>();
            int tried = 0;

            using (Image<Bgr, byte> imgSrc = source)
            {
                while (tried++ < 100)
                {
                    using (Image<Gray, float> result = imgSrc.MatchTemplate(template, TemplateMatchingType.CcoeffNormed))
                    {
                        double[] minValues, maxValues;
                        Point[] minLocations, maxLocations;
                        result.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

                        if (maxValues[0] > 0.98)
                        {
                            Rectangle match = new Rectangle(maxLocations[0], template.Size);
                            centerPoints.Add(new Point(match.X + match.Width / 2, match.Y + match.Height / 2));

                            //CompareHist(template, imgSrc.Copy(match));

                            imgSrc.Draw(match, new Bgr(Color.White), -1);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            template.Dispose();

            return centerPoints.ToArray();
        }

        private bool IsEqual(Image<Bgr, byte> img1, Image<Bgr, byte> img2)
        {
            var img3 = img1.AbsDiff(img2);

            for (int width = 0; width < img3.Width; width++)
            {
                for (int height = 0; height < img3.Height; height++)
                {
                    for (int value = 0; value < 3; value++)
                    {
                        if (img3.Data[width, height, value] != 0)
                            return false;
                    }
                }
            }


            return true;
        }
    }
}
