namespace radiosw {
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Avalonia.Threading;

    public class ThePodcastProcessing : iProcessing {
        public Boolean xctIsLoaded(Boolean arg_shouldcheckrss = true) => _processing_data.IsLoaded(arg_shouldcheckrss);

        private class ProcessingData {
            private eProcessingPodcastState _processing_podcast_state = eProcessingPodcastState.STANDBY;
            private eFriendlyProcessingState _friendly_processing_podcast_state
                = eFriendlyProcessingState.STAND_BY;

            public Boolean IsLoaded(Boolean arg_shouldcheckrss = true)
                => _processing_podcast_state == eProcessingPodcastState.LOADED
                   && ( !arg_shouldcheckrss || FloppyIo.CheckRSS() );

            public Boolean IsNotState(params eProcessingPodcastState[] arg_states)
                => arg_states.All(arg_state => _processing_podcast_state != arg_state);

            public Boolean IsState(params eProcessingPodcastState[] arg_states)
                => arg_states.Any(arg_state => _processing_podcast_state == arg_state);

            public eFriendlyProcessingState GetFriendlyState() => _friendly_processing_podcast_state;
            /*  public void SetToError() {
                  if (_processing_podcast_state == eProcessingPodcastState.ERROR) { return; }
                  _processing_podcast_state          = eProcessingPodcastState.ERROR;
                  _friendly_processing_podcast_state = eFriendlyProcessingState.ERROR;
                  if (_processing_podcast_state == eProcessingPodcastState.LOADED) {
                      MainDataManager.Stop();
                      return;
                  }
                  MainDataManager.UpdateUi();
                  Task.Run(WaitAndUpdate);
              }*/

            public void SetToBroadcastOver() {
                MainDataManager.xctStop();
                _processing_podcast_state          = eProcessingPodcastState.STANDBY;
                _friendly_processing_podcast_state = eFriendlyProcessingState.BROADCAST_OVER;
                MainDataManager.xctUpdateUi();
                Task.Run(WaitAndUpdate);
            }

            public void SetToLoading(Boolean arg_is_ui_loading = true) {
                _processing_podcast_state = eProcessingPodcastState.LOADING;
                if (arg_is_ui_loading) { _friendly_processing_podcast_state = eFriendlyProcessingState.LOADING; }
                MainDataManager.xctUpdateUi();
            }

            public void SetToLoadedAndPlaying(Boolean arg_is_ui_playing = true) {
                _processing_podcast_state = eProcessingPodcastState.LOADED;
                if (arg_is_ui_playing) { _friendly_processing_podcast_state = eFriendlyProcessingState.PLAYING; }
                MainDataManager.xctUpdateUi();
            }

            public void SetToPaused() {
                _processing_podcast_state          = eProcessingPodcastState.LOADED;
                _friendly_processing_podcast_state = eFriendlyProcessingState.PAUSED;
                MainDataManager.xctUpdateUi();
            }

            public void SetToStandBy() {
                _processing_podcast_state          = eProcessingPodcastState.STANDBY;
                _friendly_processing_podcast_state = eFriendlyProcessingState.STAND_BY;
                MainDataManager.xctUpdateUi();
            }

            private async Task WaitAndUpdate() {
                await Task.Delay(10_000).ConfigureAwait(false);
                SetToStandBy();
            }
        }

        private enum ePlaybackOver { NEXT , BROADCASTOVER }
        private readonly DispatcherTimer _progress_saver  = new DispatcherTimer();
        private readonly ProcessingData  _processing_data = new ProcessingData();
        private          Int32           _savecount;
        private          ePlaybackOver   _playback_over = ePlaybackOver.NEXT;
        private          TheMediaPlayer  _the_media_player_                          { get; } = new TheMediaPlayer();

        public ThePodcastProcessing() {
            _the_media_player_.nvkPlaybackOver += TheMediaPlayer_OnPlaybackOver;
            _progress_saver.Interval         =  TimeSpan.FromSeconds(1);
            _progress_saver.Tick             += ProgressSaverOnTick;
        }

        /*private void NoFloppy() {
            if (!_processing_data.IsLoaded()) {
                Stop();
                return;
            }
            MainDataManager.CloseMenu();
            _processing_data.SetToError();
            ERROR.RINVE(ERROR.eErrorType.NOFLOPPY);
        }*/

