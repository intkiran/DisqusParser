#r "packages/FSharp.Data.2.1.1/lib/net40/FSharp.Data.dll"
#r "packages/FsYaml.2.1.0/lib/net45/FSYaml.dll"
#r "packages/YamlDotNet.3.8.0/lib/portable-net45+netcore45+wpa81+wp8+MonoAndroid1+MonoTouch1/YamlDotNet.dll"
#r "System.Xml.Linq.dll"

open FSharp.Data
open FsYaml
open System
open System.IO

type Xml = XmlProvider<"kijanawoodard-2015-03-19T23_28_52.887832-all.xml">
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

type CommentYaml = { Name:string; Email: string; When: DateTime; Message: string}
let serializer = Yaml.dump<List<CommentYaml>>

let comments = data.Posts |> Seq.map (fun post -> {PostId=post.Thread.Id; CreatedAt=post.CreatedAt; Name=post.Author.Name; Email=post.Author.Email; Message=post.Message})
let filteredComments postId = comments |> Seq.filter (fun c -> c.PostId = postId) |> Seq.toList

let parseSlug (thread:Xml.Thread) = 
    let t = thread
    match t.Id2 with    
        | Some s -> s
        | None -> thread.Title.ToLower().Replace(" | kijana woodard", "").Replace(" ", "-")

let posts = data.Threads |> Seq.map (fun thread -> {PostId=thread.Id; Url=thread.Link; Slug = parseSlug thread; Title=thread.Title; Comments = filteredComments thread.Id}) |> Seq.sortBy (fun p -> p.Slug)

let writeComment (comment:Comment) = {Name = comment.Name; Email = comment.Email; When = comment.CreatedAt; Message = comment.Message}
    
let writePost (post:Post) =
    use sw = new StreamWriter(@"c:\temp\" + post.Slug + ".comments.yaml")
    let comments = post.Comments |> List.map writeComment
    match comments with 
    | [] -> sw.Write("")
    | _ -> 
        let data = serializer comments
        printfn "%s %s" post.Slug data  
        sw.WriteLine(data)   

comments |> Seq.groupBy  (fun c -> c.PostId)|> Seq.toList
posts |> Seq.toList
posts |> Seq.iter writePost


let postsWithComments = posts |> Seq.filter(fun p -> p.Comments.Length > 0)
let counted = postsWithComments |> Seq.countBy (fun p -> p.Slug)
let duplicates = counted |> Seq.filter(fun (_, c) -> c > 1)

counted |> Seq.toList
duplicates |> Seq.toList

postsWithComments |> Seq.sortBy (fun p -> p.Slug) |> Seq.iter (fun p -> printfn "%s: %d" p.Slug p.Comments.Length)