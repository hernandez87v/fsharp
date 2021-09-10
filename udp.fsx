open System.IO
open System.Net
open System.Net.Sockets
open System.Text

[<Literal>]
let Path = __SOURCE_DIRECTORY__ + "/gps.log"

let agent =
    MailboxProcessor<string>.Start
        (fun inbox ->
            let rec loop () =
                async {
                    let! (message: string) = inbox.Receive()

                    do!
                        File.AppendAllLinesAsync(Path, [ message ])
                        |> Async.AwaitTask

                    return! loop ()
                }

            loop ())


let run port =
    async {
        let listenerEp = IPEndPoint(IPAddress.Any, port)

        let socket =
            new Socket(listenerEp.Address.AddressFamily, SocketType.Dgram, ProtocolType.Udp)

        try
            do socket.Bind(listenerEp)
        with
        | e ->
            printfn $"{e}"
            ()

        do! Async.Sleep 3000

        let rec loop () : Async<unit> =
            async {
                printfn "loop"
                let mutable remote: EndPoint = IPEndPoint(IPAddress.Any, 0) :> _
                let buffer = Array.zeroCreate<byte> 1024
                let read = socket.ReceiveFrom(buffer, &remote)

                if read > 0 then
                    printfn $"Remote Ip {remote}"

                let parseResult =
                    Encoding.ASCII.GetString(buffer.[0..read - 1])

                agent.Post(parseResult)
                return! loop ()
            }

        printfn "starting"
        return! loop ()
    }

Async.Start(run 21001)



let text = File.ReadAllLines Path

text
|> Array.map
    (fun line ->
        match line.Split(":") with
        | [| left; right |] -> right.Split(",").[0]
        | _ -> "")
|> Array.distinct
