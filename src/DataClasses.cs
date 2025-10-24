// ReSharper disable PropertyCanBeMadeInitOnly.Global
namespace radiosw {
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Avalonia.Media.Imaging;

    public enum eMode { PODCAST , MUSIC , EXTERNALAUDIO , DND }
    public enum eProcessingPodcastState { STANDBY , LOADING , LOADED , ERROR }

    public enum eFriendlyProcessingState {
        STAND_BY
      , LOADING
      , PLAYING
      , PAUSED
      , BROADCAST_OVER
      , ERROR
       ,
    }

    public enum eTopMenuElements {
        JUMP_TO
      , DOWNLOAD
      , UNDOWNLOAD
      , FINISH
      , UNFINISH
      , SONGS
      , ALBUMS
      , ARTISTS
      , AUTO_STOP
      , RESET
       ,
    }

    public enum eDulu { NONE , DOWNLOAD , UNDOWNLOAD , MARKFIN , UNMARKFIN }

    public class MenuInfo {
        public MenuInfo(List<String> arg_choice
                      , Int32        arg_progress
                      , Int32        arg_subprogress
                      , eDulu        arg_dulu_state
                      , String?      arg_title
                      , DateTime?    arg_time_of_death) {
            optionlist_ = MenuInfo.xctGetFormattedMenu(arg_choice);
            statusline_ = arg_dulu_state switch {
                              eDulu.NONE       => "STATUS: NO ACTION"
                            , eDulu.DOWNLOAD   => "STATUS: DOWN_"
                            , eDulu.UNDOWNLOAD => "STATUS: UNDO_"
                            , eDulu.MARKFIN    => "STATUS: FINI_"
                            , eDulu.UNMARKFIN  => "STATUS: UNFI_"
                             ,
                          };
            modeline_ = $"\n{MenuInfo.xctGetProgressBar(arg_subprogress)}\n{MenuInfo.xctGetProgressBar(arg_progress)}";
            timeline_ = MenuInfo.xctGetFormattedTitle(arg_title);
            timeod_   = arg_time_of_death;
        }

        private static List<String> xctGetFormattedMenu(List<String> arg_list) {
            List<String> retlist = new List<String>();
            foreach (String entry in arg_list) {
                try {
                    if (String.IsNullOrEmpty(entry)) {
                        retlist.Add("~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                        continue;
                    }
                    retlist.Add(entry);
                } catch (Exception e) {
                    Console.WriteLine(e.Message);
                    retlist.Add(e.Message);
                }
            }
            return retlist;
        }

        private static String xctGetProgressBar(Double arg_percentage) {
            const Int32 total_length  = 13;
            Int32       filled_length = ( Int32 ) Math.Round(arg_percentage / 100 * total_length);
            String      bar           = new String('~' , filled_length).PadRight(total_length , ' ');
            return bar;
        }

        private static String xctGetFormattedTitle(String? arg_title) {
            if (arg_title == null) { return "ME|NU|XX"; }
            arg_title = arg_title.ToUpper();
            StringBuilder sb = new StringBuilder();
            for (Int32 i = 0 ; i < arg_title.Length ; i += 2) {
                Int32 len = Math.Min(2 , arg_title.Length - i);
                sb.Append(arg_title.AsSpan(i , len));
                if (i + 2 < arg_title.Length) { sb.Append('|'); }
            }
            while (sb.Length < 8) { sb.Append(sb.Length % 3 != 0 ? "|XX" : 'X'); }
            return sb.ToString();
        }

        public readonly List<String> optionlist_;
        public readonly String       modeline_;
        public readonly String       statusline_;
        public readonly String       timeline_;
        public readonly DateTime?    timeod_;
    }

    public class PlaybackInfo {
        public Bitmap?       cover_        = null;
        public List<String>? eplist_       = null;
        public String?       status_       = null;
        public String?       title_        = null;
        public String?       mode_         = null;
        public Boolean?      isloading_    = null;
        public Boolean       resetnums_    = false;
        public String?       timestring_   = null;
        public DateTime?     timedatetime_ = null;
    }

    /*public class TopMenuEnum {

        private readonly eTopMenuElements[] _top_menu_elements
            = TopMenuEnum.R_ALLOWED_ELEMENTS[MainDataManager.GetMode()];
        private Int32            _currentid;
        public  eTopMenuElements GetTopMenuElement()      => _top_menu_elements[_currentid];
        public  Int32            GetCount()                 => _top_menu_elements.Length;
        public  eTopMenuElements GetFirstTopMenuElement() => _top_menu_elements[0];
        public  eTopMenuElements GetLastTopMenuElement()  => _top_menu_elements[^1];

        public TopMenuEnum NextElement() {
            ++_currentid;
            if (_currentid >= _top_menu_elements.Length) { _currentid = 0; }
            return this;
        }

        public TopMenuEnum PrevElement() {
            --_currentid;
            if (_currentid < 0) { _currentid = _top_menu_elements.Length - 1; }
            return this;
        }
    }*/

    public class MenuItem {
        public String            title_;
        public Boolean           is_top_level_menu_;
        public Int32?            parameter_;
        public eTopMenuElements? top_level_id_;
        public List<MenuItem>?   children_;
    }

    public class Episode {
        public Boolean _finished_           { get; set; }
        public Int32   _id_                 { get; set; }
        public String  _image_web_source_   { get; set; } = "n";
        public String? _image_local_source_ { get; set; }
        public String  _audio_web_source_   { get; set; } = "n";
        public String? _audio_local_source_ { get; set; }
        public UInt64  _stoppedat_          { get; set; }
        public String  _title_              { get; set; } = "n";
    }

    public class Podcast {
        public Int32                       _currentepid_             { get; set; }
        public Dictionary<Int32 , Episode> _episodes_                { get; set; } = new Dictionary<Int32 , Episode>();
        public String                      _main_image_web_source_   { get; set; } = "n";
        public String?                     _main_image_local_source_ { get; set; }
        public Int32                       _prof_id_                 { get; set; } = 1;
        public String                      _title_                   { get; set; } = "n";

        public Episode? GetEpisode(Int32 arg_id) {
            _episodes_.TryGetValue(arg_id , out Episode episode);
            if (episode?._id_ == arg_id) { return episode; }
            ERROR.RINVE(eErrorType.IDMISMATCH);
            return null;
        }

        public void UpdateEpid(Int32 arg_epid) => _currentepid_ = arg_epid;
    }
}