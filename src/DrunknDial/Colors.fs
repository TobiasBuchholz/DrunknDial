namespace DrunknDial

open System
open Xamarin.Forms


module Colors =
    let primary = Color(0.2)
    let primaryDark = Color(0.1)
    let accent = Color.Teal
    let primaryText = Color.White
    let primarySubTitle = Color(0.8)
    let entryPlaceholder = Color(0.4)
    
    let random () =
        let colors = [
            Color.DarkRed
            Color.DarkGreen
            Color.DarkOrange
            Color.DarkBlue
            Color.DarkCyan
            Color.DarkKhaki
            Color.DarkOrchid
            Color.DarkMagenta
            Color.DarkViolet
            Color.DarkOliveGreen
            Color.DarkTurquoise
            Color.DarkSalmon
            Color.DarkSeaGreen
            Color.DarkSlateBlue
            Color.DarkGoldenrod
        ]
        colors.[Random().Next(0, colors.Length - 1)]
