namespace radiosw {
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class ThePodcastActions : iActions {
        public enum eActionType {
            JUMPTO
          , DOWNLOAD
          , UNDOWNLOAD
          , MARKFIN
          , UNMARKFIN
          , CANCELDULU
          , AUTOSTOP
          , CANCELSTOP
          , RESET
           ,
        }

        public  eDulu                   _dulu_state_        { get; private set; } = eDulu.NONE;
        public  Int32                   _dulu_progress_     { get; private set; } = 0;
        public  DateTime?               _tod_               { get; private set; } = null;
        public  Int32                   _dulu_sub_progress_ { get; private set; } = 0;
        private CancellationTokenSource _dulu_cancel  = new CancellationTokenSource();
        private CancellationTokenSource _timer_cancel = new CancellationTokenSource();

        public void Dispose() {
            _dulu_cancel.Cancel();
            _timer_cancel.Cancel();
            GC.SuppressFinalize(this);
        }

        private static void Track(Task<Boolean> arg_task)
            => arg_task.ContinueWith(arg_completed => {
                                         if (!arg_task.Result) { ERROR.RINVE(ERROR.eErrorType.BACKFAIL); }
                                         arg_completed.Dispose();
                                     }
                                   , TaskScheduler.Default
                                    );

        private void xctResetTimerCt() {
            _timer_cancel.Cancel();
            _timer_cancel.Dispose();
            _timer_cancel = new CancellationTokenSource();
        }

        private void xctResetDuluCts() {
            _dulu_cancel.Cancel();
            _dulu_cancel.Dispose();
            _dulu_cancel = new CancellationTokenSource();
        }

        public Boolean xctExecute(eActionType arg_type , params Int32[] arg_arguments) {
            switch (arg_type) {
                case eActionType.JUMPTO :
                    ThePodcastActions.Track(ThePodcastActions.ActJumpTo(arg_arguments[0] , arg_arguments[1]));
                    break;
                case eActionType.DOWNLOAD :
                    xctResetDuluCts();
                    ThePodcastActions.Track(ActDownload(arg_arguments[0] , arg_arguments[1] , _dulu_cancel.Token));
                    break;
                case eActionType.UNDOWNLOAD :
                    xctResetDuluCts();
                    ThePodcastActions.Track(ActUndownload(arg_arguments[0] , arg_arguments[1] , _dulu_cancel.Token));
                    break;
                case eActionType.MARKFIN :
                    xctResetDuluCts();
                    ThePodcastActions.Track(ActMarkFin(arg_arguments[0] , arg_arguments[1] , _dulu_cancel.Token));
                    break;
                case eActionType.UNMARKFIN :
                    xctResetDuluCts();
                    ThePodcastActions.Track(ActUnMarkFin(arg_arguments[0] , arg_arguments[1] , _dulu_cancel.Token));
                    break;
                case eActionType.CANCELDULU : _dulu_cancel.Cancel(); break;
                case eActionType.AUTOSTOP :
                    xctResetTimerCt();
                    ThePodcastActions.Track(ActAutoStop(arg_arguments[0] , _timer_cancel.Token));
                    break;
                case eActionType.CANCELSTOP : _timer_cancel.Cancel(); break;
                case eActionType.RESET :      ThePodcastActions.Track(ThePodcastActions.ActReset()); break;
                default :                     throw new ArgumentOutOfRangeException(nameof(arg_type) , arg_type , null);
            }
            return true;
        }

        private static async Task<Boolean> ActJumpTo(Int32 arg_jumpto , Int32 arg_last) {
            switch (arg_jumpto) {
                case -3 : MainDataManager.xctGetProcessing().xctJump(0); break;
                case -2 : MainDataManager.xctGetProcessing().xctJump(arg_last); break;
                case -1 :
                    Random rnd = new Random();
                    MainDataManager.xctGetProcessing().xctJump(rnd.Next(0 , arg_last));
                    break;
                default : MainDataManager.xctGetProcessing().xctJump(arg_jumpto); break;
            }
            return true;
        }

        private async Task<Boolean> ActDownload(Int32             arg_first
                                              , Int32             arg_last
                                              , CancellationToken arg_cancellation_token) {
            if (_dulu_state_ != eDulu.NONE) { return false; }
            _dulu_state_ = eDulu.DOWNLOAD;
            EventHandler<Int32> handler = (_ , arg_i) => { _dulu_sub_progress_ = arg_i; MainDataManager.xctUpdateUi();};
            WebHandling.eProgressReport += handler;
            try {
                Int32   localprogress;
                Podcast podcast    = MainDataManager.xctGetSource().xctGetSourceAsPodcast();
                String  local_path = Paths.xctGetDownloadPath(podcast._title_);
                for (Int32 epid = arg_first ; epid <= arg_last ; epid++) {
                    Episode? episode = podcast.GetEpisode(epid);
                    if (episode == null) { return false; }
                    String episode_path = Path.Combine(local_path , $"{epid:0000}.mp3");
                    if (episode._audio_local_source_ != null
                        && File.Exists(episode._audio_local_source_.Replace("file://" , ""))) { continue; }
                    if (!await WebHandling.DownloadMp3(episode._audio_web_source_
                                                     , episode_path
                                                     , arg_cancellation_token
                                                      )) { continue; }
                    podcast._episodes_[epid]._audio_local_source_ = $"file://{episode_path}";
                    _dulu_progress_ = arg_last == arg_first
                                          ? 100
                                          : ( Int32 ) ( ( epid       - ( Double ) arg_first )
                                                        / ( arg_last - ( Double ) arg_first )
                                                        * 100 );
                    MainDataManager.xctUpdateUi();
                }
            } catch (Exception e) {
                await ActUndownload(arg_first , arg_last , CancellationToken.None);
                return false;
            } finally {
                WebHandling.eProgressReport -= handler;
                _dulu_progress_             =  0;
                _dulu_sub_progress_         =  0;
                _dulu_state_                =  eDulu.NONE;
                MainDataManager.xctUpdateUi();
            }
            return true;
        }

        private async Task<Boolean> ActUndownload(Int32             arg_first
                                                , Int32             arg_last
                                                , CancellationToken arg_cancellation_token) {
            if (_dulu_state_ != eDulu.NONE) { return false; }
            _dulu_state_ = eDulu.UNDOWNLOAD;
            Podcast data = MainDataManager.xctGetSource().xctGetSourceAsPodcast();
            try {
                for (Int32 epid = arg_first ; epid <= arg_last ; epid++) {
                    _dulu_sub_progress_ = 0;
                    arg_cancellation_token.ThrowIfCancellationRequested();
                    String? todel = data._episodes_[epid]._audio_local_source_;
                    if (todel == null) { continue; }
                    File.Delete(todel);
                    data._episodes_[epid]._audio_local_source_ = null;
                    MainDataManager.xctUpdateUi();
                    _dulu_sub_progress_                        = 100;
                    _dulu_progress_ = arg_last == arg_first
                                          ? 100
                                          : ( Int32 ) ( ( epid       - ( Double ) arg_first )
                                                        / ( arg_last - ( Double ) arg_first )
                                                        * 100 );
                    MainDataManager.xctUpdateUi();
                }
            } catch (Exception e) { return false; } finally {
                _dulu_progress_     = 0;
                _dulu_sub_progress_ = 0;
                _dulu_state_        = eDulu.NONE;
                MainDataManager.xctUpdateUi();
            }
            return true;
        }

        private async Task<Boolean> ActMarkFin(Int32             arg_first
                                             , Int32             arg_last
                                             , CancellationToken arg_cancellation_token) {
            if (_dulu_state_ != eDulu.NONE) { return false; }
            _dulu_state_ = eDulu.MARKFIN;
            Podcast data = MainDataManager.xctGetSource().xctGetSourceAsPodcast();
            try {
                for (Int32 epid = arg_first ; epid <= arg_last ; epid++) {
                    arg_cancellation_token.ThrowIfCancellationRequested();
                    _dulu_sub_progress_              = 0;
                    data._episodes_[epid]._finished_ = true;
                    MainDataManager.xctUpdateUi();
                    _dulu_sub_progress_              = 100;
                    _dulu_progress_ = arg_last == arg_first
                                          ? 100
                                          : ( Int32 ) ( ( epid       - ( Double ) arg_first )
                                                        / ( arg_last - ( Double ) arg_first )
                                                        * 100 );
                    MainDataManager.xctUpdateUi();
                }
            } catch (Exception e) { return false; } finally {
                _dulu_progress_     = 0;
                _dulu_sub_progress_ = 0;
                _dulu_state_        = eDulu.NONE;
            }
            return true;
        }

        private async Task<Boolean> ActUnMarkFin(Int32             arg_first
                                               , Int32             arg_last
                                               , CancellationToken arg_cancellation_token) {
            if (_dulu_state_ != eDulu.NONE) { return false; }
            _dulu_state_ = eDulu.UNMARKFIN;
            Podcast data = MainDataManager.xctGetSource().xctGetSourceAsPodcast();
            try {
                for (Int32 epid = arg_first ; epid <= arg_last ; epid++) {
                    arg_cancellation_token.ThrowIfCancellationRequested();
                    _dulu_sub_progress_              = 0;
                    data._episodes_[epid]._finished_ = false;
                    MainDataManager.xctUpdateUi();
                    _dulu_sub_progress_              = 100;
                    _dulu_progress_ = arg_last == arg_first
                                          ? 100
                                          : ( Int32 ) ( ( epid       - ( Double ) arg_first )
                                                        / ( arg_last - ( Double ) arg_first )
                                                        * 100 );
                    MainDataManager.xctUpdateUi();
                }
            } catch (Exception e) { return false; } finally {
                _dulu_progress_     = 0;
                _dulu_sub_progress_ = 0;
                _dulu_state_        = eDulu.NONE;
            }
            return true;
        }

        private async Task<Boolean> ActAutoStop(Int32 arg_sel , CancellationToken arg_cancellation_token) {
            try {
                Int32 time_in_ms = arg_sel * 60000;
                _tod_ = DateTime.Now.AddMilliseconds(time_in_ms);
                await Task.Delay(time_in_ms , arg_cancellation_token);
                _tod_ = null;
                MainDataManager.xctStop();
                return true;
            } catch (TaskCanceledException) { return false; }
        }

        private static async Task<Boolean> ActReset() {
            Directory.Delete(Paths.xctGetFixedRoot() , true);
            try {
                File.Delete(Paths.xctGetProfilePath());
                File.Delete(Paths.xctGetNamePath());
            } catch (Exception e) { /*ignored*/
            }
            return true;
        }
    }
}