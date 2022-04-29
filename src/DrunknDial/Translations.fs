namespace DrunknDial

open System
open System.Globalization


module Translations =
    let isoCode () =
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
        
    module Contacts =
        let tab () = match isoCode () with "de" -> "Kontakte" | _ -> "Contacts"
        let searchBarHint () = match isoCode () with "de" -> "In Kontakten suchen..." | _ -> "Filter contacts..."
        let askPermission () = match isoCode () with "de" -> "Um deine Kontakte laden zu können, wird deine Erlaubnis benötigt" | _ -> "Your permission to access your contacts is needed"
        let grantPermission () = match isoCode () with "de" -> "Erlaubnis erteilen" | _ -> "Grant permission"
        
    module ContactDetail =
        let customAnswersTitle () = match isoCode () with "de" -> "Eigene Antwort-Fetzen" | _ -> "Custom answers"
        let customAnswersDescription () = match isoCode () with "de" -> "Nimm eigene Antwort-Fetzen auf um den Anruf noch echter erscheinen zu lassen" | _ -> "Record your own answers to make it sound even more real"
        let stopSequenceRecording () = match isoCode () with "de" -> "STOP" | _ -> "STOP"
        let sequenceRecordDescription () = match isoCode () with "de" -> "Sag etwas wie \"Aha\" und drücke anschließend auf STOP" | _ -> "Say something like \"aha\" and press on STOP"
        let startCall () = match isoCode () with "de" -> "ANRUFEN" | _ -> "START CALL"
        let stopCall () = match isoCode () with "de" -> "AUFLEGEN" | _ -> "STOP CALL"
        
    module Recents =
        let tab () = match isoCode () with "de" -> "Verlauf" | _ -> "Recents"
        let searchBarHint () = match isoCode () with "de" -> "Im Verlauf suchen..." | _ -> "Filter recents..."
        
    module RecentDetail =
        let date () = match isoCode () with "de" -> "Datum:" | _ -> "Date:"
        let length () = match isoCode () with "de" -> "Länge:" | _ -> "Length:"
        let startAudio () = match isoCode () with "de" -> "ABSPIELEN" | _ -> "PLAY AUDIO"
        let stopAudio () = match isoCode () with "de" -> "ANHALTEN" | _ -> "STOP AUDIO"

    module Common =
        let suffixTime () = match isoCode () with "de" -> " Uhr" | _ -> " h"
        let dateFormatted (date:DateTimeOffset) = date.ToString("dd. MMMM, HH:mm") + suffixTime ()
        