namespace radiosw {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class TheMusicActions : iActions {
        public void                Dispose()           => throw new NotImplementedException();
        public List<Task<Boolean>> GetTasks()          => throw new NotImplementedException();
        public Int32               _dulu_sub_progress_ { get; }
        public eDulu               _dulu_state_        { get; }
        public Int32               _dulu_progress_     { get; }
        public DateTime?           _tod_               { get; }

        public Boolean xctExecute(ThePodcastActions.eActionType type , params Int32[] args)
            => throw new NotImplementedException();
    }
}