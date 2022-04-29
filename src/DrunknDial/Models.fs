namespace DrunknDial

open System
open Xamarin.Forms

module Models =
    type ContactId =
        | ContactId of string
        member this.Value = match this with | ContactId x -> x
        
    type RecentId =
        | RecentId of string
        member this.Value = match this with | RecentId x -> x
    
    type FilePath =
        | FilePath of string
        member this.Value = match this with | FilePath x -> x
        
    type FileName =
        | FileName of string
        member this.Value = match this with | FileName x -> x
        
    type MediaInfo =
        { FilePath: FilePath
          Duration: TimeSpan }
                    
    type Contact =
        { Id: ContactId
          DisplayName: string
          AudioSequencePaths: FilePath list
          AvatarColorHex: string }
        
    type Recent =
        { Id: RecentId
          CreatedAt: DateTimeOffset
          Contact: Contact
          AudioInfo: MediaInfo }
    