        private void TheMediaPlayer_OnPlaybackOver(Object? arg_sender , EventArgs arg_event_args) {
            switch (_playback_over) {
                case ePlaybackOver.NEXT :          xctNext(); break;
                case ePlaybackOver.BROADCASTOVER : _processing_data.SetToBroadcastOver(); break;
            }
        }

        public void xctStop() {
            RIN($"BEGIN" , eDebugLevel.LOG);
            if (!_processing_data.IsLoaded(false)) {
                ERROR.RINVE(eErrorType.NOTLOADED);
                return;
            }
            _processing_data.SetToLoading();
            _progress_saver.Stop();
            _the_media_player_.xctStop();
            _processing_data.SetToStandBy();
            RIN($"END" , eDebugLevel.LOG);
        }

        public void Dispose() {
            _the_media_player_.nvkPlaybackOver -= TheMediaPlayer_OnPlaybackOver;
            _progress_saver.Tick             -= ProgressSaverOnTick;
            MainDataManager.xctGetSource().Dispose();
            MainDataManager.xctUpdateUi();
            _the_media_player_.xctStop();
            GC.SuppressFinalize(this);
        }

        public eFriendlyProcessingState xctGetFriendlyState()                          => _processing_data.GetFriendlyState();
        public eMode                    xctGetMode()                                  => eMode.PODCAST;

        private void ProgressSaverOnTick(Object? arg_sender , EventArgs arg_event_args) {
            if (!_processing_data.IsLoaded()) {
                ERROR.RINVE(ERROR.eErrorType.NOTLOADED);
                return;
            }
            MainDataManager.xctGetSource()
                           .xctGetSourceAsPodcast()
                           .GetEpisode(MainDataManager.xctGetSource().xctGetSourceAsPodcast()._currentepid_)
                           ._stoppedat_ =
                ( UInt64 ) _the_media_player_.xctGetTime().timenow_.TotalMilliseconds;
            if (++_savecount < 30) { return; }
            _savecount = 0;
            FloppyIo.CheckRSS();
        }

        public void xctLoad() {
            RIN($"BEGIN" , eDebugLevel.LOG);
            if (!FloppyIo.CheckRSS() || _processing_data.IsNotState(eProcessingPodcastState.STANDBY)) { return; }
            _processing_data.SetToLoading();
            if (!MainDataManager.xctGetSource().xctLoadWebSource()) {
                _processing_data.SetToStandBy();
                return;
            }
            _the_media_player_.xctStop();
            _processing_data.SetToLoadedAndPlaying(false);
            xctPlay();
            RIN($"END" , eDebugLevel.LOG);
        }

        public void xctRew(Int32 arg_rew_sec) {
            RIN($"BEGIN" , eDebugLevel.LOG);
            if (!_processing_data.IsLoaded()) {
                ERROR.RINVE(eErrorType.NOTLOADED);
                return;
            }
            if (_the_media_player_.xctGetTime().timenow_.TotalSeconds <= arg_rew_sec) {
                return;
            }
            _processing_data.SetToLoading(false);
            _the_media_player_.xctRew(arg_rew_sec);
            _processing_data.SetToLoadedAndPlaying(false);
            RIN($"END" , eDebugLevel.LOG);
        }

        public void xctPrev() {
            RIN($"BEGIN" , eDebugLevel.LOG);
            if (!_processing_data.IsLoaded()) {
                ERROR.RINVE(eErrorType.NOTLOADED);
                return;
            }
            if (MainDataManager.xctGetSource().xctGetSourceAsPodcast()._currentepid_ <= 0) { return; }
            _processing_data.SetToLoading();
            MainDataManager.xctGetSource()
                           .xctGetSourceAsPodcast()
                           .UpdateEpid(MainDataManager.xctGetSource().xctGetSourceAsPodcast()._currentepid_ - 1);
            _processing_data.SetToLoadedAndPlaying(false);
            xctPlay();
            RIN($"END" , eDebugLevel.LOG);
        }

