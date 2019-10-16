using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CrazyRecite.Helpers;
using CrazyRecite.Models;
using Newtonsoft.Json;
using CrazyReciteApi.Models;
using CrazyReciteApi.Utils;

namespace CrazyRecite
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        string key = "lastOpenFile";
        bool isRepeat = false;
        bool isUpper = false;
        bool isRandom = false;
        object lockObj = new object();
        readonly List<WorldQueue> worlds = new List<WorldQueue>();
        readonly BookService bookServer = new BookService();
        int num = 0;

        public MainWindow()
        {
            InitializeComponent();
            GetBooksAsync();
            string lastfile = ConfigurationUtil.GetValue(key);
            OpenFile(lastfile);
            btn_Play.Visibility = Visibility.Visible;
        }

        private async void GetBooksAsync()
        {
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var booksList = bookServer.GetBooksList();
                    cmb_books.ItemsSource = booksList;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            });
            
        }

        private void btn_Play_Click(object sender, RoutedEventArgs e)
        {
            if (worlds.Count > 0)
            {
                btn_Play.Visibility = Visibility.Collapsed;
                btn_Pause.Visibility = Visibility.Visible;
                isRepeat = true;
                Speech();
            }
        }

        private void Speech()
        {
            if (worlds.Count > 0 && num < worlds.Count)
            {
                string RegexStr = @"[a-zA-Z]";
                Regex rep_regex = new Regex(RegexStr);
                string meaning = rep_regex.Replace(worlds[num].Name, "");
                string code = worlds[num].Code;
                ImageFun(worlds[num]);
                PromptBuilder prompt = new PromptBuilder();
                PromptStyle style = new PromptStyle();
                SpeechSynthesizer synthesizer = new SpeechSynthesizer();
                Task.Factory.StartNew(() =>
                {
                    int i = num;
                    while (i == num && isRepeat)
                    {
                        lock (lockObj)
                        {
                            style.Rate = PromptRate.NotSet;
                            style.Emphasis = PromptEmphasis.NotSet;
                            style.Volume = PromptVolume.ExtraLoud;
                            prompt.StartStyle(style);
                            prompt.AppendText(code);
                            prompt.EndStyle();
                            synthesizer.Speak(prompt);
                            prompt.ClearContent();
                        }
                    }

                    synthesizer.Dispose();
                });
            }
        }
        //private void 
        private void ImageFun(WorldQueue wq)
        {
            string showCode = wq.Code;
            if (String.IsNullOrWhiteSpace(showCode))
                showCode = "???";
            else if (isUpper)
                showCode = showCode.ToUpper();
            else
                showCode = showCode.ToLower();


            //this.img_Code.Source = GetImgEnus(showCode);
            //this.img_Name.Source = GetImgZhcn(wq.Name);

            this.tb_Code.Text = showCode;
            this.tb_Name.Text = wq.Name;
        }

        private ImageSource GetImgEnus(string value)
        {
            int k = 500;
            int size = 150;
            //this.stackPanel1.Background = new SolidColorBrush(Color.FromArgb(80, 230, 230, 230));
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            for (int i = 0; i < value.ToCharArray().Length; i++)
            {
                drawingContext.DrawText(
            new FormattedText((value.ToCharArray()[i]).ToString(), CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight, new Typeface("Microsoft YaHei UI"), size, Brushes.White),
                new Point(100 * i, 0));
            }

            drawingContext.Close();
            // 利用RenderTargetBitmap对象，以保存图片

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(k, 100, k / value.Length, 50, PixelFormats.Pbgra32);
            renderBitmap.Render(drawingVisual);

            ImageSource source1 = BitmapFrame.Create(renderBitmap);
            return source1;
        }

        private ImageSource GetImgZhcn(string value)
        {
            int k = 500;
            int size = 150;
            //this.stackPanel1.Background = new SolidColorBrush(Color.FromArgb(80, 230, 230, 230));
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            for (int i = 0; i < value.ToCharArray().Length; i++)
            {
                drawingContext.DrawText(
            new FormattedText((value.ToCharArray()[i]).ToString(), CultureInfo.GetCultureInfo("zh-cn"),
                FlowDirection.LeftToRight, new Typeface("Microsoft YaHei UI"), size, Brushes.White),
                new Point(150 * i, 0));
            }

            drawingContext.Close();
            // 利用RenderTargetBitmap对象，以保存图片

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(k, 100, k / value.Length, 50, PixelFormats.Pbgra32);
            renderBitmap.Render(drawingVisual);

            ImageSource source1 = BitmapFrame.Create(renderBitmap);
            return source1;
        }

        private void btn_Pause_Click(object sender, RoutedEventArgs e)
        {
            isRepeat = false;
            btn_Play.Visibility = Visibility.Visible;
            btn_Pause.Visibility = Visibility.Collapsed;
        }

        private void btn_Openfile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;//该值确定是否可以选择多个文件
            dialog.Title = "Please select a folder";
            dialog.Filter = "文本文件|*.txt|所有文件|*.*";
            if (dialog.ShowDialog() == true)
            {
                worlds.Clear();
                OpenFile(dialog.FileName);

            }
        }

        private void OpenFile(string path)
        {
            if (File.Exists(path))
            {
                StreamReader sr = new StreamReader(path, Encoding.Default);
                String line;
                Task.Factory.StartNew(() =>
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        string RegexStr = @"\[[A-Za-z]+\]";
                        Regex rep_regex = new Regex(RegexStr);
                        string _code = Regex.Match(line.ToString(), RegexStr).ToString().TrimStart('[').TrimEnd(']');
                        string _name = rep_regex.Replace(line.ToString(), "");
                        worlds.Add(new WorldQueue() { Code = _code, Name = _name });
                    }
                });
                ConfigurationUtil.SetValue(key, path);
            }
        }

        private void btn_Exit_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("Are you sure about closing?",
                                               "Exit",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Question,
                                                MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                //notifyIcon.Dispose();
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void btn_Last_Click(object sender, RoutedEventArgs e)
        {
            if (num > 0)
            {
                num--;
            }
            if (num < worlds.Count - 1)
            {
                isRepeat = true;
                Speech();
            }
        }

        private void btn_Next_Click(object sender, RoutedEventArgs e)
        {
            if (isRandom)
            {
                Random rd = new Random();
                num = rd.Next(worlds.Count);
            }
            else if (num < worlds.Count - 1)
            {
                num++;
            }
            if (num >= 0)
            {
                isRepeat = true;
                Speech();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    break;
                case Key.Space:
                    if (!isRepeat)
                        btn_Play_Click(null, null);
                    else
                        btn_Pause_Click(null, null);
                    break;
                case Key.Left:
                    btn_Last_Click(null, null);
                    break;
                case Key.Right:
                    btn_Next_Click(null, null);
                    break;
                case Key.Up:
                    isUpper = true;
                    ImageFun(worlds[num]);
                    break;
                case Key.Down:
                    isUpper = false;
                    ImageFun(worlds[num]);
                    break;
            }
        }


        private void tbtn_Random_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton tbtn = sender as ToggleButton;
            if (tbtn.IsChecked == true)
            {
                btn_Last.IsEnabled = false;
                isRandom = true;
            }
            else
            {
                btn_Last.IsEnabled = true;
                isRandom = false;
            }
        }

        private async void btn_Download_OnClickAsync(object sender, RoutedEventArgs e)
        {
            tb_Msg.Text = "";
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;//设置为选择文件夹

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var book = cmb_books.SelectedItem as Books;

                if (book == null)
                {
                    return;
                }

                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        bookServer.DownloadBook(book.Path, dialog.FileName);
                        tb_Msg.Text = "下载完成";
                    }
                    catch (Exception ex)
                    {
                        tb_Msg.Text = ex.Message;
                    }
                });
            }
            else
            {
                return;
            }
        }

        private async void btn_Upload_OnClickAsync(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = true;//该值确定是否可以选择多个文件
            dialog.Title = "Please select a folder";
            dialog.Filter = "文本文件|*.txt|所有文件|*.*";
            if (dialog.ShowDialog() == true)
            {
                //dialog.FileName
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        tb_Msg.Text = bookServer.UploadFile(dialog.FileNames);
                    }
                    catch (Exception ex)
                    {
                        tb_Msg.Text = ex.Message;
                    }
                });
            }
        }
    }
    public partial class WorldQueue
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
