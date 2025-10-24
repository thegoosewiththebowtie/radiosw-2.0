namespace radiosw {
    using System;

    public class TheMusicMenu : iMenu {
        public event EventHandler? nvkMenuClose;
        public void                Dispose()    => throw new NotImplementedException();
        public MenuInfo            xctGetMenu() => throw new NotImplementedException();
        public void                xctBack()    => throw new NotImplementedException();
        public void                xctDown()    => throw new NotImplementedException();
        public void                xctUp()      => throw new NotImplementedException();
        public void                xctSelect()  => throw new NotImplementedException();
        public void                xctLoad()    => throw new NotImplementedException();
    }
}