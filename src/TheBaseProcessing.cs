namespace radiosw {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Avalonia.Media.Imaging;

    public static class MainDataManager {
        public static event EventHandler nvkUpdateUi;
        private static Boolean           _is_menu_open_       { get; set; }
        private static iProcessing       _main_processing = new ThePodcastProcessing();
        private static iSource?          _main_source     = new ThePodcastSource();
        private static iMenu?            _main_menu       = new ThePodcastMenu();
        private static iActions?         _actions         = new ThePodcastActions();
        public static  void              xctUpdateUi()      => MainDataManager.nvkUpdateUi.Invoke(null , null);
        public static  iProcessing       xctGetProcessing() => MainDataManager._main_processing;
        public static  iActions          xctGetActions()    => MainDataManager._actions;
        public static  iSource           xctGetSource()     => MainDataManager._main_source;
        public static  eMode             xctGetMode()       => MainDataManager._main_processing.xctGetMode();

        private static void xctSetProcessingClass(eMode arg_mode , Boolean arg_update_main_processing) {
            try {
                if (arg_update_main_processing) {
                    MainDataManager._main_processing?.Dispose();
                    MainDataManager._main_processing = arg_mode switch {
                                                           eMode.PODCAST                    => new ThePodcastProcessing()
                                                         , eMode.MUSIC                      => new TheMusicProcessing()
                                                         , eMode.EXTERNALAUDIO or eMode.DND => throw new Exception()
                                                          ,
                                                       };
                }
                MainDataManager._main_source?.Dispose();
                MainDataManager._main_source = arg_mode switch {
                                                   eMode.PODCAST => new ThePodcastSource()
                                                 , eMode.MUSIC   => new TheMusicSource()
                                                 , eMode.EXTERNALAUDIO or eMode.DND =>
                                                       throw new Exception("aint working yet")
                                                  ,
                                               };
                MainDataManager._main_menu?.Dispose();
                MainDataManager._main_menu = arg_mode switch {
                                                 eMode.PODCAST => new ThePodcastMenu()
                                               , eMode.MUSIC   => new TheMusicMenu()
                                               , eMode.EXTERNALAUDIO or eMode.DND =>
                                                     throw new Exception("aint working yet")
                                                ,
                                             };
                MainDataManager._actions?.Dispose();
                MainDataManager._actions = arg_mode switch {
                                               eMode.PODCAST => new ThePodcastActions()
                                             , eMode.MUSIC   => new TheMusicActions()
                                             , eMode.EXTERNALAUDIO or eMode.DND =>
                                                   throw new Exception("aint working yet")
                                              ,
                                           };
            } catch (Exception e) { ERROR.RINE(e , ""); }
        }

        public static void xctLoad() {
            RIN($"BEGIN" , eDebugLevel.LOG);
            if (MainDataManager._main_processing.xctIsLoaded()) { return; }
            eMode mode = FloppyIo.GetMode();
            if (MainDataManager._main_processing.xctGetMode() != mode) {
                MainDataManager.xctSetProcessingClass(mode , true);
            }
            MainDataManager._main_processing.xctLoad();
            MainDataManager._main_menu.xctLoad();
            RIN($"END" , eDebugLevel.LOG);
        }

        public static void xctStop() {
            RIN($"BEGIN" , eDebugLevel.LOG);
            MainDataManager._main_processing.xctStop();
            MainDataManager.xctSetProcessingClass(MainDataManager._main_processing.xctGetMode(), false);
            MainDataManager.xctUpdateUi();
            RIN($"END" , eDebugLevel.LOG);
        }

        public static void xctRew(Int32 arg_ = 10) {
            if (MainDataManager._is_menu_open_) {
                MainDataManager._main_menu.xctUp();
                return;
            }
            MainDataManager._main_processing.xctRew(arg_);
        }

        public static void xctPrev() {
            if (MainDataManager._is_menu_open_) { return; }
            MainDataManager._main_processing.xctPrev();
        }

        public static void xctPlay() {
            if (MainDataManager._is_menu_open_) { return; }
            MainDataManager._main_processing.xctPlay();
        }

        public static void xctPause() {
            if (MainDataManager._is_menu_open_) {
                MainDataManager._main_menu.xctBack();
                return;
            }
            MainDataManager._main_processing.xctPause();
        }

        public static void xctUnPause() {
            if (MainDataManager._is_menu_open_) {
                MainDataManager._main_menu.xctSelect();
                return;
            }
            MainDataManager._main_processing.xctUnPause();
        }

        public static void xctNext() {
            if (MainDataManager._is_menu_open_) { return; }
            MainDataManager._main_processing.xctNext();
        }

        public static void xctFfd(Int32 arg_ = 10) {
            if (MainDataManager._is_menu_open_) {
                MainDataManager._main_menu.xctDown();
                return;
            }
            MainDataManager._main_processing.xctFfd(arg_);
        }

        public static void xctOpenMenu() {
            if (!MainDataManager._main_processing.xctIsLoaded(false)) {
                return;
            }
            MainDataManager._is_menu_open_ = !_is_menu_open_;
            MainDataManager.xctUpdateUi();
        }

        public static void xctCloseMenu() {
            MainDataManager._is_menu_open_ = false;
            MainDataManager.xctUpdateUi();
        }

        public static PlaybackInfo xctGetPlaybackInfo() {
            PlaybackInfo             retinfo        = new PlaybackInfo();
            eFriendlyProcessingState friendly_state = ERROR.isError ? eFriendlyProcessingState.ERROR : MainDataManager._main_processing.xctGetFriendlyState();
            retinfo.mode_ = "Podcast mode";
            switch (friendly_state) {
                case eFriendlyProcessingState.STAND_BY :
                    retinfo.cover_     = new Bitmap("Assets/floppy_disk.png");
                    retinfo.title_     = "Please insert a floppy disk and press load";
                    retinfo.eplist_    = new List<String>();
                    retinfo.status_    = "\r\nStatus: Stand-By";
                    retinfo.isloading_ = false;
                    retinfo.resetnums_ = true;
                    break;
                case eFriendlyProcessingState.LOADING :
                    retinfo.cover_     = new Bitmap("Assets/floppy_disk_edit.png");
                    retinfo.title_     = "Awaiting transmission_";
                    retinfo.status_    = "\r\nStatus: Loading";
                    retinfo.isloading_ = true;
                    retinfo.resetnums_ = true;
                    break;
                case eFriendlyProcessingState.PAUSED :
                case eFriendlyProcessingState.PLAYING :
                    retinfo.cover_
                        = new Bitmap(MainDataManager.xctGetSource()
                                                    .xctGetSourceAsPodcast()
                                                    .GetEpisode(MainDataManager._main_source
                                                                               .xctGetSourceAsPodcast()
                                                                               ._currentepid_
                                                               )
                                                    ._image_local_source_
                                    );
                    retinfo.title_
                        = MainDataManager.xctGetSource()
                                         .xctGetSourceAsPodcast()
                                         .GetEpisode(MainDataManager.xctGetSource().xctGetSourceAsPodcast()._currentepid_)
                                         ._title_;
                    retinfo.eplist_    = MainDataManager.xctGetFormattedEpisodeList();
                    retinfo.mode_      = "Podcast mode";
                    retinfo.status_    = $"\r\nStatus: {friendly_state.ToString()}";
                    retinfo.isloading_ = false;
                    break;
                case eFriendlyProcessingState.BROADCAST_OVER :
                    retinfo.cover_     = new Bitmap("Assets/floppy_disk_information.png");
                    retinfo.title_     = "Broadcast is over. Insert a new disk.";
                    retinfo.eplist_    = new List<String>();
                    retinfo.mode_      = "Podcast mode";
                    retinfo.status_    = "\r\nStatus: Finished";
                    retinfo.isloading_ = false;
                    retinfo.resetnums_ = true;
                    break;
                case eFriendlyProcessingState.ERROR :
                    retinfo.cover_     = new Bitmap("Assets/floppy_disk_error.png");
                    retinfo.title_     = ERROR.GetGlobalErrorDisplay();
                    retinfo.eplist_    = new List<String>();
                    retinfo.mode_      = "Podcast mode";
                    retinfo.status_    = "\r\nStatus: ERROR";
                    retinfo.isloading_ = false;
                    retinfo.resetnums_ = true;
                    break;
                default : throw new ArgumentOutOfRangeException();
            }
            if (!MainDataManager._is_menu_open_) { return retinfo; }
            MenuInfo menu_info = MainDataManager._main_menu.xctGetMenu();
            retinfo.eplist_       = menu_info.optionlist_;
            retinfo.mode_         = menu_info.modeline_;
            retinfo.status_       = menu_info.statusline_;
            retinfo.timestring_   = menu_info.timeline_;
            retinfo.timedatetime_ = menu_info.timeod_;
            return retinfo;
        }

        private static List<String> xctGetFormattedEpisodeList() {
            List<String> retlist = new List<String>();
            for (Int32 i = MainDataManager.xctGetSource().xctGetSourceAsPodcast()._currentepid_ - 5
                 ; i <= MainDataManager.xctGetSource().xctGetSourceAsPodcast()._currentepid_ + 6
                 ; i++) {
                if (i < 0 || i >= MainDataManager.xctGetSource().xctGetSourceAsPodcast()._episodes_.Count) {
                    retlist.Add("~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                    continue;
                }
                Episode episode = MainDataManager.xctGetSource().xctGetSourceAsPodcast().GetEpisode(i);
                String  tick    = "£";
                if (episode._finished_) { tick                   += "✓"; }
                if (episode._audio_local_source_ != null) { tick =  tick.Replace("£" , "↓"); }
                String episodetitle = episode._title_;
                Int32 nullwidthchar =
                    episodetitle.Count(arg_c => ".:,".Contains(arg_c));
                switch (episodetitle.Length - nullwidthchar) {
                    case > 27 : retlist.Add($"{episodetitle.Remove(26 + nullwidthchar)}_{tick}"); break;
                    case < 27 : {
                        String sp = String.Concat(Enumerable.Repeat(' ' , 27 - ( episodetitle.Length - nullwidthchar ))
                                                 );
                        retlist.Add(episodetitle + sp + tick);
                        break;
                    }
                }
            }
            return retlist;
        }
    }

    public interface iSource : IDisposable {
        public Podcast xctGetSourceAsPodcast();
        public iSource xctGetSourceAsMusic();
        public void    xctSetSourceAsPodcast(Podcast arg_podcast);
        public void    xctSetSourceAsMusic(iSource   arg_source);
        public Boolean xctLoadWebSource();
        public Boolean xctLoadLocalSource(String arg_title , Int32 arg_profid);
        public Boolean xctSaveSource();
    }

    public interface iProcessing : IDisposable {
        public eFriendlyProcessingState xctGetFriendlyState();
        public Boolean                  xctIsLoaded(Boolean arg_shouldcheckrss = true);
        public eMode                    xctGetMode();
        public void                     xctLoad();
        public void                     xctRew(Int32 arg_);
        public void                     xctPrev();
        public void                     xctPlay();
        public void                     xctStop();
        public void                     xctPause();
        public void                     xctUnPause();
        public void                     xctNext();
        public void                     xctFfd(Int32  arg_);
        public void                     xctJump(Int32 arg_jumpto);
    }

    public interface iMenu : IDisposable {
        public MenuInfo            xctGetMenu();
        public void                xctBack();
        public void                xctDown();
        public void                xctUp();
        public void                xctSelect();
        public void                xctLoad();
    }

    public interface iActions : IDisposable {
        public Int32    _dulu_sub_progress_ { get; }
        public eDulu    _dulu_state_        { get; }
        public Int32    _dulu_progress_     { get; }
        public DateTime? _tod_                 { get; }

        public Boolean xctExecute(ThePodcastActions.eActionType arg_type
                                , params Int32[]                arg_args);
    }
}