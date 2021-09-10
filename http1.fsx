#r "nuget: Suave"

open System
open System.Net
open System.Net.Http
open Suave

open Suave
open Suave.Http.HttpBinding // always open suave
open Suave.Successful // for OK-result
open Suave.Web // for config

open Suave.Filters
open Suave.Operators

let config =
    { defaultConfig with
          bindings = [ HttpBinding.create Protocol.HTTP (IPAddress.Any) 8081us ] }

let webPart =
    choose [ pathScan "/configure/gv500map/%s.ini" (fun id -> OK(sprintf "Content of the article with ID: %s" id)) ]

startWebServer config webPart
