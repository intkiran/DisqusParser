#r "packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"
#r "System.Xml.Linq.dll"

open FSharp.Data
open System
open System.IO

type Xml = XmlProvider<"F:\Users\Kijana\Downloads\kijanawoodard-2015-03-19T23_28_52.887832-all.xml">
let data = Xml.GetSample()

data.Posts |> Seq.iter (fun post -> printfn "%s" post.Message)

data.Posts |> 
    Seq.map (fun post -> post.Author.Name.ToLower().Split([|' '|])) |> 
    Seq.map Seq.head |>
    Seq.sortBy id |>  
    Seq.countBy id |> 
    Seq.toList

type Comment = {PostId:int; CreatedAt:DateTime; Name:string; Email: string; Message: string}
type Post = {PostId:int; Url:string; Slug:string; Title:string; Comments: Comment list}

let comments = data.Posts |> Seq.map (fun post -> {PostId=post.Thread.Id; CreatedAt=post.CreatedAt; Name=post.Author.Name; Email=post.Author.Email; Message=post.Message})
let filteredComments postId = comments |> Seq.filter (fun c -> c.PostId = postId) |> Seq.toList

let parseSlug (thread:Xml.Thread) = 
    let t = thread
    match t.Id2 with 
        | Some s -> s
        | None -> thread.Title.ToLower().Replace(" | kijana woodard", "").Replace(" ", "-")

let posts = data.Threads |> Seq.map (fun thread -> {PostId=thread.Id; Url=thread.Link; Slug = parseSlug thread; Title=thread.Title; Comments = filteredComments thread.Id})

comments |> Seq.groupBy  (fun c -> c.PostId)|> Seq.toList
posts |> Seq.sortBy (fun p -> p.Slug) |> Seq.toList

let postsWithComments = posts |> Seq.filter(fun p -> p.Comments.Length > 0)
let counted = postsWithComments |> Seq.countBy (fun p -> p.Slug)
let duplicates = counted |> Seq.filter(fun (_, c) -> c > 1)

counted |> Seq.toList
duplicates |> Seq.toList

postsWithComments |> Seq.sortBy (fun p -> p.Slug) |> Seq.iter (fun p -> printfn "%s: %d" p.Slug p.Comments.Length)