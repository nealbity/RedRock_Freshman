﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Windows.UI.Core;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media.Animation;
using Windows.System;
using Windows.UI.Text;
using Windows.UI.Xaml.Media.Imaging;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace RedRock_Freshman.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FengCaiPage : Page
    {
        private int pivot_index;
        private int zuzhi_listview_index;
        private double[] pivotitem1_ver_offest;

        private ViewModel.FengCaiViewModel viewmodel;
        public static FengCaiPage fengcaipage;

        public FengCaiPage()
        {
            this.InitializeComponent();
            pivot_index = 0;
            zuzhi_listview_index = 0;
            viewmodel = new ViewModel.FengCaiViewModel();
            this.DataContext = viewmodel;
            fengcaipage = this;
            this.SizeChanged += FengCaiPage_SizeChanged;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                await First_Step();
                pivotitem1_ver_offest = new double[viewmodel.ZuZhi.Count];
                Dispatcher?.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PivotItem1_Add_Content(1);
                });
                await Task.Delay(100);
                zuzhi_listview.SelectedIndex = pivot.SelectedIndex = 0;
            }
        }

        private void FengCaiPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            viewmodel.Page_Width = e.NewSize.Width;
            viewmodel.Page_Height = e.NewSize.Height;
        }

        private async Task First_Step()
        {
            StorageFile file;
            string json = "";
            JObject json_object;

            #region 得到Header列表
            file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Json/fengcai_header_lists.json", UriKind.Absolute));
            json = await FileIO.ReadTextAsync(file);
            json_object = (JObject)JsonConvert.DeserializeObject(json);
            JArray headers = (JArray)json_object["header_lists"];
            ObservableCollection<Model.fengcaiheaders> header_lists = new ObservableCollection<Model.fengcaiheaders>();
            for (int i = 0; i < headers.Count; i++)
            {
                Model.fengcaiheaders item = new Model.fengcaiheaders();
                item.header = headers[i]["header"].ToString();
                header_lists.Add(item);
            }
            viewmodel.Header = header_lists;
            #endregion

            #region 得到组织列表
            file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Json/fengcai_zuzhi_lists.json", UriKind.Absolute));
            json = await FileIO.ReadTextAsync(file);
            json_object = (JObject)JsonConvert.DeserializeObject(json);
            JArray zuzhis = (JArray)json_object["zuzhi_lists"];
            ObservableCollection<Model.zuzhi> zuzhi_lists = new ObservableCollection<Model.zuzhi>();
            for (int i = 0; i < zuzhis.Count; i++)
            {
                Model.zuzhi item = new Model.zuzhi();
                item.zuzhi_name = zuzhis[i]["zuzhi"].ToString();
                zuzhi_lists.Add(item);
            }
            viewmodel.ZuZhi = zuzhi_lists;
            #endregion

            #region 得到组织介绍
            file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Json/fengcai_zuzhi_intro.json", UriKind.Absolute));
            json = await FileIO.ReadTextAsync(file);
            json_object = (JObject)JsonConvert.DeserializeObject(json);
            JArray zuzhi_intros = (JArray)json_object["zuzhi_intro"];
            ObservableCollection<Model.zuzhi_intro> intro_lists = new ObservableCollection<Model.zuzhi_intro>();
            for (int i = 0; i < zuzhi_intros.Count; i++)
            {
                Model.zuzhi_intro item = new Model.zuzhi_intro();
                item.zuzhi = new ObservableCollection<string>();
                JArray zuzhi_item = (JArray)zuzhi_intros[i]["zuzhi"];
                for (int j = 0; j < zuzhi_item.Count; j++)
                {
                    item.zuzhi.Add(zuzhi_item[j]["duanluo"].ToString());
                }
                intro_lists.Add(item);
            }
            viewmodel.Zuzhi_Intro = intro_lists;
            #endregion

            #region 得到原创重邮内容
            json = await HttpRequest.Request.YuanChuang_Request();
            if (json != null)
            {
                json_object = (JObject)JsonConvert.DeserializeObject(json);
                JArray data = (JArray)json_object["data"];
                ObservableCollection<Model.yuanchuang> yc_lists = new ObservableCollection<Model.yuanchuang>();
                for (int i = 0; i < data.Count; i++)
                {
                    Model.yuanchuang item = new Model.yuanchuang();
                    item.introduction = data[i]["introduction"].ToString();
                    item.name = data[i]["name"].ToString();
                    item.time = data[i]["time"].ToString();
                    item.video_url = data[i]["video_url"].ToString();
                    JArray photo = (JArray)data[i]["photo"];
                    for (int j = 0; j < photo.Count; j++)
                    {
                        item.photo_src = photo[j]["photo_src"].ToString();
                    }
                    yc_lists.Add(item);
                }
                viewmodel.YuanChuang = yc_lists;
            }
            #endregion

            #region 得到最美重邮文字内容
            file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Json/fengcai_zuimei.json", UriKind.Absolute));
            json = await FileIO.ReadTextAsync(file);
            json_object = (JObject)JsonConvert.DeserializeObject(json);
            JArray zuimei = (JArray)json_object["zuimei"];
            ObservableCollection<string> zuimei_lists = new ObservableCollection<string>();
            for (int i = 0; i < zuimei.Count; i++)
            {
                string content = zuimei[i]["content"].ToString();
                zuimei_lists.Add(content);
            }
            viewmodel.ZuiMei = zuimei_lists;
            #endregion

            #region 得到最美重邮图片
            json = await HttpRequest.Request.ZuiMei_Request();
            if (json != null)
            {
                json_object = (JObject)JsonConvert.DeserializeObject(json);
                JArray data = (JArray)json_object["data"];
                zuimei_lists = new ObservableCollection<string>();
                for (int i = 0; i < data.Count; i++)
                {
                    JArray photo = (JArray)data[i]["photo"];
                    zuimei_lists.Add(photo[0]["photo_src"].ToString());
                }
                viewmodel.ZuiMei_Photos = zuimei_lists;
            }
            #endregion

            #region 得到优秀学子内容
            json = await HttpRequest.Request.XueZi_Request();
            if (json != null)
            {
                json_object = (JObject)JsonConvert.DeserializeObject(json);
                JArray data = (JArray)json_object["data"];
                ObservableCollection<Model.xuezi> xuezi_lists = new ObservableCollection<Model.xuezi>();
                for (int i = 0; i < data.Count; i++)
                {
                    Model.xuezi item = new Model.xuezi();
                    item.description = data[i]["description"].ToString();
                    item.introduction = data[i]["introduction"].ToString();
                    item.name = data[i]["name"].ToString();
                    JArray photo = (JArray)data[i]["photo"];
                    item.photo_src = photo[0]["photo_src"].ToString();
                    item.photo_thumbnail_src = photo[0]["photo_thumbnail_src"].ToString();
                    xuezi_lists.Add(item);
                }
                viewmodel.XueZi = xuezi_lists;
            }
            #endregion

            #region 得到优秀教师内容
            json = await HttpRequest.Request.Teather_Request();
            if (json != null)
            {
                json_object = (JObject)JsonConvert.DeserializeObject(json);
                JArray data = (JArray)json_object["data"];
                ObservableCollection<Model.teacher> teacher_lists = new ObservableCollection<Model.teacher>();
                for (int i = 0; i < data.Count; i++)
                {
                    Model.teacher item = new Model.teacher();
                    item.name = data[i]["name"].ToString();
                    item.college = data[i]["college"].ToString();
                    JArray photo = (JArray)data[i]["photo"];
                    item.photo_src = photo[0]["photo_src"].ToString();
                    item.photo_thumbnail_src = photo[0]["photo_thumbnail_src"].ToString();
                    teacher_lists.Add(item);
                }
                viewmodel.Teacher = teacher_lists;
            }
            #endregion
        }

        private void PivotItem1_Add_Content(int p)
        {
            zuzhi_content.Children.Clear();
            if (p == 1)
            {
                for (int i = 0; i < viewmodel.Zuzhi_Intro[0].zuzhi.Count; i++)
                {
                    if (viewmodel.Zuzhi_Intro[0].zuzhi[i].Contains("【"))
                    {
                        zuzhi_content.Children.Add(New_TextBlock(1, viewmodel.Zuzhi_Intro[0].zuzhi[i]));
                    }
                    else
                    {
                        zuzhi_content.Children.Add(New_TextBlock(2, viewmodel.Zuzhi_Intro[0].zuzhi[i]));
                    }
                }
            }
            else if (p == 2)
            {
                for (int i = 0; i < viewmodel.Zuzhi_Intro[zuzhi_listview.SelectedIndex].zuzhi.Count; i++)
                {
                    if (viewmodel.Zuzhi_Intro[zuzhi_listview.SelectedIndex].zuzhi[i].Contains("【"))
                    {
                        zuzhi_content.Children.Add(New_TextBlock(1, viewmodel.Zuzhi_Intro[zuzhi_listview.SelectedIndex].zuzhi[i]));
                    }
                    else
                    {
                        zuzhi_content.Children.Add(New_TextBlock(2, viewmodel.Zuzhi_Intro[zuzhi_listview.SelectedIndex].zuzhi[i]));
                    }
                }
            }
        }

        private TextBlock New_TextBlock(int p, string content)
        {
            TextBlock tb = new TextBlock();
            switch (p)
            {
                case 1: //较重标题
                    {
                        tb.Text = content.Substring(1, (content.LastIndexOf('】') - content.IndexOf('【') - 1));
                        tb.Foreground = App.APPTheme.Content_Header_Color_Brush;
                        tb.FontSize = 16;
                        tb.Margin = new Thickness(0, 3, 0, 8);
                    }; break;
                case 2: //普通内容
                    {
                        tb.Text = content;
                        FontWeight weight = new FontWeight();
                        weight.Weight = 10;
                        tb.FontWeight = weight;
                        tb.Foreground = App.APPTheme.Gary_Color_Brush;
                        tb.FontSize = 15;
                        tb.LineHeight = 26;
                    }; break;
            }
            tb.CharacterSpacing = 100;
            tb.TextWrapping = TextWrapping.Wrap;
            return tb;
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (pivot.SelectedIndex < 0)
                {
                    pivot.SelectedIndex = pivot_index = 0;
                }
                (((pivot.Items[pivot_index] as PivotItem).Header as Grid).Children[0] as TextBlock).Foreground = App.APPTheme.Content_Header_Color_Brush;
                (((pivot.Items[pivot_index] as PivotItem).Header as Grid).Children[1] as Line).Visibility = Visibility.Collapsed;
                pivot_index = pivot.SelectedIndex;
                (((pivot.Items[pivot_index] as PivotItem).Header as Grid).Children[0] as TextBlock).Foreground = App.APPTheme.APP_Color_Brush;
                (((pivot.Items[pivot_index] as PivotItem).Header as Grid).Children[1] as Line).Visibility = Visibility.Visible;
            }
            catch (Exception)
            {
                return;
            }
        }

        private void zuzhi_listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pivotitem1_ver_offest[zuzhi_listview_index] = zuzhi_sc.VerticalOffset;
            PivotItem1_Add_Content(2);
            if (pivotitem1_ver_offest[zuzhi_listview.SelectedIndex] != 0.0)
            {
                zuzhi_sc.ChangeView(null, pivotitem1_ver_offest[zuzhi_listview.SelectedIndex], null, true);
            }
            else
            {
                zuzhi_sc.ChangeView(null, 0.0, null, true);
            }
            zuzhi_listview_index = zuzhi_listview.SelectedIndex;
        }

        private async void yc_listview_ItemClick(object sender, ItemClickEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri((e.ClickedItem as Model.yuanchuang).video_url));
        }

        private void XueZi_Rectangle_Loaded(object sender, RoutedEventArgs e)
        {
            Binding binding1 = new Binding();
            binding1.Source = viewmodel;
            binding1.Path = new PropertyPath("XueZi_Height");
            (sender as Rectangle).SetBinding(Rectangle.HeightProperty, binding1);
            Binding binding2 = new Binding();
            binding2.Source = viewmodel;
            binding2.Path = new PropertyPath("XueZi_Width");
            (sender as Rectangle).SetBinding(Rectangle.WidthProperty, binding2);
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Model.xuezi)
            {
                detail_img.ImageSource = new BitmapImage(new Uri((e.ClickedItem as Model.xuezi).photo_thumbnail_src, UriKind.Absolute));
                detail_title.Text = (e.ClickedItem as Model.xuezi).name;
                detail_content.Text = (e.ClickedItem as Model.xuezi).introduction;
            }
            else if (e.ClickedItem is Model.teacher)
            {
                detail_img.ImageSource = new BitmapImage(new Uri((e.ClickedItem as Model.teacher).photo_thumbnail_src, UriKind.Absolute));
                detail_title.Text = (e.ClickedItem as Model.teacher).name;
                detail_content.Text = (e.ClickedItem as Model.teacher).college;
            }
            detail_sc.ChangeView(null, 0, null, true);
            black_background.Visibility = Visibility.Visible;
            black_background_sb.Begin();
            detail_popup.IsOpen = true;
        }

        private void detail_popup_Closed(object sender, object e)
        {
            detail_img.ImageSource = null;
            detail_title.Text = "";
            detail_content.Text = "";
            black_background.Visibility = Visibility.Collapsed;
        }

        private void back_but_Click(object sender, RoutedEventArgs e)
        {
            FirstPage.firstpage.Second_Page_Back();
        }
    }
}
