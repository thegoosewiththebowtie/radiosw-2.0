namespace radiosw {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;

    public static class Paths {
        private const           String  K_P_REMOVABLE = "A:/";
        private const           String  K_P_FIXED     = "Z:/";
        private const           String  K_P_MUSIC     = "Z:/Music";
        private static readonly Boolean R_IS_DEBUG;

        static Paths() {
            Boolean ret = Directory.GetCurrentDirectory().Contains("Projects");
            if (!ret) {
                Directory.CreateDirectory(Path.Combine(Paths.K_P_FIXED , "RadioSW.DATA"));
                Paths.R_IS_DEBUG = false;
                return;
            }
            Directory.CreateDirectory(Paths.R_FIXED_DEBUGMODE_);
            Directory.CreateDirectory(Paths.R_REMOVABLE_DEBUGMODE_);
            Paths.R_IS_DEBUG = true;
        }

        private static readonly String R_REMOVABLE_DEBUGMODE_ =
            $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/removable/";
        private static readonly String R_FIXED_DEBUGMODE_ =
            $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/fixed/";
        private static readonly HashSet<Char> R_FORBIDDEN_CHARS = new HashSet<Char> {
                                                                                        '<'
                                                                                      , '>'
                                                                                      , ':'
                                                                                      , '"'
                                                                                      , '/'
                                                                                      , '\\'
                                                                                      , '|'
                                                                                      , '?'
                                                                                      , '*'
                                                                                       ,
                                                                                    };
        public static Boolean xctIsDebug() => Paths.R_IS_DEBUG;

        private static String xctRemoveForbidden(String arg_str) {
            StringBuilder sb = new StringBuilder(arg_str.Length);
            foreach (Char ch in arg_str) { sb.Append(Paths.R_FORBIDDEN_CHARS.Contains(ch) ? '.' : ch); }
            return sb.ToString();
        }

        private static String xctGetRemovableRoot() => !Paths.xctIsDebug() ? Paths.K_P_REMOVABLE : Paths.R_REMOVABLE_DEBUGMODE_;
        public static  String xctGetRssPath()       => Path.Combine(Paths.xctGetRemovableRoot() , "RSS");
        public static  String xctGetProfilePath()   => Path.Combine(Paths.xctGetRemovableRoot() , "PROFILE");
        public static  String xctGetNamePath()      => Path.Combine(Paths.xctGetRemovableRoot() , "NAME");

        public static String xctGetFixedRoot()
            => !Paths.xctIsDebug() ? Path.Combine(Paths.K_P_FIXED , "RadioSW.DATA") : Paths.R_FIXED_DEBUGMODE_;

        public static String xctGetPodcastPath(String arg_podcast_name) {
            String ret_path = Path.Combine(Paths.xctGetFixedRoot() , Paths.xctRemoveForbidden(arg_podcast_name));
            Directory.CreateDirectory(ret_path);
            return ret_path;
        }

        public static String xctGetDownloadPath(String arg_podcast_name) {
            String ret_path
                = Path.Combine(!Paths.xctIsDebug() ? Paths.K_P_FIXED : Paths.R_FIXED_DEBUGMODE_ , "Podcasts" , arg_podcast_name);
            Directory.CreateDirectory(ret_path);
            return ret_path;
        }

        public static String xctGetJsonPath(String arg_podcast_name , Int32 arg_podcast_id)
            => Path.Combine(Paths.xctGetPodcastPath(arg_podcast_name)
                          , $"{Paths.xctRemoveForbidden(arg_podcast_name)}-pq{arg_podcast_id:00}.json"
                           );

        public static String xctGetXmlPath(String arg_podcast_name)
            => Path.Combine(Paths.xctGetPodcastPath(arg_podcast_name) , $"{Paths.xctRemoveForbidden(arg_podcast_name)}.xml");
    }
}