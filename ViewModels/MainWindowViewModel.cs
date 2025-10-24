namespace radiosw.ViewModels {
    using System;
    using Avalonia.Threading;
    using ReactiveUI;

    public class MainWindowViewModel : ViewModelBase {
        private readonly        Random          _rnd = new Random();
        private                 Int32           _clock_now_1;
        private                 Int32           _clock_now_2;
        private                 String          _clock_now_3;
        private                 Boolean?        _is_clock;
        private                 String          _clock_text_1;
        private                 String          _clock_text_2;
        private                 DateTime        _tod;
        private                 ViewModelBase   _menu;
        private                 ViewModelBase   _offstate;
        private static readonly Char[]          R_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private readonly        DispatcherTimer _generator_timer;
        private readonly        DispatcherTimer _clock_timer;

        public MainWindowViewModel() {
            _generator_timer = new DispatcherTimer(TimeSpan.FromMinutes(1) , DispatcherPriority.Normal , xctGenerate);
            xctGenerate(null , null);
            _generator_timer.Start();
            _clock_timer =  new DispatcherTimer(TimeSpan.FromSeconds(30) , DispatcherPriority.Normal , xctApply);
            xctApply(null , null);
            _clock_timer.Start();
        }

        public ViewModelBase _off_state_    { get => _offstate;     private set => this.RaiseAndSetIfChanged(ref _offstate ,     value); }
        public ViewModelBase _menu_         { get => _menu;         set => this.RaiseAndSetIfChanged(ref _menu ,                 value); }
        public String        _clock_text_1_ { get => _clock_text_1; private set => this.RaiseAndSetIfChanged(ref _clock_text_1 , value); }
        public String        _clock_text_2_ { get => _clock_text_2; private set => this.RaiseAndSetIfChanged(ref _clock_text_2 , value); }
        public void          xctOffIt()        => _off_state_ = new OffViewModel();
        public void          xctOnIt()         => _off_state_ = new OnViewModel();

        private void xctApply(Object? arg_sender , EventArgs arg_event_args) {
            switch (_is_clock) {
                case null : {
                    _clock_text_1_ = _clock_now_1.ToString("00");
                    _clock_text_2_ = $"{_clock_now_2:00} {_clock_now_3}";
                    break;
                }
                case true :
                    TimeSpan timeleft = _tod - DateTime.Now;
                    _clock_text_1_ = timeleft.Hours.ToString("00");
                    _clock_text_2_ = $"{timeleft.Minutes:00} {timeleft.Seconds:00}";
                    break;
                case false :
                    break;
            }
        }

        private void xctGenerate(Object? arg_sender , EventArgs arg_e) {
            _clock_now_1 = _rnd.Next(0 , 99);
            _clock_now_2 = _rnd.Next(0 , 99);
            _clock_now_3 = $"{MainWindowViewModel.R_CHARS[_rnd.Next(0 , 26)]}{MainWindowViewModel.R_CHARS[_rnd.Next(0 , 26)]}";
        }

        public void xctSetTimer(DateTime arg_time) {
            _is_clock = true;
            _tod =  arg_time;
            _clock_timer.Interval =  TimeSpan.FromMilliseconds(500);
        }

        public void xctSetText(String arg_text) {
            _clock_timer.Interval =  TimeSpan.FromSeconds(30);
            _is_clock  = false;
            String[] clocktexts = arg_text.Split('|');
            _clock_text_1_ = clocktexts[0];
            _clock_text_2_ = $"{clocktexts[1]} {clocktexts[2]}";
        }

        public void xctBackToClock() {
            _clock_timer.Interval = TimeSpan.FromSeconds(30);
            _is_clock             = null;
            xctApply(null, null);
        }
    }
}