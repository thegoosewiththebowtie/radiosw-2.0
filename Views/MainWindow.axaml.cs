using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using radiosw.ViewModels;

namespace radiosw.Views {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            MainWindow.Init();
            MainDataManager.nvkUpdateUi += UpdateUi;
            TheMediaPlayer.nvkUpdateTime                     += UpdateTime;
            if (Paths.xctIsDebug()) {
                Dispatcher.UIThread.Post(() => {
                                             WindowState       = WindowState.Normal;
                                             Width             = 800;
                                             Height         = 600;
                                             SystemDecorations = SystemDecorations.Full;
                                         }
                                        );
            }
            MainWindow.LoadedEvent.AddClassHandler<TopLevel>(OnIt , handledEventsToo : true);
            KeyDownEvent.AddClassHandler<TopLevel>(MainWindow.Control , handledEventsToo : true);
        }

        private static void Init() {
            Console.WriteLine(typeof(Paths).ToString());
            Console.WriteLine(typeof(FloppyIo).ToString());
            Console.WriteLine(typeof(MainDataManager).ToString());
            Console.WriteLine(typeof(WebHandling).ToString());
            Console.WriteLine(typeof(ERROR).ToString());
        }

        private void UpdateTime(Object? arg_sender , PlayerTime arg_player_time) {
            Dispatcher.UIThread.InvokeAsync(() => {
                                                NowTimeBlock.Text
                                                    = $"{
                                                        ( Int32 ) arg_player_time.timenow_.TotalMinutes
                                                        :00}:{
                                                        arg_player_time.timenow_.Seconds
                                                        :00}";
                                                AllTimeBlock.Text
                                                    = $"{
                                                        ( Int32 ) arg_player_time.timefull_.TotalMinutes
                                                        :00}:{
                                                        arg_player_time.timefull_.Seconds
                                                        :00}";
                                                Bar.Value = arg_player_time.timepercent_;
                                            }
                                           );
        }

        private void UpdateUi(Object? arg_sender , EventArgs arg_event_args) {
            PlaybackInfo info = MainDataManager.xctGetPlaybackInfo();
            Dispatcher.UIThread.InvokeAsync(() => {
                                                Art.Source    = info.cover_  ?? Art.Source;
                                                MainText.Text = info.title_  ?? MainText.Text;
                                                status.Text   = info.status_ ?? status.Text;
                                                mode0.Text    = info.mode_   ?? mode0.Text;
                                                if (info.eplist_ != null) {
                                                    EpList.Items.Clear();
                                                    foreach (String ep in info.eplist_) {
                                                        EpList.Items.Add(new ListBoxItem() {
                                                                                               Padding
                                                                                                   = new Thickness(10
                                                                                                                 , 6
                                                                                                                 , 10
                                                                                                                 , 6
                                                                                                                  )
                                                                                             , Content = ep
                                                                                           }
                                                                        );
                                                    }
                                                    EpList.SelectedIndex = 5;
                                                }
                                                if (info.resetnums_) {
                                                    AllTimeBlock.Text = "00:00";
                                                    NowTimeBlock.Text = "00:00";
                                                }
                                                if (info.timedatetime_ != null) {
                                                    ( DataContext as MainWindowViewModel ).xctSetTimer(info.timedatetime_ ?? DateTime.Now);
                                                } else if (info.timestring_ != null) {
                                                    ( DataContext as MainWindowViewModel ).xctSetText(info.timestring_);
                                                } else {
                                                    ( DataContext as MainWindowViewModel ).xctBackToClock();
                                                }
                                                Bar.IsIndeterminate = info.isloading_ ?? Bar.IsIndeterminate;
                                            }
                                           );
        }

        private void OnIt(TopLevel arg_top_level , RoutedEventArgs arg_routed_event_args) {
            MainWindowViewModel? view_model = DataContext as MainWindowViewModel;
            view_model.xctOnIt();
            tGrid.IsVisible = false;
            Off.IsVisible   = true;
        }

        private static async void Control(TopLevel arg_top_level , KeyEventArgs arg_event_args) {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (arg_event_args.Key) {
                case Key.Enter :
                    Task.Run(MainDataManager.xctLoad);
                    break;
                case Key.Divide :   Task.Run(MainDataManager.xctPrev); break;
                case Key.Multiply : Task.Run(MainDataManager.xctNext); break;
                case Key.Subtract : Task.Run(() => MainDataManager.xctRew()); break;
                case Key.Add :      Task.Run(() => MainDataManager.xctFfd()); break;
                case Key.Decimal :  Task.Run(MainDataManager.xctPause); break;
                case Key.NumPad0 :  Task.Run(MainDataManager.xctUnPause); break;
                case Key.NumPad9 :  Task.Run(MainDataManager.xctOpenMenu); break;
                default :           break;
            }
        }
    }
}