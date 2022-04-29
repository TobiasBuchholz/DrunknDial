namespace DrunknDial

module CallService =
    type CancelFunction = CancelFunction of (unit -> unit)
    
    type State = 
        { IsRunning: bool
          CancelFunction: CancelFunction option }
        
    let initialState = { IsRunning = false; CancelFunction = None }
    
    let startCall contact state =
        async {
            if state.IsRunning then return state
            else 
                do! AudioUtils.startRecording ()
                let (VoiceGenerator.CancelFunction cancel) = VoiceGenerator.start contact
                return { IsRunning = true; CancelFunction = Some (CancelFunction (fun () -> cancel ())) }
        }
        
    let stopCall contact state =
        async {
            if not state.IsRunning then return state
            else 
                let! audioInfo = AudioUtils.stopRecording ()
                do! Repository.insertRecent contact audioInfo
                match state.CancelFunction with
                | Some (CancelFunction cancel) -> cancel ()
                | None -> ()
                return { IsRunning = false; CancelFunction = None }
        }