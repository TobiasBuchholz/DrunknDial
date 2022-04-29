namespace DrunknDial

module StringUtils =
    let getInitials (displayName: string) =
        displayName.Split [|' '|]
        |> Seq.map (fun x -> (string x.[0]).ToUpper())
        |> String.concat ""

