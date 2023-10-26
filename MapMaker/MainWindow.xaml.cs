using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text;
using System.IO;

namespace MapMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int gridSize = 16;
        private const int maxSize = 24;
        private bool updateBoard = false;

        private static readonly Dictionary<int, Tuple<Color, string>> options = new Dictionary<int, Tuple<Color, string>>
        {
            {0, new Tuple<Color, string>(Colors.White, "Nothing") },
            {1, new Tuple<Color, string>(Colors.Blue, "WALL 1") },
            {2, new Tuple<Color, string>(Colors.Red, "WALL 2") },
            {3, new Tuple<Color, string>(Colors.Yellow, "DOOR") }
        };

        private const int minimum = 0;
        private const int maximum = 3;

        public MainWindow()
        {
            InitializeComponent();
            Size.Text = gridSize.ToString();
            // do grid stuff
            Loaded += delegate
            {
                Load_Grid();
            };
        }

        private void Load_Grid()
        {
            Board.Children.Clear();
            Board.RowDefinitions.Clear();
            Board.ColumnDefinitions.Clear();

            Board.UpdateLayout();
            double width = Board.ActualWidth / gridSize;
            double height = Board.ActualHeight / gridSize;

            for (int i = 0; i < gridSize; i++)
            {
                RowDefinition rowDefinition = new RowDefinition();
                rowDefinition.Height = new GridLength(height);
                Board.RowDefinitions.Add(rowDefinition);

                for (int j = 0; j < gridSize; j++)
                {
                    ColumnDefinition columnDefinition = new ColumnDefinition();
                    columnDefinition.Width = new GridLength(width);
                    Board.ColumnDefinitions.Add(columnDefinition);

                    Button button = new Button
                    {
                        Content = "0",
                        Background = new SolidColorBrush(Colors.White),
                        Tag = $"{i}:{j}"
                    };
                    // Set the width and height of the button
                    button.Width = width;
                    button.Height = height;

                    button.Click += Grid_Button_Click;
                    Board.Children.Add(button);
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                }
            }
        }

        private void Grid_Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender!;
            Debug.Write($"Button: {button.Tag}");
            int.TryParse(button.Content.ToString(), out int value);
            if(++value > maximum) { value = minimum; }
            Tuple<Color, string> info = options[value];
            button.Content = value.ToString();
            button.Background = new SolidColorBrush(info.Item1);
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            Load_Grid(); // easiest way to reset grid
        }

        private void Key_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder bob = new StringBuilder("Key:\n");
            
            foreach(KeyValuePair<int, Tuple<Color, string>> option in options)
            {
                bob.AppendLine($"Key: {option.Key}, Value: {option.Value.Item2}");
            }
            popupText.Text = bob.ToString();
            if (!popup.IsOpen)
            {
                popup.IsOpen = true; // Open the popup.
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string path = $"map_{DateTime.Now.ToString("YYYYMMdd_HHmmss")}.txt";
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                writer.WriteLine($"{gridSize}:{gridSize}"); // size
                for(int row = 0; row < gridSize; row++)
                {
                    for(int col = 0; col < gridSize; col++)
                    {
                        Button button = Board.Children.Cast<Button>().First(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col);
                        int.TryParse(button.Content.ToString(), out int value);
                        writer.Write(value.ToString());
                        if (col != gridSize - 1) writer.Write(":");
                    }
                    writer.Write('\n');
                }
                writer.Close();
            }
            Process.Start("notepad.exe", path);
        }

        private void Size_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if(!int.TryParse(e.Text, out _)) e.Handled = true;
        }

        private void Size_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = (TextBox)sender!;

            if(int.TryParse(box.Text, out int value) && value <= maxSize)
            {
                gridSize = value;
                updateBoard = true;
                return;
            }
            if (value > maxSize) box.Text = gridSize.ToString();
        }

        private void Size_LostFocus(object sender, RoutedEventArgs e)
        {
            if (updateBoard)
            {
                Load_Grid();
                updateBoard = false;
            }
        }
    }
}
