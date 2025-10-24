namespace radiosw {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ThePodcastMenu : iMenu {
        private          List<MenuItem>      _menu;
        private readonly List<Int32> _selected = new List<Int32>();

        public void Dispose() {
            Console.WriteLine("TODO");
            GC.SuppressFinalize(this);
        }

        private MenuItem? GetItem(Int32 arg_backindex = 0) {
            if (arg_backindex != 0 && arg_backindex >= _selected.Count) { return null; }
            MenuItem node = _menu[_selected.First()];
            for (Int32 index = 1 ; index < _selected.Count - arg_backindex ; index++) {
                Int32 idx = _selected[index];
                node = node.children_![idx];
            }
            return node;
        }

        public MenuInfo xctGetMenu() {
            MenuItem       parent_item = GetItem(1);
            String?        title       = parent_item?.title_ ?? null;
            List<String>   choice      = new List<String>();
            Int32          sel         = _selected.Last();
            List<MenuItem> siblings    = parent_item?.children_ ?? _menu;
            for (Int32 i = sel - 5 ; i < sel + 6 ; i++) {
                if (i < 0 || i >= siblings.Count) {
                    choice.Add(null);
                    continue;
                }
                choice.Add(siblings[i].title_);
            }
            iActions  actions       = MainDataManager.xctGetActions();
            Int32     progress      = actions?._dulu_progress_     ?? 0;
            Int32     subprogress   = actions?._dulu_sub_progress_ ?? 0;
            eDulu     dulu_state    = actions?._dulu_state_        ?? eDulu.NONE;
            DateTime? time_of_death = actions?._tod_               ?? null;
            MenuInfo retmenu = new MenuInfo(choice
                                          , progress
                                          , subprogress
                                          , dulu_state
                                          , title
                                          , time_of_death
                                           );
            return retmenu;
        }

        public void xctBack() {
            if (_selected.Count == 1) { MainDataManager.xctCloseMenu(); } else { _selected.RemoveAt(_selected.Count - 1); }
            MainDataManager.xctUpdateUi();
        }

        public void xctDown() {
            MenuItem? current  = GetItem(1);
            Int32     boundary = _menu.Count                           - 1;
            if (current  != null) { boundary = current.children_.Count - 1; }
            if (boundary == _selected[^1]) { return; }
            ++_selected[^1];
            MainDataManager.xctUpdateUi();
        }

        public void xctUp() {
            if (_selected[^1] <= 0) { return; }
            --_selected[^1];
            MainDataManager.xctUpdateUi();
        }

        public void xctSelect() {
            iActions actions = MainDataManager.xctGetActions();
            if (_selected.Count == 0) {
                if (actions._dulu_state_ != eDulu.NONE) {
                    switch (_menu[_selected[0]].top_level_id_) {
                        case eTopMenuElements.JUMP_TO :
                        case eTopMenuElements.AUTO_STOP :
                        case eTopMenuElements.RESET :
                            break;
                        default :
                            ERROR.RINVE(eErrorType.DULU);
                            MainDataManager.xctCloseMenu();
                            return;
                    }
                }
                _selected.Add(0);
            } else {
                MenuItem sub = GetItem();
                if (sub.children_ != null) { _selected.Add(0); } else {
                    Podcast podcast = MainDataManager.xctGetSource().xctGetSourceAsPodcast();

                    void xctExecuteDulu(ThePodcastActions.eActionType arg_action_type)
                        => actions.xctExecute(arg_action_type
                                            , sub.parameter_ switch { -1 or -4 => 0 , var _ => podcast._currentepid_ }
                                            , sub.parameter_ switch {
                                                  -1 => podcast._currentepid_
                                                        - 1
                                                , -2 => podcast._currentepid_
                                                , -3 or -4 => podcast._episodes_.Count
                                                              - 1
                                                , var _ => sub.parameter_ ?? 0
                                                 ,
                                              }
                                             );

                    switch (_menu[_selected[0]].top_level_id_) {
                        case eTopMenuElements.JUMP_TO :
                            actions.xctExecute(ThePodcastActions.eActionType.JUMPTO
                                             , sub.parameter_ switch {
                                                   -1    => 0
                                                 , -2    => podcast._episodes_.Count - 1
                                                 , -3    => new Random().Next(0 , podcast._episodes_.Count)
                                                 , var _ => sub.parameter_ ?? 0
                                                  ,
                                               }
                                             , podcast._episodes_.Count
                                               - 1
                                              );
                            break;
                        case eTopMenuElements.DOWNLOAD :   xctExecuteDulu(ThePodcastActions.eActionType.DOWNLOAD); break;
                        case eTopMenuElements.UNDOWNLOAD : xctExecuteDulu(ThePodcastActions.eActionType.UNDOWNLOAD); break;
                        case eTopMenuElements.FINISH :     xctExecuteDulu(ThePodcastActions.eActionType.MARKFIN); break;
                        case eTopMenuElements.UNFINISH :   xctExecuteDulu(ThePodcastActions.eActionType.UNMARKFIN); break;
                        case eTopMenuElements.AUTO_STOP :
                            actions.xctExecute(ThePodcastActions.eActionType.AUTOSTOP , sub.parameter_ ?? 30);
                            break;
                        case eTopMenuElements.RESET : actions.xctExecute(ThePodcastActions.eActionType.RESET); break;
                        default :                     throw new ArgumentOutOfRangeException();
                    }
                    MainDataManager.xctCloseMenu();
                }
            }
            MainDataManager.xctUpdateUi();
        }

        public void xctLoad() {
            _menu?.Clear();
            _menu = MenuBuilder.BuildMenu();
            _selected.Add(0);
            MainDataManager.xctUpdateUi();
        }
    }
}
/*
public event EventHandler eMenuUiUpdate;
private ePodcastTopMenuElements   _topmenuselected = ePodcastTopMenuElements.JUMP_TO;
private readonly List<Task<Boolean>> _runningActions;
private          String  _progress = "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~";
private          Int32   _selid;
private          Boolean _istoplevel = true;
private readonly String  _title;
private readonly Int32   _profid;
private          Podcast _mainsrc;




public PodcastMenu(String arg_title , Int32 arg_profid) {
    _title  = arg_title;
    _profid = arg_profid;
    UpdateSources();
}



public MenuInfo GetMenu() {
    MenuInfo retmenu = new MenuInfo {
                                        modeline_
                                            = $"Work:£{PodcastMenu._processing_podcast_work_type.ToString()}"
                                      , statusline_ = _progress
                                      , optionlist_
                                            = PodcastMenu.GetFormattedMenu(_istoplevel
                                                                               ? _top_menu_elements_list
                                                                               : _menu[_topmenuselected]
                                                                         , _istoplevel
                                                                               ? ( Int32 ) _topmenuselected
                                                                               : _selid
                                                                          )
                                    };
    return retmenu;
}

private void Activate() {
    switch (_topmenuselected) {
        case ePodcastTopMenuElements.JUMP_TO :         Task.Run(() => ActJumpTo(_selid)); break;
        case ePodcastTopMenuElements.DOWNLOAD :        Task.Run(() => ActDownload(_selid)); break;
        case ePodcastTopMenuElements.DELETE :          Task.Run(() => ActDelete(_selid)); break;
        case ePodcastTopMenuElements.MARK_FINISHED :   Task.Run(() => ActMarkFin(_selid)); break;
        case ePodcastTopMenuElements.UNMARK_FINISHED : Task.Run(() => ActUnMarkFin(_selid)); break;
        case ePodcastTopMenuElements.AUTO_STOP :       Task.Run(() => ActAutoStop(_selid)); break;
        case ePodcastTopMenuElements.RESET :           Task.Run(() => ActReset(_selid)); break;
        default :                              throw new ArgumentOutOfRangeException();
    }
    Back();
    Back();
}

public void Back() {
    if (_istoplevel) {
        MainDataManager.GetProcessingClass().CloseMenu();
        _topmenuselected = ePodcastTopMenuElements.JUMP_TO;
    } else { _istoplevel = true; }
    eMenuUiUpdate.Invoke(null , null);
}

public void Up() {
    if (_istoplevel && ( Int32 ) _topmenuselected > 0) { _topmenuselected--; } else if (_selid > 0) {
        _selid--;
    }
    eMenuUiUpdate.Invoke(null , null);
}

public void Down() {
    switch (_istoplevel) {
        case true when ( Int32 ) _topmenuselected < _top_menu_elements_list.Count - 1 :
            _topmenuselected++;
            break;
        case false when _selid < _menu[_topmenuselected].Count : _selid++; break;
    }
    eMenuUiUpdate.Invoke(null , null);
}

public void Select() {
    if (_istoplevel) {
        _selid      = 0;
        _istoplevel = false;
    } else { Activate(); }
    eMenuUiUpdate.Invoke(null , null);
}

}*/