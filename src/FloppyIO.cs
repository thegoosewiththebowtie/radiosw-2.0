namespace radiosw {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Text.RegularExpressions;

    public static class FloppyIo {

        // ReSharper disable once InconsistentNaming
        public static Boolean CheckRSS() {
            Boolean retcheck = File.Exists(Paths.xctGetRssPath());
            if (!retcheck) { ERROR.RINVE(ERROR.eErrorType.NOFLOPPY); }
            return retcheck;
        }

        public static Int32 GetProfileId() {
            if (!FloppyIo.CheckRSS()) { return -1; }
            String profile_path = Paths.xctGetProfilePath();
            if (!File.Exists(profile_path)) {
                String? name = FloppyIo.GetPodcastName();
                if (name == null) {
                    ERROR.RINVE(ERROR.eErrorType.NOPROFILEFILE);
                    return -1;
                }
                FloppyIo.SetProfile(name);
            }
            String retstring = File.ReadAllText(profile_path);
            if (Int32.TryParse(retstring , out Int32 retprofid)) { return retprofid; }
            ERROR.RINVE(ERROR.eErrorType.PROFILEFILEUNREADABLE);
            return -1;
        }

        private static void SetProfile(String arg_podcastname) {
            if (!FloppyIo.CheckRSS()) { return; }
            String podcastpath = Paths.xctGetPodcastPath(arg_podcastname);
            if (!Directory.Exists(podcastpath)) { Directory.CreateDirectory(podcastpath); }
            List<Int32> profids = new List<Int32>();
            Regex       regex   = new Regex(@"-pq(\d+)$");
            foreach (String file_path in Directory.EnumerateFiles(podcastpath , "*-pq*")) {
                Match match = regex.Match(Path.GetFileNameWithoutExtension(file_path));
                if (match.Success && Int32.TryParse(match.Groups[1].Value , out Int32 profid)) { profids.Add(profid); }
            }
            profids.Sort();
            Int32 freeid = 1;
            foreach (Int32 id in profids) {
                if (id == freeid) {
                    freeid++;
                    continue;
                }
                break;
            }
            try { File.WriteAllText(Paths.xctGetProfilePath() , freeid.ToString()); } catch
                (Exception e) { ERROR.RINVE(ERROR.eErrorType.IOWRITINGPROFILEERROR); }
        }

        // ReSharper disable once InconsistentNaming
        public static String? GetRSS() {
            if (!FloppyIo.CheckRSS()) { return null; }
            try {
                String retstring = File.ReadAllText(Paths.xctGetRssPath());
                return retstring;
            } catch (Exception e) {
                ERROR.RINVE(ERROR.eErrorType.IOREADINGRSSERROR);
                return null;
            }
        }

        public static eMode GetMode() {

            RIN($"BEGIN" , eDebugLevel.LOG);
            if (!FloppyIo.CheckRSS()) { return eMode.PODCAST; }
            String retstring = File.ReadLines(Paths.xctGetRssPath()).FirstOrDefault();
            if (Uri.TryCreate(retstring , UriKind.Absolute , out Uri? _)) {
                RIN($"MODE: PODCAST" , eDebugLevel.LOG);
                return eMode.PODCAST;
            }
            switch (retstring) {
                case "PODCAST" :       break;
                case "EXTERNALAUDIO" :
                    RIN($"MODE: EXTERNALAUDIO" , eDebugLevel.LOG);
                    return eMode.EXTERNALAUDIO;
                case "MUSIC" :     
                    RIN($"MODE: MUSIC" , eDebugLevel.LOG);
                    return eMode.MUSIC;
            }
            RIN($"DEFAULTING TO PODCAST" , eDebugLevel.WAR);
            return eMode.PODCAST;
        }

        public static String? GetPodcastName() {
            if (!FloppyIo.CheckRSS()) { return null; }
            String name_path = Paths.xctGetNamePath();
            String? ret = File.Exists(name_path)
                              ? File.ReadAllText(name_path)
                              : null;
            if (ret != null) { return ret; }
            ERROR.RINVE(ERROR.eErrorType.IOREADINGNAMEERROR);
            return null;
        }

        public static void SetPodcastName(String arg_podcastname) {
            if (!FloppyIo.CheckRSS()) { return; }
            try { File.WriteAllText(Paths.xctGetNamePath() , arg_podcastname); } catch (Exception e) {
                ERROR.RINVE(ERROR.eErrorType.IOWRITINGNAMEERROR);
            }
        }

        
    }
}