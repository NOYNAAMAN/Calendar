using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Calender.UserControls
{
    public partial class Item : UserControl
    {
        public event EventHandler<int> DeleteItem;
        public event EventHandler<int> CheckItem;
        public event EventHandler<int> MuteItem;
        public event EventHandler<int> EditItem;

        public Item()
        {
            InitializeComponent();
        }
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(Item));


        public string Time
        {
            get { return (string)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(string), typeof(Item));


        public SolidColorBrush Color
        {
            get { return (SolidColorBrush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(SolidColorBrush), typeof(Item));


        public FontAwesome.WPF.FontAwesomeIcon Icon
        {
            get { return (FontAwesome.WPF.FontAwesomeIcon)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(FontAwesome.WPF.FontAwesomeIcon), typeof(Item));


        public FontAwesome.WPF.FontAwesomeIcon IconBell
        {
            get { return (FontAwesome.WPF.FontAwesomeIcon)GetValue(IconBellProperty); }
            set { SetValue(IconBellProperty, value); }
        }

        public static readonly DependencyProperty IconBellProperty = DependencyProperty.Register("IconBell", typeof(FontAwesome.WPF.FontAwesomeIcon), typeof(Item));

        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteItem?.Invoke(this, ItemId);
        }

        private void CheckItem_Click(object sender, RoutedEventArgs e)
        {
            CheckItem?.Invoke(this, ItemId);
        }
        private void MuteItem_Click(object sender, RoutedEventArgs e)
        {
            MuteItem?.Invoke(this, ItemId);
        }
        private void EditItem_Click(object sender, RoutedEventArgs e)
        {
            EditItem?.Invoke(this, ItemId);
        }
        public int ItemId
        {
            get { return (int)GetValue(ItemIdProperty); }
            set { SetValue(ItemIdProperty, value); }
        }

        public static readonly DependencyProperty ItemIdProperty = DependencyProperty.Register("ItemId", typeof(int), typeof(Item));
    }
}