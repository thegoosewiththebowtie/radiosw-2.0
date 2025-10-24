// ReSharper disable InconsistentNaming
namespace radiosw {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.CompilerServices;

    public static class ERROR {
        public enum eDebugLevel { LOG , WAR , VER , CER }
        public static Boolean isError = false;
        public enum eErrorType {
            NOTLOADED
          , NOFLOPPY
          , OFFLINE
          , IOREADINGRSSERROR
          , IOREADINGNAMEERROR
          , IOWRITINGNAMEERROR
          , NOPROFILEFILE
          , PROFILEFILEUNREADABLE
          , IOWRITINGPROFILEERROR
          , IOWRITINGDATAERROR
          , IOREADINGDATAERROR
          , WEBIMAGEDOWNLOADERROR
          , WEBMP3DOWNLOADINGERROR
          , IDMISMATCH
          , MPNULLPLAYER
          , MPURIERROR
          , MPNONMP3
          , MPDIDNTLOAD
          , DULU
          , BACKFAIL
           ,
        }

        public static class ErrorStrings {
            public static readonly Dictionary<eErrorType , String> Messages
                = new Dictionary<eErrorType , String> {
                                                          { eErrorType.NOTLOADED , "Resource not loaded" }
                                                        , { eErrorType.NOFLOPPY , "Floppy disk missing or corrupted" }
                                                        , { eErrorType.OFFLINE , "No network connection available" }
                                                        , { eErrorType.IOREADINGRSSERROR , "Error reading RSS feed" }
                                                        , { eErrorType.IOREADINGNAMEERROR , "Error reading name from storage" }
                                                        , { eErrorType.IOWRITINGNAMEERROR , "Error writing name to storage" }
                                                        , { eErrorType.NOPROFILEFILE , "Profile file missing" }
                                                        , { eErrorType.PROFILEFILEUNREADABLE , "Profile file unreadable or corrupt" }
                                                        , { eErrorType.IOWRITINGPROFILEERROR , "Error writing profile data" }
                                                        , { eErrorType.IOWRITINGDATAERROR , "Error writing data to disk" }
                                                        , { eErrorType.IOREADINGDATAERROR , "Error reading data from disk" }
                                                        , { eErrorType.WEBIMAGEDOWNLOADERROR , "Failed to download image from the web" }
                                                        , { eErrorType.WEBMP3DOWNLOADINGERROR , "Failed to download MP3 from the web" }
                                                        , { eErrorType.IDMISMATCH , "ID mismatch detected" }
                                                        , { eErrorType.MPNULLPLAYER , "Media player instance is null" }
                                                        , { eErrorType.MPURIERROR , "Invalid media player URI" }
                                                        , { eErrorType.MPNONMP3 , "Media file is not MP3 format" }
                                                        , { eErrorType.MPDIDNTLOAD , "Media player failed to load file" }
                                                        , { eErrorType.DULU , "Dulu-specific error occurred" }
                                                        , { eErrorType.BACKFAIL , "Failed to go back in navigation" }
                                                         ,
                                                      };
        }

        private static readonly String _global_error_display_ = "";
        public static           String GetGlobalErrorDisplay() => ERROR._global_error_display_;

        public static void RIN(String                      arg_text
                             , eDebugLevel                 arg_debug_level
                             , [ CallerFilePath ]   String arg_file   = ""
                             , [ CallerLineNumber ] Int32  arg_line   = 0
                             , [ CallerMemberName ] String arg_member = "") {
            if (!Paths.xctIsDebug()) { return; }
            Console.ForegroundColor = arg_debug_level switch {
                                          eDebugLevel.LOG => ConsoleColor.Green
                                        , eDebugLevel.WAR => ConsoleColor.Yellow
                                        , eDebugLevel.VER => ConsoleColor.DarkMagenta
                                        , eDebugLevel.CER => ConsoleColor.Red
                                         ,
                                      };
            Console.WriteLine("RIN: " + Path.GetFileName(arg_file) + " => " + arg_line + " in " + arg_member);
            Console.WriteLine($"{arg_debug_level.ToString()}: {arg_text}");
            Console.ResetColor();
        }

