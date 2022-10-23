﻿using Newtonsoft.Json.Linq;
using NexteLite.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NexteLite
{
    /// <summary>
    /// Логика взаимодействия для ServerCarousel.xaml
    /// </summary>
    public partial class ServerCarousel : UserControl
    {
        private int[] CarouselItemRender = { 0, 1};

        private int SelectedItem = 0;
        private ItemServer Selected;

        public static readonly DependencyProperty ItemsCarouselProperty = DependencyProperty.Register("ItemsCarousel", typeof(IEnumerable), typeof(ServerCarousel), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnItemsCollectionChanged)) { AffectsRender = true });
        public IEnumerable ItemsCarousel
        {
            get { return (IEnumerable)GetValue(ItemsCarouselProperty); }
            set { SetValue(ItemsCarouselProperty, value); }
        }

        public static void OnItemsCollectionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ServerCarousel;
            if (control != null)
                control.OnItemsSourceChanged(control, (IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        }

        private void OnItemsSourceChanged(object sender, IEnumerable oldValue, IEnumerable newValue)
        {
            var control = sender as ServerCarousel;
            if (control != null)
            {
                var oldValueINotifyCollectionChanged = oldValue as INotifyCollectionChanged;

                if (null != oldValueINotifyCollectionChanged)
                {
                    oldValueINotifyCollectionChanged.CollectionChanged -= new NotifyCollectionChangedEventHandler(control.Items_CollectionChanged);
                }
                var newValueINotifyCollectionChanged = newValue as INotifyCollectionChanged;
                if (null != newValueINotifyCollectionChanged)
                {
                    newValueINotifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(control.Items_CollectionChanged);
                }
            }
        }
        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddItems(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    last_margin = 0;
                    Servers.Clear();
                    break;
            }
        }

        ObservableCollection<ItemServer> _Servers = new ObservableCollection<ItemServer>();
        public ObservableCollection<ItemServer> Servers => _Servers;

        public ServerCarousel()
        {
            InitializeComponent();
            UpdateButtonRender();
        }

        private int last_margin = 0;

        public void AddItems(IList servers)
        {
            foreach(var item in servers)
            {
                if(item is ServerProfile profile)
                {
                    var itemServer = new ItemServer();
                    var index = servers.IndexOf(item);

                    itemServer.Hide();
                    itemServer.IsHide = true;
                    itemServer.Unselect();

                    Panel.SetZIndex(itemServer, 1);

                    var new_margin = last_margin;
                    itemServer.RenderTransform = new TranslateTransform(new_margin, 0);
                    last_margin = new_margin + 69 + 94;

                    itemServer.Initialize(profile);

                    Servers.Add(itemServer);

                    itemServer.Visibility = Visibility.Collapsed;

                    Container.Children.Add(itemServer);
                }
                else
                {
                    throw new ArgumentException("Incorrect ItemSource type, requires ServerProfile type");
                }

            }

            for (var i = 0; i < Servers.Count; i++)
            {
                var item = Servers[i];

                if (i == 0)
                {
                    Panel.SetZIndex(item, 2);
                    item.Select();
                    item.IsSelect = true;
                    //item.RenderTransform = new TranslateTransform(0, 0);
                    Selected = item;
                }
                else
                {
                    Panel.SetZIndex(item, 1);
                    item.Unselect();
                    item.IsSelect = true;
                }
            }

            SelectChange();
        }
        private void UpdateButtonRender()
        {

            if (Servers.Count > 1)
            {
                if (SelectedItem == 0)
                {
                    left_button.Visibility = Visibility.Collapsed;
                    right_button.Visibility = Visibility.Visible;
                }
                else if (SelectedItem == Servers.Count - 1)
                {
                    right_button.Visibility = Visibility.Collapsed;
                    left_button.Visibility = Visibility.Visible;
                }
                else
                {
                    left_button.Visibility = Visibility.Visible;
                    right_button.Visibility = Visibility.Visible;
                }
            }
            else if (Servers.Count > 0)
            {
                left_button.Visibility = Visibility.Collapsed;
                right_button.Visibility = Visibility.Collapsed;
            }


        }

        private void UpdateRender(int countservers)
        {
            if (SelectedItem == 0)
            {
                CarouselItemRender = new int[] { SelectedItem, SelectedItem + 1 };
            }
            else if (SelectedItem == countservers - 1)
            {
                CarouselItemRender = new int[] { SelectedItem - 1, SelectedItem };
            }
            else
            {
                CarouselItemRender = new int[] { SelectedItem - 1, SelectedItem, SelectedItem + 1 };
            }

            UpdateButtonRender();
        }

        private void MoveLeft()
        {
            Storyboard sb = this.FindResource("Move_Left") as Storyboard;
            var doubleanim = (DoubleAnimation)sb.Children[0];
            doubleanim.To = -((TranslateTransform)Selected.RenderTransform).X;
            sb.Begin();
        }
        private void MoveRight()
        {
            Storyboard sb = this.FindResource("Move_Right") as Storyboard;
            var doubleanim = (DoubleAnimation)sb.Children[0];
            doubleanim.To = -((TranslateTransform)Selected.RenderTransform).X;
            sb.Begin();
        }

        private void SelectChange()
        {
            var servers = Container.Children;

            UpdateRender(servers.Count);

            foreach (var item in servers)
            {
                var elem = (ItemServer)item;
                var index = servers.IndexOf((ItemServer)item);

                if (CarouselItemRender.Contains(index))
                {
                    elem.Show();
                    elem.IsHide = false;
                }
                else if (!CarouselItemRender.Contains(index))
                {
                    elem.Hide();
                    elem.IsHide = true;
                }

                if (index == SelectedItem - 1)
                {
                    Panel.SetZIndex(elem, 1);
                    elem.Unselect();
                    elem.IsSelect = false;
                }

                if (index == SelectedItem)
                {
                    Panel.SetZIndex(elem, 2);
                    elem.Select();
                    elem.IsSelect = true;
                    Selected = elem;
                }

                if (index == SelectedItem + 1)
                {
                    Panel.SetZIndex(elem, 1);
                    elem.Unselect();
                    elem.IsSelect = false;         
                }

            }
        }

        private void Carousel_ChageItem(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            if (button.Tag.ToString() == "Left")
            {
                if (SelectedItem != 0)
                {
                    SelectedItem--;
                    SelectChange();
                    MoveRight();
                }
            }

            if (button.Tag.ToString() == "Right")
            {
                if (SelectedItem < Servers.Count-1)
                {
                    SelectedItem++;
                    SelectChange();
                    MoveLeft();
                }
            }
        }

        private DateTime last_time;
        private void Carousel_Scroll(object sender, MouseWheelEventArgs e)
        {
            var delta = Math.Sign(e.Delta);

            if(DateTime.Now < last_time.AddMilliseconds(100))
            {
                return;
            }

            if (delta > 0)
            {
                if (SelectedItem != 0)
                {
                    SelectedItem--;
                    SelectChange();
                    MoveRight();
                }
            }
            else if(delta < 0)
            {
                if (SelectedItem < Servers.Count - 1)
                {
                    SelectedItem++;
                    SelectChange();
                    MoveLeft();
                }
            }
            last_time = DateTime.Now;
        }
    }
}
