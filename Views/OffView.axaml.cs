using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace radiosw.Views {
    using System;

    public partial class OffView : UserControl {
        public OffView() => InitializeComponent();

        [ DllImport("PowrProf.dll" , CharSet = CharSet.Auto , ExactSpelling = true) ]
        public static extern Boolean SetSuspendState(Boolean arg_hiberate , Boolean arg_force_critical , Boolean arg_disable_wake_event);
    }
}