        public static void RINVE(eErrorType                  arg_error_type
                               , [ CallerFilePath ]   String arg_file   = ""
                               , [ CallerLineNumber ] Int32  arg_line   = 0
                               , [ CallerMemberName ] String arg_member = "") {
            switch (arg_error_type) { }
        }

        [ DoesNotReturn ]
        public static void RINE(Exception                   arg_exception
                              , String                      arg_custom_message
                              , [ CallerFilePath ]   String arg_file   = ""
                              , [ CallerLineNumber ] Int32  arg_line   = 0
                              , [ CallerMemberName ] String arg_member = "") {
            ERROR.RIN(arg_exception.Message + " " + arg_custom_message
                    , eDebugLevel.CER
                    , arg_file
                    , arg_line
                    , arg_member
                     );
            throw arg_exception;
        }
        /*
        private static void DownloadXMLOnoffline(Object? arg_sender , EventArgs arg_event_args) {
            Paths.PrintDebug("offline" , eDebugLevel.THE_HELL_QM);
            ERROR._global_error_display_ = $"ERROR£2x{ 1:000}TWE: CANNOT TUNE IN;";
            ProcessingClass.GetProcessingClass().SetToError();
        }

        private static void TheMediaPlayerOnMediaPlayerError(Object? arg_sender , ePlayerError arg_event_args) {
            Paths.PrintDebug(arg_event_args.ToString() , eDebugLevel.FUUUUUUUCK);
            ERROR._global_error_display_ = $"ERROR£3x{( Int32 ) arg_event_args:0000}DF: {arg_event_args.ToString()}";
            ProcessingClass.GetProcessingClass().SetToError();
        }

        private static void DownloadXMLOnprogressReport(Object? arg_sender , Int32 arg_event_args)
            => Paths.PrintDebug(arg_event_args.ToString() , eDebugLevel.FYI);

        private static void FloppyIOOnhandledError(Object? arg_sender , eFloppyIoErrors arg_floppy_io_error) {
            Paths.PrintDebug(arg_floppy_io_error.ToString() , eDebugLevel.FUUUUUUUCK);
            ERROR._global_error_display_
                = $"ERROR£2x{( Int32 ) arg_floppy_io_error:0000}FE: {arg_floppy_io_error.ToString()}";
            ProcessingClass.GetProcessingClass().Stop();
            ProcessingClass.GetProcessingClass().SetToError();
        }

        private static void FloppyIOOnNoFloppy(Object? arg_sender , EventArgs arg_event_args) {
            Paths.PrintDebug("FloppyIOOnNoFloppy" , eDebugLevel.FYI);
            ERROR._global_error_display_ = "ERROR£0x001FDE: FLOPPY DISK INVALID";
            ProcessingClass.GetProcessingClass().SetToError();
        }

        private static void DownloadXMLOnunhandledDownloadError(Object? arg_sender , Exception arg_exception) {
            Paths.PrintDebug(arg_exception.Message , eDebugLevel.FUUUUUUUCK);
            ERROR._global_error_display_ = $"ERROR£3x{2115:0000}DX: \r\n{arg_exception.Message}";
            ProcessingClass.GetProcessingClass().SetToError();
        }

        private static void DownloadXMLOnhandledDownloadError(Object?            arg_sender
                                                            , eDownloadErrorType arg_download_error_type
            ) {
            Paths.PrintDebug(arg_download_error_type.ToString() , eDebugLevel.HOLYSHIT);
            ERROR._global_error_display_
                = $"ERROR£2x{( Int32 ) arg_download_error_type:0000}FE: \r\n{arg_download_error_type.ToString()}";
            ProcessingClass.GetProcessingClass().SetToError();
        }

        private static void ProcessingOnhandledProcessingError(Object?           arg_sender
                                                             , eProcessingErrors arg_processing_errors
            ) {
            Paths.PrintDebug(arg_processing_errors.ToString() , eDebugLevel.HOLYSHIT);
            ERROR._global_error_display_
                = $"ERROR£2x{( Int32 ) arg_processing_errors:0000}PE:\r\n{arg_processing_errors.ToString()}";
            ProcessingClass.GetProcessingClass().SetToError();
        }
        */
    }
}