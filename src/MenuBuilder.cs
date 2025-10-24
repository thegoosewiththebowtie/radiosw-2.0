namespace radiosw {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class MenuBuilder {
        private static readonly List<MenuItem> R_JUMP_ITEMS = new List<MenuItem> {
                                                                                     new MenuItem {
                                                                                                      title_ = "FIRST"
                                                                                                    , top_level_id_
                                                                                                          = eTopMenuElements
                                                                                                             .JUMP_TO
                                                                                                    , children_ = null
                                                                                                    , is_top_level_menu_
                                                                                                          = false
                                                                                                    , parameter_ = -1
                                                                                                     ,
                                                                                                  }
                                                                                   , new MenuItem {
                                                                                                      title_ = "LAST"
                                                                                                    , top_level_id_
                                                                                                          = eTopMenuElements
                                                                                                             .JUMP_TO
                                                                                                    , children_ = null
                                                                                                    , is_top_level_menu_
                                                                                                          = false
                                                                                                    , parameter_ = -2
                                                                                                     ,
                                                                                                  }
                                                                                   , new MenuItem {
                                                                                                      title_ = "RANDOM"
                                                                                                    , top_level_id_
                                                                                                          = eTopMenuElements
                                                                                                             .JUMP_TO
                                                                                                    , children_ = null
                                                                                                    , is_top_level_menu_
                                                                                                          = false
                                                                                                    , parameter_ = -3
                                                                                                     ,
                                                                                                  }
                                                                                    ,
                                                                                 };
        private static readonly List<MenuItem> R_DULU_ITEMS = new List<MenuItem> {
                                                                                     new MenuItem {
                                                                                                      title_
                                                                                                          = "ALL BEFORE"
                                                                                                    , top_level_id_
                                                                                                          = null
                                                                                                    , children_ = null
                                                                                                    , is_top_level_menu_
                                                                                                          = false
                                                                                                    , parameter_ = -1
                                                                                                     ,
                                                                                                  }
                                                                                   , new MenuItem {
                                                                                                      title_ = "CURRENT"
                                                                                                    , top_level_id_
                                                                                                          = null
                                                                                                    , children_ = null
                                                                                                    , is_top_level_menu_
                                                                                                          = false
                                                                                                    , parameter_ = -2
                                                                                                     ,
                                                                                                  }
                                                                                   , new MenuItem {
                                                                                                      title_
                                                                                                          = "ALL AFTER"
                                                                                                    , top_level_id_
                                                                                                          = null
                                                                                                    , children_ = null
                                                                                                    , is_top_level_menu_
                                                                                                          = false
                                                                                                    , parameter_ = -3
                                                                                                     ,
                                                                                                  }
                                                                                   , new MenuItem {
                                                                                                      title_ = "All"
                                                                                                    , top_level_id_
                                                                                                          = null
                                                                                                    , children_ = null
                                                                                                    , is_top_level_menu_
                                                                                                          = false
                                                                                                    , parameter_ = -4
                                                                                                     ,
                                                                                                  }
                                                                                    ,
                                                                                 };
        private static readonly List<MenuItem> R_RESET = new List<MenuItem> {
                                                                                new MenuItem {
                                                                                                 title_ = "YES"
                                                                                               , top_level_id_
                                                                                                     = eTopMenuElements
                                                                                                        .JUMP_TO
                                                                                               , children_ = null
                                                                                               , is_top_level_menu_
                                                                                                     = false
                                                                                               , parameter_ = -1
                                                                                                ,
                                                                                             }
                                                                              , new MenuItem {
                                                                                                 title_ = "NO"
                                                                                               , top_level_id_
                                                                                                     = eTopMenuElements
                                                                                                        .JUMP_TO
                                                                                               , children_ = null
                                                                                               , is_top_level_menu_
                                                                                                     = false
                                                                                               , parameter_ = -2
                                                                                                ,
                                                                                             }
                                                                               ,
                                                                            };
        private static readonly Dictionary<eTopMenuElements , Func<List<MenuItem>>> R_SUBMENU_FACTORY
            = new Dictionary<eTopMenuElements , Func<List<MenuItem>>> {
                                                                          [eTopMenuElements.JUMP_TO]
                                                                              = MenuBuilder.BuildJumpToItems
                                                                        , [eTopMenuElements.DOWNLOAD]
                                                                              = () => MenuBuilder
                                                                                   .BuildDuluItems(eTopMenuElements
                                                                                                      .DOWNLOAD
                                                                                                  )
                                                                        , [eTopMenuElements.UNDOWNLOAD]
                                                                              = () => MenuBuilder
                                                                                   .BuildDuluItems(eTopMenuElements
                                                                                                      .UNDOWNLOAD
                                                                                                  )
                                                                        , [eTopMenuElements.FINISH]
                                                                              = () => MenuBuilder
                                                                                   .BuildDuluItems(eTopMenuElements
                                                                                                      .FINISH
                                                                                                  )
                                                                        , [eTopMenuElements.UNFINISH]
                                                                              = () => MenuBuilder
                                                                                   .BuildDuluItems(eTopMenuElements
                                                                                                      .FINISH
                                                                                                  )
                                                                        , [eTopMenuElements.SONGS]
                                                                              = MenuBuilder.BuildSongsItems
                                                                        , [eTopMenuElements.ALBUMS]
                                                                              = MenuBuilder.BuildAlbumsItems
                                                                        , [eTopMenuElements.ARTISTS]
                                                                              = MenuBuilder.BuildArtistsItems
                                                                        , [eTopMenuElements.AUTO_STOP]
                                                                              = MenuBuilder.BuildTimeItems
                                                                        , [eTopMenuElements.RESET]
                                                                              = MenuBuilder.BuildResetItems
                                                                         ,
                                                                      };
        private static readonly Dictionary<eMode , eTopMenuElements[]> R_ALLOWED_ELEMENTS
            = new Dictionary<eMode , eTopMenuElements[]> {
                                                             [eMode.PODCAST]
                                                                 = new[] {
                                                                             eTopMenuElements.JUMP_TO 
                                                                           , eTopMenuElements.DOWNLOAD
                                                                           , eTopMenuElements.UNDOWNLOAD
                                                                           , eTopMenuElements.FINISH
                                                                           , eTopMenuElements.UNFINISH
                                                                           , eTopMenuElements.AUTO_STOP
                                                                           , eTopMenuElements.RESET
                                                                            ,
                                                                         }
                                                           , [eMode.MUSIC]
                                                                 = new[] {
                                                                             eTopMenuElements.SONGS
                                                                           , eTopMenuElements.ALBUMS
                                                                           , eTopMenuElements.ARTISTS
                                                                           , eTopMenuElements.AUTO_STOP
                                                                           , eTopMenuElements.RESET
                                                                            ,
                                                                         }
                                                            ,
                                                         };
        private static List<MenuItem> BuildJumpToItems() {
            List<MenuItem> list = new List<MenuItem>(MenuBuilder.R_JUMP_ITEMS);
            foreach (KeyValuePair<Int32 , Episode> episode in
                     MainDataManager.xctGetSource().xctGetSourceAsPodcast()._episodes_) {
                list.Add(new MenuItem {
                                          title_ = MenuBuilder.GetFormattedTitle(episode.Value._title_
                                                                               , episode.Value._finished_
                                                                               , episode.Value._audio_local_source_ != null
                                                                                )
                                        , top_level_id_
                                              = eTopMenuElements
                                                 .JUMP_TO
                                        , children_ = null
                                        , is_top_level_menu_
                                              = false
                                        , parameter_ = episode.Key
                                         ,
                                      }
                        );
            }
            return list;
        }

        private static List<MenuItem> BuildDuluItems(eTopMenuElements arg_owner) {
            List<MenuItem> list = new List<MenuItem>(MenuBuilder.R_DULU_ITEMS);
            foreach (MenuItem item in list) { item.top_level_id_ = arg_owner; }
            foreach (KeyValuePair<Int32 , Episode> episode in
                     MainDataManager.xctGetSource().xctGetSourceAsPodcast()._episodes_) {
                list.Add(new MenuItem {
                                          title_ = MenuBuilder.GetFormattedTitle(episode.Value._title_
                                                                               , episode.Value._finished_
                                                                               , episode.Value._audio_local_source_ != null
                                                                                )
                                        , top_level_id_
                                              = arg_owner
                                        , children_ = null
                                        , is_top_level_menu_
                                              = false
                                        , parameter_ = episode.Key
                                         ,
                                      }
                        );
            }
            return list;
        }

        private static List<MenuItem> BuildSongsItems()   => throw new NotImplementedException();
        private static List<MenuItem> BuildAlbumsItems()  => throw new NotImplementedException();
        private static List<MenuItem> BuildArtistsItems() => throw new NotImplementedException();

        private static List<MenuItem> BuildTimeItems() {
            List<MenuItem> retlist = new List<MenuItem>();
            const Int32    K_STEP  = 30;
            const Int32    K_MAX   = 10;
            for (Int32 i = K_STEP ; i < K_MAX * K_STEP ; i += K_STEP) {
                retlist.Add(new MenuItem {
                                             title_             = $"{i / 60:00}HOURS {i % 60:00}MINUTES"
                                           , is_top_level_menu_ = false
                                           , top_level_id_      = eTopMenuElements.AUTO_STOP
                                           , parameter_         = i
                                           , children_          = null
                                            ,
                                         }
                           );
            }
            return retlist;
        }

        private static List<MenuItem> BuildResetItems() => new List<MenuItem>(MenuBuilder.R_RESET);

        public static List<MenuItem> BuildMenu() {
            List<MenuItem> retdict
                = new List<MenuItem>();
            eTopMenuElements[] top_menu_elements = MenuBuilder.R_ALLOWED_ELEMENTS[MainDataManager.xctGetMode()];
            foreach (eTopMenuElements top_menu_element in top_menu_elements) {
                retdict.Add(new MenuItem {
                                             title_             = top_menu_element.ToString()
                                           , is_top_level_menu_ = true
                                           , top_level_id_      = top_menu_element
                                           , parameter_         = null
                                           , children_
                                                 = MenuBuilder.R_SUBMENU_FACTORY[top_menu_element]()
                                            ,
                                         }
                           );
            }
            return retdict;
        }

        private static String GetFormattedTitle(String  arg_title
                                              , Boolean arg_finished
                                              , Boolean arg_downloaded
                                              , Int32   arg_limit = 27) {
            String retstring;
            String tick = "£";
            if (arg_finished) { tick   += "✓"; }
            if (arg_downloaded) { tick =  tick.Replace("£" , "↓"); }
            Int32 nullwidthchar =
                arg_title.Count(arg_nullchar => arg_nullchar is '.' or ',' or ':');
            if (arg_title.Length - nullwidthchar >= arg_limit) {
                retstring = $"{arg_title.Remove(arg_limit - 1 + nullwidthchar)}_{tick}";
            } else if (arg_title.Length - nullwidthchar < arg_limit) {
                String sp = String.Concat(Enumerable.Repeat(' ' , arg_limit - ( arg_title.Length - nullwidthchar )));
                retstring = arg_title + sp + tick;
            } else { retstring = "Parsing error: what‽"; }
            return retstring;
        }
    }
}