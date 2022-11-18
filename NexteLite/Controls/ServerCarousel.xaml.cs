using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using NexteLite.Interfaces;
using NexteLite.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace NexteLite.Controls
{
    /// <summary>
    /// Логика взаимодействия для ServerCarousel.xaml
    /// </summary>
    public partial class ServerCarousel : UserControl
    {
        private int[] CarouselItemRender = { 0, 1 };

        private int SelectedItem = 0;
        private ItemServer Selected;

        public delegate void OnPlayClickHandler(string nID);
        public event OnPlayClickHandler PlayClick;

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
                var newCollection = newValue as IList;
                if (null != newCollection)
                {
                    control.Reset();
                    control.AddItems(newCollection);
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

        IMineStat _State;

        public ServerCarousel()
        {
            InitializeComponent();
            Loaded += ServerCarousel_Loaded;
            UpdateButtonRender();

            _State = ((App)App.Current).ServiceProvider.GetRequiredService<IMineStat>();
        }

        public void Reset()
        {
            _Servers = new ObservableCollection<ItemServer>();
            Container.Children.Clear();

            last_margin = 0;
            Selected = null;
            SelectedItem = 0;     
            TranslateTransform resetTransform = new TranslateTransform();
            resetTransform.X = 0;
            Container.RenderTransform = resetTransform;

            UpdateRender(0);
        }

        private void ServerCarousel_Loaded(object sender, RoutedEventArgs e)
        {
            var name = "Show";
            Storyboard sb = this.FindResource(name) as Storyboard;
            sb.Begin(this);
        }

        private int last_margin = 0;

        public void AddItems(IList servers)
        {
            foreach (var item in servers)
            {
                if (item is ServerProfile profile)
                {
                    var itemServer = new ItemServer(_State);
                    itemServer.OnPlayClick += ItemServer_OnPlayClick;

                    var index = servers.IndexOf(item);

                    itemServer.Visibility = Visibility.Collapsed;

                    if (index < 2)
                    {
                        itemServer.Show();
                    }
                    else
                    {
                        itemServer.Hide(true);
                    }

                    itemServer.Initialize(profile);
                    Servers.Add(itemServer);
                }
                else
                {
                    throw new ArgumentException("Incorrect ItemSource type, requires ServerProfile type");
                }

            }

            for (var i = 0; i < Servers.Count; i++)
            {
                var item = Servers[i];

                Container.Children.Add(item);

                if (i == 0)
                {
                    Panel.SetZIndex(item, 2);

                    item.Select(true);

                    item.RenderTransform = new TranslateTransform(0, 0);
                    Selected = item;
                }
                else
                {
                    Panel.SetZIndex(item, 1);

                    item.Unselect(true);

                    var new_margin = last_margin + 69 + 94;
                    item.RenderTransform = new TranslateTransform(new_margin, 0);
                    last_margin = new_margin;
                }

                
            }

            UpdateButtonRender();
        }

        private void ItemServer_OnPlayClick(string nID)
        {
            PlayClick?.Invoke(nID);
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
            UpdateRender(Servers.Count);

            foreach (var item in Servers)
            {
                var index = Servers.IndexOf(item);

                if (CarouselItemRender.Contains(index))
                {
                    item.Show();
                }
                else if (!CarouselItemRender.Contains(index))
                {
                    item.Hide();
                }

                if (index == SelectedItem - 1)
                {
                    Panel.SetZIndex(item, 1);
                    item.Unselect();
                }

                if (index == SelectedItem)
                {
                    Panel.SetZIndex(item, 2);
                    item.Select();
                    Selected = item;
                }

                if (index == SelectedItem + 1)
                {
                    Panel.SetZIndex(item, 1);
                    item.Unselect();
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
                if (SelectedItem < Servers.Count - 1)
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

            if (DateTime.Now < last_time.AddMilliseconds(100))
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
            else if (delta < 0)
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
