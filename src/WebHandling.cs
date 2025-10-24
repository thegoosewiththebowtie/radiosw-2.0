namespace radiosw {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    public static class WebHandling {
        public static event EventHandler<Int32> eProgressReport;
        public static readonly HttpClient       R_HTTP_CLIENT_ = new HttpClient();

        private struct sImageSrc {
            public sImageSrc(Int32 arg_epid , String arg_src) {
                epid_ = arg_epid;
                src_  = arg_src;
            }

            public readonly Int32  epid_;
            public readonly String src_;
        }

        private struct sImageBank {
            public sImageBank(String arg_podcast_name , String arg_base_src) {
                podcast_name_ = arg_podcast_name;
                base_src_     = arg_base_src;
                image_srcs_   = new List<sImageSrc>();
            }

            public readonly String          podcast_name_;
            public readonly String          base_src_;
            public readonly List<sImageSrc> image_srcs_;
        }

        public static Boolean IsOnline(String arg_request_uri) {
            try {
                ERROR.RIN("BEGIN" , eDebugLevel.LOG);
                using HttpResponseMessage response
                    = WebHandling.R_HTTP_CLIENT_.GetAsync(arg_request_uri , HttpCompletionOption.ResponseHeadersRead)
                                 .GetAwaiter()
                                 .GetResult();
                Boolean ret = response.IsSuccessStatusCode;
                ERROR.RIN("END" , eDebugLevel.LOG);
                return ret;
            } catch (Exception e) { return false; }
        }

        private static void SyncName(ref String? arg_podcastname , XDocument arg_xml_document) {
            ERROR.RIN("BEGIN" , eDebugLevel.LOG);
            if (arg_podcastname != null) { return; }
            arg_podcastname = arg_xml_document.Descendants("channel").Elements("title").FirstOrDefault()?.Value;
            FloppyIo.SetPodcastName(arg_podcastname);
            ERROR.RIN("END" , eDebugLevel.LOG);
        }

        private static Podcast? GetOfflinePodcast(String? arg_podcastname , Int32 arg_profid) {
            ERROR.RIN("BEGIN" , eDebugLevel.LOG);
            if (arg_podcastname == null || arg_profid == -1) {
                ERROR.RINVE(eErrorType.OFFLINE);
                return null;
            }
            if (MainDataManager.xctGetSource().xctLoadLocalSource(arg_podcastname , arg_profid)) {
                ERROR.RIN("END" , eDebugLevel.LOG);
                return MainDataManager.xctGetSource().xctGetSourceAsPodcast();
            }
            ERROR.RINVE(eErrorType.OFFLINE);
            return null;
        }

        private static Boolean SyncXml(String arg_podcastname , XDocument arg_xml_document) {
            ERROR.RIN("BEGIN" , eDebugLevel.LOG);
            String xml_path = Paths.xctGetXmlPath(arg_podcastname);
            if (File.Exists(xml_path)
                && new FileInfo(xml_path).Length != 0
                && XNode.DeepEquals(arg_xml_document , XDocument.Load(xml_path))) { return false; }
            arg_xml_document.Save(xml_path);
            ERROR.RIN("END" , eDebugLevel.LOG);
            return true;
        }

        public static Podcast? GetPodcast() {
            ERROR.RIN("BEGIN" , eDebugLevel.LOG);
            String  src          = FloppyIo.GetRSS();
            String? podcast_name = FloppyIo.GetPodcastName();
            Int32   profid       = FloppyIo.GetProfileId();
            if (!WebHandling.IsOnline(src)) { return WebHandling.GetOfflinePodcast(podcast_name , profid); }
            XDocument xml_document = XDocument.Load(src);
            WebHandling.SyncName(ref podcast_name , xml_document);
            if (!WebHandling.SyncXml(podcast_name , xml_document)) {
                if (MainDataManager.xctGetSource().xctLoadLocalSource(podcast_name , profid)) {
                    return MainDataManager.xctGetSource().xctGetSourceAsPodcast();
                }
            }
            if (!WebHandling.SyncJson(podcast_name , profid , xml_document)) {
                Directory.Delete(Paths.xctGetPodcastPath(podcast_name));
                throw new Exception();
            }
            ERROR.RIN("END" , eDebugLevel.LOG);
            return MainDataManager.xctGetSource().xctLoadLocalSource(podcast_name , profid)
                       ? MainDataManager.xctGetSource().xctGetSourceAsPodcast()
                       : null;
        }

        private static Boolean SyncJson(String arg_podcast_name , Int32 arg_profid , XDocument arg_xml_document) {
            ERROR.RIN("BEGIN" , eDebugLevel.LOG);
            Podcast podcast
                = MainDataManager.xctGetSource().xctLoadLocalSource(arg_podcast_name , arg_profid)
                      ? MainDataManager.xctGetSource().xctGetSourceAsPodcast()
                      : new Podcast();
            XElement? channel = arg_xml_document.Descendants("channel").FirstOrDefault();
            if (channel          == null) { return false; }
            if (arg_podcast_name != channel.Element("title")?.Value) { return false; }
            podcast._title_ = arg_podcast_name;
            sImageBank image_bank
                = new sImageBank(arg_podcast_name , channel.Element("image").Element("url")?.Value ?? "n");
            Int32 id = 0;
            IOrderedEnumerable<XElement> items = channel.Elements("item")
                                                        .OrderBy(arg_x => ( DateTime? ) arg_x.Element("pubDate")
                                                                          ?? DateTime.MinValue
                                                                );
            XNamespace itunesnamespace = "http://www.itunes.com/dtds/podcast-1.0.dtd";
            foreach (XElement item in items) {
                String imgsrc = item.Element(itunesnamespace + "image")?.Attribute("href")?.Value ?? "n";
                if (!podcast._episodes_.TryGetValue(id , out Episode? val)) {
                    val                    = new Episode();
                    podcast._episodes_[id] = val;
                }
                podcast._episodes_[id]._id_               = id;
                podcast._episodes_[id]._title_            = item.Element("title")?.Value                       ?? "n";
                podcast._episodes_[id]._audio_web_source_ = item.Element("enclosure")?.Attribute("url")?.Value ?? "n";
                podcast._episodes_[id]._image_web_source_ = imgsrc;
                image_bank.image_srcs_.Add(new sImageSrc(id , imgsrc));
                ++id;
            }
            if (!WebHandling.SyncCovers(image_bank , ref podcast)) { return false; }
            MainDataManager.xctGetSource().xctSetSourceAsPodcast(podcast);
            ERROR.RIN("END" , eDebugLevel.LOG);
            return true;
        }

        private static Boolean SyncCovers(in sImageBank arg_image_bank , ref Podcast arg_podcast) {
            ERROR.RIN("BEGIN" , eDebugLevel.LOG);
            String podcast_name    = arg_image_bank.podcast_name_;
            String base_cover_path = Path.Combine(Paths.xctGetPodcastPath(arg_image_bank.podcast_name_) , "base-cover");
            if (podcast_name != arg_podcast._title_) { return false; }
            List<Task> tasks = new List<Task>();
            if (!File.Exists(base_cover_path) || arg_podcast._main_image_web_source_ != arg_image_bank.base_src_) {
                tasks.Add(WebHandling.DownloadImage(arg_image_bank.base_src_ , base_cover_path));
            }
            foreach (sImageSrc imgsrc in arg_image_bank.image_srcs_) {
                if (imgsrc.src_ == "n") {
                    arg_podcast._episodes_[imgsrc.epid_]._image_local_source_ = base_cover_path;
                    continue;
                }
                String cover_path
                    = Path.Combine(Paths.xctGetPodcastPath(arg_image_bank.podcast_name_) , $"{imgsrc.epid_:0000}-cover");
                if (!File.Exists(cover_path) || imgsrc.src_ != arg_podcast._episodes_[imgsrc.epid_]._image_web_source_) {
                    tasks.Add(WebHandling.DownloadImage(imgsrc.src_ , cover_path));
                }
                arg_podcast._episodes_[imgsrc.epid_]._image_local_source_ = cover_path;
            }
            Task.WhenAll(tasks).Wait();
            ERROR.RIN("END" , eDebugLevel.LOG);
            return true;
        }

        private static async Task DownloadImage(String arg_src_link , String arg_down_path) {
            try {
                await using Stream stream = await WebHandling.R_HTTP_CLIENT_.GetStreamAsync(arg_src_link);
                await using FileStream file_stream
                    = new FileStream(arg_down_path , FileMode.Create , FileAccess.Write , FileShare.None);
                await stream.CopyToAsync(file_stream);
            } catch (Exception) { ERROR.RINVE(eErrorType.WEBIMAGEDOWNLOADERROR); }
        }

        public static async Task<Boolean> DownloadMp3(String arg_source , String arg_path , CancellationToken arg_cancellation_token) {
            try {
                ERROR.RIN("BEGIN" , eDebugLevel.LOG);
                await using Stream stream
                    = await WebHandling.R_HTTP_CLIENT_.GetStreamAsync(arg_source , arg_cancellation_token);
                await using FileStream fs
                    = new FileStream(arg_path , FileMode.Create , FileAccess.Write , FileShare.None);
                Byte[] buffer           = new Byte[8192];
                Int64  total_bytes_read = 0;
                Int64  total_bytes      = stream.CanSeek ? stream.Length : 1;
                Int32  bytes_read;
                while (( bytes_read = await stream.ReadAsync(buffer , arg_cancellation_token) ) > 0) {
                    arg_cancellation_token.ThrowIfCancellationRequested();
                    total_bytes_read += bytes_read;
                    await fs.WriteAsync(buffer.AsMemory(0 , bytes_read) , arg_cancellation_token);
                    Int32 progress = ( Int32 ) ( ( Double ) total_bytes_read / total_bytes * 100 );
                    WebHandling.eProgressReport?.Invoke(null , progress);
                }
                WebHandling.eProgressReport?.Invoke(null , 100);
                ERROR.RIN("END" , eDebugLevel.LOG);
                return true;
            } catch (HttpRequestException) {
                ERROR.RINVE(eErrorType.WEBMP3DOWNLOADINGERROR);
                return false;
            } catch (IOException) {
                ERROR.RINVE(eErrorType.WEBMP3DOWNLOADINGERROR);
                return false;
            }
        }
    }
}