namespace DrunknDial

open System
open Akavache
open Models
open System.Reactive.Linq
open Xamarin.Essentials

module Repository =
    let getAllContacts ():Async<Models.Contact list> =
        async {
            do! Async.SwitchToThreadPool()
            return BlobCache.LocalMachine.GetAllObjects<Models.Contact>().Wait()
                   |> Seq.sortBy (fun x -> x.DisplayName)
                   |> Seq.toList
            }
        
    let getContact (id: ContactId) =
        async {
            do! Async.SwitchToThreadPool()
            return BlobCache.LocalMachine.GetObject<Models.Contact>(id.Value).Wait()
        }
                       
    let insertRecent contact audioInfo =
        async {
            do! Async.SwitchToThreadPool()
            let now = DateTimeOffset.Now
            let recent =
                { Id = RecentId (string now.Ticks)
                  CreatedAt = now
                  Contact = contact
                  AudioInfo = audioInfo }
                
            return BlobCache.LocalMachine.InsertObject(recent.Id.Value, recent).Wait()
                   |> ignore
        }
        
    let getAllRecents () =
        async {
            do! Async.SwitchToThreadPool()
            return BlobCache.LocalMachine.GetAllObjects<Recent>().Wait()
                   |> Seq.sortByDescending (fun x -> x.CreatedAt)
                   |> Seq.toList
        }
        
    let updateContacts () =
        async {
            let! deviceContacts = Contacts.GetAllAsync () |> Async.AwaitTask
            let! cachedContacts = getAllContacts ()
            
            let mergeContact (c: Contact) =
                let randomColor = (Colors.random ()).ToHex()
                let contact = cachedContacts |> List.tryFind (fun x -> x.Id = ContactId c.Id)
                let audioPaths = contact |> Option.fold (fun _ x -> x.AudioSequencePaths) []
                let avatarColor = contact |> Option.fold (fun _ x -> x.AvatarColorHex) randomColor
                
                { Id = ContactId c.Id
                  DisplayName = c.DisplayName
                  AudioSequencePaths = audioPaths
                  AvatarColorHex = avatarColor }
                
            let insert =
                deviceContacts
                |> Seq.map mergeContact
                |> Seq.map (fun x -> x.Id.Value, x)
                |> dict
                |> BlobCache.LocalMachine.InsertObjects
                
            do! Async.SwitchToThreadPool()
            return insert.Wait() |> ignore
        }
        
    let updateContact (contact: Models.Contact) =
        async {
            do! Async.SwitchToThreadPool()
            return BlobCache.LocalMachine.InsertObject(contact.Id.Value, contact).Wait()
                   |> ignore
        }