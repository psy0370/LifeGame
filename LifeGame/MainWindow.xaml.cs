using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LifeGame
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int CellPixels = 6;

        private Task loopThread;
        private CancellationTokenSource tokenSource;
        private Models.Cells cells;
        volatile int generationFPS;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ウインドウを読み込んだときの処理を定義します。
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(HorizonCells.Text, out int h) || !int.TryParse(VerticalCells.Text, out int v))
            {
                return;
            }

            cells = new Models.Cells(h, v);
            generationFPS = 10;
            BuildCellView(h, v);
            MainLoopThread();
        }

        /// <summary>
        /// ボタンをクリックしたときの処理を定義します。
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (loopThread?.IsCompleted == true)
            {
                MainLoopThread();
            }
            else
            {
                tokenSource.Cancel();
            }
        }

        /// <summary>
        /// セルの表示領域でマウスホイールを操作したときの処理を定義します。
        /// </summary>
        private void CellView_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0 && generationFPS < 60)
            {
                generationFPS++;
            }
            else if (e.Delta < 0 && generationFPS > 5)
            {
                generationFPS--;
            }
        }

        /// <summary>
        /// セルの表示領域を生成します。
        /// </summary>
        /// <param name="h">セルの横の個数を設定します。</param>
        /// <param name="v">セルの縦の個数を設定します。</param>
        private void BuildCellView(int h, int v)
        {
            // 縦横いずれかが10未満の場合は例外をスロー
            if (h < 10 || v < 10)
            {
                throw new ArgumentException();
            }

            CellView.Width = h * CellPixels - 1;
            CellView.Height = v * CellPixels - 1;
            CellView.Children.Clear();

            for (var y = 0; y < v; y++)
            {
                for (var x = 0; x < h; x++)
                {
                    var rectangle = new Rectangle()
                    {
                        Width = CellPixels - 1,
                        Height = CellPixels - 1
                    };
                    rectangle.MouseDown += Rectangle_MouseDown;
                    Canvas.SetLeft(rectangle, x * CellPixels);
                    Canvas.SetTop(rectangle, y * CellPixels);
                    CellView.Children.Add(rectangle);
                }
            }

            return;
        }

        /// <summary>
        /// セルをクリックしたときの処理を定義します。
        /// </summary>
        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (loopThread?.IsCompleted == true)
            {
                var cell = e.Source as Rectangle;
                int index = CellView.Children.IndexOf(cell);
                cells.Conditions[index] = !cells.Conditions[index];
                cell.Fill = cells.Conditions[index] ? Brushes.Black : Brushes.White;
            }
        }

        /// <summary>
        /// メインループを定義します。
        /// </summary>
        private void MainLoopThread()
        {
            tokenSource = new CancellationTokenSource();
            loopThread = Task.Factory.StartNew(token =>
            {
                // キャンセルトークンが無効の間ループ
                while (!((CancellationToken)token).IsCancellationRequested)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // 生存セルを点灯
                        for (var i = 0; i < cells.Conditions.Length; i++)
                        {
                            var rectangle = (Rectangle)CellView.Children[i];
                            rectangle.Fill = cells.Conditions[i] ? Brushes.Black : Brushes.White;
                        }
                        cells.CalcNextGeneration();
                    }));

                    // FPS設定に応じてスリープ
                    Thread.Sleep(1000 / generationFPS);
                }
            }, tokenSource.Token).ContinueWith(t =>
            {
                EditButton.Dispatcher.Invoke(() =>
                {
                    EditButton.Content = "開始";
                });
            });

            EditButton.Content = "編集";
        }
    }
}
