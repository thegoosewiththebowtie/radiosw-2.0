namespace radiosw {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Timers;
    using NAudio.Wave;

    public class PlayerTime {
        public TimeSpan timefull_;
        public TimeSpan timenow_;
        public Int32    timepercent_;
    }

    public enum ePlayerState { NULL , PLAYING , PAUSED , STOPPED }

    public class TheMediaPlayer {
        private          WasapiOut?                     _themediaplayer = new WasapiOut();
        private readonly Timer                          _thetimer       = new Timer(100);
        private          WaveStream?                    _currentmedia;
        private          KeyValuePair<String , Boolean> _currentmediasrc;
        private          Stream?                        _currentstream;

        public TheMediaPlayer() {
            _thetimer.Elapsed += xctOnElapsed;
            _thetimer.Start();
        }

        ~TheMediaPlayer() {
            _thetimer.Elapsed -= xctOnElapsed;
            _thetimer.Dispose();
        }

        public event        EventHandler             nvkPlaybackOver;
        public static event EventHandler<PlayerTime> nvkUpdateTime;

        private void xctOnElapsed(Object? arg_sender , ElapsedEventArgs arg_elapsed_event_args) {
            if (xctGetState() != ePlayerState.PLAYING) { return; }
            PlayerTime player_time = xctGetTime();
            TheMediaPlayer.nvkUpdateTime.Invoke(null , player_time);
            if (player_time.timepercent_ >= 100) { nvkPlaybackOver.Invoke(this , EventArgs.Empty); }
        }

        public PlayerTime xctGetTime() {
            if (_themediaplayer == null || _currentmedia == null || _currentstream == null) { return new PlayerTime(); }
            PlayerTime retp_player_time
                = new PlayerTime { timenow_ = _currentmedia.CurrentTime , timefull_ = _currentmedia.TotalTime };
            retp_player_time.timepercent_
                = ( Int32 ) Math.Round(retp_player_time.timenow_.TotalMilliseconds
                                       / retp_player_time.timefull_.TotalMilliseconds
                                       * 100
                                      );
            return retp_player_time;
        }

        private ePlayerState xctGetState() {
            ePlayerState retstate = ePlayerState.NULL;
            if (_themediaplayer == null || _currentmedia == null) { return retstate; }
            retstate = _themediaplayer.PlaybackState switch {
                           PlaybackState.Stopped => ePlayerState.STOPPED
                         , PlaybackState.Playing => ePlayerState.PLAYING
                         , PlaybackState.Paused  => ePlayerState.PAUSED
                         , var _                 => retstate
                          ,
                       };
            return retstate;
        }

        public Boolean xctLoad(String arg_srclink , TimeSpan arg_startfrom) {
            try {
                xctStop();
                if (!SetCurrentMediaSrc(arg_srclink)) {
                    ERROR.RINVE(eErrorType.MPDIDNTLOAD);
                    return false;
                }
                switch (_currentmediasrc.Value) {
                    case true :
                        if (!xctLoadWeb(arg_srclink)) { return false; }
                        break;
                    case false :
                        if (!xctLoadFile(arg_srclink)) { return false; }
                        break;
                }
                _currentmedia.CurrentTime = arg_startfrom;
                return true;
            } catch (Exception e) {
                ERROR.RINVE(eErrorType.MPDIDNTLOAD);
                return false;
            }
        }

        private Boolean xctLoadWeb(String arg_srclink) {
            if (!WebHandling.IsOnline(arg_srclink)) { return false; }
            HttpRequestMessage  request    = new HttpRequestMessage(HttpMethod.Get , arg_srclink);
            HttpResponseMessage response   = WebHandling.R_HTTP_CLIENT_.Send(request , HttpCompletionOption.ResponseHeadersRead);
            Stream              raw_stream = response.Content.ReadAsStream();
            _currentstream = new MemoryStream();
            raw_stream.CopyTo(_currentstream);
            _currentstream.Position = 0;
            try { _currentmedia = new Mp3FileReader(_currentstream); } catch (Exception e) {
                ERROR.RINVE(eErrorType.MPNONMP3);
                return false;
            }
            _themediaplayer.Init(_currentmedia);
            return true;
        }

        private Boolean xctLoadFile(String arg_srclink) {
            String file_path = arg_srclink.Replace("file://" , "");
            file_path     = Uri.UnescapeDataString(file_path);
            _currentmedia = new Mp3FileReader(file_path);
            _themediaplayer.Init(_currentmedia);
            return true;
        }

        public void xctPlay() {
            switch (xctGetState()) {
                case ePlayerState.PLAYING : break;
                case ePlayerState.PAUSED :
                case ePlayerState.STOPPED :
                    _themediaplayer.Play();
                    break;
                case ePlayerState.NULL : ERROR.RINVE(eErrorType.MPNULLPLAYER); break;
                default :                throw new ArgumentOutOfRangeException();
            }
        }

        public void xctPause() {
            ePlayerState ps = xctGetState();
            if (ps is ePlayerState.PAUSED or ePlayerState.NULL) { return; }
            _themediaplayer.Pause();
        }

        public void xctStop() {
            _ismanual = true;
            _themediaplayer.Stop();
            _themediaplayer?.Dispose();
            _themediaplayer = new WasapiOut();
            _currentmedia?.Dispose();
            _currentmedia = null;
            _currentstream?.Dispose();
            _currentstream = null;
            _ismanual      = false;
        }

        public         void    xctRew(Int32    arg_)       => _currentmedia.CurrentTime -= TimeSpan.FromSeconds(arg_);
        public         void    xctFfd(Int32    arg_)       => _currentmedia.CurrentTime += TimeSpan.FromSeconds(arg_);
        private static Boolean IsUrl(String arg_source) => arg_source.StartsWith("http" , StringComparison.OrdinalIgnoreCase);
        private        Boolean _ismanual;

        private Boolean SetCurrentMediaSrc(String arg_value) {
            if (Uri.TryCreate(arg_value , UriKind.Absolute , out Uri? _)) {
                _currentmediasrc = new KeyValuePair<String , Boolean>(arg_value , TheMediaPlayer.IsUrl(arg_value));
                return true;
            }
            ERROR.RINVE(eErrorType.MPURIERROR);
            return false;
        }
    }
}