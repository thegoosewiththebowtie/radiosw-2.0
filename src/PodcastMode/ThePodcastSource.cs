namespace radiosw {
    using System;
    using System.IO;
    using System.Text.Json;

    public class ThePodcastSource : iSource {
        private Podcast? _main_src;
        public  Podcast  xctGetSourceAsPodcast()                    => _main_src;
        public  iSource  xctGetSourceAsMusic()                      => throw new Exception("idiot");

        public void xctSetSourceAsPodcast(Podcast arg_podcast) {
            _main_src = arg_podcast;
            xctSaveSource();
        }

        public  void     xctSetSourceAsMusic(iSource   arg_source)  => throw new Exception("idiot");

        public Boolean xctLoadWebSource() {
            RIN($"BEGIN" , eDebugLevel.LOG);
            _main_src = WebHandling.GetPodcast();
            RIN($"END ISNULL:{_main_src == null || _main_src == new Podcast()}" , eDebugLevel.LOG);
            return (_main_src != null && _main_src != new Podcast());
        }

        public Boolean xctLoadLocalSource(String arg_title , Int32 arg_profid) {
            RIN($"BEGIN" , eDebugLevel.LOG);
            _main_src = ThePodcastSource.ReadPodcastDataFile(arg_title , arg_profid);
            RIN($"END ISNULL:{_main_src == null || _main_src == new Podcast()}" , eDebugLevel.LOG);
            return (_main_src != null && _main_src != new Podcast());
        }

        public Boolean xctSaveSource() {
            RIN($"BEGIN" , eDebugLevel.LOG);
            if (_main_src == null) { return false; }
            ThePodcastSource.WritePodcastDataFile(_main_src);
            RIN($"END" , eDebugLevel.LOG);
            return true;
        }

        public void Dispose() {
            RIN($"BEGIN" , eDebugLevel.LOG);
            xctSaveSource();
            _main_src = null;
            RIN($"END" , eDebugLevel.LOG);
        }

        private static Boolean WritePodcastDataFile(Podcast arg_podcast) {
            try {
                RIN($"BEGIN" , eDebugLevel.LOG);
                String folder
                    = Path.Combine(Paths.xctGetPodcastPath(arg_podcast._title_));
                Directory.CreateDirectory(folder);
                String json = JsonSerializer.Serialize(arg_podcast);
                File.WriteAllText(Paths.xctGetJsonPath(arg_podcast._title_ , arg_podcast._prof_id_) , json);
                RIN($"END" , eDebugLevel.LOG);
                return true;
            } catch (Exception e) {
                ERROR.RINVE(ERROR.eErrorType.IOWRITINGDATAERROR);
                return false;
            }
        }

        private static Podcast? ReadPodcastDataFile(String arg_title , Int32 arg_profid) {
            RIN($"BEGIN" , eDebugLevel.LOG);
            if (!FloppyIo.CheckRSS()) { return null; }
            try {
                String  json = File.ReadAllText(Paths.xctGetJsonPath(arg_title , arg_profid));
                Podcast ret  = JsonSerializer.Deserialize<Podcast>(json);
                RIN($"END" , eDebugLevel.LOG);
                return ret ?? null;
            } catch (Exception e) {
                ERROR.RINVE(ERROR.eErrorType.IOREADINGDATAERROR);
                return null;
            }
        }
    }
}