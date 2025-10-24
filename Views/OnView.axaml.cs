namespace radiosw.Views {
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Controls.ApplicationLifetimes;
    using NAudio.Wave;

    public partial class OnView : UserControl {
        public OnView() {
            InitializeComponent();
            Load();
        }

        [ DllImport("PowrProf.dll" , CharSet = CharSet.Auto , ExactSpelling = true) ]
        public static extern Boolean SetSuspendState(Boolean arg_hiberate
                                                   , Boolean arg_force_critical
                                                   , Boolean arg_disable_wake_event);

        public async void Load() {
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                await Task.Run(slp);
                const String    filep             = "owin31.wav";
                AudioFileReader the_media_filep   = new AudioFileReader(filep);
                WasapiOut       the_media_playerp = new WasapiOut();
                the_media_playerp.Init(the_media_filep);
                the_media_playerp.Play();
                await Task.Run(slpm);
                ( desktop.MainWindow as MainWindow ).tGrid.IsVisible = true;
                await Task.Run(slpm);
                ( desktop.MainWindow as MainWindow ).Off.IsVisible = false;
            }
        }

        public void slpm() => Thread.Sleep(1000);
        public void slp()  => Thread.Sleep(10000);
    }
}