        public void xctPlay() {
            RIN($"BEGIN" , eDebugLevel.LOG);
            if (!_processing_data.IsLoaded()) {
                ERROR.RINVE(eErrorType.NOTLOADED);
                return;
            }
            _processing_data.SetToLoading();
            Episode eptoplay
                = MainDataManager.xctGetSource()
                                 .xctGetSourceAsPodcast()
                                 .GetEpisode(MainDataManager.xctGetSource().xctGetSourceAsPodcast()._currentepid_);
            _playback_over
                = eptoplay._id_ == MainDataManager.xctGetSource().xctGetSourceAsPodcast()._episodes_.Count - 1
                      ? ePlaybackOver.BROADCASTOVER
                      : ePlaybackOver.NEXT;
            if (!_the_media_player_.xctLoad(eptoplay._audio_local_source_ ?? eptoplay._audio_web_source_
                                       , TimeSpan.FromMilliseconds(eptoplay._stoppedat_)
                                        )) { return; }
            _the_media_player_.xctPlay();
            _progress_saver.Start();
            _processing_data.SetToLoadedAndPlaying();
            RIN($"END" , eDebugLevel.LOG);


        }

        public void xctPause() {
            RIN($"BEGIN" , eDebugLevel.LOG);
            if (!_processing_data.IsLoaded()) {
                ERROR.RINVE(eErrorType.NOTLOADED);
                return;
            }
            _processing_data.SetToLoading(false);
            _the_media_player_.xctPause();
            _progress_saver.Stop();
            _processing_data.SetToPaused();
            RIN($"END" , eDebugLevel.LOG);
        }

        public void xctUnPause() {
            RIN($"BEGIN" , eDebugLevel.LOG);
            if (!_processing_data.IsLoaded()) {
                ERROR.RINVE(eErrorType.NOTLOADED);
                return;
            }
            _processing_data.SetToLoading(false);
            _the_media_player_.xctPlay();
            _progress_saver.Start();
            _processing_data.SetToLoadedAndPlaying();
            RIN($"END" , eDebugLevel.LOG);
        }

        public void xctNext() {
            RIN($"BEGIN" , eDebugLevel.LOG);
            if (!_processing_data.IsLoaded()) {
                ERROR.RINVE(eErrorType.NOTLOADED);
                return;
            }
            if (MainDataManager.xctGetSource().xctGetSourceAsPodcast()._currentepid_
                >= MainDataManager.xctGetSource().xctGetSourceAsPodcast()._episodes_.Count - 1) { return; }
            _processing_data.SetToLoading();
            if (_the_media_player_.xctGetTime().timepercent_ > 95) {
                MainDataManager.xctGetSource()
                               .xctGetSourceAsPodcast()
                               ._episodes_[MainDataManager.xctGetSource().xctGetSourceAsPodcast()._currentepid_]._stoppedat_
                    = 0;
                MainDataManager.xctGetSource()
                               .xctGetSourceAsPodcast()
                               ._episodes_[MainDataManager.xctGetSource().xctGetSourceAsPodcast()._currentepid_]._finished_
                    = true;
            }
            MainDataManager.xctGetSource()
                           .xctGetSourceAsPodcast()
                           .UpdateEpid(MainDataManager.xctGetSource().xctGetSourceAsPodcast()._currentepid_ + 1);
            _processing_data.SetToLoadedAndPlaying(false);
            xctPlay();
            RIN($"END" , eDebugLevel.LOG);
        }

        public void xctFfd(Int32 arg_) {
            RIN($"BEGIN" , eDebugLevel.LOG);
            if (!_processing_data.IsLoaded()) {
                ERROR.RINVE(eErrorType.NOTLOADED);
                return;
            }
            PlayerTime time = _the_media_player_.xctGetTime();
            if (time.timefull_.TotalSeconds - time.timenow_.TotalSeconds <= arg_) { return; }
            _processing_data.SetToLoading(false);
            _the_media_player_.xctFfd(arg_);
            _processing_data.SetToLoadedAndPlaying(false);
            RIN($"END" , eDebugLevel.LOG);
        }

        public void xctJump(Int32 arg_jumpto) {
            RIN($"BEGIN" , eDebugLevel.LOG);
            MainDataManager.xctGetSource().xctGetSourceAsPodcast().UpdateEpid(arg_jumpto);
            xctPlay();
            RIN($"END" , eDebugLevel.LOG);
        }
    }
}