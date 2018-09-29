using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Random;
using Discord;
using System.Drawing;
using System.IO;
using System.Linq;

namespace FindAWordBot.Modules
{
    public class FindAWordModule : ModuleBase<SocketCommandContext>
    {
        private static Grid grid;
        private static List<string> foundWords;
        private static Dictionary<Discord.WebSocket.SocketUser, int> userFinds;

        [Command("Start")]
        public async Task StartFindAWord(bool fillLetters, params string[] words)
        {
            grid = new Grid(8, 8);
            foundWords = new List<string>();
            userFinds = new Dictionary<Discord.WebSocket.SocketUser, int>();

            grid.SubmitText(words);

            //grid.SubmitText("Banana");
            //grid.SubmitText("Robot");
            //grid.SubmitText("Woof");

            grid.GenerateGrid(3, fillLetters);

            var data = grid.GridData;

            using (var stream = new MemoryStream())
            {
                CreateImage(grid, stream, foundWords);

                stream.Position = 0;

                await Context.Channel.SendFileAsync(stream, "find_a_word.jpeg");
            }
                //ReplyAsync(string.Empty, false, )
        }

        //[Command()]
        //public async Task CheckWord(string word)
        //{

        //}

        [Command("Find")]
        public async Task FindWord(string word)
        {
            var found = grid.CheckWord(word);

            if (found)
            {
                foundWords.Add(word);

                if (userFinds.ContainsKey(Context.User))
                    userFinds[Context.User]++;
                else
                    userFinds.Add(Context.User, 1);
            }

            using (var stream = new MemoryStream())
            {---
                CreateImage(grid, stream, foundWords);

                stream.Position = 0;

                await Context.Channel.SendFileAsync(stream, "find_a_word.jpeg");

                var message = string.Join('\n', userFinds.Select(u => u.Key.Username + " word count: " + u.Value));

                await Context.Channel.SendMessageAsync(message);
            }
        }


        private void CreateImage(Grid grid, Stream memStream, List<string> words)
        {
            var foundPoints = GetFoundWords(grid, words);

            using (var bmp = new Bitmap(grid.Width * 70, grid.Height * 70))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(System.Drawing.Color.White);
                    Pen pen = new Pen(System.Drawing.Color.Black);
                    pen.Width = 4;

                    for (int i = 0; i <= grid.Width; i++)
                    {
                        g.DrawLine(pen, (i * 70), 0, i * 70, 70 * grid.Height);
                    }

                    for (int i = 0; i <= grid.Height; i++)
                    {
                        g.DrawLine(pen, 0, (i * 70), 70 * grid.Width, i * 70);
                    }

                    for (int i = 0; i < grid.Height; i++)
                    {
                        for (int q = 0; q < grid.Width; q++)
                        {
                            var value = grid.GridData[i][q].Value;

                            if (string.IsNullOrEmpty(value))
                                continue;

                            StringFormat stringFormat = new StringFormat();
                            stringFormat.Alignment = StringAlignment.Center;
                            stringFormat.LineAlignment = StringAlignment.Center;

                            g.DrawString(value, new Font(FontFamily.GenericSansSerif, 25), Brushes.Black, new Point((70 * q) + 35, (70 * i) + 35), stringFormat);
                        }
                    }

                    Pen penLine = new Pen(System.Drawing.Color.FromArgb(110, System.Drawing.Color.Red));
                    penLine.Width = 40;
                    penLine.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    penLine.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                    foreach (var foundWord in foundPoints)
                    {
                        g.DrawLines(penLine, foundWord.Select(p => new Point((p.X * 70) + 35, (p.Y * 70) + 35)).ToArray());
                    }
                }

                //bmp.Save(memStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                bmp.Save(memStream, System.Drawing.Imaging.ImageFormat.Jpeg);

                //var fileStream = File.Create("C:\\Users\\Joshua\\Documents\\Projects\\Bots\\FindAWordBot\\FindAWordBot\\FindAWordBot\\TestFile.jpg");
                //memStream.Seek(0, SeekOrigin.Begin);
                //memStream.CopyTo(fileStream);
                //fileStream.Close();
            }
        }

        private List<List<Point>> GetFoundWords(Grid grid, List<string> wordsToFind)
        {
            //if (wordsToFind == null || wordsToFind.Count == 0)
                //return null;

            List<List<Point>> returnPoints = new List<List<Point>>();

            foreach (var word in wordsToFind)
                returnPoints.Add(grid.GetWordData(word));

            return returnPoints;
        }
    }
}
