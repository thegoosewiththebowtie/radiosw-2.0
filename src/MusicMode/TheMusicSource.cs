namespace radiosw {
    using System;

    public class TheMusicSource :iSource {
        public void    Dispose() => throw new System.NotImplementedException();

        public Podcast xctGetSourceAsPodcast() => throw new System.NotImplementedException();

        public iSource xctGetSourceAsMusic()                      => throw new System.NotImplementedException();
        public void    xctSetSourceAsPodcast(Podcast arg_podcast) => throw new NotImplementedException();

        public void    xctSetSourceAsMusic(iSource   arg_source)               => throw new NotImplementedException();
        public Boolean xctLoadWebSource()                                      => throw new System.NotImplementedException();
        public Boolean xctLoadLocalSource(String arg_title , Int32 arg_profid) => throw new NotImplementedException();
        public Boolean xctSaveSource() => throw new NotImplementedException();
    }
}